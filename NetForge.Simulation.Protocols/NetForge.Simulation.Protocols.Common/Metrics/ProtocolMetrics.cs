using System.Collections.Concurrent;

namespace NetForge.Simulation.Protocols.Common.Metrics
{
    /// <summary>
    /// Concrete implementation of protocol metrics for performance monitoring
    /// Thread-safe implementation for concurrent protocol operations
    /// </summary>
    public class ProtocolMetrics : IProtocolMetrics
    {
        private readonly object _lock = new object();
        private readonly ConcurrentQueue<string> _recentErrors = new();
        private readonly List<TimeSpan> _processingTimes = new();
        private const int MaxErrorHistory = 10;
        private const int MaxProcessingTimeHistory = 100;

        // Private backing fields for thread-safe operations
        private long _packetsSent;
        private long _packetsReceived;
        private long _packetsDropped;
        private long _malformedPackets;
        private long _updatesProcessed;
        private long _memoryUsage;
        private long _errorCount;
        private TimeSpan _averageProcessingTime = TimeSpan.Zero;
        private TimeSpan _maxProcessingTime = TimeSpan.Zero;
        private TimeSpan _cpuTime = TimeSpan.Zero;
        private DateTime _lastActivity = DateTime.Now;
        private DateTime _metricsStartTime = DateTime.Now;

        // Public properties implementing interface
        public long PacketsSent => _packetsSent;
        public long PacketsReceived => _packetsReceived;
        public long PacketsDropped => _packetsDropped;
        public long MalformedPackets => _malformedPackets;
        public TimeSpan AverageProcessingTime => _averageProcessingTime;
        public TimeSpan MaxProcessingTime => _maxProcessingTime;
        public long UpdatesProcessed => _updatesProcessed;
        public long MemoryUsage => _memoryUsage;
        public TimeSpan CpuTime => _cpuTime;
        public DateTime LastActivity => _lastActivity;
        public DateTime MetricsStartTime => _metricsStartTime;
        public long ErrorCount => _errorCount;
        public IEnumerable<string> RecentErrors => _recentErrors.ToArray();

        /// <summary>
        /// Reset all metrics to zero and clear history
        /// </summary>
        public void ResetMetrics()
        {
            lock (_lock)
            {
                _packetsSent = 0;
                _packetsReceived = 0;
                _packetsDropped = 0;
                _malformedPackets = 0;
                _averageProcessingTime = TimeSpan.Zero;
                _maxProcessingTime = TimeSpan.Zero;
                _updatesProcessed = 0;
                _memoryUsage = 0;
                _cpuTime = TimeSpan.Zero;
                _errorCount = 0;
                _metricsStartTime = DateTime.Now;
                _lastActivity = DateTime.Now;

                _processingTimes.Clear();
                while (_recentErrors.TryDequeue(out _)) { }
            }
        }

        /// <summary>
        /// Record that a packet was sent
        /// </summary>
        public void RecordPacketSent()
        {
            Interlocked.Increment(ref _packetsSent);
            UpdateLastActivity();
        }

        /// <summary>
        /// Record that a packet was received
        /// </summary>
        public void RecordPacketReceived()
        {
            Interlocked.Increment(ref _packetsReceived);
            UpdateLastActivity();
        }

        /// <summary>
        /// Record that a packet was dropped
        /// </summary>
        /// <param name="reason">Reason for dropping the packet</param>
        public void RecordPacketDropped(string reason = null)
        {
            Interlocked.Increment(ref _packetsDropped);
            UpdateLastActivity();

            if (!string.IsNullOrEmpty(reason))
            {
                RecordError($"Packet dropped: {reason}");
            }
        }

        /// <summary>
        /// Record that a malformed packet was received
        /// </summary>
        public void RecordMalformedPacket()
        {
            Interlocked.Increment(ref _malformedPackets);
            UpdateLastActivity();
        }

        /// <summary>
        /// Record processing time for a protocol update
        /// </summary>
        /// <param name="processingTime">Time taken for processing</param>
        public void RecordProcessingTime(TimeSpan processingTime)
        {
            lock (_lock)
            {
                Interlocked.Increment(ref _updatesProcessed);
                
                _processingTimes.Add(processingTime);
                if (_processingTimes.Count > MaxProcessingTimeHistory)
                {
                    _processingTimes.RemoveAt(0);
                }

                // Update max processing time
                if (processingTime > _maxProcessingTime)
                {
                    _maxProcessingTime = processingTime;
                }

                // Recalculate average
                if (_processingTimes.Count > 0)
                {
                    var totalTicks = _processingTimes.Sum(t => t.Ticks);
                    _averageProcessingTime = new TimeSpan(totalTicks / _processingTimes.Count);
                }

                UpdateLastActivity();
            }
        }

        /// <summary>
        /// Record an error that occurred during protocol operation
        /// </summary>
        /// <param name="error">Error description</param>
        public void RecordError(string error)
        {
            Interlocked.Increment(ref _errorCount);
            
            var timestampedError = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {error}";
            _recentErrors.Enqueue(timestampedError);

            // Keep only the most recent errors
            while (_recentErrors.Count > MaxErrorHistory)
            {
                _recentErrors.TryDequeue(out _);
            }

            UpdateLastActivity();
        }

        /// <summary>
        /// Update memory usage estimate
        /// </summary>
        /// <param name="memoryBytes">Memory usage in bytes</param>
        public void UpdateMemoryUsage(long memoryBytes)
        {
            Interlocked.Exchange(ref _memoryUsage, memoryBytes);
        }

        /// <summary>
        /// Add CPU time consumed by protocol operations
        /// </summary>
        /// <param name="cpuTime">CPU time to add</param>
        public void AddCpuTime(TimeSpan cpuTime)
        {
            lock (_lock)
            {
                _cpuTime = _cpuTime.Add(cpuTime);
            }
        }

        /// <summary>
        /// Get a summary of all metrics as a dictionary
        /// </summary>
        /// <returns>Dictionary containing all metric values</returns>
        public Dictionary<string, object> GetMetricsSummary()
        {
            lock (_lock)
            {
                var uptime = DateTime.Now - MetricsStartTime;
                var packetsPerSecond = uptime.TotalSeconds > 0 ? 
                    (PacketsSent + PacketsReceived) / uptime.TotalSeconds : 0;

                return new Dictionary<string, object>
                {
                    ["PacketsSent"] = PacketsSent,
                    ["PacketsReceived"] = PacketsReceived,
                    ["PacketsDropped"] = PacketsDropped,
                    ["MalformedPackets"] = MalformedPackets,
                    ["PacketsPerSecond"] = Math.Round(packetsPerSecond, 2),
                    ["AverageProcessingTimeMs"] = Math.Round(AverageProcessingTime.TotalMilliseconds, 2),
                    ["MaxProcessingTimeMs"] = Math.Round(MaxProcessingTime.TotalMilliseconds, 2),
                    ["UpdatesProcessed"] = UpdatesProcessed,
                    ["MemoryUsageKB"] = Math.Round(MemoryUsage / 1024.0, 2),
                    ["CpuTimeMs"] = Math.Round(CpuTime.TotalMilliseconds, 2),
                    ["ErrorCount"] = ErrorCount,
                    ["UptimeSeconds"] = Math.Round(uptime.TotalSeconds, 2),
                    ["LastActivity"] = LastActivity,
                    ["MetricsStartTime"] = MetricsStartTime
                };
            }
        }

        /// <summary>
        /// Update the last activity timestamp
        /// </summary>
        private void UpdateLastActivity()
        {
            _lastActivity = DateTime.Now;
        }

        /// <summary>
        /// Get performance score based on current metrics
        /// </summary>
        /// <returns>Performance score from 0-100</returns>
        public double GetPerformanceScore()
        {
            lock (_lock)
            {
                double score = 100.0;

                // Deduct points for errors
                if (ErrorCount > 0)
                {
                    var errorRate = (double)ErrorCount / Math.Max(UpdatesProcessed, 1);
                    score -= Math.Min(errorRate * 100, 50); // Max 50 points deducted for errors
                }

                // Deduct points for packet drops
                if (PacketsDropped > 0)
                {
                    var dropRate = (double)PacketsDropped / Math.Max(PacketsSent + PacketsReceived, 1);
                    score -= Math.Min(dropRate * 100, 30); // Max 30 points deducted for drops
                }

                // Deduct points for slow processing
                if (AverageProcessingTime.TotalMilliseconds > 100)
                {
                    var slownessPenalty = Math.Min((AverageProcessingTime.TotalMilliseconds - 100) / 10, 20);
                    score -= slownessPenalty; // Max 20 points deducted for slowness
                }

                return Math.Max(score, 0);
            }
        }
    }
}