using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Vendors;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.CLI.Factories;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Base
{
    /// <summary>
    /// Represents the context for a CLI command execution
    /// </summary>
    public sealed class CliContext(INetworkDevice device, string[] commandParts, string fullCommand) : ICliContext
    {
        /// <summary>
        /// The device executing the command
        /// </summary>
        public INetworkDevice Device { get; } = device ?? throw new ArgumentNullException(nameof(device));

        /// <summary>
        /// The command split into parts
        /// </summary>
        public string[] CommandParts { get; } = commandParts ?? throw new ArgumentNullException(nameof(commandParts));

        /// <summary>
        /// The original, full command string
        /// </summary>
        public string FullCommand { get; } = fullCommand ?? throw new ArgumentNullException(nameof(fullCommand));

        /// <summary>
        /// The current device mode
        /// </summary>
        public string CurrentMode => Device.GetCurrentMode();

        /// <summary>
        /// Whether this is a help request (command ends with ?)
        /// </summary>
        public bool IsHelpRequest { get; init; }

        /// <summary>
        /// Additional parameters for command execution
        /// </summary>
        public Dictionary<string, string> Parameters { get; init; } = new();

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
        public static CliContext CreateHelpContext(INetworkDevice device, string[] commandParts, string fullCommand)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(commandParts);
            ArgumentNullException.ThrowIfNull(fullCommand);

            return new CliContext(device, commandParts, fullCommand) { IsHelpRequest = true };
        }

        public T GetService<T>() where T : class
        {
            //must resolve services from the device's service provider
            throw new NotImplementedException();
        }
    }
}
