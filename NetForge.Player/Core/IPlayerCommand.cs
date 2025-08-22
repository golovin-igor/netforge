// TODO: Phase 1.1 - Implement Command Line Interface System
// This interface defines the base structure for all Player commands

using System.Threading.Tasks;

namespace NetForge.Player.Core;

/// <summary>
/// Base interface for all NetForge.Player commands
/// </summary>
public interface IPlayerCommand
{
    // TODO: Enhance IPlayerCommand with comprehensive command properties
    // - Command categories and grouping
    // - Permission and access control requirements
    // - Command validation and prerequisites
    // - Async cancellation support
    // - Command chaining and pipeline support
    // - Context-aware execution
    
    /// <summary>
    /// Command name (used for invocation)
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Short description of the command
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Usage syntax and examples
    /// </summary>
    string Usage { get; }
    
    /// <summary>
    /// Execute the command asynchronously
    /// </summary>
    /// <param name="context">Command execution context</param>
    /// <returns>Command execution result</returns>
    Task<CommandResult> ExecuteAsync(CommandContext context);
    
    // TODO: Add command validation and help methods
    // bool CanExecute(CommandContext context);
    // Task<bool> ValidateArgumentsAsync(string[] args);
    // CommandMetadata GetMetadata();
    // List<string> GetCompletions(string partialInput);
}

