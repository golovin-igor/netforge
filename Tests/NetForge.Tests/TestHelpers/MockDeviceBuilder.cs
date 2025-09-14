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

    public MockDeviceBuilder WithInterface(string name, string ipAddress, bool isUp = true, bool isShutdown = false)
    {
        _interfaces[name] = new MockInterface
        {
            Name = name,
            IpAddress = ipAddress,
            IsUp = isUp,
            IsShutdown = isShutdown,
            TxPackets = 0,
            RxPackets = 0,
            TxBytes = 0,
            RxBytes = 0
        };
        return this;
    }

    public MockNetworkDevice Build()
    {
        var device = new MockNetworkDevice(_name, _vendor);

        // Add interfaces
        foreach (var kvp in _interfaces)
        {
            device.SetupInterface(kvp.Key, kvp.Value.IpAddress, kvp.Value.IsUp, kvp.Value.IsShutdown);
        }

        // Add default interface if none provided
        if (!_interfaces.Any())
        {
            device.SetupInterface("eth0", "192.168.1.10", true);
        }

        return device;
    }
}

/// <summary>
/// Mock network device implementation for testing
/// </summary>
public class MockNetworkDevice : INetworkDevice, IInterfaceManager, IDeviceLogging, INetworkConnectivity, IConfigurationProvider, INetworkContext
{
    private readonly Dictionary<string, MockInterface> _interfaces = new();
    private readonly List<string> _logEntries = new();

    public string Name { get; }
    public string Vendor { get; }

    public MockNetworkDevice(string name, string vendor)
    {
        Name = name;
        Vendor = vendor;
    }

    public void SetupInterface(string name, string ipAddress, bool isUp = true, bool isShutdown = false)
    {
        _interfaces[name] = new MockInterface
        {
            Name = name,
            IpAddress = ipAddress,
            IsUp = isUp,
            IsShutdown = isShutdown,
            TxPackets = 0,
            RxPackets = 0,
            TxBytes = 0,
            RxBytes = 0
        };
    }

    public MockInterface GetInterface(string name) => _interfaces.TryGetValue(name, out var iface) ? iface : null!;

    // IInterfaceManager implementation
    public Dictionary<string, dynamic> GetAllInterfaces()
    {
        return _interfaces.ToDictionary(kvp => kvp.Key, kvp => (dynamic)kvp.Value);
    }

    // IDeviceLogging implementation
    public void AddLogEntry(string message)
    {
        _logEntries.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} {message}");
    }

    public List<string> GetLogEntries() => new(_logEntries);

    // INetworkConnectivity implementation (minimal for testing)
    public bool IsConnected => true;

    // IConfigurationProvider implementation (minimal for testing)
    public string GetConfiguration() => "! Mock configuration";

    // INetworkContext implementation (minimal for testing)
    public INetworkDevice Device => this;
}

/// <summary>
/// Mock interface implementation for testing
/// </summary>
public class MockInterface
{
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsUp { get; set; }
    public bool IsShutdown { get; set; }
    public long TxPackets { get; set; }
    public long RxPackets { get; set; }
    public long TxBytes { get; set; }
    public long RxBytes { get; set; }
    public string Description { get; set; } = string.Empty;
}