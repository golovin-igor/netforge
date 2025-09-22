using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IOspfNeighborEvent : ILayer3Event
{
    string NeighborId { get; }
    OspfState OldState { get; }
    OspfState NewState { get; }
}
