namespace LanStream.Shared.Protocol;

/// <summary>
/// Các hằng số cấu hình mặc định cho giao thức truyền video UDP (Phiên bản 1).
/// </summary>
public static class ProtocolConstants
{
    /// <summary>
    /// Mã nhận diện gói tin hợp lệ: 0x4C56 ('L', 'V' - LanVideo).
    /// </summary>
    public const ushort Magic = 0x4C56;

    /// <summary>
    /// Phiên bản giao thức hiện tại.
    /// </summary>
    public const byte CurrentVersion = 1;

    /// <summary>
    /// Cờ đánh dấu gói dữ liệu khung hình JPEG thông thường (DATA).
    /// </summary>
    public const byte FlagData = 0x00;

    /// <summary>
    /// Cổng UDP lắng nghe mặc định trên Server.
    /// </summary>
    public const int DefaultUdpPort = 5000;

    /// <summary>
    /// Kích thước cố định của phần Header trong mỗi gói UDP (byte).
    /// </summary>
    public const int HeaderSize = 24;

    /// <summary>
    /// Kích thước tối đa của toàn bộ một gói UDP ứng dụng (Header + Payload).
    /// </summary>
    public const int MaxDatagramSize = 1200;

    /// <summary>
    /// Kích thước tối đa của dữ liệu ảnh JPEG trong một gói UDP (1.200 - 24 = 1.176 byte).
    /// </summary>
    public const int MaxPayloadSize = MaxDatagramSize - HeaderSize;

    /// <summary>
    /// Số lượng chunk tối đa cho phép trên một khung hình.
    /// </summary>
    public const ushort MaxChunksPerFrame = 256;

    /// <summary>
    /// Kích thước tối đa của một khung hình JPEG sau khi ghép (256 KiB).
    /// </summary>
    public const int MaxFrameSizeBytes = 256 * 1024;

    /// <summary>
    /// Thời gian chờ tối đa để nhận đủ các chunk của một khung hình (ms).
    /// </summary>
    public const int DefaultFrameTimeoutMs = 200;

    // Thông số hình ảnh khởi đầu cho MVP
    public const int DefaultFrameWidth = 640;
    public const int DefaultFrameHeight = 480;
    public const int DefaultTargetFps = 15;
    public const int DefaultJpegQuality = 60;
}
