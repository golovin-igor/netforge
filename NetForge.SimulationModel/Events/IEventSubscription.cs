namespace NetForge.SimulationModel.Events;

public interface IEventSubscription
{
    string SubscriberId { get; }
    Type EventType { get; }
    DateTime SubscribedAt { get; }
    long EventsHandled { get; }
    void HandleEvent(INetworkEvent networkEvent);
}
