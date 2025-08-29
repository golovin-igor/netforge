using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

/// <summary>
/// Handles 'switchport access' command
/// </summary>
public class SwitchportAccessHandler : VendorAgnosticCliHandler
{
    public SwitchportAccessHandler() : base("access", "Set access mode configuration")
    {
        AddSubHandler("vlan", new SwitchportAccessVlanHandler());
    }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        return Error(CliErrorType.IncompleteCommand,
            "% Incomplete command. Available options: vlan");
    }
}
