using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Interfaces;

namespace NetForge.Simulation.Protocols.ISIS;

public class IsisProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "IS-IS Protocol Plugin";
    public override ProtocolType ProtocolType => ProtocolType.ISIS;
    public override int Priority => 115; // IS-IS administrative distance

    public override IEnhancedDeviceProtocol CreateProtocol()
    {
        return new IsisProtocol();
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Cisco", "Juniper", "Nokia", "Generic" }; // IS-IS is multi-vendor
    }
}
