using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LanStream.Shared.Models;
using LanStream.Shared.Protocol;

namespace LanStream.Network;

/// <summary>
/// Lắng nghe cổng UDP trên Server, nhận gói tin bất đồng bộ và chuyển cho <see cref="FrameAssembler"/>.
/// Phụ trách: TV3.
/// </summary>
public sealed class UdpFrameReceiver : IDisposable
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMilliseconds(50);
    private readonly int _listenPort;
    private readonly FrameAssembler _assembler;
    private UdpClient? _udpClient = null;
    private bool _disposed;
    private bool _isRunning;
    private long _bytesReceived;

    /// <summary>
    /// Sự kiện kích hoạt mỗi khi lắp ghép hoàn chỉnh một khung hình JPEG hợp lệ.
    /// </summary>
    public event Action<VideoFrame>? FrameReceived;

    public long BytesReceived => Interlocked.Read(ref _bytesReceived);
    public long DroppedFrames => _assembler.DroppedFrames;
    public long InvalidPackets => _assembler.InvalidPackets;

    private void OnFrameReceived(VideoFrame frame) => FrameReceived?.Invoke(frame);

    public UdpFrameReceiver(
        int listenPort = ProtocolConstants.DefaultUdpPort,
        FrameAssembler? assembler = null)
    {
        _listenPort = listenPort;
        _assembler = assembler ?? new FrameAssembler();
    }

    /// <summary>
    /// Vòng lặp nhận dữ liệu UDP bất đồng bộ cho tới khi <paramref name="cancellationToken"/> bị hủy.
    /// </summary>
    public async Task ReceiveLoopAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_isRunning)
        {
            throw new InvalidOperationException("The UDP receive loop is already running.");
        }

        _isRunning = true;
        try
        {
            _udpClient = new UdpClient(_listenPort);
            Task<UdpReceiveResult>? receiveTask = null;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                receiveTask ??= _udpClient.ReceiveAsync(cancellationToken).AsTask();

                using var delayCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                Task delayTask = Task.Delay(CleanupInterval, delayCts.Token);
                Task completedTask = await Task.WhenAny(receiveTask, delayTask).ConfigureAwait(false);
                if (completedTask == delayTask)
                {
                    await delayTask.ConfigureAwait(false);
                    _assembler.CleanupExpiredFrames();
                    continue;
                }

                delayCts.Cancel();
                UdpReceiveResult result = await receiveTask.ConfigureAwait(false);
                receiveTask = null;
                Interlocked.Add(ref _bytesReceived, result.Buffer.Length);
                if (_assembler.TryAddPacket(result.RemoteEndPoint, result.Buffer, out VideoFrame? frame) &&
                    frame is not null)
                {
                    OnFrameReceived(frame);
                }

                _assembler.CleanupExpiredFrames();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _udpClient?.Dispose();
            _udpClient = null;
            _isRunning = false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _udpClient?.Dispose();
        _disposed = true;
    }
}
