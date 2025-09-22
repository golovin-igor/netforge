using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface ILayer2Event : INetworkEvent
{
    string InterfaceId { get; }
    string SourceMac { get; }
    string DestinationMac { get; }
    Layer2EventType EventType { get; }
}
