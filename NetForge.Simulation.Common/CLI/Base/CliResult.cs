namespace NetForge.Simulation.CliHandlers
{
    /// <summary>
    /// Represents the result of a CLI command execution
    /// </summary>
    public sealed class CliResult
    {
        public bool Success { get; init; }
        public string Output { get; init; } = string.Empty;
        public CliErrorType? Error { get; init; }
        public string[]? Suggestions { get; init; }
        
        private CliResult() { }
        
        public static CliResult Ok(string output = "") => new() 
        { 
            Success = true, 
            Output = output ?? string.Empty
        };
        
        public static CliResult Failed(CliErrorType errorType, string output = "", string[]? suggestions = null) => new() 
        {
            Success = false,
            Error = errorType,
            Output = output ?? string.Empty,
            Suggestions = suggestions
        };
    }
    
    /// <summary>
    /// Defines standard error types for CLI command execution
    /// </summary>
    public enum CliErrorType
    {
        InvalidCommand,
        IncompleteCommand,
        InvalidParameter,
        InvalidMode,
        PermissionDenied,
        ExecutionError,
        NotImplemented
    }
} 
