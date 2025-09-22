using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.Simulation.Common;

public abstract class BaseNetworkEvent : INetworkEvent
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public abstract string DeviceId { get; }
    public abstract string SourceProtocol { get; }
    public virtual EventSeverity Severity => EventSeverity.Information;
    public virtual EventScope Scope => EventScope.Device;
    public virtual IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
}
