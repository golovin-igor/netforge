using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Protocols;
using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer2.STP;

public class StpProtocol : INetworkProtocol
{
    private readonly Dictionary<string, PortState> _portStates;
    private readonly object _lock = new();
    private Timer? _bpduTimer;
    private bool _isRunning;

    public string Id { get; }
    public string Name => "STP";
    public ProtocolLayer Layer => ProtocolLayer.DataLink;
    public IProtocolState State { get; private set; }
    public IProtocolConfiguration Configuration { get; private set; }
    public IEventBus EventBus { get; }

    public string BridgeId { get; private set; }
    public string RootBridgeId { get; private set; }
    public int RootPathCost { get; private set; }
    public string RootPortId { get; private set; } = string.Empty;

    public StpProtocol(IEventBus eventBus, string bridgeId)
    {
        EventBus = eventBus;
        BridgeId = bridgeId;
        RootBridgeId = bridgeId; // Initially, we are the root
        RootPathCost = 0;
        _portStates = new Dictionary<string, PortState>();
        State = new StpState();
        Configuration = new StpConfiguration();
    }

    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        SubscribeToEvents();

        // Start BPDU timer
        _bpduTimer = new Timer(SendBpdus, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

        State = new StpState { Status = ProtocolStatus.Active };
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _bpduTimer?.Dispose();
        UnsubscribeFromEvents();
        State = new StpState { Status = ProtocolStatus.Disabled };
    }

    public void Reset()
    {
        Stop();
        lock (_lock)
        {
            _portStates.Clear();
            RootBridgeId = BridgeId;
            RootPathCost = 0;
            RootPortId = string.Empty;
        }
        Start();
    }

    public void ApplyConfiguration(IProtocolConfiguration config)
    {
        if (config is StpConfiguration stpConfig)
        {
            Configuration = stpConfig;

            // Update bridge priority if changed
            if (stpConfig.BridgePriority != GetBridgePriority())
            {
                UpdateBridgeId(stpConfig.BridgePriority);
            }
        }
    }

    public void OnEventReceived(INetworkEvent networkEvent)
    {
        if (!_isRunning) return;

        switch (networkEvent)
        {
            case BpduReceivedEvent bpduEvent:
                ProcessBpdu(bpduEvent);
                break;
            case PortAddedEvent portEvent:
                AddPort(portEvent.PortId);
                break;
            case PortRemovedEvent portEvent:
                RemovePort(portEvent.PortId);
                break;
        }
    }

    public void SubscribeToEvents()
    {
        EventBus.Subscribe<BpduReceivedEvent>(Id, OnEventReceived);
        EventBus.Subscribe<PortAddedEvent>(Id, OnEventReceived);
        EventBus.Subscribe<PortRemovedEvent>(Id, OnEventReceived);
    }

    public void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<BpduReceivedEvent>(Id);
        EventBus.Unsubscribe<PortAddedEvent>(Id);
        EventBus.Unsubscribe<PortRemovedEvent>(Id);
    }

    private void ProcessBpdu(BpduReceivedEvent bpduEvent)
    {
        var bpdu = bpduEvent.Bpdu;
        var portId = bpduEvent.IngressPort;

        lock (_lock)
        {
            if (!_portStates.ContainsKey(portId))
            {
                AddPort(portId);
            }

            // Check if this BPDU advertises a better root
            if (CompareBridgeIds(bpdu.RootBridgeId, RootBridgeId) < 0)
            {
                // New root discovered
                RootBridgeId = bpdu.RootBridgeId;
                RootPathCost = bpdu.RootPathCost + GetPortCost(portId);
                RootPortId = portId;

                // Update port states
                RecalculatePortStates();
            }
            else if (bpdu.RootBridgeId == RootBridgeId &&
                     bpdu.RootPathCost + GetPortCost(portId) < RootPathCost)
            {
                // Better path to root discovered
                RootPathCost = bpdu.RootPathCost + GetPortCost(portId);
                RootPortId = portId;

                RecalculatePortStates();
            }
        }
    }

    private void SendBpdus(object? state)
    {
        if (!_isRunning) return;

        lock (_lock)
        {
            foreach (var port in _portStates.Where(p => p.Value == PortState.Designated))
            {
                var bpdu = new Bpdu
                {
                    RootBridgeId = RootBridgeId,
                    RootPathCost = RootPathCost,
                    BridgeId = BridgeId,
                    PortId = port.Key,
                    MessageAge = 0,
                    MaxAge = 20,
                    HelloTime = 2,
                    ForwardDelay = 15
                };

                EventBus.Publish(new BpduSendEvent
                {
                    Bpdu = bpdu,
                    EgressPort = port.Key,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    private void RecalculatePortStates()
    {
        foreach (var port in _portStates.Keys.ToList())
        {
            if (port == RootPortId)
            {
                _portStates[port] = PortState.Root;
            }
            else if (IsDesignatedPort(port))
            {
                _portStates[port] = PortState.Designated;
            }
            else
            {
                _portStates[port] = PortState.Blocking;
            }
        }
    }

    private bool IsDesignatedPort(string portId)
    {
        // Simplified logic - in real STP, this would involve more complex calculations
        return RootBridgeId == BridgeId || _portStates.Count == 1;
    }

    private int CompareBridgeIds(string id1, string id2)
    {
        return string.Compare(id1, id2, StringComparison.OrdinalIgnoreCase);
    }

    private int GetPortCost(string portId)
    {
        // Simplified - return fixed cost based on port speed
        return 19; // 1 Gbps default
    }

    private int GetBridgePriority()
    {
        // Extract priority from bridge ID (first 4 hex digits)
        if (BridgeId.Length >= 4 && int.TryParse(BridgeId.Substring(0, 4),
            System.Globalization.NumberStyles.HexNumber, null, out var priority))
        {
            return priority;
        }
        return 32768; // Default priority
    }

    private void UpdateBridgeId(int priority)
    {
        // Format: Priority (4 hex) + MAC (12 hex)
        var mac = BridgeId.Length >= 16 ? BridgeId.Substring(4) : "000000000000";
        BridgeId = $"{priority:X4}{mac}";

        // If we were the root, update root bridge ID
        if (RootBridgeId == BridgeId)
        {
            RootBridgeId = BridgeId;
        }
    }

    private void AddPort(string portId)
    {
        lock (_lock)
        {
            if (!_portStates.ContainsKey(portId))
            {
                _portStates[portId] = PortState.Blocking;
                RecalculatePortStates();
            }
        }
    }

    private void RemovePort(string portId)
    {
        lock (_lock)
        {
            _portStates.Remove(portId);
            if (RootPortId == portId)
            {
                // Lost root port, need to recalculate
                RootPortId = string.Empty;
                RecalculatePortStates();
            }
        }
    }
}

public enum PortState
{
    Disabled,
    Blocking,
    Listening,
    Learning,
    Forwarding,
    Root,
    Designated
}

public class StpState : IProtocolState
{
    public ProtocolStatus Status { get; set; }
    public DateTime LastStateChange { get; }
    public IReadOnlyDictionary<string, object> StateVariables { get; }
    public IReadOnlyCollection<IProtocolEvent> RecentEvents { get; }
    public void UpdateState(string variable, object value)
    {
        throw new NotImplementedException();
    }

    public T GetStateValue<T>(string variable)
    {
        throw new NotImplementedException();
    }

    public void AddEvent(IProtocolEvent protocolEvent)
    {
        throw new NotImplementedException();
    }

    public DateTime LastUpdate { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}
