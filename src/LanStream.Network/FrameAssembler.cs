using System;
using System.Net;
using LanStream.Shared.Models;
using LanStream.Shared.Protocol;

namespace LanStream.Network;

/// <summary>
/// Gom các mảnh UDP nhận được theo khóa (RemoteEndPoint, StreamId, FrameId) thành khung hình JPEG hoàn chỉnh.
/// Xử lý gói đến sai thứ tự, bỏ gói trùng và loại bỏ các khung hình quá thời gian chờ (Timeout).
/// Phụ trách: TV3.
/// </summary>
public sealed class FrameAssembler
{
    private readonly TimeSpan _frameTimeout;

    public FrameAssembler(int frameTimeoutMs = ProtocolConstants.DefaultFrameTimeoutMs)
    {
        _frameTimeout = TimeSpan.FromMilliseconds(frameTimeoutMs);
    }

    /// <summary>
    /// Đưa một gói UDP nhận được vào bộ đệm lắp ghép.
    /// </summary>
    /// <param name="remoteEndPoint">Địa chỉ IP:Port của Client gửi gói tin.</param>
    /// <param name="datagram">Toàn bộ gói UDP thô nhận từ socket.</param>
    /// <param name="completedFrame">Khung hình hoàn chỉnh nếu gói vừa nhận là mảnh ghép cuối cùng; ngược lại <c>null</c>.</param>
    /// <returns><c>true</c> nếu vừa ghép xong trọn vẹn một khung hình; ngược lại <c>false</c>.</returns>
    public bool TryAddPacket(
        IPEndPoint remoteEndPoint,
        ReadOnlySpan<byte> datagram,
        out VideoFrame? completedFrame)
    {
        // TODO [TV3]:
        // 1. Giải mã gói tin qua PacketSerializer.TryDeserializePacket.
        // 2. Lưu payload vào buffer theo khóa (remoteEndPoint, header.StreamId, header.FrameId).
        // 3. Bỏ qua nếu ChunkIndex đã nhận trước đó (trùng lặp).
        // 4. Khi nhận đủ ChunkCount mảnh (và tổng dung lượng <= MaxFrameSizeBytes), nối thành byte[] JPEG
        //    và trả về VideoFrame hoàn chỉnh, đồng thời xóa buffer của frame đó.
        completedFrame = null;
        throw new NotImplementedException("TODO [TV3]: Triển khai lắp ghép các chunk thành VideoFrame.");
    }

    /// <summary>
    /// Dọn dẹp các khung hình chưa nhận đủ mảnh sau khi vượt quá thời gian chờ (<c>200 ms</c>).
    /// </summary>
    /// <returns>Số lượng khung hình bị hủy bỏ do quá hạn.</returns>
    public int CleanupExpiredFrames()
    {
        // TODO [TV3]: Duyệt các frame đang chờ trong bộ đệm, xóa các mục quá _frameTimeout và trả về số frame đã xóa.
        throw new NotImplementedException("TODO [TV3]: Triển khai dọn dẹp frame quá hạn.");
    }
}
