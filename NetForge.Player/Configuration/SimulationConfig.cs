// TODO: Phase 1.3 - Implement Configuration Management
// This class manages simulation-related configuration settings

namespace NetForge.Player.Configuration;

/// <summary>
/// Simulation engine configuration
/// </summary>
public class SimulationConfig
{
    // TODO: Implement simulation-specific configuration options
    // - Device limits and resource constraints
    // - Protocol timing and convergence settings
    // - Auto-save and persistence options
    // - Performance tuning parameters
    // - Debug and development options
    
    public bool AutoSave { get; set; } = true;
    public int SaveIntervalSeconds { get; set; } = 300;
    public int MaxDevices { get; set; } = 100;
    public bool EnableProtocolLogging { get; set; } = false;
    
    // TODO: Add simulation parameters
    // public int ProtocolUpdateIntervalMs { get; set; } = 1000;
    // public bool EnableMetrics { get; set; } = true;
    // public string DefaultScenarioPath { get; set; } = "scenarios";
    // public Dictionary<string, object> VendorSettings { get; set; } = new();
}