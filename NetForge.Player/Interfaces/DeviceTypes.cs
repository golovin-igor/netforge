namespace NetForge.Player.Interfaces;

/// <summary>
/// Device type enumeration for NetForge.Player
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Router device
    /// </summary>
    Router,
    
    /// <summary>
    /// Switch device
    /// </summary>
    Switch,
    
    /// <summary>
    /// Firewall device
    /// </summary>
    Firewall,
    
    /// <summary>
    /// Server device
    /// </summary>
    Server,
    
    /// <summary>
    /// Load balancer device
    /// </summary>
    LoadBalancer,
    
    /// <summary>
    /// Wireless access point
    /// </summary>
    AccessPoint
}

/// <summary>
/// Network interface type enumeration
/// </summary>
public enum InterfaceType
{
    /// <summary>
    /// Ethernet interface
    /// </summary>
    Ethernet,
    
    /// <summary>
    /// Serial interface
    /// </summary>
    Serial,
    
    /// <summary>
    /// Loopback interface
    /// </summary>
    Loopback,
    
    /// <summary>
    /// Tunnel interface
    /// </summary>
    Tunnel,
    
    /// <summary>
    /// VLAN interface
    /// </summary>
    Vlan,
    
    /// <summary>
    /// Wireless interface
    /// </summary>
    Wireless
}