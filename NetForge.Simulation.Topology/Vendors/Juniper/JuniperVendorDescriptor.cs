using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Vendors.Juniper
{
    /// <summary>
    /// Vendor descriptor for Juniper devices
    /// </summary>
    public class JuniperVendorDescriptor : VendorDescriptorBase
    {
        public override string VendorName => "Juniper";
        public override string DisplayName => "Juniper Networks";
        public override string Description => "Juniper Networks networking equipment vendor descriptor";
        public override int Priority => 90;

        protected override void InitializeVendor()
        {
            // Configure Juniper-specific prompts
            ConfigurePrompts(">", "#", "#");
            AddPromptMode("configuration", "[edit]");
            AddPromptMode("interface", "[edit interfaces]");
            AddPromptMode("routing", "[edit routing-options]");
            AddPromptMode("protocols", "[edit protocols]");

            // Add supported device models
            AddModel(new DeviceModelDescriptor
            {
                ModelName = "MX480",
                ModelFamily = "MX",
                Description = "Juniper MX480 Router",
                DeviceType = DeviceType.Router,
                Features = { "BGP", "OSPF", "ISIS", "MPLS", "VPN", "QoS" }
            });

            AddModel(new DeviceModelDescriptor
            {
                ModelName = "EX4300",
                ModelFamily = "EX",
                Description = "Juniper EX4300 Switch",
                DeviceType = DeviceType.Switch,
                Features = { "VLAN", "RSTP", "LACP", "PoE", "Virtual Chassis" }
            });

            AddModel(new DeviceModelDescriptor
            {
                ModelName = "SRX340",
                ModelFamily = "SRX",
                Description = "Juniper SRX340 Firewall",
                DeviceType = DeviceType.Firewall,
                Features = { "Firewall", "VPN", "IPS", "NAT", "Clustering" }
            });

            // Add supported protocols
            AddProtocol(NetworkProtocolType.OSPF, "NetForge.Simulation.Protocols.OSPF.JuniperOspfProtocol");
            AddProtocol(NetworkProtocolType.BGP, "NetForge.Simulation.Protocols.BGP.JuniperBgpProtocol");
            AddProtocol(NetworkProtocolType.ISIS, "NetForge.Simulation.Protocols.ISIS.JuniperIsisProtocol");
            AddProtocol(NetworkProtocolType.RIP, "NetForge.Simulation.Protocols.RIP.JuniperRipProtocol");
            AddProtocol(NetworkProtocolType.STP, "NetForge.Simulation.Protocols.STP.JuniperStpProtocol");
            AddProtocol(NetworkProtocolType.LLDP, "NetForge.Simulation.Protocols.LLDP.JuniperLldpProtocol");
            AddProtocol(NetworkProtocolType.VRRP, "NetForge.Simulation.Protocols.VRRP.JuniperVrrpProtocol");
            AddProtocol(NetworkProtocolType.ARP, "NetForge.Simulation.Protocols.ARP.JuniperArpProtocol");
            AddProtocol(NetworkProtocolType.SSH, "NetForge.Simulation.Protocols.SSH.JuniperSshProtocol");
            AddProtocol(NetworkProtocolType.SNMP, "NetForge.Simulation.Protocols.SNMP.JuniperSnmpProtocol");

            // Add CLI handlers
            AddHandler("ShowVersion", "show version", 
                "NetForge.Simulation.CliHandlers.Juniper.Show.ShowVersionHandler", 
                HandlerType.Show);
            
            AddHandler("ShowConfiguration", "show configuration", 
                "NetForge.Simulation.CliHandlers.Juniper.Show.ShowConfigurationHandler", 
                HandlerType.Show);
            
            AddHandler("ShowInterfaces", "show interfaces", 
                "NetForge.Simulation.CliHandlers.Juniper.Show.ShowInterfacesHandler", 
                HandlerType.Show);
            
            AddHandler("Edit", "edit", 
                "NetForge.Simulation.CliHandlers.Juniper.Configuration.EditHandler", 
                HandlerType.Configuration);
            
            AddHandler("Set", "set", 
                "NetForge.Simulation.CliHandlers.Juniper.Configuration.SetHandler", 
                HandlerType.Configuration);
            
            AddHandler("Delete", "delete", 
                "NetForge.Simulation.CliHandlers.Juniper.Configuration.DeleteHandler", 
                HandlerType.Configuration);
            
            AddHandler("Commit", "commit", 
                "NetForge.Simulation.CliHandlers.Juniper.Configuration.CommitHandler", 
                HandlerType.Configuration);
            
            AddHandler("Rollback", "rollback", 
                "NetForge.Simulation.CliHandlers.Juniper.Configuration.RollbackHandler", 
                HandlerType.Configuration);
            
            AddHandler("Ping", "ping", 
                "NetForge.Simulation.CliHandlers.Juniper.Diagnostic.PingHandler", 
                HandlerType.Diagnostic);
            
            AddHandler("Traceroute", "traceroute", 
                "NetForge.Simulation.CliHandlers.Juniper.Diagnostic.TracerouteHandler", 
                HandlerType.Diagnostic);
            
            AddHandler("Request", "request", 
                "NetForge.Simulation.CliHandlers.Juniper.System.RequestHandler", 
                HandlerType.System);
        }
    }
}