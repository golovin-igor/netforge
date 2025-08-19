using NetForge.Simulation.Common;
using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.SSH
{
    /// <summary>
    /// Plugin for SSH protocol discovery and instantiation
    /// </summary>
    public class SshProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "SSH Protocol Plugin";
        public override string Version => "2.0.0";
        public override ProtocolType ProtocolType => ProtocolType.SSH;
        public override int Priority => 900; // High priority for management protocol
        
        public override IDeviceProtocol CreateProtocol()
        {
            return new SshProtocol();
        }
        
        public override IEnumerable<string> GetSupportedVendors()
        {
            // All vendors support SSH for management
            return new[] 
            { 
                "Generic", 
                "Cisco", 
                "Juniper", 
                "Arista", 
                "Dell", 
                "Huawei", 
                "Nokia", 
                "F5", 
                "Fortinet",
                "HPE",
                "Extreme",
                "Mikrotik",
                "Ubiquiti"
            };
        }
        
        public override bool SupportsVendor(string vendorName)
        {
            // SSH is a standard protocol supported by virtually all network vendors
            return !string.IsNullOrWhiteSpace(vendorName);
        }
    }
}
