using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.CliHandlers.Common.Common;
using NetForge.Simulation.Common.CLI.CommonHandlers;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Handlers.Common;

namespace NetForge.Simulation.CliHandlers.Common
{
    /// <summary>
    /// Registry for vendor-agnostic common command handlers
    /// </summary>
    public class CommonCliHandlerRegistry : VendorHandlerRegistryBase
    {
        public override string VendorName => "Common";
        public override int Priority => 1000; // Highest priority - applies to all vendors

        public override void RegisterHandlers(ICliHandlerManager manager)
        {
            // Register common history recall handlers - these work for ALL vendors
            manager.RegisterHandler(new HistoryRecallCommandHandler());
            manager.RegisterHandler(new HistoryNumberRecallCommandHandler());
            manager.RegisterHandler(new CommonPingCommandHandler());
        }

        public override IVendorContext CreateVendorContext(INetworkDevice device)
        {
            // Common handlers don't need vendor-specific context
            return null;
        }

        public override IEnumerable<string> GetSupportedDeviceTypes()
        {
            // Common handlers support ALL device types
            return new[] { "*" };
        }

        public override bool CanHandle(string vendorName)
        {
            // Common handlers apply to ALL vendors
            return true;
        }

        public override void Initialize()
        {
            // No vendor-specific initialization needed for common handlers
            base.Initialize();
        }
    }
}
