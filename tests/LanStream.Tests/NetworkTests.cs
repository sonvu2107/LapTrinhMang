using System.Net;
using System.Net.Sockets;
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
    [Fact]
    public async Task ReceiveLoop_CancellationReleasesPortAndAllowsRestart()
    {
        int port = GetAvailablePort();
        using var receiver = new UdpFrameReceiver(port);
        for (int attempt = 0; attempt < 2; attempt++)
        {
            using var cts = new CancellationTokenSource();
            Task receiveTask = receiver.ReceiveLoopAsync(cts.Token);
            Assert.False(receiveTask.IsCompleted);
            await cts.CancelAsync();
            await receiveTask.WaitAsync(TimeSpan.FromSeconds(3));

            using var probe = new UdpClient(new IPEndPoint(IPAddress.Any, port));
        }
    }

    private static int GetAvailablePort()
    {
        using var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
    }

    [Fact]
    public async Task UdpLoopback_SendAndReceiveSingleFrame_Succeeds()
    {
        int testPort = GetAvailablePort();
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
