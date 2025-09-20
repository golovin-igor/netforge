using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Services
{
    /// <summary>
    /// Service interface for device information operations
    /// </summary>
    public interface IDeviceInformationService
    {
        /// <summary>
        /// Get comprehensive device version information
        /// </summary>
        /// <param name="device">The device to get information for</param>
        /// <returns>Device version data</returns>
        DeviceVersionData GetDeviceVersion(INetworkDevice device);

        /// <summary>
        /// Get device system information
        /// </summary>
        /// <param name="device">The device to get information for</param>
        /// <returns>System information data</returns>
        DeviceSystemInfo GetSystemInfo(INetworkDevice device);

        /// <summary>
        /// Get device hardware information
        /// </summary>
        /// <param name="device">The device to get information for</param>
        /// <returns>Hardware information data</returns>
        DeviceHardwareInfo GetHardwareInfo(INetworkDevice device);

        /// <summary>
        /// Calculate device uptime
        /// </summary>
        /// <param name="device">The device to calculate uptime for</param>
        /// <returns>Device uptime</returns>
        TimeSpan GetUptime(INetworkDevice device);

        /// <summary>
        /// Get device memory information
        /// </summary>
        /// <param name="device">The device to get memory info for</param>
        /// <returns>Memory usage information</returns>
        DeviceMemoryInfo GetMemoryInfo(INetworkDevice device);

        /// <summary>
        /// Get device flash/storage information
        /// </summary>
        /// <param name="device">The device to get storage info for</param>
        /// <returns>Storage information</returns>
        DeviceStorageInfo GetStorageInfo(INetworkDevice device);

        /// <summary>
        /// Get device boot information
        /// </summary>
        /// <param name="device">The device to get boot info for</param>
        /// <returns>Boot information</returns>
        DeviceBootInfo GetBootInfo(INetworkDevice device);
    }

    /// <summary>
    /// Device version information data
    /// </summary>
    public class DeviceVersionData
    {
        public string DeviceName { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SoftwareVersion { get; set; } = string.Empty;
        public string BuildNumber { get; set; } = string.Empty;
        public string BuildDate { get; set; } = string.Empty;
        public TimeSpan Uptime { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public DeviceSystemInfo SystemInfo { get; set; } = new();
        public DeviceHardwareInfo HardwareInfo { get; set; } = new();
        public DeviceMemoryInfo MemoryInfo { get; set; } = new();
        public DeviceStorageInfo StorageInfo { get; set; } = new();
        public DeviceBootInfo BootInfo { get; set; } = new();
    }

    /// <summary>
    /// Device system information
    /// </summary>
    public class DeviceSystemInfo
    {
        public string Hostname { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public DateTime SystemTime { get; set; } = DateTime.UtcNow;
        public string TimeZone { get; set; } = "UTC";
        public DateTime LastRestart { get; set; }
        public string RestartReason { get; set; } = "power-on";
        public int ProcessorCount { get; set; } = 1;
    }

    /// <summary>
    /// Device hardware information
    /// </summary>
    public class DeviceHardwareInfo
    {
        public string ChassisType { get; set; } = "Generic";
        public string ProcessorType { get; set; } = "Generic CPU";
        public int ProcessorSpeed { get; set; } = 1000; // MHz
        public List<string> InstalledModules { get; set; } = new();
        public Dictionary<string, string> BoardInfo { get; set; } = new();
        public string BaseMacAddress { get; set; } = "00:1A:2B:3C:4D:5E";
        public int InterfaceCount { get; set; }
    }

    /// <summary>
    /// Device memory information
    /// </summary>
    public class DeviceMemoryInfo
    {
        public long TotalMemory { get; set; } = 536870912; // 512MB default
        public long AvailableMemory { get; set; } = 402653184; // 384MB available
        public long UsedMemory => TotalMemory - AvailableMemory;
        public double UsedPercentage => (double)UsedMemory / TotalMemory * 100;
        public long ProcessorMemory { get; set; } = 67108864; // 64MB for processor
        public long IOMemory { get; set; } = 33554432; // 32MB for I/O
    }

    /// <summary>
    /// Device storage/flash information
    /// </summary>
    public class DeviceStorageInfo
    {
        public string FlashType { get; set; } = "CompactFlash";
        public long TotalFlashSize { get; set; } = 134217728; // 128MB default
        public long AvailableFlashSize { get; set; } = 67108864; // 64MB available
        public long UsedFlashSize => TotalFlashSize - AvailableFlashSize;
        public List<string> FlashContents { get; set; } = new();
        public string BootFlash { get; set; } = "bootflash:";
        public string ConfigRegister { get; set; } = "0x2102";
    }

    /// <summary>
    /// Device boot information
    /// </summary>
    public class DeviceBootInfo
    {
        public string BootDevice { get; set; } = "flash";
        public string BootImage { get; set; } = "system.bin";
        public string ConfigurationFile { get; set; } = "startup-config";
        public List<string> BootSequence { get; set; } = new();
        public TimeSpan BootTime { get; set; } = TimeSpan.FromSeconds(45);
        public string LastBootReason { get; set; } = "power-on";
        public int BootCount { get; set; } = 1;
    }
}