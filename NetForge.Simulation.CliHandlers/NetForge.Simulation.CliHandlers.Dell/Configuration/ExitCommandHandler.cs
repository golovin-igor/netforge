using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Dell.Configuration;

/// <summary>
/// Exit command handler for configuration modes
/// </summary>
public class ExitCommandHandler() : VendorAgnosticCliHandler("exit", "Exit current configuration mode")
{
    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        if (!IsVendor(context, "Dell"))
        {
            return RequireVendor(context, "Dell");
        }

        var currentMode = GetCurrentMode(context);

        var newMode = currentMode switch
        {
            "interface" => "config",
            "vlan" => "config",
            "router" => "config",
            "config" => "exec",
            _ => "exec"
        };

        SetMode(context, newMode);

        // Clear context-specific settings
        if (currentMode == "interface")
        {
            SetCurrentInterface(context, "");
        }
        else if (currentMode == "router")
        {
            SetCurrentProtocol(context, "");
        }

        return Success("");
    }
}
