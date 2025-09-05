using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

public class SwitchportModeDynamicAutoHandler() : VendorAgnosticCliHandler("auto", "Set to dynamic auto mode")
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

            var iface = device.GetInterface(currentInterface);
            if (iface != null)
            {
                iface.SwitchportMode = "dynamic auto";
                device.AddLogEntry($"Interface {currentInterface} set to dynamic auto mode");
                return Success("");
            }

            return Error(CliErrorType.ExecutionError, "% Interface not found");
        }
        catch (Exception ex)
        {
            device.AddLogEntry($"Error setting dynamic auto mode: {ex.Message}");
            return Error(CliErrorType.ExecutionError, $"% Error setting dynamic auto mode: {ex.Message}");
        }
    }
}
