#nullable enable
using System.Drawing;
using System.Windows.Forms;

namespace LanStream.Client.Forms;

partial class ClientForm
{
    private System.ComponentModel.IContainer? components = null;

    // Header controls
    private Panel pnlHeader = null!;
    private Label lblLogoIcon = null!;
    private Label lblTitle = null!;
    private Label lblSubtitle = null!;
    private Panel pnlLiveBadge = null!;
    private Label lblLiveDot = null!;
    private Label lblLiveStatus = null!;
    private Label lblStreamDuration = null!;
    private Label lblFpsBadge = null!;
    private Label lblBitrateBadge = null!;
    private Label lblFramesBadge = null!;

    // Video preview monitor
    private Panel pnlVideoWrapper = null!;
    private PictureBox picPreview = null!;
    private Label lblOverlaySource = null!;
    private Label lblOverlayLiveTag = null!;

    // Bottom dashboard deck
    private Panel pnlBottom = null!;
    private TableLayoutPanel tlpBottom = null!;

    // Card 1: Broadcast server config
    private Panel pnlServerCard = null!;
    private Label lblServerCardTitle = null!;
    private Label lblServerIp = null!;
    private TextBox txtServerIp = null!;
    private Label lblPort = null!;
    private NumericUpDown numPort = null!;

    // Card 2: Camera & video settings
    private Panel pnlSettingsCard = null!;
    private Label lblSettingsCardTitle = null!;
    private Label lblCamera = null!;
    private ComboBox cboCamera = null!;
    private Label lblQuality = null!;
    private ComboBox cboQuality = null!;

    // Card 3: Live action controls
    private Panel pnlActionCard = null!;
    private Label lblActionCardTitle = null!;
    private Button btnStartStream = null!;
    private Button btnStopStream = null!;
    private Button btnStartCamera = null!;
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
        components = new System.ComponentModel.Container();

        // 1. Header controls
        pnlHeader = new Panel();
        lblLogoIcon = new Label();
        lblTitle = new Label();
        lblSubtitle = new Label();
        pnlLiveBadge = new Panel();
        lblLiveDot = new Label();
        lblLiveStatus = new Label();
        lblStreamDuration = new Label();
        lblFpsBadge = new Label();
        lblBitrateBadge = new Label();
        lblFramesBadge = new Label();

        // 2. Video Preview
        pnlVideoWrapper = new Panel();
        picPreview = new PictureBox();
        lblOverlaySource = new Label();
        lblOverlayLiveTag = new Label();

        // 3. Bottom deck & cards
        pnlBottom = new Panel();
        tlpBottom = new TableLayoutPanel();

        pnlServerCard = new Panel();
        lblServerCardTitle = new Label();
        lblServerIp = new Label();
        txtServerIp = new TextBox();
        lblPort = new Label();
        numPort = new NumericUpDown();

        pnlSettingsCard = new Panel();
        lblSettingsCardTitle = new Label();
        lblCamera = new Label();
        cboCamera = new ComboBox();
        lblQuality = new Label();
        cboQuality = new ComboBox();

        pnlActionCard = new Panel();
        lblActionCardTitle = new Label();
        btnStartStream = new Button();
        btnStopStream = new Button();
        btnStartCamera = new Button();
        lblStatus = new Label();

        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
        pnlHeader.SuspendLayout();
        pnlLiveBadge.SuspendLayout();
        pnlVideoWrapper.SuspendLayout();
        pnlBottom.SuspendLayout();
        tlpBottom.SuspendLayout();
        pnlServerCard.SuspendLayout();
        pnlSettingsCard.SuspendLayout();
        pnlActionCard.SuspendLayout();
        SuspendLayout();

        // ==========================================
        // HEADER BAR (LANstream Style)
        // ==========================================
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 64;
        pnlHeader.BackColor = Color.FromArgb(18, 18, 22);
        pnlHeader.Padding = new Padding(16, 8, 16, 8);
        pnlHeader.Controls.AddRange(new Control[]
        {
            lblLogoIcon, lblTitle, lblSubtitle,
            pnlLiveBadge,
            lblFpsBadge, lblBitrateBadge, lblFramesBadge
        });

        // LANstream Icon / Glow Logo
        lblLogoIcon.Text = "📡";
        lblLogoIcon.Font = new Font("Segoe UI Emoji", 16F, FontStyle.Regular);
        lblLogoIcon.ForeColor = Color.FromArgb(254, 44, 85);
        lblLogoIcon.Location = new Point(14, 12);
        lblLogoIcon.Size = new Size(36, 36);
        lblLogoIcon.TextAlign = ContentAlignment.MiddleCenter;

        // Title
        lblTitle.Text = "LANstream";
        lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(52, 9);
        lblTitle.AutoSize = true;

        // Subtitle
        lblSubtitle.Text = "LAN Video Streaming Client • OpenCvSharp & UDP";
        lblSubtitle.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
        lblSubtitle.ForeColor = Color.FromArgb(140, 140, 155);
        lblSubtitle.Location = new Point(54, 34);
        lblSubtitle.AutoSize = true;

        // Live Badge Pill (Center)
        pnlLiveBadge.BackColor = Color.FromArgb(28, 28, 34);
        pnlLiveBadge.Location = new Point(340, 14);
        pnlLiveBadge.Size = new Size(210, 36);
        pnlLiveBadge.Controls.AddRange(new Control[] { lblLiveDot, lblLiveStatus, lblStreamDuration });

        lblLiveDot.Text = "●";
        lblLiveDot.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblLiveDot.ForeColor = Color.FromArgb(110, 110, 125); // Gray when offline
        lblLiveDot.Location = new Point(8, 7);
        lblLiveDot.Size = new Size(18, 20);
        lblLiveDot.TextAlign = ContentAlignment.MiddleCenter;

        lblLiveStatus.Text = "OFFLINE";
        lblLiveStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblLiveStatus.ForeColor = Color.FromArgb(180, 180, 195);
        lblLiveStatus.Location = new Point(28, 8);
        lblLiveStatus.Size = new Size(82, 20);
        lblLiveStatus.TextAlign = ContentAlignment.MiddleLeft;

        lblStreamDuration.Text = "00:00:00";
        lblStreamDuration.Font = new Font("Consolas", 10F, FontStyle.Bold);
        lblStreamDuration.ForeColor = Color.FromArgb(220, 220, 230);
        lblStreamDuration.Location = new Point(114, 8);
        lblStreamDuration.Size = new Size(88, 20);
        lblStreamDuration.TextAlign = ContentAlignment.MiddleRight;

        // Badges on Right
        lblFpsBadge.Text = "⚡ 0.0 FPS";
        lblFpsBadge.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblFpsBadge.ForeColor = Color.FromArgb(37, 244, 238);
        lblFpsBadge.BackColor = Color.FromArgb(28, 28, 34);
        lblFpsBadge.Location = new Point(620, 16);
        lblFpsBadge.Size = new Size(85, 30);
        lblFpsBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblFpsBadge.TextAlign = ContentAlignment.MiddleCenter;

        lblBitrateBadge.Text = "📶 0 Kbps";
        lblBitrateBadge.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblBitrateBadge.ForeColor = Color.FromArgb(254, 44, 85);
        lblBitrateBadge.BackColor = Color.FromArgb(28, 28, 34);
        lblBitrateBadge.Location = new Point(715, 16);
        lblBitrateBadge.Size = new Size(100, 30);
        lblBitrateBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblBitrateBadge.TextAlign = ContentAlignment.MiddleCenter;

        lblFramesBadge.Text = "📦 0 f";
        lblFramesBadge.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblFramesBadge.ForeColor = Color.FromArgb(170, 170, 190);
        lblFramesBadge.BackColor = Color.FromArgb(28, 28, 34);
        lblFramesBadge.Location = new Point(825, 16);
        lblFramesBadge.Size = new Size(95, 30);
        lblFramesBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblFramesBadge.TextAlign = ContentAlignment.MiddleCenter;

        // ==========================================
        // VIDEO CANVAS (Center Monitor)
        // ==========================================
        pnlVideoWrapper.Dock = DockStyle.Fill;
        pnlVideoWrapper.BackColor = Color.FromArgb(12, 12, 15);
        pnlVideoWrapper.Padding = new Padding(12);
        pnlVideoWrapper.Controls.AddRange(new Control[]
        {
            lblOverlaySource, lblOverlayLiveTag, picPreview
        });

        // Overlay: Source Info (Top Left)
        lblOverlaySource.Text = "📷 WEBCAM • 640×480 • 15 FPS";
        lblOverlaySource.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblOverlaySource.ForeColor = Color.White;
        lblOverlaySource.BackColor = Color.FromArgb(180, 20, 20, 26);
        lblOverlaySource.Location = new Point(24, 24);
        lblOverlaySource.Size = new Size(220, 26);
        lblOverlaySource.TextAlign = ContentAlignment.MiddleCenter;

        // Overlay: LANstream LIVE tag (Top Right)
        lblOverlayLiveTag.Text = "LIVE";
        lblOverlayLiveTag.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblOverlayLiveTag.ForeColor = Color.White;
        lblOverlayLiveTag.BackColor = Color.FromArgb(200, 254, 44, 85);
        lblOverlayLiveTag.Location = new Point(860, 24);
        lblOverlayLiveTag.Size = new Size(54, 26);
        lblOverlayLiveTag.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblOverlayLiveTag.TextAlign = ContentAlignment.MiddleCenter;
        lblOverlayLiveTag.Visible = false;

        // picPreview
        picPreview.Dock = DockStyle.Fill;
        picPreview.BackColor = Color.FromArgb(16, 16, 20);
        picPreview.BorderStyle = BorderStyle.None;
        picPreview.SizeMode = PictureBoxSizeMode.Zoom;

        // ==========================================
        // BOTTOM DASHBOARD (Cards Layout)
        // ==========================================
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 175;
        pnlBottom.BackColor = Color.FromArgb(18, 18, 22);
        pnlBottom.Padding = new Padding(12, 8, 12, 8);
        pnlBottom.Controls.Add(tlpBottom);

        tlpBottom.Dock = DockStyle.Fill;
        tlpBottom.ColumnCount = 3;
        tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
        tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
        tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        tlpBottom.Controls.Add(pnlServerCard, 0, 0);
        tlpBottom.Controls.Add(pnlSettingsCard, 1, 0);
        tlpBottom.Controls.Add(pnlActionCard, 2, 0);

        // --- Card 1: Broadcast Destination ---
        pnlServerCard.Dock = DockStyle.Fill;
        pnlServerCard.BackColor = Color.FromArgb(26, 26, 32);
        pnlServerCard.Margin = new Padding(4);
        pnlServerCard.Padding = new Padding(10);
        pnlServerCard.Controls.AddRange(new Control[]
        {
            lblServerCardTitle, lblServerIp, txtServerIp, lblPort, numPort
        });

        lblServerCardTitle.Text = "🌐 MÁY CHỦ TIẾP NHẬN (SERVER)";
        lblServerCardTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblServerCardTitle.ForeColor = Color.FromArgb(37, 244, 238);
        lblServerCardTitle.Location = new Point(10, 8);
        lblServerCardTitle.Size = new Size(260, 20);

        lblServerIp.Text = "IPv4 Server:";
        lblServerIp.Font = new Font("Segoe UI", 8.5F);
        lblServerIp.ForeColor = Color.FromArgb(180, 180, 195);
        lblServerIp.Location = new Point(10, 36);
        lblServerIp.Size = new Size(80, 22);

        txtServerIp.Location = new Point(95, 34);
        txtServerIp.Size = new Size(165, 23);
        txtServerIp.Text = "192.168.1.100";
        txtServerIp.BackColor = Color.FromArgb(38, 38, 46);
        txtServerIp.ForeColor = Color.White;
        txtServerIp.BorderStyle = BorderStyle.FixedSingle;

        lblPort.Text = "UDP Port:";
        lblPort.Font = new Font("Segoe UI", 8.5F);
        lblPort.ForeColor = Color.FromArgb(180, 180, 195);
        lblPort.Location = new Point(10, 70);
        lblPort.Size = new Size(80, 22);

        numPort.Location = new Point(95, 68);
        numPort.Size = new Size(165, 23);
        numPort.Minimum = 1024;
        numPort.Maximum = 65535;
        numPort.Value = 5000;
        numPort.BackColor = Color.FromArgb(38, 38, 46);
        numPort.ForeColor = Color.White;
        numPort.BorderStyle = BorderStyle.FixedSingle;

        // --- Card 2: Camera & Video Settings ---
        pnlSettingsCard.Dock = DockStyle.Fill;
        pnlSettingsCard.BackColor = Color.FromArgb(26, 26, 32);
        pnlSettingsCard.Margin = new Padding(4);
        pnlSettingsCard.Padding = new Padding(10);
        pnlSettingsCard.Controls.AddRange(new Control[]
        {
            lblSettingsCardTitle, lblCamera, cboCamera, lblQuality, cboQuality
        });

        lblSettingsCardTitle.Text = "⚙ CẤU HÌNH THIẾT BỊ & NÉN";
        lblSettingsCardTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblSettingsCardTitle.ForeColor = Color.FromArgb(254, 44, 85);
        lblSettingsCardTitle.Location = new Point(10, 8);
        lblSettingsCardTitle.Size = new Size(260, 20);

        lblCamera.Text = "Camera:";
        lblCamera.Font = new Font("Segoe UI", 8.5F);
        lblCamera.ForeColor = Color.FromArgb(180, 180, 195);
        lblCamera.Location = new Point(10, 36);
        lblCamera.Size = new Size(85, 22);

        cboCamera.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCamera.Items.AddRange(new object[] { "Webcam 0 (Mặc định)", "Webcam 1 (Camera phụ)" });
        cboCamera.Location = new Point(98, 34);
        cboCamera.Size = new Size(175, 23);
        cboCamera.BackColor = Color.FromArgb(38, 38, 46);
        cboCamera.ForeColor = Color.White;
        cboCamera.FlatStyle = FlatStyle.Flat;

        lblQuality.Text = "Chất lượng:";
        lblQuality.Font = new Font("Segoe UI", 8.5F);
        lblQuality.ForeColor = Color.FromArgb(180, 180, 195);
        lblQuality.Location = new Point(10, 70);
        lblQuality.Size = new Size(85, 22);

        cboQuality.DropDownStyle = ComboBoxStyle.DropDownList;
        cboQuality.Items.AddRange(new object[]
        {
            "40 (Tiết kiệm)",
            "60 (Cân bằng - Đề xuất)",
            "70 (Độ nét cao)",
            "80 (Ultra HD)"
        });
        cboQuality.Location = new Point(98, 68);
        cboQuality.Size = new Size(175, 23);
        cboQuality.BackColor = Color.FromArgb(38, 38, 46);
        cboQuality.ForeColor = Color.White;
        cboQuality.FlatStyle = FlatStyle.Flat;

        // --- Card 3: Live Action Controls ---
        pnlActionCard.Dock = DockStyle.Fill;
        pnlActionCard.BackColor = Color.FromArgb(26, 26, 32);
        pnlActionCard.Margin = new Padding(4);
        pnlActionCard.Padding = new Padding(10);
        pnlActionCard.Controls.AddRange(new Control[]
        {
            lblActionCardTitle, btnStartStream, btnStopStream, btnStartCamera, lblStatus
        });

        lblActionCardTitle.Text = "🔴 ĐIỀU KHIỂN PHÁT SÓNG";
        lblActionCardTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblActionCardTitle.ForeColor = Color.FromArgb(254, 44, 85);
        lblActionCardTitle.Location = new Point(10, 8);
        lblActionCardTitle.Size = new Size(260, 20);

        // Big LANstream Live Red Button
        btnStartStream.Text = "🔴 BẮT ĐẦU LIVE";
        btnStartStream.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnStartStream.BackColor = Color.FromArgb(254, 44, 85);
        btnStartStream.ForeColor = Color.White;
        btnStartStream.FlatStyle = FlatStyle.Flat;
        btnStartStream.FlatAppearance.BorderSize = 0;
        btnStartStream.Cursor = Cursors.Hand;
        btnStartStream.Location = new Point(10, 32);
        btnStartStream.Size = new Size(148, 38);
        btnStartStream.Click += btnStartStream_Click;

        // Stop Stream Button
        btnStopStream.Text = "⏹ DỪNG LIVE";
        btnStopStream.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnStopStream.BackColor = Color.FromArgb(45, 45, 55);
        btnStopStream.ForeColor = Color.FromArgb(170, 170, 185);
        btnStopStream.FlatStyle = FlatStyle.Flat;
        btnStopStream.FlatAppearance.BorderSize = 0;
        btnStopStream.Cursor = Cursors.Hand;
        btnStopStream.Enabled = false;
        btnStopStream.Location = new Point(164, 32);
        btnStopStream.Size = new Size(125, 38);
        btnStopStream.Click += btnStopStream_Click;

        // Preview Camera Button
        btnStartCamera.Text = "👁 Xem trước Camera";
        btnStartCamera.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnStartCamera.BackColor = Color.FromArgb(38, 38, 48);
        btnStartCamera.ForeColor = Color.FromArgb(37, 244, 238);
        btnStartCamera.FlatStyle = FlatStyle.Flat;
        btnStartCamera.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 75);
        btnStartCamera.Cursor = Cursors.Hand;
        btnStartCamera.Location = new Point(10, 76);
        btnStartCamera.Size = new Size(279, 30);
        btnStartCamera.Click += btnStartCamera_Click;

        // Status Line
        lblStatus.Text = "Sẵn sàng phát sóng | Mạng LAN UDP";
        lblStatus.Font = new Font("Segoe UI", 8F);
        lblStatus.ForeColor = Color.FromArgb(150, 150, 170);
        lblStatus.Location = new Point(10, 114);
        lblStatus.Size = new Size(280, 24);
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;

        // ==========================================
        // CLIENT FORM PROPS
        // ==========================================
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 760);
        MinimumSize = new Size(960, 650);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LANstream - LAN Video Streaming (TV2 Client)";
        BackColor = Color.FromArgb(12, 12, 15);
        ForeColor = Color.White;

        Controls.Add(pnlVideoWrapper);
        Controls.Add(pnlHeader);
        Controls.Add(pnlBottom);

        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlLiveBadge.ResumeLayout(false);
        pnlVideoWrapper.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        tlpBottom.ResumeLayout(false);
        pnlServerCard.ResumeLayout(false);
        pnlServerCard.PerformLayout();
        pnlSettingsCard.ResumeLayout(false);
        pnlSettingsCard.PerformLayout();
        pnlActionCard.ResumeLayout(false);
        ResumeLayout(false);
    }
}
