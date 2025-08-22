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

