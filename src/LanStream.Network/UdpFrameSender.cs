using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LanStream.Network;

/// <summary>
/// Đóng vai trò gửi bất đồng bộ các gói UDP của một khung hình JPEG từ Client sang Server.
/// Phụ trách: TV3.
/// </summary>
public sealed class UdpFrameSender : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly FramePacketizer _packetizer;
    private bool _disposed;

    public UdpFrameSender(FramePacketizer? packetizer = null)
    {
        _udpClient = new UdpClient();
        _packetizer = packetizer ?? new FramePacketizer();
    }

    /// <summary>
    /// Chia nhỏ khung hình JPEG và gửi lần lượt từng gói UDP tới địa chỉ đích bất đồng bộ.
    /// </summary>
    /// <param name="jpegBytes">Dữ liệu ảnh JPEG.</param>
    /// <param name="streamId">Mã phiên stream.</param>
    /// <param name="frameId">Số thứ tự khung hình.</param>
    /// <param name="serverEndPoint">Địa chỉ IP và cổng UDP của Server.</param>
    /// <param name="cancellationToken">Token hủy tác vụ khi nhấn Stop.</param>
    /// <returns>Tổng số byte UDP đã gửi.</returns>
    public async Task<int> SendFrameAsync(
        byte[] jpegBytes,
        uint streamId,
        uint frameId,
        IPEndPoint serverEndPoint,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentNullException.ThrowIfNull(jpegBytes);
        ArgumentNullException.ThrowIfNull(serverEndPoint);

        IReadOnlyList<byte[]> packets = _packetizer.Packetize(jpegBytes, streamId, frameId);
        int totalBytesSent = 0;
        foreach (byte[] packet in packets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            totalBytesSent += await _udpClient.SendAsync(
                packet,
                serverEndPoint,
                cancellationToken).ConfigureAwait(false);
        }

        return totalBytesSent;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _udpClient.Dispose();
        _disposed = true;
    }
}
