using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer2.Ethernet;

public class EthernetFrameReceivedEvent : INetworkEvent
{
    public EthernetFrame Frame { get; set; } = new();
    public string IngressInterface { get; set; } = string.Empty;
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; }
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "Ethernet";
    public string Destination => "Layer2";
}

public class EthernetFrameSendEvent : INetworkEvent
{
    public string SourceMac { get; set; } = string.Empty;
    public string DestinationMac { get; set; } = string.Empty;
    public ushort EtherType { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public int? Vlan { get; set; }
    public string EgressInterface { get; set; } = string.Empty;
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; }
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "Layer3";
    public string Destination => "Ethernet";
}

public class EthernetFrameForwardedEvent : INetworkEvent
{
    public EthernetFrame Frame { get; set; } = new();
    public string EgressInterface { get; set; } = string.Empty;
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; }
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "Ethernet";
    public string Destination => "Layer1";
}

public class EthernetFrameFloodedEvent : INetworkEvent
{
    public EthernetFrame Frame { get; set; } = new();
    public string IngressInterface { get; set; } = string.Empty;
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; }
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public EventPriority Priority => EventPriority.Low;
    public string Source => "Ethernet";
    public string Destination => "All";
}
