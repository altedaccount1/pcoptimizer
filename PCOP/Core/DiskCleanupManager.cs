using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Diagnostics;

namespace PCOptimizer.Cleanup
{
    public class DiskCleanupManager
    {
        private long totalFreedSpace = 0;
        private List<string> cleanupLog = new List<string>();

        public async Task<CleanupResult> PerformComprehensiveCleanup()
        {
            var result = new CleanupResult();
            totalFreedSpace = 0;
            cleanupLog.Clear();

            try
            {
                // Run all cleanup tasks
                await CleanTempFolders();
                await CleanWindowsCache();
                await CleanBrowserCache();
                await CleanRecycleBin();
                await CleanWindowsLogs();
                await CleanPrefetchCache();
                await CleanThumbnailCache();
                await CleanWindowsUpdate();
                await CleanGameCache();
                await RunDiskCleanup();

                result.Success = true;
                result.SpaceFreedMB = totalFreedSpace / (1024 * 1024);
                result.CleanupLog = cleanupLog.ToList();
                result.Message = $"Cleanup completed successfully! Freed {result.SpaceFreedMB:N0} MB of disk space.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Cleanup failed: {ex.Message}";
                result.CleanupLog = cleanupLog.ToList();
            }

            return result;
        }

        private async Task CleanTempFolders()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Windows Temp folder
                    string windowsTemp = Path.GetTempPath();
                    long freed = CleanDirectory(windowsTemp, "*.*", true);
                    totalFreedSpace += freed;
                    cleanupLog.Add($"Windows Temp: {freed / (1024 * 1024):N0} MB freed");

                    // User Temp folder
                    string userTemp = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
                    freed = CleanDirectory(userTemp, "*.*", true);
                    totalFreedSpace += freed;
                    cleanupLog.Add($"User Temp: {freed / (1024 * 1024):N0} MB freed");

                    // System32 Temp
                    string system32Temp = @"C:\Windows\Temp";
                    if (Directory.Exists(system32Temp))
                    {
                        freed = CleanDirectory(system32Temp, "*.*", true);
                        totalFreedSpace += freed;
                        cleanupLog.Add($"System32 Temp: {freed / (1024 * 1024):N0} MB freed");
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Temp cleanup error: {ex.Message}");
                }
            });
        }

        private async Task CleanWindowsCache()
        {
            await Task.Run(() =>
            {
                try
                {
                    var cacheFolders = new[]
                    {
                        @"C:\Windows\SoftwareDistribution\Download",
                        @"C:\Windows\Logs",
                        @"C:\Windows\Panther",
                        @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue",
                        @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive"
                    };

                    foreach (string folder in cacheFolders)
                    {
                        if (Directory.Exists(folder))
                        {
                            long freed = CleanDirectory(folder, "*.*", true);
                            totalFreedSpace += freed;
                            cleanupLog.Add($"{Path.GetFileName(folder)}: {freed / (1024 * 1024):N0} MB freed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Windows cache cleanup error: {ex.Message}");
                }
            });
        }

        private async Task CleanBrowserCache()
        {
            await Task.Run(() =>
            {
                try
                {
                    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    var browserCaches = new[]
                    {
                        // Chrome
                        Path.Combine(userProfile, @"AppData\Local\Google\Chrome\User Data\Default\Cache"),
                        Path.Combine(userProfile, @"AppData\Local\Google\Chrome\User Data\Default\Code Cache"),
                        
                        // Firefox
                        Path.Combine(userProfile, @"AppData\Local\Mozilla\Firefox\Profiles"),
                        
                        // Edge
                        Path.Combine(userProfile, @"AppData\Local\Microsoft\Edge\User Data\Default\Cache"),
                        
                        // Internet Explorer
                        Path.Combine(userProfile, @"AppData\Local\Microsoft\Windows\INetCache")
                    };

                    foreach (string cacheFolder in browserCaches)
                    {
                        if (Directory.Exists(cacheFolder))
                        {
                            long freed = CleanDirectory(cacheFolder, "*.*", true);
                            totalFreedSpace += freed;
                            if (freed > 0)
                                cleanupLog.Add($"Browser cache: {freed / (1024 * 1024):N0} MB freed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Browser cache cleanup error: {ex.Message}");
                }
            });
        }

        private async Task CleanRecycleBin()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Use PowerShell to empty recycle bin
                    using (var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = "-Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }))
                    {
                        process?.WaitForExit(30000);
                        cleanupLog.Add("Recycle Bin emptied");
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Recycle bin cleanup error: {ex.Message}");
                }
            });
        }

        private async Task CleanWindowsLogs()
        {
            await Task.Run(() =>
            {
                try
                {
                    var logFolders = new[]
                    {
                        @"C:\Windows\Logs\CBS",
                        @"C:\Windows\Logs\DISM",
                        @"C:\Windows\Logs\DPX",
                        @"C:\Windows\Logs\MoSetup"
                    };

                    foreach (string logFolder in logFolders)
                    {
                        if (Directory.Exists(logFolder))
                        {
                            long freed = CleanDirectory(logFolder, "*.log", false);
                            totalFreedSpace += freed;
                            if (freed > 0)
                                cleanupLog.Add($"Windows logs: {freed / (1024 * 1024):N0} MB freed");
                        }
                    }

                    // Clear event logs (keep recent ones)
                    ClearOldEventLogs();
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Log cleanup error: {ex.Message}");
                }
            });
        }

        private async Task CleanPrefetchCache()
        {
            await Task.Run(() =>
            {
                try
                {
                    string prefetchPath = @"C:\Windows\Prefetch";
                    if (Directory.Exists(prefetchPath))
                    {
                        // Keep recent prefetch files (last 7 days) for boot performance
                        var files = Directory.GetFiles(prefetchPath, "*.pf")
                            .Where(f => File.GetLastWriteTime(f) < DateTime.Now.AddDays(-7));

                        long freed = 0;
                        foreach (string file in files)
                        {
                            try
                            {
                                freed += new FileInfo(file).Length;
                                File.Delete(file);
                            }
                            catch { }
                        }

                        totalFreedSpace += freed;
                        cleanupLog.Add($"Prefetch cache: {freed / (1024 * 1024):N0} MB freed");
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Prefetch cleanup error: {ex.Message}");
                }
            });
        }

        private async Task CleanThumbnailCache()
        {
            await Task.Run(() =>
            {
                try
                {
                    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string thumbsPath = Path.Combine(userProfile, @"AppData\Local\Microsoft\Windows\Explorer");

                    if (Directory.Exists(thumbsPath))
                    {
                        long freed = CleanDirectory(thumbsPath, "thumbcache_*.db", false);
                        totalFreedSpace += freed;
                        cleanupLog.Add($"Thumbnail cache: {freed / (1024 * 1024):N0} MB freed");
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Thumbnail cleanup error: {ex.Message}");
                }
            });
        }

        private async Task CleanWindowsUpdate()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Clean Windows Update cache
                    var updateFolders = new[]
                    {
                        @"C:\Windows\SoftwareDistribution\Download",
                        @"C:\Windows\System32\catroot2"
                    };

                    foreach (string folder in updateFolders)
                    {
                        if (Directory.Exists(folder))
                        {
                            long freed = CleanDirectory(folder, "*.*", true);
                            totalFreedSpace += freed;
                            if (freed > 0)
                                cleanupLog.Add($"Windows Update cache: {freed / (1024 * 1024):N0} MB freed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Windows Update cleanup error: {ex.Message}");
                }
            });
        }

        private async Task CleanGameCache()
        {
            await Task.Run(() =>
            {
                try
                {
                    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    var gameCacheFolders = new[]
                    {
                        // Steam shader cache
                        Path.Combine(userProfile, @"AppData\Local\NVIDIA Corporation\NV_Cache"),
                        Path.Combine(userProfile, @"AppData\Local\D3DSCache"),
                        Path.Combine(userProfile, @"AppData\Local\CrashDumps"),
                        
                        // Epic Games cache
                        Path.Combine(userProfile, @"AppData\Local\EpicGamesLauncher\Saved\webcache"),
                        
                        // Steam logs
                        Path.Combine(userProfile, @"AppData\Local\Steam\logs"),
                        
                        // DirectX shader cache
                        Path.Combine(userProfile, @"AppData\Local\Microsoft\DirectX\ShaderCache")
                    };

                    foreach (string folder in gameCacheFolders)
                    {
                        if (Directory.Exists(folder))
                        {
                            long freed = CleanDirectory(folder, "*.*", true);
                            totalFreedSpace += freed;
                            if (freed > 0)
                                cleanupLog.Add($"Game cache: {freed / (1024 * 1024):N0} MB freed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Game cache cleanup error: {ex.Message}");
                }
            });
        }

        private async Task RunDiskCleanup()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Run Windows Disk Cleanup utility silently
                    using (var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "cleanmgr",
                        Arguments = "/sagerun:1",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }))
                    {
                        process?.WaitForExit(60000); // 1 minute timeout
                        cleanupLog.Add("Windows Disk Cleanup completed");
                    }
                }
                catch (Exception ex)
                {
                    cleanupLog.Add($"Disk cleanup tool error: {ex.Message}");
                }
            });
        }

        private void ClearOldEventLogs()
        {
            try
            {
                var logsToClean = new[]
                {
                    "Application",
                    "System",
                    "Security"
                };

                foreach (string logName in logsToClean)
                {
                    try
                    {
                        using (var eventLog = new EventLog(logName))
                        {
                            // Only clear if log is larger than 50MB
                            if (eventLog.Entries.Count > 10000)
                            {
                                eventLog.Clear();
                                cleanupLog.Add($"Event log '{logName}' cleared");
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                cleanupLog.Add($"Event log cleanup error: {ex.Message}");
            }
        }

        private long CleanDirectory(string directoryPath, string searchPattern, bool includeSubdirectories)
        {
            long totalFreed = 0;

            try
            {
                if (!Directory.Exists(directoryPath))
                    return 0;

                SearchOption searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);

                foreach (string file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        long fileSize = fileInfo.Length;
                        File.Delete(file);
                        totalFreed += fileSize;
                    }
                    catch
                    {
                        // File might be in use, skip it
                    }
                }

                // Clean empty subdirectories if specified
                if (includeSubdirectories)
                {
                    CleanEmptyDirectories(directoryPath);
                }
            }
            catch (Exception)
            {
                // Directory might not be accessible
            }

            return totalFreed;
        }

        private void CleanEmptyDirectories(string directoryPath)
        {
            try
            {
                var subdirectories = Directory.GetDirectories(directoryPath);

                foreach (string subdirectory in subdirectories)
                {
                    CleanEmptyDirectories(subdirectory);

                    if (Directory.GetFiles(subdirectory).Length == 0 &&
                        Directory.GetDirectories(subdirectory).Length == 0)
                    {
                        try
                        {
                            Directory.Delete(subdirectory);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception) { }
        }

        public async Task<DiskSpaceInfo> GetDiskSpaceInfo()
        {
            return await Task.Run(() =>
            {
                var diskInfo = new DiskSpaceInfo();

                try
                {
                    DriveInfo[] drives = DriveInfo.GetDrives();
                    var systemDrive = drives.FirstOrDefault(d => d.Name.StartsWith("C:"));

                    if (systemDrive != null)
                    {
                        diskInfo.TotalSpaceGB = systemDrive.TotalSize / (1024 * 1024 * 1024);
                        diskInfo.FreeSpaceGB = systemDrive.AvailableFreeSpace / (1024 * 1024 * 1024);
                        diskInfo.UsedSpaceGB = diskInfo.TotalSpaceGB - diskInfo.FreeSpaceGB;
                        diskInfo.FreeSpacePercent = (double)diskInfo.FreeSpaceGB / diskInfo.TotalSpaceGB * 100;
                    }
                }
                catch (Exception ex)
                {
                    diskInfo.ErrorMessage = ex.Message;
                }

                return diskInfo;
            });
        }

        public async Task<long> CalculateCleanableSpace()
        {
            return await Task.Run(() =>
            {
                long cleanableSpace = 0;

                try
                {
                    // Calculate space that could be freed
                    var foldersToCheck = new[]
                    {
                        Path.GetTempPath(),
                        @"C:\Windows\Temp",
                        @"C:\Windows\SoftwareDistribution\Download",
                        Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)
                    };

                    foreach (string folder in foldersToCheck)
                    {
                        if (Directory.Exists(folder))
                        {
                            cleanableSpace += CalculateDirectorySize(folder);
                        }
                    }
                }
                catch (Exception) { }

                return cleanableSpace;
            });
        }

        private long CalculateDirectorySize(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    return 0;

                return Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                    .Sum(file =>
                    {
                        try
                        {
                            return new FileInfo(file).Length;
                        }
                        catch
                        {
                            return 0;
                        }
                    });
            }
            catch
            {
                return 0;
            }
        }
    }

    public class CleanupResult
    {
        public bool Success { get; set; }
        public long SpaceFreedMB { get; set; }
        public string Message { get; set; } = "";
        public List<string> CleanupLog { get; set; } = new List<string>();
    }

    public class DiskSpaceInfo
    {
        public long TotalSpaceGB { get; set; }
        public long FreeSpaceGB { get; set; }
        public long UsedSpaceGB { get; set; }
        public double FreeSpacePercent { get; set; }
        public string ErrorMessage { get; set; } = "";
    }
}