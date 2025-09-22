using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IEventHandler<in TEvent> where TEvent : INetworkEvent
{
    void Handle(TEvent networkEvent);
    EventPriority Priority { get; }
    bool ShouldHandle(TEvent networkEvent);
}
