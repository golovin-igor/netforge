using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Anira.Basic
{
    /// <summary>
    /// Anira enable command handler
    /// </summary>
    public class EnableCommandHandler : VendorAgnosticCliHandler
    {
        public EnableCommandHandler() : base("enable", "Enter privileged mode")
        {
            AddAlias("en");
            AddAlias("ena");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Anira"))
            {
                return RequireVendor(context, "Anira");
            }

            if (IsInMode(context, "privileged"))
            {
                return Success("");
            }

            SetMode(context, "privileged");
            return Success("");
        }
    }
}
