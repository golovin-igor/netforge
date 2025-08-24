using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Interfaces;

namespace NetForge.Simulation.Protocols.RIP;

public class RipProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "RIP Protocol Plugin";
    public override ProtocolType ProtocolType => ProtocolType.RIP;
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
