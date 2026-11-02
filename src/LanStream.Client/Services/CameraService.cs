using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;

namespace LanStream.Client.Services;

/// <summary>
/// Quản lý kết nối tới thiết bị Webcam qua OpenCvSharp (<see cref="VideoCapture"/>)
/// và phát luồng khung hình (<see cref="Mat"/>) trên tác vụ nền.
/// Phụ trách: TV2.
/// </summary>
public sealed class CameraService : IDisposable
{
    private VideoCapture? _capture;
    private bool _disposed;

    /// <summary>
    /// Sự kiện phát ra mỗi khi đọc được một khung hình mới từ Webcam.
    /// </summary>
    public event Action<Mat>? FrameCaptured;

    private void OnFrameCaptured(Mat frame) => FrameCaptured?.Invoke(frame);

    /// <summary>
    /// Cho biết camera hiện có đang mở và thu hình hay không.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Mở Webcam và chạy vòng lặp thu hình giới hạn theo FPS mục tiêu.
    /// </summary>
    public async Task StartAsync(
        int cameraIndex,
        int width,
        int height,
        int targetFps,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // TODO [TV2]:
        // 1. Khởi tạo _capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW) (hoặc ANY).
        // 2. Thiết lập FrameWidth / FrameHeight, kiểm tra _capture.IsOpened().
        // 3. Trong vòng lặp bất đồng bộ, đọc frame bằng _capture.Read(mat), resize về (width, height),
        //    gọi FrameCaptured?.Invoke(mat) và điều tiết tốc độ theo targetFps (Task.Delay).
        await Task.CompletedTask;
        throw new NotImplementedException("TODO [TV2]: Triển khai mở Webcam và vòng lặp thu hình.");
    }

    /// <summary>
    /// Dừng thu hình và giải phóng thiết bị Webcam an toàn.
    /// </summary>
    public Task StopAsync()
    {
        // TODO [TV2]: Dừng vòng lặp thu hình, giải phóng _capture?.Release() và đặt IsRunning = false.
        IsRunning = false;
        _capture?.Release();
        _capture?.Dispose();
        _capture = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _capture?.Release();
        _capture?.Dispose();
        _disposed = true;
    }
}
