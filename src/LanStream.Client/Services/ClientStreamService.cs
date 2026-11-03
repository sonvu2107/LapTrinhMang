using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LanStream.Client.Config;
using LanStream.Network;
using LanStream.Shared.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace LanStream.Client.Services;

/// <summary>
/// Điều phối luồng làm việc phía Client: Nhận frame từ <see cref="CameraService"/>,
/// cập nhật preview lên UI, nén qua <see cref="FrameEncoder"/> và gửi bằng <see cref="UdpFrameSender"/>.
/// Phụ trách: TV2.
/// </summary>
public sealed class ClientStreamService : IDisposable
{
    private readonly CameraService _cameraService;
    private readonly FrameEncoder _frameEncoder;
    private readonly UdpFrameSender _udpSender;
    private CancellationTokenSource? _streamCts;
    private IPEndPoint? _serverEndPoint;
    private uint _streamId;
    private uint _frameId;
    private int _jpegQuality;
    private int _isSending;
    private bool _disposed;

    // Quản lý đo FPS và băng thông gửi thực tế
    private readonly Stopwatch _statsStopwatch = Stopwatch.StartNew();
    private readonly object _statsLock = new();
    private int _framesSinceLastStats;
    private long _bytesSinceLastStats;

    /// <summary>
    /// Sự kiện đẩy ảnh <see cref="Bitmap"/> lên giao diện <c>ClientForm</c> để hiển thị preview.
    /// </summary>
    public event Action<Bitmap>? PreviewFrameReady;

    /// <summary>
    /// Sự kiện cập nhật thông số FPS và dung lượng gửi định kỳ.
    /// </summary>
    public event Action<StreamStatistics>? StatisticsUpdated;

    private void OnPreviewFrameReady(Bitmap bitmap) => PreviewFrameReady?.Invoke(bitmap);
    private void OnStatisticsUpdated() => StatisticsUpdated?.Invoke(Statistics);

    public StreamStatistics Statistics { get; } = new();

    public bool IsStreaming { get; private set; }

    public bool IsCameraRunning => _cameraService.IsRunning;

    public ClientStreamService(
        CameraService? cameraService = null,
        FrameEncoder? frameEncoder = null,
        UdpFrameSender? udpSender = null)
    {
        _cameraService = cameraService ?? new CameraService();
        _frameEncoder = frameEncoder ?? new FrameEncoder();
        _udpSender = udpSender ?? new UdpFrameSender();

        _cameraService.FrameCaptured += HandleFrameCaptured;
    }

    /// <summary>
    /// Mở camera xem trước cục bộ tại Client (chưa phát luồng UDP tới Server).
    /// </summary>
    public async Task StartCameraPreviewAsync(int cameraIndex, int width, int height, int targetFps)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _cameraService.StartAsync(cameraIndex, width, height, targetFps);
    }

    /// <summary>
    /// Dừng camera xem trước nếu đang không stream.
    /// </summary>
    public async Task StopCameraAsync()
    {
        if (IsStreaming)
        {
            await StopStreamAsync();
            return;
        }

        await _cameraService.StopAsync();
    }

    /// <summary>
    /// Bắt đầu phiên mở Webcam và truyền luồng JPEG UDP tới Server.
    /// </summary>
    public async Task StartStreamAsync(ClientSettings settings)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!IPAddress.TryParse(settings.ServerIp, out var ipAddress))
        {
            throw new ArgumentException($"Địa chỉ IP Server '{settings.ServerIp}' không hợp lệ.");
        }

        if (settings.ServerPort is < 1024 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(settings.ServerPort), "Port phải nằm trong khoảng 1024 đến 65535.");
        }

        _serverEndPoint = new IPEndPoint(ipAddress, settings.ServerPort);
        _streamId = (uint)Random.Shared.Next(1, int.MaxValue);
        _frameId = 0;
        _jpegQuality = settings.JpegQuality;

        lock (_statsLock)
        {
            Statistics.Reset();
            _framesSinceLastStats = 0;
            _bytesSinceLastStats = 0;
            _statsStopwatch.Restart();
        }

        _streamCts = new CancellationTokenSource();

        if (!_cameraService.IsRunning)
        {
            await _cameraService.StartAsync(
                settings.CameraIndex,
                settings.FrameWidth,
                settings.FrameHeight,
                settings.TargetFps,
                _streamCts.Token);
        }

        IsStreaming = true;
    }

    private void HandleFrameCaptured(Mat frame)
    {
        // 1. Chuyển đổi sang Bitmap để hiển thị preview cục bộ lên PictureBox
        try
        {
            Bitmap previewBitmap = BitmapConverter.ToBitmap(frame);
            OnPreviewFrameReady(previewBitmap);
        }
        catch
        {
            // Bỏ qua nếu Form đang đóng hoặc BitmapConverter gặp lỗi
        }

        // 2. Nếu đang bật Streaming và có cấu hình đích Server UDP
        if (!IsStreaming || _serverEndPoint is null)
        {
            return;
        }

        // Bỏ qua nếu lượt gửi frame trước chưa xong (tránh dồn đống độ trễ khi mạng LAN bị nghẽn)
        if (Interlocked.CompareExchange(ref _isSending, 1, 0) != 0)
        {
            lock (_statsLock)
            {
                Statistics.DroppedFrames++;
            }
            return;
        }

        byte[] jpegBytes;
        try
        {
            jpegBytes = _frameEncoder.EncodeJpeg(frame, _jpegQuality);
        }
        catch
        {
            Interlocked.Exchange(ref _isSending, 0);
            return;
        }

        uint currentFrameId = _frameId++;
        var endpoint = _serverEndPoint;
        var token = _streamCts?.Token ?? default;

        _ = Task.Run(async () =>
        {
            try
            {
                int bytesSent = await _udpSender.SendFrameAsync(jpegBytes, _streamId, currentFrameId, endpoint, token);
                UpdateStatistics(bytesSent);
            }
            catch (NotImplementedException)
            {
                // Nếu TV3 chưa viết xong UdpFrameSender, không làm sập camera preview
            }
            catch (Exception)
            {
                lock (_statsLock)
                {
                    Statistics.DroppedFrames++;
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isSending, 0);
            }
        }, token);
    }

    private void UpdateStatistics(int bytesSent)
    {
        lock (_statsLock)
        {
            Statistics.CompletedFrames++;
            Statistics.TotalBytesTransferred += bytesSent;
            _framesSinceLastStats++;
            _bytesSinceLastStats += bytesSent;

            long elapsedMs = _statsStopwatch.ElapsedMilliseconds;
            if (elapsedMs >= 1000)
            {
                Statistics.CurrentFps = Math.Round((_framesSinceLastStats * 1000.0) / elapsedMs, 1);
                Statistics.EstimatedBitrateKbps = Math.Round((_bytesSinceLastStats * 8.0) / elapsedMs, 1);
                _framesSinceLastStats = 0;
                _bytesSinceLastStats = 0;
                _statsStopwatch.Restart();
                OnStatisticsUpdated();
            }
        }
    }

    /// <summary>
    /// Dừng phiên truyền phát hiện tại an toàn, không làm treo UI.
    /// </summary>
    public async Task StopStreamAsync()
    {
        IsStreaming = false;

        if (_streamCts is not null)
        {
            await _streamCts.CancelAsync();
            _streamCts.Dispose();
            _streamCts = null;
        }

        await _cameraService.StopAsync();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cameraService.FrameCaptured -= HandleFrameCaptured;
        _streamCts?.Cancel();
        _streamCts?.Dispose();
        _cameraService.Dispose();
        _udpSender.Dispose();
        _disposed = true;
    }
}
