using NetForge.SimulationModel.Events;

namespace NetForge.Simulation.Events;

public class EventSubscription<TEvent> : IEventSubscription where TEvent : INetworkEvent
{
    public string SubscriberId { get; }
    public Type EventType => typeof(TEvent);
    public DateTime SubscribedAt { get; }
    public long EventsHandled { get; private set; }
    public IEventHandler<TEvent> Handler { get; }

    public EventSubscription(string subscriberId, IEventHandler<TEvent> handler)
    {
        SubscriberId = subscriberId;
        Handler = handler;
        SubscribedAt = DateTime.UtcNow;
        EventsHandled = 0;
    }

    public void HandleEvent(INetworkEvent networkEvent)
    {
        if (networkEvent is TEvent typedEvent)
        {
            Handler.Handle(typedEvent);
            EventsHandled++;
        }
    }
}

public class ActionEventSubscription<TEvent> : IEventSubscription where TEvent : INetworkEvent
{
    public string SubscriberId { get; }
    public Type EventType => typeof(TEvent);
    public DateTime SubscribedAt { get; }
    public long EventsHandled { get; private set; }
    public Action<TEvent> Handler { get; }

    public ActionEventSubscription(string subscriberId, Action<TEvent> handler)
    {
        SubscriberId = subscriberId;
        Handler = handler;
        SubscribedAt = DateTime.UtcNow;
        EventsHandled = 0;
    }

    public void HandleEvent(INetworkEvent networkEvent)
    {
        if (networkEvent is TEvent typedEvent)
        {
            Handler(typedEvent);
            EventsHandled++;
        }
    }
}
