namespace NetSim.Simulation.CliHandlers.Common
{
    /// <summary>
    /// Generic write command handler for vendors without a specific implementation
    /// </summary>
    public class CommonWriteCommandHandler : BaseCliHandler
    {
        public CommonWriteCommandHandler() : base("write", "Save configuration")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Success("Configuration saved.\n");
        }
    }
}


