using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Anira.Basic
{
    /// <summary>
    /// Anira exit command handler
    /// </summary>
    public class ExitCommandHandler() : VendorAgnosticCliHandler("exit", "Exit current mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }

            var currentMode = context.Device.GetCurrentMode();

            switch (currentMode)
            {
                case "interface":
                    SetMode(context, "config");
                    break;
                case "config":
                    SetMode(context, "privileged");
                    break;
                case "privileged":
                    SetMode(context, "user");
                    break;
                case "user":
                    // Stay in user mode
                    break;
                default:
                    SetMode(context, "user");
                    break;
            }

            return Success("");
        }
    }
}
