using System.Collections.Generic;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.HSRP
{
    /// <summary>
    /// Plugin for the HSRP (Hot Standby Router Protocol) implementation
    /// Enables auto-discovery and registration of the HSRP protocol
    /// </summary>
    public class HsrpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "HSRP Protocol Plugin";
        public override NetworkProtocolType ProtocolType => NetworkProtocolType.HSRP;
        public override int Priority => 150; // Medium-high priority for redundancy protocol

        public override IDeviceProtocol CreateProtocol()
        {
            return new HsrpProtocol();
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // HSRP is Cisco proprietary but can be simulated for other vendors in lab environments
            return new[] { "Cisco", "Generic" };
        }
    }
}
