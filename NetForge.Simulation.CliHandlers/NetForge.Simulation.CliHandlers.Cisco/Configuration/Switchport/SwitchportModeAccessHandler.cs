using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

public class SwitchportModeAccessHandler() : VendorAgnosticCliHandler("access", "Set switchport to access mode")
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
                iface.SwitchportMode = "access";

                // Update running configuration
                var vendorContext = GetVendorContext(context);
                if (vendorContext?.Capabilities != null)
                {
                    vendorContext.Capabilities.AppendToRunningConfig(" switchport mode access");
                }

                device.AddLogEntry($"Interface {currentInterface} set to access mode");
                return Success("");
            }

            return Error(CliErrorType.ExecutionError, "% Interface not found");
        }
        catch (Exception ex)
        {
            device.AddLogEntry($"Error setting access mode: {ex.Message}");
            return Error(CliErrorType.ExecutionError, $"% Error setting access mode: {ex.Message}");
        }
    }
}
