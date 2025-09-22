using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

// Concrete event interfaces
public interface ILinkStateChangeEvent : ILayer1Event
{
    LinkState OldState { get; }
    LinkState NewState { get; }
    string Reason { get; }
}
