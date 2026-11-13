using System;
using System.Buffers.Binary;

namespace LanStream.Shared.Protocol;

/// <summary>
/// Chịu trách nhiệm đóng gói (Serialize) và giải mã (Deserialize) UDP datagram
/// theo chuẩn Big-Endian (Network Byte Order).
/// Phụ trách: TV3 (thống nhất cùng TV1).
/// </summary>
public static class PacketSerializer
{
    /// <summary>
    /// Ghi thông tin <see cref="PacketHeader"/> (24 byte) và phần dữ liệu JPEG chunk vào một mảng byte datagram.
    /// </summary>
    /// <param name="header">Thông tin header 24 byte.</param>
    /// <param name="payload">Dữ liệu phần cắt JPEG (tối đa 1.176 byte).</param>
    /// <returns>Mảng byte hoàn chỉnh để gửi qua UDP.</returns>
    public static byte[] SerializePacket(in PacketHeader header, ReadOnlySpan<byte> payload)
    {
        if (!header.IsValid)
        {
            throw new ArgumentException("Packet header is invalid.", nameof(header));
        }

        if (payload.Length > ProtocolConstants.MaxPayloadSize)
        {
            throw new ArgumentOutOfRangeException(nameof(payload), "Payload exceeds the UDP datagram limit.");
        }

        byte[] datagram = new byte[ProtocolConstants.HeaderSize + payload.Length];
        Span<byte> headerBytes = datagram.AsSpan(0, ProtocolConstants.HeaderSize);
        BinaryPrimitives.WriteUInt16BigEndian(headerBytes, header.Magic);
        headerBytes[2] = header.Version;
        headerBytes[3] = header.Flags;
        BinaryPrimitives.WriteUInt32BigEndian(headerBytes[4..], header.StreamId);
        BinaryPrimitives.WriteUInt32BigEndian(headerBytes[8..], header.FrameId);
        BinaryPrimitives.WriteUInt16BigEndian(headerBytes[12..], header.ChunkIndex);
        BinaryPrimitives.WriteUInt16BigEndian(headerBytes[14..], header.ChunkCount);
        BinaryPrimitives.WriteInt64BigEndian(headerBytes[16..], header.TimestampMs);
        payload.CopyTo(datagram.AsSpan(ProtocolConstants.HeaderSize));
        return datagram;
    }

    /// <summary>
    /// Đọc và kiểm tra tính hợp lệ của gói UDP nhận được, tách riêng Header và Payload.
    /// </summary>
    /// <param name="datagram">Toàn bộ gói UDP nhận được từ socket.</param>
    /// <param name="header">Header đọc được nếu hợp lệ.</param>
    /// <param name="payload">Phần dữ liệu JPEG trích xuất từ offset 24 trở đi.</param>
    /// <returns><c>true</c> nếu gói đúng cấu trúc và hợp lệ; ngược lại <c>false</c>.</returns>
    public static bool TryDeserializePacket(
        ReadOnlySpan<byte> datagram,
        out PacketHeader header,
        out ReadOnlySpan<byte> payload)
    {
        header = default;
        payload = default;
        if (datagram.Length < ProtocolConstants.HeaderSize ||
            datagram.Length > ProtocolConstants.MaxDatagramSize)
        {
            return false;
        }

        header = new PacketHeader(
            Magic: BinaryPrimitives.ReadUInt16BigEndian(datagram),
            Version: datagram[2],
            Flags: datagram[3],
            StreamId: BinaryPrimitives.ReadUInt32BigEndian(datagram[4..]),
            FrameId: BinaryPrimitives.ReadUInt32BigEndian(datagram[8..]),
            ChunkIndex: BinaryPrimitives.ReadUInt16BigEndian(datagram[12..]),
            ChunkCount: BinaryPrimitives.ReadUInt16BigEndian(datagram[14..]),
            TimestampMs: BinaryPrimitives.ReadInt64BigEndian(datagram[16..]));

        if (!header.IsValid)
        {
            header = default;
            return false;
        }

        payload = datagram[ProtocolConstants.HeaderSize..];
        return true;
    }
}
