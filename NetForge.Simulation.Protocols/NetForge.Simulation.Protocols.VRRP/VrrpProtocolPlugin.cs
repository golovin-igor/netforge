using NetForge.Interfaces.Devices;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.VRRP
{
    /// <summary>
    /// Plugin for the VRRP (Virtual Router Redundancy Protocol) implementation
    /// Enables auto-discovery and registration of the VRRP protocol
    /// </summary>
    public class VrrpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "VRRP Protocol Plugin";
        public override NetworkProtocolType ProtocolType => NetworkProtocolType.VRRP;
        public override int Priority => 150; // Medium-high priority for redundancy protocol

        public override IDeviceProtocol CreateProtocol()
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
