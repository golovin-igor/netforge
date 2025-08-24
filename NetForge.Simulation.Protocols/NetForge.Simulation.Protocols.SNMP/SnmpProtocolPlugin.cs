using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Interfaces;

namespace NetForge.Simulation.Protocols.SNMP;

public class SnmpProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "SNMP Protocol Plugin";
    public override ProtocolType ProtocolType => ProtocolType.SNMP;
    public override int Priority => 200; // High priority for management protocol

    public override IDeviceProtocol CreateProtocol()
    {
        return new SnmpProtocol();
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Generic", "Cisco", "Juniper", "Arista" };
    }
}
