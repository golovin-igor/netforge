using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Engine;
using NetForge.SimulationModel.Types;

namespace NetForge.Simulation.Engine;

public class SimulationEngine : ISimulationEngine
{
    private readonly Dictionary<string, ITopology> _topologies = new();
    private readonly List<ScheduledEvent> _eventQueue = new();
    private readonly Timer _timer;
    private SimulationState _state = SimulationState.Stopped;
    private TimeSpan _simulationTime = TimeSpan.Zero;
    private double _speedMultiplier = 1.0;
    private DateTime _lastTick = DateTime.UtcNow;

    public SimulationEngine()
    {
        _timer = new Timer(OnTimerTick, null, Timeout.Infinite, Timeout.Infinite);
    }

    public SimulationState State => _state;
    public TimeSpan SimulationTime => _simulationTime;

    public double SpeedMultiplier
    {
        get => _speedMultiplier;
        set => _speedMultiplier = Math.Max(0.1, Math.Min(10.0, value));
    }

    public void Start()
    {
        if (_state == SimulationState.Running) return;

        _state = SimulationState.Running;
        _lastTick = DateTime.UtcNow;
        _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));

        foreach (var topology in _topologies.Values)
        {
            topology.Start();
        }
    }

    public void Pause()
    {
        if (_state != SimulationState.Running) return;

        _state = SimulationState.Paused;
        _timer.Change(Timeout.Infinite, Timeout.Infinite);

        foreach (var topology in _topologies.Values)
        {
            topology.Stop();
        }
    }

    public void Stop()
    {
        _state = SimulationState.Stopped;
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _simulationTime = TimeSpan.Zero;
        _eventQueue.Clear();

        foreach (var topology in _topologies.Values)
        {
            topology.Stop();
        }
    }

    public void Reset()
    {
        Stop();
        foreach (var topology in _topologies.Values)
        {
            topology.Reset();
        }
    }

    public void StepForward(TimeSpan duration)
    {
        var targetTime = _simulationTime + duration;
        while (_simulationTime < targetTime)
        {
            ProcessEventQueue();
            _simulationTime = _simulationTime.Add(TimeSpan.FromMilliseconds(10));
        }
    }

    public void ScheduleEvent(TimeSpan delay, Action action)
    {
        var scheduledTime = _simulationTime + delay;
        _eventQueue.Add(new ScheduledEvent(scheduledTime, action));
        _eventQueue.Sort((a, b) => a.ScheduledTime.CompareTo(b.ScheduledTime));
    }

    public void RegisterTopology(ITopology topology)
    {
        _topologies[topology.Id] = topology;
    }

    public void UnregisterTopology(string topologyId)
    {
        if (_topologies.TryGetValue(topologyId, out var topology))
        {
            topology.Stop();
            _topologies.Remove(topologyId);
        }
    }

    private void OnTimerTick(object? state)
    {
        if (_state != SimulationState.Running) return;

        var now = DateTime.UtcNow;
        var elapsed = (now - _lastTick).Multiply(_speedMultiplier);
        _simulationTime = _simulationTime.Add(elapsed);
        _lastTick = now;

        ProcessEventQueue();
    }

    private void ProcessEventQueue()
    {
        while (_eventQueue.Count > 0 && _eventQueue[0].ScheduledTime <= _simulationTime)
        {
            var scheduledEvent = _eventQueue[0];
            _eventQueue.RemoveAt(0);
            scheduledEvent.Action.Invoke();
        }
    }

    private record ScheduledEvent(TimeSpan ScheduledTime, Action Action);
}
