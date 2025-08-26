// Program.cs - Complete updated version with licensing enforcement and admin check
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Management;
using System.Security.Principal;
using PCOptimizer.Security;

namespace PCOptimizer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Perform security checks first
            var securityResult = SecurityManager.PerformSecurityChecks();
            if (!securityResult.IsSafe)
            {
                Environment.Exit(0);
                return;
            }

            // Check for administrator privileges
            EnsureAdministrator();

            // Single instance check
            string mutexName = "PCOptimizerPro_SingleInstance_" + Environment.UserName;
            using (Mutex mutex = new Mutex(false, mutexName))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("PC Optimizer Pro is already running.", "Already Running",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Initialize application
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Set up unhandled exception handlers
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                try
                {
                    // CRITICAL: Check license before allowing app to run
                    if (!CheckInitialLicense())
                    {
                        // Show license form if no valid license
                        using (var licenseForm = new LicenseForm())
                        {
                            if (licenseForm.ShowDialog() != DialogResult.OK)
                            {
                                // User canceled licensing - exit
                                return;
                            }
                        }
                    }

                    // License validated - run main application
                    Application.Run(new MainForm());
                }
                catch (Exception ex)
                {
                    LogError(ex);
                    MessageBox.Show("An unexpected error occurred. Please restart the application.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static bool IsRunAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private static void EnsureAdministrator()
        {
            if (!IsRunAsAdministrator())
            {
                var result = MessageBox.Show(
                    "PC Optimizer Pro requires administrator privileges to apply system optimizations.\n\n" +
                    "Click OK to restart as administrator, or Cancel to run in limited mode.",
                    "Administrator Required",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.OK)
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            WorkingDirectory = Environment.CurrentDirectory,
                            FileName = Application.ExecutablePath,
                            Verb = "runas"
                        };
                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to restart as administrator: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Application.Exit();
                    }
                }
                else
                {
                    // Running in limited mode - show warning
                    MessageBox.Show(
                        "Running in limited mode. Some optimizations may not work properly without administrator privileges.",
                        "Limited Mode",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        private static bool CheckInitialLicense()
        {
            try
            {
                // Check for trial mode first
                if (IsTrialValid())
                {
                    return true;
                }

                // Check for valid license
                using (var licenseManager = new LicenseManager())
                {
                    var result = licenseManager.ValidateLicenseAsync().Result;
                    return result.IsValid;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool IsTrialValid()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\PCOptimizer"))
                {
                    if (key != null)
                    {
                        var trialMode = key.GetValue("TrialMode");
                        var trialExpiry = key.GetValue("TrialExpiry");

                        if (trialMode != null && (bool)trialMode && trialExpiry != null)
                        {
                            DateTime expiryDate = DateTime.FromBinary((long)trialExpiry);
                            return DateTime.Now < expiryDate;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogError(e.Exception);
            MessageBox.Show("An error occurred in the application. The error has been logged.",
                "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogError(ex);
            }
        }

        private static void LogError(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PCOptimizer", "Logs");

                Directory.CreateDirectory(logPath);

                string logFile = Path.Combine(logPath, $"error_{DateTime.Now:yyyyMMdd}.log");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.ToString()}{Environment.NewLine}";

                File.AppendAllText(logFile, logEntry);
            }
            catch (Exception)
            {
                // If we can't log the error, there's nothing more we can do
            }
        }
    }
}