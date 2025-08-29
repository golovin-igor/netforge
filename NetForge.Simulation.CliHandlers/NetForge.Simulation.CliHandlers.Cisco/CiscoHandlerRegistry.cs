using NetForge.Interfaces.Cli;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Factories;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Handlers.Common;

namespace NetForge.Simulation.CliHandlers.Cisco
{
    /// <summary>
    /// Registry for Cisco CLI handlers
    /// </summary>
    public class CiscoHandlerRegistry : VendorHandlerRegistryBase
    {
        public override string VendorName => "Cisco";
        public override int Priority => 200; // Higher priority for well-tested vendor

        public override void RegisterHandlers(ICliHandlerManager manager)
        {
            // Register basic handlers (migrated from old architecture)
            manager.RegisterHandler(new Basic.EnableCommandHandler());
            manager.RegisterHandler(new Basic.DisableCommandHandler());
            manager.RegisterHandler(new Basic.PingCommandHandler());
            manager.RegisterHandler(new Basic.TracerouteCommandHandler());
            manager.RegisterHandler(new Basic.WriteCommandHandler());
            manager.RegisterHandler(new Basic.ReloadCommandHandler());
            manager.RegisterHandler(new Basic.HistoryCommandHandler());
            manager.RegisterHandler(new Basic.HelpCommandHandler());
            manager.RegisterHandler(new Basic.CopyCommandHandler());
            manager.RegisterHandler(new Basic.ClearCommandHandler());

            // Register show handlers (migrated from old architecture)
            manager.RegisterHandler(new Show.ShowCommandHandler());


            // Register configuration handlers (migrated from old architecture)
            manager.RegisterHandler(new Configuration.ConfigureCommandHandler());
            manager.RegisterHandler(new Configuration.ExitCommandHandler());
            manager.RegisterHandler(new Configuration.InterfaceCommandHandler());
            manager.RegisterHandler(new Configuration.IpCommandHandler());
            manager.RegisterHandler(new Configuration.NoCommandHandler());
            manager.RegisterHandler(new Configuration.HostnameCommandHandler());
            manager.RegisterHandler(new Configuration.VlanCommandHandler());
            manager.RegisterHandler(new Configuration.RouterCommandHandler());

            // Register VLAN configuration handlers
            manager.RegisterHandler(new Configuration.CiscoVlanNameHandler());

            // Register router mode command handler
            manager.RegisterHandler(new Configuration.CiscoRouterModeCommandHandler());

            // Router sub-handlers are automatically registered via RouterCommandHandler.AddSubHandler()

            // Register interface configuration handlers
            manager.RegisterHandler(new Configuration.IpAddressCommandHandler());
            manager.RegisterHandler(new Configuration.IpAccessGroupCommandHandler());
            manager.RegisterHandler(new Configuration.ShutdownCommandHandler());
            manager.RegisterHandler(new Configuration.NoShutdownHandler());

            // Register Access List handlers (newly migrated)
            manager.RegisterHandler(new Configuration.AccessListCommandHandler());
            manager.RegisterHandler(new Configuration.IpAccessListCommandHandler());
            manager.RegisterHandler(new Configuration.IpAccessListStandardHandler());
            manager.RegisterHandler(new Configuration.IpAccessListExtendedHandler());
            manager.RegisterHandler(new Configuration.PermitCommandHandler());
            manager.RegisterHandler(new Configuration.DenyCommandHandler());

            // Register Switchport handlers (newly migrated)
            manager.RegisterHandler(new Configuration.SwitchportCommandHandler());
            manager.RegisterHandler(new Configuration.SwitchportModeHandler());
            manager.RegisterHandler(new Configuration.SwitchportModeAccessHandler());
            manager.RegisterHandler(new Configuration.SwitchportModeTrunkHandler());
            manager.RegisterHandler(new Configuration.SwitchportModeDynamicHandler());
            manager.RegisterHandler(new Configuration.SwitchportModeDynamicAutoHandler());
            manager.RegisterHandler(new Configuration.SwitchportModeDynamicDesirableHandler());
            manager.RegisterHandler(new Configuration.SwitchportAccessHandler());
            manager.RegisterHandler(new Configuration.SwitchportAccessVlanHandler());
            manager.RegisterHandler(new Configuration.SwitchportTrunkHandler());
            manager.RegisterHandler(new Configuration.SwitchportTrunkEncapsulationHandler());
            manager.RegisterHandler(new Configuration.SwitchportTrunkEncapsulationDot1qHandler());
            manager.RegisterHandler(new Configuration.SwitchportTrunkEncapsulationIslHandler());
            manager.RegisterHandler(new Configuration.SwitchportTrunkAllowedHandler());
            manager.RegisterHandler(new Configuration.SwitchportTrunkAllowedVlanHandler());
            manager.RegisterHandler(new Configuration.SwitchportTrunkNativeHandler());
            manager.RegisterHandler(new Configuration.SwitchportTrunkNativeVlanHandler());
            manager.RegisterHandler(new Configuration.SwitchportVoiceHandler());
            manager.RegisterHandler(new Configuration.SwitchportVoiceVlanHandler());

            // Register Spanning Tree handlers (newly migrated)
            manager.RegisterHandler(new Configuration.SpanningTreeCommandHandler());
            manager.RegisterHandler(new Configuration.SpanningTreeModeHandler());
            manager.RegisterHandler(new Configuration.SpanningTreeModePvstHandler());
            manager.RegisterHandler(new Configuration.SpanningTreeModeRapidPvstHandler());
            manager.RegisterHandler(new Configuration.SpanningTreeModeMstHandler());
            manager.RegisterHandler(new Configuration.SpanningTreeVlanHandler());
            manager.RegisterHandler(new Configuration.SpanningTreePortfastHandler());
            manager.RegisterHandler(new Configuration.SpanningTreePortfastDefaultHandler());
            manager.RegisterHandler(new Configuration.SpanningTreePortfastBpduguardHandler());
            manager.RegisterHandler(new Configuration.SpanningTreePortfastBpduguardDefaultHandler());
            manager.RegisterHandler(new Configuration.SpanningTreePriorityHandler());

            // Register CDP handlers (newly migrated)
            manager.RegisterHandler(new Configuration.CdpCommandHandler());
            manager.RegisterHandler(new Configuration.CdpRunHandler());
            manager.RegisterHandler(new Configuration.CdpEnableHandler());
            manager.RegisterHandler(new Configuration.CdpTimerHandler());
            manager.RegisterHandler(new Configuration.CdpHoldtimeHandler());
            manager.RegisterHandler(new Configuration.NoCdpCommandHandler());
            manager.RegisterHandler(new Configuration.NoCdpRunHandler());
            manager.RegisterHandler(new Configuration.NoCdpEnableHandler());
        }

        public override IVendorContext CreateVendorContext(INetworkDevice device)
        {
            if (device is {} networkDevice)
            {
                return new CiscoVendorContext(networkDevice);
            }

            throw new ArgumentException($"Device type {device.GetType().Name} is not compatible with Cisco handler registry");
        }

        public override IEnumerable<string> GetSupportedDeviceTypes()
        {
            return new[] { "router", "switch", "catalyst", "nexus", "asr", "isr" };
        }

        public override void Initialize()
        {
            // Register Cisco vendor context factory
            VendorContextFactory.RegisterVendorContext("cisco", device => new CiscoVendorContext(device));

            // Any other Cisco-specific initialization
            base.Initialize();
        }

        public override bool CanHandle(string vendorName)
        {
            // Handle various Cisco vendor name variations
            var ciscoNames = new[] { "cisco", "cisco systems", "ios", "ios-xe", "ios-xr", "nx-os" };
            return ciscoNames.Any(name => name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
        }

        public override void Cleanup()
        {
            // Cleanup Cisco-specific resources if needed
            base.Cleanup();
        }
    }
}
