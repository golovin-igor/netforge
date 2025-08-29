using NetForge.Interfaces.Devices;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.CDP
{
    /// <summary>
    /// CDP protocol plugin for auto-discovery and registration
    /// </summary>
    public class CdpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "CDP Protocol Plugin";
        public override string Version => "2.0.0";
        public override NetworkProtocolType NetworkProtocolType => NetworkProtocolType.CDP;
        public override int Priority => 200; // Higher priority than generic protocols

        public override IDeviceProtocol CreateProtocol()
        {
            return new CdpProtocol();
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // CDP is Cisco proprietary but can be simulated on other vendors
            return new[] { "Cisco", "Generic" };
        }

        public override bool IsValid()
        {
            try
            {
                // Test that we can create a protocol instance
                var protocol = CreateProtocol();
                return protocol != null &&
                       protocol.Type == NetworkProtocolType.CDP &&
                       !string.IsNullOrEmpty(protocol.Name);
            }
            catch
            {
                return false;
            }
        }
    }
}
