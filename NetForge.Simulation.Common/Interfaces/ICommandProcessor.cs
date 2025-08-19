namespace NetForge.Simulation.Core
{
    /// <summary>
    /// Interface for command processing
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        /// Asynchronously process a command and return the output
        /// </summary>
        Task<string> ProcessCommandAsync(string command);
        
        /// <summary>
        /// Get the current prompt
        /// </summary>
        string GetCurrentPrompt();
    }
} 
