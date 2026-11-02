namespace LanStream.Shared.Models;

/// <summary>
/// Lưu trữ thông số thống kê tốc độ khung hình, băng thông và mất gói để hiển thị trên UI.
/// </summary>
public sealed class StreamStatistics
{
    /// <summary>
    /// Tốc độ khung hình hiện tại (Frames Per Second).
    /// </summary>
    public double CurrentFps { get; set; }

    /// <summary>
    /// Tổng số khung hình gửi hoặc ghép hoàn chỉnh.
    /// </summary>
    public long CompletedFrames { get; set; }

    /// <summary>
    /// Tổng số khung hình bị hủy bỏ do thiếu gói / quá thời gian chờ (Timeout).
    /// </summary>
    public long DroppedFrames { get; set; }

    /// <summary>
    /// Tổng số gói UDP không hợp lệ đã bị loại bỏ.
    /// </summary>
    public long InvalidPackets { get; set; }

    /// <summary>
    /// Tổng dung lượng dữ liệu đã truyền/nhận (byte).
    /// </summary>
    public long TotalBytesTransferred { get; set; }

    /// <summary>
    /// Băng thông ước tính hiện tại (Kilobits per second - Kbps).
    /// </summary>
    public double EstimatedBitrateKbps { get; set; }

    /// <summary>
    /// Đặt lại toàn bộ bộ đếm về 0 khi bắt đầu phiên mới.
    /// </summary>
    public void Reset()
    {
        CurrentFps = 0;
        CompletedFrames = 0;
        DroppedFrames = 0;
        InvalidPackets = 0;
        TotalBytesTransferred = 0;
        EstimatedBitrateKbps = 0;
    }
}
