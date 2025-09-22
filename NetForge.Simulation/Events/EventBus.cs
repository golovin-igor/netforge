using System.Collections.Concurrent;
using NetForge.SimulationModel.Events;

namespace NetForge.Simulation.Events;

public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<IEventSubscription>> _subscriptions = new();
    private readonly List<IEventFilter> _filters = new();
    private readonly object _lockObject = new();

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : INetworkEvent
    {
        var eventType = typeof(TEvent);
        var subscription = new EventSubscription<TEvent>(Guid.NewGuid().ToString(), handler);

        lock (_lockObject)
        {
            if (!_subscriptions.ContainsKey(eventType))
                _subscriptions[eventType] = new List<IEventSubscription>();

            _subscriptions[eventType].Add(subscription);
        }
    }

    public void Subscribe<TEvent>(string subscriberId, Action<TEvent> handler) where TEvent : INetworkEvent
    {
        var eventType = typeof(TEvent);
        var subscription = new ActionEventSubscription<TEvent>(subscriberId, handler);

        lock (_lockObject)
        {
            if (!_subscriptions.ContainsKey(eventType))
                _subscriptions[eventType] = new List<IEventSubscription>();

            _subscriptions[eventType].Add(subscription);
        }
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : INetworkEvent
    {
        var eventType = typeof(TEvent);

        lock (_lockObject)
        {
            if (_subscriptions.TryGetValue(eventType, out var subscriptions))
            {
                subscriptions.RemoveAll(s => s is EventSubscription<TEvent> eventSub &&
                    ReferenceEquals(eventSub.Handler, handler));
            }
        }
    }

    public void Unsubscribe<TEvent>(string subscriberId) where TEvent : INetworkEvent
    {
        var eventType = typeof(TEvent);

        lock (_lockObject)
        {
            if (_subscriptions.TryGetValue(eventType, out var subscriptions))
            {
                subscriptions.RemoveAll(s => s.SubscriberId == subscriberId);
            }
        }
    }

    public void Publish<TEvent>(TEvent networkEvent) where TEvent : INetworkEvent
    {
        var eventType = typeof(TEvent);

        if (!ShouldPublishEvent(networkEvent))
            return;

        List<IEventSubscription> subscriptions;
        lock (_lockObject)
        {
            if (!_subscriptions.TryGetValue(eventType, out subscriptions))
                return;

            subscriptions = new List<IEventSubscription>(subscriptions);
        }

        foreach (var subscription in subscriptions)
        {
            try
            {
                subscription.HandleEvent(networkEvent);
            }
            catch (Exception ex)
            {
                // Log error but continue processing other handlers
                Console.WriteLine($"Error handling event {eventType.Name}: {ex.Message}");
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent networkEvent) where TEvent : INetworkEvent
    {
        await Task.Run(() => Publish(networkEvent));
    }

    public void RegisterFilter(IEventFilter filter)
    {
        lock (_lockObject)
        {
            _filters.Add(filter);
        }
    }

    public void ClearSubscriptions()
    {
        lock (_lockObject)
        {
            _subscriptions.Clear();
        }
    }

    public IReadOnlyCollection<IEventSubscription> GetSubscriptions()
    {
        lock (_lockObject)
        {
            return _subscriptions.Values.SelectMany(list => list).ToList().AsReadOnly();
        }
    }

    private bool ShouldPublishEvent<TEvent>(TEvent networkEvent) where TEvent : INetworkEvent
    {
        lock (_lockObject)
        {
            return _filters.All(filter => filter.ShouldPropagate(networkEvent));
        }
    }
}
