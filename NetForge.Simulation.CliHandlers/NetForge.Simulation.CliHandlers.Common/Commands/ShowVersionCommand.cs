using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.CLI.Formatters;
using NetForge.Simulation.Common.CLI.Handlers;
using NetForge.Simulation.Common.CLI.Services;

namespace NetForge.Simulation.Common.CLI.Commands
{
    /// <summary>
    /// Vendor-agnostic show version command that contains only business logic
    /// </summary>
    public class ShowVersionCommand : NetworkCommand
    {
        private readonly IDeviceInformationService _deviceInfoService;

        public ShowVersionCommand(IDeviceInformationService deviceInfoService)
        {
            _deviceInfoService = deviceInfoService;
        }

        public override string CommandName => "show version";
        public override string Description => "Display device version and system information";

        protected override async Task<CommandData> ExecuteBusinessLogicAsync(INetworkDevice device, string[] args)
        {
            try
            {
                // Get comprehensive device version information using business service
                var versionData = _deviceInfoService.GetDeviceVersion(device);

                return new ShowVersionCommandData(versionData);
            }
            catch (Exception ex)
            {
                throw new CommandExecutionException($"Failed to retrieve device version information: {ex.Message}", ex);
            }
        }

        public override string GetHelpText()
        {
            return "Display device version and system information\n" +
                   "Usage: show version\n" +
                   "Shows:\n" +
                   "  - Software version and build information\n" +
                   "  - Hardware details and serial numbers\n" +
                   "  - System uptime and memory usage\n" +
                   "  - Boot information and configuration\n" +
                   "  - Installed modules and interfaces";
        }

        public override IEnumerable<string> GetCompletions(string partial)
        {
            // Show version typically doesn't have additional parameters
            // But some vendors support modifiers
            var completions = new List<string>
            {
                "brief",     // Some vendors support brief output
                "detail",    // Some vendors support detailed output
                "hardware",  // Hardware-specific version info
                "software"   // Software-specific version info
            };

            return completions.Where(c => c.StartsWith(partial, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Show version command result data
    /// </summary>
    public class ShowVersionCommandData : CommandData
    {
        public DeviceVersionData VersionData { get; }

        public ShowVersionCommandData(DeviceVersionData versionData)
        {
            VersionData = versionData;
            Success = true;
        }
    }

    /// <summary>
    /// Factory for creating show version handlers
    /// </summary>
    public static class ShowVersionHandlerFactory
    {
        /// <summary>
        /// Create a unified show version handler for a specific vendor
        /// </summary>
        public static UnifiedCliHandler CreateShowVersionHandler(
            IDeviceInformationService deviceInfoService,
            IVendorCommandFormatter formatter)
        {
            var showVersionCommand = new ShowVersionCommand(deviceInfoService);
            return new UnifiedCliHandler(showVersionCommand, formatter);
        }

        /// <summary>
        /// Create a unified show version handler with default formatter
        /// </summary>
        public static UnifiedCliHandler CreateShowVersionHandler(IDeviceInformationService deviceInfoService)
        {
            var showVersionCommand = new ShowVersionCommand(deviceInfoService);
            var defaultFormatter = new DefaultShowVersionFormatter();
            return new UnifiedCliHandler(showVersionCommand, defaultFormatter);
        }
    }

    /// <summary>
    /// Default show version formatter for vendors without specific formatting
    /// </summary>
    public class DefaultShowVersionFormatter : BaseCommandFormatter
    {
        public override string VendorName => "Default";

        protected override string FormatPingResult(PingCommandData pingData)
        {
            // This formatter only handles show version, not ping
            throw new NotSupportedException("DefaultShowVersionFormatter does not support ping commands");
        }

        public override string Format(CommandData data)
        {
            return data switch
            {
                ShowVersionCommandData versionData => FormatShowVersionResult(versionData),
                _ => FormatGenericResult(data)
            };
        }

        public override bool CanFormat(Type dataType)
        {
            return dataType == typeof(ShowVersionCommandData) ||
                   dataType.IsSubclassOf(typeof(CommandData));
        }

        protected virtual string FormatShowVersionResult(ShowVersionCommandData versionData)
        {
            var version = versionData.VersionData;
            var output = new System.Text.StringBuilder();

            if (!versionData.Success)
            {
                output.AppendLine($"% {versionData.ErrorMessage}");
                return output.ToString();
            }

            // Generic show version output format
            output.AppendLine($"{version.Vendor} Network Device");
            output.AppendLine($"Device name: {version.DeviceName}");
            output.AppendLine($"Model: {version.Model}");
            output.AppendLine($"Software version: {version.SoftwareVersion}");

            if (!string.IsNullOrEmpty(version.BuildNumber))
                output.AppendLine($"Build: {version.BuildNumber} ({version.BuildDate})");

            output.AppendLine($"Serial number: {version.SerialNumber}");
            output.AppendLine($"Uptime: {FormatUptime(version.Uptime)}");

            // Hardware information
            output.AppendLine();
            output.AppendLine("Hardware Information:");
            output.AppendLine($"  Chassis: {version.HardwareInfo.ChassisType}");
            output.AppendLine($"  Processor: {version.HardwareInfo.ProcessorType} @ {version.HardwareInfo.ProcessorSpeed}MHz");
            output.AppendLine($"  Interfaces: {version.HardwareInfo.InterfaceCount}");

            // Memory information
            output.AppendLine();
            output.AppendLine("Memory Information:");
            output.AppendLine($"  Total: {FormatBytes(version.MemoryInfo.TotalMemory)}");
            output.AppendLine($"  Used: {FormatBytes(version.MemoryInfo.UsedMemory)} ({version.MemoryInfo.UsedPercentage:F1}%)");
            output.AppendLine($"  Available: {FormatBytes(version.MemoryInfo.AvailableMemory)}");

            // Flash information
            output.AppendLine();
            output.AppendLine("Flash Information:");
            output.AppendLine($"  Type: {version.StorageInfo.FlashType}");
            output.AppendLine($"  Total: {FormatBytes(version.StorageInfo.TotalFlashSize)}");
            output.AppendLine($"  Used: {FormatBytes(version.StorageInfo.UsedFlashSize)}");
            output.AppendLine($"  Available: {FormatBytes(version.StorageInfo.AvailableFlashSize)}");

            return output.ToString();
        }

        protected virtual string FormatUptime(TimeSpan uptime)
        {
            if (uptime.Days > 0)
                return $"{uptime.Days} day{(uptime.Days != 1 ? "s" : "")}, {uptime.Hours} hour{(uptime.Hours != 1 ? "s" : "")}, {uptime.Minutes} minute{(uptime.Minutes != 1 ? "s" : "")}";
            else if (uptime.Hours > 0)
                return $"{uptime.Hours} hour{(uptime.Hours != 1 ? "s" : "")}, {uptime.Minutes} minute{(uptime.Minutes != 1 ? "s" : "")}";
            else
                return $"{uptime.Minutes} minute{(uptime.Minutes != 1 ? "s" : "")}";
        }

        protected virtual string FormatBytes(long bytes)
        {
            string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
            int suffixIndex = 0;
            double value = bytes;

            while (value >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1024;
                suffixIndex++;
            }

            return $"{value:F1} {suffixes[suffixIndex]}";
        }
    }
}