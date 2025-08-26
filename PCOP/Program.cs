using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PCOptimizer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Anti-debugging checks (obfuscate these in production)
            if (IsDebuggingDetected())
            {
                Environment.Exit(0);
                return;
            }

            // Check for virtual machines (basic detection)
            if (IsVirtualMachine())
            {
                MessageBox.Show("This software cannot run in virtual environments.",
                    "Environment Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }

            // Single instance check
            string mutexName = "PCOptimizerPro_SingleInstance";
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

        private static bool IsDebuggingDetected()
        {
            // Basic anti-debugging checks
            if (Debugger.IsAttached)
                return true;

            // Check for common debugging tools in process list
            Process[] processes = Process.GetProcesses();
            string[] debuggerProcesses = { "ollydbg", "ida", "x64dbg", "windbg", "processhacker" };

            foreach (Process process in processes)
            {
                foreach (string debugger in debuggerProcesses)
                {
                    if (process.ProcessName.ToLower().Contains(debugger))
                        return true;
                }
            }

            return false;
        }

        private static bool IsVirtualMachine()
        {
            try
            {
                // Basic VM detection - check for common VM artifacts
                string[] vmArtifacts = {
                    "VMware", "VirtualBox", "QEMU", "Xen", "Hyper-V"
                };

                // Check system manufacturer
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT Manufacturer FROM Win32_ComputerSystem"))
                {
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"].ToString().ToLower();
                        foreach (string vm in vmArtifacts)
                        {
                            if (manufacturer.Contains(vm.ToLower()))
                                return true;
                        }
                    }
                }

                // Check for VM-specific files
                string[] vmFiles = {
                    @"C:\windows\system32\drivers\vmmouse.sys",
                    @"C:\windows\system32\drivers\vmhgfs.sys",
                    @"C:\windows\system32\drivers\VBoxMouse.sys"
                };

                foreach (string file in vmFiles)
                {
                    if (File.Exists(file))
                        return true;
                }
            }
            catch (Exception) { }

            return false;
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
            catch (Exception) { }
        }
    }
}