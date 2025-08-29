using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration
{
    /// <summary>
    /// Handles 'cdp' command in configuration mode
    /// </summary>
    public class CdpCommandHandler : VendorAgnosticCliHandler
    {
        public CdpCommandHandler() : base("cdp", "Configure CDP (Cisco Discovery Protocol)")
        {
            AddSubHandler("run", new CdpRunHandler());
            AddSubHandler("enable", new CdpEnableHandler());
            AddSubHandler("timer", new CdpTimerHandler());
            AddSubHandler("holdtime", new CdpHoldtimeHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "config") && !IsInMode(context, "interface"))
            {
                return Error(CliErrorType.InvalidMode, 
                    "% This command can only be used in config or interface mode");
            }

            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: run, enable, timer, holdtime");
        }
    }

    public class CdpRunHandler() : VendorAgnosticCliHandler("run", "Enable CDP globally")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                device.AddLogEntry("CDP enabled globally");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error enabling CDP globally: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error enabling CDP globally: {ex.Message}");
            }
        }
    }

    public class CdpEnableHandler() : VendorAgnosticCliHandler("enable", "Enable CDP on interface")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
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

            var device = context.Device as NetworkDevice;
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

                device.AddLogEntry($"CDP enabled on interface {currentInterface}");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error enabling CDP on interface: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error enabling CDP on interface: {ex.Message}");
            }
        }
    }

    public class CdpTimerHandler() : VendorAgnosticCliHandler("timer", "Set CDP update timer")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: cdp timer <seconds>");
            }

            if (!int.TryParse(context.CommandParts[1], out int seconds))
            {
                return Error(CliErrorType.InvalidParameter, "% Invalid timer value");
            }

            if (seconds < 5 || seconds > 254)
            {
                return Error(CliErrorType.InvalidParameter, 
                    "% Invalid timer value. Range is 5-254 seconds");
            }

            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                device.AddLogEntry($"CDP timer set to {seconds} seconds");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting CDP timer: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting CDP timer: {ex.Message}");
            }
        }
    }

    public class CdpHoldtimeHandler() : VendorAgnosticCliHandler("holdtime", "Set CDP holdtime")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command. Usage: cdp holdtime <seconds>");
            }

            if (!int.TryParse(context.CommandParts[1], out int seconds))
            {
                return Error(CliErrorType.InvalidParameter, "% Invalid holdtime value");
            }

            if (seconds < 10 || seconds > 65535)
            {
                return Error(CliErrorType.InvalidParameter, 
                    "% Invalid holdtime value. Range is 10-65535 seconds");
            }

            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                device.AddLogEntry($"CDP holdtime set to {seconds} seconds");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error setting CDP holdtime: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error setting CDP holdtime: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Handles 'no cdp' commands for disabling CDP
    /// </summary>
    public class NoCdpCommandHandler : VendorAgnosticCliHandler
    {
        public NoCdpCommandHandler() : base("cdp", "Disable CDP")
        {
            AddSubHandler("run", new NoCdpRunHandler());
            AddSubHandler("enable", new NoCdpEnableHandler());
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            return Error(CliErrorType.IncompleteCommand, 
                "% Incomplete command. Available options: run, enable");
        }
    }

    public class NoCdpRunHandler() : VendorAgnosticCliHandler("run", "Disable CDP globally")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            var device = context.Device as NetworkDevice;
            if (device == null)
            {
                return Error(CliErrorType.ExecutionError, "% Device not available");
            }

            try
            {
                device.AddLogEntry("CDP disabled globally");
                return Success("");
            }
            catch (Exception ex)
            {
                device.AddLogEntry($"Error disabling CDP globally: {ex.Message}");
                return Error(CliErrorType.ExecutionError, $"% Error disabling CDP globally: {ex.Message}");
            }
        }
    }

    public class NoCdpEnableHandler() : VendorAgnosticCliHandler("enable", "Disable CDP on interface")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
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

            var device = context.Device as NetworkDevice;
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
} 
