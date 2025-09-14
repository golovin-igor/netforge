using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;

namespace NetForge.Tests.TestHelpers;

/// <summary>
/// Builder for creating mock network devices for testing
/// </summary>
public class MockDeviceBuilder
{
    private string _name = "TestDevice";
    private string _vendor = "Generic";
    private readonly Dictionary<string, MockInterface> _interfaces = new();
    private readonly List<string> _logEntries = new();

    public static MockDeviceBuilder Create() => new();

    public MockDeviceBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public MockDeviceBuilder WithVendor(string vendor)
    {
        _vendor = vendor;
        return this;
    }

    public MockDeviceBuilder WithInterface(string name, string ipAddress, bool isUp = true, bool isShutdown = false, string subnetMask = "255.255.255.0")
    {
        _interfaces[name] = new MockInterface
        {
            Name = name,
            IpAddress = ipAddress,
            SubnetMask = subnetMask,
            IsUp = isUp,
            IsShutdown = isShutdown,
            TxPackets = 0,
            RxPackets = 0,
            TxBytes = 0,
            RxBytes = 0
        };
        return this;
    }

    public MockDeviceBuilder WithEthernetInterface(string name, string ipAddress, string macAddress = "")
    {
        var iface = new MockInterface
        {
            Name = name,
            IpAddress = ipAddress,
            InterfaceType = "Ethernet",
            IsUp = true,
            IsShutdown = false
        };

        if (!string.IsNullOrEmpty(macAddress))
        {
            iface.MacAddress = macAddress;
        }

        _interfaces[name] = iface;
        return this;
    }

    public MockDeviceBuilder WithLoopbackInterface(string name = "lo0", string ipAddress = "127.0.0.1")
    {
        _interfaces[name] = new MockInterface
        {
            Name = name,
            IpAddress = ipAddress,
            SubnetMask = "255.0.0.0",
            InterfaceType = "Loopback",
            IsUp = true,
            IsShutdown = false,
            MacAddress = "00:00:00:00:00:00"
        };
        return this;
    }

    public MockNetworkDevice Build()
    {
        var device = new MockNetworkDevice(_name, _vendor);

        // Add interfaces (preserving all their properties)
        foreach (var kvp in _interfaces)
        {
            device.SetupInterface(kvp.Key, kvp.Value.IpAddress, kvp.Value.IsUp, kvp.Value.IsShutdown, kvp.Value.SubnetMask);

            // Copy additional properties
            var deviceInterface = device.GetInterface(kvp.Key);
            deviceInterface.InterfaceType = kvp.Value.InterfaceType;
            deviceInterface.MacAddress = kvp.Value.MacAddress;
            deviceInterface.Mtu = kvp.Value.Mtu;
            deviceInterface.Description = kvp.Value.Description;
        }

        // Add default interface if none provided
        if (_interfaces.Count == 0)
        {
            device.SetupInterface("eth0", "192.168.1.10", true);
        }

        return device;
    }
}

/// <summary>
/// Mock network device implementation for testing that implements essential INetworkDevice interfaces
/// </summary>
public class MockNetworkDevice : IDeviceIdentity, ISimulatedNetworking
{
    private readonly Dictionary<string, MockInterface> _interfaces = new();
    private readonly List<string> _logEntries = new();
    private string _hostname;
    private readonly Dictionary<string, string> _systemSettings = new();

    public string Name { get; }
    public string Vendor { get; }
    public string DeviceId { get; set; }
    public string DeviceName => Name;
    public string DeviceType { get; set; } = "Router";

    public MockNetworkDevice(string name, string vendor)
    {
        Name = name;
        Vendor = vendor;
        DeviceId = $"{vendor}-{name}-{Guid.NewGuid():N}";
        _hostname = name;
    }

    public void SetupInterface(string name, string ipAddress, bool isUp = true, bool isShutdown = false, string subnetMask = "255.255.255.0")
    {
        _interfaces[name] = new MockInterface
        {
            Name = name,
            IpAddress = ipAddress,
            SubnetMask = subnetMask,
            IsUp = isUp,
            IsShutdown = isShutdown,
            TxPackets = 0,
            RxPackets = 0,
            TxBytes = 0,
            RxBytes = 0
        };
    }

    public MockInterface GetInterface(string name) => _interfaces.TryGetValue(name, out var iface) ? iface : null!;

    public IEnumerable<MockInterface> GetAllInterfaces() => _interfaces.Values;

    // IDeviceIdentity implementation
    public string GetHostname() => _hostname;

    public void SetHostname(string hostname)
    {
        _hostname = hostname;
    }

    // Additional testing methods
    public void SetSystemSetting(string name, string value)
    {
        _systemSettings[name] = value;
    }

    public string? GetSystemSetting(string name)
    {
        return _systemSettings.TryGetValue(name, out var value) ? value : null;
    }

    public void AddLogEntry(string entry)
    {
        _logEntries.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {entry}");
    }

    public IReadOnlyList<string> GetLogEntries() => _logEntries.AsReadOnly();

    // ISimulatedNetworking implementation (simplified for testing)
    public bool CanReach(string destination)
    {
        // Simple simulation: can reach if it looks like a valid IP or hostname
        if (string.IsNullOrWhiteSpace(destination))
            return false;

        // Simulate some unreachable destinations for testing
        var unreachableDestinations = new[] { "192.168.999.1", "unreachable.example.com", "10.0.0.0" };
        return !unreachableDestinations.Contains(destination.ToLowerInvariant());
    }

    public TimeSpan CalculateLatency(string destination)
    {
        // Simulate realistic network latencies for testing
        if (!CanReach(destination))
            return TimeSpan.FromMilliseconds(5000); // Timeout

        // Simple latency simulation based on destination patterns
        if (destination.StartsWith("127.") || destination.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return TimeSpan.FromMilliseconds(1); // Localhost

        if (destination.StartsWith("192.168.") || destination.StartsWith("10.0."))
            return TimeSpan.FromMilliseconds(Random.Shared.Next(1, 5)); // Local network

        // Remote destination
        return TimeSpan.FromMilliseconds(Random.Shared.Next(20, 100));
    }

    public void UpdateInterfaceCounters(string interfaceName, long txBytes, long rxBytes)
    {
        if (_interfaces.TryGetValue(interfaceName, out var iface))
        {
            iface.TxBytes += txBytes;
            iface.RxBytes += rxBytes;
        }
    }

    public MockInterface? GetActiveInterface()
    {
        return _interfaces.Values.FirstOrDefault(i => i.IsUp && !i.IsShutdown);
    }

    public bool HasActiveInterface()
    {
        return GetActiveInterface() != null;
    }
}

/// <summary>
/// Interface for simulated networking capabilities in mock devices
/// </summary>
public interface ISimulatedNetworking
{
    /// <summary>
    /// Determines if the device can reach a given destination
    /// </summary>
    bool CanReach(string destination);

    /// <summary>
    /// Calculates simulated network latency to a destination
    /// </summary>
    TimeSpan CalculateLatency(string destination);

    /// <summary>
    /// Updates interface traffic counters
    /// </summary>
    void UpdateInterfaceCounters(string interfaceName, long txBytes, long rxBytes);

    /// <summary>
    /// Gets the first active interface for network operations
    /// </summary>
    MockInterface? GetActiveInterface();

    /// <summary>
    /// Checks if the device has any active interfaces
    /// </summary>
    bool HasActiveInterface();
}

/// <summary>
/// Mock interface implementation for testing
/// </summary>
public class MockInterface
{
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string SubnetMask { get; set; } = "255.255.255.0";
    public bool IsUp { get; set; }
    public bool IsShutdown { get; set; }
    public long TxPackets { get; set; }
    public long RxPackets { get; set; }
    public long TxBytes { get; set; }
    public long RxBytes { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public int Mtu { get; set; } = 1500;
    public string InterfaceType { get; set; } = "Ethernet";
    public DateTime LastActivity { get; set; } = DateTime.Now;

    public MockInterface()
    {
        // Generate a random MAC address for testing
        var random = new Random();
        var macBytes = new byte[6];
        random.NextBytes(macBytes);
        macBytes[0] = (byte)(macBytes[0] | 0x02); // Set locally administered bit
        macBytes[0] = (byte)(macBytes[0] & 0xFE); // Clear multicast bit
        MacAddress = string.Join(":", macBytes.Select(b => b.ToString("X2")));
    }

    /// <summary>
    /// Simulates network activity on this interface
    /// </summary>
    public void SimulateActivity(int packetCount = 1, int avgPacketSize = 64)
    {
        TxPackets += packetCount;
        TxBytes += packetCount * avgPacketSize;
        LastActivity = DateTime.Now;
    }

    /// <summary>
    /// Checks if this interface is operational (up and not shutdown)
    /// </summary>
    public bool IsOperational => IsUp && !IsShutdown;

    /// <summary>
    /// Gets a summary of interface statistics
    /// </summary>
    public string GetStatisticsSummary()
    {
        return $"TX: {TxPackets} packets ({TxBytes} bytes), RX: {RxPackets} packets ({RxBytes} bytes)";
    }
}