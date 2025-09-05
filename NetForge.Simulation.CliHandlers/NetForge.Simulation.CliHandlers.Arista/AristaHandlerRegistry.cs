using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Handlers;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Arista;

public class AristaHandlerRegistry : IVendorHandlerRegistry
{
    public string VendorName => "Arista";

    public int Priority => 120; // High priority for Arista

    public bool CanHandle(string vendorName)
    {
        return vendorName.Equals("Arista", StringComparison.OrdinalIgnoreCase);
    }

    public void RegisterHandlers(ICliHandlerManager manager)
    {
        // Register Arista show handlers
        manager.RegisterHandler(new Show.ShowCommandHandler());

        // Register Arista configuration handlers
        manager.RegisterHandler(new Configuration.ConfigureCommandHandler());
        manager.RegisterHandler(new Configuration.InterfaceCommandHandler());
        manager.RegisterHandler(new Configuration.HostnameCommandHandler());
        manager.RegisterHandler(new Configuration.VlanCommandHandler());
        manager.RegisterHandler(new Configuration.NoCommandHandler());
        manager.RegisterHandler(new Configuration.ExitCommandHandler());
        manager.RegisterHandler(new Configuration.IpCommandHandler());

        // Register Arista basic handlers
        manager.RegisterHandler(new Basic.EnableCommandHandler());
        manager.RegisterHandler(new Basic.PingCommandHandler());
        manager.RegisterHandler(new Basic.WriteCommandHandler());
        manager.RegisterHandler(new Basic.ReloadCommandHandler());
        manager.RegisterHandler(new Basic.HistoryCommandHandler());
        manager.RegisterHandler(new Basic.CopyCommandHandler());
        manager.RegisterHandler(new Basic.TracerouteCommandHandler());
    }

    public IVendorContext CreateVendorContext(INetworkDevice device)
    {
        return new AristaVendorContext(device);
    }

    public IEnumerable<string> GetSupportedDeviceTypes()
    {
        return new[] { "arista", "switch", "router" };
    }

    public void Initialize()
    {
        // Arista registry initialization
    }

    public void Cleanup()
    {
        // Arista registry cleanup
    }
}
