using System;
using System.Management;

namespace PCOptimizer
{
    public class SystemInfo
    {
        public string GetCPUInfo()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["Name"].ToString();
                    }
                }
            }
            catch (Exception) { }
            return "Unknown CPU";
        }

        public string GetGPUInfo()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"].ToString();
                        if (name.Contains("NVIDIA") || name.Contains("AMD") || name.Contains("Intel"))
                        {
                            return name;
                        }
                    }
                }
            }
            catch (Exception) { }
            return "Unknown GPU";
        }

        public string GetRAMInfo()
        {
            try
            {
                long totalMemory = 0;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalMemory = Convert.ToInt64(obj["TotalPhysicalMemory"]);
                        break;
                    }
                }
                return $"{totalMemory / (1024 * 1024 * 1024)} GB";
            }
            catch (Exception) { }
            return "Unknown RAM";
        }

        public string GetOSInfo()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return $"{obj["Caption"]} ({obj["Version"]})";
                    }
                }
            }
            catch (Exception) { }
            return Environment.OSVersion.ToString();
        }

        public string GetMotherboardInfo()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return $"{obj["Manufacturer"]} {obj["Product"]}";
                    }
                }
            }
            catch (Exception) { }
            return "Unknown Motherboard";
        }

        public bool IsNVIDIAGraphics()
        {
            return GetGPUInfo().ToLower().Contains("nvidia");
        }

        public bool IsAMDGraphics()
        {
            return GetGPUInfo().ToLower().Contains("amd");
        }

        public bool IsIntelGraphics()
        {
            return GetGPUInfo().ToLower().Contains("intel");
        }

        public int GetCoreCount()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return Convert.ToInt32(obj["NumberOfCores"]);
                    }
                }
            }
            catch (Exception) { }
            return Environment.ProcessorCount;
        }

        public string GetWindowsVersion()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT BuildNumber FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string buildNumber = obj["BuildNumber"].ToString();
                        int build = int.Parse(buildNumber);

                        if (build >= 22000)
                            return "Windows 11";
                        else if (build >= 10240)
                            return "Windows 10";
                        else
                            return "Windows (Older Version)";
                    }
                }
            }
            catch (Exception) { }
            return "Unknown Windows Version";
        }
    }
}