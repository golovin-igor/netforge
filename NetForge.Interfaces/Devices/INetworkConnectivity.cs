using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Protocols;

namespace NetForge.Interfaces.Devices;

/// <summary>
/// Defines network connectivity operations including routing, ARP, and connectivity testing.
/// This interface handles all network-layer connectivity for a device.
/// </summary>
public interface INetworkConnectivity
{
    /// <summary>
    /// Gets the routing table for the device.
    /// </summary>
    List<Route> GetRoutingTable();

    /// <summary>
    /// Adds a route to the routing table.
    /// </summary>
    /// <param name="route">The route to add.</param>
    void AddRoute(Route route);

    /// <summary>
    /// Removes a route from the routing table.
    /// </summary>
    /// <param name="route">The route to remove.</param>
    void RemoveRoute(Route route);

    /// <summary>
    /// Clears all routes of a specific protocol from the routing table.
    /// </summary>
    /// <param name="protocol">The protocol whose routes should be cleared.</param>
    void ClearRoutesByProtocol(string protocol);

    /// <summary>
    /// Adds a static route to the routing table.
    /// </summary>
    /// <param name="network">The network address.</param>
    /// <param name="mask">The subnet mask.</param>
    /// <param name="nextHop">The next hop IP address.</param>
    /// <param name="metric">The route metric.</param>
    void AddStaticRoute(string network, string mask, string nextHop, int metric);

    /// <summary>
    /// Removes a static route from the routing table.
    /// </summary>
    /// <param name="network">The network address.</param>
    /// <param name="mask">The subnet mask.</param>
    void RemoveStaticRoute(string network, string mask);

    /// <summary>
    /// Forces an update of connected routes based on interface configurations.
    /// </summary>
    void ForceUpdateConnectedRoutes();

    /// <summary>
    /// Gets the ARP table as a dictionary of IP to MAC address mappings.
    /// </summary>
    Dictionary<string, string> GetArpTable();

    /// <summary>
    /// Gets the ARP table formatted as a string for display.
    /// </summary>
    string GetArpTableOutput();

    /// <summary>
    /// Executes a ping command to test connectivity to a destination.
    /// </summary>
    /// <param name="destination">The destination IP address or hostname.</param>
    /// <returns>The ping result as a formatted string.</returns>
    string ExecutePing(string destination);

    /// <summary>
    /// Calculates the network address from an IP address and subnet mask.
    /// </summary>
    /// <param name="ip">The IP address.</param>
    /// <param name="mask">The subnet mask.</param>
    /// <returns>The network address.</returns>
    string GetNetworkAddress(string ip, string mask);

    /// <summary>
    /// Checks if an IP address is within a specific network.
    /// </summary>
    /// <param name="ip">The IP address to check.</param>
    /// <param name="network">The network address.</param>
    /// <param name="mask">The subnet mask.</param>
    /// <returns>True if the IP is in the network, false otherwise.</returns>
    bool CheckIpInNetwork(string ip, string network, string mask);

    /// <summary>
    /// Converts a subnet mask to CIDR notation.
    /// </summary>
    /// <param name="mask">The subnet mask.</param>
    /// <returns>The CIDR prefix length.</returns>
    int MaskToCidr(string mask);
}