using NetForge.Simulation.Events;
using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Devices;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.Simulation.Core;

public class Topology : ITopology
{
    private readonly Dictionary<string, INetworkDevice> _devices = new();
    private readonly Dictionary<string, IPhysicalConnection> _connections = new();

    public string Id { get; }
    public string Name { get; }
    public IReadOnlyCollection<INetworkDevice> Devices => _devices.Values.ToList().AsReadOnly();
    public IReadOnlyCollection<IPhysicalConnection> Connections => _connections.Values.ToList().AsReadOnly();
    public IEventBus GlobalEventBus { get; }

    public Topology(string id, string name)
    {
        Id = id;
        Name = name;
        GlobalEventBus = new EventBus();
    }

    public void AddDevice(INetworkDevice device)
    {
        _devices[device.Id] = device;

        // Subscribe device to global events
        device.LocalEventBus.Subscribe<INetworkEvent>("global-relay", evt =>
            GlobalEventBus.Publish(evt));
    }

    public void RemoveDevice(string deviceId)
    {
        if (_devices.TryGetValue(deviceId, out var device))
        {
            // Remove all connections involving this device
            var connectionsToRemove = _connections.Values
                .Where(c => c.Endpoint1.Id.StartsWith(deviceId) || c.Endpoint2.Id.StartsWith(deviceId))
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                DisconnectDevices(connection.Id);
            }

            device.PowerOff();
            _devices.Remove(deviceId);
        }
    }

    public INetworkDevice GetDevice(string deviceId)
    {
        return _devices.TryGetValue(deviceId, out var device) ? device :
            throw new KeyNotFoundException($"Device with ID '{deviceId}' not found");
    }

    public void ConnectDevices(string deviceId1, string interfaceId1, string deviceId2, string interfaceId2)
    {
        var device1 = GetDevice(deviceId1);
        var device2 = GetDevice(deviceId2);
        var interface1 = device1.GetInterface(interfaceId1);
        var interface2 = device2.GetInterface(interfaceId2);

        var connectionId = $"{deviceId1}:{interfaceId1}-{deviceId2}:{interfaceId2}";
        var connection = new PhysicalConnection(connectionId, interface1, interface2);

        _connections[connectionId] = connection;
        connection.Connect();
    }

    public void DisconnectDevices(string connectionId)
    {
        if (_connections.TryGetValue(connectionId, out var connection))
        {
            connection.Disconnect();
            _connections.Remove(connectionId);
        }
    }

    public IEnumerable<INetworkDevice> FindDevicesByCapability(DeviceCapabilityType capability)
    {
        return _devices.Values.Where(device =>
            device.Capabilities.Any(cap => cap.Type == capability));
    }

    public IEnumerable<IPhysicalConnection> GetDeviceConnections(string deviceId)
    {
        return _connections.Values.Where(connection =>
            connection.Endpoint1.Id.StartsWith(deviceId) ||
            connection.Endpoint2.Id.StartsWith(deviceId));
    }

    public void Start()
    {
        foreach (var device in _devices.Values)
        {
            if (device.State == DeviceState.PoweredOff)
                device.PowerOn();
        }

        foreach (var connection in _connections.Values)
        {
            if (connection.State == ConnectionState.Disconnected)
                connection.Connect();
        }
    }

    public void Stop()
    {
        foreach (var device in _devices.Values)
        {
            if (device.State == DeviceState.Running)
                device.PowerOff();
        }

        foreach (var connection in _connections.Values)
        {
            connection.Disconnect();
        }
    }

    public void Reset()
    {
        Stop();

        foreach (var device in _devices.Values)
        {
            // Reset device configurations and state
        }
    }

    public void BroadcastGlobalEvent(INetworkEvent networkEvent)
    {
        GlobalEventBus.Publish(networkEvent);
    }

    public void SubscribeToGlobalEvents<TEvent>(Action<TEvent> handler) where TEvent : INetworkEvent
    {
        GlobalEventBus.Subscribe("topology-subscription", handler);
    }
}
