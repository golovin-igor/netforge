namespace NetForge.Interfaces.CLI
{
    /// <summary>
    /// Interface for vendor-specific command formatting and validation
    /// </summary>
    public interface ICommandFormatter
    {
        /// <summary>
        /// Execute vendor-specific command formatting
        /// </summary>
        string FormatCommandOutput(string command, object? data = null);

        /// <summary>
        /// Get vendor-specific error message
        /// </summary>
        string GetVendorErrorMessage(string errorType, string? context = null);

        /// <summary>
        /// Validate vendor-specific syntax
        /// </summary>
        bool ValidateVendorSyntax(string[] commandParts, string command);

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
    }
}