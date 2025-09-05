// TODO: Phase 1.1 - Implement Command Line Interface System
// This class represents the result of command execution

namespace NetForge.Player.Core;

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