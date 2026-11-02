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
    private readonly int _listenPort;
    private readonly FrameAssembler _assembler;
    private UdpClient? _udpClient = null;
    private bool _disposed;

    /// <summary>
    /// Sự kiện kích hoạt mỗi khi lắp ghép hoàn chỉnh một khung hình JPEG hợp lệ.
    /// </summary>
    public event Action<VideoFrame>? FrameReceived;

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

        // TODO [TV3]:
        // 1. Khởi tạo _udpClient = new UdpClient(_listenPort).
        // 2. Trong vòng lặp while (!cancellationToken.IsCancellationRequested):
        //    - Gọi var result = await _udpClient.ReceiveAsync(cancellationToken).
        //    - Đưa result.RemoteEndPoint và result.Buffer vào _assembler.TryAddPacket(...).
        //    - Nếu hoàn thành frame, phát sự kiện FrameReceived?.Invoke(frame).
        //    - Định kỳ gọi _assembler.CleanupExpiredFrames() để dọn các frame quá hạn 200ms.
        // 3. Bắt OperationCanceledException an toàn khi Stop.
        await Task.CompletedTask;
        throw new NotImplementedException("TODO [TV3]: Triển khai vòng lặp nhận UDP bất đồng bộ.");
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
