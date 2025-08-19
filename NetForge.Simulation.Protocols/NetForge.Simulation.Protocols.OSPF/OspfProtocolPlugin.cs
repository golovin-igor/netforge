using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.OSPF
{
    /// <summary>
    /// OSPF protocol plugin for auto-discovery and registration
    /// </summary>
    public class OspfProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "OSPF Protocol Plugin";
        public override string Version => "2.0.0";
        public override ProtocolType ProtocolType => ProtocolType.OSPF;
        public override int Priority => 110; // OSPF administrative distance
        
        public override IDeviceProtocol CreateProtocol()
        {
            return new OspfProtocol();
        }
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            // OSPF is a standard protocol supported by most vendors
            return new[] { "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "Generic" };
        }
        
        public override bool IsValid()
        {
            try
            {
                // Test that we can create a protocol instance
                var protocol = CreateProtocol();
                return protocol != null && 
                       protocol.Type == ProtocolType.OSPF && 
                       !string.IsNullOrEmpty(protocol.Name);
            }
            catch
            {
                return false;
            }
        }
    }
}
