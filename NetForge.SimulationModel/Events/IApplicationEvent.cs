using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Events;

public interface IApplicationEvent : INetworkEvent
{
    string ApplicationProtocol { get; }
    ApplicationEventType EventType { get; }
}
