using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

/// <summary>
/// Handles 'switchport mode' command
/// </summary>
public class SwitchportModeHandler : VendorAgnosticCliHandler
{
    public SwitchportModeHandler() : base("mode", "Set switchport mode")
    {
        AddSubHandler("access", new SwitchportModeAccessHandler());
        AddSubHandler("trunk", new SwitchportModeTrunkHandler());
        AddSubHandler("dynamic", new SwitchportModeDynamicHandler());
    }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        return Error(CliErrorType.IncompleteCommand,
            "% Incomplete command. Available options: access, trunk, dynamic");
    }
}
