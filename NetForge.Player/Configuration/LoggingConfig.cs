// TODO: Phase 1.3 - Implement Configuration Management
// This class manages logging configuration settings

using System;
using System.Collections.Generic;

namespace NetForge.Player.Configuration;

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingConfig
{
    // TODO: Implement comprehensive logging configuration
    // - Multiple log levels and categories
    // - File rotation and archival
    // - Structured logging formats
    // - Performance monitoring
    // - External logging system integration
    
    public string Level { get; set; } = "Information";
    public string FilePath { get; set; } = "netsim.log";
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
    public int MaxFileSizeMB { get; set; } = 100;
    public int MaxLogFiles { get; set; } = 10;
    
    // TODO: Add logging categories and formats
    // public Dictionary<string, string> CategoryLevels { get; set; } = new();
    // public string LogFormat { get; set; } = "json"; // json, text, structured
    // public bool EnableMetrics { get; set; } = false;
    // public string MetricsEndpoint { get; set; } = string.Empty;
}