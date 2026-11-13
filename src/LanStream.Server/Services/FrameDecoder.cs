using System;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace LanStream.Server.Services;

/// <summary>
/// Decodes incoming JPEG byte buffers into OpenCV <see cref="Mat"/> frames
/// and converts them into WinForms <see cref="Bitmap"/> instances.
/// </summary>
public sealed class FrameDecoder
{
    private const int MinJpegBytes = 4;

    public Mat? Decode(byte[]? jpegData)
    {
        if (jpegData is null || jpegData.Length < MinJpegBytes)
        {
            return null;
        }

        // Verify JPEG Start-Of-Image (SOI: 0xFF 0xD8) marker
        if (jpegData[0] != 0xFF || jpegData[1] != 0xD8)
        {
            return null;
        }

        Mat? mat = null;
        try
        {
            mat = Cv2.ImDecode(jpegData, ImreadModes.Color);
            if (mat.Empty() || mat.Width <= 0 || mat.Height <= 0)
            {
                mat.Dispose();
                return null;
            }

            return mat;
        }
        catch (OpenCVException)
        {
            mat?.Dispose();
            return null;
        }
        catch (Exception)
        {
            mat?.Dispose();
            return null;
        }
    }

    public Bitmap ConvertToBitmap(Mat frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        if (frame.Empty())
        {
            throw new ArgumentException("Cannot convert an empty Mat to Bitmap.", nameof(frame));
        }

        return BitmapConverter.ToBitmap(frame);
    }
}
