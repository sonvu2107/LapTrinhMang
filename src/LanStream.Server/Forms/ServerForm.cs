using System;
using System.Drawing;
using System.Windows.Forms;
using LanStream.Server.Config;
using LanStream.Server.Services;
using LanStream.Shared.Models;

namespace LanStream.Server.Forms;

/// <summary>
/// Giao diện chính của ứng dụng LAN Video Streaming Server.
/// Phụ trách: TV1.
/// </summary>
public partial class ServerForm : Form
{
    private readonly ServerStreamService _serverService;
    private readonly ServerSettings _settings;

    public ServerForm()
    {
        InitializeComponent();

        _settings = new ServerSettings();
        _serverService = new ServerStreamService();
        _serverService.FrameDecoded += OnFrameDecoded;
    }

    private void OnFrameDecoded(Bitmap bitmap, VideoFrame frame)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateVideoView(bitmap, frame));
            return;
        }

        UpdateVideoView(bitmap, frame);
    }

    private void UpdateVideoView(Bitmap newBitmap, VideoFrame frame)
    {
        var oldImage = picReceivedVideo.Image;
        picReceivedVideo.Image = newBitmap;
        oldImage?.Dispose();

        lblClientStatus.Text = $"Source: {frame.RemoteEndPoint} | Đang nhận video";
    }

    private async void btnStartServer_Click(object? sender, EventArgs e)
    {
        try
        {
            _settings.ListenPort = (int)numListenPort.Value;
            _settings.EnableFaceDetection = chkFaceDetection.Checked;

            btnStartServer.Enabled = false;
            btnStopServer.Enabled = true;
            lblClientStatus.Text = $"Listening on UDP {_settings.ListenPort} - Waiting for video...";

            await _serverService.StartServerAsync(_settings);
        }
        catch (NotImplementedException ex)
        {
            btnStartServer.Enabled = true;
            btnStopServer.Enabled = false;
            lblClientStatus.Text = "Waiting for video... (Skeleton)";
            MessageBox.Show(ex.Message, "Thông báo khung dự án", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            btnStartServer.Enabled = true;
            btnStopServer.Enabled = false;
            lblClientStatus.Text = "Lỗi khởi động Server";
            MessageBox.Show(ex.Message, "Lỗi Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnStopServer_Click(object? sender, EventArgs e)
    {
        await _serverService.StopServerAsync();
        btnStartServer.Enabled = true;
        btnStopServer.Enabled = false;
        btnRecord.Enabled = true;
        btnStopRecord.Enabled = false;
        lblClientStatus.Text = "Đã dừng lắng nghe";
        lblStatus.Text = "FPS: 0 | Dropped: 0 | Recording: Off";
    }

    private void btnRecord_Click(object? sender, EventArgs e)
    {
        try
        {
            string filePath = _serverService.StartRecording(_settings.RecordingsDirectory);
            btnRecord.Enabled = false;
            btnStopRecord.Enabled = true;
            lblStatus.Text = $"FPS: 0 | Dropped: 0 | Recording: {filePath}";
        }
        catch (NotImplementedException ex)
        {
            MessageBox.Show(ex.Message, "Thông báo khung dự án", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi ghi hình", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnStopRecord_Click(object? sender, EventArgs e)
    {
        _serverService.StopRecording();
        btnRecord.Enabled = true;
        btnStopRecord.Enabled = false;
        lblStatus.Text = "FPS: 0 | Dropped: 0 | Recording: Off";
    }

    private void chkFaceDetection_CheckedChanged(object? sender, EventArgs e)
    {
        _settings.EnableFaceDetection = chkFaceDetection.Checked;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _serverService.FrameDecoded -= OnFrameDecoded;
        _serverService.Dispose();
        picReceivedVideo.Image?.Dispose();
        base.OnFormClosing(e);
    }
}
