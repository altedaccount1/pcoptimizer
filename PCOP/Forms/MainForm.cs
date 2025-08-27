// MainForm.cs - Modern UI with organized layout and rounded elements
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        // Modern UI Controls
        private Panel headerPanel, sidebarPanel, contentPanel, statusPanel;
        private Label titleLabel, subtitleLabel, welcomeLabel;
        private PictureBox logoBox;

        // Sidebar Navigation
        private Button btnDashboard, btnOptimizations, btnMaintenance, btnBackup, btnSettings;
        private Panel activeIndicator;

        // Content Panels
        private Panel dashboardPanel, optimizationsPanel, maintenancePanel, backupPanel, settingsPanel;

        // Dashboard Elements
        private Panel systemInfoCard, pingCard, statusCard, quickActionsCard;
        private Label labelSystemInfo, labelCurrentPing, labelPingStatus, labelOptimizationStatus;

        // Optimization Elements
        private Button btnOptimizeForFPS, btnOptimizeWindows, btnOptimizeGraphics, btnOptimizeNetwork;
        private Button btnTestPing, btnCheckStatus;

        // Maintenance Elements
        private Button btnDiskCleanup, btnHealthCheck, btnQuickClean;

        // Backup Elements
        private Button btnCreateBackup, btnRestoreBackup;

        // Status Elements
        private ProgressBar modernProgressBar;
        private RichTextBox logTextBox;
        private Label statusLabel;

        // Current active panel
        private string currentPanel = "dashboard";

        public MainForm()
        {
            InitializeComponent();
            Task.Run(async () => await InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            try
            {
                licenseManager = new LicenseManager();
                systemInfo = new SystemInfo();
                optimizationEngine = new OptimizationEngine();
                diskCleanupManager = new DiskCleanupManager();
                healthChecker = new SystemHealthChecker();

                statusUpdateTimer = new Timer();
                statusUpdateTimer.Interval = 5000;
                statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
                statusUpdateTimer.Start();

                await LoadSystemInformation();
                await CheckLicenseStatus();
                await UpdateOptimizationStatus();
                await TestCurrentPing();

                LogMessage("PC Optimizer initialized successfully.");
            }
            catch (Exception ex)
            {
                LogMessage($"Initialization error: {ex.Message}");
                MessageBox.Show("Failed to initialize the application properly.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Size = new Size(1200, 800);
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "PC Optimizer";
            this.Font = new Font("Segoe UI", 9F);

            CreateModernUI();
            SetupEventHandlers();

            this.ResumeLayout(false);
        }

        private void CreateModernUI()
        {
            CreateHeader();
            CreateSidebar();
            CreateContentArea();
            CreateStatusBar();
            CreateContentPanels();

            // Show dashboard by default
            ShowPanel("dashboard");
        }

        private void CreateHeader()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            // Add subtle gradient
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

            // Logo placeholder (you can add an actual logo image here)
            logoBox = new PictureBox
            {
                Location = new Point(20, 15),
                Size = new Size(50, 50),
                BackColor = Color.FromArgb(0, 120, 215),
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            logoBox.Paint += (s, e) =>
            {
                // Draw rounded logo background
                using (var path = CreateRoundedRectanglePath(logoBox.ClientRectangle, 12))
                using (var brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);

                    // Draw "PC" text
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

            // Window controls
            var closeBtn = CreateWindowButton("×", new Point(headerPanel.Width - 50, 15));
            closeBtn.Click += (s, e) => this.Close();
            closeBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerPanel.Controls.Add(closeBtn);

            var minimizeBtn = CreateWindowButton("−", new Point(headerPanel.Width - 100, 15));
            minimizeBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            minimizeBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            headerPanel.Controls.Add(minimizeBtn);

            this.Controls.Add(headerPanel);
        }

        private Button CreateWindowButton(string text, Point location)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(40, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 50);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 70, 70);
            return btn;
        }

        private void CreateSidebar()
        {
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            // Navigation buttons
            var navButtons = new[]
            {
                ("Dashboard", "dashboard", "📊"),
                ("Optimizations", "optimizations", "🚀"),
                ("Maintenance", "maintenance", "🔧"),
                ("Backup", "backup", "💾"),
                ("Settings", "settings", "⚙️")
            };

            int y = 20;
            foreach (var (text, id, icon) in navButtons)
            {
                var btn = CreateSidebarButton(text, icon, new Point(10, y), id);
                sidebarPanel.Controls.Add(btn);
                y += 60;

                // Store reference to buttons
                switch (id)
                {
                    case "dashboard": btnDashboard = btn; break;
                    case "optimizations": btnOptimizations = btn; break;
                    case "maintenance": btnMaintenance = btn; break;
                    case "backup": btnBackup = btn; break;
                    case "settings": btnSettings = btn; break;
                }
            }

            // Active indicator
            activeIndicator = new Panel
            {
                Width = 4,
                Height = 50,
                BackColor = Color.FromArgb(0, 120, 215),
                Location = new Point(0, 20)
            };
            sidebarPanel.Controls.Add(activeIndicator);

            this.Controls.Add(sidebarPanel);
        }

        private Button CreateSidebarButton(string text, string icon, Point location, string panelId)
        {
            var btn = new Button
            {
                Text = $"{icon}  {text}",
                Location = location,
                Size = new Size(180, 50),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand,
                Tag = panelId
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 35, 35);

            btn.Click += (s, e) => ShowPanel(panelId);

            return btn;
        }

        private void CreateContentArea()
        {
            contentPanel = new Panel
            {
                Location = new Point(200, 80), // Start after sidebar (200px) and header (80px)
                Size = new Size(1000, 680), // Width: 1200 - 200 = 1000, Height: 800 - 80 - 40 = 680
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(18, 18, 18),
                Padding = new Padding(30, 20, 30, 20)
            };
            this.Controls.Add(contentPanel);
        }

        private void CreateStatusBar()
        {
            statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            statusLabel = new Label
            {
                Text = "Ready",
                Location = new Point(20, 12),
                Size = new Size(200, 16),
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 9)
            };
            statusPanel.Controls.Add(statusLabel);

            modernProgressBar = new ProgressBar
            {
                Location = new Point(statusPanel.Width - 220, 10),
                Size = new Size(180, 20),
                Style = ProgressBarStyle.Blocks,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            statusPanel.Controls.Add(modernProgressBar);

            this.Controls.Add(statusPanel);
        }

        private void CreateContentPanels()
        {
            // Dashboard Panel
            dashboardPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            CreateDashboard();
            contentPanel.Controls.Add(dashboardPanel);

            // Optimizations Panel
            optimizationsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            CreateOptimizationsPanel();
            contentPanel.Controls.Add(optimizationsPanel);

            // Maintenance Panel
            maintenancePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            CreateMaintenancePanel();
            contentPanel.Controls.Add(maintenancePanel);

            // Backup Panel
            backupPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            CreateBackupPanel();
            contentPanel.Controls.Add(backupPanel);

            // Settings Panel
            settingsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Visible = false };
            CreateSettingsPanel();
            contentPanel.Controls.Add(settingsPanel);
        }

        private void CreateDashboard()
        {
            welcomeLabel = new Label
            {
                Text = "Welcome to PC Optimizer",
                Location = new Point(0, 0),
                Size = new Size(400, 35),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            dashboardPanel.Controls.Add(welcomeLabel);

            var subtitle = new Label
            {
                Text = "Optimize your PC for maximum gaming performance",
                Location = new Point(0, 40),
                Size = new Size(400, 25),
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.Transparent
            };
            dashboardPanel.Controls.Add(subtitle);

            // Create dashboard cards
            CreateDashboardCards();
        }

        private void CreateDashboardCards()
        {
            // System Info Card
            systemInfoCard = CreateModernCard("System Information", new Point(0, 100), new Size(320, 180));
            labelSystemInfo = new Label
            {
                Text = "Loading system information...",
                Location = new Point(20, 50),
                Size = new Size(280, 120),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };
            systemInfoCard.Controls.Add(labelSystemInfo);
            dashboardPanel.Controls.Add(systemInfoCard);

            // Ping Status Card
            pingCard = CreateModernCard("Network Status", new Point(340, 100), new Size(250, 180));
            labelCurrentPing = new Label
            {
                Text = "Testing ping...",
                Location = new Point(20, 50),
                Size = new Size(210, 25),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            pingCard.Controls.Add(labelCurrentPing);

            labelPingStatus = new Label
            {
                Text = "Please wait...",
                Location = new Point(20, 80),
                Size = new Size(210, 25),
                ForeColor = Color.FromArgb(255, 193, 7),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent
            };
            pingCard.Controls.Add(labelPingStatus);

            btnTestPing = CreateModernButton("Test Ping", new Point(20, 120), new Size(100, 35));
            btnTestPing.Click += BtnTestPing_Click;
            pingCard.Controls.Add(btnTestPing);
            dashboardPanel.Controls.Add(pingCard);

            // Quick Actions Card
            quickActionsCard = CreateModernCard("Quick Actions", new Point(610, 100), new Size(250, 340));

            var quickOptimizeBtn = CreateModernButton("🚀 Quick Optimize", new Point(20, 50), new Size(210, 45));
            quickOptimizeBtn.BackColor = Color.FromArgb(0, 180, 0);
            quickOptimizeBtn.Click += BtnOptimizeForFPS_Click;
            quickActionsCard.Controls.Add(quickOptimizeBtn);

            var quickCleanBtn = CreateModernButton("🧹 Quick Clean", new Point(20, 110), new Size(210, 45));
            quickCleanBtn.BackColor = Color.FromArgb(255, 140, 0);
            quickCleanBtn.Click += BtnQuickClean_Click;
            quickActionsCard.Controls.Add(quickCleanBtn);

            var healthCheckBtn = CreateModernButton("❤️ Health Check", new Point(20, 170), new Size(210, 45));
            healthCheckBtn.BackColor = Color.FromArgb(220, 53, 69);
            healthCheckBtn.Click += BtnHealthCheck_Click;
            quickActionsCard.Controls.Add(healthCheckBtn);

            var backupBtn = CreateModernButton("💾 Create Backup", new Point(20, 230), new Size(210, 45));
            backupBtn.BackColor = Color.FromArgb(108, 117, 125);
            backupBtn.Click += BtnCreateBackup_Click;
            quickActionsCard.Controls.Add(backupBtn);

            dashboardPanel.Controls.Add(quickActionsCard);

            // Status Card (full width below other cards)
            statusCard = CreateModernCard("Optimization Status", new Point(0, 300), new Size(860, 140));
            labelOptimizationStatus = new Label
            {
                Text = "Checking optimization status...",
                Location = new Point(20, 50),
                Size = new Size(820, 80),
                ForeColor = Color.FromArgb(255, 193, 7),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.Transparent
            };
            statusCard.Controls.Add(labelOptimizationStatus);
            dashboardPanel.Controls.Add(statusCard);
        }

        private void CreateOptimizationsPanel()
        {
            var title = new Label
            {
                Text = "Performance Optimizations",
                Location = new Point(0, 0),
                Size = new Size(400, 35),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            optimizationsPanel.Controls.Add(title);

            // Main FPS Optimization
            var mainOptCard = CreateModernCard("Ultimate FPS Optimization", new Point(0, 60), new Size(860, 120));
            var mainOptDesc = new Label
            {
                Text = "Apply comprehensive optimizations for maximum FPS and lowest ping. Includes CPU, Memory, Graphics, Network, and Windows tweaks.",
                Location = new Point(20, 50),
                Size = new Size(600, 40),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent
            };
            mainOptCard.Controls.Add(mainOptDesc);

            btnOptimizeForFPS = CreateModernButton("🚀 OPTIMIZE FOR MAXIMUM FPS", new Point(650, 35), new Size(180, 50));
            btnOptimizeForFPS.BackColor = Color.FromArgb(0, 180, 0);
            btnOptimizeForFPS.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnOptimizeForFPS.Click += BtnOptimizeForFPS_Click;
            mainOptCard.Controls.Add(btnOptimizeForFPS);
            optimizationsPanel.Controls.Add(mainOptCard);

            // Individual Optimizations
            var individualCard = CreateModernCard("Individual Optimizations", new Point(0, 200), new Size(860, 160));

            btnOptimizeWindows = CreateModernButton("Windows\nOptimization", new Point(30, 50), new Size(140, 70));
            btnOptimizeWindows.Click += BtnOptimizeWindows_Click;
            individualCard.Controls.Add(btnOptimizeWindows);

            btnOptimizeGraphics = CreateModernButton("Graphics\nOptimization", new Point(200, 50), new Size(140, 70));
            btnOptimizeGraphics.Click += BtnOptimizeGraphics_Click;
            individualCard.Controls.Add(btnOptimizeGraphics);

            btnOptimizeNetwork = CreateModernButton("Network\nOptimization", new Point(370, 50), new Size(140, 70));
            btnOptimizeNetwork.Click += BtnOptimizeNetwork_Click;
            individualCard.Controls.Add(btnOptimizeNetwork);

            btnCheckStatus = CreateModernButton("Check Status", new Point(540, 50), new Size(140, 70));
            btnCheckStatus.BackColor = Color.FromArgb(108, 117, 125);
            btnCheckStatus.Click += BtnCheckStatus_Click;
            individualCard.Controls.Add(btnCheckStatus);

            optimizationsPanel.Controls.Add(individualCard);

            // Activity Log
            var logCard = CreateModernCard("Activity Log", new Point(0, 380), new Size(860, 200));
            logTextBox = new RichTextBox
            {
                Location = new Point(20, 50),
                Size = new Size(820, 130),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            logCard.Controls.Add(logTextBox);
            optimizationsPanel.Controls.Add(logCard);
        }

        private void CreateMaintenancePanel()
        {
            var title = new Label
            {
                Text = "System Maintenance",
                Location = new Point(0, 0),
                Size = new Size(400, 35),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            maintenancePanel.Controls.Add(title);

            // Cleanup Section
            var cleanupCard = CreateModernCard("Disk Cleanup", new Point(0, 60), new Size(420, 250));

            var cleanupDesc = new Label
            {
                Text = "Free up disk space by removing temporary files, cache, and unnecessary data.",
                Location = new Point(20, 50),
                Size = new Size(380, 40),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent
            };
            cleanupCard.Controls.Add(cleanupDesc);

            btnDiskCleanup = CreateModernButton("🧹 Deep Cleanup", new Point(20, 110), new Size(170, 50));
            btnDiskCleanup.BackColor = Color.FromArgb(150, 0, 150);
            btnDiskCleanup.Click += BtnDiskCleanup_Click;
            cleanupCard.Controls.Add(btnDiskCleanup);

            btnQuickClean = CreateModernButton("⚡ Quick Clean", new Point(210, 110), new Size(170, 50));
            btnQuickClean.BackColor = Color.FromArgb(255, 140, 0);
            btnQuickClean.Click += BtnQuickClean_Click;
            cleanupCard.Controls.Add(btnQuickClean);

            maintenancePanel.Controls.Add(cleanupCard);

            // Health Check Section
            var healthCard = CreateModernCard("System Health", new Point(440, 60), new Size(420, 250));

            var healthDesc = new Label
            {
                Text = "Analyze your system's health including CPU, memory, storage, and network status.",
                Location = new Point(20, 50),
                Size = new Size(380, 40),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent
            };
            healthCard.Controls.Add(healthDesc);

            btnHealthCheck = CreateModernButton("❤️ Full Health Check", new Point(20, 110), new Size(170, 50));
            btnHealthCheck.BackColor = Color.FromArgb(220, 53, 69);
            btnHealthCheck.Click += BtnHealthCheck_Click;
            healthCard.Controls.Add(btnHealthCheck);

            maintenancePanel.Controls.Add(healthCard);
        }

        private void CreateBackupPanel()
        {
            var title = new Label
            {
                Text = "System Backup & Restore",
                Location = new Point(0, 0),
                Size = new Size(400, 35),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            backupPanel.Controls.Add(title);

            var backupCard = CreateModernCard("Backup Management", new Point(0, 60), new Size(860, 200));

            var backupDesc = new Label
            {
                Text = "Create system backups before applying optimizations and restore if needed.",
                Location = new Point(20, 50),
                Size = new Size(600, 30),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.Transparent
            };
            backupCard.Controls.Add(backupDesc);

            btnCreateBackup = CreateModernButton("💾 Create Backup", new Point(50, 100), new Size(200, 60));
            btnCreateBackup.BackColor = Color.FromArgb(40, 167, 69);
            btnCreateBackup.Click += BtnCreateBackup_Click;
            backupCard.Controls.Add(btnCreateBackup);

            btnRestoreBackup = CreateModernButton("🔄 Restore Backup", new Point(300, 100), new Size(200, 60));
            btnRestoreBackup.BackColor = Color.FromArgb(255, 193, 7);
            btnRestoreBackup.ForeColor = Color.Black;
            btnRestoreBackup.Click += BtnRestoreBackup_Click;
            backupCard.Controls.Add(btnRestoreBackup);

            backupPanel.Controls.Add(backupCard);
        }

        private void CreateSettingsPanel()
        {
            var title = new Label
            {
                Text = "Settings",
                Location = new Point(0, 0),
                Size = new Size(400, 35),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            settingsPanel.Controls.Add(title);

            var settingsCard = CreateModernCard("Application Settings", new Point(0, 60), new Size(860, 300));

            var aboutLabel = new Label
            {
                Text = "PC Optimizer v1.2\nGaming Performance Suite\n\nOptimize your PC for maximum gaming performance with advanced FPS and ping optimizations.",
                Location = new Point(20, 50),
                Size = new Size(400, 100),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent
            };
            settingsCard.Controls.Add(aboutLabel);

            settingsPanel.Controls.Add(settingsCard);
        }

        private Panel CreateModernCard(string title, Point location, Size size)
        {
            var card = new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.FromArgb(28, 28, 28)
            };

            // Add rounded corners and shadow effect
            card.Paint += (s, e) =>
            {
                var rect = card.ClientRectangle;
                using (var path = CreateRoundedRectanglePath(rect, 12))
                using (var brush = new SolidBrush(Color.FromArgb(28, 28, 28)))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);

                    // Add subtle border
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
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
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
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
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

                    // Draw text
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
                btn.BackColor = Color.FromArgb(0, 120, 215); // Reset to default if needed
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

        private void ShowPanel(string panelName)
        {
            // Hide all panels
            dashboardPanel.Visible = false;
            optimizationsPanel.Visible = false;
            maintenancePanel.Visible = false;
            backupPanel.Visible = false;
            settingsPanel.Visible = false;

            // Reset all button colors
            ResetSidebarButtons();

            // Show selected panel and update active indicator
            switch (panelName)
            {
                case "dashboard":
                    dashboardPanel.Visible = true;
                    btnDashboard.ForeColor = Color.White;
                    btnDashboard.BackColor = Color.FromArgb(45, 45, 45);
                    activeIndicator.Location = new Point(0, 20);
                    break;
                case "optimizations":
                    optimizationsPanel.Visible = true;
                    btnOptimizations.ForeColor = Color.White;
                    btnOptimizations.BackColor = Color.FromArgb(45, 45, 45);
                    activeIndicator.Location = new Point(0, 80);
                    break;
                case "maintenance":
                    maintenancePanel.Visible = true;
                    btnMaintenance.ForeColor = Color.White;
                    btnMaintenance.BackColor = Color.FromArgb(45, 45, 45);
                    activeIndicator.Location = new Point(0, 140);
                    break;
                case "backup":
                    backupPanel.Visible = true;
                    btnBackup.ForeColor = Color.White;
                    btnBackup.BackColor = Color.FromArgb(45, 45, 45);
                    activeIndicator.Location = new Point(0, 200);
                    break;
                case "settings":
                    settingsPanel.Visible = true;
                    btnSettings.ForeColor = Color.White;
                    btnSettings.BackColor = Color.FromArgb(45, 45, 45);
                    activeIndicator.Location = new Point(0, 260);
                    break;
            }

            currentPanel = panelName;
        }

        private void ResetSidebarButtons()
        {
            var buttons = new[] { btnDashboard, btnOptimizations, btnMaintenance, btnBackup, btnSettings };
            foreach (var btn in buttons)
            {
                if (btn != null)
                {
                    btn.ForeColor = Color.FromArgb(200, 200, 200);
                    btn.BackColor = Color.Transparent;
                }
            }
        }

        private void SetupEventHandlers()
        {
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;

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

        // Event handlers and methods from original implementation
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
                    LogMessage($"Valid license detected for {license.CustomerName}");
                    EnableOptimizationButtons(true);
                }
                else
                {
                    LogMessage("License validation required.");
                    EnableOptimizationButtons(false);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"License check error: {ex.Message}");
                EnableOptimizationButtons(true); // Allow usage if license server is down
            }
        }

        private void EnableOptimizationButtons(bool enabled)
        {
            if (btnOptimizeForFPS != null) btnOptimizeForFPS.Enabled = enabled;
            if (btnOptimizeWindows != null) btnOptimizeWindows.Enabled = enabled;
            if (btnOptimizeGraphics != null) btnOptimizeGraphics.Enabled = enabled;
            if (btnOptimizeNetwork != null) btnOptimizeNetwork.Enabled = enabled;
        }

        private async Task UpdateOptimizationStatus()
        {
            try
            {
                var status = await Task.Run(() => optimizationEngine.GetOptimizationStatus());

                string statusText = $"Overall Optimized: {(status.OverallOptimized ? "✅" : "❌")}\n" +
                                   $"CPU: {(status.CPUOptimized ? "✅" : "❌")} | " +
                                   $"Memory: {(status.MemoryOptimized ? "✅" : "❌")} | " +
                                   $"Graphics: {(status.GraphicsOptimized ? "✅" : "❌")}\n" +
                                   $"Network: {(status.NetworkOptimized ? "✅" : "❌")} | " +
                                   $"Services: {(status.ServicesOptimized ? "✅" : "❌")} | " +
                                   $"Power: {(status.PowerOptimized ? "✅" : "❌")}";

                labelOptimizationStatus.Text = statusText;
                labelOptimizationStatus.ForeColor = status.OverallOptimized ?
                    Color.FromArgb(40, 167, 69) : Color.FromArgb(255, 193, 7);
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
                labelPingStatus.ForeColor = Color.FromArgb(255, 193, 7);

                int ping = await optimizationEngine.TestCurrentPing();

                if (ping > 0)
                {
                    labelCurrentPing.Text = $"Ping: {ping}ms";

                    if (ping < 30)
                    {
                        labelPingStatus.Text = "🟢 Excellent";
                        labelPingStatus.ForeColor = Color.FromArgb(40, 167, 69);
                    }
                    else if (ping < 60)
                    {
                        labelPingStatus.Text = "🟡 Good";
                        labelPingStatus.ForeColor = Color.FromArgb(255, 193, 7);
                    }
                    else if (ping < 100)
                    {
                        labelPingStatus.Text = "🟠 Fair";
                        labelPingStatus.ForeColor = Color.FromArgb(255, 140, 0);
                    }
                    else
                    {
                        labelPingStatus.Text = "🔴 Poor";
                        labelPingStatus.ForeColor = Color.FromArgb(220, 53, 69);
                    }
                }
                else
                {
                    labelCurrentPing.Text = "Ping test failed";
                    labelPingStatus.Text = "❌ Unable to test";
                    labelPingStatus.ForeColor = Color.FromArgb(220, 53, 69);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Ping test error: {ex.Message}");
                labelCurrentPing.Text = "Ping test error";
                labelPingStatus.Text = "❌ Test failed";
                labelPingStatus.ForeColor = Color.FromArgb(220, 53, 69);
            }
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            Task.Run(async () => await UpdateOptimizationStatus());
        }

        private void SetUIState(bool enabled, string statusMessage)
        {
            // Update status
            statusLabel.Text = statusMessage;

            // Update progress bar
            if (enabled)
            {
                modernProgressBar.Style = ProgressBarStyle.Blocks;
                modernProgressBar.Value = 0;
            }
            else
            {
                modernProgressBar.Style = ProgressBarStyle.Marquee;
            }

            // Enable/disable main buttons
            if (btnOptimizeForFPS != null) btnOptimizeForFPS.Enabled = enabled;
            if (btnOptimizeWindows != null) btnOptimizeWindows.Enabled = enabled;
            if (btnOptimizeGraphics != null) btnOptimizeGraphics.Enabled = enabled;
            if (btnOptimizeNetwork != null) btnOptimizeNetwork.Enabled = enabled;
            if (btnCreateBackup != null) btnCreateBackup.Enabled = enabled;
            if (btnRestoreBackup != null) btnRestoreBackup.Enabled = enabled;
            if (btnCheckStatus != null) btnCheckStatus.Enabled = enabled;
            if (btnTestPing != null) btnTestPing.Enabled = enabled;
            if (btnDiskCleanup != null) btnDiskCleanup.Enabled = enabled;
            if (btnQuickClean != null) btnQuickClean.Enabled = enabled;
            if (btnHealthCheck != null) btnHealthCheck.Enabled = enabled;
        }

        private void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(LogMessage), message);
                return;
            }

            if (logTextBox != null)
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                logTextBox.AppendText($"[{timestamp}] {message}\n");
                logTextBox.ScrollToCaret();
            }
        }

        // Event handlers for buttons
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            LogMessage("PC Optimizer started successfully.");
            LogMessage("System analysis in progress...");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit PC Optimizer?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}