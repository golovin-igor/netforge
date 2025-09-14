using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Services
{
    /// <summary>
    /// Service interface for ping operations
    /// </summary>
    public interface IPingService
    {
        /// <summary>
        /// Execute a ping operation
        /// </summary>
        /// <param name="device">The source device</param>
        /// <param name="destination">The destination IP address</param>
        /// <param name="pingCount">Number of ping packets to send</param>
        /// <param name="packetSize">Size of each ping packet in bytes</param>
        /// <returns>Ping result data</returns>
        PingResultData ExecutePing(INetworkDevice device, string destination, int pingCount = 5, int packetSize = 64);

        /// <summary>
        /// Execute a ping operation with advanced options
        /// </summary>
        /// <param name="device">The source device</param>
        /// <param name="options">Advanced ping options</param>
        /// <returns>Ping result data</returns>
        PingResultData ExecutePingWithOptions(INetworkDevice device, PingOptions options);

        /// <summary>
        /// Execute a continuous ping operation
        /// </summary>
        /// <param name="device">The source device</param>
        /// <param name="destination">The destination IP address</param>
        /// <param name="cancellationToken">Token to cancel continuous ping</param>
        /// <returns>Async enumerable of ping results</returns>
        IAsyncEnumerable<PingResultData> ExecuteContinuousPing(INetworkDevice device, string destination, CancellationToken cancellationToken);

        /// <summary>
        /// Update interface counters for ping simulation
        /// </summary>
        /// <param name="device">The device to update</param>
        /// <param name="destination">The destination IP address</param>
        /// <param name="pingCount">Number of ping packets</param>
        /// <param name="packetSize">Size of each packet</param>
        void UpdatePingCounters(INetworkDevice device, string destination, int pingCount, int packetSize);

        /// <summary>
        /// Find the source interface for ping operations
        /// </summary>
        /// <param name="device">The device to search</param>
        /// <returns>The interface to use as ping source, or null if none found</returns>
        dynamic GetPingSourceInterface(INetworkDevice device);

        /// <summary>
        /// Find a specific source interface for ping operations
        /// </summary>
        /// <param name="device">The device to search</param>
        /// <param name="interfaceName">The specific interface name to use</param>
        /// <returns>The specified interface if found and valid, null otherwise</returns>
        dynamic GetPingSourceInterface(INetworkDevice device, string interfaceName);

        /// <summary>
        /// Validate if the target IP address is valid
        /// </summary>
        /// <param name="ipAddress">The IP address to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValidIpAddress(string ipAddress);

        /// <summary>
        /// Validate if the target hostname is valid
        /// </summary>
        /// <param name="hostname">The hostname to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValidHostname(string hostname);

        /// <summary>
        /// Resolve hostname to IP address
        /// </summary>
        /// <param name="hostname">The hostname to resolve</param>
        /// <returns>Resolved IP address or null if resolution fails</returns>
        string? ResolveHostname(string hostname);

        /// <summary>
        /// Check if destination is reachable from device
        /// </summary>
        /// <param name="device">The source device</param>
        /// <param name="destination">The destination IP address</param>
        /// <returns>True if reachable, false otherwise</returns>
        bool IsDestinationReachable(INetworkDevice device, string destination);

        /// <summary>
        /// Calculate round-trip time simulation
        /// </summary>
        /// <param name="device">The source device</param>
        /// <param name="destination">The destination IP address</param>
        /// <returns>Simulated RTT in milliseconds</returns>
        int CalculateRoundTripTime(INetworkDevice device, string destination);
    }

    /// <summary>
    /// Advanced ping options
    /// </summary>
    public class PingOptions
    {
        public string Destination { get; set; } = string.Empty;
        public int PingCount { get; set; } = 5;
        public int PacketSize { get; set; } = 64;
        public int TimeoutSeconds { get; set; } = 2;
        public int Interval { get; set; } = 1000; // milliseconds between pings
        public string? SourceInterface { get; set; }
        public string? SourceIpAddress { get; set; }
        public int? Ttl { get; set; }
        public bool DontFragment { get; set; }
        public bool RecordRoute { get; set; }
        public bool Timestamp { get; set; }
        public bool Verbose { get; set; }
        public bool QuietMode { get; set; }
        public int? Pattern { get; set; } // Data pattern for packet payload
        public int? Tos { get; set; } // Type of Service
    }

    /// <summary>
    /// Data structure for ping results
    /// </summary>
    public class PingResultData
    {
        public string Destination { get; set; } = string.Empty;
        public string? ResolvedAddress { get; set; }
        public int PacketsSent { get; set; }
        public int PacketsReceived { get; set; }
        public int PacketsLost { get; set; }
        public double PacketLossPercentage { get; set; }
        public int MinRoundTripTime { get; set; } = 1;
        public int AvgRoundTripTime { get; set; } = 2;
        public int MaxRoundTripTime { get; set; } = 4;
        public double StandardDeviation { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SourceInterface { get; set; }
        public string? SourceIpAddress { get; set; }
        public int PacketSize { get; set; }
        public int TimeoutSeconds { get; set; } = 2;
        public int? Ttl { get; set; }
        public List<PingReplyData> Replies { get; set; } = new();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }

    /// <summary>
    /// Individual ping reply data
    /// </summary>
    public class PingReplyData
    {
        public int SequenceNumber { get; set; }
        public string FromAddress { get; set; } = string.Empty;
        public int BytesReceived { get; set; }
        public int RoundTripTime { get; set; }
        public int Ttl { get; set; }
        public bool Success { get; set; }
        public string? ErrorType { get; set; } // timeout, unreachable, etc.
        public DateTime Timestamp { get; set; }
    }
}