using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

public class SwitchportTrunkAllowedHandler : VendorAgnosticCliHandler
{
    public SwitchportTrunkAllowedHandler() : base("allowed", "Set allowed VLANs on trunk")
    {
        AddSubHandler("vlan", new SwitchportTrunkAllowedVlanHandler());
    }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        return Error(CliErrorType.IncompleteCommand,
            "% Incomplete command. Available options: vlan");
    }
}
