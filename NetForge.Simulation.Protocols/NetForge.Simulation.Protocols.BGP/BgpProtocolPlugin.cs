using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.BGP
{
    /// <summary>
    /// BGP protocol plugin for auto-discovery and registration
    /// </summary>
    public class BgpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "BGP Protocol Plugin";
        public override string Version => "4.0.0";
        public override ProtocolType ProtocolType => ProtocolType.BGP;
        public override int Priority => 200; // IBGP administrative distance

        public override IDeviceProtocol CreateProtocol()
        {
            return new BgpProtocol();
        }

        public override IEnumerable<string> GetSupportedVendors()
        {
            // BGP-4 is a standard protocol supported by all major vendors
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "Quagga", "FRRouting", "Generic" };
        }

        public override bool IsValid()
        {
            try
            {
                // Test that we can create a protocol instance
                var protocol = CreateProtocol();
                return protocol != null &&
                       protocol.Type == ProtocolType.BGP &&
                       !string.IsNullOrEmpty(protocol.Name) &&
                       protocol.Version.StartsWith("4."); // BGP-4
            }
            catch
            {
                return false;
            }
        }

        public override bool SupportsVendor(string vendorName)
        {
            // BGP is universally supported
            return true;
        }
    }
}
