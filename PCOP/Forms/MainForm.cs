using System;
using System.Drawing;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Management;
using System.IO;
using Microsoft.Win32;
using System.Net.Http;
using System.Text.Json;

namespace PCOptimizer.Security
{
    // Enhanced Main Form with Security Integration
    public partial class SecureMainForm : Form
    {
        private ProductionLicenseManager licenseManager;
        private SystemInfo systemInfo;
        private OptimizationEngine optimizationEngine;
        private Timer securityCheckTimer;
        private Timer licenseRecheckTimer;

        // Controls
        private Label labelTitle;
        private Label labelCPU, labelGPU, labelRAM, labelOS, labelLicenseStatus;
        private Button btnOptimizeWindows, btnOptimizeGraphics, btnGameTweaks;
        private Button btnCreateBackup, btnRestoreBackup;
        private ProgressBar progressBarMain;
        private GroupBox groupSystemInfo, groupOptimizations, groupBackup, groupLicense;

        public SecureMainForm()
        {
            InitializeComponent();
            InitializeSecureApplication();
        }

        private async void InitializeSecureApplication()
        {
            try
            {
                // 1. Comprehensive security checks
                var securityResult = PerformSecurityChecks();
                if (!securityResult.IsSafe)
                {
                    // Silently exit if environment is unsafe
                    Environment.Exit(0);
                    return;
                }

                // 2. Check admin privileges
                if (!IsRunningAsAdministrator())
                {
                    MessageBox.Show("This application requires administrator privileges to function properly.",
                        "Administrator Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RestartAsAdmin();
                    return;
                }

                // 3. Initialize secure components
                licenseManager = new ProductionLicenseManager();
                systemInfo = new SystemInfo();
                optimizationEngine = new OptimizationEngine();

                // 4. Continuous security monitoring
                securityCheckTimer = new Timer();
                securityCheckTimer.Interval = 30000; // Check every 30 seconds
                securityCheckTimer.Tick += OnSecurityCheck;
                securityCheckTimer.Start();

                // 5. License revalidation timer
                licenseRecheckTimer = new Timer();
                licenseRecheckTimer.Interval = 300000; // Recheck every 5 minutes
                licenseRecheckTimer.Tick += OnLicenseRecheck;
                licenseRecheckTimer.Start();

                // 6. Verify license
                await VerifyLicenseAsync();

                // 7. Load system information
                LoadSystemInfo();

                // 8. Set up form security
                this.TopMost = false;
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;

                UpdateLicenseStatusDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize application security.", "Security Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private SecurityCheckResult PerformSecurityChecks()
        {
            var result = new SecurityCheckResult();

            try
            {
                // 1. Anti-debugging checks
                result.IsDebuggerDetected = IsDebuggerPresent() || Debugger.IsAttached;

                // 2. Virtual machine detection
                result.IsVirtualMachine = DetectVirtualMachine();

                // 3. Process monitoring detection
                result.IsMonitoringDetected = DetectProcessMonitoring();

                // 4. Timing attack detection
                result.IsTimingAnomalyDetected = DetectTimingAnomaly();

                result.IsSafe = !result.IsDebuggerDetected &&
                               !result.IsVirtualMachine &&
                               !result.IsMonitoringDetected &&
                               !result.IsTimingAnomalyDetected;
            }
            catch (Exception)
            {
                result.IsSafe = false;
            }

            return result;
        }

        private bool DetectVirtualMachine()
        {
            try
            {
                // Check system manufacturer
                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString()?.ToLower() ?? "";
                        if (manufacturer.Contains("vmware") || manufacturer.Contains("virtualbox") ||
                            manufacturer.Contains("qemu") || manufacturer.Contains("microsoft corporation"))
                        {
                            return true;
                        }
                    }
                }

                // Check for VM files
                string[] vmFiles = {
                    @"C:\windows\system32\drivers\vmmouse.sys",
                    @"C:\windows\system32\drivers\VBoxMouse.sys"
                };

                foreach (string file in vmFiles)
                {
                    if (File.Exists(file)) return true;
                }
            }
            catch (Exception) { }

            return false;
        }

        private bool DetectProcessMonitoring()
        {
            try
            {
                string[] blacklistedProcesses = {
                    "ollydbg", "x64dbg", "windbg", "ida", "cheatengine", "processhacker"
                };

                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    string processName = process.ProcessName.ToLower();
                    foreach (string blacklisted in blacklistedProcesses)
                    {
                        if (processName.Contains(blacklisted))
                            return true;
                    }
                }
            }
            catch (Exception) { }

            return false;
        }

        private bool DetectTimingAnomaly()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < 1000; i++)
                {
                    Math.Sqrt(i * 3.14159);
                }
                stopwatch.Stop();

                return stopwatch.ElapsedMilliseconds > 100;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        private async Task VerifyLicenseAsync()
        {
            var result = await licenseManager.ValidateLicenseAsync();

            if (!result.IsValid)
            {
                await ShowSecureLicenseDialog();
            }
        }

        private async Task ShowSecureLicenseDialog()
        {
            using (var licenseForm = new SecureLicenseForm(licenseManager))
            {
                if (licenseForm.ShowDialog() != DialogResult.OK)
                {
                    Environment.Exit(0);
                }
            }
        }

        private void OnSecurityCheck(object sender, EventArgs e)
        {
            var result = PerformSecurityChecks();
            if (!result.IsSafe)
            {
                Environment.Exit(0);
            }
        }

        private async void OnLicenseRecheck(object sender, EventArgs e)
        {
            try
            {
                var result = await licenseManager.ValidateLicenseAsync();
                if (!result.IsValid)
                {
                    MessageBox.Show("License validation failed. Please restart the application.",
                        "License Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                UpdateLicenseStatusDisplay();
            }
            catch (Exception)
            {
                // Continue running if network error
            }
        }

        private bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void RestartAsAdmin()
        {
            var proc = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Application.ExecutablePath,
                Verb = "runas"
            };

            try
            {
                Process.Start(proc);
                Environment.Exit(0);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to restart as administrator.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void LoadSystemInfo()
        {
            try
            {
                labelCPU.Text = $"CPU: {systemInfo.GetCPUInfo()}";
                labelGPU.Text = $"GPU: {systemInfo.GetGPUInfo()}";
                labelRAM.Text = $"RAM: {systemInfo.GetRAMInfo()}";
                labelOS.Text = $"OS: {systemInfo.GetOSInfo()}";
            }
            catch (Exception)
            {
                labelCPU.Text = "CPU: Detection failed";
                labelGPU.Text = "GPU: Detection failed";
                labelRAM.Text = "RAM: Detection failed";
                labelOS.Text = $"OS: {Environment.OSVersion}";
            }
        }

        private void UpdateLicenseStatusDisplay()
        {
            var license = licenseManager.GetCurrentLicense();
            if (license != null)
            {
                labelLicenseStatus.Text = $"Licensed to: {license.CustomerName} (Expires: {license.ExpirationDate:yyyy-MM-dd})";
                labelLicenseStatus.ForeColor = Color.LimeGreen;
            }
            else
            {
                labelLicenseStatus.Text = "No valid license";
                labelLicenseStatus.ForeColor = Color.Red;
            }
        }

        // Optimization button handlers with security checks
        private async void BtnOptimizeWindows_Click(object sender, EventArgs e)
        {
            if (!await ValidateFeatureAccess())
                return;

            await PerformOptimizationAsync(() => optimizationEngine.OptimizeWindowsPerformance(),
                "Windows optimization completed successfully!");
        }

        private async void BtnOptimizeGraphics_Click(object sender, EventArgs e)
        {
            if (!await ValidateFeatureAccess())
                return;

            await PerformOptimizationAsync(() => optimizationEngine.OptimizeGraphicsSettings(),
                "Graphics optimization completed successfully!");
        }

        private async void BtnGameTweaks_Click(object sender, EventArgs e)
        {
            if (!await ValidateFeatureAccess())
                return;

            await PerformOptimizationAsync(() => optimizationEngine.ApplyGameTweaks(),
                "Game tweaks applied successfully!");
        }

        private async Task<bool> ValidateFeatureAccess()
        {
            // Additional security check before sensitive operations
            var securityCheck = PerformSecurityChecks();
            if (!securityCheck.IsSafe)
            {
                Environment.Exit(0);
                return false;
            }

            // Validate license is still valid
            var licenseResult = await licenseManager.ValidateLicenseAsync();
            if (!licenseResult.IsValid)
            {
                MessageBox.Show("License validation failed. Please restart the application.",
                    "License Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private async Task PerformOptimizationAsync(Action optimizationAction, string successMessage)
        {
            progressBarMain.Style = ProgressBarStyle.Marquee;
            SetButtonsEnabled(false);

            try
            {
                await Task.Run(optimizationAction);
                MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during optimization: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBarMain.Style = ProgressBarStyle.Blocks;
                SetButtonsEnabled(true);
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            btnOptimizeWindows.Enabled = enabled;
            btnOptimizeGraphics.Enabled = enabled;
            btnGameTweaks.Enabled = enabled;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                securityCheckTimer?.Stop();
                securityCheckTimer?.Dispose();
                licenseRecheckTimer?.Stop();
                licenseRecheckTimer?.Dispose();
                licenseManager?.Dispose();
            }
            base.Dispose(disposing);
        }

        // Form initialization code (same as before but with license status)
        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ClientSize = new Size(800, 650); // Slightly taller for license status
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SecureMainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "PC Performance Optimizer Pro";

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
            labelTitle.Size = new Size(400, 30);
            this.Controls.Add(labelTitle);

            // License Status Group
            groupLicense = new GroupBox();
            groupLicense.Text = "License Status";
            groupLicense.ForeColor = Color.White;
            groupLicense.Location = new Point(420, 20);
            groupLicense.Size = new Size(360, 60);

            labelLicenseStatus = new Label();
            labelLicenseStatus.Text = "Checking license...";
            labelLicenseStatus.Location = new Point(10, 25);
            labelLicenseStatus.Size = new Size(340, 20);
            labelLicenseStatus.ForeColor = Color.Yellow;

            groupLicense.Controls.Add(labelLicenseStatus);
            this.Controls.Add(groupLicense);

            // System Info Group
            groupSystemInfo = new GroupBox();
            groupSystemInfo.Text = "System Information";
            groupSystemInfo.ForeColor = Color.White;
            groupSystemInfo.Location = new Point(20, 100);
            groupSystemInfo.Size = new Size(360, 150);

            labelCPU = new Label() { Text = "CPU: Detecting...", Location = new Point(10, 25), Size = new Size(340, 20), ForeColor = Color.White };
            labelGPU = new Label() { Text = "GPU: Detecting...", Location = new Point(10, 50), Size = new Size(340, 20), ForeColor = Color.White };
            labelRAM = new Label() { Text = "RAM: Detecting...", Location = new Point(10, 75), Size = new Size(340, 20), ForeColor = Color.White };
            labelOS = new Label() { Text = "OS: Detecting...", Location = new Point(10, 100), Size = new Size(340, 20), ForeColor = Color.White };

            groupSystemInfo.Controls.AddRange(new Control[] { labelCPU, labelGPU, labelRAM, labelOS });
            this.Controls.Add(groupSystemInfo);

            // Optimizations Group
            groupOptimizations = new GroupBox();
            groupOptimizations.Text = "Performance Optimizations";
            groupOptimizations.ForeColor = Color.White;
            groupOptimizations.Location = new Point(400, 100);
            groupOptimizations.Size = new Size(360, 200);

            btnOptimizeWindows = CreateStyledButton("Optimize Windows Performance", new Point(20, 30));
            btnOptimizeGraphics = CreateStyledButton("Optimize Graphics Settings", new Point(20, 80));
            btnGameTweaks = CreateStyledButton("Apply Game Tweaks", new Point(20, 130));

            btnOptimizeWindows.Click += BtnOptimizeWindows_Click;
            btnOptimizeGraphics.Click += BtnOptimizeGraphics_Click;
            btnGameTweaks.Click += BtnGameTweaks_Click;

            groupOptimizations.Controls.AddRange(new Control[] { btnOptimizeWindows, btnOptimizeGraphics, btnGameTweaks });
            this.Controls.Add(groupOptimizations);

            // Backup Group
            groupBackup = new GroupBox();
            groupBackup.Text = "System Backup & Restore";
            groupBackup.ForeColor = Color.White;
            groupBackup.Location = new Point(20, 270);
            groupBackup.Size = new Size(360, 100);

            btnCreateBackup = CreateStyledButton("Create Backup", new Point(20, 30));
            btnRestoreBackup = CreateStyledButton("Restore Backup", new Point(200, 30));

            btnCreateBackup.Click += (s, e) => {
                try
                {
                    optimizationEngine.CreateSystemBackup();
                    MessageBox.Show("System backup created successfully!", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating backup: {ex.Message}", "Error");
                }
            };

            btnRestoreBackup.Click += (s, e) => {
                var result = MessageBox.Show("Are you sure you want to restore the system backup?",
                    "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        optimizationEngine.RestoreSystemBackup();
                        MessageBox.Show("System backup restored successfully!", "Success");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error restoring backup: {ex.Message}", "Error");
                    }
                }
            };

            groupBackup.Controls.AddRange(new Control[] { btnCreateBackup, btnRestoreBackup });
            this.Controls.Add(groupBackup);

            // Progress Bar
            progressBarMain = new ProgressBar();
            progressBarMain.Location = new Point(20, 600);
            progressBarMain.Size = new Size(740, 30);
            this.Controls.Add(progressBarMain);
        }

        private Button CreateStyledButton(string text, Point location)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = location;
            btn.Size = new Size(160, 35);
            btn.BackColor = Color.FromArgb(0, 120, 215);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9);
            return btn;
        }
    }

    // Supporting classes
    public class SecurityCheckResult
    {
        public bool IsSafe { get; set; }
        public bool IsDebuggerDetected { get; set; }
        public bool IsVirtualMachine { get; set; }
        public bool IsMonitoringDetected { get; set; }
        public bool IsTimingAnomalyDetected { get; set; }
    }
}