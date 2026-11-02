using System;
using System.Drawing;
using System.Windows.Forms;
using LanStream.Client.Config;
using LanStream.Client.Services;

namespace LanStream.Client.Forms;

/// <summary>
/// Giao diện chính của ứng dụng LAN Video Streaming Client.
/// Phụ trách: TV2.
/// </summary>
public partial class ClientForm : Form
{
    private readonly ClientStreamService _streamService;
    private readonly ClientSettings _settings;

    public ClientForm()
    {
        InitializeComponent();

        _settings = new ClientSettings();
        _streamService = new ClientStreamService();
        _streamService.PreviewFrameReady += OnPreviewFrameReady;

        cboCamera.SelectedIndex = 0;
        cboQuality.SelectedItem = "60";
    }

    private void OnPreviewFrameReady(Bitmap frameBitmap)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdatePreviewImage(frameBitmap));
            return;
        }

        UpdatePreviewImage(frameBitmap);
    }

    private void UpdatePreviewImage(Bitmap newBitmap)
    {
        var oldImage = picPreview.Image;
        picPreview.Image = newBitmap;
        oldImage?.Dispose();
    }

    private void btnStartCamera_Click(object? sender, EventArgs e)
    {
        // TODO [TV2]: Mở preview Webcam cục bộ (chưa gửi UDP) qua CameraService.
        MessageBox.Show(
            "TODO [TV2]: Kết nối CameraService để hiển thị Webcam Preview.",
            "LAN Video Client",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private async void btnStartStream_Click(object? sender, EventArgs e)
    {
        try
        {
            _settings.ServerIp = txtServerIp.Text.Trim();
            _settings.ServerPort = (int)numPort.Value;
            _settings.CameraIndex = Math.Max(0, cboCamera.SelectedIndex);
            if (int.TryParse(cboQuality.SelectedItem?.ToString(), out int quality))
            {
                _settings.JpegQuality = quality;
            }

            btnStartStream.Enabled = false;
            btnStopStream.Enabled = true;
            lblStatus.Text = $"Trạng thái: Đang khởi động... | FPS: 0 | {_settings.FrameWidth}×{_settings.FrameHeight}";

            await _streamService.StartStreamAsync(_settings);
        }
        catch (NotImplementedException ex)
        {
            btnStartStream.Enabled = true;
            btnStopStream.Enabled = false;
            lblStatus.Text = "Trạng thái: Chưa triển khai (Skeleton) | FPS: 0 | 640×480";
            MessageBox.Show(ex.Message, "Thông báo khung dự án", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            btnStartStream.Enabled = true;
            btnStopStream.Enabled = false;
            lblStatus.Text = "Trạng thái: Lỗi khởi động | FPS: 0 | 640×480";
            MessageBox.Show(ex.Message, "Lỗi Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnStopStream_Click(object? sender, EventArgs e)
    {
        await _streamService.StopStreamAsync();
        btnStartStream.Enabled = true;
        btnStopStream.Enabled = false;
        lblStatus.Text = "Trạng thái: Đã dừng | FPS: 0 | 640×480";
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _streamService.PreviewFrameReady -= OnPreviewFrameReady;
        _streamService.Dispose();
        picPreview.Image?.Dispose();
        base.OnFormClosing(e);
    }
}
