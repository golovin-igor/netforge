using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;
using System.Collections.Concurrent;

namespace NetForge.Simulation.Common;

public class BaseProtocolState : IProtocolState
{
    private readonly ConcurrentDictionary<string, object> _stateVariables = new();
    private readonly Queue<IProtocolEvent> _recentEvents = new();
    private readonly object _eventLock = new();
    private const int MaxRecentEvents = 100;

    public ProtocolStatus Status { get; protected set; } = ProtocolStatus.Disabled;
    public DateTime LastStateChange { get; protected set; } = DateTime.UtcNow;
    public IReadOnlyDictionary<string, object> StateVariables => _stateVariables;
    public IReadOnlyCollection<IProtocolEvent> RecentEvents
    {
        get
        {
            lock (_eventLock)
            {
                return _recentEvents.ToList().AsReadOnly();
            }
        }
    }

    public void UpdateState(string variable, object value)
    {
        _stateVariables[variable] = value;
        LastStateChange = DateTime.UtcNow;
    }

    public T GetStateValue<T>(string variable)
    {
        if (_stateVariables.TryGetValue(variable, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default!;
    }

    public void AddEvent(IProtocolEvent protocolEvent)
    {
        lock (_eventLock)
        {
            _recentEvents.Enqueue(protocolEvent);
            while (_recentEvents.Count > MaxRecentEvents)
            {
                _recentEvents.Dequeue();
            }
        }
    }

    public void ChangeStatus(ProtocolStatus newStatus)
    {
        Status = newStatus;
        LastStateChange = DateTime.UtcNow;
    }
}
