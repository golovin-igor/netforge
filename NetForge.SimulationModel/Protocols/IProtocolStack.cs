using NetForge.SimulationModel.Events;

namespace NetForge.SimulationModel.Protocols;

public interface IProtocolStack
{
    ILayer1Protocol Physical { get; }
    ILayer2Protocol DataLink { get; }
    ILayer3Protocol Network { get; }
    ILayer4Protocol Transport { get; }
    IApplicationProtocolCollection Applications { get; }
    IEventBus EventBus { get; }

    void RegisterProtocol(INetworkProtocol protocol);
    void UnregisterProtocol(string protocolName);
    T GetProtocol<T>(string name) where T : INetworkProtocol;
    void BroadcastEvent(INetworkEvent networkEvent);
    void SubscribeProtocolToEvents(INetworkProtocol protocol);
}
