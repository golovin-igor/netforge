namespace NetForge.SimulationModel.Events;

public interface IEventBus
{
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : INetworkEvent;
    void Subscribe<TEvent>(string subscriberId, Action<TEvent> handler) where TEvent : INetworkEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : INetworkEvent;
    void Unsubscribe<TEvent>(string subscriberId) where TEvent : INetworkEvent;
    void Publish<TEvent>(TEvent networkEvent) where TEvent : INetworkEvent;
    Task PublishAsync<TEvent>(TEvent networkEvent) where TEvent : INetworkEvent;
    void RegisterFilter(IEventFilter filter);
    void ClearSubscriptions();
    IReadOnlyCollection<IEventSubscription> GetSubscriptions();
}
