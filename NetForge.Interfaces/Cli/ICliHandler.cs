
using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.Common.CLI.Interfaces
{
    /// <summary>
    /// Interface for CLI command handlers
    /// </summary>
    public interface ICliHandler
    {
        /// <summary>
        /// Checks if this handler can handle the given command
        /// </summary>
        bool CanHandle(ICliContext context);

        /// <summary>
        /// Handles the command synchronously
        /// </summary>
        CliResult Handle(ICliContext context);

        /// <summary>
        /// Handles the command asynchronously
        /// </summary>
        Task<CliResult> HandleAsync(ICliContext context);

        /// <summary>
        /// Gets help text for this command
        /// </summary>
        string GetHelp();

        /// <summary>
        /// Gets possible completions for tab completion
        /// </summary>
        List<string> GetCompletions(ICliContext context);

        /// <summary>
        /// Gets information about this command
        /// </summary>
        (string, string)? GetCommandInfo();

        /// <summary>
        /// Checks if this handler could potentially handle a command prefix
        /// </summary>
        bool CanHandlePrefix(ICliContext context);

        /// <summary>
        /// Gets available sub-commands
        /// </summary>
        List<(string, string)> GetSubCommands(ICliContext context);

        /// <summary>
        /// Gets sub-handlers for this command
        /// </summary>
        Dictionary<string, ICliHandler> GetSubHandlers();
    }
}
