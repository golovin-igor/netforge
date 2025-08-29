using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

/// <summary>
/// Handles 'switchport' command in interface configuration mode
/// </summary>
public class SwitchportCommandHandler : VendorAgnosticCliHandler
{
    public SwitchportCommandHandler() : base("switchport", "Configure switchport parameters")
    {
        AddSubHandler("mode", new SwitchportModeHandler());
        AddSubHandler("access", new SwitchportAccessHandler());
        AddSubHandler("trunk", new SwitchportTrunkHandler());
        AddSubHandler("voice", new SwitchportVoiceHandler());
    }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        if (!IsVendor(context, "Cisco"))
        {
            return RequireVendor(context, "Cisco");
        }

        if (!IsInMode(context, "interface"))
        {
            return Error(CliErrorType.InvalidMode,
                "% This command can only be used in interface configuration mode");
        }

        var device = context.Device;
        if (device == null)
        {
            return Error(CliErrorType.ExecutionError, "% Device not available");
        }

        // Basic switchport command without parameters
        if (context.CommandParts.Length == 1)
        {
            try
            {
                var currentInterface = context.Device.GetCurrentInterface();
                if (string.IsNullOrEmpty(currentInterface))
                {
                    return Error(CliErrorType.InvalidMode, "% No interface selected");
                }

                var iface = device.GetInterface(currentInterface);
                if (iface != null)
                {
                    // Enable switchport mode (default to access)
                    iface.SwitchportMode = "access";
                    device.AddLogEntry($"Switchport enabled on interface {currentInterface}");
                    return Success("");
                }

                return Error(CliErrorType.ExecutionError, "% Interface not found");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error enabling switchport: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error enabling switchport: {ex.Message}");
            }
        }

        return Error(CliErrorType.IncompleteCommand, "% Incomplete command");
    }
}
