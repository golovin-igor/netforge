namespace NetForge.Simulation.Common.Events;

public interface INetworkEventBus
{
    // Core functionality
    void Subscribe<TEventArgs>(Func<TEventArgs, Task> handler) where TEventArgs : NetworkEventArgs;
    Task PublishAsync<TEventArgs>(TEventArgs eventArgs) where TEventArgs : NetworkEventArgs;

    // Enhanced functionality
    Task PublishBatchAsync<TEventArgs>(IEnumerable<TEventArgs> events) where TEventArgs : NetworkEventArgs;
    void Subscribe<TEventArgs>(string filter, Func<TEventArgs, Task> handler) where TEventArgs : NetworkEventArgs;

    // Metrics and monitoring
    EventBusMetrics GetMetrics();
}
