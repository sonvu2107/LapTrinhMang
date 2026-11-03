using System;
using System.Diagnostics;
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
    private Task? _captureTask;
    private CancellationTokenSource? _captureCts;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Sự kiện phát ra mỗi khi đọc được một khung hình mới từ Webcam.
    /// </summary>
    public event Action<Mat>? FrameCaptured;

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

        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (targetFps <= 0) throw new ArgumentOutOfRangeException(nameof(targetFps));

        if (IsRunning)
        {
            await StopAsync();
        }

        lock (_lock)
        {
            // Ưu tiên dùng DirectShow trên Windows để khởi động nhanh, fallback sang ANY nếu cần
            _capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.ANY);
            }

            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = null;
                throw new InvalidOperationException($"Không thể mở Webcam (Camera #{cameraIndex}). Vui lòng kiểm tra quyền truy cập camera hoặc thiết bị.");
            }

            _capture.Set(VideoCaptureProperties.FrameWidth, width);
            _capture.Set(VideoCaptureProperties.FrameHeight, height);
            _capture.Set(VideoCaptureProperties.Fps, targetFps);

            IsRunning = true;
            _captureCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        _captureTask = Task.Run(() => CaptureLoopAsync(width, height, targetFps, _captureCts.Token), _captureCts.Token);
    }

    private async Task CaptureLoopAsync(int width, int height, int targetFps, CancellationToken ct)
    {
        using var rawMat = new Mat();
        using var resizedMat = new Mat();
        var sw = Stopwatch.StartNew();
        int intervalMs = Math.Max(1, 1000 / targetFps);

        while (!ct.IsCancellationRequested && IsRunning)
        {
            sw.Restart();
            bool hasFrame = false;

            lock (_lock)
            {
                if (_capture is not null && _capture.IsOpened())
                {
                    hasFrame = _capture.Read(rawMat) && !rawMat.Empty();
                }
            }

            if (hasFrame)
            {
                if (rawMat.Width != width || rawMat.Height != height)
                {
                    Cv2.Resize(rawMat, resizedMat, new OpenCvSharp.Size(width, height));
                    FrameCaptured?.Invoke(resizedMat);
                }
                else
                {
                    FrameCaptured?.Invoke(rawMat);
                }
            }
            else
            {
                try
                {
                    await Task.Delay(15, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                continue;
            }

            long elapsed = sw.ElapsedMilliseconds;
            int sleepMs = (int)(intervalMs - elapsed);
            if (sleepMs > 0)
            {
                try
                {
                    await Task.Delay(sleepMs, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Dừng thu hình và giải phóng thiết bị Webcam an toàn.
    /// </summary>
    public async Task StopAsync()
    {
        if (!IsRunning && _capture is null)
        {
            return;
        }

        IsRunning = false;

        if (_captureCts is not null)
        {
            await _captureCts.CancelAsync();
            _captureCts.Dispose();
            _captureCts = null;
        }

        if (_captureTask is not null)
        {
            try
            {
                await _captureTask;
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
            _captureTask = null;
        }

        lock (_lock)
        {
            _capture?.Release();
            _capture?.Dispose();
            _capture = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _captureCts?.Cancel();
        _captureCts?.Dispose();
        _captureCts = null;

        lock (_lock)
        {
            _capture?.Release();
            _capture?.Dispose();
            _capture = null;
        }

        _disposed = true;
    }
}
