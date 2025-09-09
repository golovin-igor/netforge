using System.Globalization;
using System.Text;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Protocols;

namespace NetForge.Simulation.Topology.Services;

/// <summary>
/// Manages network connectivity operations including routing, ARP, and connectivity testing.
/// This service extracts networking responsibilities from NetworkDevice.
/// </summary>
public class DeviceConnectivityService : INetworkConnectivity
{
    private readonly List<Route> _routingTable = [];
    private readonly Dictionary<string, string> _arpTable = new();
    private readonly IInterfaceManager _interfaceManager;
    private readonly IDeviceIdentity _deviceIdentity;

    public DeviceConnectivityService(IInterfaceManager interfaceManager, IDeviceIdentity deviceIdentity)
    {
        _interfaceManager = interfaceManager ?? throw new ArgumentNullException(nameof(interfaceManager));
        _deviceIdentity = deviceIdentity ?? throw new ArgumentNullException(nameof(deviceIdentity));
    }

    /// <summary>
    /// Gets the routing table for the device.
    /// </summary>
    public List<Route> GetRoutingTable() => _routingTable;

    /// <summary>
    /// Adds a route to the routing table.
    /// </summary>
    public void AddRoute(Route route)
    {
        if (route != null && !_routingTable.Any(r => 
            r.Network == route.Network && r.Mask == route.Mask && r.NextHop == route.NextHop))
        {
            _routingTable.Add(route);
        }
    }

    /// <summary>
    /// Removes a route from the routing table.
    /// </summary>
    public void RemoveRoute(Route route)
    {
        if (route != null)
        {
            _routingTable.RemoveAll(r => 
                r.Network == route.Network && r.Mask == route.Mask && r.NextHop == route.NextHop);
        }
    }

    /// <summary>
    /// Clears all routes of a specific protocol from the routing table.
    /// </summary>
    public void ClearRoutesByProtocol(string protocol)
    {
        _routingTable.RemoveAll(r => r.Protocol.Equals(protocol, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Adds a static route to the routing table.
    /// </summary>
    public void AddStaticRoute(string network, string mask, string nextHop, int metric)
    {
        var route = new Route(network, mask, nextHop, "", "Static")
        {
            Metric = metric,
            AdminDistance = 1
        };
        AddRoute(route);
    }

    /// <summary>
    /// Removes a static route from the routing table.
    /// </summary>
    public void RemoveStaticRoute(string network, string mask)
    {
        _routingTable.RemoveAll(r => 
            r.Network == network && r.Mask == mask && r.Protocol == "Static");
    }

    /// <summary>
    /// Forces an update of connected routes based on interface configurations.
    /// </summary>
    public void ForceUpdateConnectedRoutes()
    {
        // Remove all connected routes
        ClearRoutesByProtocol("Connected");

        // Add routes for all configured interfaces
        foreach (var iface in _interfaceManager.GetAllInterfaces().Values)
        {
            if (!string.IsNullOrEmpty(iface.IpAddress) && !string.IsNullOrEmpty(iface.SubnetMask))
            {
                var network = GetNetworkAddress(iface.IpAddress, iface.SubnetMask);
                var route = new Route(network, iface.SubnetMask, "0.0.0.0", iface.Name, "Connected")
                {
                    Metric = 0,
                    AdminDistance = 0
                };
                AddRoute(route);
            }
        }
    }

    /// <summary>
    /// Gets the ARP table as a dictionary of IP to MAC address mappings.
    /// </summary>
    public Dictionary<string, string> GetArpTable() => _arpTable;

    /// <summary>
    /// Gets the ARP table formatted as a string for display.
    /// </summary>
    public string GetArpTableOutput()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Protocol  Address          Age (min)  Hardware Addr    Type   Interface");
        
        foreach (var entry in _arpTable)
        {
            sb.AppendLine($"Internet  {entry.Key,-15}  -          {entry.Value,-15}  ARPA   ");
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Executes a ping command to test connectivity to a destination.
    /// </summary>
    public string ExecutePing(string destination)
    {
        // Simple ping simulation
        var sb = new StringBuilder();
        sb.AppendLine($"Type escape sequence to abort.");
        sb.AppendLine($"Sending 5, 100-byte ICMP Echos to {destination}, timeout is 2 seconds:");
        
        // Check if destination is reachable (simplified logic)
        bool isReachable = IsDestinationReachable(destination);
        
        if (isReachable)
        {
            sb.AppendLine("!!!!!");
            sb.AppendLine("Success rate is 100 percent (5/5), round-trip min/avg/max = 1/2/4 ms");
        }
        else
        {
            sb.AppendLine(".....");
            sb.AppendLine("Success rate is 0 percent (0/5)");
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Calculates the network address from an IP address and subnet mask.
    /// </summary>
    public string GetNetworkAddress(string ip, string mask)
    {
        var ipBytes = ip.Split('.').Select(byte.Parse).ToArray();
        var maskBytes = mask.Split('.').Select(byte.Parse).ToArray();
        var networkBytes = new byte[4];
        
        for (int i = 0; i < 4; i++)
        {
            networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
        }
        
        return string.Join(".", networkBytes);
    }

    /// <summary>
    /// Checks if an IP address is within a specific network.
    /// </summary>
    public bool CheckIpInNetwork(string ip, string network, string mask)
    {
        var ipNet = GetNetworkAddress(ip, mask);
        var targetNet = GetNetworkAddress(network, mask);
        return ipNet == targetNet;
    }

    /// <summary>
    /// Converts a subnet mask to CIDR notation.
    /// </summary>
    public int MaskToCidr(string mask)
    {
        var octets = mask.Split('.');
        int cidr = 0;
        
        foreach (var octet in octets)
        {
            var value = int.Parse(octet, CultureInfo.InvariantCulture);
            while (value > 0)
            {
                cidr += value & 1;
                value >>= 1;
            }
        }
        
        return cidr;
    }

    /// <summary>
    /// Updates an entry in the ARP table.
    /// </summary>
    public void UpdateArpTable(string ipAddress, string macAddress)
    {
        _arpTable[ipAddress] = macAddress;
    }

    /// <summary>
    /// Helper method to check if a destination is reachable.
    /// </summary>
    private bool IsDestinationReachable(string destination)
    {
        // Check if there's a route to the destination
        foreach (var route in _routingTable)
        {
            if (CheckIpInNetwork(destination, route.Network, route.Mask))
            {
                return true;
            }
        }
        
        // Check if it's a directly connected network
        foreach (var iface in _interfaceManager.GetAllInterfaces().Values)
        {
            if (!string.IsNullOrEmpty(iface.IpAddress) && !string.IsNullOrEmpty(iface.SubnetMask))
            {
                if (CheckIpInNetwork(destination, iface.IpAddress, iface.SubnetMask))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}