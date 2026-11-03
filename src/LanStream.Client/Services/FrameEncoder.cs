using System;
using LanStream.Shared.Protocol;
using OpenCvSharp;

namespace LanStream.Client.Services;

/// <summary>
/// Chịu trách nhiệm resize (nếu cần) và nén khung hình <see cref="Mat"/> sang mảng byte JPEG.
/// Phụ trách: TV2.
/// </summary>
public sealed class FrameEncoder
{
    /// <summary>
    /// Nén một khung hình OpenCV <see cref="Mat"/> sang định dạng JPEG với mức chất lượng chỉ định.
    /// </summary>
    /// <param name="frame">Khung hình gốc từ Webcam.</param>
    /// <param name="quality">Chất lượng JPEG (1..100, mặc định 60).</param>
    /// <returns>Mảng byte ảnh JPEG đã nén.</returns>
    public byte[] EncodeJpeg(Mat frame, int quality = ProtocolConstants.DefaultJpegQuality)
    {
        if (frame is null || frame.Empty())
        {
            throw new ArgumentException("Khung hình không hợp lệ hoặc rỗng.", nameof(frame));
        }

        int clampedQuality = Math.Clamp(quality, 1, 100);
        var encoderParams = new ImageEncodingParam(ImwriteFlags.JpegQuality, clampedQuality);

        Cv2.ImEncode(".jpg", frame, out byte[] jpegBytes, encoderParams);
        if (jpegBytes is null || jpegBytes.Length == 0)
        {
            throw new InvalidOperationException("Không thể nén khung hình sang định dạng JPEG.");
        }

        return jpegBytes;
    }
}
