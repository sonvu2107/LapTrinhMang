using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LanStream.Client.Config;
using LanStream.Client.Services;
using LanStream.Shared.Models;

namespace LanStream.Client.Forms;

/// <summary>
/// Giao diện chính của ứng dụng LAN Video Streaming Client (Phong cách TikTok Live Studio).
/// Phụ trách: TV2.
/// </summary>
public partial class ClientForm : Form
{
    private readonly ClientStreamService _streamService;
    private readonly ClientSettings _settings;
    private readonly System.Windows.Forms.Timer _durationTimer;
    private readonly Stopwatch _streamStopwatch = new();

    public ClientForm()
    {
        InitializeComponent();

        _settings = new ClientSettings();
        _streamService = new ClientStreamService();
        _streamService.PreviewFrameReady += OnPreviewFrameReady;
        _streamService.StatisticsUpdated += OnStatisticsUpdated;

        // Custom Paint for Preview placeholder
        picPreview.Paint += OnPicPreviewPaint;

        // Duration timer for TikTok Live
        _durationTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _durationTimer.Tick += OnDurationTimerTick;

        cboCamera.SelectedIndex = 0;
        cboQuality.SelectedIndex = 1; // 60 (Chuẩn TikTok)
        UpdateLiveStatusUi(LiveState.Offline);
    }

    private enum LiveState
    {
        Offline,
        Preview,
        Live
    }

    private void OnPicPreviewPaint(object? sender, PaintEventArgs e)
    {
        if (picPreview.Image is null)
        {
            var rect = picPreview.ClientRectangle;
            using var fontTitle = new Font("Segoe UI", 13F, FontStyle.Bold);
            using var fontSub = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            using var brushTitle = new SolidBrush(Color.FromArgb(254, 44, 85)); // TikTok Red
            using var brushSub = new SolidBrush(Color.FromArgb(130, 130, 150));
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var rectTitle = new RectangleF(rect.X, rect.Y - 22, rect.Width, rect.Height);
            var rectSub = new RectangleF(rect.X, rect.Y + 22, rect.Width, rect.Height);

            e.Graphics.DrawString("🎥 TIKTOK LAN LIVE STUDIO", fontTitle, brushTitle, rectTitle, format);
            e.Graphics.DrawString("Webcam chưa mở • Nhấn 'Xem trước Camera' hoặc 'BẮT ĐẦU LIVE'", fontSub, brushSub, rectSub, format);
        }
    }

    private void OnDurationTimerTick(object? sender, EventArgs e)
    {
        if (_streamStopwatch.IsRunning)
        {
            lblStreamDuration.Text = _streamStopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        }
    }

    private void UpdateLiveStatusUi(LiveState state)
    {
        switch (state)
        {
            case LiveState.Live:
                lblLiveDot.ForeColor = Color.FromArgb(254, 44, 85); // Red dot
                lblLiveStatus.Text = "LIVE";
                lblLiveStatus.ForeColor = Color.FromArgb(254, 44, 85);
                lblOverlayLiveTag.Visible = true;
                btnStartStream.Enabled = false;
                btnStartStream.BackColor = Color.FromArgb(60, 60, 70);
                btnStopStream.Enabled = true;
                btnStopStream.BackColor = Color.FromArgb(254, 44, 85);
                btnStopStream.ForeColor = Color.White;
                btnStartCamera.Enabled = false;
                break;

            case LiveState.Preview:
                lblLiveDot.ForeColor = Color.FromArgb(37, 244, 238); // Cyan dot
                lblLiveStatus.Text = "PREVIEW";
                lblLiveStatus.ForeColor = Color.FromArgb(37, 244, 238);
                lblOverlayLiveTag.Visible = false;
                btnStartCamera.Text = "⏹ Tắt Camera";
                btnStartCamera.ForeColor = Color.FromArgb(255, 120, 120);
                btnStartStream.Enabled = true;
                btnStartStream.BackColor = Color.FromArgb(254, 44, 85);
                btnStopStream.Enabled = false;
                btnStopStream.BackColor = Color.FromArgb(45, 45, 55);
                btnStopStream.ForeColor = Color.FromArgb(170, 170, 185);
                break;

            case LiveState.Offline:
            default:
                lblLiveDot.ForeColor = Color.FromArgb(110, 110, 125); // Gray dot
                lblLiveStatus.Text = "OFFLINE";
                lblLiveStatus.ForeColor = Color.FromArgb(180, 180, 195);
                lblStreamDuration.Text = "00:00:00";
                lblOverlayLiveTag.Visible = false;
                btnStartCamera.Text = "👁 Xem trước Camera";
                btnStartCamera.ForeColor = Color.FromArgb(37, 244, 238);
                btnStartCamera.Enabled = true;
                btnStartStream.Enabled = true;
                btnStartStream.BackColor = Color.FromArgb(254, 44, 85);
                btnStopStream.Enabled = false;
                btnStopStream.BackColor = Color.FromArgb(45, 45, 55);
                btnStopStream.ForeColor = Color.FromArgb(170, 170, 185);
                break;
        }
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
            BeginInvoke(() => UpdateStatsDisplay(stats));
            return;
        }

        UpdateStatsDisplay(stats);
    }

    private void UpdateStatsDisplay(StreamStatistics stats)
    {
        lblFpsBadge.Text = $"⚡ {stats.CurrentFps:F1} FPS";
        lblBitrateBadge.Text = $"📶 {stats.EstimatedBitrateKbps:F0} Kbps";
        lblFramesBadge.Text = $"📦 {stats.CompletedFrames} f";

        lblStatus.Text = $"Đang phát tới {_settings.ServerIp}:{_settings.ServerPort} | " +
                         $"Drop: {stats.DroppedFrames} frame | " +
                         $"Đã gửi: {stats.TotalBytesTransferred / 1024:N0} KB";
    }

    private async void btnStartCamera_Click(object? sender, EventArgs e)
    {
        try
        {
            // Bấm lần 2 để tắt camera preview nếu không stream
            if (_streamService.IsCameraRunning && !_streamService.IsStreaming)
            {
                await _streamService.StopCameraAsync();
                UpdateLiveStatusUi(LiveState.Offline);
                lblStatus.Text = "Đã dừng camera xem trước | Trạng thái: Offline";
                var oldImg = picPreview.Image;
                picPreview.Image = null;
                oldImg?.Dispose();
                picPreview.Invalidate();
                return;
            }

            int cameraIndex = Math.Max(0, cboCamera.SelectedIndex);
            await _streamService.StartCameraPreviewAsync(
                cameraIndex,
                _settings.FrameWidth,
                _settings.FrameHeight,
                _settings.TargetFps);

            UpdateLiveStatusUi(LiveState.Preview);
            lblOverlaySource.Text = $"📷 WEBCAM #{cameraIndex} • {_settings.FrameWidth}×{_settings.FrameHeight} • {_settings.TargetFps} FPS";
            lblStatus.Text = $"Đang xem trước Webcam #{cameraIndex} • Nhấn 'BẮT ĐẦU LIVE' để phát sóng";
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

            // Trích xuất số chất lượng JPEG từ chuỗi ComboBox
            string? qualityText = cboQuality.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(qualityText))
            {
                var match = Regex.Match(qualityText, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int q))
                {
                    _settings.JpegQuality = q;
                }
            }

            lblStatus.Text = $"Đang kết nối tới máy chủ {_settings.ServerIp}:{_settings.ServerPort}...";

            await _streamService.StartStreamAsync(_settings);

            UpdateLiveStatusUi(LiveState.Live);
            lblOverlaySource.Text = $"📷 WEBCAM #{_settings.CameraIndex} • {_settings.FrameWidth}×{_settings.FrameHeight} • Q{_settings.JpegQuality}";
            _streamStopwatch.Restart();
            _durationTimer.Start();
        }
        catch (NotImplementedException ex)
        {
            UpdateLiveStatusUi(_streamService.IsCameraRunning ? LiveState.Preview : LiveState.Offline);
            lblStatus.Text = "Module mạng (TV3) đang hoàn thiện | Chế độ Offline";
            MessageBox.Show(
                "Phần truyền tải UDP (TV3) hiện đang để trống:\n" + ex.Message +
                "\n\nBạn có thể nhấn 'Xem trước Camera' để kiểm tra kết nối Webcam & Preview của TV2!",
                "TikTok Live Studio - Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            UpdateLiveStatusUi(_streamService.IsCameraRunning ? LiveState.Preview : LiveState.Offline);
            lblStatus.Text = "Lỗi khởi động phát sóng";
            MessageBox.Show(ex.Message, "Lỗi Live Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnStopStream_Click(object? sender, EventArgs e)
    {
        _durationTimer.Stop();
        _streamStopwatch.Stop();

        await _streamService.StopStreamAsync();

        UpdateLiveStatusUi(LiveState.Offline);
        lblFpsBadge.Text = "⚡ 0.0 FPS";
        lblBitrateBadge.Text = "📶 0 Kbps";
        lblStatus.Text = "Đã dừng phiên Live | Trạng thái: Sẵn sàng";

        var oldImg = picPreview.Image;
        picPreview.Image = null;
        oldImg?.Dispose();
        picPreview.Invalidate();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _durationTimer.Stop();
        _durationTimer.Dispose();
        _streamService.PreviewFrameReady -= OnPreviewFrameReady;
        _streamService.StatisticsUpdated -= OnStatisticsUpdated;
        _streamService.Dispose();
        picPreview.Image?.Dispose();
        base.OnFormClosing(e);
    }
}
