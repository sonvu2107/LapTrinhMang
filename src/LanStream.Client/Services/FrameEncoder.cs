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
        // TODO [TV2]:
        // Sử dụng Cv2.ImEncode(".jpg", frame, out byte[] jpegBytes, new ImageEncodingParam(ImwriteFlags.JpegQuality, quality))
        // và trả về mảng jpegBytes.
        throw new NotImplementedException("TODO [TV2]: Triển khai nén Mat sang mảng byte JPEG.");
    }
}
