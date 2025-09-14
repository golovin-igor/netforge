namespace NetForge.Player.Interfaces;

/// <summary>
/// Stub interface for network devices (temporary until NetForge interfaces are available)
/// </summary>
public interface INetworkDevice
{
    /// <summary>
    /// Device name
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Device type
    /// </summary>
    DeviceType DeviceType { get; }
    
    /// <summary>
    /// Vendor name
    /// </summary>
    string Vendor { get; }
    
    /// <summary>
    /// Device model
    /// </summary>
    string Model { get; }
    
    /// <summary>
    /// Whether device is running
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Get device interfaces
    /// </summary>
    IEnumerable<INetworkInterface>? GetInterfaces();
    
    /// <summary>
    /// Add interface to device
    /// </summary>
    Task AddInterfaceAsync(string name, InterfaceType type);
}

/// <summary>
/// Stub interface for network interfaces (temporary until NetForge interfaces are available)
/// </summary>
public interface INetworkInterface
{
    /// <summary>
    /// Interface name
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Interface type
    /// </summary>
    InterfaceType InterfaceType { get; }
    
    /// <summary>
    /// Whether interface is enabled
    /// </summary>
    bool IsEnabled { get; }
    
    /// <summary>
    /// IP address assigned to interface
    /// </summary>
    string? IpAddress { get; }
    
    /// <summary>
    /// Interface description
    /// </summary>
    string? Description { get; }
}