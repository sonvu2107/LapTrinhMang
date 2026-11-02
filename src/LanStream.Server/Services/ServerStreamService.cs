using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using LanStream.Network;
using LanStream.Server.Config;
using LanStream.Shared.Models;

namespace LanStream.Server.Services;

/// <summary>
/// Điều phối luồng xử lý phía Server: Lắng nghe UDP qua <see cref="UdpFrameReceiver"/>,
/// giải mã bằng <see cref="FrameDecoder"/>, ghi hình qua <see cref="VideoRecorder"/>,
/// xử lý nhận diện khuôn mặt (nếu bật) và đẩy ảnh lên <c>ServerForm</c>.
/// Phụ trách: TV1.
/// </summary>
public sealed class ServerStreamService : IDisposable
{
    private readonly FrameDecoder _frameDecoder;
    private readonly VideoRecorder _videoRecorder;
    private readonly FaceDetectionService _faceDetectionService;
    private UdpFrameReceiver? _udpReceiver;
    private CancellationTokenSource? _serverCts;
    private bool _disposed;

    /// <summary>
    /// Sự kiện phát ra mỗi khi có khung hình mới đã giải mã sẵn sàng hiển thị lên UI.
    /// </summary>
    public event Action<Bitmap, VideoFrame>? FrameDecoded;

    /// <summary>
    /// Sự kiện cập nhật các chỉ số FPS nhận, số frame mất và băng thông.
    /// </summary>
    public event Action<StreamStatistics>? StatisticsUpdated;

    private void OnFrameDecoded(Bitmap bitmap, VideoFrame frame) => FrameDecoded?.Invoke(bitmap, frame);
    private void OnStatisticsUpdated() => StatisticsUpdated?.Invoke(Statistics);

    public StreamStatistics Statistics { get; } = new();

    public bool IsListening { get; private set; }

    public bool IsRecording => _videoRecorder.IsRecording;

    public ServerStreamService(
        FrameDecoder? frameDecoder = null,
        VideoRecorder? videoRecorder = null,
        FaceDetectionService? faceDetectionService = null)
    {
        _frameDecoder = frameDecoder ?? new FrameDecoder();
        _videoRecorder = videoRecorder ?? new VideoRecorder();
        _faceDetectionService = faceDetectionService ?? new FaceDetectionService();
    }

    /// <summary>
    /// Bắt đầu lắng nghe luồng video UDP trên cổng cấu hình.
    /// </summary>
    public async Task StartServerAsync(ServerSettings settings)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // TODO [TV1]:
        // 1. Khởi tạo _serverCts = new CancellationTokenSource(), Statistics.Reset().
        // 2. Tạo _udpReceiver = new UdpFrameReceiver(settings.ListenPort, new FrameAssembler(settings.FrameTimeoutMs)).
        // 3. Đăng ký _udpReceiver.FrameReceived -> đưa vào Channel giới hạn (1-2 frame) để giải mã,
        //    ghi hình (nếu đang Record), phát hiện khuôn mặt (nếu bật) và gọi FrameDecoded.
        // 4. Chạy await _udpReceiver.ReceiveLoopAsync(_serverCts.Token).
        await Task.CompletedTask;
        throw new NotImplementedException("TODO [TV1]: Triển khai StartServerAsync tích hợp UDP Receiver -> Decoder -> UI.");
    }

    /// <summary>
    /// Dừng lắng nghe UDP và đóng tệp ghi hình (nếu đang mở) một cách an toàn.
    /// </summary>
    public async Task StopServerAsync()
    {
        if (_serverCts is not null)
        {
            await _serverCts.CancelAsync();
            _serverCts.Dispose();
            _serverCts = null;
        }

        _videoRecorder.Stop();
        _udpReceiver?.Dispose();
        _udpReceiver = null;
        IsListening = false;
    }

    /// <summary>
    /// Bắt đầu ghi lại video đang nhận vào thư mục <c>recordings/</c>.
    /// </summary>
    public string StartRecording(string recordingsDirectory)
    {
        // TODO [TV1]: Tạo tên file theo timestamp và gọi _videoRecorder.Start(...).
        throw new NotImplementedException("TODO [TV1]: Triển khai StartRecording.");
    }

    /// <summary>
    /// Dừng ghi video.
    /// </summary>
    public void StopRecording()
    {
        _videoRecorder.Stop();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _serverCts?.Cancel();
        _serverCts?.Dispose();
        _videoRecorder.Dispose();
        _faceDetectionService.Dispose();
        _udpReceiver?.Dispose();
        _disposed = true;
    }
}
