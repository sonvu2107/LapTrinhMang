using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using LanStream.Network;
using LanStream.Server.Config;
using LanStream.Shared.Models;
using OpenCvSharp;

namespace LanStream.Server.Services;

/// <summary>
/// Orchestrates the server video pipeline:
/// <see cref="UdpFrameReceiver"/> -> Bounded Channel -> <see cref="FrameDecoder"/> ->
/// <see cref="VideoRecorder"/> / <see cref="FaceDetectionService"/> -> UI <see cref="Bitmap"/> output.
/// </summary>
public sealed class ServerStreamService : IDisposable
{
    private readonly FrameDecoder _frameDecoder;
    private readonly VideoRecorder _videoRecorder;
    private readonly FaceDetectionService _faceDetectionService;

    private ServerSettings _settings = new();
    private UdpFrameReceiver? _udpReceiver;
    private Channel<VideoFrame>? _decodeChannel;
    private CancellationTokenSource? _serverCts;

    private Task? _receiveTask;
    private Task? _decodeTask;
    private Task? _statsTask;
    private Task? _stopTask;

    private OpenCvSharp.Size _lastDecodedFrameSize;
    private uint _rawFrameCounter;
    private long _decodedFramesInWindow;
    private long _droppedDecodeQueueFrames;
    private long _bytesReceivedInWindow;
    private long _lastUdpBytesReceived;
    private long _lastUdpDroppedFrames;
    private long _lastUdpInvalidPackets;
    private long _lastFrameTimestampTicksUtc;
    private volatile bool _faceDetectionEnabled;
    private bool _hasActiveVideoSignal;
    private bool _disposed;

    public event Action<Bitmap, VideoFrame>? FrameDecoded;

    public event Action<StreamStatistics>? StatisticsUpdated;

    public event Action<string>? StatusChanged;

    public event Action<string>? ErrorOccurred;

    public StreamStatistics Statistics { get; } = new();

    public bool IsRunning { get; private set; }

    public bool IsRecording => _videoRecorder.IsRecording;

    public string? CurrentRecordingPath => _videoRecorder.CurrentOutputPath;

    public bool IsFaceDetectionEnabled => _faceDetectionEnabled;

    public bool IsFaceModelAvailable => _faceDetectionService.IsModelLoaded;

    public ServerStreamService(
        FrameDecoder? frameDecoder = null,
        VideoRecorder? videoRecorder = null,
        FaceDetectionService? faceDetectionService = null)
    {
        _frameDecoder = frameDecoder ?? new FrameDecoder();
        _videoRecorder = videoRecorder ?? new VideoRecorder();
        _faceDetectionService = faceDetectionService ?? new FaceDetectionService();
        _lastDecodedFrameSize = new OpenCvSharp.Size(_settings.FrameWidth, _settings.FrameHeight);
    }

    public Task StartAsync(int port)
    {
        _settings.ListenPort = port;
        return StartAsync(_settings);
    }

    public async Task StartAsync(ServerSettings settings)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(settings);

        if (!settings.Validate(out string validationError))
        {
            throw new ArgumentException(validationError, nameof(settings));
        }

        if (_serverCts is not null)
        {
            await StopAsync();
        }

        PrepareRuntimePipeline(settings);
        CancellationToken token = _serverCts!.Token;

        _udpReceiver = new UdpFrameReceiver(
            settings.ListenPort,
            new FrameAssembler(settings.FrameTimeoutMs));
        _udpReceiver.FrameReceived += OnFrameReceived;

        IsRunning = true;
        StatusChanged?.Invoke($"Listening on UDP port {settings.ListenPort} - Waiting for video...");

        _decodeTask = Task.Run(() => ConsumeDecodeQueueAsync(token), token);
        _statsTask = Task.Run(() => MonitorStatisticsLoopAsync(token), token);
        UdpFrameReceiver receiver = _udpReceiver;
        _receiveTask = Task.Run(() => RunReceiverLoopSafeAsync(receiver, token), token);
    }

    public Task StopAsync()
    {
        if (_stopTask is { IsCompleted: false })
        {
            return _stopTask;
        }

        return _stopTask = StopCoreAsync();
    }

    private async Task StopCoreAsync()
    {
        if (!IsRunning && _serverCts is null)
        {
            return;
        }

        IsRunning = false;
        _hasActiveVideoSignal = false;

        if (_udpReceiver is not null)
        {
            _udpReceiver.FrameReceived -= OnFrameReceived;
        }

        if (_serverCts is not null)
        {
            try
            {
                await _serverCts.CancelAsync();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        _decodeChannel?.Writer.TryComplete();

        _udpReceiver?.Dispose();

        await AwaitTaskQuietlyAsync(_receiveTask);
        await AwaitTaskQuietlyAsync(_decodeTask);
        await AwaitTaskQuietlyAsync(_statsTask);
        UpdateReceiverStatistics();
        _udpReceiver = null;

        _receiveTask = null;
        _decodeTask = null;
        _statsTask = null;

        _videoRecorder.StopRecording();

        _serverCts?.Dispose();
        _serverCts = null;
        _decodeChannel = null;

        Statistics.CurrentFps = 0;
        Statistics.EstimatedBitrateKbps = 0;
        Statistics.DroppedFrames += Interlocked.Exchange(ref _droppedDecodeQueueFrames, 0);
        StatisticsUpdated?.Invoke(Statistics);
        StatusChanged?.Invoke("Stopped");
    }

    public void OnFrameReceived(byte[] jpegData)
    {
        if (jpegData is null || jpegData.Length == 0)
        {
            return;
        }

        var frame = new VideoFrame
        {
            StreamId = 1,
            FrameId = Interlocked.Increment(ref _rawFrameCounter),
            TimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            JpegBytes = jpegData
        };

        OnFrameReceived(frame);
    }

    public void OnFrameReceived(VideoFrame frame)
    {
        if (!IsRunning || frame?.JpegBytes is null || frame.JpegBytes.Length == 0)
        {
            return;
        }

        var channel = _decodeChannel;
        if (channel is null)
        {
            return;
        }

        Interlocked.Exchange(ref _lastFrameTimestampTicksUtc, DateTime.UtcNow.Ticks);
        if (_udpReceiver is null)
        {
            Interlocked.Add(ref _bytesReceivedInWindow, frame.JpegBytes.Length);
            Statistics.TotalBytesTransferred += frame.JpegBytes.Length;
        }

        if (!channel.Writer.TryWrite(frame))
        {
            Interlocked.Increment(ref _droppedDecodeQueueFrames);
        }
    }

    public bool ProcessFrame(byte[] jpegData)
    {
        if (jpegData is null || jpegData.Length == 0)
        {
            return false;
        }

        var frame = new VideoFrame
        {
            StreamId = 1,
            FrameId = Interlocked.Increment(ref _rawFrameCounter),
            TimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            JpegBytes = jpegData
        };

        return ProcessFrame(frame);
    }

    public bool ProcessFrame(VideoFrame frame)
    {
        using Mat? decodedMat = _frameDecoder.Decode(frame.JpegBytes);
        if (decodedMat is null)
        {
            Statistics.InvalidPackets++;
            return false;
        }

        _lastDecodedFrameSize = new OpenCvSharp.Size(decodedMat.Width, decodedMat.Height);

        if (_videoRecorder.IsRecording)
        {
            _videoRecorder.WriteFrame(decodedMat);
        }

        if (_faceDetectionEnabled)
        {
            _faceDetectionService.DrawTrackedFaceBoxes(decodedMat, frame.StreamId, frame.RemoteEndPoint);
        }

        var handler = FrameDecoded;
        if (handler is not null)
        {
            Bitmap bitmap = _frameDecoder.ConvertToBitmap(decodedMat);
            handler.Invoke(bitmap, frame);
        }

        Statistics.CompletedFrames++;
        Interlocked.Increment(ref _decodedFramesInWindow);

        if (!_hasActiveVideoSignal)
        {
            _hasActiveVideoSignal = true;
            string sourceText = frame.RemoteEndPoint?.ToString() ?? "Client";
            StatusChanged?.Invoke($"Receiving video from {sourceText} ({decodedMat.Width}x{decodedMat.Height})");
        }

        return true;
    }

    public string StartRecording(string? customFilePath = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        string targetPath = customFilePath ?? Path.Combine(
            _settings.RecordingsDirectory,
            $"stream_{DateTime.Now:yyyy-MM-dd_HHmmss_fff}_{Guid.NewGuid():N}.mp4");

        OpenCvSharp.Size recordSize = _lastDecodedFrameSize.Width > 0 && _lastDecodedFrameSize.Height > 0
            ? _lastDecodedFrameSize
            : new OpenCvSharp.Size(_settings.FrameWidth, _settings.FrameHeight);

        int recordFps = Math.Max(1, _settings.TargetFps);
        return _videoRecorder.StartRecording(targetPath, recordFps, recordSize);
    }

    public void StopRecording()
    {
        _videoRecorder.StopRecording();
    }

    public bool SetFaceDetectionEnabled(bool enabled)
    {
        _settings.EnableFaceDetection = enabled;
        _faceDetectionEnabled = false;
        _faceDetectionService.ResetTracking();
        _faceDetectionEnabled = enabled && _faceDetectionService.LoadModel(_settings.FaceModelPath);
        return !enabled || _faceDetectionEnabled;
    }

    private void PrepareRuntimePipeline(ServerSettings settings)
    {
        _settings = settings;
        _hasActiveVideoSignal = false;
        SetFaceDetectionEnabled(settings.EnableFaceDetection);
        _decodedFramesInWindow = 0;
        _bytesReceivedInWindow = 0;
        _lastUdpBytesReceived = 0;
        _lastUdpDroppedFrames = 0;
        _lastUdpInvalidPackets = 0;
        _droppedDecodeQueueFrames = 0;
        _lastFrameTimestampTicksUtc = 0;
        Statistics.Reset();

        _serverCts = new CancellationTokenSource();
        int capacity = Math.Clamp(settings.MaxDecodeQueueSize, 1, 8);
        _decodeChannel = Channel.CreateBounded<VideoFrame>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        }, _ => Interlocked.Increment(ref _droppedDecodeQueueFrames));
    }

    private async Task RunReceiverLoopSafeAsync(UdpFrameReceiver receiver, CancellationToken cancellationToken)
    {
        try
        {
            await receiver.ReceiveLoopAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (NotImplementedException)
        {
            ErrorOccurred?.Invoke("UDP receiver module is not implemented yet.");
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            ErrorOccurred?.Invoke($"UDP receiver stopped on port {_settings.ListenPort}: {ex.Message}");
        }
    }

    private async Task ConsumeDecodeQueueAsync(CancellationToken cancellationToken)
    {
        var reader = _decodeChannel?.Reader;
        if (reader is null)
        {
            return;
        }

        try
        {
            await foreach (VideoFrame frame in reader.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ProcessFrame(frame);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            ErrorOccurred?.Invoke($"Frame decode error: {ex.Message}");
        }
    }

    private async Task MonitorStatisticsLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var stopwatch = Stopwatch.StartNew();

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                UpdateReceiverStatistics();
                double elapsedSeconds = Math.Max(0.1, stopwatch.Elapsed.TotalSeconds);
                stopwatch.Restart();

                long frames = Interlocked.Exchange(ref _decodedFramesInWindow, 0);
                long bytes = Interlocked.Exchange(ref _bytesReceivedInWindow, 0);
                long queueDropped = Interlocked.Exchange(ref _droppedDecodeQueueFrames, 0);

                Statistics.CurrentFps = Math.Round(frames / elapsedSeconds, 1);
                Statistics.EstimatedBitrateKbps = Math.Round((bytes * 8.0) / (elapsedSeconds * 1000.0), 1);
                Statistics.DroppedFrames += queueDropped;
                StatisticsUpdated?.Invoke(Statistics);

                long lastTicks = Interlocked.Read(ref _lastFrameTimestampTicksUtc);
                if (_hasActiveVideoSignal && lastTicks > 0)
                {
                    double silentMs = (DateTime.UtcNow - new DateTime(lastTicks, DateTimeKind.Utc)).TotalMilliseconds;
                    if (silentMs > _settings.SignalLossTimeoutMs)
                    {
                        _hasActiveVideoSignal = false;
                        StatusChanged?.Invoke($"No video signal (listening on UDP port {_settings.ListenPort}...)");
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void UpdateReceiverStatistics()
    {
        var receiver = _udpReceiver;
        if (receiver is null)
        {
            return;
        }

        long bytes = receiver.BytesReceived;
        long dropped = receiver.DroppedFrames;
        long invalid = receiver.InvalidPackets;
        long receivedDelta = bytes - _lastUdpBytesReceived;
        Interlocked.Add(ref _bytesReceivedInWindow, receivedDelta);
        Statistics.TotalBytesTransferred += receivedDelta;
        Statistics.DroppedFrames += dropped - _lastUdpDroppedFrames;
        Statistics.InvalidPackets += invalid - _lastUdpInvalidPackets;
        _lastUdpBytesReceived = bytes;
        _lastUdpDroppedFrames = dropped;
        _lastUdpInvalidPackets = invalid;
    }

    private static async Task AwaitTaskQuietlyAsync(Task? task)
    {
        if (task is null)
        {
            return;
        }

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _serverCts?.Cancel();
        _decodeChannel?.Writer.TryComplete();
        _udpReceiver?.Dispose();
        _videoRecorder.Dispose();
        _faceDetectionService.Dispose();
        _serverCts?.Dispose();
    }
}
