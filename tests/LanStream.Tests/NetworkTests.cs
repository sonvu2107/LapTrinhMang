using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LanStream.Network;
using Xunit;

namespace LanStream.Tests;

/// <summary>
/// Kiểm thử tích hợp gửi & nhận qua UDP Loopback (127.0.0.1) giữa <see cref="UdpFrameSender"/> và <see cref="UdpFrameReceiver"/>.
/// Phụ trách: TV3 + TV4.
/// </summary>
public class NetworkTests
{
    [Fact(Skip = "TODO [TV3]: Bỏ Skip sau khi hoàn thiện UdpFrameSender và UdpFrameReceiver (TC03)")]
    public async Task UdpLoopback_SendAndReceiveSingleFrame_Succeeds()
    {
        const int testPort = 15005;
        using var cts = new CancellationTokenSource(millisecondsDelay: 3000);
        using var receiver = new UdpFrameReceiver(testPort);
        using var sender = new UdpFrameSender();

        var tcs = new TaskCompletionSource<byte[]>();
        receiver.FrameReceived += frame => tcs.TrySetResult(frame.JpegBytes);

        Task receiveTask = receiver.ReceiveLoopAsync(cts.Token);

        byte[] sampleJpeg = new byte[4096];
        await sender.SendFrameAsync(
            sampleJpeg,
            streamId: 99,
            frameId: 1,
            new IPEndPoint(IPAddress.Loopback, testPort),
            cts.Token);

        byte[] receivedBytes = await tcs.Task.WaitAsync(cts.Token);
        Assert.Equal(sampleJpeg, receivedBytes);

        await cts.CancelAsync();
        await receiveTask;
    }
}
