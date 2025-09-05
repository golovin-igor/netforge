using NetForge.Interfaces.Devices;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.SNMP;

public class SnmpProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "SNMP Protocol Plugin";
    public override NetworkProtocolType ProtocolType => NetworkProtocolType.SNMP;
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
