namespace NetSim.Simulation.Events
{
    public abstract class NetworkEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }

        protected NetworkEventArgs()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
} 
