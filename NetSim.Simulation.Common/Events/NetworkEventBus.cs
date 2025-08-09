namespace NetSim.Simulation.Events
{
    public class NetworkEventBus
    {
        private readonly Dictionary<Type, List<Func<NetworkEventArgs, Task>>> _subscriptions = 
            new Dictionary<Type, List<Func<NetworkEventArgs, Task>>>();

        public void Subscribe<TEventArgs>(Func<TEventArgs, Task> handler) where TEventArgs : NetworkEventArgs
        {
            var eventType = typeof(TEventArgs);
            if (!_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType] = new List<Func<NetworkEventArgs, Task>>();
            }
            _subscriptions[eventType].Add(args => handler((TEventArgs)args));
        }

        public async Task PublishAsync<TEventArgs>(TEventArgs eventArgs) where TEventArgs : NetworkEventArgs
        {
            var eventType = typeof(TEventArgs);
            if (_subscriptions.ContainsKey(eventType))
            {
                var handlers = _subscriptions[eventType].ToList(); // ToList to avoid modification issues if a handler unsubscribes
                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler(eventArgs);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception, but don't let one subscriber crash others.
                        Console.Error.WriteLine($"Error in event handler for {eventType.Name}: {ex.Message}"); 
                        // In a real app, use a proper logging framework.
                    }
                }
            }
        }
    }
} 
