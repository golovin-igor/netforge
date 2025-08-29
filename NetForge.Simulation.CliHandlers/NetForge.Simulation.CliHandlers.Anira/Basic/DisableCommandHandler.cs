using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Anira.Basic
{
    /// <summary>
    /// Anira disable command handler
    /// </summary>
    public class DisableCommandHandler() : VendorAgnosticCliHandler("disable", "Exit privileged mode")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode, "Not in privileged mode");
            }

            SetMode(context, "user");
            return Success("");
        }
    }
}
