using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

public class SwitchportTrunkEncapsulationDot1qHandler() : VendorAgnosticCliHandler("dot1q", "Set 802.1Q encapsulation")
{
    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        if (!IsVendor(context, "Cisco"))
        {
            return RequireVendor(context, "Cisco");
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

            device.AddLogEntry($"Interface {currentInterface} trunk encapsulation set to 802.1Q");
            return Success("");
        }
        catch (Exception ex)
        {
            device.AddLogEntry($"Error setting trunk encapsulation: {ex.Message}");
            return Error(CliErrorType.ExecutionError, $"% Error setting trunk encapsulation: {ex.Message}");
        }
    }
}
