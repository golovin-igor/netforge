using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Interfaces;

namespace NetForge.Interfaces.Cli;

public interface ICliHandlerManager
{
    /// <summary>
    /// Registers a CLI handler
    /// </summary>
    void RegisterHandler(ICliHandler handler);

    /// <summary>
    /// Asynchronously processes a command using the registered handlers
    /// </summary>
    /// <returns>The command result</returns>
    Task<CliResult> ProcessCommandAsync(string command);

    /// <summary>
    /// Gets completions for tab completion
    /// </summary>
    List<string> GetCompletions(string command);

    /// <summary>
    /// Gets help text for a specific command
    /// </summary>
    string GetCommandHelp(string command);
}
