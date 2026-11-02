using System;
using System.Collections.Generic;
using LanStream.Shared.Protocol;

namespace LanStream.Network;

/// <summary>
/// Chia một mảng byte ảnh JPEG hoàn chỉnh thành danh sách các gói UDP (datagram <= 1.200 byte).
/// Phụ trách: TV3.
/// </summary>
public sealed class FramePacketizer
{
    private readonly int _maxPayloadSize;

    public FramePacketizer(int maxPayloadSize = ProtocolConstants.MaxPayloadSize)
    {
        if (maxPayloadSize <= 0 || maxPayloadSize > ProtocolConstants.MaxPayloadSize)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPayloadSize));
        }

        _maxPayloadSize = maxPayloadSize;
    }

    /// <summary>
    /// Cắt dữ liệu JPEG thành các gói UDP kèm Header 24-byte.
    /// </summary>
    /// <param name="jpegBytes">Mảng byte ảnh JPEG đã nén.</param>
    /// <param name="streamId">ID phiên stream hiện tại.</param>
    /// <param name="frameId">Số thứ tự khung hình.</param>
    /// <param name="timestampMs">Thời điểm gửi (nếu bỏ trống sẽ lấy thời gian UTC hiện tại).</param>
    /// <returns>Danh sách các gói UDP hoàn chỉnh sẵn sàng gửi qua socket.</returns>
    public IReadOnlyList<byte[]> Packetize(
        ReadOnlySpan<byte> jpegBytes,
        uint streamId,
        uint frameId,
        long? timestampMs = null)
    {
        // TODO [TV3]:
        // 1. Kiểm tra jpegBytes.Length > 0 và <= ProtocolConstants.MaxFrameSizeBytes.
        // 2. Tính chunkCount = (ushort)Math.Ceiling((double)jpegBytes.Length / _maxPayloadSize).
        // 3. Tạo PacketHeader cho từng chunkIndex (0 .. chunkCount - 1) và gọi PacketSerializer.SerializePacket.
        throw new NotImplementedException("TODO [TV3]: Triển khai chia nhỏ JPEG thành các gói UDP.");
    }
}
