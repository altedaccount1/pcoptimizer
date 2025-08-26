using System;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Threading;

namespace PCOptimizer
{
    public class OptimizationEngine
    {
        private string backupPath;
        private SystemInfo systemInfo;

        public OptimizationEngine()
        {
            backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCOptimizer", "Backup");
            Directory.CreateDirectory(backupPath);
            systemInfo = new SystemInfo();
        }

        public void OptimizeWindowsPerformance()
        {
            try
            {
                // Create backup first
                CreateSystemBackup();

                // Registry optimizations (obfuscated in real version)
                OptimizeRegistry();

                // Service optimizations
                OptimizeServices();

                // Power plan optimization
                SetHighPerformancePowerPlan();

                // Visual effects optimization
                OptimizeVisualEffects();

                // Memory optimization
                OptimizeMemoryManagement();

                Thread.Sleep(2000); // Simulate work
            }
            catch (Exception ex)
            {
                throw new Exception($"Windows optimization failed: {ex.Message}");
            }
        }

        public void OptimizeGraphicsSettings()
        {
            try
            {
                // NVIDIA optimizations
                if (systemInfo.IsNVIDIAGraphics())
                {
                    OptimizeNVIDIA();
                }

                // AMD optimizations  
                if (systemInfo.IsAMDGraphics())
                {
                    OptimizeAMD();
                }

                // Display optimizations
                OptimizeDisplaySettings();

                // Game Mode optimization
                EnableGameMode();

                Thread.Sleep(1500); // Simulate work
            }
            catch (Exception ex)
            {
                throw new Exception($"Graphics optimization failed: {ex.Message}");
            }
        }

        public void ApplyGameTweaks()
        {
            try
            {
                // Game-specific optimizations
                ApplyGeneralGameTweaks();

                // Network optimizations for gaming
                OptimizeNetworkForGaming();

                // Mouse/input optimizations
                OptimizeInputSettings();

                // CPU priority optimizations
                OptimizeCPUScheduling();

                Thread.Sleep(1000); // Simulate work
            }
            catch (Exception ex)
            {
                throw new Exception($"Game tweaks failed: {ex.Message}");
            }
        }

        // Registry optimization methods (these would be obfuscated)
        private void OptimizeRegistry()
        {
            // Disable Windows Update automatic restart
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1);

            // Disable game mode notifications
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "ShowStartupPanel", 0);

            // Optimize network settings
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 1);
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay", 1);

            // Disable telemetry
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);

            // Optimize CPU scheduling
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 38);

            // Disable background apps
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1);
        }

        private void OptimizeServices()
        {
            string[] servicesToDisable = {
                "Fax", "WSearch", "Spooler", "Themes", "TabletInputService",
                "WbioSrvc", "WMPNetworkSvc", "XblAuthManager", "XblGameSave",
                "XboxNetApiSvc", "XboxGipSvc", "MapsBroker"
            };

            foreach (string serviceName in servicesToDisable)
            {
                try
                {
                    ServiceController service = new ServiceController(serviceName);
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }

                    // Set startup type to disabled
                    SetServiceStartupType(serviceName, "Disabled");
                }
                catch (Exception) { } // Continue if service doesn't exist
            }
        }

        private void SetServiceStartupType(string serviceName, string startupType)
        {
            try
            {
                string keyPath = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\{serviceName}";
                int startValue;
                switch (startupType.ToLower())
                {
                    case "disabled":
                        startValue = 4;
                        break;
                    case "manual":
                        startValue = 3;
                        break;
                    case "automatic":
                        startValue = 2;
                        break;
                    default:
                        startValue = 3;
                        break;
                }
                SetRegistryValue(keyPath, "Start", startValue);
            }
            catch (Exception) { }
        }

        private void SetHighPerformancePowerPlan()
        {
            try
            {
                // Set high performance power plan
                Process.Start("powercfg", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c")?.WaitForExit();

                // Disable USB selective suspend
                Process.Start("powercfg", "/setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0")?.WaitForExit();
                Process.Start("powercfg", "/setdcvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0")?.WaitForExit();

                // Set CPU to 100%
                Process.Start("powercfg", "/setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc5038f7-23e0-4960-96da-33abaf5935ec 100")?.WaitForExit();
            }
            catch (Exception) { }
        }

        private void OptimizeVisualEffects()
        {
            // Disable visual effects for performance
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2);

            // Disable animations
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", "0");

            // Disable menu show delay
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", "0");
        }

        private void OptimizeMemoryManagement()
        {
            // Clear standby memory
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "ClearPageFileAtShutdown", 0);

            // Optimize paging file
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 1);

            // Large system cache
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 1);
        }

        private void OptimizeNVIDIA()
        {
            // NVIDIA Control Panel optimizations via registry
            string nvidiaKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers";

            // Disable preemption
            SetRegistryValue(nvidiaKey, "TdrLevel", 0);
            SetRegistryValue(nvidiaKey, "TdrDelay", 60);

            // NVIDIA-specific optimizations would go here
            // These would be much more comprehensive in the real version
        }

        private void OptimizeAMD()
        {
            // AMD-specific optimizations
            // Registry tweaks for AMD graphics settings
            try
            {
                string amdKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";

                // AMD-specific registry optimizations would go here
                // Example: SetRegistryValue(amdKey, "SomeAMDSetting", 1);
            }
            catch (Exception) { }
        }

        private void OptimizeDisplaySettings()
        {
            // Disable fullscreen optimizations
            SetRegistryValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 0);
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);
        }

        private void EnableGameMode()
        {
            // Enable Windows Game Mode
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "AutoGameModeEnabled", 1);
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "AllowAutoGameMode", 1);
        }

        private void ApplyGeneralGameTweaks()
        {
            // Disable Game DVR
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);

            // Optimize mouse settings
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0");
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "0");
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "0");
        }

        private void OptimizeNetworkForGaming()
        {
            // Network adapter optimizations
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay", 1);
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 1);

            // Disable Nagle algorithm
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", "TcpAckFrequency", 1);
        }

        private void OptimizeInputSettings()
        {
            // Mouse acceleration disable
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0");

            // Keyboard repeat rate optimization
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", "0");
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", "31");
        }

        private void OptimizeCPUScheduling()
        {
            // Optimize for programs (not background services)
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 38);
        }

        private void SetRegistryValue(string keyPath, string valueName, object value)
        {
            try
            {
                string[] parts = keyPath.Split('\\');
                RegistryKey baseKey = GetRegistryHive(parts[0]);
                string subKeyPath = string.Join("\\", parts, 1, parts.Length - 1);

                using (RegistryKey key = baseKey.CreateSubKey(subKeyPath))
                {
                    key?.SetValue(valueName, value);
                }
            }
            catch (Exception) { }
        }

        private RegistryKey GetRegistryHive(string hive)
        {
            switch (hive.ToUpper())
            {
                case "HKEY_LOCAL_MACHINE":
                    return Registry.LocalMachine;
                case "HKEY_CURRENT_USER":
                    return Registry.CurrentUser;
                case "HKEY_CLASSES_ROOT":
                    return Registry.ClassesRoot;
                default:
                    return Registry.CurrentUser;
            }
        }

        public void CreateSystemBackup()
        {
            try
            {
                // Create registry backup
                string backupFile = Path.Combine(backupPath, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg");

                // Export important registry keys
                Process.Start("reg", $"export \"HKLM\\SOFTWARE\\Policies\" \"{backupFile}_policies.reg\"")?.WaitForExit();
                Process.Start("reg", $"export \"HKCU\\SOFTWARE\\Microsoft\\GameBar\" \"{backupFile}_gamebar.reg\"")?.WaitForExit();

                // Create restore point
                CreateRestorePoint();

                // Save service states
                SaveServiceStates();
            }
            catch (Exception ex)
            {
                throw new Exception($"Backup creation failed: {ex.Message}");
            }
        }

        private void CreateRestorePoint()
        {
            try
            {
                // Create system restore point
                Process.Start("powershell", "-Command \"Checkpoint-Computer -Description 'PC Optimizer Backup' -RestorePointType 'MODIFY_SETTINGS'\"")?.WaitForExit();
            }
            catch (Exception) { }
        }

        private void SaveServiceStates()
        {
            try
            {
                string serviceBackupFile = Path.Combine(backupPath, $"services_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                ServiceController[] services = ServiceController.GetServices();
                using (StreamWriter writer = new StreamWriter(serviceBackupFile))
                {
                    foreach (ServiceController service in services)
                    {
                        try
                        {
                            writer.WriteLine($"{service.ServiceName}|{service.Status}|{service.StartType}");
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
        }

        public void RestoreSystemBackup()
        {
            try
            {
                // Find latest backup files
                string[] backupFiles = Directory.GetFiles(backupPath, "backup_*.reg");
                if (backupFiles.Length == 0)
                {
                    throw new Exception("No backup files found.");
                }

                Array.Sort(backupFiles);

                // Restore registry files
                foreach (string backupFile in backupFiles)
                {
                    if (backupFile.EndsWith(".reg"))
                    {
                        Process.Start("reg", $"import \"{backupFile}\"")?.WaitForExit();
                    }
                }

                // Restore services
                RestoreServiceStates();
            }
            catch (Exception ex)
            {
                throw new Exception($"Backup restore failed: {ex.Message}");
            }
        }

        private void RestoreServiceStates()
        {
            try
            {
                string[] serviceFiles = Directory.GetFiles(backupPath, "services_*.txt");
                if (serviceFiles.Length == 0) return;

                Array.Sort(serviceFiles);
                string latestServiceFile = serviceFiles[serviceFiles.Length - 1];

                string[] lines = File.ReadAllLines(latestServiceFile);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 3)
                    {
                        string serviceName = parts[0];
                        string status = parts[1];
                        string startType = parts[2];

                        // Restore service start type
                        SetServiceStartupType(serviceName, startType);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}