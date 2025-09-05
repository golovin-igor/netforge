using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Vendors.Arista
{
    /// <summary>
    /// Vendor descriptor for Arista devices
    /// </summary>
    public class AristaVendorDescriptor : VendorDescriptorBase
    {
        public override string VendorName => "Arista";
        public override string DisplayName => "Arista Networks";
        public override string Description => "Arista Networks networking equipment vendor descriptor";
        public override int Priority => 80;

        protected override void InitializeVendor()
        {
            // Configure Arista-specific prompts (similar to Cisco but with some differences)
            ConfigurePrompts(">", "#", "(config)#");
            AddPromptMode("interface", "(config-if)#");
            AddPromptMode("router-bgp", "(config-router-bgp)#");
            AddPromptMode("router-ospf", "(config-router-ospf)#");
            AddPromptMode("vlan", "(config-vlan)#");
            AddPromptMode("mlag", "(config-mlag)#");

            // Add supported device models
            AddModel(new DeviceModelDescriptor
            {
                ModelName = "DCS-7280SR",
                ModelFamily = "7280",
                Description = "Arista 7280SR Data Center Switch",
                DeviceType = DeviceType.Switch,
                Features = { "VLAN", "MLAG", "BGP", "VXLAN", "CloudVision", "ZTP" }
            });

            AddModel(new DeviceModelDescriptor
            {
                ModelName = "DCS-7050SX",
                ModelFamily = "7050",
                Description = "Arista 7050SX Data Center Switch",
                DeviceType = DeviceType.Switch,
                Features = { "VLAN", "LACP", "BGP", "OSPF", "PIM", "QoS" }
            });

            AddModel(new DeviceModelDescriptor
            {
                ModelName = "DCS-7500R",
                ModelFamily = "7500",
                Description = "Arista 7500R Modular Switch",
                DeviceType = DeviceType.Switch,
                Features = { "VLAN", "MLAG", "BGP", "EVPN", "VXLAN", "Segment Routing" }
            });

            // Add supported protocols
            AddProtocol(NetworkProtocolType.OSPF, "NetForge.Simulation.Protocols.OSPF.AristaOspfProtocol");
            AddProtocol(NetworkProtocolType.BGP, "NetForge.Simulation.Protocols.BGP.AristaBgpProtocol");
            AddProtocol(NetworkProtocolType.ISIS, "NetForge.Simulation.Protocols.ISIS.AristaIsisProtocol");
            AddProtocol(NetworkProtocolType.STP, "NetForge.Simulation.Protocols.STP.AristaStpProtocol");
            AddProtocol(NetworkProtocolType.LLDP, "NetForge.Simulation.Protocols.LLDP.AristaLldpProtocol");
            AddProtocol(NetworkProtocolType.VRRP, "NetForge.Simulation.Protocols.VRRP.AristaVrrpProtocol");
            AddProtocol(NetworkProtocolType.ARP, "NetForge.Simulation.Protocols.ARP.AristaArpProtocol");
            AddProtocol(NetworkProtocolType.SSH, "NetForge.Simulation.Protocols.SSH.AristaSshProtocol");
            AddProtocol(NetworkProtocolType.SNMP, "NetForge.Simulation.Protocols.SNMP.AristaSnmpProtocol");

            // Add CLI handlers - Arista uses similar commands to Cisco but with some extensions
            AddHandler("ShowVersion", "show version", 
                "NetForge.Simulation.CliHandlers.Arista.Show.ShowVersionHandler", 
                HandlerType.Show);
            
            AddHandler("ShowRunningConfig", "show running-config", 
                "NetForge.Simulation.CliHandlers.Arista.Show.ShowRunningConfigHandler", 
                HandlerType.Show);
            
            AddHandler("ShowInterfaces", "show interfaces", 
                "NetForge.Simulation.CliHandlers.Arista.Show.ShowInterfacesHandler", 
                HandlerType.Show);
            
            AddHandler("ShowMlag", "show mlag", 
                "NetForge.Simulation.CliHandlers.Arista.Show.ShowMlagHandler", 
                HandlerType.Show);
            
            AddHandler("ShowVxlan", "show vxlan", 
                "NetForge.Simulation.CliHandlers.Arista.Show.ShowVxlanHandler", 
                HandlerType.Show);
            
            AddHandler("Configure", "configure terminal", 
                "NetForge.Simulation.CliHandlers.Arista.Configuration.ConfigureTerminalHandler", 
                HandlerType.Configuration);
            
            AddHandler("Interface", "interface", 
                "NetForge.Simulation.CliHandlers.Arista.Configuration.InterfaceHandler", 
                HandlerType.Interface);
            
            AddHandler("Mlag", "mlag configuration", 
                "NetForge.Simulation.CliHandlers.Arista.Configuration.MlagHandler", 
                HandlerType.Configuration);
            
            AddHandler("RouterBgp", "router bgp", 
                "NetForge.Simulation.CliHandlers.Arista.Routing.RouterBgpHandler", 
                HandlerType.Routing);
            
            AddHandler("RouterOspf", "router ospf", 
                "NetForge.Simulation.CliHandlers.Arista.Routing.RouterOspfHandler", 
                HandlerType.Routing);
            
            AddHandler("Enable", "enable", 
                "NetForge.Simulation.CliHandlers.Arista.Basic.EnableHandler", 
                HandlerType.Basic);
            
            AddHandler("Bash", "bash", 
                "NetForge.Simulation.CliHandlers.Arista.System.BashHandler", 
                HandlerType.System);
            
            AddHandler("ZeroTouch", "zerotouch", 
                "NetForge.Simulation.CliHandlers.Arista.System.ZeroTouchHandler", 
                HandlerType.System);
            
            AddHandler("WriteMemory", "write memory", 
                "NetForge.Simulation.CliHandlers.Arista.System.WriteMemoryHandler", 
                HandlerType.System);
            
            AddHandler("Reload", "reload", 
                "NetForge.Simulation.CliHandlers.Arista.System.ReloadHandler", 
                HandlerType.System);
        }
    }
}