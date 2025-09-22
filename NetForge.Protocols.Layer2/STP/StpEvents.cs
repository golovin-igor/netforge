using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer2.STP;

public class BpduReceivedEvent : INetworkEvent
{
    public Bpdu Bpdu { get; set; } = new();
    public string IngressPort { get; set; } = string.Empty;
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; }
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public EventPriority Priority => EventPriority.High;
    public string Source => "Layer2";
    public string Destination => "STP";
}

public class BpduSendEvent : INetworkEvent
{
    public Bpdu Bpdu { get; set; } = new();
    public string EgressPort { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public EventPriority Priority => EventPriority.High;
    public string Source => "STP";
    public string Destination => "Layer2";
}

public class PortAddedEvent : INetworkEvent
{
    public string PortId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "System";
    public string Destination => "STP";
}

public class PortRemovedEvent : INetworkEvent
{
    public string PortId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "System";
    public string Destination => "STP";
}

public class TopologyChangeEvent : INetworkEvent
{
    public string RootBridgeId { get; set; } = string.Empty;
    public Dictionary<string, PortState> PortStates { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public EventPriority Priority => EventPriority.High;
    public string Source => "STP";
    public string Destination => "All";
}
