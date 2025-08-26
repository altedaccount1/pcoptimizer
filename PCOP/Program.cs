// Program.cs - Fixed version with proper security checks and error handling
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Management;
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
                // Silently exit if security checks fail
                Environment.Exit(0);
                return;
            }

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
                    // Create main form and run
                    Application.Run(new MainForm());
                }
                catch (Exception ex)
                {
                    // Log error and show user-friendly message
                    LogError(ex);
                    MessageBox.Show("An unexpected error occurred. Please restart the application.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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