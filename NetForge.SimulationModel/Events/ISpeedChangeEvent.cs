using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface ISpeedChangeEvent : ILayer1Event
{
    LinkSpeed OldSpeed { get; }
    LinkSpeed NewSpeed { get; }
}
