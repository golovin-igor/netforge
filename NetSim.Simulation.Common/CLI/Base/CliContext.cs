using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.CliHandlers
{
    /// <summary>
    /// Represents the context for a CLI command execution
    /// </summary>
    public class CliContext(NetworkDevice device, string[] commandParts, string fullCommand)
    {
        /// <summary>
        /// The device executing the command
        /// </summary>
        public NetworkDevice Device { get; } = device;

        /// <summary>
        /// The command split into parts
        /// </summary>
        public string[] CommandParts { get; } = commandParts;

        /// <summary>
        /// The original, full command string
        /// </summary>
        public string FullCommand { get; } = fullCommand;

        /// <summary>
        /// The current device mode
        /// </summary>
        public string CurrentMode => Device.GetCurrentMode();
        
        /// <summary>
        /// Whether this is a help request (command ends with ?)
        /// </summary>
        public bool IsHelpRequest { get; set; }
        
        /// <summary>
        /// Additional parameters for command execution
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new();
        
        /// <summary>
        /// Vendor-specific context for the device (lazy-loaded)
        /// </summary>
        private IVendorContext? _vendorContext;
        public IVendorContext? VendorContext 
        { 
            get => _vendorContext ??= VendorContextFactory.GetVendorContext(Device);
            set => _vendorContext = value;
        }

        /// <summary>
        /// Creates a context for a help request
        /// </summary>
        public static CliContext CreateHelpContext(NetworkDevice device, string[] commandParts, string fullCommand)
        {
            return new CliContext(device, commandParts, fullCommand) { IsHelpRequest = true };
        }
    }
} 
