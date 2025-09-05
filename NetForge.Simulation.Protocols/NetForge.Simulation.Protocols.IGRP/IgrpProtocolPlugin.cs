using NetForge.Interfaces.Devices;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Simulation.Protocols.IGRP;

public class IgrpProtocolPlugin : ProtocolPluginBase
{
    public override string PluginName => "IGRP Protocol Plugin";
    public override NetworkProtocolType ProtocolType => NetworkProtocolType.IGRP;
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
