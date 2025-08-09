
using NetSim.Simulation.CliHandlers;

namespace NetSim.Simulation.Interfaces
{
    /// <summary>
    /// Interface for CLI command handlers
    /// </summary>
    public interface ICliHandler
    {
        /// <summary>
        /// Checks if this handler can handle the given command
        /// </summary>
        bool CanHandle(CliContext context);

        /// <summary>
        /// Handles the command synchronously
        /// </summary>
        CliResult Handle(CliContext context);

        /// <summary>
        /// Handles the command asynchronously
        /// </summary>
        Task<CliResult> HandleAsync(CliContext context);

        /// <summary>
        /// Gets help text for this command
        /// </summary>
        string GetHelp();

        /// <summary>
        /// Gets possible completions for tab completion
        /// </summary>
        List<string> GetCompletions(CliContext context);

        /// <summary>
        /// Gets information about this command
        /// </summary>
        (string, string)? GetCommandInfo();

        /// <summary>
        /// Checks if this handler could potentially handle a command prefix
        /// </summary>
        bool CanHandlePrefix(CliContext context);

        /// <summary>
        /// Gets available sub-commands
        /// </summary>
        List<(string, string)> GetSubCommands(CliContext context);

        /// <summary>
        /// Gets sub-handlers for this command
        /// </summary>
        Dictionary<string, ICliHandler> GetSubHandlers();
    }
} 
