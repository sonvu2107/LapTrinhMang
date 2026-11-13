using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using LanStream.Network;
using LanStream.Server.Config;
using LanStream.Server.Forms;
using LanStream.Server.Services;
using LanStream.Shared.Models;
using OpenCvSharp;

namespace LanStream.Server.Tests;

public sealed class ServerRegressionTests
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [Fact]
    public async Task UdpPipeline_DecodesJpegAndReportsInvalidPacketsAndExpiredFrames()
    {
        int port;
        using (var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0)))
            port = ((IPEndPoint)probe.Client.LocalEndPoint!).Port;

        using var service = new ServerStreamService();
        using var sender = new UdpFrameSender();
        using var rawSender = new UdpClient();
        using var image = new Mat(48, 64, MatType.CV_8UC3, Scalar.Red);
        byte[] jpeg = image.ToBytes(".jpg");
        var endpoint = new IPEndPoint(IPAddress.Loopback, port);
        var decoded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var statisticsReady = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        service.FrameDecoded += (bitmap, frame) =>
        {
            using (bitmap)
            {
                if (bitmap.Width == 64 && bitmap.Height == 48 && frame.StreamId == 99)
                    decoded.TrySetResult();
            }
        };
        service.StatisticsUpdated += statistics =>
        {
            if (statistics.InvalidPackets == 1 && statistics.DroppedFrames == 1)
                statisticsReady.TrySetResult();
        };

        await service.StartAsync(new ServerSettings { ListenPort = port, FrameTimeoutMs = 50 });
        try
        {
            // Gửi lại trong khi tác vụ nền mở socket, không phụ thuộc thời gian khởi động cố định.
            for (uint id = 0; id < 50 && !decoded.Task.IsCompleted; id++)
            {
                await sender.SendFrameAsync(jpeg, 99, id, endpoint);
                await Task.WhenAny(decoded.Task, Task.Delay(20));
            }
            await decoded.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await rawSender.SendAsync(new byte[] { 1, 2, 3 }, endpoint);
            var incomplete = new FramePacketizer().Packetize(new byte[1500], 100, 1);
            await rawSender.SendAsync(incomplete[0], endpoint);
            await statisticsReady.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            await service.StopAsync();
        }

        Assert.True(service.Statistics.CompletedFrames >= 1);
        Assert.True(service.Statistics.TotalBytesTransferred >= jpeg.Length + 24 + 3 + 1200);
        Assert.Equal(1, service.Statistics.InvalidPackets);
        Assert.Equal(1, service.Statistics.DroppedFrames);
    }

    [Fact]
    public async Task DecodeQueue_OverloadKeepsLatestFramesAndCountsEveryDrop()
    {
        using var service = new ServerStreamService();
        PreparePipeline(service);
        for (uint id = 1; id <= 5; id++)
        {
            service.OnFrameReceived(new VideoFrame { FrameId = id, JpegBytes = [1] });
        }

        var channel = GetField<Channel<VideoFrame>>(service, "_decodeChannel");
        Assert.True(channel.Reader.TryRead(out var first));
        Assert.True(channel.Reader.TryRead(out var second));
        Assert.Equal(4u, first.FrameId);
        Assert.Equal(5u, second.FrameId);
        Assert.False(channel.Reader.TryRead(out _));

        await service.StopAsync();
        Assert.Equal(3, service.Statistics.DroppedFrames);
    }

    [Fact]
    public async Task StopAsync_ConcurrentCallersWaitForTheSameShutdown()
    {
        using var service = new ServerStreamService();
        PreparePipeline(service);
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SetField(service, "_decodeTask", pending.Task);
        Task first = service.StopAsync();
        Task second = service.StopAsync();
        Assert.Same(first, second);
        Assert.False(first.IsCompleted);
        pending.SetResult();
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(service.IsRunning);
    }

    [Fact]
    public void FormClose_WaitsForShutdownBeforeDisposingAndRaisesClosingOnce()
    {
        RunSta(() =>
        {
            using var form = new ServerForm { ShowInTaskbar = false, Opacity = 0 };
            var service = GetField<ServerStreamService>(form, "_serverService");
            PreparePipeline(service);
            var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            SetField(service, "_decodeTask", pending.Task);
            int closingEvents = 0;
            form.FormClosing += (_, _) => closingEvents++;
            form.Show();
            try
            {
                form.Close();
                Assert.False(form.IsDisposed);
                Assert.False(form.Enabled);
                Assert.False(GetField<bool>(service, "_disposed"));
                form.Close();
                Assert.False(form.IsDisposed);
            }
            finally
            {
                pending.TrySetResult();
                PumpUntil(() => form.IsDisposed);
            }

            Assert.True(GetField<bool>(service, "_disposed"));
            Assert.Equal(1, closingEvents);
        });
    }

    [Fact]
    public void FormClose_CancelledBySubscriberKeepsServiceUsable()
    {
        RunSta(() =>
        {
            using var form = new ServerForm { ShowInTaskbar = false, Opacity = 0 };
            var service = GetField<ServerStreamService>(form, "_serverService");
            FormClosingEventHandler cancel = (_, e) => e.Cancel = true;
            form.FormClosing += cancel;
            form.Show();
            try
            {
                form.Close();
                Assert.False(form.IsDisposed);
                Assert.True(form.Enabled);
                Assert.False(GetField<bool>(service, "_disposed"));
            }
            finally
            {
                form.FormClosing -= cancel;
                form.Close();
                PumpUntil(() => form.IsDisposed);
            }
        });
    }

    [Fact]
    public void Recording_ExistingMp4AndAviArePreservedAndNewFileIsPlayable()
    {
        string directory = Path.Combine(Path.GetTempPath(), "LanStream.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string requested = Path.Combine(directory, "existing.mp4");
        string fallback = Path.ChangeExtension(requested, ".avi");
        byte[] original = [10, 20, 30, 40];
        File.WriteAllBytes(requested, original);
        File.WriteAllBytes(fallback, original);
        try
        {
            using var recorder = new VideoRecorder();
            string actual = recorder.StartRecording(requested, 10, new OpenCvSharp.Size(64, 48));
            using var frame = new Mat(48, 64, MatType.CV_8UC3, Scalar.Red);
            for (int i = 0; i < 5; i++) recorder.WriteFrame(frame);
            recorder.StopRecording();

            Assert.NotEqual(requested, actual);
            Assert.NotEqual(fallback, actual);
            Assert.Equal(original, File.ReadAllBytes(requested));
            Assert.Equal(original, File.ReadAllBytes(fallback));
            using var capture = new VideoCapture(actual);
            using var decoded = new Mat();
            Assert.True(capture.IsOpened());
            Assert.True(capture.Read(decoded));
            Assert.Equal(64, decoded.Width);
            Assert.Equal(48, decoded.Height);
        }
        finally
        {
            // Chỉ xóa thư mục tạm do chính kiểm thử này tạo.
            Directory.Delete(directory, recursive: true);
        }
    }

    private static void PreparePipeline(ServerStreamService service)
    {
        typeof(ServerStreamService).GetMethod("PrepareRuntimePipeline", Private)!
            .Invoke(service, [new ServerSettings { MaxDecodeQueueSize = 2 }]);
        typeof(ServerStreamService).GetProperty(nameof(ServerStreamService.IsRunning))!.SetValue(service, true);
    }

    private static T GetField<T>(object instance, string name) =>
        (T)instance.GetType().GetField(name, Private)!.GetValue(instance)!;

    private static void SetField(object instance, string name, object value) =>
        instance.GetType().GetField(name, Private)!.SetValue(instance, value);

    private static void PumpUntil(Func<bool> completed)
    {
        var timeout = Stopwatch.StartNew();
        while (!completed() && timeout.Elapsed < TimeSpan.FromSeconds(5))
        {
            Application.DoEvents();
            Thread.Sleep(5);
        }
        Assert.True(completed(), "Cửa sổ chưa đóng sau khi tác vụ nền đã kết thúc.");
    }

    private static void RunSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { error = ex; }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)), "Kiểm thử giao diện vượt thời gian chờ.");
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }
}
