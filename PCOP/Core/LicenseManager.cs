using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.DataProtection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Management;
using System.IO;
using System.Linq;

namespace PCOptimizer.Security
{
    public class ProductionLicenseManager : IDisposable
    {
        private const string LICENSE_SERVER_URL = "https://pcoptimzer-licensing-gvema2b0d0g0b9et.eastus-01.azurewebsites.net/api/license";
        private const string REGISTRY_KEY = @"SOFTWARE\PCOptimizer";
        private const string LICENSE_VALUE = "SecureLicense";
        private const string HARDWARE_VALUE = "HardwareFingerprint";

        private string hardwareFingerprint;
        private LicenseInfo currentLicense;
        private readonly HttpClient httpClient;

        public ProductionLicenseManager()
        {
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PCOptimizer/1.0");

            hardwareFingerprint = GenerateSecureHardwareFingerprint();
            LoadStoredLicense();
        }

        private string GenerateSecureHardwareFingerprint()
        {
            var components = new List<string>();

            try
            {
                // CPU Serial Number
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var cpuId = obj["ProcessorId"]?.ToString();
                        if (!string.IsNullOrEmpty(cpuId))
                        {
                            components.Add($"CPU:{cpuId}");
                            break;
                        }
                    }
                }

                // Motherboard Serial
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var serial = obj["SerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(serial) && serial != "To be filled by O.E.M.")
                        {
                            components.Add($"MB:{serial}");
                            break;
                        }
                    }
                }

                // Primary MAC Address
                using (var searcher = new ManagementObjectSearcher("SELECT MACAddress FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL"))
                {
                    var macAddresses = new List<string>();
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var mac = obj["MACAddress"]?.ToString();
                        if (!string.IsNullOrEmpty(mac))
                        {
                            macAddresses.Add(mac);
                        }
                    }

                    if (macAddresses.Any())
                    {
                        var primaryMac = macAddresses.OrderBy(x => x).First();
                        components.Add($"MAC:{primaryMac}");
                    }
                }
            }
            catch (Exception)
            {
                // Fallback fingerprint
                components.Add($"MACHINE:{Environment.MachineName}");
                components.Add($"USER:{Environment.UserName}");
            }

            // Create secure hash
            string combined = string.Join("|", components);
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined + "PCOpt_Salt_2024"));
                return Convert.ToBase64String(hash).Substring(0, 24);
            }
        }

        public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey = null)
        {
            try
            {
                // Use provided key or stored key
                string keyToValidate = licenseKey ?? currentLicense?.LicenseKey;

                if (string.IsNullOrEmpty(keyToValidate))
                {
                    return new LicenseValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "No license key available"
                    };
                }

                var validationRequest = new LicenseValidationRequest
                {
                    LicenseKey = keyToValidate,
                    HardwareFingerprint = hardwareFingerprint,
                    ProductVersion = GetProductVersion(),
                    MachineName = Environment.MachineName
                };

                var json = JsonSerializer.Serialize(validationRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{LICENSE_SERVER_URL}/validate", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LicenseValidationResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (result.IsValid)
                    {
                        var licenseInfo = new LicenseInfo
                        {
                            LicenseKey = keyToValidate,
                            CustomerName = result.CustomerName,
                            ExpirationDate = result.ExpirationDate,
                            ValidationDate = DateTime.UtcNow
                        };

                        StoreLicenseSecurely(licenseInfo);
                        currentLicense = licenseInfo;

                        return new LicenseValidationResult
                        {
                            IsValid = true,
                            LicenseInfo = licenseInfo
                        };
                    }
                    else
                    {
                        // Clear invalid license
                        InvalidateLicense();
                        return new LicenseValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = result.ErrorMessage
                        };
                    }
                }

                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Server communication failed"
                };
            }
            catch (HttpRequestException)
            {
                // Network error - try offline validation
                return ValidateOffline(licenseKey);
            }
            catch (Exception ex)
            {
                return new LicenseValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Validation error: {ex.Message}"
                };
            }
        }

        private LicenseValidationResult ValidateOffline(string licenseKey)
        {
            // Allow offline validation for up to 24 hours
            if (currentLicense != null &&
                (licenseKey == null || currentLicense.LicenseKey == licenseKey) &&
                DateTime.UtcNow < currentLicense.ExpirationDate &&
                DateTime.UtcNow.Subtract(currentLicense.ValidationDate).TotalHours < 24)
            {
                return new LicenseValidationResult
                {
                    IsValid = true,
                    LicenseInfo = currentLicense,
                    IsOfflineValidation = true
                };
            }

            return new LicenseValidationResult
            {
                IsValid = false,
                ErrorMessage = "License validation required - please check internet connection"
            };
        }

        private void StoreLicenseSecurely(LicenseInfo licenseInfo)
        {
            try
            {
                string serialized = JsonSerializer.Serialize(licenseInfo);

                // Simple XOR encryption instead of ProtectedData for compatibility
                byte[] data = Encoding.UTF8.GetBytes(serialized);
                byte[] key = Encoding.UTF8.GetBytes(hardwareFingerprint.PadRight(32).Substring(0, 32));

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ key[i % key.Length]);
                }

                string encryptedBase64 = Convert.ToBase64String(data);

                using (var regKey = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    regKey.SetValue(LICENSE_VALUE, encryptedBase64);
                    regKey.SetValue(HARDWARE_VALUE, hardwareFingerprint);
                }
            }
            catch (Exception)
            {
                // Fallback to file storage
                StoreInFile(licenseInfo);
            }
        }

        private void StoreInFile(LicenseInfo licenseInfo)
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PCOptimizer");
                Directory.CreateDirectory(appDataPath);

                string filePath = Path.Combine(appDataPath, ".license");
                string serialized = JsonSerializer.Serialize(licenseInfo);

                // Simple XOR encryption
                byte[] data = Encoding.UTF8.GetBytes(serialized);
                byte[] key = Encoding.UTF8.GetBytes(hardwareFingerprint.PadRight(32).Substring(0, 32));

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ key[i % key.Length]);
                }

                File.WriteAllBytes(filePath, data);
            }
            catch (Exception) { }
        }

        private void LoadStoredLicense()
        {
            try
            {
                // Try registry first
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY))
                {
                    if (key != null)
                    {
                        string encryptedData = key.GetValue(LICENSE_VALUE) as string;
                        string storedFingerprint = key.GetValue(HARDWARE_VALUE) as string;

                        if (!string.IsNullOrEmpty(encryptedData) && storedFingerprint == hardwareFingerprint)
                        {
                            currentLicense = DecryptLicense(encryptedData);
                            return;
                        }
                    }
                }

                // Try file backup
                LoadFromFile();
            }
            catch (Exception) { }
        }

        private void LoadFromFile()
        {
            try
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PCOptimizer", ".license");

                if (File.Exists(filePath))
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    byte[] key = Encoding.UTF8.GetBytes(hardwareFingerprint.PadRight(32).Substring(0, 32));

                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)(data[i] ^ key[i % key.Length]);
                    }

                    string json = Encoding.UTF8.GetString(data);
                    currentLicense = JsonSerializer.Deserialize<LicenseInfo>(json);
                }
            }
            catch (Exception) { }
        }

        private LicenseInfo DecryptLicense(string encryptedData)
        {
            try
            {
                byte[] data = Convert.FromBase64String(encryptedData);
                byte[] key = Encoding.UTF8.GetBytes(hardwareFingerprint.PadRight(32).Substring(0, 32));

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ key[i % key.Length]);
                }

                string json = Encoding.UTF8.GetString(data);
                return JsonSerializer.Deserialize<LicenseInfo>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public LicenseInfo GetCurrentLicense()
        {
            return currentLicense;
        }

        public string GetHardwareFingerprint()
        {
            return hardwareFingerprint;
        }

        public void InvalidateLicense()
        {
            currentLicense = null;

            // Clear stored license
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true))
                {
                    key?.DeleteValue(LICENSE_VALUE, false);
                    key?.DeleteValue(HARDWARE_VALUE, false);
                }
            }
            catch (Exception) { }

            try
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PCOptimizer", ".license");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception) { }
        }

        private string GetProductVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }

    // Data classes
    public class LicenseInfo
    {
        public string LicenseKey { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public DateTime ExpirationDate { get; set; }
        public DateTime ValidationDate { get; set; }
    }

    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = "";
        public LicenseInfo LicenseInfo { get; set; }
        public bool IsOfflineValidation { get; set; }
    }

    public class LicenseValidationRequest
    {
        public string LicenseKey { get; set; } = "";
        public string HardwareFingerprint { get; set; } = "";
        public string ProductVersion { get; set; } = "";
        public string MachineName { get; set; } = "";
    }

    public class LicenseValidationResponse
    {
        public bool IsValid { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime ExpirationDate { get; set; }
        public int RemainingActivations { get; set; }
        public string ErrorMessage { get; set; } = "";
    }
}