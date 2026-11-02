using System.Net;
using LanStream.Network;
using Xunit;

namespace LanStream.Tests;

/// <summary>
/// Kiểm thử đơn vị cho <see cref="FramePacketizer"/> và <see cref="FrameAssembler"/>
/// (bao gồm TC07 gói sai thứ tự, TC08 thiếu chunk quá hạn, gói trùng lặp).
/// Phụ trách: TV3 + TV4.
/// </summary>
public class FrameAssemblerTests
{
    [Fact(Skip = "TODO [TV3]: Bỏ Skip sau khi hoàn thiện FramePacketizer và FrameAssembler (TC07)")]
    public void TryAddPacket_OutOfOrderChunks_ReassemblesOriginalJpeg()
    {
        var packetizer = new FramePacketizer();
        var assembler = new FrameAssembler();
        var endpoint = new IPEndPoint(IPAddress.Loopback, 54321);

        byte[] fakeJpeg = new byte[3000];
        for (int i = 0; i < fakeJpeg.Length; i++)
        {
            fakeJpeg[i] = (byte)(i % 251);
        }

        var packets = packetizer.Packetize(fakeJpeg, streamId: 1, frameId: 10);
        Assert.Equal(3, packets.Count);

        // Giả lập gói UDP đến sai thứ tự: 2 -> 0 -> 1
        Assert.False(assembler.TryAddPacket(endpoint, packets[2], out _));
        Assert.False(assembler.TryAddPacket(endpoint, packets[0], out _));
        Assert.True(assembler.TryAddPacket(endpoint, packets[1], out var frame));

        Assert.NotNull(frame);
        Assert.Equal(fakeJpeg, frame!.JpegBytes);
    }

    [Fact(Skip = "TODO [TV3]: Bỏ Skip sau khi hoàn thiện CleanupExpiredFrames (TC08)")]
    public async System.Threading.Tasks.Task CleanupExpiredFrames_MissingChunkAfterTimeout_DropsIncompleteFrame()
    {
        var packetizer = new FramePacketizer();
        var assembler = new FrameAssembler(frameTimeoutMs: 50);
        var endpoint = new IPEndPoint(IPAddress.Loopback, 54321);

        byte[] fakeJpeg = new byte[2500];
        var packets = packetizer.Packetize(fakeJpeg, streamId: 1, frameId: 11);

        // Chỉ gửi mảnh đầu tiên, cố tình làm mất mảnh còn lại
        Assert.False(assembler.TryAddPacket(endpoint, packets[0], out _));

        await System.Threading.Tasks.Task.Delay(80);
        int dropped = assembler.CleanupExpiredFrames();

        Assert.Equal(1, dropped);
    }
}
