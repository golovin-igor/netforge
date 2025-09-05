using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.Common.CLI.CommonHandlers
{
    /// <summary>
    /// Generic hostname command handler for vendors without a specific implementation
    /// </summary>
    public class CommonHostnameCommandHandler() : BaseCliHandler("hostname", "Set system host name")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command. Usage: hostname <name>");
            }

            var mode = context.Device.GetCurrentMode();
            if (mode != "config" && mode != "configuration" && mode != "system-view")
            {
                return Error(CliErrorType.InvalidMode, "Command not available in current mode");
            }

            context.Device.SetHostname(context.CommandParts[1]);
            return Success("");
        }
    }
}



