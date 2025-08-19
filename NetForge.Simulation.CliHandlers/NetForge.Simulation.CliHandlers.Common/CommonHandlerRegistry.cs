using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Interfaces;
using NetForge.Simulation.CliHandlers.Common;
using NetForge.Simulation.CliHandlers.Common.Common;

namespace NetForge.Simulation.CliHandlers.Common
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
