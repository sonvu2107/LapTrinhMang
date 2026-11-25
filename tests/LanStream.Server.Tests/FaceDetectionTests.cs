using System.Drawing;
using LanStream.Server.Services;
using OpenCvSharp;

namespace LanStream.Server.Tests;

public sealed class FaceDetectionTests
{
    [Fact]
    public void ProcessFrame_DrawsBothFacesOnlyWhenEnabled()
    {
        // Ảnh NASA thuộc phạm vi công cộng, cắt từ https://scikit-image.org/docs/0.26.x/api/skimage.data.html#skimage.data.astronaut
        using var face = Cv2.ImRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "astronaut-face.jpg"));
        Assert.False(face.Empty());
        using var mirrored = new Mat();
        Cv2.Flip(face, mirrored, FlipMode.Y);
        using var twoFaces = new Mat();
        Cv2.HConcat(face, mirrored, twoFaces);
        byte[] jpeg = twoFaces.ToBytes(".jpg");

        using var service = new ServerStreamService();
        var greenPixels = new List<(int Left, int Right)>();
        service.FrameDecoded += (bitmap, _) =>
        {
            using (bitmap)
            {
                greenPixels.Add(CountBoxPixels(bitmap));
            }
        };

        Assert.True(service.ProcessFrame(jpeg));
        Assert.True(service.SetFaceDetectionEnabled(true));
        Assert.True(service.ProcessFrame(jpeg));
        Assert.True(service.SetFaceDetectionEnabled(false));
        Assert.True(service.ProcessFrame(jpeg));

        Assert.Equal((0, 0), greenPixels[0]);
        Assert.True(greenPixels[1].Left > 100);
        Assert.True(greenPixels[1].Right > 100);
        Assert.Equal((0, 0), greenPixels[2]);
    }

    [Fact]
    public void DrawFaceBoxes_FollowsTranslatedFaceWhenYuNetMissesBlurredFrame()
    {
        using var detector = new FaceDetectionService();
        Assert.True(detector.LoadModel());
        using var face = Cv2.ImRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "astronaut-face.jpg"));
        Assert.False(face.Empty());
        using var first = new Mat(480, 640, MatType.CV_8UC3, Scalar.Black);
        using var faceRegion = new Mat(first, new Rect(180, 140, face.Width, face.Height));
        face.CopyTo(faceRegion);
        Rect original = Assert.Single(detector.DetectFaces(first));

        using var translation = Mat.FromArray(new double[,] { { 1, 0, 22 }, { 0, 1, -12 } });
        using var moved = new Mat();
        Cv2.WarpAffine(first, moved, translation, first.Size());
        using var blurred = new Mat();
        Cv2.Blur(moved, blurred, new OpenCvSharp.Size(31, 1));
        Assert.Empty(detector.DetectFaces(blurred));

        Assert.Equal(1, detector.DrawFaceBoxes(first));
        Assert.Equal(1, detector.DrawFaceBoxes(blurred));
        using var outline = new Mat();
        Cv2.InRange(blurred, Scalar.LimeGreen, Scalar.LimeGreen, outline);
        Rect outlineBounds = Cv2.BoundingRect(outline);
        Assert.InRange(outlineBounds.X - original.X, 17, 27);
        Assert.InRange(outlineBounds.Y - original.Y, -17, -7);

        using var blank = new Mat(first.Size(), MatType.CV_8UC3, Scalar.Black);
        Assert.Equal(0, detector.DrawFaceBoxes(blank));
    }

    [Fact]
    public void DetectFaces_SkipsFramesSmallerThanTheModelWindow()
    {
        string model = Path.Combine(AppContext.BaseDirectory, "assets", "models", "haarcascade_frontalface_default.xml");
        using var detector = new FaceDetectionService();
        Assert.True(detector.LoadModel(model));
        using var frame = new Mat(1, 1, MatType.CV_8UC3, Scalar.Black);

        Assert.Empty(detector.DetectFaces(frame));
    }

    private static (int Left, int Right) CountBoxPixels(Bitmap bitmap)
    {
        int left = 0;
        int right = 0;
        int midpoint = bitmap.Width / 2;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (pixel.R == 50 && pixel.G == 205 && pixel.B == 50)
                {
                    if (x < midpoint)
                    {
                        left++;
                    }
                    else
                    {
                        right++;
                    }
                }
            }
        }

        return (left, right);
    }
}
