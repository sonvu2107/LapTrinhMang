using System.Net;

namespace LanStream.Shared.Models;

/// <summary>
/// Đại diện cho một khung hình JPEG hoàn chỉnh sau khi nén (Client) hoặc ghép gói thành công (Server).
/// </summary>
public sealed class VideoFrame
{
    /// <summary>
    /// Mã định danh phiên truyền phát.
    /// </summary>
    public uint StreamId { get; init; }

    /// <summary>
    /// Số thứ tự khung hình tăng dần.
    /// </summary>
    public uint FrameId { get; init; }

    /// <summary>
    /// Mốc thời gian tạo/gửi khung hình (Unix time milliseconds UTC).
    /// </summary>
    public long TimestampMs { get; init; }

    /// <summary>
    /// Dữ liệu ảnh nén định dạng JPEG.
    /// </summary>
    public required byte[] JpegBytes { get; init; }

    /// <summary>
    /// Địa chỉ nguồn gửi khung hình (sử dụng ở phía Server).
    /// </summary>
    public IPEndPoint? RemoteEndPoint { get; init; }
}
