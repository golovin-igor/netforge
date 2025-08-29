namespace NetForge.Simulation.Common.Events;

public interface INetworkEventBus
{
    void Subscribe<TEventArgs>(Func<TEventArgs, Task> handler) where TEventArgs : NetworkEventArgs;
    Task PublishAsync<TEventArgs>(TEventArgs eventArgs) where TEventArgs : NetworkEventArgs;
}
