using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer1;

public class FrameTransmittedEvent : INetworkEvent
{
    public string DeviceId { get; set; } = string.Empty;
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public string InterfaceId { get; set; } = string.Empty;
    public EthernetFrame Frame { get; set; } = new();
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => $"{DeviceId}:{InterfaceId}";
    public string Destination => "Physical";
}

public class FrameReceivedEvent : INetworkEvent
{
    public string DeviceId { get; set; } = string.Empty;
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public string InterfaceId { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public EventPriority Priority => EventPriority.Normal;
    public string Source => "Physical";
    public string Destination => $"{DeviceId}:{InterfaceId}";
}

public class InterfaceStateChangedEvent : INetworkEvent
{
    public string DeviceId { get; set; } = string.Empty;
    public string SourceProtocol { get; }
    public EventSeverity Severity { get; }
    public EventScope Scope { get; }
    public IDictionary<string, object> Metadata { get; }
    public string InterfaceId { get; set; } = string.Empty;
    public InterfaceState PreviousStatus { get; set; }
    public InterfaceState NewStatus { get; set; }
    public string Id { get; }
    public DateTime Timestamp { get; set; }
    public EventPriority Priority => EventPriority.High;
    public string Source => $"{DeviceId}:{InterfaceId}";
    public string Destination => "All";
}
