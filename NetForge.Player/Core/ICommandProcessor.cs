// TODO: Phase 1.1 - Implement Command Line Interface System
// This interface defines the core command processing functionality for the NetForge.Player

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetForge.Player.Core;

/// <summary>
/// Core interface for processing Player commands
/// </summary>
public interface ICommandProcessor
{
    // TODO: Implement command processing methods
    // - ParseCommand: Tokenize and validate command syntax
    // - ProcessCommandAsync: Execute commands asynchronously
    // - GetCompletionsAsync: Provide tab completion suggestions
    // - GetCommandHelp: Return help information for commands
    // - RegisterCommand: Dynamic command registration
    // - UnregisterCommand: Remove commands at runtime
    
    /// <summary>
    /// Process a command string asynchronously
    /// </summary>
    /// <param name="command">The command string to process</param>
    /// <returns>Command execution result</returns>
    Task<CommandResult> ProcessCommandAsync(string command);
    
    /// <summary>
    /// Get command completion suggestions
    /// </summary>
    /// <param name="partialCommand">Partial command for completion</param>
    /// <returns>List of completion suggestions</returns>
    Task<List<string>> GetCompletionsAsync(string partialCommand);
    
    /// <summary>
    /// Get help information for a command
    /// </summary>
    /// <param name="command">Command name</param>
    /// <returns>Command metadata and help</returns>
    CommandMetadata GetCommandHelp(string command);
    
    // TODO: Add command registration methods
    // void RegisterCommand<T>() where T : IPlayerCommand;
    // void UnregisterCommand(string commandName);
    // IEnumerable<string> GetAvailableCommands();
    // bool IsCommandAvailable(string commandName);
}

/// <summary>
/// Result of command execution
/// </summary>
public class CommandResult
{
    // TODO: Enhance CommandResult with comprehensive execution information
    // - Success/failure status
    // - Output messages and formatting
    // - Error details and suggestions
    // - Execution time and performance metrics
    // - Return values for chaining commands
    // - Context changes for mode switching
    
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    
    // TODO: Add additional result properties
    // public object? ReturnValue { get; set; }
    // public Dictionary<string, object> Context { get; set; } = new();
    // public List<string> Warnings { get; set; } = new();
    // public CommandResultType ResultType { get; set; }
}

/// <summary>
/// Command metadata for help and documentation
/// </summary>
public class CommandMetadata
{
    // TODO: Expand CommandMetadata with comprehensive command information
    // - Command aliases and shortcuts
    // - Parameter descriptions and types
    // - Usage examples and patterns
    // - Related commands and see-also references
    // - Command categories and tags
    // - Version and availability information
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Usage { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new();
    
    // TODO: Add parameter metadata
    // public List<CommandParameter> Parameters { get; set; } = new();
    // public List<string> Aliases { get; set; } = new();
    // public string Category { get; set; } = string.Empty;
    // public List<string> SeeAlso { get; set; } = new();
}

// TODO: Define command context for execution environment
// public class CommandContext
// {
//     public INetworkManager NetworkManager { get; set; }
//     public ISessionManager SessionManager { get; set; }
//     public PlayerConfiguration Configuration { get; set; }
//     public CancellationToken CancellationToken { get; set; }
//     public Dictionary<string, object> Variables { get; set; } = new();
// }