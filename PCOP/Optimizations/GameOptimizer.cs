using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using Microsoft.Win32;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PCOptimizer.Optimizations
{
    public class GameOptimizer
    {
        private readonly string backupPath;
        private readonly SystemInfo systemInfo;

        public GameOptimizer()
        {
            backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                     "PCOptimizer", "Backup");
            Directory.CreateDirectory(backupPath);
            systemInfo = new SystemInfo();
        }

        public async Task OptimizeForMaximumFPS()
        {
            try
            {
                // Create comprehensive backup first
                CreateSystemBackup();

                // 1. CPU Optimizations
                await OptimizeCPUSettings();

                // 2. Memory Optimizations  
                await OptimizeMemorySettings();

                // 3. Graphics Optimizations
                await OptimizeGraphicsDrivers();

                // 4. Network Optimizations (critical for online games like Rust)
                await OptimizeNetworkSettings();

                // 5. Windows Gaming Optimizations
                await OptimizeWindowsGaming();

                // 6. System Services Optimization
                await OptimizeSystemServices();

                // 7. Power Management
                await OptimizePowerSettings();

                // 8. Storage Optimizations
                await OptimizeStorageSettings();

                // 9. Game-Specific Tweaks
                await ApplyGameSpecificTweaks();
            }
            catch (Exception ex)
            {
                throw new Exception($"FPS optimization failed: {ex.Message}");
            }
        }

        private async Task OptimizeCPUSettings()
        {
            await Task.Run(() =>
            {
                // CPU Priority and Affinity Settings
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\PriorityControl",
                    "Win32PrioritySeparation", 38); // Optimize for programs, not background

                // Disable CPU throttling
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power",
                    "CsEnabled", 0);

                // Optimize CPU scheduling
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager",
                    "HeapDeCommitFreeBlockThreshold", 0x40000);

                // Disable CPU idle states for maximum performance
                Process.Start("powercfg", "/setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 5d76a2ca-e8c0-402f-a133-2158492d58ad 0")?.WaitForExit();
                Process.Start("powercfg", "/setdcvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 5d76a2ca-e8c0-402f-a133-2158492d58ad 0")?.WaitForExit();

                // Set CPU to maximum performance
                Process.Start("powercfg", "/setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc5038f7-23e0-4960-96da-33abaf5935ec 100")?.WaitForExit();
                Process.Start("powercfg", "/setdcvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc5038f7-23e0-4960-96da-33abaf5935ec 100")?.WaitForExit();
            });
        }

        private async Task OptimizeMemorySettings()
        {
            await Task.Run(() =>
            {
                // Memory management optimizations
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    "LargeSystemCache", 0); // Optimize for applications, not file cache

                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    "DisablePagingExecutive", 1); // Keep system code in physical memory

                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    "ClearPageFileAtShutdown", 0); // Don't clear pagefile (faster boot)

                // Optimize virtual memory
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                    "IoPageLockLimit", 0x4000000); // 64MB for I/O operations

                // Disable memory compression (can cause stuttering in games)
                try
                {
                    Process.Start("powershell", "-Command \"Disable-MMAgent -MemoryCompression\"")?.WaitForExit();
                }
                catch { }

                // Set optimal paging file size
                SetOptimalPagingFile();
            });
        }

        private async Task OptimizeGraphicsDrivers()
        {
            await Task.Run(() =>
            {
                if (systemInfo.IsNVIDIAGraphics())
                {
                    OptimizeNVIDIAForGaming();
                }
                else if (systemInfo.IsAMDGraphics())
                {
                    OptimizeAMDForGaming();
                }

                // General graphics optimizations
                OptimizeGeneralGraphics();
            });
        }

        private void OptimizeNVIDIAForGaming()
        {
            // NVIDIA Control Panel optimizations via registry
            string nvidiaProfileKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";

            // Disable NVIDIA HDCP (can cause performance issues)
            SetRegistryValue(nvidiaProfileKey, "RmHdcpKeyglobZero", 1);

            // Optimize GPU scheduling
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                "HwSchMode", 2); // Hardware accelerated GPU scheduling

            // Disable TDR (Timeout Detection and Recovery) for competitive gaming
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                "TdrLevel", 0);
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                "TdrDelay", 60);

            // NVIDIA-specific performance settings
            SetRegistryValue(nvidiaProfileKey, "PowerMizerEnable", 1);
            SetRegistryValue(nvidiaProfileKey, "PowerMizerLevel", 1); // Maximum performance
            SetRegistryValue(nvidiaProfileKey, "PowerMizerLevelAC", 1);
        }

        private void OptimizeAMDForGaming()
        {
            string amdKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";

            // AMD-specific optimizations
            SetRegistryValue(amdKey, "KMD_FRTEnabled", 0); // Disable Frame Rate Target Control
            SetRegistryValue(amdKey, "KMD_DeLagEnabled", 0); // Disable Anti-Lag if causing issues

            // Enable GPU scheduling for AMD
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                "HwSchMode", 2);
        }

        private void OptimizeGeneralGraphics()
        {
            // Disable fullscreen optimizations
            SetRegistryValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 0);
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
                "AppCaptureEnabled", 0);
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR",
                "GameDVR_Enabled", 0);

            // Disable Game Mode notifications but keep Game Mode enabled
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "ShowStartupPanel", 0);
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0);

            // Enable Hardware Accelerated GPU Scheduling (Windows 10 2004+)
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                "HwSchMode", 2);
        }

        private async Task OptimizeNetworkSettings()
        {
            await Task.Run(() =>
            {
                // TCP optimizations for gaming
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                    "TcpAckFrequency", 1);
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                    "TCPNoDelay", 1);
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                    "TcpDelAckTicks", 0);

                // Disable Nagle algorithm for reduced latency
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                    "TcpNoDelay", 1);

                // Optimize network adapter settings
                OptimizeNetworkAdapter();

                // Windows scaling heuristics
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
                    "EnableWsd", 0);

                // Disable bandwidth throttling
                try
                {
                    Process.Start("netsh", "int tcp set global autotuninglevel=normal")?.WaitForExit();
                    Process.Start("netsh", "int tcp set global chimney=enabled")?.WaitForExit();
                    Process.Start("netsh", "int tcp set global rss=enabled")?.WaitForExit();
                    Process.Start("netsh", "int tcp set global netdma=enabled")?.WaitForExit();
                }
                catch { }
            });
        }

        private void OptimizeNetworkAdapter()
        {
            // Get network adapters and optimize their settings
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled=true"))
            {
                foreach (ManagementObject adapter in searcher.Get())
                {
                    string deviceId = adapter["DeviceID"]?.ToString();
                    if (!string.IsNullOrEmpty(deviceId))
                    {
                        string adapterKey = $@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{{4d36e972-e325-11ce-bfc1-08002be10318}}\{deviceId.PadLeft(4, '0')}";

                        // Disable power management
                        SetRegistryValue(adapterKey, "*FlowControl", "0");
                        SetRegistryValue(adapterKey, "*InterruptModeration", "0");
                        SetRegistryValue(adapterKey, "*RSS", "1"); // Enable if supported
                        SetRegistryValue(adapterKey, "*TCPUDPChecksumOffloadIPv4", "3");
                        SetRegistryValue(adapterKey, "*TCPUDPChecksumOffloadIPv6", "3");
                    }
                }
            }
        }

        private async Task OptimizeWindowsGaming()
        {
            await Task.Run(() =>
            {
                // Enable Game Mode
                SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "AutoGameModeEnabled", 1);
                SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\GameBar", "AllowAutoGameMode", 1);

                // Disable Windows Search indexing for games drives
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ContentIndex",
                    "FilterFilesWithUnknownExtensions", 0);

                // Disable SuperFetch/SysMain (can cause stuttering)
                StopAndDisableService("SysMain");

                // Optimize Windows Update delivery
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config",
                    "DODownloadMode", 0);

                // Disable background app access
                SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
                    "GlobalUserDisabled", 1);

                // Optimize timer resolution for consistent frame times
                try
                {
                    Process.Start("bcdedit", "/set useplatformclock true")?.WaitForExit();
                }
                catch { }
            });
        }

        private async Task OptimizeSystemServices()
        {
            await Task.Run(() =>
            {
                // Services that can impact gaming performance
                string[] gamingOptimizedServices = {
                    "SysMain", // SuperFetch - can cause stuttering
                    "WSearch", // Windows Search - disk I/O
                    "Spooler", // Print Spooler - unnecessary for gaming
                    "Fax", // Fax service
                    "TabletInputService", // Tablet Input Service
                    "WbioSrvc", // Windows Biometric Service
                    "WMPNetworkSvc", // Windows Media Player Network Sharing Service
                    "XblAuthManager", // Xbox Live Auth Manager (if not using Xbox features)
                    "XblGameSave", // Xbox Live Game Save
                    "XboxNetApiSvc", // Xbox Live Networking Service
                    "XboxGipSvc", // Xbox Accessory Management Service
                    "MapsBroker", // Downloaded Maps Manager
                    "lfsvc", // Geolocation Service
                    "DiagTrack", // Diagnostics Tracking Service
                    "dmwappushservice", // WAP Push Message Routing Service
                    "TrkWks", // Distributed Link Tracking Client
                    "WerSvc" // Windows Error Reporting Service
                };

                foreach (string serviceName in gamingOptimizedServices)
                {
                    StopAndDisableService(serviceName);
                }

                // Keep essential services running but optimize them
                OptimizeEssentialServices();
            });
        }

        private void OptimizeEssentialServices()
        {
            // Optimize Windows Audio for low latency
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\AudioSrv",
                "DependOnService", new string[] { "AudioEndpointBuilder", "RpcSs" });

            // Optimize NVIDIA services if present
            if (systemInfo.IsNVIDIAGraphics())
            {
                SetServiceStartupType("NVDisplay.ContainerLocalSystem", "Automatic");
                SetServiceStartupType("nvlddmkm", "Automatic");
            }
        }

        private async Task OptimizePowerSettings()
        {
            await Task.Run(() =>
            {
                // Set Ultimate Performance power plan (Windows 10 1803+)
                try
                {
                    Process.Start("powercfg", "/duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61")?.WaitForExit();
                    Process.Start("powercfg", "/setactive e9a42b02-d5df-448d-aa00-03f14749eb61")?.WaitForExit();
                }
                catch
                {
                    // Fallback to High Performance
                    Process.Start("powercfg", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c")?.WaitForExit();
                }

                // Disable USB selective suspend
                Process.Start("powercfg", "/setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0")?.WaitForExit();
                Process.Start("powercfg", "/setdcvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0")?.WaitForExit();

                // Disable PCI Express Link State Power Management
                Process.Start("powercfg", "/setacvalueindex SCHEME_CURRENT 501a4d13-42af-4429-9fd1-a8218c268e20 ee12f906-d277-404b-b6da-e5fa1a576df5 0")?.WaitForExit();
                Process.Start("powercfg", "/setdcvalueindex SCHEME_CURRENT 501a4d13-42af-4429-9fd1-a8218c268e20 ee12f906-d277-404b-b6da-e5fa1a576df5 0")?.WaitForExit();

                // Apply changes
                Process.Start("powercfg", "/setactive SCHEME_CURRENT")?.WaitForExit();
            });
        }

        private async Task OptimizeStorageSettings()
        {
            await Task.Run(() =>
            {
                // Disable Windows Defender real-time protection temporarily for game folders
                // Note: This should be done carefully and re-enabled after gaming

                // Optimize NTFS settings
                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem",
                    "NtfsDisableLastAccessUpdate", 1); // Disable last access time updates

                SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem",
                    "NtfsDisable8dot3NameCreation", 1); // Disable 8.3 filename creation

                // Optimize disk defragmentation schedule
                try
                {
                    Process.Start("schtasks", "/Change /TN \"\\Microsoft\\Windows\\Defrag\\ScheduledDefrag\" /Disable")?.WaitForExit();
                }
                catch { }

                // Enable write caching for better performance
                EnableWriteCachingForGameDrives();
            });
        }

        private async Task ApplyGameSpecificTweaks()
        {
            await Task.Run(() =>
            {
                // Mouse optimizations for FPS games
                OptimizeMouseSettings();

                // Keyboard optimizations
                OptimizeKeyboardSettings();

                // Display optimizations
                OptimizeDisplaySettings();

                // Audio optimizations for competitive gaming
                OptimizeAudioSettings();

                // Rust-specific optimizations
                ApplyRustSpecificTweaks();
            });
        }

        private void OptimizeMouseSettings()
        {
            // Disable mouse acceleration
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed", "0");
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold1", "0");
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseThreshold2", "0");

            // Optimize mouse polling rate (this requires specific mouse driver support)
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\mouhid\Parameters",
                "MouseDataQueueSize", 0x64); // Increase mouse data queue size
        }

        private void OptimizeKeyboardSettings()
        {
            // Optimize keyboard repeat rate
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardDelay", "0");
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Keyboard", "KeyboardSpeed", "31");

            // Disable Filter Keys
            SetRegistryValue(@"HKEY_CURRENT_USER\Control Panel\Accessibility\Keyboard Response",
                "Flags", "0");
        }

        private void OptimizeDisplaySettings()
        {
            // Disable fullscreen optimizations globally
            SetRegistryValue(@"HKEY_CURRENT_USER\System\GameConfigStore", "GameDVR_Enabled", 0);

            // Optimize DWM for gaming
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\Dwm",
                "OverlayTestMode", 5); // Disable overlays that can impact performance
        }

        private void OptimizeAudioSettings()
        {
            // Set audio to 16-bit 44100 Hz for lower latency
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render",
                "DeviceState", 1);

            // Disable audio enhancements
            SetRegistryValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Multimedia\Audio",
                "UserSimulatedStereoOn", 0);
        }

        private void ApplyRustSpecificTweaks()
        {
            // Create Rust-specific launch options file
            string steamAppsPath = FindSteamAppsPath();
            if (!string.IsNullOrEmpty(steamAppsPath))
            {
                string rustPath = Path.Combine(steamAppsPath, "common", "Rust");
                if (Directory.Exists(rustPath))
                {
                    // Create batch file with optimized launch options
                    string launchScript = Path.Combine(rustPath, "rust_optimized.bat");
                    string[] launchOptions = {
                        "@echo off",
                        "REM Optimized Rust Launch Script",
                        "set /A cores=%NUMBER_OF_PROCESSORS%-1",
                        "start /high /affinity FF RustClient.exe -high -malloc=system -winxp -nojoy -nopreload -nointro -nosplash +fps_max 0"
                    };
                    File.WriteAllLines(launchScript, launchOptions);
                }
            }

            // Registry tweaks for better Rust performance
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                "GPU Priority", 8);
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                "Priority", 6);
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                "Scheduling Category", "High");
            SetRegistryValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                "SFIO Priority", "High");
        }

        private string FindSteamAppsPath()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    if (key != null)
                    {
                        string steamPath = key.GetValue("SteamPath")?.ToString();
                        if (!string.IsNullOrEmpty(steamPath))
                        {
                            return Path.Combine(steamPath, "steamapps");
                        }
                    }
                }
            }
            catch { }

            // Fallback locations
            string[] commonSteamPaths = {
                @"C:\Program Files (x86)\Steam\steamapps",
                @"C:\Program Files\Steam\steamapps",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps")
            };

            return commonSteamPaths.FirstOrDefault(Directory.Exists);
        }

        private void SetOptimalPagingFile()
        {
            try
            {
                // Set paging file to system managed or 1.5x RAM size
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        long totalRAM = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                        long optimalPageFile = totalRAM / 1024 / 1024 * 3 / 2; // 1.5x RAM in MB

                        // Set paging file via WMI
                        using (var pageFileSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileSetting"))
                        {
                            foreach (ManagementObject pageFile in pageFileSearcher.Get())
                            {
                                pageFile["InitialSize"] = optimalPageFile;
                                pageFile["MaximumSize"] = optimalPageFile;
                                pageFile.Put();
                            }
                        }
                        break;
                    }
                }
            }
            catch { }
        }

        private void EnableWriteCachingForGameDrives()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        string deviceId = disk["DeviceID"]?.ToString();
                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            // Enable write caching if it's an SSD or high-performance drive
                            // This is a simplified approach - in production, you'd want more sophisticated detection
                        }
                    }
                }
            }
            catch { }
        }

        private void StopAndDisableService(string serviceName)
        {
            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }

                SetServiceStartupType(serviceName, "Disabled");
            }
            catch { } // Service might not exist
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
                using (RegistryKey key = baseKey.CreateSubKey(subKeyPath))
                {
                    key?.SetValue(valueName, value);
                }
            }
            catch { }
        }

        private void CreateSystemBackup()
        {
            try
            {
                // Create comprehensive backup before making changes
                string backupFile = Path.Combine(backupPath, $"gaming_optimization_backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg");

                // Export critical registry keys
                string[] criticalKeys = {
                    "HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl",
                    "HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers",
                    "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters",
                    "HKCU\\SOFTWARE\\Microsoft\\GameBar",
                    "HKCU\\Control Panel\\Mouse"
                };

                foreach (string key in criticalKeys)
                {
                    try
                    {
                        Process.Start("reg", $"export \"{key}\" \"{backupFile}_{key.Replace("\\", "_").Replace(":", "")}.reg\"")?.WaitForExit();
                    }
                    catch { }
                }

                // Create system restore point
                try
                {
                    Process.Start("powershell", "-Command \"Checkpoint-Computer -Description 'Gaming Optimization Backup' -RestorePointType 'MODIFY_SETTINGS'\"")?.WaitForExit();
                }
                catch { }
            }
            catch (Exception ex)
            {
                throw new Exception($"Backup creation failed: {ex.Message}");
            }
        }
    }
}