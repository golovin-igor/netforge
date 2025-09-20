using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Handlers;
using NetForge.Interfaces.Vendors;
using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Cli;

namespace NetForge.Simulation.Handlers.Common;

/// <summary>
/// Base implementation of vendor handler registry
/// </summary>
public abstract class VendorHandlerRegistryBase : IVendorHandlerRegistry
{
    public abstract string VendorName { get; }
    public virtual int Priority => 100;

    public virtual bool CanHandle(string vendorName)
    {
        return VendorName.Equals(vendorName, StringComparison.OrdinalIgnoreCase);
    }

    public abstract void RegisterHandlers(ICliHandlerManager manager);
    public abstract IVendorContext CreateVendorContext(INetworkDevice device);

    public virtual IEnumerable<string> GetSupportedDeviceTypes()
    {
        return new[] { "router", "switch", "firewall" };
    }

    public virtual void Initialize()
    {
        // Override in derived classes if needed
    }

    public virtual void Cleanup()
    {
        // Override in derived classes if needed
    }
}
