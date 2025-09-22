using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IEventFilter
{
    bool ShouldPropagate(INetworkEvent networkEvent);
    EventFilterType Type { get; }
}
