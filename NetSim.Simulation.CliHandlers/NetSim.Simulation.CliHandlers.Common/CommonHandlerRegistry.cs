using NetSim.Simulation.CliHandlers;
using NetSim.Simulation.Interfaces;
using NetSim.Simulation.CliHandlers.Common;
using NetSim.Simulation.CliHandlers.Common.Common;

namespace NetSim.Simulation.CliHandlers.Common
{
    /// <summary>
    /// Registry for vendor-agnostic common command handlers
    /// </summary>
    public class CommonHandlerRegistry : VendorHandlerRegistryBase
    {
        public override string VendorName => "Common";
        public override int Priority => 1000; // Highest priority - applies to all vendors
        
        public override void RegisterHandlers(CliHandlerManager manager)
        {
            // Register common history recall handlers - these work for ALL vendors
            manager.RegisterHandler(new HistoryRecallCommandHandler());
            manager.RegisterHandler(new HistoryNumberRecallCommandHandler());
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
