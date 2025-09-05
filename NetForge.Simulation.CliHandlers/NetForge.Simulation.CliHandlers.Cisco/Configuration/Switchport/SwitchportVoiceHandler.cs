using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

/// <summary>
/// Handles 'switchport voice' command
/// </summary>
public class SwitchportVoiceHandler : VendorAgnosticCliHandler
{
    public SwitchportVoiceHandler() : base("voice", "Set voice VLAN configuration")
    {
        AddSubHandler("vlan", new SwitchportVoiceVlanHandler());
    }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        return Error(CliErrorType.IncompleteCommand,
            "% Incomplete command. Available options: vlan");
    }
}
