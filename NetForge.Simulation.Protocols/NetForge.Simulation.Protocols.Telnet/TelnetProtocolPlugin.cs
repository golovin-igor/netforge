using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Interfaces;

namespace NetForge.Simulation.Protocols.Telnet
{
    /// <summary>
    /// Plugin for auto-discovery of the Telnet protocol
    /// </summary>
    public class TelnetProtocolPlugin : ProtocolPluginBase
    {
        public override string PluginName => "Telnet Protocol Plugin";
        public override string Version => "1.0.0";
        public override ProtocolType ProtocolType => ProtocolType.TELNET;
        public override int Priority => 1000; // Highest priority for management protocol

        public override IEnhancedDeviceProtocol CreateProtocol() => new TelnetProtocol();

        public override IEnumerable<string> GetSupportedVendors()
        {
            // All vendors support Telnet for management
            return new[] { "Generic", "Cisco", "Juniper", "Arista", "Dell", "Huawei", "Nokia", "F5", "Fortinet", "Alcatel", "Anira", "Broadcom", "Extreme", "Linux", "MikroTik" };
        }
    }
}
