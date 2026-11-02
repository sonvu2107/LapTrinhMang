using LanStream.Shared.Protocol;

namespace LanStream.Client.Config;

/// <summary>
/// Cấu hình kết nối và thông số thu hình của Client.
/// Phụ trách: TV2.
/// </summary>
public sealed class ClientSettings
{
    /// <summary>
    /// Địa chỉ IPv4 của máy Server trong mạng LAN (mặc định loopback khi chạy thử cục bộ).
    /// </summary>
    public string ServerIp { get; set; } = "127.0.0.1";

    /// <summary>
    /// Cổng UDP lắng nghe trên Server.
    /// </summary>
    public int ServerPort { get; set; } = ProtocolConstants.DefaultUdpPort;

    /// <summary>
    /// Chỉ số thiết bị Webcam (0 = camera mặc định).
    /// </summary>
    public int CameraIndex { get; set; } = 0;

    /// <summary>
    /// Chiều rộng khung hình sau khi resize.
    /// </summary>
    public int FrameWidth { get; set; } = ProtocolConstants.DefaultFrameWidth;

    /// <summary>
    /// Chiều cao khung hình sau khi resize.
    /// </summary>
    public int FrameHeight { get; set; } = ProtocolConstants.DefaultFrameHeight;

    /// <summary>
    /// Số khung hình mục tiêu mỗi giây (~15 FPS).
    /// </summary>
    public int TargetFps { get; set; } = ProtocolConstants.DefaultTargetFps;

    /// <summary>
    /// Chất lượng nén ảnh JPEG (1..100, khuyến nghị 60..70 cho mạng LAN).
    /// </summary>
    public int JpegQuality { get; set; } = ProtocolConstants.DefaultJpegQuality;
}
