using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Protocols;
using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer2.Ethernet;

public class EthernetProtocol : INetworkProtocol
{
    private readonly IMacAddressTable _macTable;
    private bool _isRunning;

    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name => "Ethernet";
    public ProtocolLayer Layer => ProtocolLayer.DataLink;
    public IProtocolState State { get; private set; }
    public IProtocolConfiguration Configuration { get; private set; }
    public IEventBus EventBus { get; }

    public EthernetProtocol(IEventBus eventBus, IMacAddressTable macTable)
    {
        EventBus = eventBus;
        _macTable = macTable;
        State = new EthernetState();
        Configuration = new EthernetConfiguration();
    }

    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        SubscribeToEvents();
        State = new EthernetState { Status = ProtocolStatus.Active };
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        UnsubscribeFromEvents();
        State = new EthernetState { Status = ProtocolStatus.Disabled };
    }

    public void Reset()
    {
        Stop();
        _macTable.Clear();
        Start();
    }

    public void ApplyConfiguration(IProtocolConfiguration config)
    {
        if (config is EthernetConfiguration ethConfig)
        {
            Configuration = ethConfig;
        }
    }

    public void OnEventReceived(INetworkEvent networkEvent)
    {
        if (!_isRunning) return;

        switch (networkEvent)
        {
            case EthernetFrameReceivedEvent frameEvent:
                ProcessIncomingFrame(frameEvent);
                break;
            case EthernetFrameSendEvent sendEvent:
                ProcessOutgoingFrame(sendEvent);
                break;
        }
    }

    public void SubscribeToEvents()
    {
        EventBus.Subscribe<EthernetFrameReceivedEvent>(Id, OnEventReceived);
        EventBus.Subscribe<EthernetFrameSendEvent>(Id,OnEventReceived);
    }

    public void UnsubscribeFromEvents()
    {
        EventBus.Unsubscribe<EthernetFrameReceivedEvent>(Id);
        EventBus.Unsubscribe<EthernetFrameSendEvent>(Id);
    }

    private void ProcessIncomingFrame(EthernetFrameReceivedEvent frameEvent)
    {
        var frame = frameEvent.Frame;

        // Learn source MAC address
        _macTable.AddEntry(frame.SourceMac, frameEvent.IngressInterface);

        // Check destination MAC
        if (frame.DestinationMac == "FF:FF:FF:FF:FF:FF")
        {
            // Broadcast frame - flood to all interfaces except ingress
            FloodFrame(frame, frameEvent.IngressInterface);
        }
        else
        {
            // Unicast frame - forward based on MAC table
            var outInterface = _macTable.GetInterface(frame.DestinationMac);
            if (outInterface != null && outInterface != frameEvent.IngressInterface)
            {
                ForwardFrame(frame, outInterface);
            }
            else if (outInterface == null)
            {
                // Unknown unicast - flood
                FloodFrame(frame, frameEvent.IngressInterface);
            }
        }
    }

    private void ProcessOutgoingFrame(EthernetFrameSendEvent sendEvent)
    {
        var frame = new EthernetFrame
        {
            SourceMac = sendEvent.SourceMac,
            DestinationMac = sendEvent.DestinationMac,
            EtherType = sendEvent.EtherType,
            Payload = sendEvent.Payload,
            Vlan = sendEvent.Vlan
        };

        ForwardFrame(frame, sendEvent.EgressInterface);
    }

    private void ForwardFrame(EthernetFrame frame, string egressInterface)
    {
        EventBus.Publish(new EthernetFrameForwardedEvent
        {
            Frame = frame,
            EgressInterface = egressInterface,
            Timestamp = DateTime.UtcNow
        });
    }

    private void FloodFrame(EthernetFrame frame, string ingressInterface)
    {
        EventBus.Publish(new EthernetFrameFloodedEvent
        {
            Frame = frame,
            IngressInterface = ingressInterface,
            Timestamp = DateTime.UtcNow
        });
    }
}

public class EthernetState : IProtocolState
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
