using System;
using System.IO;
using System.Threading;
using OpenCvSharp;

namespace LanStream.Server.Services;

/// <summary>
/// Detects faces in video frames using OpenCV Haar Cascade and draws bounding boxes.
/// </summary>
public sealed class FaceDetectionService : IDisposable
{
    private const double DownscaleFactor = 0.5;
    private readonly Lock _syncLock = new();
    private CascadeClassifier? _cascadeClassifier;
    private string _configuredModelPath;
    private bool _disposed;

    public bool IsModelLoaded
    {
        get
        {
            lock (_syncLock)
            {
                return _cascadeClassifier is not null;
            }
        }
    }

    public string? LoadedModelFullPath { get; private set; }

    public FaceDetectionService(string defaultModelPath = "assets/models/haarcascade_frontalface_default.xml")
    {
        _configuredModelPath = defaultModelPath;
    }

    public bool LoadModel(string? modelPath = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!string.IsNullOrWhiteSpace(modelPath))
        {
            _configuredModelPath = modelPath;
        }

        string? resolvedPath = ResolveModelPath(_configuredModelPath);
        if (resolvedPath is null)
        {
            return false;
        }

        lock (_syncLock)
        {
            try
            {
                var classifier = new CascadeClassifier(resolvedPath);
                if (classifier.Empty())
                {
                    classifier.Dispose();
                    return false;
                }

                _cascadeClassifier?.Dispose();
                _cascadeClassifier = classifier;
                LoadedModelFullPath = resolvedPath;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public Rect[] DetectFaces(Mat frame)
    {
        if (_disposed || frame is null || frame.Empty())
        {
            return [];
        }

        lock (_syncLock)
        {
            if (_cascadeClassifier is null)
            {
                return [];
            }

            using var gray = new Mat();
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

            using var smallGray = new Mat();
            Cv2.Resize(gray, smallGray, new OpenCvSharp.Size(), DownscaleFactor, DownscaleFactor, InterpolationFlags.Linear);
            Cv2.EqualizeHist(smallGray, smallGray);

            Rect[] detectedSmall = _cascadeClassifier.DetectMultiScale(
                image: smallGray,
                scaleFactor: 1.15,
                minNeighbors: 4,
                flags: HaarDetectionTypes.ScaleImage,
                minSize: new OpenCvSharp.Size(24, 24));

            if (detectedSmall.Length == 0)
            {
                return [];
            }

            double invScale = 1.0 / DownscaleFactor;
            var scaledRects = new Rect[detectedSmall.Length];
            for (int i = 0; i < detectedSmall.Length; i++)
            {
                Rect r = detectedSmall[i];
                scaledRects[i] = new Rect(
                    (int)Math.Round(r.X * invScale),
                    (int)Math.Round(r.Y * invScale),
                    (int)Math.Round(r.Width * invScale),
                    (int)Math.Round(r.Height * invScale));
            }

            return scaledRects;
        }
    }

    public int DrawFaceBoxes(Mat frame)
    {
        Rect[] faces = DetectFaces(frame);
        DrawFaceBoxes(frame, faces);
        return faces.Length;
    }

    public void DrawFaceBoxes(Mat frame, ReadOnlySpan<Rect> faces)
    {
        if (frame is null || frame.Empty() || faces.IsEmpty)
        {
            return;
        }

        for (int i = 0; i < faces.Length; i++)
        {
            Rect rect = faces[i];
            Cv2.Rectangle(frame, rect, Scalar.LimeGreen, thickness: 2);

            int labelY = Math.Max(rect.Y - 8, 16);
            Cv2.PutText(
                frame,
                $"Face #{i + 1}",
                new OpenCvSharp.Point(rect.X, labelY),
                HersheyFonts.HersheySimplex,
                0.55,
                Scalar.LimeGreen,
                thickness: 2);
        }
    }

    private static string? ResolveModelPath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return File.Exists(configuredPath) ? configuredPath : null;
        }

        string fromCurrentDir = Path.GetFullPath(configuredPath);
        if (File.Exists(fromCurrentDir))
        {
            return fromCurrentDir;
        }

        DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int depth = 0; depth < 6 && dir is not null; depth++, dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, configuredPath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_syncLock)
        {
            _cascadeClassifier?.Dispose();
            _cascadeClassifier = null;
            _disposed = true;
        }
    }
}
