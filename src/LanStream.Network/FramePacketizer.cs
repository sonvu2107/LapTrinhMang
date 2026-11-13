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
        if (jpegBytes.IsEmpty || jpegBytes.Length > ProtocolConstants.MaxFrameSizeBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(jpegBytes),
                $"Frame size must be between 1 and {ProtocolConstants.MaxFrameSizeBytes} bytes.");
        }

        int chunkCount = (jpegBytes.Length + _maxPayloadSize - 1) / _maxPayloadSize;
        if (chunkCount > ProtocolConstants.MaxChunksPerFrame)
        {
            throw new ArgumentOutOfRangeException(
                nameof(jpegBytes),
                $"Frame requires more than {ProtocolConstants.MaxChunksPerFrame} chunks.");
        }

        long frameTimestampMs = timestampMs ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var packets = new byte[chunkCount][];
        for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
        {
            int offset = chunkIndex * _maxPayloadSize;
            int payloadLength = Math.Min(_maxPayloadSize, jpegBytes.Length - offset);
            var header = new PacketHeader(
                Magic: ProtocolConstants.Magic,
                Version: ProtocolConstants.CurrentVersion,
                Flags: ProtocolConstants.FlagData,
                StreamId: streamId,
                FrameId: frameId,
                ChunkIndex: (ushort)chunkIndex,
                ChunkCount: (ushort)chunkCount,
                TimestampMs: frameTimestampMs);
            packets[chunkIndex] = PacketSerializer.SerializePacket(
                header,
                jpegBytes.Slice(offset, payloadLength));
        }

        return packets;
    }
}
