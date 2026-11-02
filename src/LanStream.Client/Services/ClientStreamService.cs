using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using LanStream.Client.Config;
using LanStream.Network;
using LanStream.Shared.Models;

namespace LanStream.Client.Services;

/// <summary>
/// Điều phối luồng làm việc phía Client: Nhận frame từ <see cref="CameraService"/>,
/// cập nhật preview lên UI, nén qua <see cref="FrameEncoder"/> và gửi bằng <see cref="UdpFrameSender"/>.
/// Phụ trách: TV2.
/// </summary>
public sealed class ClientStreamService : IDisposable
{
    private readonly CameraService _cameraService;
    private readonly FrameEncoder _frameEncoder;
    private readonly UdpFrameSender _udpSender;
    private CancellationTokenSource? _streamCts;
    private bool _disposed;

    /// <summary>
    /// Sự kiện đẩy ảnh <see cref="Bitmap"/> lên giao diện <c>ClientForm</c> để hiển thị preview.
    /// </summary>
    public event Action<Bitmap>? PreviewFrameReady;

    /// <summary>
    /// Sự kiện cập nhật thông số FPS và dung lượng gửi định kỳ.
    /// </summary>
    public event Action<StreamStatistics>? StatisticsUpdated;

    private void OnPreviewFrameReady(Bitmap bitmap) => PreviewFrameReady?.Invoke(bitmap);
    private void OnStatisticsUpdated() => StatisticsUpdated?.Invoke(Statistics);

    public StreamStatistics Statistics { get; } = new();

    public bool IsStreaming { get; private set; }

    public ClientStreamService(
        CameraService? cameraService = null,
        FrameEncoder? frameEncoder = null,
        UdpFrameSender? udpSender = null)
    {
        _cameraService = cameraService ?? new CameraService();
        _frameEncoder = frameEncoder ?? new FrameEncoder();
        _udpSender = udpSender ?? new UdpFrameSender();
    }

    /// <summary>
    /// Bắt đầu phiên mở Webcam và truyền luồng JPEG UDP tới Server.
    /// </summary>
    public async Task StartStreamAsync(ClientSettings settings)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // TODO [TV2]:
        // 1. Kiểm tra định dạng IPAddress.TryParse(settings.ServerIp, ...) và cổng hợp lệ.
        // 2. Khởi tạo StreamId ngẫu nhiên, đặt lại FrameId = 0 và Statistics.Reset().
        // 3. Tạo _streamCts = new CancellationTokenSource(), lắng nghe _cameraService.FrameCaptured:
        //    - Chuyển Mat sang Bitmap (OpenCvSharp.Extensions.BitmapConverter.ToBitmap) cho PreviewFrameReady.
        //    - Nén JPEG qua _frameEncoder.EncodeJpeg(...) và gọi _udpSender.SendFrameAsync(...).
        await Task.CompletedTask;
        throw new NotImplementedException("TODO [TV2]: Triển khai luồng StartStreamAsync tích hợp Camera -> Encoder -> UDP.");
    }

    /// <summary>
    /// Dừng phiên truyền phát hiện tại một cách an toàn, không làm treo UI.
    /// </summary>
    public async Task StopStreamAsync()
    {
        if (_streamCts is not null)
        {
            await _streamCts.CancelAsync();
            _streamCts.Dispose();
            _streamCts = null;
        }

        await _cameraService.StopAsync();
        IsStreaming = false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _streamCts?.Cancel();
        _streamCts?.Dispose();
        _cameraService.Dispose();
        _udpSender.Dispose();
        _disposed = true;
    }
}
