using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using LanStream.Shared.Models;
using LanStream.Shared.Protocol;

namespace LanStream.Network;

/// <summary>
/// Gom các mảnh UDP nhận được theo khóa (RemoteEndPoint, StreamId, FrameId) thành khung hình JPEG hoàn chỉnh.
/// Xử lý gói đến sai thứ tự, bỏ gói trùng và loại bỏ các khung hình quá thời gian chờ (Timeout).
/// Phụ trách: TV3.
/// </summary>
public sealed class FrameAssembler
{
    private readonly TimeSpan _frameTimeout;
    private readonly Dictionary<FrameKey, FrameBuffer> _frames = new();
    private readonly object _sync = new();
    private long _invalidPackets;
    private long _droppedFrames;

    public long InvalidPackets => Interlocked.Read(ref _invalidPackets);
    public long DroppedFrames => Interlocked.Read(ref _droppedFrames);

    public FrameAssembler(int frameTimeoutMs = ProtocolConstants.DefaultFrameTimeoutMs)
    {
        if (frameTimeoutMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameTimeoutMs));
        }

        _frameTimeout = TimeSpan.FromMilliseconds(frameTimeoutMs);
    }

    /// <summary>
    /// Đưa một gói UDP nhận được vào bộ đệm lắp ghép.
    /// </summary>
    /// <param name="remoteEndPoint">Địa chỉ IP:Port của Client gửi gói tin.</param>
    /// <param name="datagram">Toàn bộ gói UDP thô nhận từ socket.</param>
    /// <param name="completedFrame">Khung hình hoàn chỉnh nếu gói vừa nhận là mảnh ghép cuối cùng; ngược lại <c>null</c>.</param>
    /// <returns><c>true</c> nếu vừa ghép xong trọn vẹn một khung hình; ngược lại <c>false</c>.</returns>
    public bool TryAddPacket(
        IPEndPoint remoteEndPoint,
        ReadOnlySpan<byte> datagram,
        out VideoFrame? completedFrame)
    {
        completedFrame = null;
        ArgumentNullException.ThrowIfNull(remoteEndPoint);

        if (!PacketSerializer.TryDeserializePacket(datagram, out PacketHeader header, out var payload))
        {
            Interlocked.Increment(ref _invalidPackets);
            return false;
        }

        var key = new FrameKey(remoteEndPoint, header.StreamId, header.FrameId);
        lock (_sync)
        {
            if (!_frames.TryGetValue(key, out FrameBuffer? frame))
            {
                frame = new FrameBuffer(header.ChunkCount, header.TimestampMs);
                _frames.Add(key, frame);
            }
            else if (Stopwatch.GetElapsedTime(frame.CreatedTimestamp) >= _frameTimeout)
            {
                _frames.Remove(key);
                Interlocked.Increment(ref _droppedFrames);
                return false;
            }
            else if (frame.Chunks.Length != header.ChunkCount ||
                     frame.TimestampMs != header.TimestampMs)
            {
                Interlocked.Increment(ref _invalidPackets);
                return false;
            }

            int chunkIndex = header.ChunkIndex;
            if (frame.Chunks[chunkIndex] is not null)
            {
                return false;
            }

            if (frame.TotalBytes + payload.Length > ProtocolConstants.MaxFrameSizeBytes)
            {
                _frames.Remove(key);
                Interlocked.Increment(ref _droppedFrames);
                return false;
            }

            frame.Chunks[chunkIndex] = payload.ToArray();
            frame.TotalBytes += payload.Length;
            frame.ReceivedChunks++;

            if (frame.ReceivedChunks != frame.Chunks.Length)
            {
                return false;
            }

            byte[] jpegBytes = new byte[frame.TotalBytes];
            int offset = 0;
            foreach (byte[]? chunk in frame.Chunks)
            {
                byte[] receivedChunk = chunk
                    ?? throw new InvalidOperationException("A completed frame is missing a chunk.");
                receivedChunk.CopyTo(jpegBytes, offset);
                offset += receivedChunk.Length;
            }

            _frames.Remove(key);
            completedFrame = new VideoFrame
            {
                StreamId = header.StreamId,
                FrameId = header.FrameId,
                TimestampMs = frame.TimestampMs,
                JpegBytes = jpegBytes,
                RemoteEndPoint = new IPEndPoint(remoteEndPoint.Address, remoteEndPoint.Port)
            };
            return true;
        }
    }

    /// <summary>
    /// Dọn dẹp các khung hình chưa nhận đủ mảnh sau khi vượt quá thời gian chờ (<c>200 ms</c>).
    /// </summary>
    /// <returns>Số lượng khung hình bị hủy bỏ do quá hạn.</returns>
    public int CleanupExpiredFrames()
    {
        lock (_sync)
        {
            long now = Stopwatch.GetTimestamp();
            List<FrameKey>? expiredKeys = null;
            foreach (var entry in _frames)
            {
                if (Stopwatch.GetElapsedTime(entry.Value.CreatedTimestamp, now) >= _frameTimeout)
                {
                    (expiredKeys ??= new List<FrameKey>()).Add(entry.Key);
                }
            }

            if (expiredKeys is null)
            {
                return 0;
            }

            foreach (FrameKey key in expiredKeys)
            {
                _frames.Remove(key);
            }

            Interlocked.Add(ref _droppedFrames, expiredKeys.Count);
            return expiredKeys.Count;
        }
    }

    private readonly record struct FrameKey(IPEndPoint RemoteEndPoint, uint StreamId, uint FrameId);

    private sealed class FrameBuffer(ushort chunkCount, long timestampMs)
    {
        public byte[]?[] Chunks { get; } = new byte[chunkCount][];
        public long TimestampMs { get; } = timestampMs;
        public long CreatedTimestamp { get; } = Stopwatch.GetTimestamp();
        public int ReceivedChunks { get; set; }
        public int TotalBytes { get; set; }
    }
}
