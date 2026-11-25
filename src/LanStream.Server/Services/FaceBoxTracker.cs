using System.Diagnostics;
using OpenCvSharp;

namespace LanStream.Server.Services;

internal sealed class FaceBoxTracker : IDisposable
{
    private static readonly TimeSpan MaxDetectionGap = TimeSpan.FromMilliseconds(250);
    private Mat? _previousGray;
    private List<Track> _previousFaces = [];

    public Rect[] Update(Mat frame, Rect[] detections)
    {
        if (_previousGray is not null && _previousGray.Size() != frame.Size())
        {
            Reset();
        }

        if (detections.Length == 0 && _previousFaces.Count == 0)
        {
            return [];
        }

        long now = Stopwatch.GetTimestamp();
        var nextFaces = new List<Track>(detections.Length + _previousFaces.Count);
        foreach (Rect face in detections)
        {
            nextFaces.Add(new Track(face, now));
        }

        using var gray = new Mat();
        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
        if (_previousGray is not null)
        {
            foreach (Track previous in _previousFaces)
            {
                if (Stopwatch.GetElapsedTime(previous.DetectedAt, now) > MaxDetectionGap ||
                    OverlapsExisting(previous.Bounds, nextFaces) ||
                    FollowMotion(_previousGray, gray, previous.Bounds) is not { } moved ||
                    OverlapsExisting(moved, nextFaces))
                {
                    continue;
                }

                nextFaces.Add(new Track(moved, previous.DetectedAt));
            }
        }

        _previousGray ??= new Mat();
        gray.CopyTo(_previousGray);
        _previousFaces = nextFaces;
        return nextFaces.Select(face => face.Bounds).ToArray();
    }

    private static Rect? FollowMotion(Mat previous, Mat current, Rect bounds)
    {
        using var faceRegion = new Mat(previous, bounds);
        using var mask = new Mat();
        Point2f[] points = faceRegion.GoodFeaturesToTrack(24, 0.02, 4, mask, 3, false, 0.04);
        if (points.Length < 5)
        {
            return null;
        }

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Point2f(points[i].X + bounds.X, points[i].Y + bounds.Y);
        }

        Point2f[] moved = [];
        Cv2.CalcOpticalFlowPyrLK(previous, current, points, ref moved, out byte[] found, out float[] errors);
        var dx = new List<float>(points.Length);
        var dy = new List<float>(points.Length);
        for (int i = 0; i < points.Length; i++)
        {
            if (found[i] != 0 && errors[i] <= 24)
            {
                dx.Add(moved[i].X - points[i].X);
                dy.Add(moved[i].Y - points[i].Y);
            }
        }

        if (dx.Count < Math.Max(5, (points.Length + 1) / 2))
        {
            return null;
        }

        dx.Sort();
        dy.Sort();
        int center = dx.Count / 2;
        int x = bounds.X + (int)Math.Round(dx[center]);
        int y = bounds.Y + (int)Math.Round(dy[center]);
        Rect clipped = new Rect(x, y, bounds.Width, bounds.Height) & new Rect(0, 0, current.Width, current.Height);
        return clipped.Width >= bounds.Width / 2 && clipped.Height >= bounds.Height / 2 ? clipped : null;
    }

    private static bool OverlapsExisting(Rect candidate, List<Track> faces)
    {
        foreach (Track face in faces)
        {
            Rect overlap = candidate & face.Bounds;
            int smallerArea = Math.Min(candidate.Width * candidate.Height, face.Bounds.Width * face.Bounds.Height);
            if (overlap.Width * overlap.Height >= 0.5 * smallerArea)
            {
                return true;
            }
        }

        return false;
    }

    public void Reset()
    {
        _previousGray?.Dispose();
        _previousGray = null;
        _previousFaces.Clear();
    }

    public void Dispose() => Reset();

    private readonly record struct Track(Rect Bounds, long DetectedAt);
}
