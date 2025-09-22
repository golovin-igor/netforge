using NetForge.SimulationModel.Configuration;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.SimulationModel.Protocols;

public interface INetworkProtocol
{
    string Id { get; }
    string Name { get; }
    ProtocolLayer Layer { get; }
    IProtocolState State { get; }
    IProtocolConfiguration Configuration { get; }
    IEventBus EventBus { get; }

    void Start();
    void Stop();
    void Reset();
    void ApplyConfiguration(IProtocolConfiguration config);
    void OnEventReceived(INetworkEvent networkEvent);
    void SubscribeToEvents();
    void UnsubscribeFromEvents();
}
