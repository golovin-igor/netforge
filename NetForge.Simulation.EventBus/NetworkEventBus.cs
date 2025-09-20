using System.Collections.Concurrent;
using System.Diagnostics;
using NetForge.Interfaces.Events;
using NetForge.Simulation.Common.Events;
using NetForge.Simulation.DataTypes.Events;

namespace NetForge.Simulation.EventBus
{
    /// <summary>
    /// Enhanced NetworkEventBus with batch processing, filtering, and metrics
    /// </summary>
    public class NetworkEventBus : INetworkEventBus
    {
        private readonly ConcurrentDictionary<Type, List<EventSubscription>> _subscriptions = new();
        private readonly EventBusMetrics _metrics = new();
        private readonly object _metricsLock = new();

        /// <summary>
        /// Subscribe to events with basic handler
        /// </summary>
        public void Subscribe<TEventArgs>(Func<TEventArgs, Task> handler) where TEventArgs : NetworkEventArgs
        {
            Subscribe("*", handler);
        }

        /// <summary>
        /// Subscribe to events with filter pattern
        /// </summary>
        public void Subscribe<TEventArgs>(string filter, Func<TEventArgs, Task> handler) where TEventArgs : NetworkEventArgs
        {
            var eventType = typeof(TEventArgs);
            var subscription = new EventSubscription
            {
                Filter = new EventFilter { DeviceNamePattern = filter },
                Handler = args => handler((TEventArgs)args)
            };

            _subscriptions.AddOrUpdate(eventType,
                new List<EventSubscription> { subscription },
                (key, existing) =>
                {
                    var newList = new List<EventSubscription>(existing) { subscription };
                    return newList;
                });

            lock (_metricsLock)
            {
                _metrics.ActiveSubscriptions++;
            }
        }

        /// <summary>
        /// Publish a single event
        /// </summary>
        public async Task PublishAsync<TEventArgs>(TEventArgs eventArgs) where TEventArgs : NetworkEventArgs
        {
            var stopwatch = Stopwatch.StartNew();

            lock (_metricsLock)
            {
                _metrics.EventsPublished++;
                var eventTypeName = typeof(TEventArgs).Name;
                _metrics.EventTypeCounters[eventTypeName] = _metrics.EventTypeCounters.GetValueOrDefault(eventTypeName) + 1;
            }

            var eventType = typeof(TEventArgs);
            if (_subscriptions.TryGetValue(eventType, out var subscriptions))
            {
                var matchingSubscriptions = subscriptions.Where(s => s.Filter.Matches(eventArgs)).ToList();

                foreach (var subscription in matchingSubscriptions)
                {
                    try
                    {
                        await subscription.Handler(eventArgs);

                        lock (_metricsLock)
                        {
                            _metrics.EventsProcessed++;
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (_metricsLock)
                        {
                            _metrics.ProcessingErrors++;
                        }

                        // Log the exception, but don't let one subscriber crash others
                        Console.Error.WriteLine($"Error in event handler for {eventType.Name}: {ex.Message}");
                    }
                }
            }

            stopwatch.Stop();
            UpdateAverageProcessingTime(stopwatch.Elapsed);
        }

        /// <summary>
        /// Publish multiple events in batch for better performance
        /// </summary>
        public async Task PublishBatchAsync<TEventArgs>(IEnumerable<TEventArgs> events) where TEventArgs : NetworkEventArgs
        {
            var eventList = events.ToList();
            if (eventList.Count == 0) return;

            // For small batches, process sequentially for simplicity
            if (eventList.Count <= 10)
            {
                foreach (var eventArgs in eventList)
                {
                    await PublishAsync(eventArgs);
                }
                return;
            }

            // For larger batches, process in parallel with limited concurrency
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = eventList.Select(async eventArgs =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await PublishAsync(eventArgs);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Get current EventBus metrics
        /// </summary>
        public EventBusMetrics GetMetrics()
        {
            lock (_metricsLock)
            {
                return new EventBusMetrics
                {
                    EventsPublished = _metrics.EventsPublished,
                    EventsProcessed = _metrics.EventsProcessed,
                    AverageProcessingTime = _metrics.AverageProcessingTime,
                    EventTypeCounters = new Dictionary<string, long>(_metrics.EventTypeCounters),
                    ActiveSubscriptions = _metrics.ActiveSubscriptions,
                    ProcessingErrors = _metrics.ProcessingErrors,
                    CollectionStartTime = _metrics.CollectionStartTime
                };
            }
        }

        /// <summary>
        /// Updates the rolling average processing time
        /// </summary>
        private void UpdateAverageProcessingTime(TimeSpan processingTime)
        {
            lock (_metricsLock)
            {
                if (_metrics.EventsProcessed == 0)
                {
                    _metrics.AverageProcessingTime = processingTime;
                }
                else
                {
                    // Simple moving average approximation
                    var totalTime = _metrics.AverageProcessingTime.Ticks * _metrics.EventsProcessed;
                    _metrics.AverageProcessingTime = new TimeSpan((totalTime + processingTime.Ticks) / (_metrics.EventsProcessed + 1));
                }
            }
        }
    }

    /// <summary>
    /// Internal subscription information
    /// </summary>
    internal class EventSubscription
    {
        public EventFilter Filter { get; set; } = new();
        public Func<NetworkEventArgs, Task> Handler { get; set; } = _ => Task.CompletedTask;
    }
}
