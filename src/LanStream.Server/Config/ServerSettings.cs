using LanStream.Shared.Protocol;

namespace LanStream.Server.Config;

/// <summary>
/// Cấu hình lắng nghe mạng, lưu trữ video và nhận diện khuôn mặt của Server.
/// Phụ trách: TV1.
/// </summary>
public sealed class ServerSettings
{
    /// <summary>
    /// Cổng UDP lắng nghe các gói video gửi tới.
    /// </summary>
    public int ListenPort { get; set; } = ProtocolConstants.DefaultUdpPort;

    /// <summary>
    /// Thời gian chờ tối đa (ms) để ghép đủ các chunk của một khung hình trước khi hủy bỏ.
    /// </summary>
    public int FrameTimeoutMs { get; set; } = ProtocolConstants.DefaultFrameTimeoutMs;

    /// <summary>
    /// Thư mục lưu các tệp video được ghi hình trên Server.
    /// </summary>
    public string RecordingsDirectory { get; set; } = "recordings";

    /// <summary>
    /// Đường dẫn tới tệp model phát hiện khuôn mặt (Haar Cascade .xml hoặc YuNet .onnx).
    /// </summary>
    public string FaceModelPath { get; set; } = "assets/models/haarcascade_frontalface_default.xml";

    /// <summary>
    /// Bật/tắt chế độ phát hiện khuôn mặt trên khung hình nhận được.
    /// </summary>
    public bool EnableFaceDetection { get; set; }
}
