// MainForm.cs - Complete version with maintenance features
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PCOptimizer.Security;
using PCOptimizer.Cleanup;
using PCOptimizer.Diagnostics;

namespace PCOptimizer
{
    public partial class MainForm : Form
    {
        private LicenseManager licenseManager;
        private SystemInfo systemInfo;
        private OptimizationEngine optimizationEngine;
        private DiskCleanupManager diskCleanupManager;
        private SystemHealthChecker healthChecker;
        private Timer statusUpdateTimer;

        // Main Controls
        private Label labelTitle, labelSystemInfo, labelLicenseStatus, labelOptimizationStatus;
        private Label labelCurrentPing, labelPingStatus;
        private Button btnOptimizeForFPS, btnOptimizeWindows, btnOptimizeGraphics, btnOptimizeNetwork;
        private Button btnCreateBackup, btnRestoreBackup, btnCheckStatus, btnTestPing;
        private Button btnDiskCleanup, btnHealthCheck, btnQuickClean;
        private ProgressBar progressBarMain;
        private GroupBox groupSystemInfo, groupOptimizations, groupBackup, groupStatus, groupPing, groupMaintenance;
        private RichTextBox logTextBox;

        public MainForm()
        {
            InitializeComponent();
            Task.Run(async () => await InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Initialize core components
                licenseManager = new LicenseManager();
                systemInfo = new SystemInfo();
                optimizationEngine = new OptimizationEngine();
                diskCleanupManager = new DiskCleanupManager();
                healthChecker = new SystemHealthChecker();

                // Start status update timer
                statusUpdateTimer = new Timer();
                statusUpdateTimer.Interval = 5000; // Update every 5 seconds
                statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
                statusUpdateTimer.Start();

                // Load initial information
                await LoadSystemInformation();
                await CheckLicenseStatus();
                await UpdateOptimizationStatus();
                await TestCurrentPing();

                LogMessage("PC Performance Optimizer initialized successfully.");
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

                    this.Invoke(new Action(() =>
                    {
                        labelSystemInfo.Text = $"CPU: {cpuInfo}\n" +
                                             $"GPU: {gpuInfo}\n" +
                                             $"RAM: {ramInfo}\n" +
                                             $"OS: {osInfo}";
                    }));
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

                    EnableOptimizationButtons(true);
                    LogMessage("Valid license detected - all features enabled.");
                }
                else
                {
                    labelLicenseStatus.Text = "No valid license found";
                    labelLicenseStatus.ForeColor = Color.Red;

                    EnableOptimizationButtons(false);
                    LogMessage("License validation required.");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"License check error: {ex.Message}");
                labelLicenseStatus.Text = "License check failed";
                labelLicenseStatus.ForeColor = Color.Yellow;
                EnableOptimizationButtons(true); // Allow usage if license server is down
            }
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

        private async Task TestCurrentPing()
        {
            try
            {
                labelPingStatus.Text = "Testing ping...";
                labelPingStatus.ForeColor = Color.Yellow;

                int ping = await optimizationEngine.TestCurrentPing();

                if (ping > 0)
                {
                    labelCurrentPing.Text = $"Current Ping: {ping}ms";
                    labelPingStatus.Text = ping < 50 ? "Excellent" : ping < 100 ? "Good" : ping < 150 ? "Fair" : "Poor";
                    labelPingStatus.ForeColor = ping < 50 ? Color.LimeGreen : ping < 100 ? Color.Green : ping < 150 ? Color.Orange : Color.Red;
                }
                else
                {
                    labelCurrentPing.Text = "Ping test failed";
                    labelPingStatus.Text = "Unable to test";
                    labelPingStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ping test error: {ex.Message}");
                labelCurrentPing.Text = "Ping test error";
                labelPingStatus.Text = "Test failed";
                labelPingStatus.ForeColor = Color.Red;
            }
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            Task.Run(async () => await UpdateOptimizationStatus());
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
                "• Power management tweaks\n" +
                "• Ping optimization tweaks\n\n" +
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

                int pingBefore = await optimizationEngine.TestCurrentPing();
                LogMessage($"Ping before optimization: {(pingBefore > 0 ? pingBefore + "ms" : "Unable to test")}");

                var result = await optimizationEngine.OptimizeForMaximumFPS();

                if (result.Success)
                {
                    await Task.Delay(2000);
                    int pingAfter = await optimizationEngine.TestCurrentPing();

                    string pingImprovement = "";
                    if (pingBefore > 0 && pingAfter > 0)
                    {
                        int improvement = pingBefore - pingAfter;
                        pingImprovement = $"\nPing: {pingBefore}ms → {pingAfter}ms ({(improvement > 0 ? $"-{improvement}ms" : $"+{Math.Abs(improvement)}ms")})";
                    }

                    LogMessage($"FPS optimization completed successfully! Applied {result.OptimizationsApplied} optimizations.");
                    MessageBox.Show(
                        $"FPS optimization completed successfully!\n\n" +
                        $"Optimizations applied: {result.OptimizationsApplied}\n" +
                        $"Backup created: {(result.BackupCreated ? "Yes" : "No")}" +
                        pingImprovement + "\n\n" +
                        "Restart your computer for all changes to take effect.",
                        "Optimization Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

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
                await TestCurrentPing();
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
                LogMessage("Applying Windows-specific optimizations...");
                return await optimizationEngine.OptimizeWindowsForGaming();
            });
        }

        private async void BtnOptimizeGraphics_Click(object sender, EventArgs e)
        {
            await PerformSpecificOptimization("Graphics", async () =>
            {
                LogMessage("Applying graphics driver optimizations...");
                return await optimizationEngine.OptimizeGraphicsForGaming();
            });
        }

        private async void BtnOptimizeNetwork_Click(object sender, EventArgs e)
        {
            await PerformSpecificOptimization("Network", async () =>
            {
                LogMessage("Applying network latency optimizations...");
                bool result = await optimizationEngine.OptimizeNetworkForGaming();

                if (result)
                {
                    await Task.Delay(1000);
                    await TestCurrentPing();
                }

                return result;
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

        // Maintenance feature event handlers
        private async void BtnDiskCleanup_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will perform a comprehensive disk cleanup including:\n\n" +
                "• Temporary files and cache\n" +
                "• Browser data and downloads cache\n" +
                "• Windows update files\n" +
                "• System logs and error reports\n" +
                "• Game cache and shader files\n" +
                "• Recycle bin contents\n\n" +
                "This may take several minutes. Continue?",
                "Deep Disk Cleanup",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    SetUIState(false, "Deep cleanup in progress...");
                    LogMessage("Starting comprehensive disk cleanup...");

                    var diskInfo = await diskCleanupManager.GetDiskSpaceInfo();
                    LogMessage($"Free space before cleanup: {diskInfo.FreeSpaceGB} GB ({diskInfo.FreeSpacePercent:F1}%)");

                    var cleanupResult = await diskCleanupManager.PerformComprehensiveCleanup();

                    if (cleanupResult.Success)
                    {
                        var newDiskInfo = await diskCleanupManager.GetDiskSpaceInfo();
                        LogMessage($"Free space after cleanup: {newDiskInfo.FreeSpaceGB} GB ({newDiskInfo.FreeSpacePercent:F1}%)");

                        string improvementText = "";
                        if (newDiskInfo.FreeSpaceGB > diskInfo.FreeSpaceGB)
                        {
                            long improvement = newDiskInfo.FreeSpaceGB - diskInfo.FreeSpaceGB;
                            improvementText = $"\nImprovement: +{improvement} GB free space";
                        }

                        string cleanupDetails = string.Join("\n", cleanupResult.CleanupLog.Take(8));

                        MessageBox.Show(
                            $"Cleanup completed successfully!\n\n" +
                            $"Space freed: {cleanupResult.SpaceFreedMB:N0} MB\n" +
                            $"Current free space: {newDiskInfo.FreeSpaceGB} GB ({newDiskInfo.FreeSpacePercent:F1}%)" +
                            improvementText + "\n\n" +
                            $"Details:\n{cleanupDetails}",
                            "Cleanup Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        LogMessage($"Cleanup completed successfully! Freed {cleanupResult.SpaceFreedMB:N0} MB");
                    }
                    else
                    {
                        LogMessage($"Cleanup completed with issues: {cleanupResult.Message}");
                        MessageBox.Show(
                            $"Cleanup completed with some issues:\n\n{cleanupResult.Message}",
                            "Cleanup Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Cleanup failed: {ex.Message}");
                    MessageBox.Show(
                        $"Cleanup failed: {ex.Message}",
                        "Cleanup Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    SetUIState(true, "Ready");
                }
            }
        }

        private async void BtnQuickClean_Click(object sender, EventArgs e)
        {
            try
            {
                SetUIState(false, "Quick cleanup in progress...");
                LogMessage("Starting quick cleanup...");

                long totalFreed = 0;
                string tempPath = Path.GetTempPath();

                var tempFiles = Directory.GetFiles(tempPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => DateTime.Now - File.GetLastWriteTime(f) > TimeSpan.FromHours(1));

                foreach (string file in tempFiles)
                {
                    try
                    {
                        totalFreed += new FileInfo(file).Length;
                        File.Delete(file);
                    }
                    catch { }
                }

                long mbFreed = totalFreed / (1024 * 1024);
                MessageBox.Show($"Quick cleanup completed!\n\nSpace freed: {mbFreed:N0} MB",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LogMessage($"Quick cleanup freed {mbFreed:N0} MB");
            }
            catch (Exception ex)
            {
                LogMessage($"Quick cleanup failed: {ex.Message}");
                MessageBox.Show($"Quick cleanup failed: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true, "Ready");
            }
        }

        private async void BtnHealthCheck_Click(object sender, EventArgs e)
        {
            try
            {
                SetUIState(false, "Running comprehensive health check...");
                LogMessage("Starting PC health analysis...");

                var result = await healthChecker.PerformComprehensiveHealthCheck();

                if (result.Success)
                {
                    ShowHealthCheckResults(result);
                    LogMessage($"Health check completed - Overall score: {result.OverallHealthScore}/100 ({result.OverallStatus})");
                }
                else
                {
                    LogMessage($"Health check failed: {result.ErrorMessage}");
                    MessageBox.Show(
                        $"Health check failed: {result.ErrorMessage}",
                        "Health Check Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Health check error: {ex.Message}");
                MessageBox.Show(
                    $"Health check error: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetUIState(true, "Ready");
            }
        }

        private void ShowHealthCheckResults(HealthCheckResult result)
        {
            var healthForm = new Form
            {
                Text = "PC Health Report",
                Size = new Size(700, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(32, 32, 32),
                ForeColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            var textBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                Padding = new Padding(10)
            };

            var healthReport = $"PC HEALTH REPORT\n" +
                              $"================\n\n" +
                              $"Overall Score: {result.OverallHealthScore}/100 ({result.OverallStatus})\n\n" +
                              $"COMPONENT BREAKDOWN:\n" +
                              $"CPU: {result.CPUHealth.HealthScore}/100 - {result.CPUHealth.Status}\n" +
                              $"Memory: {result.MemoryHealth.HealthScore}/100 - {result.MemoryHealth.Status}\n" +
                              $"Storage: {result.StorageHealth.HealthScore}/100 - {result.StorageHealth.Status}\n" +
                              $"Graphics: {result.GraphicsHealth.HealthScore}/100 - {result.GraphicsHealth.Status}\n" +
                              $"Network: {result.NetworkHealth.HealthScore}/100 - {result.NetworkHealth.Status}\n" +
                              $"System: {result.SystemHealth.HealthScore}/100 - {result.SystemHealth.Status}\n" +
                              $"Performance: {result.PerformanceHealth.HealthScore}/100 - {result.PerformanceHealth.Status}\n" +
                              $"Security: {result.SecurityHealth.HealthScore}/100 - {result.SecurityHealth.Status}\n\n";

            if (result.Recommendations.Any())
            {
                healthReport += "RECOMMENDATIONS:\n";
                healthReport += "================\n";
                foreach (var rec in result.Recommendations)
                {
                    healthReport += $"• {rec}\n";
                }
            }

            textBox.Text = healthReport;
            healthForm.Controls.Add(textBox);
            healthForm.ShowDialog();
        }

        private async void BtnCreateBackup_Click(object sender, EventArgs e)
        {
            try
            {
                SetUIState(false, "Creating system backup...");
                LogMessage("Creating comprehensive system backup...");

                bool success = await optimizationEngine.CreateComprehensiveBackup();

                if (success)
                {
                    LogMessage("System backup created successfully.");
                    MessageBox.Show(
                        "System backup created successfully!\n\nBackup location: " +
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCOptimizer", "Backup"),
                        "Backup Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    LogMessage("Backup creation failed.");
                    MessageBox.Show("Backup creation failed.", "Backup Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private async void BtnTestPing_Click(object sender, EventArgs e)
        {
            SetUIState(false, "Testing ping...");
            await TestCurrentPing();
            LogMessage("Ping test completed.");
            SetUIState(true, "Ready");
        }

        private void SetUIState(bool enabled, string statusMessage)
        {
            // Enable/disable main buttons
            btnOptimizeForFPS.Enabled = enabled;
            btnOptimizeWindows.Enabled = enabled;
            btnOptimizeGraphics.Enabled = enabled;
            btnOptimizeNetwork.Enabled = enabled;
            btnCreateBackup.Enabled = enabled;
            btnRestoreBackup.Enabled = enabled;
            btnCheckStatus.Enabled = enabled;
            btnTestPing.Enabled = enabled;

            // Enable/disable maintenance buttons
            if (btnDiskCleanup != null) btnDiskCleanup.Enabled = enabled;
            if (btnQuickClean != null) btnQuickClean.Enabled = enabled;
            if (btnHealthCheck != null) btnHealthCheck.Enabled = enabled;

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
            this.ClientSize = new Size(1000, 800);
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

            // Ping Status Group
            groupPing = new GroupBox
            {
                Text = "Network Ping Status",
                ForeColor = Color.White,
                Location = new Point(520, 120),
                Size = new Size(230, 120)
            };

            labelCurrentPing = new Label
            {
                Text = "Testing ping...",
                Location = new Point(10, 25),
                Size = new Size(210, 20),
                ForeColor = Color.White
            };
            groupPing.Controls.Add(labelCurrentPing);

            labelPingStatus = new Label
            {
                Text = "Please wait...",
                Location = new Point(10, 50),
                Size = new Size(210, 20),
                ForeColor = Color.Yellow,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            groupPing.Controls.Add(labelPingStatus);

            btnTestPing = CreateStyledButton("Test Ping", new Point(10, 80), new Size(100, 25));
            btnTestPing.Click += BtnTestPing_Click;
            groupPing.Controls.Add(btnTestPing);
            this.Controls.Add(groupPing);

            // Optimization Status
            groupStatus = new GroupBox
            {
                Text = "Optimization Status",
                ForeColor = Color.White,
                Location = new Point(770, 120),
                Size = new Size(210, 120)
            };

            labelOptimizationStatus = new Label
            {
                Text = "Checking optimization status...",
                Location = new Point(10, 25),
                Size = new Size(190, 60),
                ForeColor = Color.Yellow
            };
            groupStatus.Controls.Add(labelOptimizationStatus);

            btnCheckStatus = CreateStyledButton("Refresh Status", new Point(10, 85), new Size(120, 25));
            btnCheckStatus.Click += BtnCheckStatus_Click;
            groupStatus.Controls.Add(btnCheckStatus);
            this.Controls.Add(groupStatus);

            // Main Optimizations
            groupOptimizations = new GroupBox
            {
                Text = "FPS & Ping Optimizations",
                ForeColor = Color.White,
                Location = new Point(20, 260),
                Size = new Size(960, 120)
            };

            // Main FPS optimization button
            btnOptimizeForFPS = new Button
            {
                Text = "🚀 OPTIMIZE FOR MAXIMUM FPS & LOW PING",
                Location = new Point(20, 30),
                Size = new Size(350, 50),
                BackColor = Color.FromArgb(0, 180, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnOptimizeForFPS.FlatAppearance.BorderSize = 0;
            btnOptimizeForFPS.Click += BtnOptimizeForFPS_Click;
            groupOptimizations.Controls.Add(btnOptimizeForFPS);

            // Individual optimization buttons
            btnOptimizeWindows = CreateStyledButton("Optimize Windows", new Point(400, 30));
            btnOptimizeWindows.Click += BtnOptimizeWindows_Click;
            groupOptimizations.Controls.Add(btnOptimizeWindows);

            btnOptimizeGraphics = CreateStyledButton("Optimize Graphics", new Point(580, 30));
            btnOptimizeGraphics.Click += BtnOptimizeGraphics_Click;
            groupOptimizations.Controls.Add(btnOptimizeGraphics);

            btnOptimizeNetwork = CreateStyledButton("Optimize Network", new Point(760, 30));
            btnOptimizeNetwork.Click += BtnOptimizeNetwork_Click;
            groupOptimizations.Controls.Add(btnOptimizeNetwork);

            this.Controls.Add(groupOptimizations);

            // System Maintenance & Health
            groupMaintenance = new GroupBox
            {
                Text = "System Maintenance & Health",
                ForeColor = Color.White,
                Location = new Point(20, 400),
                Size = new Size(480, 80)
            };

            btnDiskCleanup = CreateStyledButton("Deep Cleanup", new Point(20, 30), new Size(120, 35));
            btnDiskCleanup.BackColor = Color.FromArgb(150, 0, 150);
            btnDiskCleanup.Click += BtnDiskCleanup_Click;
            groupMaintenance.Controls.Add(btnDiskCleanup);

            btnQuickClean = CreateStyledButton("Quick Clean", new Point(160, 30), new Size(120, 35));
            btnQuickClean.BackColor = Color.FromArgb(255, 140, 0);
            btnQuickClean.Click += BtnQuickClean_Click;
            groupMaintenance.Controls.Add(btnQuickClean);

            btnHealthCheck = CreateStyledButton("Health Check", new Point(300, 30), new Size(120, 35));
            btnHealthCheck.BackColor = Color.FromArgb(0, 150, 100);
            btnHealthCheck.Click += BtnHealthCheck_Click;
            groupMaintenance.Controls.Add(btnHealthCheck);

            this.Controls.Add(groupMaintenance);

            // Backup and Restore
            groupBackup = new GroupBox
            {
                Text = "System Backup & Restore",
                ForeColor = Color.White,
                Location = new Point(520, 400),
                Size = new Size(460, 80)
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
                Location = new Point(20, 500),
                Size = new Size(960, 200)
            };

            logTextBox = new RichTextBox
            {
                Location = new Point(10, 25),
                Size = new Size(940, 160),
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
                Location = new Point(20, 720),
                Size = new Size(960, 30),
                Style = ProgressBarStyle.Blocks
            };
            this.Controls.Add(progressBarMain);

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
                Text = "Ready - PC Performance Optimizer Pro v1.2",
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

            AddTooltips();
            AddEventHandlers();
        }

        private void AddTooltips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnOptimizeForFPS,
                "Apply comprehensive FPS and ping optimizations including CPU, memory, graphics, and network tweaks");
            toolTip.SetToolTip(btnOptimizeWindows,
                "Optimize Windows settings for better gaming performance");
            toolTip.SetToolTip(btnOptimizeGraphics,
                "Optimize graphics drivers and GPU settings for maximum FPS");
            toolTip.SetToolTip(btnOptimizeNetwork,
                "Reduce network latency and optimize TCP settings for online gaming");
            toolTip.SetToolTip(btnDiskCleanup,
                "Comprehensive disk cleanup: temp files, browser cache, Windows logs, game cache, and more");
            toolTip.SetToolTip(btnQuickClean,
                "Quick cleanup of temporary files and immediate cache - takes under 30 seconds");
            toolTip.SetToolTip(btnHealthCheck,
                "Complete PC health analysis: CPU, memory, storage, graphics, network, and system status");
            toolTip.SetToolTip(btnCreateBackup,
                "Create a comprehensive system backup before applying optimizations");
            toolTip.SetToolTip(btnRestoreBackup,
                "Restore your system to the state before optimizations were applied");
            toolTip.SetToolTip(btnCheckStatus,
                "Check current optimization status and refresh display");
            toolTip.SetToolTip(btnTestPing,
                "Test your current ping to Google servers");
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

            button.MouseEnter += new EventHandler((s, e) => button.BackColor = Color.FromArgb(30, 140, 235));
            button.MouseLeave += new EventHandler((s, e) => button.BackColor = Color.FromArgb(0, 120, 215));

            return button;
        }

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
    }
}