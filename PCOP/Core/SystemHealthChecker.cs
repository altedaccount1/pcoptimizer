using System;
using System.Collections.Generic;
using System.Management;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Win32;
using System.ServiceProcess;

namespace PCOptimizer.Diagnostics
{
    public class SystemHealthChecker
    {
        public async Task<HealthCheckResult> PerformComprehensiveHealthCheck()
        {
            var result = new HealthCheckResult();

            try
            {
                // Run all health checks
                result.CPUHealth = await CheckCPUHealth();
                result.MemoryHealth = await CheckMemoryHealth();
                result.StorageHealth = await CheckStorageHealth();
                result.GraphicsHealth = await CheckGraphicsHealth();
                result.NetworkHealth = await CheckNetworkHealth();
                result.SystemHealth = await CheckSystemHealth();
                result.PerformanceHealth = await CheckPerformanceHealth();
                result.SecurityHealth = await CheckSecurityHealth();

                // Calculate overall health score
                var healthScores = new[]
                {
                    result.CPUHealth.HealthScore,
                    result.MemoryHealth.HealthScore,
                    result.StorageHealth.HealthScore,
                    result.GraphicsHealth.HealthScore,
                    result.NetworkHealth.HealthScore,
                    result.SystemHealth.HealthScore,
                    result.PerformanceHealth.HealthScore,
                    result.SecurityHealth.HealthScore
                };

                result.OverallHealthScore = (int)healthScores.Average();
                result.OverallStatus = GetHealthStatus(result.OverallHealthScore);
                result.Success = true;
                result.Recommendations = GenerateRecommendations(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private async Task<ComponentHealth> CheckCPUHealth()
        {
            return await Task.Run(() =>
            {
                var health = new ComponentHealth { ComponentName = "CPU" };

                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Name, LoadPercentage FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            health.ComponentInfo = obj["Name"]?.ToString() ?? "Unknown CPU";

                            if (obj["LoadPercentage"] != null)
                            {
                                int cpuUsage = Convert.ToInt32(obj["LoadPercentage"]);
                                health.Details.Add($"Current CPU Usage: {cpuUsage}%");

                                if (cpuUsage < 30)
                                    health.HealthScore += 30;
                                else if (cpuUsage < 70)
                                    health.HealthScore += 20;
                                else
                                    health.HealthScore += 10;
                            }
                            break;
                        }
                    }

                    // Check CPU temperature if available
                    try
                    {
                        using (var tempSearcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                        {
                            foreach (ManagementObject obj in tempSearcher.Get())
                            {
                                double temp = Convert.ToDouble(obj["CurrentTemperature"]);
                                temp = (temp / 10.0) - 273.15; // Convert from Kelvin to Celsius

                                health.Details.Add($"CPU Temperature: {temp:F1}°C");

                                if (temp < 70)
                                    health.HealthScore += 20;
                                else if (temp < 85)
                                    health.HealthScore += 10;
                                else
                                    health.Issues.Add("High CPU temperature detected!");

                                break;
                            }
                        }
                    }
                    catch
                    {
                        health.Details.Add("Temperature monitoring not available");
                        health.HealthScore += 15; // Neutral score if can't check
                    }

                    // Check CPU core count
                    using (var coreSearcher = new ManagementObjectSearcher("SELECT NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in coreSearcher.Get())
                        {
                            int cores = Convert.ToInt32(obj["NumberOfCores"]);
                            int threads = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);

                            health.Details.Add($"Cores: {cores}, Threads: {threads}");

                            if (cores >= 8)
                                health.HealthScore += 25;
                            else if (cores >= 4)
                                health.HealthScore += 20;
                            else
                                health.HealthScore += 10;
                            break;
                        }
                    }

                    health.Status = GetHealthStatus(health.HealthScore);
                    if (health.HealthScore < 50)
                        health.Issues.Add("CPU may be under heavy load or thermal throttling");
                }
                catch (Exception ex)
                {
                    health.Issues.Add($"CPU health check failed: {ex.Message}");
                    health.HealthScore = 50; // Default neutral score
                }

                return health;
            });
        }

        private async Task<ComponentHealth> CheckMemoryHealth()
        {
            return await Task.Run(() =>
            {
                var health = new ComponentHealth { ComponentName = "Memory" };

                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory, AvailablePhysicalMemory FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            long totalRAM = Convert.ToInt64(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                            long availableRAM = Convert.ToInt64(obj["AvailablePhysicalMemory"]) / (1024 * 1024 * 1024);
                            long usedRAM = totalRAM - availableRAM;
                            double usagePercent = (double)usedRAM / totalRAM * 100;

                            health.ComponentInfo = $"{totalRAM} GB Total RAM";
                            health.Details.Add($"Used: {usedRAM} GB ({usagePercent:F1}%)");
                            health.Details.Add($"Available: {availableRAM} GB");

                            // Score based on RAM amount and usage
                            if (totalRAM >= 32)
                                health.HealthScore += 30;
                            else if (totalRAM >= 16)
                                health.HealthScore += 25;
                            else if (totalRAM >= 8)
                                health.HealthScore += 15;
                            else
                                health.HealthScore += 5;

                            if (usagePercent < 70)
                                health.HealthScore += 25;
                            else if (usagePercent < 85)
                                health.HealthScore += 15;
                            else
                                health.HealthScore += 5;

                            if (usagePercent > 90)
                                health.Issues.Add("High memory usage detected - consider closing applications");

                            break;
                        }
                    }

                    // Check for memory errors
                    CheckMemoryErrors(health);

                    health.Status = GetHealthStatus(health.HealthScore);
                }
                catch (Exception ex)
                {
                    health.Issues.Add($"Memory health check failed: {ex.Message}");
                    health.HealthScore = 50;
                }

                return health;
            });
        }

        private async Task<ComponentHealth> CheckStorageHealth()
        {
            return await Task.Run(() =>
            {
                var health = new ComponentHealth { ComponentName = "Storage" };

                try
                {
                    DriveInfo[] drives = DriveInfo.GetDrives();
                    var systemDrive = drives.FirstOrDefault(d => d.Name.StartsWith("C:"));

                    if (systemDrive != null)
                    {
                        long totalGB = systemDrive.TotalSize / (1024 * 1024 * 1024);
                        long freeGB = systemDrive.AvailableFreeSpace / (1024 * 1024 * 1024);
                        double freePercent = (double)freeGB / totalGB * 100;

                        health.ComponentInfo = $"{totalGB} GB {systemDrive.DriveFormat} Drive";
                        health.Details.Add($"Free Space: {freeGB} GB ({freePercent:F1}%)");

                        // Check drive type (SSD vs HDD)
                        string driveType = GetDriveType(systemDrive.Name);
                        health.Details.Add($"Drive Type: {driveType}");

                        // Score based on free space and drive type
                        if (freePercent > 30)
                            health.HealthScore += 25;
                        else if (freePercent > 15)
                            health.HealthScore += 15;
                        else
                            health.HealthScore += 5;

                        if (driveType.Contains("SSD"))
                            health.HealthScore += 25;
                        else
                            health.HealthScore += 15;

                        if (freePercent < 10)
                            health.Issues.Add("Low disk space - performance may be affected");

                        // Check for disk errors
                        CheckDiskErrors(health, systemDrive.Name);
                    }

                    health.Status = GetHealthStatus(health.HealthScore);
                }
                catch (Exception ex)
                {
                    health.Issues.Add($"Storage health check failed: {ex.Message}");
                    health.HealthScore = 50;
                }

                return health;
            });
        }

        private async Task<ComponentHealth> CheckGraphicsHealth()
        {
            return await Task.Run(() =>
            {
                var health = new ComponentHealth { ComponentName = "Graphics" };

                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, VideoMemoryType FROM Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            string name = obj["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(name) && (name.Contains("NVIDIA") || name.Contains("AMD") || name.Contains("Intel")))
                            {
                                health.ComponentInfo = name;
                                health.Details.Add($"Driver Version: {obj["DriverVersion"]?.ToString() ?? "Unknown"}");

                                // Score based on GPU brand and capabilities
                                if (name.ToLower().Contains("nvidia") && name.Contains("RTX"))
                                    health.HealthScore += 30;
                                else if (name.ToLower().Contains("nvidia") && name.Contains("GTX"))
                                    health.HealthScore += 25;
                                else if (name.ToLower().Contains("amd") && name.Contains("RX"))
                                    health.HealthScore += 25;
                                else if (name.ToLower().Contains("intel"))
                                    health.HealthScore += 15;
                                else
                                    health.HealthScore += 20;

                                break;
                            }
                        }
                    }

                    // Check graphics optimizations
                    CheckGraphicsOptimizations(health);

                    health.Status = GetHealthStatus(health.HealthScore);
                }
                catch (Exception ex)
                {
                    health.Issues.Add($"Graphics health check failed: {ex.Message}");
                    health.HealthScore = 50;
                }

                return health;
            });
        }

        private async Task<ComponentHealth> CheckNetworkHealth()
        {
            return await Task.Run(() =>
            {
                var health = new ComponentHealth { ComponentName = "Network" };

                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Name, Speed FROM Win32_NetworkAdapter WHERE NetEnabled=true"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            string name = obj["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(name) && !name.Contains("Virtual") && !name.Contains("Loopback"))
                            {
                                health.ComponentInfo = name;

                                if (obj["Speed"] != null)
                                {
                                    long speed = Convert.ToInt64(obj["Speed"]) / 1000000; // Convert to Mbps
                                    health.Details.Add($"Speed: {speed} Mbps");

                                    if (speed >= 1000) // Gigabit
                                        health.HealthScore += 25;
                                    else if (speed >= 100)
                                        health.HealthScore += 15;
                                    else
                                        health.HealthScore += 10;
                                }
                                break;
                            }
                        }
                    }

                    // Test internet connectivity
                    var pingResult = TestInternetConnectivity();
                    health.Details.Add($"Internet Connectivity: {(pingResult.Success ? "Connected" : "Issues detected")}");
                    if (pingResult.Success)
                    {
                        health.Details.Add($"Ping to Google: {pingResult.PingTime}ms");
                        if (pingResult.PingTime < 50)
                            health.HealthScore += 25;
                        else if (pingResult.PingTime < 100)
                            health.HealthScore += 15;
                        else
                            health.HealthScore += 10;
                    }
                    else
                    {
                        health.Issues.Add("Internet connectivity issues detected");
                        health.HealthScore += 5;
                    }

                    health.Status = GetHealthStatus(health.HealthScore);
                }
                catch (Exception ex)
                {
                    health.Issues.Add($"Network health check failed: {ex.Message}");
                    health.HealthScore = 50;
                }

                return health;
            });
        }

        private async Task<ComponentHealth> CheckSystemHealth()
        {
            return await Task.Run(() =>
            {
                var health = new ComponentHealth { ComponentName = "System" };

                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version, LastBootUpTime FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            health.ComponentInfo = $"{obj["Caption"]} ({obj["Version"]})";

                            // Calculate uptime
                            var bootTime = ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"].ToString());
                            var uptime = DateTime.Now - bootTime;
                            health.Details.Add($"Uptime: {uptime.Days} days, {uptime.Hours} hours");

                            if (uptime.Days < 7)
                                health.HealthScore += 20;
                            else if (uptime.Days < 30)
                                health.HealthScore += 15;
                            else
                                health.HealthScore += 10;

                            break;
                        }
                    }

                    // Check Windows version
                    var windowsVersion = GetWindowsVersion();
                    health.Details.Add($"Windows Version: {windowsVersion}");
                    if (windowsVersion.Contains("Windows 11") || windowsVersion.Contains("Windows 10"))
                        health.HealthScore += 20;
                    else
                        health.HealthScore += 10;

                    // Check system errors
                    CheckSystemErrors(health);

                    // Check Windows updates
                    CheckWindowsUpdates(health);

                    health.Status = GetHealthStatus(health.HealthScore);
                }
                catch (Exception ex)
                {
                    health.Issues.Add($"System health check failed: {ex.Message}");
                    health.HealthScore = 50;
                }

                return health;
            });
        }

        private async Task<ComponentHealth> CheckPerformanceHealth()
        {
            return await Task.Run(() =>
            {
                var health = new ComponentHealth { ComponentName = "Performance" };

                try
                {
                    // Check startup programs
                    int startupPrograms = CountStartupPrograms();
                    health.Details.Add($"Startup Programs: {startupPrograms}");

                    if (startupPrograms < 10)
                        health.HealthScore += 20;
                    else if (startupPrograms < 20)
                        health.HealthScore += 15;
                    else
                        health.HealthScore += 5;

                    // Check running processes
                    Process[] processes = Process.GetProcesses();
                    int processCount = processes.Length;
                    health.Details.Add($"Running Processes: {processCount}");

                    if (processCount < 80)
                        health.HealthScore += 15;
                    else if (processCount < 120)
                        health.HealthScore += 10;
                    else
                        health.HealthScore += 5;

                    // Check for performance-impacting services
                    CheckPerformanceServices(health);

                    // Check power plan
                    string powerPlan = GetActivePowerPlan();
                    health.Details.Add($"Power Plan: {powerPlan}");
                    if (powerPlan.Contains("High performance") || powerPlan.Contains("Ultimate"))
                        health.HealthScore += 15;
                    else
                        health.HealthScore += 10;

                    if (startupPrograms > 15)
                        health.Issues.Add("Too many startup programs may slow boot time");

                    health.Status = GetHealthStatus(health.HealthScore);
                }
                catch (Exception ex)
                {
                    health.Issues.Add($"Performance health check failed: {ex.Message}");
                    health.HealthScore = 50;
                }

                return health;
            });
        }

        private async Task<ComponentHealth> CheckSecurityHealth()
        {
            return await Task.Run(() =>
            {
                var health = new ComponentHealth { ComponentName = "Security" };

                try
                {
                    // Check Windows Defender status
                    bool defenderEnabled = CheckWindowsDefender();
                    health.Details.Add($"Windows Defender: {(defenderEnabled ? "Enabled" : "Disabled/Issues")}");

                    if (defenderEnabled)
                        health.HealthScore += 25;
                    else
                        health.Issues.Add("Windows Defender may not be properly configured");

                    // Check firewall status
                    bool firewallEnabled = CheckWindowsFirewall();
                    health.Details.Add($"Windows Firewall: {(firewallEnabled ? "Enabled" : "Disabled")}");

                    if (firewallEnabled)
                        health.HealthScore += 25;
                    else
                        health.Issues.Add("Windows Firewall is disabled");

                    // Check for pending updates
                    CheckSecurityUpdates(health);

                    health.Status = GetHealthStatus(health.HealthScore);
                }
                catch (Exception ex)
                {
                    health.Issues.Add($"Security health check failed: {ex.Message}");
                    health.HealthScore = 50;
                }

                return health;
            });
        }

        // Helper methods
        private void CheckMemoryErrors(ComponentHealth health)
        {
            try
            {
                using (var eventLog = new EventLog("System"))
                {
                    var recentEvents = eventLog.Entries.Cast<EventLogEntry>()
                        .Where(e => e.TimeGenerated > DateTime.Now.AddDays(-7))
                        .Where(e => e.EntryType == EventLogEntryType.Error)
                        .Where(e => e.Message.ToLower().Contains("memory") || e.Message.ToLower().Contains("ram"))
                        .Take(5);

                    if (recentEvents.Any())
                    {
                        health.Issues.Add("Recent memory errors detected in system log");
                        health.HealthScore -= 10;
                    }
                    else
                    {
                        health.HealthScore += 20;
                    }
                }
            }
            catch { }
        }

        private string GetDriveType(string driveLetter)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Model, MediaType FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string model = obj["Model"]?.ToString()?.ToLower();
                        if (model?.Contains("ssd") == true || model?.Contains("nvme") == true)
                            return "SSD";
                    }
                }
            }
            catch { }

            return "HDD";
        }

        private void CheckDiskErrors(ComponentHealth health, string driveLetter)
        {
            try
            {
                using (var eventLog = new EventLog("System"))
                {
                    var diskErrors = eventLog.Entries.Cast<EventLogEntry>()
                        .Where(e => e.TimeGenerated > DateTime.Now.AddDays(-7))
                        .Where(e => e.EntryType == EventLogEntryType.Error)
                        .Where(e => e.Message.ToLower().Contains("disk") || e.Message.ToLower().Contains("drive"))
                        .Take(5);

                    if (diskErrors.Any())
                    {
                        health.Issues.Add("Recent disk errors detected - consider running disk check");
                        health.HealthScore -= 5;
                    }
                    else
                    {
                        health.HealthScore += 15;
                    }
                }
            }
            catch { }
        }

        private void CheckGraphicsOptimizations(ComponentHealth health)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers"))
                {
                    var hwSchMode = key?.GetValue("HwSchMode");
                    if (hwSchMode?.ToString() == "2")
                    {
                        health.Details.Add("Hardware GPU Scheduling: Enabled");
                        health.HealthScore += 15;
                    }
                    else
                    {
                        health.Details.Add("Hardware GPU Scheduling: Disabled");
                        health.HealthScore += 10;
                    }
                }
            }
            catch { }
        }

        private PingResult TestInternetConnectivity()
        {
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = ping.Send("8.8.8.8", 5000);
                    return new PingResult
                    {
                        Success = reply.Status == System.Net.NetworkInformation.IPStatus.Success,
                        PingTime = (int)reply.RoundtripTime
                    };
                }
            }
            catch
            {
                return new PingResult { Success = false, PingTime = 0 };
            }
        }

        private string GetWindowsVersion()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    string productName = key?.GetValue("ProductName")?.ToString() ?? "Unknown";
                    string displayVersion = key?.GetValue("DisplayVersion")?.ToString() ?? "";
                    return $"{productName} {displayVersion}".Trim();
                }
            }
            catch
            {
                return "Unknown Windows Version";
            }
        }

        private void CheckSystemErrors(ComponentHealth health)
        {
            try
            {
                using (var eventLog = new EventLog("System"))
                {
                    var recentErrors = eventLog.Entries.Cast<EventLogEntry>()
                        .Where(e => e.TimeGenerated > DateTime.Now.AddDays(-7))
                        .Where(e => e.EntryType == EventLogEntryType.Error)
                        .Take(10)
                        .Count();

                    health.Details.Add($"Recent System Errors: {recentErrors}");

                    if (recentErrors < 5)
                        health.HealthScore += 15;
                    else if (recentErrors < 20)
                        health.HealthScore += 10;
                    else
                        health.HealthScore += 5;

                    if (recentErrors > 20)
                        health.Issues.Add("High number of system errors detected");
                }
            }
            catch { }
        }

        private void CheckWindowsUpdates(ComponentHealth health)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_QuickFixEngineering"))
                {
                    var updates = searcher.Get().Cast<ManagementObject>().ToList();
                    var recentUpdates = updates.Where(u =>
                    {
                        try
                        {
                            var installDate = u["InstalledOn"]?.ToString();
                            if (DateTime.TryParse(installDate, out DateTime date))
                                return date > DateTime.Now.AddDays(-30);
                        }
                        catch { }
                        return false;
                    }).Count();

                    health.Details.Add($"Recent Updates: {recentUpdates}");

                    if (recentUpdates > 0)
                        health.HealthScore += 15;
                    else
                        health.Issues.Add("No recent Windows updates - system may be outdated");
                }
            }
            catch { }
        }

        private int CountStartupPrograms()
        {
            try
            {
                int count = 0;
                var startupKeys = new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce"
                };

                foreach (string keyPath in startupKeys)
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(keyPath))
                    {
                        if (key != null)
                            count += key.GetValueNames().Length; // Fixed: use GetValueNames()
                    }

                    using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        if (key != null)
                            count += key.GetValueNames().Length; // Fixed: use GetValueNames()
                    }
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }

        private void CheckPerformanceServices(ComponentHealth health)
        {
            try
            {
                var performanceImpactingServices = new[]
                {
                    "SysMain", // SuperFetch
                    "WSearch", // Windows Search
                    "DiagTrack" // Diagnostics Tracking
                };

                int runningServices = 0;
                foreach (string serviceName in performanceImpactingServices)
                {
                    try
                    {
                        using (var service = new ServiceController(serviceName))
                        {
                            if (service.Status == ServiceControllerStatus.Running)
                                runningServices++;
                        }
                    }
                    catch { }
                }

                if (runningServices == 0)
                {
                    health.Details.Add("Performance services optimized");
                    health.HealthScore += 10;
                }
                else
                {
                    health.Details.Add($"Performance-impacting services running: {runningServices}");
                    health.HealthScore += 5;
                }
            }
            catch { }
        }

        private string GetActivePowerPlan()
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

                        if (output.Contains("High performance"))
                            return "High Performance";
                        else if (output.Contains("Ultimate"))
                            return "Ultimate Performance";
                        else if (output.Contains("Balanced"))
                            return "Balanced";
                        else
                            return "Power Saver";
                    }
                }
            }
            catch { }

            return "Unknown";
        }

        private bool CheckWindowsDefender()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntivirusProduct"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string displayName = obj["displayName"]?.ToString();
                        if (displayName?.Contains("Windows Defender") == true)
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private bool CheckWindowsFirewall()
        {
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall show allprofiles state",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        return output.Contains("State                                 ON");
                    }
                }
            }
            catch { }

            return false;
        }

        private void CheckSecurityUpdates(ComponentHealth health)
        {
            health.Details.Add("Security updates check requires Windows Update scan");
            health.HealthScore += 10;
        }

        private string GetHealthStatus(int score)
        {
            if (score >= 80) return "Excellent";
            if (score >= 60) return "Good";
            if (score >= 40) return "Fair";
            if (score >= 20) return "Poor";
            return "Critical";
        }

        private List<string> GenerateRecommendations(HealthCheckResult result)
        {
            var recommendations = new List<string>();

            if (result.OverallHealthScore < 70)
                recommendations.Add("Run PC Performance Optimizer to improve system performance");

            if (result.StorageHealth.HealthScore < 50)
                recommendations.Add("Clean up disk space and consider disk cleanup");

            if (result.MemoryHealth.HealthScore < 50)
                recommendations.Add("Close unnecessary applications or consider RAM upgrade");

            if (result.PerformanceHealth.HealthScore < 50)
                recommendations.Add("Optimize startup programs and system services");

            if (result.SecurityHealth.HealthScore < 70)
                recommendations.Add("Update security settings and run Windows updates");

            if (result.GraphicsHealth.HealthScore < 50)
                recommendations.Add("Update graphics drivers and optimize GPU settings");

            return recommendations;
        }
    }

    // Data classes
    public class HealthCheckResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = ""; 
        public int OverallHealthScore { get; set; }
        public string OverallStatus { get; set; } = "";
        public ComponentHealth CPUHealth { get; set; } = new ComponentHealth();
        public ComponentHealth MemoryHealth { get; set; } = new ComponentHealth();
        public ComponentHealth StorageHealth { get; set; } = new ComponentHealth();
        public ComponentHealth GraphicsHealth { get; set; } = new ComponentHealth();
        public ComponentHealth NetworkHealth { get; set; } = new ComponentHealth();
        public ComponentHealth SystemHealth { get; set; } = new ComponentHealth();
        public ComponentHealth PerformanceHealth { get; set; } = new ComponentHealth();
        public ComponentHealth SecurityHealth { get; set; } = new ComponentHealth();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    public class ComponentHealth
    {
        public string ComponentName { get; set; } = "";
        public string ComponentInfo { get; set; } = "";
        public int HealthScore { get; set; } = 0;
        public string Status { get; set; } = "";
        public List<string> Details { get; set; } = new List<string>();
        public List<string> Issues { get; set; } = new List<string>();
    }

    public class PingResult
    {
        public bool Success { get; set; }
        public int PingTime { get; set; }
    }
}