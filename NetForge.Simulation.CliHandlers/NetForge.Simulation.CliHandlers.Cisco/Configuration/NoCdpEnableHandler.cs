using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

public class NoCdpEnableHandler() : VendorAgnosticCliHandler("enable", "Disable CDP on interface")
{
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

        try
        {
            var currentInterface = context.Device.GetCurrentInterface();
            if (string.IsNullOrEmpty(currentInterface))
            {
                return Error(CliErrorType.InvalidMode, "% No interface selected");
            }

            device.AddLogEntry($"CDP disabled on interface {currentInterface}");
            return Success("");
        }
        catch (Exception ex)
        {
            device.AddLogEntry($"Error disabling CDP on interface: {ex.Message}");
            return Error(CliErrorType.ExecutionError, $"% Error disabling CDP on interface: {ex.Message}");
        }
    }
}
