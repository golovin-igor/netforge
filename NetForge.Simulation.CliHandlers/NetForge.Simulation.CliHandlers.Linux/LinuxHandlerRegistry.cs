using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.CliHandlers.Linux;

public class LinuxHandlerRegistry : IVendorHandlerRegistry
{
    public string VendorName => "Linux";

    public int Priority => 150; // Medium priority

    public bool CanHandle(string vendorName)
    {
        return vendorName.Equals("Linux", StringComparison.OrdinalIgnoreCase);
    }

    public void RegisterHandlers(ICliHandlerManager manager)
    {
        // Register Linux basic handlers (only the ones that actually exist)
        manager.RegisterHandler(new Basic.EnableCommandHandler());
        manager.RegisterHandler(new Basic.PingCommandHandler());

        // Register Linux system handlers (using the actual class names from SystemHandlers.cs)
        manager.RegisterHandler(new System.SystemHandlers.IpAddressHandler());
        manager.RegisterHandler(new System.SystemHandlers.IpLinkHandler());
        manager.RegisterHandler(new System.SystemHandlers.IpRouteHandler());
        manager.RegisterHandler(new System.SystemHandlers.IfconfigHandler());
        manager.RegisterHandler(new System.SystemHandlers.RouteHandler());
        manager.RegisterHandler(new System.SystemHandlers.ArpHandler());
        manager.RegisterHandler(new System.SystemHandlers.LsmodHandler());

        // Register Linux routing protocol handlers
        manager.RegisterHandler(new System.SystemHandlers.OspfHandler());
        manager.RegisterHandler(new System.SystemHandlers.BgpHandler());
        manager.RegisterHandler(new System.SystemHandlers.RipHandler());
    }

    public IVendorContext CreateVendorContext(INetworkDevice device)
    {
        return new LinuxVendorContext(device);
    }

    public IEnumerable<string> GetSupportedDeviceTypes()
    {
        return new[] { "server", "workstation", "host" };
    }

    public void Initialize()
    {
        // Linux registry initialization
    }

    public void Cleanup()
    {
        // Linux registry cleanup
    }
}
