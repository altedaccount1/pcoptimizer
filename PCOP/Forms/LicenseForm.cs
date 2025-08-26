// LicenseForm.cs - Fixed and complete implementation
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
        private Button btnValidate, btnTrial, btnCancel;
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
            this.Text = "License Activation - PC Performance Optimizer Pro";
            this.Size = new Size(500, 350);
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
            labelTitle.Text = "PC Performance Optimizer Pro";
            labelTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            labelTitle.ForeColor = Color.FromArgb(0, 150, 255);
            labelTitle.Location = new Point(20, 20);
            labelTitle.Size = new Size(450, 30);
            labelTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(labelTitle);

            // License panel
            panelLicense = new Panel();
            panelLicense.Location = new Point(20, 60);
            panelLicense.Size = new Size(440, 200);
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
            labelHardwareId.Size = new Size(400, 20);
            labelHardwareId.ForeColor = Color.LightGray;
            labelHardwareId.Font = new Font("Segoe UI", 8);
            panelLicense.Controls.Add(labelHardwareId);

            // Status label
            labelStatus = new Label();
            labelStatus.Text = "Enter a valid license key to continue";
            labelStatus.Location = new Point(20, 110);
            labelStatus.Size = new Size(400, 40);
            labelStatus.ForeColor = Color.Yellow;
            panelLicense.Controls.Add(labelStatus);

            // Progress bar
            progressValidation = new ProgressBar();
            progressValidation.Location = new Point(20, 160);
            progressValidation.Size = new Size(400, 20);
            progressValidation.Style = ProgressBarStyle.Marquee;
            progressValidation.Visible = false;
            panelLicense.Controls.Add(progressValidation);

            this.Controls.Add(panelLicense);

            // Buttons
            btnValidate = new Button();
            btnValidate.Text = "Activate License";
            btnValidate.Location = new Point(20, 280);
            btnValidate.Size = new Size(120, 35);
            btnValidate.BackColor = Color.FromArgb(0, 120, 215);
            btnValidate.ForeColor = Color.White;
            btnValidate.FlatStyle = FlatStyle.Flat;
            btnValidate.FlatAppearance.BorderSize = 0;
            btnValidate.Enabled = false;
            btnValidate.Click += BtnValidate_Click;
            this.Controls.Add(btnValidate);

            btnTrial = new Button();
            btnTrial.Text = "7-Day Trial";
            btnTrial.Location = new Point(160, 280);
            btnTrial.Size = new Size(100, 35);
            btnTrial.BackColor = Color.FromArgb(80, 80, 80);
            btnTrial.ForeColor = Color.White;
            btnTrial.FlatStyle = FlatStyle.Flat;
            btnTrial.FlatAppearance.BorderSize = 0;
            btnTrial.Click += BtnTrial_Click;
            this.Controls.Add(btnTrial);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(280, 280);
            btnCancel.Size = new Size(100, 35);
            btnCancel.BackColor = Color.FromArgb(100, 100, 100);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);

            // Additional info
            Label labelInfo = new Label();
            labelInfo.Text = "Need a license? Visit our website or contact support.";
            labelInfo.Location = new Point(20, 325);
            labelInfo.Size = new Size(400, 15);
            labelInfo.ForeColor = Color.LightGray;
            labelInfo.Font = new Font("Segoe UI", 8);
            this.Controls.Add(labelInfo);
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
            btnValidate.Enabled = key.Length >= 20; // Minimum length check

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
            btnTrial.Enabled = false;
            textBoxLicenseKey.Enabled = false;
            progressValidation.Visible = true;

            labelStatus.Text = "Validating license...";
            labelStatus.ForeColor = Color.Yellow;

            try
            {
                // Simulate validation delay
                await Task.Delay(2000);

                bool isValid = await licenseManager.ValidateLicenseOnline(licenseKey);

                if (isValid)
                {
                    labelStatus.Text = "License activated successfully!";
                    labelStatus.ForeColor = Color.LimeGreen;

                    await Task.Delay(1000);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    labelStatus.Text = "Invalid license key. Please check and try again.";
                    labelStatus.ForeColor = Color.Red;

                    // Re-enable controls
                    btnValidate.Enabled = true;
                    btnTrial.Enabled = true;
                    textBoxLicenseKey.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"Validation error: {ex.Message}";
                labelStatus.ForeColor = Color.Red;

                // Re-enable controls
                btnValidate.Enabled = true;
                btnTrial.Enabled = true;
                textBoxLicenseKey.Enabled = true;
            }
            finally
            {
                progressValidation.Visible = false;
            }
        }

        private void BtnTrial_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Start a 7-day free trial? Some features may be limited during the trial period.",
                "Trial Mode", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Set trial mode
                SetTrialMode();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SetTrialMode()
        {
            try
            {
                // Set trial expiration date
                DateTime trialExpiry = DateTime.Now.AddDays(7);

                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\PCOptimizer"))
                {
                    key.SetValue("TrialExpiry", trialExpiry.ToBinary());
                    key.SetValue("TrialMode", true);
                }

                MessageBox.Show("Trial mode activated! You have 7 days to evaluate the software.",
                    "Trial Activated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting trial mode: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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