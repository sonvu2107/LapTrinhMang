using System;
using System.IO;
using System.Threading;
using OpenCvSharp;

namespace LanStream.Server.Services;

/// <summary>
/// Records decoded video frames to an MP4 file (with automatic fallback to AVI/MJPG if MP4 codec is unavailable).
/// </summary>
public sealed class VideoRecorder : IDisposable
{
    private readonly Lock _syncLock = new();
    private VideoWriter? _writer;
    private OpenCvSharp.Size _targetFrameSize;
    private bool _disposed;

    public bool IsRecording { get; private set; }

    public string? CurrentOutputPath { get; private set; }

    public long RecordedFramesCount { get; private set; }

    public string StartRecording(string filePath, int fps, OpenCvSharp.Size frameSize)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (fps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fps), "Recording FPS must be greater than zero.");
        }

        if (frameSize.Width <= 0 || frameSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameSize), "Frame size must be positive.");
        }

        lock (_syncLock)
        {
            StopRecordingInternal();

            string fullPath = Path.GetFullPath(filePath);
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string mp4Path = ReserveOutputPath(fullPath, ".mp4");
            int mp4vFourCc = VideoWriter.FourCC('m', 'p', '4', 'v');
            var writer = new VideoWriter(mp4Path, mp4vFourCc, fps, frameSize, isColor: true);
            if (writer.IsOpened())
            {
                _writer = writer;
                _targetFrameSize = frameSize;
                CurrentOutputPath = mp4Path;
                RecordedFramesCount = 0;
                IsRecording = true;
                return mp4Path;
            }

            writer.Dispose();

            string aviPath = ReserveOutputPath(fullPath, ".avi");
            writer = new VideoWriter(aviPath, FourCC.MJPG, fps, frameSize, isColor: true);
            if (writer.IsOpened())
            {
                _writer = writer;
                _targetFrameSize = frameSize;
                CurrentOutputPath = aviPath;
                RecordedFramesCount = 0;
                IsRecording = true;
                return aviPath;
            }

            writer.Dispose();
            throw new InvalidOperationException("Unable to initialize video encoder (both MP4V and MJPG failed).");
        }
    }

    private static string ReserveOutputPath(string requestedPath, string extension)
    {
        string candidate = Path.ChangeExtension(requestedPath, extension);
        while (true)
        {
            try
            {
                // Giữ tên tệp riêng cho phiên này, kể cả khi nhiều phiên bắt đầu cùng lúc.
                using var reservation = new FileStream(candidate, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                return candidate;
            }
            catch (IOException) when (File.Exists(candidate))
            {
                candidate = Path.Combine(
                    Path.GetDirectoryName(requestedPath)!,
                    $"{Path.GetFileNameWithoutExtension(requestedPath)}_{Guid.NewGuid():N}{extension}");
            }
        }
    }

    public void WriteFrame(Mat frame)
    {
        if (_disposed || frame is null || frame.Empty())
        {
            return;
        }

        lock (_syncLock)
        {
            if (!IsRecording || _writer is null || !_writer.IsOpened())
            {
                return;
            }

            if (frame.Width == _targetFrameSize.Width && frame.Height == _targetFrameSize.Height)
            {
                _writer.Write(frame);
            }
            else
            {
                using var resized = new Mat();
                Cv2.Resize(frame, resized, _targetFrameSize);
                _writer.Write(resized);
            }

            RecordedFramesCount++;
        }
    }

    public void StopRecording()
    {
        lock (_syncLock)
        {
            StopRecordingInternal();
        }
    }

    private void StopRecordingInternal()
    {
        IsRecording = false;
        if (_writer is not null)
        {
            try
            {
                _writer.Release();
            }
            finally
            {
                _writer.Dispose();
                _writer = null;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_syncLock)
        {
            StopRecordingInternal();
            _disposed = true;
        }
    }
}
