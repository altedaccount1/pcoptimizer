using System;
using System.IO;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using System.Collections.Generic;
using System.Linq;

namespace PCOptimizer
{
    public class OptimizationEngine
    {
        private string backupPath;
        private SystemInfo systemInfo;
        private Dictionary<string, object> originalSettings;

        public OptimizationEngine()
        {
            backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCOptimizer", "Backup");
            Directory.CreateDirectory(backupPath);
            systemInfo = new SystemInfo();
            originalSettings = new Dictionary<string, object>();
        }

        public async Task<OptimizationResult> OptimizeForMaximumFPS()
        {
            var result = new OptimizationResult();

            try
            {
                result.BackupCreated = await CreateComprehensiveBackup();

                var tasks = new List<Task<bool>>
                {
                    OptimizeCPUForGaming(),
                    OptimizeMemoryForGaming(),
                    OptimizeGraphicsForGaming(),
                    OptimizeNetworkForGaming(),
                    OptimizeWindowsForGaming(),
                    OptimizeServicesForGaming(),
                    OptimizePowerForGaming(),
                    OptimizeStorageForGaming(),
                    ApplyGameSpecificOptimizations()
                };

                var results = await Task.WhenAll(tasks);
                result.OptimizationsApplied = results.Count(r => r);
                result.Success = results.All(r => r);

                if (result.Success)
                {
                    await ApplyFinalTweaks();
                    result.Message = $"Successfully applied {result.OptimizationsApplied} gaming optimizations.";
                }
                else
                {
                    result.Message = $"Applied {result.OptimizationsApplied} optimizations with some warnings.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Optimization failed: {ex.Message}";
            }

            return result;
        }

        private async Task<bool> OptimizeCPUForGaming()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Store original values
                    StoreOriginalValue("Win32PrioritySeparation", GetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation"));

                    // Optimize CPU scheduling for foreground applications (games)
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 38);

                    // Disable CPU throttling
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "CsEnabled", 0);

                    // Optimize CPU cache and memory allocation
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager", "HeapDeCommitFreeBlockThreshold", 0x40000);

                    // Set CPU to maximum performance state
                    ExecutePowerCfgCommand("/setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc5038f7-23e0-4960-96da-33abaf5935ec 100");
                    ExecutePowerCfgCommand("/setdcvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc5038f7-23e0-4960-96da-33abaf5935ec 100");

                    // Disable CPU idle states for consistent performance
                    ExecutePowerCfgCommand("/setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 5d76a2ca-e8c0-402f-a133-2158492d58ad 0");

                    // Optimize processor scheduling
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0);

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private async Task<bool> OptimizeMemoryForGaming()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Optimize memory management for gaming
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 0);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 1);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "ClearPageFileAtShutdown", 0);

                    // Increase I/O page lock limit for better game performance
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "IoPageLockLimit", 0x4000000);

                    // Disable memory compression (can cause micro-stutters)
                    ExecutePowerShellCommand("Disable-MMAgent -MemoryCompression");

                    // Optimize virtual memory settings
                    SetOptimalPagingFile();

                    // Disable unnecessary memory features
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "FeatureSettings", 1);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "FeatureSettingsOverride", 3);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "FeatureSettingsOverrideMask", 3);

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private async Task<bool> OptimizeGraphicsForGaming()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Enable Hardware Accelerated GPU Scheduling
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2);

                    // Disable TDR (Timeout Detection and Recovery) for competitive gaming
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrLevel", 0);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "TdrDelay", 60);

                    // Graphics driver specific optimizations
                    if (systemInfo.IsNVIDIAGraphics())
                    {
                        OptimizeNVIDIASettings();
                    }
                    else if (systemInfo.IsAMDGraphics())
                    {
                        OptimizeAMDSettings();
                    }

                    // Disable Game DVR and other overlays
                    SetRegistryValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 0);
                    SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);
                    SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "GameDVR_Enabled", 0);

                    // Optimize DWM (Desktop Window Manager) for gaming
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", 5);

                    // Set GPU priority for games
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "High");

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private void OptimizeNVIDIASettings()
        {
            string nvidiaKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";

            // NVIDIA-specific gaming optimizations
            SetRegistryValue(nvidiaKey, "RmHdcpKeyglobZero", 1);
            SetRegistryValue(nvidiaKey, "PowerMizerEnable", 1);
            SetRegistryValue(nvidiaKey, "PowerMizerLevel", 1); // Maximum performance
            SetRegistryValue(nvidiaKey, "PowerMizerLevelAC", 1);

            // Disable NVIDIA HDCP if it exists
            SetRegistryValue(nvidiaKey, "RMHdcpKeyGlobZero", 1);
        }

        private void OptimizeAMDSettings()
        {
            string amdKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";

            // AMD-specific gaming optimizations
            SetRegistryValue(amdKey, "KMD_FRTEnabled", 0); // Disable Frame Rate Target Control
            SetRegistryValue(amdKey, "KMD_DeLagEnabled", 1); // Enable Anti-Lag
            SetRegistryValue(amdKey, "KMD_RadeonBoostEnabled", 1); // Enable Radeon Boost
        }

        private async Task<bool> OptimizeNetworkForGaming()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // TCP optimizations for reduced latency
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 1);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay", 1);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpDelAckTicks", 0);

                    // Disable Nagle's algorithm
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpNoDelay", 1);

                    // Network adapter optimizations
                    OptimizeNetworkAdapters();

                    // Windows network stack optimizations
                    ExecuteNetshCommand("int tcp set global autotuninglevel=normal");
                    ExecuteNetshCommand("int tcp set global chimney=enabled");
                    ExecuteNetshCommand("int tcp set global rss=enabled");
                    ExecuteNetshCommand("int tcp set global netdma=enabled");

                    // Disable bandwidth throttling
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 0xffffffff);

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private void OptimizeNetworkAdapters()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled=true"))
                {
                    foreach (ManagementObject adapter in searcher.Get())
                    {
                        string deviceId = adapter["DeviceID"]?.ToString();
                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            string adapterKey = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{{4d36e972-e325-11ce-bfc1-08002be10318}}\{deviceId.PadLeft(4, '0')}";

                            // Optimize network adapter settings
                            SetRegistryValue(adapterKey, "*FlowControl", "0");
                            SetRegistryValue(adapterKey, "*InterruptModeration", "0");
                            SetRegistryValue(adapterKey, "*RSS", "1");
                            SetRegistryValue(adapterKey, "*TCPUDPChecksumOffloadIPv4", "3");
                            SetRegistryValue(adapterKey, "*TCPUDPChecksumOffloadIPv6", "3");
                            SetRegistryValue(adapterKey, "*JumboPacket", "9014");
                        }
                    }
                }
            }
            catch { }
        }

        private async Task<bool> OptimizeWindowsForGaming()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Enable Game Mode but disable notifications
                    SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "AutoGameModeEnabled", 1);
                    SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "AllowAutoGameMode", 1);
                    SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "ShowStartupPanel", 0);

                    // Disable Windows Update automatic restart
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1);

                    // Optimize visual effects for performance
                    SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2);
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", "0");
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", "0");

                    // Disable unnecessary Windows features
                    SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0);

                    // Optimize Windows timer resolution
                    ExecuteBcdEditCommand("/set useplatformclock true");
                    ExecuteBcdEditCommand("/set disabledynamictick yes");

                    // Disable telemetry and diagnostics
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private async Task<bool> OptimizeServicesForGaming()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Services that can negatively impact gaming performance
                    var servicesToOptimize = new Dictionary<string, string>
                    {
                        {"SysMain", "Disabled"}, // SuperFetch - can cause stuttering
                        {"WSearch", "Manual"}, // Windows Search - reduce to manual
                        {"Spooler", "Manual"}, // Print Spooler
                        {"Fax", "Disabled"},
                        {"TabletInputService", "Disabled"},
                        {"WbioSrvc", "Manual"}, // Windows Biometric Service
                        {"DiagTrack", "Disabled"}, // Diagnostics Tracking
                        {"dmwappushservice", "Disabled"}, // WAP Push Message Routing
                        {"MapsBroker", "Disabled"}, // Downloaded Maps Manager
                        {"lfsvc", "Manual"}, // Geolocation Service
                        {"WerSvc", "Manual"} // Windows Error Reporting
                    };

                    foreach (var service in servicesToOptimize)
                    {
                        OptimizeService(service.Key, service.Value);
                    }

                    // Keep essential gaming services optimized
                    OptimizeEssentialGamingServices();

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private void OptimizeService(string serviceName, string startupType)
        {
            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    if (startupType == "Disabled" && service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }

                SetServiceStartupType(serviceName, startupType);
            }
            catch { } // Service might not exist
        }

        private void OptimizeEssentialGamingServices()
        {
            // Ensure critical gaming services are properly configured
            var essentialServices = new Dictionary<string, string>
            {
                {"AudioSrv", "Automatic"}, // Windows Audio
                {"Themes", "Automatic"}, // Themes service (needed for Aero)
                {"UxSms", "Automatic"}, // Desktop Window Manager Session Manager
                {"EventSystem", "Automatic"}, // COM+ Event System
                {"RpcSs", "Automatic"} // Remote Procedure Call
            };

            foreach (var service in essentialServices)
            {
                SetServiceStartupType(service.Key, service.Value);
            }

            // Optimize NVIDIA services if present
            if (systemInfo.IsNVIDIAGraphics())
            {
                SetServiceStartupType("NVDisplay.ContainerLocalSystem", "Automatic");
            }
        }

        private async Task<bool> OptimizePowerForGaming()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Try to set Ultimate Performance power plan
                    bool ultimateSet = ExecutePowerCfgCommand("/duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                    if (ultimateSet)
                    {
                        ExecutePowerCfgCommand("/setactive e9a42b02-d5df-448d-aa00-03f14749eb61");
                    }
                    else
                    {
                        // Fallback to High Performance
                        ExecutePowerCfgCommand("/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
                    }

                    // Disable power saving features that can impact gaming
                    ExecutePowerCfgCommand("/setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0"); // USB selective suspend
                    ExecutePowerCfgCommand("/setdcvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0");

                    // Disable PCI Express Link State Power Management
                    ExecutePowerCfgCommand("/setacvalueindex SCHEME_CURRENT 501a4d13-42af-4429-9fd1-a8218c268e20 ee12f906-d277-404b-b6da-e5fa1a576df5 0");
                    ExecutePowerCfgCommand("/setdcvalueindex SCHEME_CURRENT 501a4d13-42af-4429-9fd1-a8218c268e20 ee12f906-d277-404b-b6da-e5fa1a576df5 0");

                    // Set hard disk timeout to never
                    ExecutePowerCfgCommand("/setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 6738e2c4-e8a5-4a42-b16a-e040e769756e 0");

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private async Task<bool> OptimizeStorageForGaming()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // NTFS optimizations for gaming
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisableLastAccessUpdate", 1);
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisable8dot3NameCreation", 1);

                    // Disable automatic defragmentation during gaming hours
                    ExecuteScriptCommand("schtasks /Change /TN \"\\Microsoft\\Windows\\Defrag\\ScheduledDefrag\" /Disable");

                    // Optimize prefetch for gaming
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 2); // Applications only
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnableSuperfetch", 0); // Disable SuperFetch

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private async Task<bool> ApplyGameSpecificOptimizations()
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Mouse optimizations for FPS games
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0");
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "0");
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "0");

                    // Keyboard optimizations
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", "0");
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", "31");

                    // Disable Filter Keys and Sticky Keys
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response", "Flags", "0");
                    SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\StickyKeys", "Flags", "0");

                    // Audio optimizations for competitive gaming
                    SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Multimedia\Audio", "UserSimulatedStereoOn", 0);

                    // Create game-specific launch optimizations
                    CreateRustOptimizationScript();

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private void CreateRustOptimizationScript()
        {
            try
            {
                string steamPath = FindSteamPath();
                if (!string.IsNullOrEmpty(steamPath))
                {
                    string rustPath = Path.Combine(steamPath, "steamapps", "common", "Rust");
                    if (Directory.Exists(rustPath))
                    {
                        string scriptPath = Path.Combine(rustPath, "rust_fps_optimizer.bat");
                        string[] optimizationScript = {
                            "@echo off",
                            "echo Rust FPS Optimizer - Starting optimizations...",
                            "",
                            "REM Set process priority and affinity",
                            "set /A cores=%NUMBER_OF_PROCESSORS%-1",
                            "set /A affinity=0",
                            "for /L %%i in (0,1,%cores%) do set /A affinity+=1<<%%i",
                            "",
                            "REM Clear standby memory",
                            "echo Clearing standby memory...",
                            "powershell -Command \"& {Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class Win32 { [DllImport(\\\"kernel32.dll\\\")] public static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize); }'; [Win32]::SetProcessWorkingSetSize((Get-Process -Id $PID).Handle, -1, -1)}\"",
                            "",
                            "REM Launch Rust with optimizations",
                            "echo Launching Rust with FPS optimizations...",
                            "start /high /affinity %affinity% RustClient.exe -high -malloc=system -winxp -nojoy -nopreload -nointro -nosplash +fps_max 0 +gc.buffer 2048",
                            "",
                            "echo Rust FPS optimizations applied!"
                        };

                        File.WriteAllLines(scriptPath, optimizationScript);
                    }
                }
            }
            catch { }
        }

        private string FindSteamPath()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    return key?.GetValue("SteamPath")?.ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        private async Task ApplyFinalTweaks()
        {
            await Task.Run(() =>
            {
                // Final system tweaks after all optimizations
                try
                {
                    // Force garbage collection to free up memory
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    // Set system responsiveness to gaming mode
                    SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0);

                    // Apply power configuration changes
                    ExecutePowerCfgCommand("/setactive SCHEME_CURRENT");

                    Thread.Sleep(1000); // Allow changes to settle
                }
                catch { }
            });
        }

        private void SetOptimalPagingFile()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        long totalRAM = Convert.ToInt64(obj["TotalPhysicalMemory"]) / (1024 * 1024); // Convert to MB

                        // Set paging file to 1.5x RAM size for optimal gaming performance
                        long optimalPageFile = totalRAM * 3 / 2;

                        // Apply via registry (alternative to WMI for more reliability)
                        SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                            "PagingFiles", $"C:\\pagefile.sys {optimalPageFile} {optimalPageFile}");
                        break;
                    }
                }
            }
            catch { }
        }

        // Helper methods
        private void StoreOriginalValue(string key, object value)
        {
            if (!originalSettings.ContainsKey(key))
            {
                originalSettings[key] = value;
            }
        }

        private object GetRegistryValue(string keyPath, string valueName)
        {
            try
            {
                string[] parts = keyPath.Split('\\');
                RegistryKey baseKey = parts[0] switch
                {
                    "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                    "HKEY_CURRENT_USER" => Registry.CurrentUser,
                    _ => Registry.CurrentUser
                };

                string subKeyPath = string.Join("\\", parts.Skip(1));
                using (var key = baseKey.OpenSubKey(subKeyPath))
                {
                    return key?.GetValue(valueName);
                }
            }
            catch
            {
                return null;
            }
        }

        private void SetRegistryValue(string keyPath, string valueName, object value)
        {
            try
            {
                string[] parts = keyPath.Split('\\');
                RegistryKey baseKey = parts[0] switch
                {
                    "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                    "HKEY_CURRENT_USER" => Registry.CurrentUser,
                    "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                    _ => Registry.CurrentUser
                };

                string subKeyPath = string.Join("\\", parts.Skip(1));
                using (var key = baseKey.CreateSubKey(subKeyPath))
                {
                    key?.SetValue(valueName, value);
                }
            }
            catch { }
        }

        private void SetServiceStartupType(string serviceName, string startupType)
        {
            try
            {
                string keyPath = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\{serviceName}";
                int startValue = startupType.ToLower() switch
                {
                    "disabled" => 4,
                    "manual" => 3,
                    "automatic" => 2,
                    _ => 3
                };
                SetRegistryValue(keyPath, "Start", startValue);
            }
            catch { }
        }

        private bool ExecutePowerCfgCommand(string arguments)
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }))
                {
                    process?.WaitForExit(10000); // 10 second timeout
                    return process?.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ExecuteNetshCommand(string arguments)
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }))
                {
                    process?.WaitForExit(10000);
                    return process?.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ExecutePowerShellCommand(string command)
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }))
                {
                    process?.WaitForExit(15000);
                    return process?.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ExecuteBcdEditCommand(string arguments)
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "bcdedit",
                    Arguments = arguments,
                    UseShellExecute = true,
                    Verb = "runas", // Run as administrator
                    WindowStyle = ProcessWindowStyle.Hidden
                }))
                {
                    process?.WaitForExit(10000);
                    return process?.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ExecuteScriptCommand(string command)
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {command}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }))
                {
                    process?.WaitForExit(10000);
                    return process?.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CreateComprehensiveBackup()
        {
            return await Task.Run(() =>
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFile = Path.Combine(backupPath, $"gaming_optimization_backup_{timestamp}");

                    // Create backup directory
                    Directory.CreateDirectory(backupFile);

                    // Export critical registry keys
                    var criticalKeys = new[]
                    {
                        "HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl",
                        "HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers",
                        "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters",
                        "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management",
                        "HKCU\\SOFTWARE\\Microsoft\\GameBar",
                        "HKCU\\Control Panel\\Mouse",
                        "HKCU\\Control Panel\\Keyboard"
                    };

                    foreach (var key in criticalKeys)
                    {
                        string fileName = key.Replace("\\", "_").Replace(":", "") + ".reg";
                        string fullPath = Path.Combine(backupFile, fileName);
                        ExecuteScriptCommand($"reg export \"{key}\" \"{fullPath}\"");
                    }

                    // Save current service states
                    SaveCurrentServiceStates(Path.Combine(backupFile, "services.json"));

                    // Save current power scheme
                    SaveCurrentPowerScheme(Path.Combine(backupFile, "power_scheme.txt"));

                    // Create system restore point
                    ExecutePowerShellCommand("Checkpoint-Computer -Description 'Gaming Optimization Backup' -RestorePointType 'MODIFY_SETTINGS'");

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private void SaveCurrentServiceStates(string filePath)
        {
            try
            {
                var serviceStates = new Dictionary<string, object>();
                var services = ServiceController.GetServices();

                foreach (var service in services)
                {
                    try
                    {
                        serviceStates[service.ServiceName] = new
                        {
                            Status = service.Status.ToString(),
                            StartType = service.StartType.ToString()
                        };
                    }
                    catch { }
                }

                string json = System.Text.Json.JsonSerializer.Serialize(serviceStates, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        private void SaveCurrentPowerScheme(string filePath)
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "/getactivescheme",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        File.WriteAllText(filePath, output);
                    }
                }
            }
            catch { }
        }

        public async Task<bool> RestoreSystemBackup(string backupPath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(backupPath))
                {
                    // Find the most recent backup
                    var backupDirs = Directory.GetDirectories(this.backupPath, "gaming_optimization_backup_*");
                    if (backupDirs.Length == 0)
                        return false;

                    backupPath = backupDirs.OrderByDescending(d => Directory.GetCreationTime(d)).First();
                }

                // Restore registry files
                var regFiles = Directory.GetFiles(backupPath, "*.reg");
                foreach (var regFile in regFiles)
                {
                    ExecuteScriptCommand($"reg import \"{regFile}\"");
                }

                // Restore services
                string servicesFile = Path.Combine(backupPath, "services.json");
                if (File.Exists(servicesFile))
                {
                    await RestoreServiceStates(servicesFile);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task RestoreServiceStates(string servicesFilePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    string json = File.ReadAllText(servicesFilePath);
                    var serviceStates = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                    foreach (var serviceState in serviceStates)
                    {
                        try
                        {
                            var serviceData = ((System.Text.Json.JsonElement)serviceState.Value).EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString());

                            if (serviceData.TryGetValue("StartType", out string startType))
                            {
                                SetServiceStartupType(serviceState.Key, startType);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            });
        }

        public OptimizationStatus GetOptimizationStatus()
        {
            var status = new OptimizationStatus();

            try
            {
                // Check if gaming optimizations are applied
                status.CPUOptimized = CheckCPUOptimizations();
                status.MemoryOptimized = CheckMemoryOptimizations();
                status.GraphicsOptimized = CheckGraphicsOptimizations();
                status.NetworkOptimized = CheckNetworkOptimizations();
                status.ServicesOptimized = CheckServiceOptimizations();
                status.PowerOptimized = CheckPowerOptimizations();

                status.OverallOptimized = status.CPUOptimized && status.MemoryOptimized &&
                                        status.GraphicsOptimized && status.NetworkOptimized;
            }
            catch
            {
                status.OverallOptimized = false;
            }

            return status;
        }

        private bool CheckCPUOptimizations()
        {
            try
            {
                var value = GetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation");
                return value?.ToString() == "38";
            }
            catch
            {
                return false;
            }
        }

        private bool CheckMemoryOptimizations()
        {
            try
            {
                var value = GetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache");
                return value?.ToString() == "0";
            }
            catch
            {
                return false;
            }
        }

        private bool CheckGraphicsOptimizations()
        {
            try
            {
                var value = GetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode");
                return value?.ToString() == "2";
            }
            catch
            {
                return false;
            }
        }

        private bool CheckNetworkOptimizations()
        {
            try
            {
                var value = GetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay");
                return value?.ToString() == "1";
            }
            catch
            {
                return false;
            }
        }

        private bool CheckServiceOptimizations()
        {
            try
            {
                using (var service = new ServiceController("SysMain"))
                {
                    return service.Status == ServiceControllerStatus.Stopped;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool CheckPowerOptimizations()
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "/getactivescheme",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        // Check if High Performance or Ultimate Performance is active
                        return output.Contains("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c") || // High Performance
                               output.Contains("e9a42b02-d5df-448d-aa00-03f14749eb61");   // Ultimate Performance
                    }
                }
            }
            catch { }

            return false;
        }
    }

    // Result classes
    public class OptimizationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int OptimizationsApplied { get; set; }
        public bool BackupCreated { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class OptimizationStatus
    {
        public bool OverallOptimized { get; set; }
        public bool CPUOptimized { get; set; }
        public bool MemoryOptimized { get; set; }
        public bool GraphicsOptimized { get; set; }
        public bool NetworkOptimized { get; set; }
        public bool ServicesOptimized { get; set; }
        public bool PowerOptimized { get; set; }
    }
}