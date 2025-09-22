using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;


public interface IStpTopologyChangeEvent : ILayer2Event
{
    string RootBridgeId { get; }
    StpPortState OldState { get; }
    StpPortState NewState { get; }
    string AffectedPort { get; }
}
