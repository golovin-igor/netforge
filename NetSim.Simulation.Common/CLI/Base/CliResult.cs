namespace NetSim.Simulation.CliHandlers
{
    /// <summary>
    /// Represents the result of a CLI command execution
    /// </summary>
    public class CliResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public CliErrorType? Error { get; set; }
        public string[]? Suggestions { get; set; }
        
        public static CliResult Ok(string output = "") => new CliResult { 
            Success = true, 
            Output = output 
        };
        
        public static CliResult Failed(CliErrorType errorType, string output = "", string[]? suggestions = null) => new CliResult {
            Success = false,
            Error = errorType,
            Output = output,
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
