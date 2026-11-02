using System;
using System.Drawing;
using OpenCvSharp;

namespace LanStream.Server.Services;

/// <summary>
/// Giải mã mảng byte ảnh JPEG nhận được thành đối tượng <see cref="Mat"/> (OpenCV)
/// và chuyển đổi sang <see cref="Bitmap"/> phục vụ hiển thị trên WinForms.
/// Phụ trách: TV1.
/// </summary>
public sealed class FrameDecoder
{
    /// <summary>
    /// Giải mã dữ liệu JPEG sang <see cref="Mat"/> của OpenCV để xử lý tiếp (ghi hình, phát hiện khuôn mặt).
    /// </summary>
    /// <param name="jpegBytes">Dữ liệu ảnh JPEG đã ghép đủ gói.</param>
    /// <returns><see cref="Mat"/> màu BGR, hoặc <c>null</c> nếu dữ liệu JPEG bị lỗi.</returns>
    public Mat? DecodeToMat(byte[] jpegBytes)
    {
        // TODO [TV1]:
        // Gọi Cv2.ImDecode(jpegBytes, ImreadModes.Color), kiểm tra !mat.Empty() trước khi trả về.
        throw new NotImplementedException("TODO [TV1]: Triển khai giải mã JPEG sang OpenCV Mat.");
    }

    /// <summary>
    /// Chuyển đổi <see cref="Mat"/> sang <see cref="Bitmap"/> để cập nhật lên <c>PictureBox</c>.
    /// </summary>
    public Bitmap ConvertToBitmap(Mat frame)
    {
        // TODO [TV1]:
        // Sử dụng OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame).
        throw new NotImplementedException("TODO [TV1]: Triển khai chuyển đổi Mat sang Bitmap.");
    }
}
