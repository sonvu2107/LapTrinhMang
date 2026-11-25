using LanStream.Shared.Protocol;

namespace LanStream.Server.Config;

/// <summary>
/// Configuration settings for the LAN Video Streaming Server.
/// </summary>
public sealed class ServerSettings
{
    internal const string DefaultFaceModelPath = "assets/models/face_detection_yunet_2023mar.onnx";

    public int ListenPort { get; set; } = ProtocolConstants.DefaultUdpPort;

    public int TargetFps { get; set; } = ProtocolConstants.DefaultTargetFps;

    public int FrameWidth { get; set; } = ProtocolConstants.DefaultFrameWidth;

    public int FrameHeight { get; set; } = ProtocolConstants.DefaultFrameHeight;

    public int FrameTimeoutMs { get; set; } = ProtocolConstants.DefaultFrameTimeoutMs;

    public int SignalLossTimeoutMs { get; set; } = 2500;

    public int MaxDecodeQueueSize { get; set; } = 2;

    public string RecordingsDirectory { get; set; } = "recordings";

    public string FaceModelPath { get; set; } = DefaultFaceModelPath;

    public bool EnableFaceDetection { get; set; }

    public bool Validate(out string errorMessage)
    {
        if (ListenPort is < 1024 or > 65535)
        {
            errorMessage = "UDP port must be between 1024 and 65535.";
            return false;
        }

        if (TargetFps is < 1 or > 60)
        {
            errorMessage = "Target FPS must be between 1 and 60.";
            return false;
        }

        if (FrameWidth <= 0 || FrameHeight <= 0)
        {
            errorMessage = "Frame dimensions must be greater than zero.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(RecordingsDirectory))
        {
            errorMessage = "Recordings directory cannot be empty.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
