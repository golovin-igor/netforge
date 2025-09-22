using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface INetworkEvent
{
    string Id { get; }
    DateTime Timestamp { get; }
    string DeviceId { get; }
    string SourceProtocol { get; }
    EventSeverity Severity { get; }
    EventScope Scope { get; }
    IDictionary<string, object> Metadata { get; }
}
