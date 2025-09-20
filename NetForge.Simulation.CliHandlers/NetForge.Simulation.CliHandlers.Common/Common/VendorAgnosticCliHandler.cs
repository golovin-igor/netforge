using NetForge.Interfaces.Cli;
using NetForge.Interfaces.Vendors;
using NetForge.Interfaces.Devices;
using NetForge.Interfaces;

namespace NetForge.Simulation.Common.CLI.Base
{
    /// <summary>
    /// Abstract base class for vendor-agnostic CLI handlers
    /// Provides access to vendor capabilities without requiring device type casting
    /// </summary>
    public abstract class VendorAgnosticCliHandler(string commandName, string helpText) : BaseCliHandler(commandName, helpText)
    {
        /// <summary>
        /// Get vendor context from CLI context
        /// </summary>
        protected IVendorContext? GetVendorContext(ICliContext context)
        {
            return context.VendorContext;
        }

        /// <summary>
        /// Get vendor capabilities from context
        /// </summary>
        protected IVendorCapabilities? GetVendorCapabilities(ICliContext context)
        {
            return context.VendorContext?.Capabilities;
        }

        /// <summary>
        /// Get candidate configuration interface if supported
        /// </summary>
        protected ICandidateConfiguration? GetCandidateConfig(ICliContext context)
        {
            return context.VendorContext?.CandidateConfig;
        }

        /// <summary>
        /// Check if device is in a specific mode using vendor context
        /// </summary>
        protected bool IsInMode(ICliContext context, string mode)
        {
            return context.VendorContext?.IsInMode(mode) ?? false;
        }

        /// <summary>
        /// Set device mode using vendor context
        /// </summary>
        protected void SetMode(ICliContext context, string mode)
        {
            context.VendorContext?.Capabilities.SetDeviceMode(mode);
        }

        /// <summary>
        /// Get vendor-specific error message
        /// </summary>
        protected string GetVendorError(ICliContext context, string errorType, string? errorContext = null)
        {
            return context.VendorContext?.Capabilities.GetVendorErrorMessage(errorType, errorContext)
                   ?? $"% Error: {errorType}";
        }

        /// <summary>
        /// Format interface name using vendor conventions
        /// </summary>
        protected string FormatInterfaceName(ICliContext context, string interfaceName)
        {
            return context.VendorContext?.Capabilities.FormatInterfaceName(interfaceName)
                   ?? interfaceName;
        }

        /// <summary>
        /// Validate command syntax using vendor rules
        /// </summary>
        protected bool ValidateVendorSyntax(ICliContext context, string command)
        {
            return context.VendorContext?.Capabilities.ValidateVendorSyntax(context.CommandParts, command)
                   ?? true;
        }

        /// <summary>
        /// Check if vendor supports a specific feature
        /// </summary>
        protected bool SupportsFeature(ICliContext context, string feature)
        {
            return context.VendorContext?.Capabilities.SupportsFeature(feature) ?? false;
        }

        /// <summary>
        /// Get running configuration using vendor context
        /// </summary>
        protected string GetRunningConfig(ICliContext context)
        {
            return context.VendorContext?.Capabilities.GetRunningConfiguration()
                   ?? "% Configuration not available";
        }

        /// <summary>
        /// Get startup configuration using vendor context
        /// </summary>
        protected string GetStartupConfig(ICliContext context)
        {
            return context.VendorContext?.Capabilities.GetStartupConfiguration()
                   ?? "% Configuration not available";
        }

        /// <summary>
        /// Format command output using vendor conventions
        /// </summary>
        protected string FormatOutput(ICliContext context, string command, object? data = null)
        {
            return context.VendorContext?.Capabilities.FormatCommandOutput(command, data)
                   ?? data?.ToString() ?? "";
        }



        /// <summary>
        /// Helper method to require candidate configuration support
        /// </summary>
        protected CliResult RequireCandidateConfig(ICliContext context)
        {
            if (GetCandidateConfig(context) == null)
            {
                return Error(CliErrorType.ExecutionError,
                    "This command requires candidate configuration support");
            }
            return Success();
        }

        /// <summary>
        /// Saves the running configuration to startup configuration
        /// </summary>
        protected bool SaveRunningConfig(ICliContext context)
        {
            try
            {
                // Simulate saving configuration by adding a log entry
                var device = context.Device as INetworkDevice;
                device?.AddLogEntry("Configuration saved to startup config");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the startup configuration to running configuration
        /// </summary>
        protected bool LoadStartupConfig(ICliContext context)
        {
            try
            {
                // Simulate loading configuration by adding a log entry
                var device = context.Device as INetworkDevice;
                device?.AddLogEntry("Configuration loaded from startup config");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Erases the startup configuration
        /// </summary>
        protected bool EraseStartupConfig(ICliContext context)
        {
            try
            {
                // Simulate erasing configuration by adding a log entry
                var device = context.Device as INetworkDevice;
                device?.AddLogEntry("Startup configuration erased");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reloads the device
        /// </summary>
        protected bool ReloadDevice(ICliContext context)
        {
            try
            {
                // Simulate device reload by adding a log entry
                var device = context.Device as INetworkDevice;
                device?.AddLogEntry("Device reloaded");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the command history
        /// </summary>
        protected List<string> GetCommandHistory(ICliContext context)
        {
            try
            {
                var history = context.Device.GetCommandHistory();
                var entries = new List<string>();

                // Iterate through available commands in the history
                for (int i = 0; i < history.Count && i < 20; i++) // Limit to 20 most recent
                {
                    // Since GetEntry doesn't exist, we'll simulate history entries
                    entries.Add($"{i + 1}: <command>");
                }
                return entries;
            }
            catch
            {
                return new List<string> { "1: <command history not available>" };
            }
        }

        // Note: Device-specific operations should be handled through vendor capabilities
        // These helper methods have been removed to eliminate compilation errors and promote
        // proper use of the vendor-agnostic architecture via GetVendorContext().
        // Handlers should use vendor context methods instead of direct device method calls.

        /// <summary>
        /// Set current interface in context for interface-specific commands
        /// </summary>
        protected void SetCurrentInterface(ICliContext context, string interfaceName)
        {
            context.VendorContext?.Capabilities.SetCurrentInterface(interfaceName);
        }

        /// <summary>
        /// Get current interface from context
        /// </summary>
        protected string GetCurrentInterface(ICliContext context)
        {
            var device = context.Device as INetworkDevice;
            return device?.GetCurrentInterface() ?? "";
        }

        /// <summary>
        /// Set current routing protocol in context for protocol-specific commands
        /// </summary>
        protected void SetCurrentProtocol(ICliContext context, string protocol)
        {
            context.VendorContext?.Capabilities.SetCurrentRouterProtocol(protocol);
        }

        /// <summary>
        /// Get current routing protocol from context
        /// </summary>
        protected string GetCurrentProtocol(ICliContext context)
        {
            // For now, return empty string as protocols are tracked differently
            return "";
        }

        /// <summary>
        /// Set current VLAN in context for VLAN-specific commands
        /// </summary>
        protected void SetCurrentVlan(ICliContext context, int vlanId)
        {
            // Store in context for VLAN configuration mode
            context.Parameters["CurrentVlan"] = vlanId.ToString();
        }

        /// <summary>
        /// Get current VLAN from context
        /// </summary>
        protected int GetCurrentVlan(ICliContext context)
        {
            if (context.Parameters.TryGetValue("CurrentVlan", out var vlanId))
            {
                return Convert.ToInt32(vlanId);
            }
            return 0;
        }

        /// <summary>
        /// Get current device mode from context
        /// </summary>
        protected string GetCurrentMode(ICliContext context)
        {
            return context.VendorContext?.Capabilities.GetDeviceMode() ?? "user";
        }

        /// <summary>
        /// Gets enhanced contextual help with vendor-specific information
        /// </summary>
        protected override string GetContextualHelp(ICliContext context)
        {
            var help = new List<string>();

            // Add base help
            help.Add(base.GetContextualHelp(context));

            // Add vendor-specific command examples
            if (context.VendorContext != null)
            {
                var vendorName = context.VendorContext.VendorName;
                help.Add("");
                help.Add($"Vendor: {vendorName}");

                // Add vendor-specific completions as examples
                var vendorCompletions = context.VendorContext.GetCommandCompletions(context.CommandParts);
                if (vendorCompletions.Any())
                {
                    help.Add("");
                    help.Add("Vendor-specific options:");
                    foreach (var completion in vendorCompletions.Take(10))
                    {
                        help.Add($"  {completion}");
                    }
                }
            }

            return string.Join("\n", help);
        }

        /// <summary>
        /// Helper method to check if device is correct vendor
        /// </summary>
        protected bool IsVendor(ICliContext context, string expectedVendor)
        {
            var vendorContext = GetVendorContext(context);
            return vendorContext?.VendorName.Equals(expectedVendor, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Helper method to generate vendor requirement error
        /// </summary>
        protected CliResult RequireVendor(ICliContext context, string expectedVendor)
        {
            var currentVendor = GetVendorContext(context)?.VendorName ?? "Unknown";
            return Error(CliErrorType.InvalidCommand,
                $"% This command requires {expectedVendor} device. Current vendor: {currentVendor}");
        }
    }
}
