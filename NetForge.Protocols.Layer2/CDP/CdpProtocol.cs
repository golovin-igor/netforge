using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Devices;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Protocols;
using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer2.CDP;

public class CdpProtocol : INetworkProtocol
{
    private readonly INetworkDevice _device;
    private readonly Dictionary<string, CdpNeighbor> _neighbors;
    private Timer? _advertisementTimer;
    private Timer? _holdTimer;
    private bool _isRunning;

    public string Id { get; } = Guid.NewGuid().ToString();

    public string Name => "CDP";

    public ProtocolLayer Layer => ProtocolLayer.DataLink;
    public IProtocolState State { get; private set; }
    public IProtocolConfiguration Configuration { get; private set; }
    public IEventBus EventBus { get; }

    public CdpProtocol(INetworkDevice device, IEventBus eventBus)
    {
        _device = device;
        EventBus = eventBus;
        _neighbors = new Dictionary<string, CdpNeighbor>();
        State = new CdpState();
        Configuration = new CdpConfiguration();
    }

    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        SubscribeToEvents();

        var config = Configuration as CdpConfiguration ?? new CdpConfiguration();

        // Start advertisement timer
        _advertisementTimer = new Timer(
            SendAdvertisements,
            null,
            TimeSpan.FromSeconds(config.Timer),
            TimeSpan.FromSeconds(config.Timer)
        );

        // Start hold timer to clean expired neighbors
        _holdTimer = new Timer(
            CleanExpiredNeighbors,
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(30)
        );

        State = new CdpState { Status = ProtocolStatus.Active };
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _advertisementTimer?.Dispose();
        _holdTimer?.Dispose();
        UnsubscribeFromEvents();

        lock (_neighbors)
        {
            _neighbors.Clear();
        }

        State = new CdpState { Status = ProtocolStatus.Disabled };
    }

    public void Reset()
    {
        Stop();
        Start();
    }

    public void ApplyConfiguration(IProtocolConfiguration config)
    {
        if (config is CdpConfiguration cdpConfig)
        {
            Configuration = cdpConfig;

            // Restart timers if configuration changed
            if (_isRunning)
            {
                _advertisementTimer?.Change(
                    TimeSpan.FromSeconds(cdpConfig.Timer),
                    TimeSpan.FromSeconds(cdpConfig.Timer)
                );
            }
        }
    }

    public void OnEventReceived(INetworkEvent networkEvent)
    {
        if (!_isRunning) return;

        if (networkEvent is CdpPacketReceivedEvent cdpEvent)
        {
            ProcessCdpPacket(cdpEvent);
        }
    }

    public void SubscribeToEvents()
    {
        EventBus.Subscribe<CdpPacketReceivedEvent>(Id, OnEventReceived);
    }

    public void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<CdpPacketReceivedEvent>(Id);
    }

    private void SendAdvertisements(object? state)
    {
        if (!_isRunning) return;

        var config = Configuration as CdpConfiguration ?? new CdpConfiguration();

        foreach (var iface in _device.Interfaces)
        {
            if (iface.State == InterfaceState.Up)
            {
                var packet = new CdpPacket
                {
                    Version = config.Version,
                    Ttl = (byte)config.HoldTime,
                    DeviceId = _device.Hostname,
                    Platform = GetPlatform(),
                    Capabilities = GetCapabilities(),
                    InterfaceId = iface.Id,
                    IpAddress = GetManagementAddress()
                };

                packet.AddTlv(CdpTlvType.DeviceId, _device.Hostname);
                packet.AddTlv(CdpTlvType.Platform, GetPlatform());
                packet.AddTlv(CdpTlvType.Capabilities, GetCapabilitiesBytes());
                packet.AddTlv(CdpTlvType.PortId, iface.Id);

                EventBus.Publish(new CdpPacketSendEvent
                {
                    Packet = packet,
                    EgressInterface = iface.Id,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        UpdateState();
    }

    private void ProcessCdpPacket(CdpPacketReceivedEvent cdpEvent)
    {
        var packet = cdpEvent.Packet;
        var neighborId = $"{packet.DeviceId}:{packet.InterfaceId}";

        lock (_neighbors)
        {
            if (_neighbors.ContainsKey(neighborId))
            {
                // Update existing neighbor
                _neighbors[neighborId].Update(packet);
            }
            else
            {
                // Add new neighbor
                var neighbor = new CdpNeighbor
                {
                    DeviceId = packet.DeviceId,
                    Platform = packet.Platform,
                    Capabilities = packet.Capabilities,
                    LocalInterface = cdpEvent.IngressInterface,
                    RemoteInterface = packet.InterfaceId,
                    IpAddress = packet.IpAddress,
                    LastHeard = DateTime.UtcNow,
                    HoldTime = packet.Ttl
                };

                _neighbors[neighborId] = neighbor;

                EventBus.Publish(new CdpNeighborDiscoveredEvent
                {
                    Neighbor = neighbor,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        UpdateState();
    }

    private void CleanExpiredNeighbors(object? state)
    {
        if (!_isRunning) return;

        var now = DateTime.UtcNow;
        var expiredNeighbors = new List<string>();

        lock (_neighbors)
        {
            foreach (var kvp in _neighbors)
            {
                if (now - kvp.Value.LastHeard > TimeSpan.FromSeconds(kvp.Value.HoldTime))
                {
                    expiredNeighbors.Add(kvp.Key);
                }
            }

            foreach (var neighborId in expiredNeighbors)
            {
                var neighbor = _neighbors[neighborId];
                _neighbors.Remove(neighborId);

                EventBus.Publish(new CdpNeighborLostEvent
                {
                    DeviceId = neighbor.DeviceId,
                    RemoteInterface = neighbor.RemoteInterface,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        if (expiredNeighbors.Any())
        {
            UpdateState();
        }
    }

    private void UpdateState()
    {
        lock (_neighbors)
        {
            State = new CdpState
            {
                Status = ProtocolStatus.Active,
                LastUpdate = DateTime.UtcNow,
                Metrics = new Dictionary<string, object>
                {
                    ["NeighborCount"] = _neighbors.Count,
                    ["PacketsSent"] = GetPacketsSent(),
                    ["PacketsReceived"] = GetPacketsReceived()
                }
            };
        }
    }

    private string GetPlatform()
    {
        return $"{_device.Vendor.Name} {_device.Vendor.Model}";
    }

    private string GetCapabilities()
    {
        var caps = _device.Capabilities.Select(capability => capability.Type.ToString()).ToList();

        return string.Join(", ", caps);
    }

    private byte[] GetCapabilitiesBytes()
    {
        uint capabilities = 0;

        foreach (var capability in _device.Capabilities)
        {
            if (capability.Type == DeviceCapabilityType.Router) capabilities |= 0x01;
            if (capability.Type == DeviceCapabilityType.Switch) capabilities |= 0x08;
            if (capability.Type == DeviceCapabilityType.Bridge) capabilities |= 0x04;
        }

        return BitConverter.GetBytes(capabilities);
    }

    private string GetManagementAddress()
    {
        // Return the first IP address found on any interface
        foreach (var iface in _device.Interfaces)
        {
            if (iface.IpAddresses != null && iface.IpAddresses.Any())
            {
                return iface.IpAddresses.First().ToString();
            }
        }
        return "0.0.0.0";
    }

    private long GetPacketsSent()
    {
        // In real implementation, track this metric
        return 0;
    }

    private long GetPacketsReceived()
    {
        // In real implementation, track this metric
        return 0;
    }

    public IReadOnlyDictionary<string, CdpNeighbor> GetNeighbors()
    {
        lock (_neighbors)
        {
            return new Dictionary<string, CdpNeighbor>(_neighbors);
        }
    }
}

public class CdpState : IProtocolState
{
    Dictionary<string, object> stateVariables = new();
    public ProtocolStatus Status { get; set; }
    public DateTime LastStateChange { get; }
    public IReadOnlyDictionary<string, object> StateVariables { get; }
    public IReadOnlyCollection<IProtocolEvent> RecentEvents { get; }

    public void UpdateState(string variable, object value)
    {
        stateVariables[variable] = value;
    }

    public T GetStateValue<T>(string variable)
    {
        if (stateVariables.TryGetValue(variable, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default!;
    }

    public void AddEvent(IProtocolEvent protocolEvent)
    {
        throw new NotImplementedException();
    }

    public DateTime LastUpdate { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}
