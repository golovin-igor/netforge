using NetForge.Interfaces.Devices;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.RIP;

public class RipProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "RIP Protocol Plugin";
    public override NetworkProtocolType ProtocolType => NetworkProtocolType.RIP;
    public override int Priority => 120; // Administrative distance as priority

    public override IDeviceProtocol CreateProtocol()
    {
        return new RipProtocol();
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Cisco", "Juniper", "Generic" };
    }
}
