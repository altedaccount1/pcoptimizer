// LicenseForm.cs - Modern UI with rounded elements and clean design
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using PCOptimizer.Security;

namespace PCOptimizer
{
    public partial class LicenseForm : Form
    {
        private LicenseManager licenseManager;

        private Panel headerPanel, contentPanel, buttonPanel;
        private Label titleLabel, subtitleLabel;
        private PictureBox logoBox;

        private Panel licensePanel, hardwarePanel;
        private TextBox textBoxLicenseKey;
        private Button btnValidate, btnCancel, btnPurchase, btnCopyHardwareId;
        private Label labelStatus, labelHardwareId, labelHardwareTitle;
        private ProgressBar progressValidation;

        public LicenseForm()
        {
            licenseManager = new LicenseManager();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "PC Optimizer - License Activation";
            this.Size = new Size(550, 450);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);

            CreateModernLicenseUI();

            this.ResumeLayout(false);
        }

        private void CreateModernLicenseUI()
        {
            CreateHeader();
            CreateContent();
            CreateButtons();
            SetupEventHandlers();
        }

        private void CreateHeader()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            // Add gradient background
            headerPanel.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    headerPanel.ClientRectangle,
                    Color.FromArgb(25, 25, 25),
                    Color.FromArgb(15, 15, 15),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
                }
            };

            // Logo
            logoBox = new PictureBox
            {
                Location = new Point(20, 15),
                Size = new Size(50, 50),
                BackColor = Color.FromArgb(0, 120, 215),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            logoBox.Paint += (s, e) =>
            {
                using (var path = CreateRoundedRectanglePath(logoBox.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);

                    using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
                    using (var textBrush = new SolidBrush(Color.White))
                    {
                        var textSize = e.Graphics.MeasureString("PC", font);
                        var x = (logoBox.Width - textSize.Width) / 2;
                        var y = (logoBox.Height - textSize.Height) / 2;
                        e.Graphics.DrawString("PC", font, textBrush, x, y);
                    }
                }
            };
            headerPanel.Controls.Add(logoBox);

            titleLabel = new Label
            {
                Text = "PC Optimizer",
                Location = new Point(85, 20),
                Size = new Size(200, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(titleLabel);

            subtitleLabel = new Label
            {
                Text = "License Activation",
                Location = new Point(85, 45),
                Size = new Size(200, 20),
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };
            headerPanel.Controls.Add(subtitleLabel);

            // Close button
            var closeBtn = new Button
            {
                Text = "×",
                Location = new Point(this.Width - 50, 15),
                Size = new Size(40, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 53, 69);
            closeBtn.Click += (s, e) => this.Close();
            headerPanel.Controls.Add(closeBtn);

            this.Controls.Add(headerPanel);
        }

        private void CreateContent()
        {
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18),
                Padding = new Padding(30, 20, 30, 20)
            };

            // License key section
            licensePanel = CreateModernCard("Enter License Key", new Point(0, 20), new Size(490, 120));

            textBoxLicenseKey = new TextBox
            {
                Location = new Point(20, 55),
                Size = new Size(450, 30),
                Font = new Font("Consolas", 11),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.FromArgb(120, 120, 120),
                BorderStyle = BorderStyle.None,
                Text = "Enter your license key here...",
            };

            // Custom rounded textbox appearance
            var textBoxBorder = new Panel
            {
                Location = new Point(15, 50),
                Size = new Size(460, 40),
                BackColor = Color.FromArgb(40, 40, 40)
            };
            textBoxBorder.Paint += (s, e) =>
            {
                using (var path = CreateRoundedRectanglePath(textBoxBorder.ClientRectangle, 8))
                using (var brush = new SolidBrush(Color.FromArgb(40, 40, 40)))
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 1))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                    if (textBoxLicenseKey.Focused)
                        e.Graphics.DrawPath(pen, path);
                }
            };

            textBoxLicenseKey.Location = new Point(10, 10);
            textBoxLicenseKey.Size = new Size(440, 20);
            textBoxBorder.Controls.Add(textBoxLicenseKey);
            licensePanel.Controls.Add(textBoxBorder);
            contentPanel.Controls.Add(licensePanel);

            // Hardware ID section
            hardwarePanel = CreateModernCard("Hardware Information", new Point(0, 160), new Size(490, 100));

            labelHardwareTitle = new Label
            {
                Text = "Hardware ID (for support):",
                Location = new Point(20, 50),
                Size = new Size(200, 20),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent
            };
            hardwarePanel.Controls.Add(labelHardwareTitle);

            labelHardwareId = new Label
            {
                Text = licenseManager.GetHardwareId(),
                Location = new Point(20, 72),
                Size = new Size(300, 20),
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Consolas", 9),
                BackColor = Color.Transparent
            };
            hardwarePanel.Controls.Add(labelHardwareId);

            btnCopyHardwareId = CreateModernButton("Copy ID", new Point(390, 65), new Size(80, 25));
            btnCopyHardwareId.BackColor = Color.FromArgb(108, 117, 125);
            btnCopyHardwareId.Font = new Font("Segoe UI", 8);
            btnCopyHardwareId.Click += BtnCopyHardwareId_Click;
            hardwarePanel.Controls.Add(btnCopyHardwareId);
            contentPanel.Controls.Add(hardwarePanel);

            // Status section
            labelStatus = new Label
            {
                Text = "Enter a valid license key to continue",
                Location = new Point(20, 280),
                Size = new Size(450, 25),
                ForeColor = Color.FromArgb(255, 193, 7),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(labelStatus);

            // Progress bar
            progressValidation = new ProgressBar
            {
                Location = new Point(50, 315),
                Size = new Size(390, 8),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            contentPanel.Controls.Add(progressValidation);

            this.Controls.Add(contentPanel);
        }

        private void CreateButtons()
        {
            buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(25, 25, 25),
                Padding = new Padding(30, 15, 30, 15)
            };

            btnValidate = CreateModernButton("Activate License", new Point(30, 15), new Size(140, 40));
            btnValidate.BackColor = Color.FromArgb(0, 120, 215);
            btnValidate.Enabled = false;
            btnValidate.Click += BtnValidate_Click;
            buttonPanel.Controls.Add(btnValidate);

            btnPurchase = CreateModernButton("Purchase License", new Point(190, 15), new Size(140, 40));
            btnPurchase.BackColor = Color.FromArgb(40, 167, 69);
            btnPurchase.Click += BtnPurchase_Click;
            buttonPanel.Controls.Add(btnPurchase);

            btnCancel = CreateModernButton("Cancel", new Point(350, 15), new Size(100, 40));
            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.Click += BtnCancel_Click;
            buttonPanel.Controls.Add(btnCancel);

            this.Controls.Add(buttonPanel);
        }

        private Panel CreateModernCard(string title, Point location, Size size)
        {
            var card = new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.FromArgb(28, 28, 28)
            };

            card.Paint += (s, e) =>
            {
                var rect = card.ClientRectangle;
                using (var path = CreateRoundedRectanglePath(rect, 12))
                using (var brush = new SolidBrush(Color.FromArgb(28, 28, 28)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);

                    using (var pen = new Pen(Color.FromArgb(45, 45, 45), 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(20, 15),
                Size = new Size(size.Width - 40, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            card.Controls.Add(titleLabel);

            return card;
        }

        private Button CreateModernButton(string text, Point location, Size size)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.Paint += (s, e) =>
            {
                var rect = btn.ClientRectangle;
                using (var path = CreateRoundedRectanglePath(rect, 8))
                using (var brush = new SolidBrush(btn.BackColor))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);

                    using (var textBrush = new SolidBrush(btn.ForeColor))
                    {
                        var textSize = e.Graphics.MeasureString(btn.Text, btn.Font);
                        var x = (btn.Width - textSize.Width) / 2;
                        var y = (btn.Height - textSize.Height) / 2;
                        e.Graphics.DrawString(btn.Text, btn.Font, textBrush, x, y);
                    }
                }
            };

            btn.MouseEnter += (s, e) =>
            {
                var originalColor = btn.BackColor;
                btn.BackColor = Color.FromArgb(
                    Math.Min(255, originalColor.R + 30),
                    Math.Min(255, originalColor.G + 30),
                    Math.Min(255, originalColor.B + 30));
                btn.Invalidate();
            };

            btn.MouseLeave += (s, e) =>
            {
                // Reset to original color based on button type
                if (btn == btnValidate)
                    btn.BackColor = Color.FromArgb(0, 120, 215);
                else if (btn == btnPurchase)
                    btn.BackColor = Color.FromArgb(40, 167, 69);
                else if (btn == btnCancel)
                    btn.BackColor = Color.FromArgb(108, 117, 125);
                else if (btn == btnCopyHardwareId)
                    btn.BackColor = Color.FromArgb(108, 117, 125);

                btn.Invalidate();
            };

            return btn;
        }

        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            var path = new GraphicsPath();
            int diameter = cornerRadius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private void SetupEventHandlers()
        {
            textBoxLicenseKey.TextChanged += TextBoxLicenseKey_TextChanged;
            textBoxLicenseKey.GotFocus += TextBoxLicenseKey_GotFocus;
            textBoxLicenseKey.LostFocus += TextBoxLicenseKey_LostFocus;

            // Make form draggable
            headerPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
        }

        // Windows API for dragging
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void TextBoxLicenseKey_GotFocus(object sender, EventArgs e)
        {
            if (textBoxLicenseKey.Text == "Enter your license key here...")
            {
                textBoxLicenseKey.Text = "";
                textBoxLicenseKey.ForeColor = Color.White;
            }

            // Redraw parent for border highlight
            textBoxLicenseKey.Parent.Invalidate();
        }

        private void TextBoxLicenseKey_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxLicenseKey.Text))
            {
                textBoxLicenseKey.Text = "Enter your license key here...";
                textBoxLicenseKey.ForeColor = Color.FromArgb(120, 120, 120);
            }

            // Redraw parent for border
            textBoxLicenseKey.Parent.Invalidate();
        }

        private void TextBoxLicenseKey_TextChanged(object sender, EventArgs e)
        {
            string key = textBoxLicenseKey.Text.Trim();
            btnValidate.Enabled = key.Length >= 20 && key != "Enter your license key here...";

            if (key.Length > 0 && key != "Enter your license key here...")
            {
                labelStatus.Text = "Ready to validate license key";
                labelStatus.ForeColor = Color.FromArgb(255, 193, 7);
            }
            else
            {
                labelStatus.Text = "Enter a valid license key to continue";
                labelStatus.ForeColor = Color.FromArgb(160, 160, 160);
            }
        }

        private void BtnCopyHardwareId_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(licenseManager.GetHardwareId());
                labelStatus.Text = "Hardware ID copied to clipboard!";
                labelStatus.ForeColor = Color.FromArgb(40, 167, 69);

                // Reset status after 3 seconds
                var timer = new Timer { Interval = 3000 };
                timer.Tick += (s, args) =>
                {
                    labelStatus.Text = "Enter a valid license key to continue";
                    labelStatus.ForeColor = Color.FromArgb(160, 160, 160);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Failed to copy: {ex.Message}";
                labelStatus.ForeColor = Color.FromArgb(220, 53, 69);
            }
        }

        private async void BtnValidate_Click(object sender, EventArgs e)
        {
            string licenseKey = textBoxLicenseKey.Text.Trim();

            if (string.IsNullOrEmpty(licenseKey) || licenseKey == "Enter your license key here...")
            {
                ShowMessage("Please enter a license key.", MessageType.Warning);
                return;
            }

            SetValidationState(false, "Validating license...");

            try
            {
                await Task.Delay(1000); // UX delay

                var result = await licenseManager.ValidateLicenseAsync(licenseKey);

                if (result.IsValid)
                {
                    ShowMessage("License activated successfully!", MessageType.Success);
                    await Task.Delay(1500);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowMessage($"License validation failed: {result.ErrorMessage}", MessageType.Error);
                    SetValidationState(true, "Validation failed");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Validation error: {ex.Message}", MessageType.Error);
                SetValidationState(true, "Validation error occurred");
            }
        }

        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(licenseManager.GetHardwareId());

                var purchaseForm = new Form
                {
                    Text = "Purchase License - PC Optimizer",
                    Size = new Size(500, 350),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(32, 32, 32),
                    ForeColor = Color.White,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var infoLabel = new Label
                {
                    Text = "To purchase a PC Optimizer license:\n\n" +
                          "📧 Email us with your Hardware ID\n" +
                          "🌐 Visit our website for purchase options\n" +
                          "💼 Contact support for custom licensing\n\n" +
                          $"Your Hardware ID: {licenseManager.GetHardwareId()}\n\n" +
                          "✅ Hardware ID copied to clipboard automatically!",
                    Location = new Point(30, 30),
                    Size = new Size(440, 220),
                    Font = new Font("Segoe UI", 11),
                    BackColor = Color.Transparent
                };
                purchaseForm.Controls.Add(infoLabel);

                var okButton = new Button
                {
                    Text = "Got it!",
                    Location = new Point(200, 270),
                    Size = new Size(100, 35),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                okButton.FlatAppearance.BorderSize = 0;
                okButton.Click += (s, e) => purchaseForm.Close();
                purchaseForm.Controls.Add(okButton);

                purchaseForm.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}", MessageType.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SetValidationState(bool enabled, string message)
        {
            btnValidate.Enabled = enabled;
            btnPurchase.Enabled = enabled;
            textBoxLicenseKey.Enabled = enabled;
            progressValidation.Visible = !enabled;

            labelStatus.Text = message;
            if (!enabled)
                labelStatus.ForeColor = Color.FromArgb(255, 193, 7);
        }

        private enum MessageType
        {
            Success,
            Warning,
            Error,
            Info
        }

        private void ShowMessage(string message, MessageType type)
        {
            labelStatus.Text = message;
            labelStatus.ForeColor = type switch
            {
                MessageType.Success => Color.FromArgb(40, 167, 69),
                MessageType.Warning => Color.FromArgb(255, 193, 7),
                MessageType.Error => Color.FromArgb(220, 53, 69),
                MessageType.Info => Color.FromArgb(23, 162, 184),
                _ => Color.White
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                licenseManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}