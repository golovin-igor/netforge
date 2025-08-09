using NetSim.Simulation.Common;

namespace NetSim.Simulation.Factories
{
    /// <summary>
    /// Result of a network topology conversion operation
    /// </summary>
    public class NetworkConversionResult
    {
        /// <summary>
        /// Whether the conversion was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The converted network instance
        /// </summary>
        public Network Network { get; set; }

        /// <summary>
        /// Summary message of the conversion operation
        /// </summary>
        public string Summary { get; set; } = "";

        /// <summary>
        /// List of errors encountered during conversion
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// List of warnings generated during conversion
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Information about devices that were successfully converted
        /// Key: Original device ID, Value: Converted device name
        /// </summary>
        public Dictionary<string, string> ConvertedDevices { get; set; } = new();

        /// <summary>
        /// List of connection IDs that were successfully converted
        /// </summary>
        public List<string> ConvertedConnections { get; set; } = new();

        /// <summary>
        /// Devices that failed to convert
        /// Key: Original device ID, Value: Error message
        /// </summary>
        public Dictionary<string, string> FailedDevices { get; set; } = new();

        /// <summary>
        /// Connections that failed to convert
        /// Key: Original connection ID, Value: Error message
        /// </summary>
        public Dictionary<string, string> FailedConnections { get; set; } = new();

        /// <summary>
        /// Conversion start time
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Conversion end time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Total conversion duration
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// Statistics about the conversion process
        /// </summary>
        public NetworkConversionStatistics Statistics => new()
        {
            TotalDevicesProcessed = ConvertedDevices.Count + FailedDevices.Count,
            TotalConnectionsProcessed = ConvertedConnections.Count + FailedConnections.Count,
            DevicesConverted = ConvertedDevices.Count,
            ConnectionsConverted = ConvertedConnections.Count,
            DevicesFailed = FailedDevices.Count,
            ConnectionsFailed = FailedConnections.Count,
            ErrorCount = Errors.Count,
            WarningCount = Warnings.Count,
            ConversionDuration = Duration
        };

        /// <summary>
        /// Mark the conversion as completed
        /// </summary>
        public void MarkCompleted()
        {
            EndTime = DateTime.UtcNow;
            if (Errors.Count == 0)
            {
                Success = true;
            }
        }

        /// <summary>
        /// Add an error to the result
        /// </summary>
        public void AddError(string error)
        {
            Errors.Add($"[{DateTime.UtcNow:HH:mm:ss}] {error}");
            Success = false;
        }

        /// <summary>
        /// Add a warning to the result
        /// </summary>
        public void AddWarning(string warning)
        {
            Warnings.Add($"[{DateTime.UtcNow:HH:mm:ss}] {warning}");
        }

        /// <summary>
        /// Record a successful device conversion
        /// </summary>
        public void RecordDeviceConversion(string originalId, string convertedName)
        {
            ConvertedDevices[originalId] = convertedName;
        }

        /// <summary>
        /// Record a failed device conversion
        /// </summary>
        public void RecordDeviceFailure(string originalId, string errorMessage)
        {
            FailedDevices[originalId] = errorMessage;
        }

        /// <summary>
        /// Record a successful connection conversion
        /// </summary>
        public void RecordConnectionConversion(string connectionId)
        {
            ConvertedConnections.Add(connectionId);
        }

        /// <summary>
        /// Record a failed connection conversion
        /// </summary>
        public void RecordConnectionFailure(string connectionId, string errorMessage)
        {
            FailedConnections[connectionId] = errorMessage;
        }

        /// <summary>
        /// Get a detailed report of the conversion results
        /// </summary>
        public string GetDetailedReport()
        {
            var report = new List<string>();

            report.Add("=== Network Topology Conversion Report ===");
            report.Add($"Conversion Status: {(Success ? "SUCCESS" : "FAILED")}");
            report.Add($"Duration: {Duration.TotalSeconds:F2} seconds");
            report.Add("");

            report.Add("=== Summary ===");
            report.Add(Summary);
            report.Add("");

            var stats = Statistics;
            report.Add("=== Statistics ===");
            report.Add($"Total Devices Processed: {stats.TotalDevicesProcessed}");
            report.Add($"  - Successfully Converted: {stats.DevicesConverted}");
            report.Add($"  - Failed: {stats.DevicesFailed}");
            report.Add($"Total Connections Processed: {stats.TotalConnectionsProcessed}");
            report.Add($"  - Successfully Converted: {stats.ConnectionsConverted}");
            report.Add($"  - Failed: {stats.ConnectionsFailed}");
            report.Add($"Errors: {stats.ErrorCount}");
            report.Add($"Warnings: {stats.WarningCount}");
            report.Add("");

            if (ConvertedDevices.Any())
            {
                report.Add("=== Converted Devices ===");
                foreach (var device in ConvertedDevices)
                {
                    report.Add($"  {device.Key} -> {device.Value}");
                }
                report.Add("");
            }

            if (FailedDevices.Any())
            {
                report.Add("=== Failed Devices ===");
                foreach (var device in FailedDevices)
                {
                    report.Add($"  {device.Key}: {device.Value}");
                }
                report.Add("");
            }

            if (ConvertedConnections.Any())
            {
                report.Add("=== Converted Connections ===");
                foreach (var connection in ConvertedConnections)
                {
                    report.Add($"  {connection}");
                }
                report.Add("");
            }

            if (FailedConnections.Any())
            {
                report.Add("=== Failed Connections ===");
                foreach (var connection in FailedConnections)
                {
                    report.Add($"  {connection.Key}: {connection.Value}");
                }
                report.Add("");
            }

            if (Warnings.Any())
            {
                report.Add("=== Warnings ===");
                foreach (var warning in Warnings)
                {
                    report.Add($"  {warning}");
                }
                report.Add("");
            }

            if (Errors.Any())
            {
                report.Add("=== Errors ===");
                foreach (var error in Errors)
                {
                    report.Add($"  {error}");
                }
                report.Add("");
            }

            if (Network != null)
            {
                var networkStats = Network.GetNetworkStatistics();
                report.Add("=== Network Statistics ===");
                report.Add($"Total Devices: {networkStats.TotalDevices}");
                report.Add($"Total Connections: {networkStats.TotalConnections}");
                report.Add($"Operational Connections: {networkStats.OperationalConnections}");
                report.Add($"Failed Connections: {networkStats.FailedConnections}");
                report.Add($"Degraded Connections: {networkStats.DegradedConnections}");
                report.Add($"Connection Reliability: {networkStats.ConnectionReliability:F1}%");
            }

            return string.Join(Environment.NewLine, report);
        }

        /// <summary>
        /// Get a summary report for logging
        /// </summary>
        public string GetSummaryReport()
        {
            var stats = Statistics;
            return $"Conversion {(Success ? "SUCCESS" : "FAILED")}: " +
                   $"{stats.DevicesConverted}/{stats.TotalDevicesProcessed} devices, " +
                   $"{stats.ConnectionsConverted}/{stats.TotalConnectionsProcessed} connections, " +
                   $"{stats.ErrorCount} errors, {stats.WarningCount} warnings " +
                   $"({Duration.TotalSeconds:F1}s)";
        }
    }

    /// <summary>
    /// Statistics about the network conversion process
    /// </summary>
    public class NetworkConversionStatistics
    {
        public int TotalDevicesProcessed { get; set; }
        public int TotalConnectionsProcessed { get; set; }
        public int DevicesConverted { get; set; }
        public int ConnectionsConverted { get; set; }
        public int DevicesFailed { get; set; }
        public int ConnectionsFailed { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public TimeSpan ConversionDuration { get; set; }

        public double DeviceSuccessRate => TotalDevicesProcessed > 0 
            ? (double)DevicesConverted / TotalDevicesProcessed * 100 
            : 0;

        public double ConnectionSuccessRate => TotalConnectionsProcessed > 0 
            ? (double)ConnectionsConverted / TotalConnectionsProcessed * 100 
            : 0;

        public double OverallSuccessRate => (TotalDevicesProcessed + TotalConnectionsProcessed) > 0 
            ? (double)(DevicesConverted + ConnectionsConverted) / (TotalDevicesProcessed + TotalConnectionsProcessed) * 100 
            : 0;
    }
} 
