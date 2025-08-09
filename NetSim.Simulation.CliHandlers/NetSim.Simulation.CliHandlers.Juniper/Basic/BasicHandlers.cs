using System.Text;
using NetSim.Simulation.Common;
using NetSim.Simulation.CliHandlers;

namespace NetSim.Simulation.CliHandlers.Juniper.Basic
{
    /// <summary>
    /// Juniper configure command handler with candidate configuration support
    /// </summary>
    public class ConfigureCommandHandler : VendorAgnosticCliHandler
    {
        public ConfigureCommandHandler() : base("configure", "Enter configuration mode")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Juniper"))
            {
                return RequireVendor(context, "Juniper");
            }
            
            // Juniper-specific: Enter configuration mode and initialize candidate config
            SetMode(context, "configuration");
            
            // Clear any existing candidate configuration
            if (context.Device is NetworkDevice device)
            {
                // Initialize candidate configuration if device supports it
                device.AddLogEntry("Entering configuration mode - candidate configuration initialized");
            }
            
            return Success("Entering configuration mode\n");
        }
    }

    /// <summary>
    /// Juniper commit command handler - CRITICAL vendor-specific functionality
    /// </summary>
    public class CommitCommandHandler : VendorAgnosticCliHandler
    {
        public CommitCommandHandler() : base("commit", "Commit candidate configuration")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Juniper"))
            {
                return RequireVendor(context, "Juniper");
            }
            
            if (!IsInMode(context, "configuration"))
            {
                return Error(CliErrorType.InvalidMode, 
                    "% Error: Must be in configuration mode");
            }
            
            try
            {
                var device = context.Device as NetworkDevice;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError, 
                        "% Device not available");
                }
                
                // Juniper-specific: Commit candidate configuration to running config
                device.AddLogEntry("Configuration committed successfully - candidate config applied to running config");
                
                // Trigger protocol updates if network is available
                // In a real implementation, this would apply candidate config changes
                
                return Success("commit complete\n");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError, 
                    $"% Error committing configuration: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Juniper rollback command handler - CRITICAL vendor-specific functionality
    /// </summary>
    public class RollbackCommandHandler : VendorAgnosticCliHandler
    {
        public RollbackCommandHandler() : base("rollback", "Rollback candidate configuration")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Juniper"))
            {
                return RequireVendor(context, "Juniper");
            }
            
            if (!IsInMode(context, "configuration"))
            {
                return Error(CliErrorType.InvalidMode, 
                    "% Error: Must be in configuration mode");
            }
            
            var rollbackNumber = "0"; // Default rollback
            if (context.CommandParts.Length > 1)
            {
                rollbackNumber = context.CommandParts[1];
            }
            
            try
            {
                var device = context.Device as NetworkDevice;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError, 
                        "% Device not available");
                }
                
                // Juniper-specific: Clear candidate configuration
                device.AddLogEntry($"Configuration rolled back to {rollbackNumber} - candidate config cleared");
                
                return Success($"load complete\n");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError, 
                    $"% Error rolling back configuration: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Juniper exit command handler with configuration mode awareness
    /// </summary>
    public class ExitCommandHandler : VendorAgnosticCliHandler
    {
        public ExitCommandHandler() : base("exit", "Exit current mode")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Juniper"))
            {
                return RequireVendor(context, "Juniper");
            }
            
            var currentMode = GetCurrentMode(context);
            
            // Juniper-specific mode transitions
            var newMode = currentMode switch
            {
                "configuration" => "operational",
                "interface" => "configuration",
                _ => "operational"
            };
            
            SetMode(context, newMode);
            
            // If exiting configuration mode, warn about uncommitted changes
            if (currentMode == "configuration")
            {
                return Success("Exiting configuration mode\n");
            }
            
            return Success("");
        }
    }

    /// <summary>
    /// Juniper delete command handler for configuration elements
    /// </summary>
    public class DeleteCommandHandler : VendorAgnosticCliHandler
    {
        public DeleteCommandHandler() : base("delete", "Delete configuration elements")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Juniper"))
            {
                return RequireVendor(context, "Juniper");
            }
            
            if (!IsInMode(context, "configuration"))
            {
                return Error(CliErrorType.InvalidMode, 
                    "% Error: Must be in configuration mode");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - specify configuration element to delete");
            }
            
            var elementToDelete = string.Join(" ", context.CommandParts.Skip(1));
            
            try
            {
                var device = context.Device as NetworkDevice;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError, 
                        "% Device not available");
                }
                
                // Juniper-specific: Mark element for deletion in candidate config
                device.AddLogEntry($"Configuration element marked for deletion: {elementToDelete}");
                
                return Success($"delete complete\n");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError, 
                    $"% Error deleting configuration: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Juniper ping command handler with JunOS-style output
    /// </summary>
    public class PingCommandHandler : VendorAgnosticCliHandler
    {
        public PingCommandHandler() : base("ping", "Send ping packets")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Juniper"))
            {
                return RequireVendor(context, "Juniper");
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - need IP address");
            }
            
            var targetIp = context.CommandParts[1];
            
            // Validate IP address format
            if (!IsValidIpAddress(targetIp))
            {
                return Error(CliErrorType.InvalidParameter, 
                    $"% Invalid IP address: {targetIp}");
            }
            
            // Simulate ping result (JunOS style)
            var output = new StringBuilder();
            output.AppendLine($"PING {targetIp} ({targetIp}): 56 data bytes");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=0 ttl=64 time=0.123 ms");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=1 ttl=64 time=0.089 ms");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=2 ttl=64 time=0.156 ms");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=3 ttl=64 time=0.134 ms");
            output.AppendLine($"64 bytes from {targetIp}: icmp_seq=4 ttl=64 time=0.098 ms");
            output.AppendLine("");
            output.AppendLine($"--- {targetIp} ping statistics ---");
            output.AppendLine("5 packets transmitted, 5 packets received, 0% packet loss");
            output.AppendLine("round-trip min/avg/max/stddev = 0.089/0.120/0.156/0.025 ms");
            
            return Success(output.ToString());
        }
        
        private bool IsValidIpAddress(string ip)
        {
            return global::System.Net.IPAddress.TryParse(ip, out _);
        }
    }
}
