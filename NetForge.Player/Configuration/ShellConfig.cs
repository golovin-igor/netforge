namespace NetForge.Player.Configuration;

/// <summary>
/// Configuration for the interactive shell
/// </summary>
public class ShellConfig
{
    /// <summary>
    /// Command prompt text
    /// </summary>
    public string Prompt { get; set; } = "NetForge> ";
    
    /// <summary>
    /// Prompt color
    /// </summary>
    public string PromptColor { get; set; } = "Green";
    
    /// <summary>
    /// Maximum command history size
    /// </summary>
    public int MaxHistorySize { get; set; } = 100;
    
    /// <summary>
    /// Enable tab completion
    /// </summary>
    public bool EnableTabCompletion { get; set; } = true;
    
    /// <summary>
    /// Enable command history
    /// </summary>
    public bool EnableHistory { get; set; } = true;
    
    /// <summary>
    /// Auto-save command history
    /// </summary>
    public bool AutoSaveHistory { get; set; } = true;
    
    /// <summary>
    /// History file path
    /// </summary>
    public string? HistoryFilePath { get; set; }
}