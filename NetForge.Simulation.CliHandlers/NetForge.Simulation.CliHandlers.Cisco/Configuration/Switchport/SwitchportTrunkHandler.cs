using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

/// <summary>
/// Handles 'switchport trunk' command
/// </summary>
public class SwitchportTrunkHandler : VendorAgnosticCliHandler
{
    public SwitchportTrunkHandler() : base("trunk", "Set trunk mode configuration")
    {
        AddSubHandler("encapsulation", new SwitchportTrunkEncapsulationHandler());
        AddSubHandler("allowed", new SwitchportTrunkAllowedHandler());
        AddSubHandler("native", new SwitchportTrunkNativeHandler());
    }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        return Error(CliErrorType.IncompleteCommand,
            "% Incomplete command. Available options: encapsulation, allowed, native");
    }
}
