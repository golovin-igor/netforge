using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface ILayer1Event : INetworkEvent
{
    string InterfaceId { get; }
    Layer1EventType EventType { get; }
}
