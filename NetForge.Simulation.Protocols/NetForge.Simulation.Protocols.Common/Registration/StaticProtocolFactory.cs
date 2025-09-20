using NetForge.Interfaces.Protocol;
using NetForge.Simulation.Protocols.ARP;
using NetForge.Simulation.Protocols.BGP;
using NetForge.Simulation.Protocols.CDP;
using NetForge.Simulation.Protocols.EIGRP;
using NetForge.Simulation.Protocols.HSRP;
using NetForge.Simulation.Protocols.HTTP;
using NetForge.Simulation.Protocols.IGRP;
using NetForge.Simulation.Protocols.ISIS;
using NetForge.Simulation.Protocols.LLDP;
using NetForge.Simulation.Protocols.OSPF;
using NetForge.Simulation.Protocols.RIP;
using NetForge.Simulation.Protocols.SNMP;
using NetForge.Simulation.Protocols.SSH;
using NetForge.Simulation.Protocols.STP;
using NetForge.Simulation.Protocols.Telnet;
using NetForge.Simulation.Protocols.VRRP;

namespace NetForge.Simulation.Protocols.Common.Registration
{
    /// <summary>
    /// Static protocol factory that creates protocol instances without reflection
    /// All protocol instantiation is done through direct constructor calls
    /// </summary>
    public static class StaticProtocolFactory
    {
        /// <summary>
        /// Create ARP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateArp() => new ArpProtocol();

        /// <summary>
        /// Create BGP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateBgp() => new BgpProtocol();

        /// <summary>
        /// Create CDP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateCdp() => new CdpProtocol();

        /// <summary>
        /// Create EIGRP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateEigrp() => new EigrpProtocol();

        /// <summary>
        /// Create HSRP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateHsrp() => new HsrpProtocol();

        /// <summary>
        /// Create HTTP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateHttp() => new HttpProtocol();

        /// <summary>
        /// Create IGRP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateIgrp() => new IgrpProtocol();

        /// <summary>
        /// Create IS-IS protocol instance
        /// </summary>
        public static IDeviceProtocol CreateIsis() => new IsisProtocol();

        /// <summary>
        /// Create LLDP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateLldp() => new LldpProtocol();

        /// <summary>
        /// Create OSPF protocol instance
        /// </summary>
        public static IDeviceProtocol CreateOspf() => new OspfProtocol();

        /// <summary>
        /// Create RIP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateRip() => new RipProtocol();

        /// <summary>
        /// Create SNMP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateSnmp() => new SnmpProtocol();

        /// <summary>
        /// Create SSH protocol instance
        /// </summary>
        public static IDeviceProtocol CreateSsh() => new SshProtocol();

        /// <summary>
        /// Create STP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateStp() => new StpProtocol();

        /// <summary>
        /// Create Telnet protocol instance
        /// </summary>
        public static IDeviceProtocol CreateTelnet() => new TelnetProtocol();

        /// <summary>
        /// Create VRRP protocol instance
        /// </summary>
        public static IDeviceProtocol CreateVrrp() => new VrrpProtocol();
    }
}