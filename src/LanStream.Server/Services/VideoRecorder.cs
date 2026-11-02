using System;
using OpenCvSharp;

namespace LanStream.Server.Services;

/// <summary>
/// Ghi lại luồng khung hình nhận được ra tệp video (.mp4 hoặc dự phòng .avi/MJPG) bằng <see cref="VideoWriter"/>.
/// Phụ trách: TV1.
/// </summary>
public sealed class VideoRecorder : IDisposable
{
    private VideoWriter? _writer;
    private bool _disposed;

    public bool IsRecording { get; private set; }

    public string? CurrentOutputPath { get; private set; }

    /// <summary>
    /// Mở tệp đích và khởi tạo <see cref="VideoWriter"/>.
    /// </summary>
    /// <param name="outputPath">Đường dẫn tệp video đầu ra (ví dụ: <c>recordings/2026-11-02_0830.mp4</c>).</param>
    /// <param name="fps">Tốc độ khung hình của tệp video.</param>
    /// <param name="frameSize">Kích thước khung hình (ví dụ: 640×480).</param>
    public void Start(string outputPath, double fps, OpenCvSharp.Size frameSize)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // TODO [TV1]:
        // 1. Tạo thư mục cha nếu chưa tồn tại (Directory.CreateDirectory).
        // 2. Khởi tạo VideoWriter với FourCC.MP4V (hoặc FourCC.MJPG cho .avi nếu máy thiếu codec).
        // 3. Đặt CurrentOutputPath = outputPath và IsRecording = true.
        throw new NotImplementedException("TODO [TV1]: Triển khai khởi tạo VideoWriter.");
    }

    /// <summary>
    /// Ghi một khung hình vào tệp video đang mở.
    /// </summary>
    public void Write(Mat frame)
    {
        // TODO [TV1]: Kiểm tra IsRecording && _writer is not null, sau đó gọi _writer.Write(frame).
        throw new NotImplementedException("TODO [TV1]: Triển khai ghi frame vào VideoWriter.");
    }

    /// <summary>
    /// Đóng tệp video và giải phóng <see cref="VideoWriter"/>.
    /// </summary>
    public void Stop()
    {
        IsRecording = false;
        _writer?.Release();
        _writer?.Dispose();
        _writer = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _disposed = true;
    }
}
