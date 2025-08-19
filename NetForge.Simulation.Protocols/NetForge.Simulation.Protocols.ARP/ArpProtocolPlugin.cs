using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.ARP
{
    /// <summary>
    /// ARP protocol plugin for auto-discovery and registration
    /// </summary>
    public class ArpProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "ARP Protocol Plugin";
        public override string Version => "1.0.0";
        public override ProtocolType ProtocolType => ProtocolType.ARP;
        public override int Priority => 1000; // High priority as ARP is fundamental
        
        public override IDeviceProtocol CreateProtocol()
        {
            return new ArpProtocol();
        }
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            // ARP is a fundamental protocol supported by all vendors
            return new[] { "Generic", "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "F5", "Fortinet" };
        }
        
        public override bool IsValid()
        {
            try
            {
                // Test that we can create a protocol instance
                var protocol = CreateProtocol();
                return protocol != null && 
                       protocol.Type == ProtocolType.ARP && 
                       !string.IsNullOrEmpty(protocol.Name);
            }
            catch
            {
                return false;
            }
        }
    }
}
