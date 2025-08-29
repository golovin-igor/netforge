using System.Collections.Generic;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.STP
{
    /// <summary>
    /// Plugin for the STP (Spanning Tree Protocol) implementation
    /// Enables auto-discovery and registration of the STP protocol
    /// </summary>
    public class StpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "STP Protocol Plugin";
        public override NetworkProtocolType ProtocolType => NetworkProtocolType.STP;
        public override int Priority => 200; // High priority for infrastructure protocol

        public override IDeviceProtocol CreateProtocol()
        {
            return new StpProtocol();
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // STP is IEEE 802.1D standard supported by all vendors
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "HP", "Generic" };
        }
    }
}
