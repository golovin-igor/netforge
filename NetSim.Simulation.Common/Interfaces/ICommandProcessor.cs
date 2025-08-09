namespace NetSim.Simulation.Core
{
    /// <summary>
    /// Interface for command processing
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        /// Process a command and return the output
        /// </summary>
        string ProcessCommand(string command);

        /// <summary>
        /// Asynchronously process a command and return the output
        /// </summary>
        async Task<string> ProcessCommandAsync(string command)
            => await Task.FromResult(ProcessCommand(command));
        
        /// <summary>
        /// Get the current prompt
        /// </summary>
        string GetCurrentPrompt();
    }
} 
