namespace NetForge.Simulation.Protocols.Common.Metrics
{
    /// <summary>
    /// Interface for protocol performance metrics and monitoring
    /// Provides standardized metrics collection across all protocols
    /// </summary>
    public interface IProtocolMetrics
    {
        // Packet statistics
        /// <summary>
        /// Total number of packets sent by this protocol
        /// </summary>
        long PacketsSent { get; }

        /// <summary>
        /// Total number of packets received by this protocol
        /// </summary>
        long PacketsReceived { get; }

        /// <summary>
        /// Total number of packets dropped by this protocol
        /// </summary>
        long PacketsDropped { get; }

        /// <summary>
        /// Total number of malformed packets received
        /// </summary>
        long MalformedPackets { get; }

        // Processing statistics
        /// <summary>
        /// Average time taken to process protocol updates
        /// </summary>
        TimeSpan AverageProcessingTime { get; }

        /// <summary>
        /// Maximum time taken for a single protocol update
        /// </summary>
        TimeSpan MaxProcessingTime { get; }

        /// <summary>
        /// Number of protocol updates processed
        /// </summary>
        long UpdatesProcessed { get; }

        // Resource usage
        /// <summary>
        /// Estimated memory usage of this protocol in bytes
        /// </summary>
        long MemoryUsage { get; }

        /// <summary>
        /// CPU time consumed by this protocol
        /// </summary>
        TimeSpan CpuTime { get; }

        // Activity tracking
        /// <summary>
        /// Timestamp of the last protocol activity
        /// </summary>
        DateTime LastActivity { get; }

        /// <summary>
        /// Timestamp when metrics collection started
        /// </summary>
        DateTime MetricsStartTime { get; }

        // Error tracking
        /// <summary>
        /// Number of errors encountered during protocol operation
        /// </summary>
        long ErrorCount { get; }

        /// <summary>
        /// Details of recent errors (limited to last N errors)
        /// </summary>
        IEnumerable<string> RecentErrors { get; }

        // Methods for metrics management
        /// <summary>
        /// Reset all metrics to zero
        /// </summary>
        void ResetMetrics();

        /// <summary>
        /// Record that a packet was sent
        /// </summary>
        void RecordPacketSent();

        /// <summary>
        /// Record that a packet was received
        /// </summary>
        void RecordPacketReceived();

        /// <summary>
        /// Record that a packet was dropped
        /// </summary>
        /// <param name="reason">Reason for dropping the packet</param>
        void RecordPacketDropped(string? reason = null);

        /// <summary>
        /// Record processing time for a protocol update
        /// </summary>
        /// <param name="processingTime">Time taken for processing</param>
        void RecordProcessingTime(TimeSpan processingTime);

        /// <summary>
        /// Record an error that occurred during protocol operation
        /// </summary>
        /// <param name="error">Error description</param>
        void RecordError(string error);

        /// <summary>
        /// Get a summary of all metrics as a dictionary
        /// </summary>
        /// <returns>Dictionary containing all metric values</returns>
        Dictionary<string, object> GetMetricsSummary();
    }
}