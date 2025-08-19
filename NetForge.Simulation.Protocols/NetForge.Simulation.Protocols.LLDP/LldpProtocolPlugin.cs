using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.LLDP
{
    /// <summary>
    /// LLDP protocol plugin for auto-discovery and registration
    /// IEEE 802.1AB - Link Layer Discovery Protocol
    /// </summary>
    public class LldpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "LLDP Protocol Plugin";
        public override string Version => "1.0.0";
        public override ProtocolType ProtocolType => ProtocolType.LLDP;
        public override int Priority => 150; // Higher priority than CDP for standards-based protocol
        
        public override IDeviceProtocol CreateProtocol()
        {
            return new LldpProtocol();
        }
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            // LLDP is IEEE 802.1AB standard supported by all major vendors
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "Generic" };
        }
        
        public override bool IsValid()
        {
            try
            {
                // Test that we can create a protocol instance
                var protocol = CreateProtocol();
                return protocol != null && 
                       protocol.Type == ProtocolType.LLDP && 
                       !string.IsNullOrEmpty(protocol.Name);
            }
            catch
            {
                return false;
            }
        }
        
        public override bool SupportsVendor(string vendorName)
        {
            // LLDP is universally supported IEEE standard
            return true;
        }
    }
}
