using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.CommonHandlers
{
    /// <summary>
    /// Common enable command handler for entering privileged mode
    /// </summary>
    public class CommonEnableCommandHandler : BaseCliHandler
    {
        public CommonEnableCommandHandler() : base("enable", "Enter privileged mode")
        {
            AddAlias("en");
            AddAlias("ena");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            var currentMode = context.Device.GetCurrentModeEnum();

            if (currentMode == DeviceMode.User)
            {
                context.Device.SetCurrentModeEnum(DeviceMode.Privileged);
                return Success("");
            }
            else
            {
                return Error(CliErrorType.InvalidMode, "% Already in privileged mode\n");
            }
        }
    }
}


