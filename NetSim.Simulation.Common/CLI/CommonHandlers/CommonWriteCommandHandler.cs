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

        protected override CliResult ExecuteCommand(CliContext context)
        {
            return Success("Configuration saved.\n");
        }
    }
}


