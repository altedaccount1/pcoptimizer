// LicenseForm.cs - Clean licensing without demo/trial modes
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using PCOptimizer.Security;

namespace PCOptimizer
{
    public partial class LicenseForm : Form
    {
        private LicenseManager licenseManager;

        private TextBox textBoxLicenseKey;
        private Button btnValidate, btnCancel, btnPurchase;
        private Label labelStatus, labelHardwareId, labelTitle;
        private ProgressBar progressValidation;
        private Panel panelLicense;

        public LicenseForm()
        {
            licenseManager = new LicenseManager();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "License Activation - PC Optimizer";
            this.Size = new Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.White;

            CreateControls();

            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            // Title
            labelTitle = new Label();
            labelTitle.Text = "PC Optimizer";
            labelTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            labelTitle.ForeColor = Color.FromArgb(0, 150, 255);
            labelTitle.Location = new Point(20, 20);
            labelTitle.Size = new Size(450, 30);
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(labelTitle);

            // License panel
            panelLicense = new Panel();
            panelLicense.Location = new Point(20, 60);
            panelLicense.Size = new Size(440, 160);
            panelLicense.BorderStyle = BorderStyle.FixedSingle;
            panelLicense.BackColor = Color.FromArgb(45, 45, 45);

            // License key label
            Label labelLicense = new Label();
            labelLicense.Text = "Enter your license key:";
            labelLicense.Location = new Point(20, 20);
            labelLicense.Size = new Size(200, 20);
            labelLicense.ForeColor = Color.White;
            panelLicense.Controls.Add(labelLicense);

            // License key textbox
            textBoxLicenseKey = new TextBox();
            textBoxLicenseKey.Location = new Point(20, 45);
            textBoxLicenseKey.Size = new Size(400, 25);
            textBoxLicenseKey.Font = new Font("Consolas", 10);
            textBoxLicenseKey.BackColor = Color.FromArgb(60, 60, 60);
            textBoxLicenseKey.ForeColor = Color.White;
            textBoxLicenseKey.BorderStyle = BorderStyle.FixedSingle;
            textBoxLicenseKey.Text = "Enter license key here...";
            textBoxLicenseKey.TextChanged += TextBoxLicenseKey_TextChanged;
            textBoxLicenseKey.GotFocus += TextBoxLicenseKey_GotFocus;
            panelLicense.Controls.Add(textBoxLicenseKey);

            // Hardware ID label
            labelHardwareId = new Label();
            labelHardwareId.Text = $"Hardware ID: {licenseManager.GetHardwareId()}";
            labelHardwareId.Location = new Point(20, 80);
            labelHardwareId.Size = new Size(300, 20);
            labelHardwareId.ForeColor = Color.LightGray;
            labelHardwareId.Font = new Font("Segoe UI", 8);
            panelLicense.Controls.Add(labelHardwareId);

            // Copy Hardware ID button
            Button btnCopyHardwareId = new Button();
            btnCopyHardwareId.Text = "Copy";
            btnCopyHardwareId.Location = new Point(330, 78);
            btnCopyHardwareId.Size = new Size(90, 22);
            btnCopyHardwareId.BackColor = Color.FromArgb(80, 80, 80);
            btnCopyHardwareId.ForeColor = Color.White;
            btnCopyHardwareId.FlatStyle = FlatStyle.Flat;
            btnCopyHardwareId.FlatAppearance.BorderSize = 0;
            btnCopyHardwareId.Font = new Font("Segoe UI", 8);
            btnCopyHardwareId.Click += (s, e) => {
                Clipboard.SetText(licenseManager.GetHardwareId());
                MessageBox.Show("Hardware ID copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panelLicense.Controls.Add(btnCopyHardwareId);

            // Status label
            labelStatus = new Label();
            labelStatus.Text = "Enter a valid license key to continue";
            labelStatus.Location = new Point(20, 110);
            labelStatus.Size = new Size(400, 20);
            labelStatus.ForeColor = Color.Yellow;
            panelLicense.Controls.Add(labelStatus);

            // Progress bar
            progressValidation = new ProgressBar();
            progressValidation.Location = new Point(20, 130);
            progressValidation.Size = new Size(400, 20);
            progressValidation.Style = ProgressBarStyle.Marquee;
            progressValidation.Visible = false;
            panelLicense.Controls.Add(progressValidation);

            this.Controls.Add(panelLicense);

            // Buttons
            btnValidate = new Button();
            btnValidate.Text = "Activate License";
            btnValidate.Location = new Point(20, 240);
            btnValidate.Size = new Size(120, 35);
            btnValidate.BackColor = Color.FromArgb(0, 120, 215);
            btnValidate.ForeColor = Color.White;
            btnValidate.FlatStyle = FlatStyle.Flat;
            btnValidate.FlatAppearance.BorderSize = 0;
            btnValidate.Enabled = false;
            btnValidate.Click += BtnValidate_Click;
            this.Controls.Add(btnValidate);

            btnPurchase = new Button();
            btnPurchase.Text = "Purchase License";
            btnPurchase.Location = new Point(160, 240);
            btnPurchase.Size = new Size(120, 35);
            btnPurchase.BackColor = Color.FromArgb(0, 150, 0);
            btnPurchase.ForeColor = Color.White;
            btnPurchase.FlatStyle = FlatStyle.Flat;
            btnPurchase.FlatAppearance.BorderSize = 0;
            btnPurchase.Click += BtnPurchase_Click;
            this.Controls.Add(btnPurchase);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(300, 240);
            btnCancel.Size = new Size(100, 35);
            btnCancel.BackColor = Color.FromArgb(100, 100, 100);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        private void TextBoxLicenseKey_GotFocus(object sender, EventArgs e)
        {
            if (textBoxLicenseKey.Text == "Enter license key here...")
            {
                textBoxLicenseKey.Text = "";
                textBoxLicenseKey.ForeColor = Color.White;
            }
        }

        private void TextBoxLicenseKey_TextChanged(object sender, EventArgs e)
        {
            string key = textBoxLicenseKey.Text.Trim();
            btnValidate.Enabled = key.Length >= 20 && key != "Enter license key here...";

            if (key.Length > 0 && key != "Enter license key here...")
            {
                labelStatus.Text = "Ready to validate license key";
                labelStatus.ForeColor = Color.Yellow;
            }
            else
            {
                labelStatus.Text = "Enter a valid license key to continue";
                labelStatus.ForeColor = Color.LightGray;
            }
        }

        private async void BtnValidate_Click(object sender, EventArgs e)
        {
            string licenseKey = textBoxLicenseKey.Text.Trim();

            if (string.IsNullOrEmpty(licenseKey) || licenseKey == "Enter license key here...")
            {
                MessageBox.Show("Please enter a license key.", "Invalid Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable controls during validation
            btnValidate.Enabled = false;
            btnPurchase.Enabled = false;
            textBoxLicenseKey.Enabled = false;
            progressValidation.Visible = true;

            labelStatus.Text = "Validating license...";
            labelStatus.ForeColor = Color.Yellow;

            try
            {
                // Simulate validation delay for UX
                await Task.Delay(1000);

                var result = await licenseManager.ValidateLicenseAsync(licenseKey);

                if (result.IsValid)
                {
                    labelStatus.Text = "License activated successfully!";
                    labelStatus.ForeColor = Color.LimeGreen;

                    await Task.Delay(1000);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    labelStatus.Text = $"License validation failed: {result.ErrorMessage}";
                    labelStatus.ForeColor = Color.Red;

                    // Show detailed error message
                    MessageBox.Show($"License validation failed:\n\n{result.ErrorMessage}\n\nPlease check your license key and internet connection.",
                        "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Re-enable controls
                    btnValidate.Enabled = true;
                    btnPurchase.Enabled = true;
                    textBoxLicenseKey.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Validation error: {ex.Message}";
                labelStatus.ForeColor = Color.Red;

                MessageBox.Show($"An error occurred during validation:\n\n{ex.Message}",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Re-enable controls
                btnValidate.Enabled = true;
                btnPurchase.Enabled = true;
                textBoxLicenseKey.Enabled = true;
            }
            finally
            {
                progressValidation.Visible = false;
            }
        }

        private void BtnPurchase_Click(object sender, EventArgs e)
        {
            // Show purchase information
            MessageBox.Show(
                "To purchase a license:\n\n" +
                "1. Contact support with your Hardware ID\n" +
                "2. Visit our website for purchase options\n" +
                "3. Email us for custom licensing needs\n\n" +
                $"Your Hardware ID: {licenseManager.GetHardwareId()}\n\n" +
                "The Hardware ID has been copied to your clipboard.",
                "Purchase License",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Copy hardware ID to clipboard for easy sharing
            Clipboard.SetText(licenseManager.GetHardwareId());
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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