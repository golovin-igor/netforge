using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.Common.CLI.CommonHandlers
{
    /// <summary>
    /// Common exit command handler
    /// </summary>
    public class CommonExitCommandHandler : BaseCliHandler
    {
        public CommonExitCommandHandler() : base("exit", "Exit from current mode")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            var currentMode = context.Device.GetCurrentMode();

            switch (currentMode)
            {
                case "interface":
                case "vlan":
                case "router":
                case "acl":
                    context.Device.SetCurrentMode("config");
                    context.Device.SetCurrentInterface("");
                    return Success("");

                case "config":
                    context.Device.SetCurrentMode("privileged");
                    return Success("");

                case "privileged":
                    context.Device.SetCurrentMode("user");
                    return Success("");

                default:
                    return Success("");
            }
        }
    }
}

