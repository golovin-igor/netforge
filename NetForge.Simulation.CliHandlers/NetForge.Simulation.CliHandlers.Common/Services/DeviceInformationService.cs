using NetForge.Interfaces.Devices;

namespace NetForge.Simulation.Common.CLI.Services
{
    /// <summary>
    /// Concrete implementation of device information service
    /// </summary>
    public class DeviceInformationService : IDeviceInformationService
    {
        private readonly Random _random = new();
        private static readonly DateTime _serviceStartTime = DateTime.UtcNow;

        public DeviceVersionData GetDeviceVersion(INetworkDevice device)
        {
            var systemInfo = GetSystemInfo(device);
            var hardwareInfo = GetHardwareInfo(device);
            var memoryInfo = GetMemoryInfo(device);
            var storageInfo = GetStorageInfo(device);
            var bootInfo = GetBootInfo(device);

            return new DeviceVersionData
            {
                DeviceName = device.Name ?? "Unknown",
                Vendor = device.Vendor ?? "Unknown",
                Model = GetDeviceModel(device),
                SoftwareVersion = GetSoftwareVersion(device),
                BuildNumber = GenerateBuildNumber(device),
                BuildDate = GenerateBuildDate(device),
                Uptime = GetUptime(device),
                SerialNumber = GenerateSerialNumber(device),
                SystemInfo = systemInfo,
                HardwareInfo = hardwareInfo,
                MemoryInfo = memoryInfo,
                StorageInfo = storageInfo,
                BootInfo = bootInfo
            };
        }

        public DeviceSystemInfo GetSystemInfo(INetworkDevice device)
        {
            var uptime = GetUptime(device);
            var lastRestart = DateTime.UtcNow - uptime;

            return new DeviceSystemInfo
            {
                Hostname = device.Name ?? "Unknown",
                Domain = "example.com",
                SystemTime = DateTime.UtcNow,
                TimeZone = "UTC",
                LastRestart = lastRestart,
                RestartReason = "power-on",
                ProcessorCount = GetProcessorCount(device)
            };
        }

        public DeviceHardwareInfo GetHardwareInfo(INetworkDevice device)
        {
            var interfaceCount = 0;
            if (device is IInterfaceManager interfaceManager)
            {
                interfaceCount = interfaceManager.GetAllInterfaces().Count;
            }

            return new DeviceHardwareInfo
            {
                ChassisType = GetChassisType(device),
                ProcessorType = GetProcessorType(device),
                ProcessorSpeed = GetProcessorSpeed(device),
                InstalledModules = GetInstalledModules(device),
                BoardInfo = GetBoardInfo(device),
                BaseMacAddress = GenerateBaseMacAddress(device),
                InterfaceCount = interfaceCount
            };
        }

        public TimeSpan GetUptime(INetworkDevice device)
        {
            // Simulate realistic uptime based on device name hash for consistency
            var hash = device.Name?.GetHashCode() ?? 0;
            var baseDays = Math.Abs(hash % 30) + 1; // 1-30 days
            var hours = Math.Abs(hash % 24);
            var minutes = Math.Abs(hash % 60);

            return new TimeSpan(baseDays, hours, minutes, 0);
        }

        public DeviceMemoryInfo GetMemoryInfo(INetworkDevice device)
        {
            var memoryMultiplier = GetMemoryMultiplier(device);

            return new DeviceMemoryInfo
            {
                TotalMemory = 536870912L * memoryMultiplier, // Base 512MB
                AvailableMemory = 402653184L * memoryMultiplier, // Base 384MB available
                ProcessorMemory = 67108864L * memoryMultiplier, // Base 64MB for processor
                IOMemory = 33554432L * memoryMultiplier // Base 32MB for I/O
            };
        }

        public DeviceStorageInfo GetStorageInfo(INetworkDevice device)
        {
            var storageMultiplier = GetStorageMultiplier(device);
            var contents = GenerateFlashContents(device);

            return new DeviceStorageInfo
            {
                FlashType = GetFlashType(device),
                TotalFlashSize = 134217728L * storageMultiplier, // Base 128MB
                AvailableFlashSize = 67108864L * storageMultiplier, // Base 64MB available
                FlashContents = contents,
                BootFlash = "bootflash:",
                ConfigRegister = "0x2102"
            };
        }

        public DeviceBootInfo GetBootInfo(INetworkDevice device)
        {
            return new DeviceBootInfo
            {
                BootDevice = "flash",
                BootImage = GetBootImage(device),
                ConfigurationFile = "startup-config",
                BootSequence = GetBootSequence(device),
                BootTime = TimeSpan.FromSeconds(45 + _random.Next(0, 30)), // 45-75 seconds
                LastBootReason = "power-on",
                BootCount = GenerateBootCount(device)
            };
        }

        #region Private Helper Methods

        private string GetDeviceModel(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" => "Catalyst 2960-24TT-L",
                "juniper" => "EX2200-24T-4G",
                "nokia" => "7750 SR-1",
                "alcatel" => "OmniSwitch 6850",
                "extreme" => "Summit X440-24t",
                "arista" => "DCS-7050T-36",
                "huawei" => "S5700-28C-HI",
                "dell" => "PowerConnect 6248",
                "mikrotik" => "RouterBoard RB750",
                "fortinet" => "FortiSwitch 124D",
                "aruba" => "2930F-24G-4SFP+",
                "f5" => "BIG-IP 1600",
                _ => "Generic Switch"
            };
        }

        private string GetSoftwareVersion(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" => "15.2(4)E7",
                "juniper" => "12.3R12-S13",
                "nokia" => "TiMOS-B-20.10.R6",
                "alcatel" => "AOS 8.6.1.1226.R02 GA",
                "extreme" => "ExtremeXOS 22.7.1.4",
                "arista" => "4.20.1F",
                "huawei" => "V200R010C00SPC600",
                "dell" => "6.3.1.3",
                "mikrotik" => "6.48.6",
                "fortinet" => "6.0.7",
                "aruba" => "16.10.0008",
                "f5" => "13.1.3.4",
                _ => "1.0.0"
            };
        }

        private string GenerateBuildNumber(INetworkDevice device)
        {
            var hash = device.Name?.GetHashCode() ?? 0;
            var buildNum = Math.Abs(hash % 9999) + 1000; // 1000-9999
            return buildNum.ToString();
        }

        private string GenerateBuildDate(INetworkDevice device)
        {
            var baseDate = new DateTime(2023, 1, 1);
            var hash = device.Name?.GetHashCode() ?? 0;
            var daysOffset = Math.Abs(hash % 365); // Within last year
            return baseDate.AddDays(daysOffset).ToString("MMM dd yyyy");
        }

        private string GenerateSerialNumber(INetworkDevice device)
        {
            var hash = device.Name?.GetHashCode() ?? 0;
            var serial = $"FCW{Math.Abs(hash % 99999):D5}X{Math.Abs(hash % 999):D3}";
            return serial;
        }

        private int GetProcessorCount(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" or "juniper" or "arista" => 2,
                "nokia" or "f5" => 4,
                _ => 1
            };
        }

        private string GetChassisType(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" => "WS-C2960-24TT-L",
                "juniper" => "EX2200-24T-4G",
                "nokia" => "7750-SR-1",
                _ => "Generic Chassis"
            };
        }

        private string GetProcessorType(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" => "PowerPC405",
                "juniper" => "MIPS64",
                "nokia" => "Intel x86_64",
                "f5" => "Intel Xeon",
                _ => "Generic CPU"
            };
        }

        private int GetProcessorSpeed(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" => 266, // MHz
                "juniper" => 533,
                "nokia" => 1800,
                "f5" => 2400,
                _ => 1000
            };
        }

        private List<string> GetInstalledModules(INetworkDevice device)
        {
            var modules = new List<string>();

            if (device is IInterfaceManager interfaceManager)
            {
                var interfaceCount = interfaceManager.GetAllInterfaces().Count;
                modules.Add($"{interfaceCount}-Port Ethernet Module");
            }

            modules.Add("System Management Module");
            modules.Add("Power Supply Module");

            return modules;
        }

        private Dictionary<string, string> GetBoardInfo(INetworkDevice device)
        {
            return new Dictionary<string, string>
            {
                ["Main Board"] = GetChassisType(device),
                ["Revision"] = "1.0",
                ["Part Number"] = GeneratePartNumber(device),
                ["Serial Number"] = GenerateSerialNumber(device)
            };
        }

        private string GenerateBaseMacAddress(INetworkDevice device)
        {
            var hash = device.Name?.GetHashCode() ?? 0;
            var bytes = BitConverter.GetBytes(hash);
            return $"00:1A:{bytes[0]:X2}:{bytes[1]:X2}:{bytes[2]:X2}:{bytes[3]:X2}";
        }

        private int GetMemoryMultiplier(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "f5" or "nokia" => 8, // High-end devices
                "cisco" or "juniper" or "arista" => 2, // Mid-range
                _ => 1 // Basic devices
            };
        }

        private int GetStorageMultiplier(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "f5" or "nokia" => 4, // High-end devices
                "cisco" or "juniper" => 2, // Mid-range
                _ => 1 // Basic devices
            };
        }

        private string GetFlashType(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" => "CompactFlash",
                "juniper" => "USB",
                "nokia" => "eMMC",
                _ => "Flash"
            };
        }

        private List<string> GenerateFlashContents(INetworkDevice device)
        {
            var contents = new List<string>
            {
                GetBootImage(device),
                "startup-config",
                "vlan.dat"
            };

            // Add vendor-specific files
            switch (device.Vendor?.ToLower())
            {
                case "cisco":
                    contents.AddRange(["info", "env_vars", "multiple-fs"]);
                    break;
                case "juniper":
                    contents.AddRange(["junos-install-media-usb.tgz", "config.rescue"]);
                    break;
                case "nokia":
                    contents.AddRange(["bof.cfg", "config.cfg", "boot.tim"]);
                    break;
            }

            return contents;
        }

        private string GetBootImage(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" => "c2960-lanbasek9-mz.152-4.E7.bin",
                "juniper" => "junos-12.3R12-S13-domestic.tgz",
                "nokia" => "TiMOS-B-20.10.R6.bin",
                _ => "system.bin"
            };
        }

        private List<string> GetBootSequence(INetworkDevice device)
        {
            return device.Vendor?.ToLower() switch
            {
                "cisco" =>
                [
                    "System Bootstrap, Version 12.2(46r)EX",
                    "Loading flash:c2960-lanbasek9-mz.152-4.E7.bin",
                    "Starting system services",
                    "Initializing interfaces"
                ],
                "juniper" =>
                [
                    "FreeBSD/MIPS boot loader",
                    "Loading /boot/loader",
                    "Loading kernel",
                    "Starting Junos"
                ],
                _ =>
                [
                    "System boot loader",
                    "Loading system image",
                    "Starting system"
                ]
            };
        }

        private int GenerateBootCount(INetworkDevice device)
        {
            var hash = device.Name?.GetHashCode() ?? 0;
            return Math.Abs(hash % 50) + 1; // 1-50 boots
        }

        private string GeneratePartNumber(INetworkDevice device)
        {
            var hash = device.Name?.GetHashCode() ?? 0;
            return $"73-{Math.Abs(hash % 99999):D5}-{Math.Abs(hash % 99):D2}";
        }

        #endregion
    }
}