using System;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using LanStream.Server.Config;
using LanStream.Server.Services;
using LanStream.Shared.Models;

namespace LanStream.Server.Forms;

/// <summary>
/// Main Windows Forms window for the LAN Video Streaming Server.
/// </summary>
public partial class ServerForm : Form
{
    private readonly ServerStreamService _serverService;
    private readonly ServerSettings _settings;

    private int _isUiFrameUpdatePending;
    private bool _isClosingForm;
    private bool _closeReady;

    public ServerForm()
    {
        InitializeComponent();

        _settings = new ServerSettings();
        _serverService = new ServerStreamService();

        InitializeServerUI();
    }

    private void InitializeServerUI()
    {
        numListenPort.Value = _settings.ListenPort;
        lblServerIp.Text = $"Server IP: {DetectLocalLanIpv4()}";
        UpdateButtonStates(isServerRunning: false, isRecording: false);

        _serverService.FrameDecoded += OnServiceFrameDecoded;
        _serverService.StatisticsUpdated += OnServiceStatisticsUpdated;
        _serverService.StatusChanged += UpdateServerStatus;
        _serverService.ErrorOccurred += OnServiceErrorOccurred;
    }

    private async void StartServer_Click(object? sender, EventArgs e)
    {
        try
        {
            ApplySettingsFromControls();
            UpdateButtonStates(isServerRunning: true, isRecording: false);

            await _serverService.StartAsync(_settings);
            if (chkFaceDetection.Checked && !_serverService.IsFaceDetectionEnabled)
            {
                ShowFaceModelError();
            }
        }
        catch (Exception ex)
        {
            UpdateButtonStates(isServerRunning: false, isRecording: false);
            UpdateServerStatus("Failed to start");
            MessageBox.Show(ex.Message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async void StopServer_Click(object? sender, EventArgs e)
    {
        btnStopServer.Enabled = false;
        btnStartRecording.Enabled = false;
        btnStopRecording.Enabled = false;
        await _serverService.StopAsync();

        UpdateButtonStates(isServerRunning: false, isRecording: false);
        lblRecordingPath.Text = "Recording: Off";
        lblRecordingPath.ForeColor = Color.DimGray;
    }

    private void StartRecording_Click(object? sender, EventArgs e)
    {
        try
        {
            string savedPath = _serverService.StartRecording();
            UpdateButtonStates(isServerRunning: _serverService.IsRunning, isRecording: true);

            lblRecordingPath.Text = $"Recording: On ({savedPath})";
            lblRecordingPath.ForeColor = Color.Firebrick;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to start video recording:\n{ex.Message}",
                "Recording Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void StopRecording_Click(object? sender, EventArgs e)
    {
        string? lastPath = _serverService.CurrentRecordingPath;
        _serverService.StopRecording();

        UpdateButtonStates(isServerRunning: _serverService.IsRunning, isRecording: false);
        lblRecordingPath.Text = lastPath is not null
            ? $"Recording: Saved to {lastPath}"
            : "Recording: Off";
        lblRecordingPath.ForeColor = Color.SeaGreen;
    }

    private void ChkFaceDetection_CheckedChanged(object? sender, EventArgs e)
    {
        if (_serverService.IsRunning && !_serverService.SetFaceDetectionEnabled(chkFaceDetection.Checked))
        {
            ShowFaceModelError();
        }
    }

    private void ShowFaceModelError()
    {
        chkFaceDetection.Checked = false;
        MessageBox.Show(
            this,
            $"Không tải được mô hình OpenCV từ đường dẫn {_settings.FaceModelPath}.\n" +
            "Hãy kiểm tra tệp mô hình rồi bật lại tính năng.",
            "Phát hiện khuôn mặt",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private void OnServiceFrameDecoded(Bitmap frameBitmap, VideoFrame metadata)
    {
        if (_isClosingForm || IsDisposed || !IsHandleCreated)
        {
            frameBitmap.Dispose();
            return;
        }

        if (!InvokeRequired)
        {
            UpdateVideoFrame(frameBitmap);
            return;
        }

        if (Interlocked.CompareExchange(ref _isUiFrameUpdatePending, 1, 0) != 0)
        {
            frameBitmap.Dispose();
            return;
        }

        try
        {
            BeginInvoke(() =>
            {
                try
                {
                    UpdateVideoFrame(frameBitmap);
                }
                finally
                {
                    Interlocked.Exchange(ref _isUiFrameUpdatePending, 0);
                }
            });
        }
        catch (ObjectDisposedException)
        {
            Interlocked.Exchange(ref _isUiFrameUpdatePending, 0);
            frameBitmap.Dispose();
        }
        catch (InvalidOperationException)
        {
            Interlocked.Exchange(ref _isUiFrameUpdatePending, 0);
            frameBitmap.Dispose();
        }
    }

    public void UpdateVideoFrame(Bitmap frame)
    {
        if (_isClosingForm || IsDisposed)
        {
            frame.Dispose();
            return;
        }

        Image? previousImage = picReceivedVideo.Image;
        picReceivedVideo.Image = frame;
        previousImage?.Dispose();
    }

    public void UpdateServerStatus(string status)
    {
        if (_isClosingForm || IsDisposed)
        {
            return;
        }

        if (InvokeRequired && IsHandleCreated)
        {
            BeginInvoke(() => UpdateServerStatus(status));
            return;
        }

        lblClientStatus.Text = $"Client Status: {status}";
    }

    public void UpdateFPS(double fps)
    {
        if (_isClosingForm || IsDisposed)
        {
            return;
        }

        if (InvokeRequired && IsHandleCreated)
        {
            BeginInvoke(() => UpdateFPS(fps));
            return;
        }

        StreamStatistics stats = _serverService.Statistics;
        lblFpsStats.Text =
            $"FPS: {fps:0.0} | Frames: {stats.CompletedFrames} | Dropped: {stats.DroppedFrames + stats.InvalidPackets} | Bitrate: {stats.EstimatedBitrateKbps:0.0} Kbps";
    }

    private void OnServiceStatisticsUpdated(StreamStatistics stats)
    {
        UpdateFPS(stats.CurrentFps);
    }

    private async void OnServiceErrorOccurred(string errorMessage)
    {
        if (_isClosingForm || IsDisposed)
        {
            return;
        }

        if (InvokeRequired && IsHandleCreated)
        {
            BeginInvoke(() => OnServiceErrorOccurred(errorMessage));
            return;
        }

        btnStartServer.Enabled = false;
        btnStopServer.Enabled = false;
        btnStartRecording.Enabled = false;
        btnStopRecording.Enabled = false;
        numListenPort.Enabled = false;
        await _serverService.StopAsync();
        if (_isClosingForm || IsDisposed)
        {
            return;
        }

        UpdateButtonStates(isServerRunning: false, isRecording: false);
        UpdateServerStatus("Stopped");
        MessageBox.Show(errorMessage, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ApplySettingsFromControls()
    {
        _settings.ListenPort = (int)numListenPort.Value;
        _settings.EnableFaceDetection = chkFaceDetection.Checked;
    }

    private void UpdateButtonStates(bool isServerRunning, bool isRecording)
    {
        numListenPort.Enabled = !isServerRunning;
        btnStartServer.Enabled = !isServerRunning;
        btnStopServer.Enabled = isServerRunning;

        btnStartRecording.Enabled = isServerRunning && !isRecording;
        btnStopRecording.Enabled = isServerRunning && isRecording;
    }

    private static string DetectLocalLanIpv4()
    {
        try
        {
            var candidateIps = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    !ni.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase) &&
                    !ni.Description.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase))
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(ua => ua.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(ua => ua.Address.ToString())
                .ToList();

            return candidateIps.FirstOrDefault() ?? "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        if (_closeReady)
        {
            return;
        }

        if (_isClosingForm)
        {
            e.Cancel = true;
            return;
        }

        base.OnFormClosing(e);
        if (e.Cancel)
        {
            return;
        }

        // WinForms không chờ async void: hoãn đóng cho tới khi tệp video được hoàn tất.
        e.Cancel = true;
        _isClosingForm = true;
        Enabled = false;

        try
        {
            await _serverService.StopAsync();
            _serverService.Dispose();

            _serverService.FrameDecoded -= OnServiceFrameDecoded;
            _serverService.StatisticsUpdated -= OnServiceStatisticsUpdated;
            _serverService.StatusChanged -= UpdateServerStatus;
            _serverService.ErrorOccurred -= OnServiceErrorOccurred;

            Image? currentImg = picReceivedVideo.Image;
            picReceivedVideo.Image = null;
            currentImg?.Dispose();

            _closeReady = true;
            BeginInvoke(() => Close());
        }
        catch (Exception ex)
        {
            _isClosingForm = false;
            Enabled = true;
            UpdateButtonStates(_serverService.IsRunning, _serverService.IsRecording);
            MessageBox.Show(ex.Message, "Shutdown Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
