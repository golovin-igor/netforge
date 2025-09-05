using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Vendors.Cisco
{
    /// <summary>
    /// Vendor descriptor for Cisco devices
    /// </summary>
    public class CiscoVendorDescriptor : VendorDescriptorBase
    {
        public override string VendorName => "Cisco";
        public override string DisplayName => "Cisco Systems";
        public override string Description => "Cisco Systems networking equipment vendor descriptor";
        public override int Priority => 100;

        protected override void InitializeVendor()
        {
            // Configure Cisco-specific prompts
            ConfigurePrompts(">", "#", "(config)#");
            AddPromptMode("interface", "(config-if)#");
            AddPromptMode("router", "(config-router)#");
            AddPromptMode("line", "(config-line)#");
            AddPromptMode("vlan", "(config-vlan)#");

            // Add supported device models
            AddModel(new DeviceModelDescriptor
            {
                ModelName = "ISR4451",
                ModelFamily = "ISR4000",
                Description = "Cisco ISR 4451 Router",
                DeviceType = DeviceType.Router,
                Features = { "BGP", "OSPF", "EIGRP", "NAT", "VPN", "QoS" }
            });

            AddModel(new DeviceModelDescriptor
            {
                ModelName = "Catalyst9300",
                ModelFamily = "Catalyst9000",
                Description = "Cisco Catalyst 9300 Switch",
                DeviceType = DeviceType.Switch,
                Features = { "VLAN", "STP", "VTP", "LACP", "PoE", "Stacking" }
            });

            AddModel(new DeviceModelDescriptor
            {
                ModelName = "ASA5506",
                ModelFamily = "ASA5500",
                Description = "Cisco ASA 5506 Firewall",
                DeviceType = DeviceType.Firewall,
                Features = { "Firewall", "VPN", "IPS", "NAT", "Failover" }
            });

            // Add supported protocols - using existing implementations
            AddProtocol(NetworkProtocolType.OSPF, "NetForge.Simulation.Protocols.OSPF.OspfProtocol", "NetForge.Simulation.Protocols.OSPF");
            AddProtocol(NetworkProtocolType.BGP, "NetForge.Simulation.Protocols.BGP.BgpProtocol", "NetForge.Simulation.Protocols.BGP");
            AddProtocol(NetworkProtocolType.EIGRP, "NetForge.Simulation.Protocols.EIGRP.EigrpProtocol", "NetForge.Simulation.Protocols.EIGRP");
            AddProtocol(NetworkProtocolType.RIP, "NetForge.Simulation.Protocols.RIP.RipProtocol", "NetForge.Simulation.Protocols.RIP");
            AddProtocol(NetworkProtocolType.STP, "NetForge.Simulation.Protocols.STP.StpProtocol", "NetForge.Simulation.Protocols.STP");
            AddProtocol(NetworkProtocolType.CDP, "NetForge.Simulation.Protocols.CDP.CdpProtocol", "NetForge.Simulation.Protocols.CDP");
            AddProtocol(NetworkProtocolType.HSRP, "NetForge.Simulation.Protocols.HSRP.HsrpProtocol", "NetForge.Simulation.Protocols.HSRP");
            AddProtocol(NetworkProtocolType.VRRP, "NetForge.Simulation.Protocols.VRRP.VrrpProtocol", "NetForge.Simulation.Protocols.VRRP");
            AddProtocol(NetworkProtocolType.LLDP, "NetForge.Simulation.Protocols.LLDP.LldpProtocol", "NetForge.Simulation.Protocols.LLDP");
            AddProtocol(NetworkProtocolType.ARP, "NetForge.Simulation.Protocols.ARP.ArpProtocol", "NetForge.Simulation.Protocols.ARP");
            AddProtocol(NetworkProtocolType.SSH, "NetForge.Simulation.Protocols.SSH.SshProtocol", "NetForge.Simulation.Protocols.SSH");
            AddProtocol(NetworkProtocolType.TELNET, "NetForge.Simulation.Protocols.Telnet.TelnetProtocol", "NetForge.Simulation.Protocols.Telnet");
            AddProtocol(NetworkProtocolType.SNMP, "NetForge.Simulation.Protocols.SNMP.SnmpProtocol", "NetForge.Simulation.Protocols.SNMP");

            // Add CLI handlers
            AddHandler("ShowVersion", "show version", 
                "NetForge.Simulation.CliHandlers.Cisco.Show.ShowVersionHandler", 
                HandlerType.Show);
            
            AddHandler("ShowRunningConfig", "show running-config", 
                "NetForge.Simulation.CliHandlers.Cisco.Show.ShowRunningConfigHandler", 
                HandlerType.Show);
            
            AddHandler("ShowInterfaces", "show interfaces", 
                "NetForge.Simulation.CliHandlers.Cisco.Show.ShowInterfacesHandler", 
                HandlerType.Show);
            
            AddHandler("Configure", "configure terminal", 
                "NetForge.Simulation.CliHandlers.Cisco.Configuration.ConfigureTerminalHandler", 
                HandlerType.Configuration);
            
            AddHandler("Interface", "interface", 
                "NetForge.Simulation.CliHandlers.Cisco.Configuration.InterfaceHandler", 
                HandlerType.Interface);
            
            AddHandler("RouterOspf", "router ospf", 
                "NetForge.Simulation.CliHandlers.Cisco.Routing.RouterOspfHandler", 
                HandlerType.Routing);
            
            AddHandler("RouterBgp", "router bgp", 
                "NetForge.Simulation.CliHandlers.Cisco.Routing.RouterBgpHandler", 
                HandlerType.Routing);
            
            AddHandler("Enable", "enable", 
                "NetForge.Simulation.CliHandlers.Cisco.Basic.EnableHandler", 
                HandlerType.Basic);
            
            AddHandler("Exit", "exit", 
                "NetForge.Simulation.CliHandlers.Cisco.Basic.ExitHandler", 
                HandlerType.Basic);
            
            AddHandler("Ping", "ping", 
                "NetForge.Simulation.CliHandlers.Cisco.Diagnostic.PingHandler", 
                HandlerType.Diagnostic);
            
            AddHandler("Traceroute", "traceroute", 
                "NetForge.Simulation.CliHandlers.Cisco.Diagnostic.TracerouteHandler", 
                HandlerType.Diagnostic);
            
            AddHandler("WriteMemory", "write memory", 
                "NetForge.Simulation.CliHandlers.Cisco.System.WriteMemoryHandler", 
                HandlerType.System);
            
            AddHandler("CopyRunStart", "copy running-config startup-config", 
                "NetForge.Simulation.CliHandlers.Cisco.System.CopyRunStartHandler", 
                HandlerType.System);
            
            AddHandler("Reload", "reload", 
                "NetForge.Simulation.CliHandlers.Cisco.System.ReloadHandler", 
                HandlerType.System);
        }
    }
}