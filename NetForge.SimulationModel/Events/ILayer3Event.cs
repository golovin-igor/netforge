using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface ILayer3Event : INetworkEvent
{
    string SourceIp { get; }
    string DestinationIp { get; }
    Layer3EventType EventType { get; }
}
