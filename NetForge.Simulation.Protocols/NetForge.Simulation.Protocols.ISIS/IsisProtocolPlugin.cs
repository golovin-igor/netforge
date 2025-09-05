using NetForge.Interfaces.Devices;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.ISIS;

public class IsisProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "IS-IS Protocol Plugin";
    public override NetworkProtocolType ProtocolType => NetworkProtocolType.ISIS;
    public override int Priority => 115; // IS-IS administrative distance

    public override IDeviceProtocol CreateProtocol()
    {
        return new IsisProtocol();
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Cisco", "Juniper", "Nokia", "Generic" }; // IS-IS is multi-vendor
    }
}
