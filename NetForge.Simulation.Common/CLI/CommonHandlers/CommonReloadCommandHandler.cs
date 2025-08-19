namespace NetForge.Simulation.CliHandlers.Common
{
    /// <summary>
    /// Generic reload command handler for vendors without a specific implementation
    /// </summary>
    public class CommonReloadCommandHandler : BaseCliHandler
    {
        public CommonReloadCommandHandler() : base("reload", "Restart the system")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            // Simply acknowledge the reload request
            return Success("System configuration has been modified. Save? [yes/no]: ");
        }
    }
}


