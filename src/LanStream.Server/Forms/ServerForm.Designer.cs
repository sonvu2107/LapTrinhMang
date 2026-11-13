#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace LanStream.Server.Forms;

partial class ServerForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlTop = null!;
    private Label lblServerIp = null!;
    private Label lblListenPort = null!;
    private NumericUpDown numListenPort = null!;
    private Button btnStartServer = null!;
    private Button btnStopServer = null!;
    private Label lblClientStatus = null!;

    private PictureBox picReceivedVideo = null!;

    private Panel pnlBottom = null!;
    private Button btnStartRecording = null!;
    private Button btnStopRecording = null!;
    private CheckBox chkFaceDetection = null!;
    private Label lblFpsStats = null!;
    private Label lblRecordingPath = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlTop = new Panel();
        lblServerIp = new Label();
        lblListenPort = new Label();
        numListenPort = new NumericUpDown();
        btnStartServer = new Button();
        btnStopServer = new Button();
        lblClientStatus = new Label();

        picReceivedVideo = new PictureBox();

        pnlBottom = new Panel();
        btnStartRecording = new Button();
        btnStopRecording = new Button();
        chkFaceDetection = new CheckBox();
        lblFpsStats = new Label();
        lblRecordingPath = new Label();

        ((System.ComponentModel.ISupportInitialize)numListenPort).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picReceivedVideo).BeginInit();
        pnlTop.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();

        // pnlTop
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Height = 84;
        pnlTop.Padding = new Padding(12);
        pnlTop.Controls.AddRange(new Control[]
        {
            lblServerIp, lblListenPort, numListenPort,
            btnStartServer, btnStopServer, lblClientStatus
        });

        lblListenPort.AutoSize = true;
        lblListenPort.Location = new Point(12, 16);
        lblListenPort.Text = "Listening Port:";

        numListenPort.Location = new Point(108, 12);
        numListenPort.Width = 85;
        numListenPort.Minimum = 1024;
        numListenPort.Maximum = 65535;
        numListenPort.Value = 5000;

        btnStartServer.Location = new Point(208, 10);
        btnStartServer.Size = new Size(120, 30);
        btnStartServer.Text = "Start Server";
        btnStartServer.UseVisualStyleBackColor = true;
        btnStartServer.Click += StartServer_Click;

        btnStopServer.Location = new Point(336, 10);
        btnStopServer.Size = new Size(120, 30);
        btnStopServer.Text = "Stop Server";
        btnStopServer.Enabled = false;
        btnStopServer.UseVisualStyleBackColor = true;
        btnStopServer.Click += StopServer_Click;

        lblServerIp.AutoSize = true;
        lblServerIp.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblServerIp.Location = new Point(474, 16);
        lblServerIp.Text = "Server IP: 127.0.0.1";

        lblClientStatus.AutoSize = true;
        lblClientStatus.Location = new Point(12, 52);
        lblClientStatus.Text = "Client Status: Stopped";

        // picReceivedVideo
        picReceivedVideo.Dock = DockStyle.Fill;
        picReceivedVideo.BackColor = Color.FromArgb(20, 22, 26);
        picReceivedVideo.BorderStyle = BorderStyle.FixedSingle;
        picReceivedVideo.SizeMode = PictureBoxSizeMode.Zoom;

        // pnlBottom
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 96;
        pnlBottom.Padding = new Padding(12);
        pnlBottom.Controls.AddRange(new Control[]
        {
            btnStartRecording, btnStopRecording, chkFaceDetection,
            lblFpsStats, lblRecordingPath
        });

        btnStartRecording.Location = new Point(12, 10);
        btnStartRecording.Size = new Size(135, 32);
        btnStartRecording.Text = "Start Recording";
        btnStartRecording.Enabled = false;
        btnStartRecording.UseVisualStyleBackColor = true;
        btnStartRecording.Click += StartRecording_Click;

        btnStopRecording.Location = new Point(155, 10);
        btnStopRecording.Size = new Size(135, 32);
        btnStopRecording.Text = "Stop Recording";
        btnStopRecording.Enabled = false;
        btnStopRecording.UseVisualStyleBackColor = true;
        btnStopRecording.Click += StopRecording_Click;

        chkFaceDetection.AutoSize = true;
        chkFaceDetection.Location = new Point(308, 16);
        chkFaceDetection.Text = "Face Detection";
        chkFaceDetection.CheckedChanged += ChkFaceDetection_CheckedChanged;

        lblFpsStats.AutoSize = true;
        lblFpsStats.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        lblFpsStats.Location = new Point(12, 50);
        lblFpsStats.Text = "FPS: 0.0 | Frames: 0 | Dropped: 0 | Bitrate: 0.0 Kbps";

        lblRecordingPath.AutoSize = true;
        lblRecordingPath.ForeColor = Color.DimGray;
        lblRecordingPath.Location = new Point(12, 72);
        lblRecordingPath.Text = "Recording: Off";

        // ServerForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(780, 640);
        MinimumSize = new Size(680, 540);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LAN Video Streaming - Server";
        Controls.Add(picReceivedVideo);
        Controls.Add(pnlTop);
        Controls.Add(pnlBottom);

        ((System.ComponentModel.ISupportInitialize)numListenPort).EndInit();
        ((System.ComponentModel.ISupportInitialize)picReceivedVideo).EndInit();
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        pnlBottom.ResumeLayout(false);
        pnlBottom.PerformLayout();
        ResumeLayout(false);
    }
}
