using System;
using System.Drawing;
using System.Windows.Forms;
using LanStream.Client.Config;
using LanStream.Client.Services;
using LanStream.Shared.Models;

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
        _streamService.StatisticsUpdated += OnStatisticsUpdated;

        cboCamera.SelectedIndex = 0;
        cboQuality.SelectedItem = "60";
    }

    private void OnPreviewFrameReady(Bitmap frameBitmap)
    {
        if (IsDisposed || Disposing)
        {
            frameBitmap.Dispose();
            return;
        }

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

    private void OnStatisticsUpdated(StreamStatistics stats)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateStatsLabel(stats));
            return;
        }

        UpdateStatsLabel(stats);
    }

    private void UpdateStatsLabel(StreamStatistics stats)
    {
        lblStatus.Text = $"Trạng thái: Đang stream tới {_settings.ServerIp}:{_settings.ServerPort} | " +
                         $"FPS: {stats.CurrentFps:F1} | " +
                         $"Băng thông: {stats.EstimatedBitrateKbps:F0} Kbps | " +
                         $"Đã gửi: {stats.CompletedFrames} frames";
    }

    private async void btnStartCamera_Click(object? sender, EventArgs e)
    {
        try
        {
            // Bấm lần 2 để tắt camera preview nếu không stream
            if (_streamService.IsCameraRunning && !_streamService.IsStreaming)
            {
                await _streamService.StopCameraAsync();
                btnStartCamera.Text = "Start Camera";
                lblStatus.Text = "Trạng thái: Đã dừng camera | FPS: 0 | 640×480";
                var oldImg = picPreview.Image;
                picPreview.Image = null;
                oldImg?.Dispose();
                return;
            }

            int cameraIndex = Math.Max(0, cboCamera.SelectedIndex);
            await _streamService.StartCameraPreviewAsync(
                cameraIndex,
                _settings.FrameWidth,
                _settings.FrameHeight,
                _settings.TargetFps);

            btnStartCamera.Text = "Stop Camera";
            lblStatus.Text = $"Trạng thái: Đang xem trước Webcam {cameraIndex} | {_settings.FrameWidth}×{_settings.FrameHeight} @ {_settings.TargetFps} FPS";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi kết nối Webcam", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
            btnStartCamera.Enabled = false;
            btnStopStream.Enabled = true;
            lblStatus.Text = $"Trạng thái: Đang kết nối tới {_settings.ServerIp}:{_settings.ServerPort}...";

            await _streamService.StartStreamAsync(_settings);
            btnStartCamera.Text = "Start Camera";
        }
        catch (NotImplementedException ex)
        {
            btnStartStream.Enabled = true;
            btnStartCamera.Enabled = true;
            btnStopStream.Enabled = false;
            lblStatus.Text = "Trạng thái: Module mạng (TV3) chưa cài đặt | FPS: 0";
            MessageBox.Show(
                "Phần giao tiếp UDP (TV3) hiện đang để trống:\n" + ex.Message +
                "\n\nBạn vẫn có thể nhấn 'Start Camera' để kiểm tra kết nối Webcam & Preview của TV2!",
                "Thông báo tích hợp",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            btnStartStream.Enabled = true;
            btnStartCamera.Enabled = true;
            btnStopStream.Enabled = false;
            lblStatus.Text = "Trạng thái: Lỗi khởi động | FPS: 0";
            MessageBox.Show(ex.Message, "Lỗi Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnStopStream_Click(object? sender, EventArgs e)
    {
        await _streamService.StopStreamAsync();
        btnStartStream.Enabled = true;
        btnStartCamera.Enabled = true;
        btnStartCamera.Text = "Start Camera";
        btnStopStream.Enabled = false;
        lblStatus.Text = "Trạng thái: Đã dừng | FPS: 0 | 640×480";
        var oldImg = picPreview.Image;
        picPreview.Image = null;
        oldImg?.Dispose();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _streamService.PreviewFrameReady -= OnPreviewFrameReady;
        _streamService.StatisticsUpdated -= OnStatisticsUpdated;
        _streamService.Dispose();
        picPreview.Image?.Dispose();
        base.OnFormClosing(e);
    }
}
