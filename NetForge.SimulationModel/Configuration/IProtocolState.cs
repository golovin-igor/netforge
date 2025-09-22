using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Configuration;

public interface IProtocolState
{
    ProtocolStatus Status { get; }
    DateTime LastStateChange { get; }
    IReadOnlyDictionary<string, object> StateVariables { get; }
    IReadOnlyCollection<IProtocolEvent> RecentEvents { get; }

    void UpdateState(string variable, object value);
    T GetStateValue<T>(string variable);
    void AddEvent(IProtocolEvent protocolEvent);
}
