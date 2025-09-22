using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Engine;

public interface ISimulationEngine
{
    SimulationState State { get; }
    TimeSpan SimulationTime { get; }
    double SpeedMultiplier { get; set; }

    void Start();
    void Pause();
    void Stop();
    void Reset();
    void StepForward(TimeSpan duration);

    void ScheduleEvent(TimeSpan delay, Action action);
    void RegisterTopology(ITopology topology);
    void UnregisterTopology(string topologyId);
}
