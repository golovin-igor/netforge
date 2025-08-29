namespace NetForge.Simulation.Common.Events
{
    public abstract class NetworkEventArgs : EventArgs
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
