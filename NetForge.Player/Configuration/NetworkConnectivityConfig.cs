// TODO: Phase 1.3 - Implement Configuration Management
// This class manages external network connectivity configuration settings

namespace NetForge.Player.Configuration;

/// <summary>
/// External network connectivity configuration
/// </summary>
public class NetworkConnectivityConfig
{
    // TODO: Implement network bridge configuration
    // - Virtual interface settings
    // - IP address management
    // - Traffic routing and filtering
    // - Performance and security options
    // - Cross-platform compatibility settings
    
    public bool Enabled { get; set; } = false;
    public string Mode { get; set; } = "virtual_interfaces"; // virtual_interfaces, host_binding, nat_proxy
    public string BridgeNetwork { get; set; } = "192.168.100.0/24";
    public string Gateway { get; set; } = "192.168.100.1";
    public string InterfacePrefix { get; set; } = "netsim";
    public bool AutoCreateInterfaces { get; set; } = true;
    public bool RequireAdmin { get; set; } = true;
    
    // TODO: Add advanced networking options
    // public List<string> AllowedNetworks { get; set; } = new();
    // public Dictionary<string, int> PortMapping { get; set; } = new();
    // public bool EnableTrafficShaping { get; set; } = false;
    // public string DnsServers { get; set; } = "8.8.8.8,8.8.4.4";
}