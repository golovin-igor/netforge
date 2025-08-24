using System.Collections.Generic;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.EIGRP
{
    /// <summary>
    /// Plugin for the EIGRP (Enhanced Interior Gateway Routing Protocol) implementation
    /// Enables auto-discovery and registration of the EIGRP protocol
    /// </summary>
    public class EigrpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "EIGRP Protocol Plugin";
        public override ProtocolType ProtocolType => ProtocolType.EIGRP;
        public override int Priority => 90; // Administrative distance for EIGRP internal routes

        public override IDeviceProtocol CreateProtocol()
        {
            return new EigrpProtocol();
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // EIGRP is Cisco proprietary but can be simulated for other vendors in lab environments
            return new[] { "Cisco", "Generic" };
        }
    }
}
