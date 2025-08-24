namespace NetForge.Simulation.Common.Common
{
    /// <summary>
    /// Represents physical connection quality metrics that protocols can use for decision making
    /// </summary>
    public class PhysicalConnectionMetrics
    {
        public string ConnectionId { get; set; } = "";
        public PhysicalConnectionState State { get; set; }
        public PhysicalConnectionType ConnectionType { get; set; }

        // Performance metrics
        public int Bandwidth { get; set; } // Mbps
        public int Latency { get; set; } // milliseconds
        public double PacketLoss { get; set; } // percentage
        public int MaxTransmissionUnit { get; set; } // bytes

        // Health metrics
        public int ErrorCount { get; set; }

        /// <summary>
        /// Calculate a quality score (0-100) based on connection metrics
        /// </summary>
        public double QualityScore
        {
            get
            {
                if (State != PhysicalConnectionState.Connected && State != PhysicalConnectionState.Degraded)
                    return 0.0;

                double score = 100.0;

                // Reduce score based on packet loss
                score -= PacketLoss * 2; // 2% score reduction per 1% packet loss

                // Reduce score based on latency (above baseline)
                if (Latency > 1)
                {
                    score -= (Latency - 1) * 0.5; // 0.5% score reduction per ms above 1ms
                }

                // Reduce score based on error count
                score -= Math.Min(ErrorCount * 0.1, 10); // Max 10% reduction for errors

                return Math.Max(score, 0.0);
            }
        }

        /// <summary>
        /// Determine if this connection is suitable for time-sensitive protocols
        /// </summary>
        public bool IsSuitableForRealTime => PacketLoss < 1.0 && Latency < 10 && State == PhysicalConnectionState.Connected;

        /// <summary>
        /// Determine if this connection is suitable for routing protocols
        /// </summary>
        public bool IsSuitableForRouting => PacketLoss < 5.0 && State != PhysicalConnectionState.Failed;
    }
}
