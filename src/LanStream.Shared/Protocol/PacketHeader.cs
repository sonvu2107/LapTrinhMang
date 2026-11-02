namespace LanStream.Shared.Protocol;

/// <summary>
/// Cấu trúc Header 24-byte của mỗi gói tin UDP truyền khung hình (Big-Endian / Network Byte Order).
/// </summary>
/// <param name="Magic">2 byte (Offset 0): Mã định danh gói (0x4C56).</param>
/// <param name="Version">1 byte (Offset 2): Phiên bản giao thức (1).</param>
/// <param name="Flags">1 byte (Offset 3): Cờ loại gói tin (0 = DATA).</param>
/// <param name="StreamId">4 byte (Offset 4): Mã phiên stream do Client tạo khi nhấn Start.</param>
/// <param name="FrameId">4 byte (Offset 8): Số thứ tự khung hình tăng dần.</param>
/// <param name="ChunkIndex">2 byte (Offset 12): Chỉ số mảnh hiện tại (0 .. ChunkCount - 1).</param>
/// <param name="ChunkCount">2 byte (Offset 14): Tổng số mảnh của khung hình (1 .. 256).</param>
/// <param name="TimestampMs">8 byte (Offset 16): Thời điểm gửi tính theo Unix epoch milliseconds.</param>
public readonly record struct PacketHeader(
    ushort Magic,
    byte Version,
    byte Flags,
    uint StreamId,
    uint FrameId,
    ushort ChunkIndex,
    ushort ChunkCount,
    long TimestampMs)
{
    /// <summary>
    /// Kiểm tra các điều kiện hợp lệ cơ bản của Header theo đặc tả giao thức.
    /// </summary>
    public bool IsValid =>
        Magic == ProtocolConstants.Magic &&
        Version == ProtocolConstants.CurrentVersion &&
        ChunkCount >= 1 &&
        ChunkCount <= ProtocolConstants.MaxChunksPerFrame &&
        ChunkIndex < ChunkCount;
}
