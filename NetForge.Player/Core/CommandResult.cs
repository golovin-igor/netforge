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
    public bool ShouldExit { get; set; }

    public bool IsError => !Success;
    public bool IsWarning { get; set; }

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static CommandResult Ok(string message = "")
    {
        return new CommandResult
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Create an error result
    /// </summary>
    public static CommandResult Fail(string message)
    {
        return new CommandResult
        {
            Success = false,
            Message = message,
            ErrorMessage = message
        };
    }

    /// <summary>
    /// Create an empty result
    /// </summary>
    public static CommandResult Empty()
    {
        return new CommandResult
        {
            Success = true,
            Message = ""
        };
    }

    /// <summary>
    /// Create an exit result
    /// </summary>
    public static CommandResult Exit(string message = "")
    {
        return new CommandResult
        {
            Success = true,
            Message = message,
            ShouldExit = true
        };
    }

    // TODO: Add additional result properties
    // public object? ReturnValue { get; set; }
    // public Dictionary<string, object> Context { get; set; } = new();
    // public List<string> Warnings { get; set; } = new();
    // public CommandResultType ResultType { get; set; }
}