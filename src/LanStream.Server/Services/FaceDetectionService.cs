using System;
using OpenCvSharp;

namespace LanStream.Server.Services;

/// <summary>
/// Dịch vụ mở rộng: Phát hiện khuôn mặt trên khung hình bằng Haar Cascade hoặc YuNet (OpenCV)
/// và vẽ khung chữ nhật đánh dấu.
/// Phụ trách: TV1 (phối hợp TV2).
/// </summary>
public sealed class FaceDetectionService : IDisposable
{
    private CascadeClassifier? _cascadeClassifier = null;
    private bool _disposed;

    public bool IsModelLoaded => _cascadeClassifier is not null;

    /// <summary>
    /// Nạp tệp mô hình Haar Cascade XML từ thư mục <c>assets/models</c>.
    /// </summary>
    public bool LoadModel(string modelPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // TODO [TV1]: Kiểm tra File.Exists(modelPath), khởi tạo _cascadeClassifier = new CascadeClassifier(modelPath).
        throw new NotImplementedException("TODO [TV1]: Triển khai nạp model phát hiện khuôn mặt.");
    }

    /// <summary>
    /// Trả về danh sách các vùng chữ nhật chứa khuôn mặt trong khung hình.
    /// </summary>
    public Rect[] Detect(Mat frame)
    {
        // TODO [TV1]: Chuyển sang ảnh xám (Cv2.CvtColor), cân bằng sáng (Cv2.EqualizeHist)
        // và gọi _cascadeClassifier.DetectMultiScale(...).
        throw new NotImplementedException("TODO [TV1]: Triển khai phát hiện khuôn mặt trên Mat.");
    }

    /// <summary>
    /// Vẽ khung chữ nhật bao quanh các khuôn mặt phát hiện được trực tiếp lên <paramref name="frame"/>.
    /// </summary>
    public void DrawDetections(Mat frame, ReadOnlySpan<Rect> faces)
    {
        foreach (var rect in faces)
        {
            Cv2.Rectangle(frame, rect, Scalar.LimeGreen, thickness: 2);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _cascadeClassifier?.Dispose();
        _disposed = true;
    }
}
