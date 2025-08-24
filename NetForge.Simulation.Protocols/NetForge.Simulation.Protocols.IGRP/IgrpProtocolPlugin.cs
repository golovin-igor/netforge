using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.IGRP;

public class IgrpProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "IGRP Protocol Plugin";
    public override ProtocolType ProtocolType => ProtocolType.IGRP;
    public override int Priority => 100; // IGRP administrative distance

    public override IDeviceProtocol CreateProtocol()
    {
        return new IgrpProtocol();
    }

    public override IEnumerable<string> GetSupportedVendors()
    {
        return new[] { "Cisco" }; // IGRP is Cisco proprietary
    }
}
