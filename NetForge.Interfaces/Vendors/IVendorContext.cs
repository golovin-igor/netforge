using NetForge.Interfaces.CLI;

namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Provides vendor-specific context and capabilities to CLI handlers
    /// </summary>
    public interface IVendorContext
    {
        /// <summary>
        /// The vendor name (e.g., "Cisco", "Juniper", "Arista")
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// Device capabilities interface
        /// </summary>
        IVendorCapabilities Capabilities { get; }

        /// <summary>
        /// Candidate configuration interface (null if not supported)
        /// </summary>
        ICandidateConfiguration? CandidateConfig { get; }

        /// <summary>
        /// Check if the device is in a specific mode
        /// </summary>
        bool IsInMode(string mode);

        /// <summary>
        /// Get the prompt format for current mode
        /// </summary>
        string GetModePrompt();

        /// <summary>
        /// Get vendor-specific help text for a command
        /// </summary>
        string GetCommandHelp(string command);

        /// <summary>
        /// Get vendor-specific command completions
        /// </summary>
        IEnumerable<string> GetCommandCompletions(string[] commandParts);

        /// <summary>
        /// Execute vendor-specific preprocessing on command
        /// </summary>
        string PreprocessCommand(string command);

        /// <summary>
        /// Execute vendor-specific postprocessing on output
        /// </summary>
        string PostprocessOutput(string output);

        /// <summary>
        /// Get vendor-specific configuration renderer
        /// </summary>
        string RenderConfiguration(object configData);

        /// <summary>
        /// Get the currently selected interface for configuration
        /// </summary>
        string GetCurrentInterface();
    }
}
