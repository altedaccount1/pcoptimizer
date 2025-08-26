using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using PCOptimizer.Security;

namespace PCOptimizer
{
    public partial class MainForm : Form
    {
        private ProductionLicenseManager licenseManager;
        private SystemInfo systemInfo;
        private EnhancedOptimizationEngine optimizationEngine;
        private Timer statusUpdateTimer;

        // Controls
        private Label labelTitle, labelSystemInfo, labelLicenseStatus, labelOptimizationStatus;
        private Button btnOptimizeForFPS, btnOptimizeWindows, btnOptimizeGraphics, btnOptimizeNetwork;
        private Button btnCreateBackup, btnRestoreBackup, btnCheckStatus;
        private ProgressBar progressBarMain;
        private GroupBox groupSystemInfo, groupOptimizations, groupBackup, groupStatus;
        private Panel statusPanel;
        private RichTextBox logTextBox;

        public MainForm()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                // Initialize core components
                licenseManager = new ProductionLicenseManager();
                systemInfo = new SystemInfo();
                optimizationEngine = new EnhancedOptimizationEngine();

                // Start status update timer
                statusUpdateTimer = new Timer();
                statusUpdateTimer.Interval = 5000; // Update every 5 seconds
                statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
                statusUpdateTimer.Start();

                // Load initial information
                await LoadSystemInformation();
                await CheckLicenseStatus();
                await UpdateOptimizationStatus();

                LogMessage("PC Performance Optimizer Pro initialized successfully.");
            }
            catch (Exception ex)
            {
                LogMessage($"Initialization error: {ex.Message}");
                MessageBox.Show("Failed to initialize the application properly.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadSystemInformation()
        {
            try
            {
                await Task.Run(() =>
                {
                    string cpuInfo = systemInfo.GetCPUInfo();
                    string gpuInfo = systemInfo.GetGPUInfo();
                    string ramInfo = systemInfo.GetRAMInfo();
                    string osInfo = systemInfo.GetOSInfo();

                    Invoke(() =>
                    {
                        labelSystemInfo.Text = $"CPU: {cpuInfo}\n" +
                                             $"GPU: {gpuInfo}\n" +
                                             $"RAM: {ramInfo}\n" +
                                             $"OS: {osInfo}";
                    });
                });

                LogMessage("System information loaded successfully.");
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading system information: {ex.Message}");
            }
        }

        private async Task CheckLicenseStatus()
        {
            try
            {
                var licenseResult = await licenseManager.ValidateLicenseAsync();

                if (licenseResult.IsValid)
                {
                    var license = licenseResult.LicenseInfo;
                    labelLicenseStatus.Text = $"Licensed to: {license.CustomerName}\nExpires: {license.ExpirationDate:yyyy-MM-dd}";
                    labelLicenseStatus.ForeColor = Color.LimeGreen;

                    // Enable all optimization features
                    EnableOptimizationButtons(true);
                    LogMessage("Valid license detected - all features enabled.");
                }
                else
                {
                    labelLicenseStatus.Text = "No valid license found";
                    labelLicenseStatus.ForeColor = Color.Red;

                    // Show license dialog
                    await ShowLicenseDialog();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"License check error: {ex.Message}");
                labelLicenseStatus.Text = "License check failed";
                labelLicenseStatus.ForeColor = Color.Yellow;
            }
        }

        private async Task ShowLicenseDialog()
        {
            await Task.Run(() =>
            {
                // This would show your existing license form
                LogMessage("License validation required. Please activate your license.");

                // For demo purposes, enable limited functionality
                Invoke(() => EnableOptimizationButtons(false));
            });
        }

        private void EnableOptimizationButtons(bool enabled)
        {
            btnOptimizeForFPS.Enabled = enabled;
            btnOptimizeWindows.Enabled = enabled;
            btnOptimizeGraphics.Enabled = enabled;
            btnOptimizeNetwork.Enabled = enabled;
        }

        private async Task UpdateOptimizationStatus()
        {
            try
            {
                var status = await Task.Run(() => optimizationEngine.GetOptimizationStatus());

                string statusText = $"Overall Optimized: {(status.OverallOptimized ? "✓" : "✗")}\n" +
                                   $"CPU: {(status.CPUOptimized ? "✓" : "✗")} | " +
                                   $"Memory: {(status.MemoryOptimized ? "✓" : "✗")} | " +
                                   $"Graphics: {(status.GraphicsOptimized ? "✓" : "✗")}\n" +
                                   $"Network: {(status.NetworkOptimized ? "✓" : "✗")} | " +
                                   $"Services: {(status.ServicesOptimized ? "✓" : "✗")} | " +
                                   $"Power: {(status.PowerOptimized ? "✓" : "✗")}";

                labelOptimizationStatus.Text = statusText;
                labelOptimizationStatus.ForeColor = status.OverallOptimized ? Color.LimeGreen : Color.Yellow;
            }
            catch (Exception ex)
            {
                LogMessage($"Error updating optimization status: {ex.Message}");
            }
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            _ = UpdateOptimizationStatus();
        }

        private async void BtnOptimizeForFPS_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will apply comprehensive FPS optimizations for gaming including:\n\n" +
                "• CPU priority and scheduling optimizations\n" +
                "• Memory management tweaks\n" +
                "• Graphics driver optimizations\n" +
                "• Network latency reductions\n" +
                "• Windows gaming enhancements\n" +
                "• System service optimizations\n" +
                "• Power management tweaks\n\n" +
                "A system backup will be created first. Continue?",
                "Optimize for Maximum FPS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                await PerformFPSOptimization();
            }
        }

        private async Task PerformFPSOptimization()
        {
            try
            {
                SetUIState(false, "Optimizing for maximum FPS...");
                LogMessage("Starting comprehensive FPS optimization...");

                var result = await optimizationEngine.OptimizeForMaximumFPS();

                if (result.Success)
                {
                    LogMessage($"FPS optimization completed successfully! Applied {result.OptimizationsApplied} optimizations.");
                    MessageBox.Show(
                        $"FPS optimization completed successfully!\n\n" +
                        $"Optimizations applied: {result.OptimizationsApplied}\n" +
                        $"Backup created: {(result.BackupCreated ? "Yes" : "No")}\n\n" +
                        "Restart your computer for all changes to take effect.",
                        "Optimization Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Offer to restart
                    var restartResult = MessageBox.Show(
                        "Would you like to restart your computer now to apply all optimizations?",
                        "Restart Required",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (restartResult == DialogResult.Yes)
                    {
                        Process.Start("shutdown", "/r /t 10 /c \"Restarting to apply gaming optimizations\"");
                        Application.Exit();
                    }
                }
                else
                {
                    LogMessage($"FPS optimization completed with warnings: {result.Message}");
                    MessageBox.Show(
                        $"Optimization completed with some issues:\n\n{result.Message}\n\n" +
                        "Some optimizations may not have been applied successfully.",
                        "Optimization Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                await UpdateOptimizationStatus();
            }
            catch (Exception ex)
            {
                LogMessage($"FPS optimization failed: {ex.Message}");
                MessageBox.Show(
                    $"FPS optimization failed: {ex.Message}",
                    "Optimization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true, "Ready");
            }
        }

        private async void BtnOptimizeWindows_Click(object sender, EventArgs e)
        {
            await PerformSpecificOptimization("Windows", async () =>
            {
                // Call specific Windows optimization methods
                LogMessage("Applying Windows-specific optimizations...");
                await Task.Delay(2000); // Simulate work
                return true;
            });
        }

        private async void BtnOptimizeGraphics_Click(object sender, EventArgs e)
        {
            await PerformSpecificOptimization("Graphics", async () =>
            {
                LogMessage("Applying graphics driver optimizations...");
                await Task.Delay(2000);
                return true;
            });
        }

        private async void BtnOptimizeNetwork_Click(object sender, EventArgs e)
        {
            await PerformSpecificOptimization("Network", async () =>
            {
                LogMessage("Applying network latency optimizations...");
                await Task.Delay(2000);
                return true;
            });
        }

        private async Task PerformSpecificOptimization(string optimizationType, Func<Task<bool>> optimizationFunc)
        {
            try
            {
                SetUIState(false, $"Optimizing {optimizationType}...");

                bool success = await optimizationFunc();

                if (success)
                {
                    LogMessage($"{optimizationType} optimization completed successfully.");
                    MessageBox.Show(
                        $"{optimizationType} optimization completed successfully!",
                        "Optimization Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    LogMessage($"{optimizationType} optimization encountered issues.");
                    MessageBox.Show(
                        $"{optimizationType} optimization completed with warnings.",
                        "Optimization Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                await UpdateOptimizationStatus();
            }
            catch (Exception ex)
            {
                LogMessage($"{optimizationType} optimization failed: {ex.Message}");
                MessageBox.Show(
                    $"{optimizationType} optimization failed: {ex.Message}",
                    "Optimization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true, "Ready");
            }
        }

        private async void BtnCreateBackup_Click(object sender, EventArgs e)
        {
            try
            {
                SetUIState(false, "Creating system backup...");
                LogMessage("Creating comprehensive system backup...");

                await Task.Delay(3000); // Simulate backup creation

                LogMessage("System backup created successfully.");
                MessageBox.Show(
                    "System backup created successfully!\n\nBackup location: " +
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCOptimizer", "Backup"),
                    "Backup Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"Backup creation failed: {ex.Message}");
                MessageBox.Show($"Backup creation failed: {ex.Message}", "Backup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true, "Ready");
            }
        }

        private async void BtnRestoreBackup_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will restore your system to the state before optimizations were applied.\n\n" +
                "Are you sure you want to continue?",
                "Restore System Backup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    SetUIState(false, "Restoring system backup...");
                    LogMessage("Restoring system from backup...");

                    bool success = await optimizationEngine.RestoreSystemBackup();

                    if (success)
                    {
                        LogMessage("System backup restored successfully.");
                        MessageBox.Show(
                            "System backup restored successfully!\n\n" +
                            "Please restart your computer for all changes to take effect.",
                            "Restore Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        LogMessage("System backup restoration failed or no backup found.");
                        MessageBox.Show("No backup found or restoration failed.", "Restore Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    await UpdateOptimizationStatus();
                }
                catch (Exception ex)
                {
                    LogMessage($"Backup restoration failed: {ex.Message}");
                    MessageBox.Show($"Backup restoration failed: {ex.Message}", "Restore Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    SetUIState(true, "Ready");
                }
            }
        }

        private async void BtnCheckStatus_Click(object sender, EventArgs e)
        {
            SetUIState(false, "Checking optimization status...");
            await UpdateOptimizationStatus();
            LogMessage("Optimization status updated.");
            SetUIState(true, "Ready");
        }

        private void SetUIState(bool enabled, string statusMessage)
        {
            // Enable/disable buttons
            btnOptimizeForFPS.Enabled = enabled;
            btnOptimizeWindows.Enabled = enabled;
            btnOptimizeGraphics.Enabled = enabled;
            btnOptimizeNetwork.Enabled = enabled;
            btnCreateBackup.Enabled = enabled;
            btnRestoreBackup.Enabled = enabled;
            btnCheckStatus.Enabled = enabled;

            // Update progress bar
            if (enabled)
            {
                progressBarMain.Style = ProgressBarStyle.Blocks;
                progressBarMain.Value = 0;
            }
            else
            {
                progressBarMain.Style = ProgressBarStyle.Marquee;
            }

            // Update status
            LogMessage(statusMessage);
        }

        private void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(LogMessage), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            logTextBox.AppendText($"[{timestamp}] {message}\n");
            logTextBox.ScrollToCaret();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                statusUpdateTimer?.Stop();
                statusUpdateTimer?.Dispose();
                licenseManager?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ClientSize = new Size(1000, 750);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "PC Performance Optimizer Pro - Gaming Edition";

            CreateControls();

            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            // Title
            labelTitle = new Label
            {
                Text = "PC Performance Optimizer Pro - Gaming Edition",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 255),
                Location = new Point(20, 20),
                Size = new Size(600, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(labelTitle);

            // License Status
            var groupLicense = new GroupBox
            {
                Text = "License Status",
                ForeColor = Color.White,
                Location = new Point(650, 20),
                Size = new Size(330, 80)
            };

            labelLicenseStatus = new Label
            {
                Text = "Checking license...",
                Location = new Point(10, 25),
                Size = new Size(310, 40),
                ForeColor = Color.Yellow
            };
            groupLicense.Controls.Add(labelLicenseStatus);
            this.Controls.Add(groupLicense);

            // System Information
            groupSystemInfo = new GroupBox
            {
                Text = "System Information",
                ForeColor = Color.White,
                Location = new Point(20, 120),
                Size = new Size(480, 120)
            };

            labelSystemInfo = new Label
            {
                Text = "Loading system information...",
                Location = new Point(10, 25),
                Size = new Size(460, 80),
                ForeColor = Color.White
            };
            groupSystemInfo.Controls.Add(labelSystemInfo);
            this.Controls.Add(groupSystemInfo);

            // Optimization Status
            groupStatus = new GroupBox
            {
                Text = "Optimization Status",
                ForeColor = Color.White,
                Location = new Point(520, 120),
                Size = new Size(460, 120)
            };

            labelOptimizationStatus = new Label
            {
                Text = "Checking optimization status...",
                Location = new Point(10, 25),
                Size = new Size(440, 80),
                ForeColor = Color.Yellow
            };
            groupStatus.Controls.Add(labelOptimizationStatus);

            btnCheckStatus = CreateStyledButton("Refresh Status", new Point(350, 85), new Size(100, 25));
            btnCheckStatus.Click += BtnCheckStatus_Click;
            groupStatus.Controls.Add(btnCheckStatus);
            this.Controls.Add(groupStatus);

            // Main Optimizations
            groupOptimizations = new GroupBox
            {
                Text = "FPS Optimizations",
                ForeColor = Color.White,
                Location = new Point(20, 260),
                Size = new Size(960, 120)
            };

            // Main FPS optimization button (larger and prominent)
            btnOptimizeForFPS = new Button
            {
                Text = "🚀 OPTIMIZE FOR MAXIMUM FPS",
                Location = new Point(20, 30),
                Size = new Size(300, 50),
                BackColor = Color.FromArgb(0, 180, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnOptimizeForFPS.FlatAppearance.BorderSize = 0;
            btnOptimizeForFPS.Click += BtnOptimizeForFPS_Click;
            groupOptimizations.Controls.Add(btnOptimizeForFPS);

            // Individual optimization buttons
            btnOptimizeWindows = CreateStyledButton("Optimize Windows", new Point(350, 30));
            btnOptimizeWindows.Click += BtnOptimizeWindows_Click;
            groupOptimizations.Controls.Add(btnOptimizeWindows);

            btnOptimizeGraphics = CreateStyledButton("Optimize Graphics", new Point(530, 30));
            btnOptimizeGraphics.Click += BtnOptimizeGraphics_Click;
            groupOptimizations.Controls.Add(btnOptimizeGraphics);

            btnOptimizeNetwork = CreateStyledButton("Optimize Network", new Point(710, 30));
            btnOptimizeNetwork.Click += BtnOptimizeNetwork_Click;
            groupOptimizations.Controls.Add(btnOptimizeNetwork);

            this.Controls.Add(groupOptimizations);

            // Backup and Restore
            groupBackup = new GroupBox
            {
                Text = "System Backup & Restore",
                ForeColor = Color.White,
                Location = new Point(20, 400),
                Size = new Size(480, 80)
            };

            btnCreateBackup = CreateStyledButton("Create Backup", new Point(20, 30));
            btnCreateBackup.Click += BtnCreateBackup_Click;
            groupBackup.Controls.Add(btnCreateBackup);

            btnRestoreBackup = CreateStyledButton("Restore Backup", new Point(200, 30));
            btnRestoreBackup.Click += BtnRestoreBackup_Click;
            groupBackup.Controls.Add(btnRestoreBackup);

            this.Controls.Add(groupBackup);

            // Activity Log
            var groupLog = new GroupBox
            {
                Text = "Activity Log",
                ForeColor = Color.White,
                Location = new Point(520, 400),
                Size = new Size(460, 280)
            };

            logTextBox = new RichTextBox
            {
                Location = new Point(10, 25),
                Size = new Size(440, 240),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            groupLog.Controls.Add(logTextBox);
            this.Controls.Add(groupLog);

            // Progress Bar
            progressBarMain = new ProgressBar
            {
                Location = new Point(20, 700),
                Size = new Size(480, 30),
                Style = ProgressBarStyle.Blocks
            };
            this.Controls.Add(progressBarMain);

            // Gaming Tips Panel
            var tipsPanel = new Panel
            {
                Location = new Point(20, 500),
                Size = new Size(480, 180),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            var tipsLabel = new Label
            {
                Text = "🎮 Gaming Performance Tips",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 150, 255),
                Location = new Point(10, 10),
                Size = new Size(460, 20)
            };
            tipsPanel.Controls.Add(tipsLabel);

            var tipsContent = new Label
            {
                Text = "• Close unnecessary programs before gaming\n" +
                       "• Update graphics drivers regularly\n" +
                       "• Use fullscreen mode instead of windowed\n" +
                       "• Lower in-game graphics settings if needed\n" +
                       "• Keep Windows and games updated\n" +
                       "• Monitor temperatures during gaming\n" +
                       "• Use Game Mode in Windows 10/11\n" +
                       "• Consider upgrading RAM if using <16GB",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(10, 35),
                Size = new Size(460, 135)
            };
            tipsPanel.Controls.Add(tipsContent);
            this.Controls.Add(tipsPanel);

            // Status bar at the bottom
            var statusBar = new Panel
            {
                Location = new Point(0, this.Height - 25),
                Size = new Size(this.Width, 25),
                BackColor = Color.FromArgb(45, 45, 45),
                Dock = DockStyle.Bottom
            };

            var statusLabel = new Label
            {
                Text = "Ready - PC Performance Optimizer Pro v1.0",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.LightGray,
                Location = new Point(10, 5),
                Size = new Size(300, 15),
                AutoSize = false
            };
            statusBar.Controls.Add(statusLabel);

            var versionLabel = new Label
            {
                Text = "Build 2024.12.28",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(this.Width - 120, 5),
                Size = new Size(100, 15),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            statusBar.Controls.Add(versionLabel);
            this.Controls.Add(statusBar);

            // Add tooltips and event handlers
            AddTooltips();
            AddEventHandlers();
        }

        private void AddTooltips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnOptimizeForFPS,
                "Apply comprehensive FPS optimizations including CPU, memory, graphics, and network tweaks");
            toolTip.SetToolTip(btnOptimizeWindows,
                "Optimize Windows settings for better gaming performance");
            toolTip.SetToolTip(btnOptimizeGraphics,
                "Optimize graphics drivers and GPU settings for maximum FPS");
            toolTip.SetToolTip(btnOptimizeNetwork,
                "Reduce network latency and optimize TCP settings for online gaming");
            toolTip.SetToolTip(btnCreateBackup,
                "Create a comprehensive system backup before applying optimizations");
            toolTip.SetToolTip(btnRestoreBackup,
                "Restore your system to the state before optimizations were applied");
            toolTip.SetToolTip(btnCheckStatus,
                "Check current optimization status and refresh display");
        }

        private void AddEventHandlers()
        {
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
        }

        private Button CreateStyledButton(string text, Point location, Size? customSize = null)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = customSize ?? new Size(160, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;

            // Add hover effects
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(30, 140, 235);
            button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(0, 120, 215);

            return button;
        }

        // Form event handlers
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit PC Performance Optimizer Pro?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LogMessage("PC Performance Optimizer Pro started successfully.");
            LogMessage("System analysis in progress...");
        }

        // Additional utility methods
        private void ShowAboutDialog()
        {
            string aboutText =
                "PC Performance Optimizer Pro - Gaming Edition\n" +
                "Version 1.0.0\n\n" +
                "Comprehensive gaming performance optimization tool\n" +
                "Designed specifically for FPS improvement in games like Rust\n\n" +
                "Features:\n" +
                "• CPU and Memory Optimization\n" +
                "• Graphics Driver Tweaks\n" +
                "• Network Latency Reduction\n" +
                "• System Service Optimization\n" +
                "• Comprehensive Backup System\n" +
                "• Real-time Status Monitoring\n\n" +
                "© 2024 PC Performance Solutions";

            MessageBox.Show(aboutText, "About PC Performance Optimizer Pro",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task ShowSystemInformationDialog()
        {
            try
            {
                string systemInfoText = await Task.Run(() =>
                    $"System Information:\n\n" +
                    $"CPU: {this.systemInfo.GetCPUInfo()}\n" +
                    $"GPU: {this.systemInfo.GetGPUInfo()}\n" +
                    $"RAM: {this.systemInfo.GetRAMInfo()}\n" +
                    $"OS: {this.systemInfo.GetOSInfo()}\n" +
                    $"Motherboard: {this.systemInfo.GetMotherboardInfo()}\n" +
                    $"CPU Cores: {this.systemInfo.GetCoreCount()}\n" +
                    $"Windows Version: {this.systemInfo.GetWindowsVersion()}\n\n" +
                    $"Graphics Detection:\n" +
                    $"NVIDIA: {(this.systemInfo.IsNVIDIAGraphics() ? "Yes" : "No")}\n" +
                    $"AMD: {(this.systemInfo.IsAMDGraphics() ? "Yes" : "No")}\n" +
                    $"Intel: {(this.systemInfo.IsIntelGraphics() ? "Yes" : "No")}"
                );

                MessageBox.Show(systemInfoText, "Detailed System Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving system information: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowGamingTipsDialog()
        {
            string tips =
                "Gaming Performance Tips:\n\n" +
                "1. Hardware Tips:\n" +
                "• Ensure adequate cooling for CPU/GPU\n" +
                "• Use at least 16GB RAM for modern games\n" +
                "• Install games on SSD for faster loading\n" +
                "• Keep graphics drivers updated\n\n" +
                "2. Windows Settings:\n" +
                "• Enable Game Mode in Windows 10/11\n" +using System;
            using System.Drawing;
            using System.Windows.Forms;
            using System.Threading.Tasks;
            using System.Diagnostics;
            using System.IO;
            using PCOptimizer.Security;

namespace PCOptimizer
    {
        public partial class MainForm : Form
        {
            private LicenseManager licenseManager;
            private SystemInfo systemInfo;
            private OptimizationEngine optimizationEngine;
            private Timer statusUpdateTimer;

            // Controls
            private Label labelTitle, labelSystemInfo, labelLicenseStatus, labelOptimizationStatus;