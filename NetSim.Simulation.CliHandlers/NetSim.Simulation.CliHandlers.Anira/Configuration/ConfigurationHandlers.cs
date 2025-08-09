using NetSim.Simulation.Common;

namespace NetSim.Simulation.CliHandlers.Anira.Configuration
{
    /// <summary>
    /// Anira configure command handler
    /// </summary>
    public class ConfigureCommandHandler : VendorAgnosticCliHandler
    {
        public ConfigureCommandHandler() : base("configure", "Enter configuration mode")
        {
            AddAlias("conf");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode, 
                    GetVendorError(context, "invalid_mode"));
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    GetVendorError(context, "incomplete_command"));
            }
            
            if (context.CommandParts[1].Equals("terminal", StringComparison.OrdinalIgnoreCase))
            {
                SetMode(context, "config");
                return Success("");
            }
            
            return Error(CliErrorType.InvalidParameter, 
                GetVendorError(context, "invalid_parameter"));
        }
    }
    
    /// <summary>
    /// Anira interface command handler
    /// </summary>
    public class InterfaceCommandHandler : VendorAgnosticCliHandler
    {
        public InterfaceCommandHandler() : base("interface", "Configure interface")
        {
            AddAlias("int");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode, 
                    GetVendorError(context, "invalid_mode"));
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    GetVendorError(context, "incomplete_command"));
            }
            
            var interfaceName = context.CommandParts[1];
            
            // Set current interface and enter interface mode
            var vendorContext = GetVendorContext(context);
            vendorContext?.Capabilities.SetCurrentInterface(interfaceName);
            
            SetMode(context, "interface");
            
            return Success("");
        }
    }
    
    /// <summary>
    /// Anira hostname command handler
    /// </summary>
    public class HostnameCommandHandler : VendorAgnosticCliHandler
    {
        public HostnameCommandHandler() : base("hostname", "Set device hostname")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (!IsInMode(context, "config"))
            {
                return Error(CliErrorType.InvalidMode, 
                    GetVendorError(context, "invalid_mode"));
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    GetVendorError(context, "incomplete_command"));
            }
            
            var hostname = context.CommandParts[1];
            
            // Set hostname using vendor capabilities
            var vendorContext = GetVendorContext(context);
            vendorContext?.Capabilities.SetHostname(hostname);
            
            return Success("");
        }
    }
    
    /// <summary>
    /// Anira IP address command handler
    /// </summary>
    public class IpAddressCommandHandler : VendorAgnosticCliHandler
    {
        public IpAddressCommandHandler() : base("ip", "Configure IP parameters")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode, 
                    GetVendorError(context, "invalid_mode"));
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    GetVendorError(context, "incomplete_command"));
            }
            
            if (context.CommandParts[1].Equals("address", StringComparison.OrdinalIgnoreCase))
            {
                return HandleIpAddress(context);
            }
            
            return Error(CliErrorType.InvalidParameter, 
                GetVendorError(context, "invalid_parameter"));
        }
        
        private CliResult HandleIpAddress(CliContext context)
        {
            if (context.CommandParts.Length < 4)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Usage: ip address <ip-address> <subnet-mask>");
            }
            
            var ipAddress = context.CommandParts[2];
            var subnetMask = context.CommandParts[3];
            
            // Validate IP address and subnet mask
            if (!IsValidIpAddress(ipAddress))
            {
                return Error(CliErrorType.InvalidParameter, 
                    GetVendorError(context, "invalid_ip"));
            }
            
            if (!IsValidIpAddress(subnetMask))
            {
                return Error(CliErrorType.InvalidParameter, 
                    "% Invalid subnet mask");
            }
            
            // Set IP address using vendor capabilities
            var vendorContext = GetVendorContext(context);
            var currentInterface = vendorContext?.GetCurrentInterface();
            
            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.ExecutionError, 
                    "% No interface selected");
            }
            
            vendorContext?.Capabilities.SetInterface(currentInterface, ipAddress, subnetMask);
            
            return Success("");
        }
        
        private bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
    }
    
    /// <summary>
    /// Anira shutdown command handler
    /// </summary>
    public class ShutdownCommandHandler : VendorAgnosticCliHandler
    {
        public ShutdownCommandHandler() : base("shutdown", "Shutdown interface")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode, 
                    GetVendorError(context, "invalid_mode"));
            }
            
            // Shutdown current interface
            var vendorContext = GetVendorContext(context);
            var currentInterface = vendorContext?.GetCurrentInterface();
            
            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.ExecutionError, 
                    "% No interface selected");
            }
            
            vendorContext?.Capabilities.SetInterfaceState(currentInterface, "shutdown");
            
            return Success("");
        }
    }
    
    /// <summary>
    /// Anira no shutdown command handler
    /// </summary>
    public class NoShutdownCommandHandler : VendorAgnosticCliHandler
    {
        public NoShutdownCommandHandler() : base("no", "Negate a command")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }
            
            if (!IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode, 
                    GetVendorError(context, "invalid_mode"));
            }
            
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    GetVendorError(context, "incomplete_command"));
            }
            
            if (context.CommandParts[1].Equals("shutdown", StringComparison.OrdinalIgnoreCase))
            {
                // Enable current interface
                var vendorContext = GetVendorContext(context);
                var currentInterface = vendorContext?.GetCurrentInterface();
                
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.ExecutionError, 
                        "% No interface selected");
                }
                
                vendorContext?.Capabilities.SetInterfaceState(currentInterface, "enable");
                
                return Success("");
            }
            
            return Error(CliErrorType.InvalidParameter, 
                GetVendorError(context, "invalid_parameter"));
        }
    }
} 
