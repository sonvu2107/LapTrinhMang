#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace LanStream.Client.Forms;

partial class ClientForm
{
    private System.ComponentModel.IContainer? components = null;

    private Panel pnlTop = null!;
    private Label lblServerIp = null!;
    private TextBox txtServerIp = null!;
    private Label lblPort = null!;
    private NumericUpDown numPort = null!;
    private Label lblCamera = null!;
    private ComboBox cboCamera = null!;
    private Label lblQuality = null!;
    private ComboBox cboQuality = null!;

    private PictureBox picPreview = null!;

    private Panel pnlBottom = null!;
    private Button btnStartCamera = null!;
    private Button btnStartStream = null!;
    private Button btnStopStream = null!;
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
        lblServerIp = new Label();
        txtServerIp = new TextBox();
        lblPort = new Label();
        numPort = new NumericUpDown();
        lblCamera = new Label();
        cboCamera = new ComboBox();
        lblQuality = new Label();
        cboQuality = new ComboBox();
        picPreview = new PictureBox();
        pnlBottom = new Panel();
        btnStartCamera = new Button();
        btnStartStream = new Button();
        btnStopStream = new Button();
        lblStatus = new Label();

        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
        pnlTop.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();

        // pnlTop
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Height = 76;
        pnlTop.Padding = new Padding(12);
        pnlTop.Controls.AddRange(new Control[]
        {
            lblServerIp, txtServerIp, lblPort, numPort,
            lblCamera, cboCamera, lblQuality, cboQuality
        });

        // Row 1: Server IP & Port
        lblServerIp.AutoSize = true;
        lblServerIp.Location = new Point(12, 14);
        lblServerIp.Text = "Server IP:";

        txtServerIp.Location = new Point(82, 10);
        txtServerIp.Width = 150;
        txtServerIp.Text = "192.168.1.100";

        lblPort.AutoSize = true;
        lblPort.Location = new Point(252, 14);
        lblPort.Text = "Port:";

        numPort.Location = new Point(295, 10);
        numPort.Width = 90;
        numPort.Minimum = 1024;
        numPort.Maximum = 65535;
        numPort.Value = 5000;

        // Row 2: Camera & Quality
        lblCamera.AutoSize = true;
        lblCamera.Location = new Point(12, 46);
        lblCamera.Text = "Camera:";

        cboCamera.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCamera.Items.AddRange(new object[] { "Webcam 0", "Webcam 1" });
        cboCamera.Location = new Point(82, 42);
        cboCamera.Width = 150;

        lblQuality.AutoSize = true;
        lblQuality.Location = new Point(252, 46);
        lblQuality.Text = "JPEG Quality:";

        cboQuality.DropDownStyle = ComboBoxStyle.DropDownList;
        cboQuality.Items.AddRange(new object[] { "40", "60", "70", "80" });
        cboQuality.Location = new Point(340, 42);
        cboQuality.Width = 75;

        // picPreview
        picPreview.Dock = DockStyle.Fill;
        picPreview.BackColor = Color.FromArgb(24, 24, 28);
        picPreview.BorderStyle = BorderStyle.FixedSingle;
        picPreview.SizeMode = PictureBoxSizeMode.Zoom;

        // pnlBottom
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 76;
        pnlBottom.Padding = new Padding(12);
        pnlBottom.Controls.AddRange(new Control[]
        {
            btnStartCamera, btnStartStream, btnStopStream, lblStatus
        });

        btnStartCamera.Location = new Point(12, 10);
        btnStartCamera.Size = new Size(120, 32);
        btnStartCamera.Text = "Start Camera";
        btnStartCamera.UseVisualStyleBackColor = true;
        btnStartCamera.Click += btnStartCamera_Click;

        btnStartStream.Location = new Point(142, 10);
        btnStartStream.Size = new Size(120, 32);
        btnStartStream.Text = "Start Stream";
        btnStartStream.UseVisualStyleBackColor = true;
        btnStartStream.Click += btnStartStream_Click;

        btnStopStream.Location = new Point(272, 10);
        btnStopStream.Size = new Size(110, 32);
        btnStopStream.Text = "Stop";
        btnStopStream.Enabled = false;
        btnStopStream.UseVisualStyleBackColor = true;
        btnStopStream.Click += btnStopStream_Click;

        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(12, 50);
        lblStatus.Text = "Trạng thái: Chờ thao tác | FPS: 0 | 640×480";

        // ClientForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(720, 620);
        MinimumSize = new Size(640, 520);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LAN Video Streaming - Client";
        Controls.Add(picPreview);
        Controls.Add(pnlTop);
        Controls.Add(pnlBottom);

        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        pnlBottom.ResumeLayout(false);
        pnlBottom.PerformLayout();
        ResumeLayout(false);
    }
}
