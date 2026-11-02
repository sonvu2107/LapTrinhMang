#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace LanStream.Server.Forms;

partial class ServerForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlTop = null!;
    private Label lblListenPort = null!;
    private NumericUpDown numListenPort = null!;
    private Button btnStartServer = null!;
    private Button btnStopServer = null!;
    private Label lblClientStatus = null!;

    private PictureBox picReceivedVideo = null!;

    private Panel pnlBottom = null!;
    private Button btnRecord = null!;
    private Button btnStopRecord = null!;
    private CheckBox chkFaceDetection = null!;
    private Label lblStatus = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlTop = new Panel();
        lblListenPort = new Label();
        numListenPort = new NumericUpDown();
        btnStartServer = new Button();
        btnStopServer = new Button();
        lblClientStatus = new Label();
        picReceivedVideo = new PictureBox();
        pnlBottom = new Panel();
        btnRecord = new Button();
        btnStopRecord = new Button();
        chkFaceDetection = new CheckBox();
        lblStatus = new Label();

        ((System.ComponentModel.ISupportInitialize)numListenPort).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picReceivedVideo).BeginInit();
        pnlTop.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();

        // pnlTop
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Height = 76;
        pnlTop.Padding = new Padding(12);
        pnlTop.Controls.AddRange(new Control[]
        {
            lblListenPort, numListenPort, btnStartServer, btnStopServer, lblClientStatus
        });

        lblListenPort.AutoSize = true;
        lblListenPort.Location = new Point(12, 16);
        lblListenPort.Text = "Listening Port:";

        numListenPort.Location = new Point(108, 12);
        numListenPort.Width = 90;
        numListenPort.Minimum = 1024;
        numListenPort.Maximum = 65535;
        numListenPort.Value = 5000;

        btnStartServer.Location = new Point(214, 9);
        btnStartServer.Size = new Size(115, 30);
        btnStartServer.Text = "Start Server";
        btnStartServer.UseVisualStyleBackColor = true;
        btnStartServer.Click += btnStartServer_Click;

        btnStopServer.Location = new Point(338, 9);
        btnStopServer.Size = new Size(115, 30);
        btnStopServer.Text = "Stop Server";
        btnStopServer.Enabled = false;
        btnStopServer.UseVisualStyleBackColor = true;
        btnStopServer.Click += btnStopServer_Click;

        lblClientStatus.AutoSize = true;
        lblClientStatus.Location = new Point(12, 48);
        lblClientStatus.Text = "Client Status: Waiting for video...";

        // picReceivedVideo
        picReceivedVideo.Dock = DockStyle.Fill;
        picReceivedVideo.BackColor = Color.FromArgb(20, 20, 24);
        picReceivedVideo.BorderStyle = BorderStyle.FixedSingle;
        picReceivedVideo.SizeMode = PictureBoxSizeMode.Zoom;

        // pnlBottom
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 76;
        pnlBottom.Padding = new Padding(12);
        pnlBottom.Controls.AddRange(new Control[]
        {
            btnRecord, btnStopRecord, chkFaceDetection, lblStatus
        });

        btnRecord.Location = new Point(12, 10);
        btnRecord.Size = new Size(125, 32);
        btnRecord.Text = "Record";
        btnRecord.UseVisualStyleBackColor = true;
        btnRecord.Click += btnRecord_Click;

        btnStopRecord.Location = new Point(146, 10);
        btnStopRecord.Size = new Size(125, 32);
        btnStopRecord.Text = "Stop Recording";
        btnStopRecord.Enabled = false;
        btnStopRecord.UseVisualStyleBackColor = true;
        btnStopRecord.Click += btnStopRecord_Click;

        chkFaceDetection.AutoSize = true;
        chkFaceDetection.Location = new Point(290, 17);
        chkFaceDetection.Text = "Face Detection (OpenCV)";
        chkFaceDetection.CheckedChanged += chkFaceDetection_CheckedChanged;

        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(12, 50);
        lblStatus.Text = "FPS: 0 | Dropped: 0 | Recording: Off";

        // ServerForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(760, 620);
        MinimumSize = new Size(640, 520);
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
