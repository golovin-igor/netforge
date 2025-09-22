using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer2.CDP;

public class CdpPacketReceivedEvent : INetworkEvent
{
    public CdpPacket Packet { get; set; } = new();
    public string IngressInterface { get; set; } = string.Empty;
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; }
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "Layer2";
    public string Destination => "CDP";
}

public class CdpPacketSendEvent : INetworkEvent
{
    public CdpPacket Packet { get; set; } = new();
    public string EgressInterface { get; set; } = string.Empty;
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; }
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public EventPriority Priority => EventPriority.Low;
    public string Source => "CDP";
    public string Destination => "Layer2";
}

public class CdpNeighborDiscoveredEvent : INetworkEvent
{
    public CdpNeighbor Neighbor { get; set; } = new();
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; }
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "CDP";
    public string Destination => "Management";
}

public class CdpNeighborLostEvent : INetworkEvent
{
    public string DeviceId { get; set; } = string.Empty;
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public string RemoteInterface { get; set; } = string.Empty;
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "CDP";
    public string Destination => "Management";
}
