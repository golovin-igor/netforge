using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.CommonHandlers
{
    /// <summary>
    /// Common disable command handler for exiting privileged mode
    /// </summary>
    public class CommonDisableCommandHandler() : BaseCliHandler("disable", "Exit privileged mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            var currentMode = context.Device.GetCurrentModeEnum();

            if (currentMode == DeviceMode.Privileged)
            {
                context.Device.SetCurrentModeEnum(DeviceMode.User);
                return Success("");
            }
            else
            {
                return Error(CliErrorType.InvalidMode, "% Not in privileged mode\n");
            }
        }
    }
}


