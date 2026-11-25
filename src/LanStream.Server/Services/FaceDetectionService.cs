using System;
using System.IO;
using System.Net;
using System.Threading;
using LanStream.Server.Config;
using OpenCvSharp;
using Size = OpenCvSharp.Size;

namespace LanStream.Server.Services;

/// <summary>
/// Phát hiện khuôn mặt bằng OpenCV và theo dõi chuyển động giữa các khung hình.
/// </summary>
public sealed class FaceDetectionService : IDisposable
{
    private static readonly Size YuNetInputSize = new(320, 320);
    private const double DownscaleFactor = 0.5;
    private const int MinimumFaceSize = 24;
    private readonly Lock _syncLock = new();
    private CascadeClassifier? _cascadeClassifier;
    private FaceDetectorYN? _yuNet;
    private readonly FaceBoxTracker _tracker = new();
    private (uint StreamId, IPEndPoint? Sender)? _trackedStream;
    private string _configuredModelPath;
    private bool _disposed;

    public bool IsModelLoaded
    {
        get
        {
            lock (_syncLock)
            {
                return _yuNet is not null || _cascadeClassifier is not null;
            }
        }
    }

    public string? LoadedModelFullPath { get; private set; }

    public FaceDetectionService(string defaultModelPath = ServerSettings.DefaultFaceModelPath)
    {
        _configuredModelPath = defaultModelPath;
    }

    public bool LoadModel(string? modelPath = null)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!string.IsNullOrWhiteSpace(modelPath))
            {
                _configuredModelPath = modelPath;
            }

            try
            {
                string? resolvedPath = ResolveModelPath(_configuredModelPath);
                if (resolvedPath is null)
                {
                    return false;
                }

                if (resolvedPath == LoadedModelFullPath)
                {
                    return true;
                }

                if (string.Equals(Path.GetExtension(resolvedPath), ".onnx", StringComparison.OrdinalIgnoreCase))
                {
                    var detector = FaceDetectorYN.Create(resolvedPath, "", YuNetInputSize, scoreThreshold: 0.7f);
                    _yuNet?.Dispose();
                    _yuNet = detector;
                    _cascadeClassifier?.Dispose();
                    _cascadeClassifier = null;
                }
                else
                {
                    var classifier = new CascadeClassifier(resolvedPath);
                    if (classifier.Empty())
                    {
                        classifier.Dispose();
                        return false;
                    }

                    _cascadeClassifier?.Dispose();
                    _cascadeClassifier = classifier;
                    _yuNet?.Dispose();
                    _yuNet = null;
                }

                ResetTracking();
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
        if (frame is null || frame.Empty() || Math.Min(frame.Width, frame.Height) < MinimumFaceSize)
        {
            return [];
        }

        lock (_syncLock)
        {
            if (_yuNet is not null)
            {
                return DetectWithYuNet(frame, _yuNet);
            }

            if (_cascadeClassifier is null)
            {
                return [];
            }

            using var gray = new Mat();
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

            using var smallGray = new Mat();
            Mat detectionImage = gray;
            if (Math.Min(gray.Width, gray.Height) * DownscaleFactor >= MinimumFaceSize)
            {
                Cv2.Resize(gray, smallGray, new OpenCvSharp.Size(), DownscaleFactor, DownscaleFactor, InterpolationFlags.Linear);
                detectionImage = smallGray;
            }

            Cv2.EqualizeHist(detectionImage, detectionImage);
            Rect[] detectedFaces = _cascadeClassifier.DetectMultiScale(
                image: detectionImage,
                scaleFactor: 1.15,
                minNeighbors: 4,
                flags: HaarDetectionTypes.ScaleImage,
                minSize: new OpenCvSharp.Size(MinimumFaceSize, MinimumFaceSize));

            if (detectedFaces.Length == 0)
            {
                return [];
            }

            double scaleX = (double)frame.Width / detectionImage.Width;
            double scaleY = (double)frame.Height / detectionImage.Height;
            var scaledRects = new Rect[detectedFaces.Length];
            for (int i = 0; i < detectedFaces.Length; i++)
            {
                Rect r = detectedFaces[i];
                int left = (int)Math.Round(r.Left * scaleX);
                int top = (int)Math.Round(r.Top * scaleY);
                int right = (int)Math.Round(r.Right * scaleX);
                int bottom = (int)Math.Round(r.Bottom * scaleY);
                scaledRects[i] = new Rect(left, top, right - left, bottom - top);
            }

            return scaledRects;
        }
    }

    public int DrawFaceBoxes(Mat frame)
        => DrawTrackedFaceBoxes(frame, 0, null);

    internal int DrawTrackedFaceBoxes(Mat frame, uint streamId, IPEndPoint? sender)
    {
        if (frame is null || frame.Empty())
        {
            return 0;
        }

        lock (_syncLock)
        {
            if (_trackedStream is not { } previous || previous.StreamId != streamId || !Equals(previous.Sender, sender))
            {
                _tracker.Reset();
                _trackedStream = (streamId, sender);
            }

            Rect[] faces = _tracker.Update(frame, DetectFaces(frame));
            DrawFaceBoxes(frame, faces);
            return faces.Length;
        }
    }

    internal void ResetTracking()
    {
        lock (_syncLock)
        {
            _tracker.Reset();
            _trackedStream = null;
        }
    }

    private static Rect[] DetectWithYuNet(Mat frame, FaceDetectorYN detector)
    {
        double ratio = (double)YuNetInputSize.Width / Math.Max(frame.Width, frame.Height);
        int width = Math.Max(1, (int)Math.Round(frame.Width * ratio));
        int height = Math.Max(1, (int)Math.Round(frame.Height * ratio));
        using var input = new Mat(YuNetInputSize, MatType.CV_8UC3, Scalar.Black);
        using var resized = new Mat(input, new Rect(0, 0, width, height));
        Cv2.Resize(frame, resized, new Size(width, height), interpolation: InterpolationFlags.Linear);

        using var predictions = new Mat();
        detector.Detect(input, predictions);
        int count = predictions.Rows;
        var faces = new List<Rect>(count);
        var imageBounds = new Rect(0, 0, frame.Width, frame.Height);
        double scaleX = (double)frame.Width / width;
        double scaleY = (double)frame.Height / height;
        for (int row = 0; row < count; row++)
        {
            float x = predictions.At<float>(row, 0);
            float y = predictions.At<float>(row, 1);
            int left = (int)Math.Round(x * scaleX);
            int top = (int)Math.Round(y * scaleY);
            int right = (int)Math.Round((x + predictions.At<float>(row, 2)) * scaleX);
            int bottom = (int)Math.Round((y + predictions.At<float>(row, 3)) * scaleY);
            Rect face = new Rect(left, top, right - left, bottom - top) & imageBounds;
            if (face.Width > 0 && face.Height > 0)
            {
                faces.Add(face);
            }
        }

        return faces.ToArray();
    }

    public void DrawFaceBoxes(Mat frame, ReadOnlySpan<Rect> faces)
    {
        if (frame is null || frame.Empty() || faces.IsEmpty)
        {
            return;
        }

        foreach (Rect face in faces)
        {
            Cv2.Rectangle(frame, face, Scalar.LimeGreen, thickness: 2);
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
            _yuNet?.Dispose();
            _yuNet = null;
            _tracker.Dispose();
            _disposed = true;
        }
    }
}
