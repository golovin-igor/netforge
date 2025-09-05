using NetForge.Interfaces.CLI;

namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Composite interface for vendor-agnostic device capabilities used by CLI handlers
    /// Composed of focused capability interfaces for better separation of concerns
    /// </summary>
    public interface IVendorCapabilities : 
        IDeviceCapabilities, 
        IInterfaceCapabilities, 
        IRoutingCapabilities, 
        ISwitchingCapabilities, 
        ISecurityCapabilities,
        IDiscoveryCapabilities,
        ICommandFormatter
    {
        /// <summary>
        /// Append a line to the running configuration
        /// </summary>
        bool AppendToRunningConfig(string configLine);
    }
}
