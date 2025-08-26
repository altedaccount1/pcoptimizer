using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace PCOptimizer.Security
{
    public static class SecurityManager
    {
        private static readonly string[] BLACKLISTED_PROCESSES = {
            "ollydbg", "x64dbg", "windbg", "ida", "ida64", "idaq", "idaq64",
            "immunitydebugger", "wireshark", "fiddler", "cheatengine",
            "processhacker", "processmonitor", "procmon", "regmon", "filemon",
            "apimonitor", "detours", "injection", "dnspy", "reflexil", "dumper"
        };

        private static readonly string[] VM_ARTIFACTS = {
            "vmware", "virtualbox", "vbox", "qemu", "xen", "hyper-v", "parallels"
        };

        private static readonly string[] SUSPICIOUS_MODULES = {
            "sbiedll", "dbghelp", "api-ms-win-core-debug", "detours", "easyhook"
        };

        // P/Invoke declarations for advanced checks
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll")]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
            ref IntPtr processInformation, int processInformationLength, ref int returnLength);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();

        public static SecurityCheckResult PerformSecurityChecks()
        {
            var result = new SecurityCheckResult();

            try
            {
                // 1. Anti-debugging checks
                result.IsDebuggerDetected = DetectDebugging();

                // 2. Virtual machine detection
                result.IsVirtualMachine = DetectVirtualMachine();

                // 3. Process monitoring detection
                result.IsMonitoringDetected = DetectProcessMonitoring();

                // 4. File integrity check
                result.IsFileIntegrityValid = CheckFileIntegrity();

                // 5. Suspicious modules check
                result.HasSuspiciousModules = DetectSuspiciousModules();

                // 6. Timing attack detection
                result.IsTimingAnomalyDetected = DetectTimingAnomaly();

                // 7. Memory patching detection
                result.IsMemoryPatched = DetectMemoryPatching();

                result.IsSafe = !result.IsDebuggerDetected &&
                               !result.IsVirtualMachine &&
                               !result.IsMonitoringDetected &&
                               result.IsFileIntegrityValid &&
                               !result.HasSuspiciousModules &&
                               !result.IsTimingAnomalyDetected &&
                               !result.IsMemoryPatched;
            }
            catch (Exception)
            {
                // If security checks fail, assume unsafe
                result.IsSafe = false;
            }

            return result;
        }

        private static bool DetectDebugging()
        {
            try
            {
                // 1. IsDebuggerPresent API
                if (IsDebuggerPresent())
                    return true;

                // 2. CheckRemoteDebuggerPresent
                bool isRemoteDebuggerPresent = false;
                CheckRemoteDebuggerPresent(GetCurrentProcess(), ref isRemoteDebuggerPresent);
                if (isRemoteDebuggerPresent)
                    return true;

                // 3. Managed debugger check
                if (Debugger.IsAttached)
                    return true;

                // 4. NtQueryInformationProcess check
                IntPtr debugPort = IntPtr.Zero;
                int returnLength = 0;
                int status = NtQueryInformationProcess(GetCurrentProcess(), 7, ref debugPort, IntPtr.Size, ref returnLength);
                if (status == 0 && debugPort != IntPtr.Zero)
                    return true;

                // 5. Parent process check (common debugger parent processes)
                string parentProcess = GetParentProcessName();
                string[] debuggerParents = { "devenv", "vstesthost", "testhost", "vstest", "dotnet" };
                if (debuggerParents.Any(p => parentProcess.ToLower().Contains(p)))
                    return true;

                return false;
            }
            catch (Exception)
            {
                return true; // Assume debugger if checks fail
            }
        }

        private static bool DetectVirtualMachine()
        {
            try
            {
                // 1. Check system manufacturer
                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString().ToLower() ?? "";
                        string model = obj["Model"]?.ToString().ToLower() ?? "";

                        if (VM_ARTIFACTS.Any(vm => manufacturer.Contains(vm) || model.Contains(vm)))
                            return true;
                    }
                }

                // 2. Check BIOS
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber, SMBIOSBIOSVersion FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string serial = obj["SerialNumber"]?.ToString().ToLower() ?? "";
                        string version = obj["SMBIOSBIOSVersion"]?.ToString().ToLower() ?? "";

                        if (VM_ARTIFACTS.Any(vm => serial.Contains(vm) || version.Contains(vm)))
                            return true;
                    }
                }

                // 3. Check for VM-specific files
                string[] vmFiles = {
                    @"C:\windows\system32\drivers\vmmouse.sys",
                    @"C:\windows\system32\drivers\vmhgfs.sys",
                    @"C:\windows\system32\drivers\VBoxMouse.sys",
                    @"C:\windows\system32\drivers\VBoxGuest.sys",
                    @"C:\windows\system32\vboxdisp.dll",
                    @"C:\windows\system32\vboxhook.dll"
                };

                if (vmFiles.Any(File.Exists))
                    return true;

                // 4. Check registry for VM artifacts
                string[] vmRegKeys = {
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\VMware, Inc.\VMware Tools",
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Oracle\VirtualBox Guest Additions"
                };

                foreach (string keyPath in vmRegKeys)
                {
                    try
                    {
                        string[] parts = keyPath.Split('\\');
                        RegistryKey baseKey = parts[0] == "HKEY_LOCAL_MACHINE" ? Registry.LocalMachine : Registry.CurrentUser;
                        string subKey = string.Join("\\", parts.Skip(1));

                        using (var key = baseKey.OpenSubKey(subKey))
                        {
                            if (key != null)
                                return true;
                        }
                    }
                    catch (Exception) { }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool DetectProcessMonitoring()
        {
            try
            {
                Process[] processes = Process.GetProcesses();

                foreach (Process process in processes)
                {
                    string processName = process.ProcessName.ToLower();
                    if (BLACKLISTED_PROCESSES.Any(bp => processName.Contains(bp)))
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool CheckFileIntegrity()
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;

                // Calculate file hash
                using (var sha256 = SHA256.Create())
                {
                    byte[] fileBytes = File.ReadAllBytes(exePath);
                    byte[] hash = sha256.ComputeHash(fileBytes);
                    string currentHash = Convert.ToBase64String(hash);

                    // In production, compare against known good hash
                    // For now, just check if file exists and is readable
                    return File.Exists(exePath) && fileBytes.Length > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool DetectSuspiciousModules()
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();

                foreach (ProcessModule module in currentProcess.Modules)
                {
                    string moduleName = module.ModuleName.ToLower();
                    if (SUSPICIOUS_MODULES.Any(sm => moduleName.Contains(sm)))
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool DetectTimingAnomaly()
        {
            try
            {
                // Measure execution time of simple operations
                uint start = GetTickCount();

                // Perform some calculations
                for (int i = 0; i < 1000; i++)
                {
                    Math.Sqrt(i * 3.14159);
                }

                uint end = GetTickCount();
                uint elapsed = end - start;

                // If execution took too long, might be under analysis
                return elapsed > 100; // 100ms threshold
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool DetectMemoryPatching()
        {
            try
            {
                // Check if critical methods have been patched
                var assembly = Assembly.GetExecutingAssembly();
                var types = assembly.GetTypes();

                // Look for unexpected method modifications
                foreach (var type in types.Take(5)) // Check first 5 types
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                    foreach (var method in methods.Take(10)) // Check first 10 methods per type
                    {
                        try
                        {
                            // Get method body
                            var body = method.GetMethodBody();
                            if (body != null)
                            {
                                byte[] il = body.GetILAsByteArray();

                                // Look for suspicious IL patterns that might indicate patching
                                if (il != null && il.Length > 0)
                                {
                                    // Check for NOP sleds or unusual patterns
                                    int nopCount = il.Count(b => b == 0x00); // NOP instruction
                                    if (nopCount > il.Length / 2)
                                        return true;
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetParentProcessName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {Process.GetCurrentProcess().Id}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        int parentPid = Convert.ToInt32(obj["ParentProcessId"]);
                        Process parentProcess = Process.GetProcessById(parentPid);
                        return parentProcess.ProcessName;
                    }
                }
            }
            catch (Exception) { }

            return "";
        }

        public static void ExitIfUnsafe(SecurityCheckResult result)
        {
            if (!result.IsSafe)
            {
                // Silently exit without revealing why
                Environment.Exit(0);
            }
        }

        public static string GenerateIntegrityChecksum()
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                using (var sha256 = SHA256.Create())
                {
                    byte[] fileBytes = File.ReadAllBytes(exePath);
                    byte[] hash = sha256.ComputeHash(fileBytes);
                    return Convert.ToBase64String(hash);
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }

    public class SecurityCheckResult
    {
        public bool IsSafe { get; set; }
        public bool IsDebuggerDetected { get; set; }
        public bool IsVirtualMachine { get; set; }
        public bool IsMonitoringDetected { get; set; }
        public bool IsFileIntegrityValid { get; set; }
        public bool HasSuspiciousModules { get; set; }
        public bool IsTimingAnomalyDetected { get; set; }
        public bool IsMemoryPatched { get; set; }
    }
}