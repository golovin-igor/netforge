// TODO: Phase 1.1 - Implement Command Line Interface System
// This class provides metadata and help information for commands

namespace NetForge.Player.Core;

/// <summary>
/// Command metadata for help and documentation
/// </summary>
public class CommandMetadata
{
    // TODO: Expand CommandMetadata with comprehensive command information
    // - Command aliases and shortcuts
    // - Parameter descriptions and types
    // - Usage examples and patterns
    // - Related commands and see-also references
    // - Command categories and tags
    // - Version and availability information
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Usage { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new();
    
    // TODO: Add parameter metadata
    // public List<CommandParameter> Parameters { get; set; } = new();
    // public List<string> Aliases { get; set; } = new();
    // public string Category { get; set; } = string.Empty;
    // public List<string> SeeAlso { get; set; } = new();
}