namespace NetForge.Simulation.DataTypes.Events
{
    public abstract class NetworkEventArgs : EventArgs
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
