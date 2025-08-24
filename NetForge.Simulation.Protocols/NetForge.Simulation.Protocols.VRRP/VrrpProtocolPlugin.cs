using System.Collections.Generic;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Interfaces;

namespace NetForge.Simulation.Protocols.VRRP
{
    /// <summary>
    /// Plugin for the VRRP (Virtual Router Redundancy Protocol) implementation
    /// Enables auto-discovery and registration of the VRRP protocol
    /// </summary>
    public class VrrpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "VRRP Protocol Plugin";
        public override ProtocolType ProtocolType => ProtocolType.VRRP;
        public override int Priority => 150; // Medium-high priority for redundancy protocol

        public override IEnhancedDeviceProtocol CreateProtocol()
        {
            return new VrrpProtocol();
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // VRRP is an industry standard supported by most vendors
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "HP", "Generic" };
        }
    }
}
