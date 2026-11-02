using System;

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
        // TODO [TV3]: Sử dụng BinaryPrimitives.WriteUInt16BigEndian / WriteUInt32BigEndian / WriteInt64BigEndian
        // để ghi 24 byte header theo bảng offset tại docs/UDP_PROTOCOL.md, sau đó copy payload vào sau offset 24.
        throw new NotImplementedException("TODO [TV3]: Triển khai SerializePacket theo chuẩn Big-Endian.");
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
        // TODO [TV3]: Kiểm tra datagram.Length >= 24 && <= 1200, đọc Big-Endian các trường Header,
        // kiểm tra header.IsValid và cắt lát payload = datagram.Slice(ProtocolConstants.HeaderSize).
        header = default;
        payload = default;
        throw new NotImplementedException("TODO [TV3]: Triển khai TryDeserializePacket.");
    }
}
