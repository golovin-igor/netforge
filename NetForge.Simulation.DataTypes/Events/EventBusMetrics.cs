namespace NetForge.Simulation.Common.Events
{
    /// <summary>
    /// Metrics for monitoring EventBus performance and usage
    /// </summary>
    public class EventBusMetrics
    {
        /// <summary>
        /// Total number of events published
        /// </summary>
        public long EventsPublished { get; set; }

        /// <summary>
        /// Total number of events processed by all handlers
        /// </summary>
        public long EventsProcessed { get; set; }

        /// <summary>
        /// Average processing time per event
        /// </summary>
        public TimeSpan AverageProcessingTime { get; set; }

        /// <summary>
        /// Count of events by type
        /// </summary>
        public Dictionary<string, long> EventTypeCounters { get; set; } = new();

        /// <summary>
        /// Total number of active subscriptions
        /// </summary>
        public int ActiveSubscriptions { get; set; }

        /// <summary>
        /// Number of failed event processing attempts
        /// </summary>
        public long ProcessingErrors { get; set; }

        /// <summary>
        /// When metrics collection started
        /// </summary>
        public DateTime CollectionStartTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a summary string of the metrics
        /// </summary>
        public override string ToString()
        {
            var uptime = DateTime.UtcNow - CollectionStartTime;
            return $"EventBus Metrics - Published: {EventsPublished}, Processed: {EventsProcessed}, " +
                   $"Avg Time: {AverageProcessingTime.TotalMilliseconds:F2}ms, Errors: {ProcessingErrors}, " +
                   $"Subscriptions: {ActiveSubscriptions}, Uptime: {uptime:hh\\:mm\\:ss}";
        }
    }
}