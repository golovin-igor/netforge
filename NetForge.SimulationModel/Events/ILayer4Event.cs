using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface ILayer4Event : INetworkEvent
{
    string SourceEndpoint { get; }
    string DestinationEndpoint { get; }
    Layer4EventType EventType { get; }
}
