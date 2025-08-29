using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;

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
