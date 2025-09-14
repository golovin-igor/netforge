using System.Net;
using System.Runtime.CompilerServices;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Services
{
    /// <summary>
    /// Concrete implementation of ping service with common business logic
    /// </summary>
    public class PingService : IPingService
    {
        private readonly Random _random = new();

        /// <summary>
        /// Execute a ping operation
        /// </summary>
        public PingResultData ExecutePing(INetworkDevice device, string destination, int pingCount = 5, int packetSize = 64)
        {
            var options = new PingOptions
            {
                Destination = destination,
                PingCount = pingCount,
                PacketSize = packetSize
            };

            return ExecutePingWithOptions(device, options);
        }

        /// <summary>
        /// Execute a ping operation with advanced options
        /// </summary>
        public PingResultData ExecutePingWithOptions(INetworkDevice device, PingOptions options)
        {
            var result = new PingResultData
            {
                Destination = options.Destination,
                PacketsSent = options.PingCount,
                PacketSize = options.PacketSize,
                TimeoutSeconds = options.TimeoutSeconds,
                StartTime = DateTime.UtcNow
            };

            // Validate destination
            if (!IsValidIpAddress(options.Destination))
            {
                // Try to resolve hostname
                var resolvedIp = ResolveHostname(options.Destination);
                if (resolvedIp == null)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Cannot resolve hostname: {options.Destination}";
                    result.EndTime = DateTime.UtcNow;
                    return result;
                }
                result.ResolvedAddress = resolvedIp;
                options.Destination = resolvedIp;
            }

            // Find source interface
            var sourceInterface = options.SourceInterface != null
                ? GetPingSourceInterface(device, options.SourceInterface)
                : GetPingSourceInterface(device);

            if (sourceInterface == null)
            {
                result.Success = false;
                result.ErrorMessage = "No suitable source interface found";
                result.EndTime = DateTime.UtcNow;
                return result;
            }

            result.SourceInterface = sourceInterface.Name;
            result.SourceIpAddress = sourceInterface.IpAddress;

            // Check reachability
            bool isReachable = IsDestinationReachable(device, options.Destination);

            // Update counters before simulating
            UpdatePingCounters(device, options.Destination, options.PingCount, options.PacketSize);

            // Simulate ping replies
            var successCount = 0;
            var rttValues = new List<int>();

            for (int i = 0; i < options.PingCount; i++)
            {
                var reply = new PingReplyData
                {
                    SequenceNumber = i,
                    FromAddress = options.Destination,
                    BytesReceived = options.PacketSize,
                    Ttl = options.Ttl ?? 64,
                    Timestamp = DateTime.UtcNow
                };

                if (isReachable && _random.Next(100) > 5) // 95% success rate when reachable
                {
                    reply.Success = true;
                    reply.RoundTripTime = CalculateRoundTripTime(device, options.Destination);
                    rttValues.Add(reply.RoundTripTime);
                    successCount++;
                }
                else
                {
                    reply.Success = false;
                    reply.ErrorType = isReachable ? "timeout" : "unreachable";
                    reply.RoundTripTime = 0;
                }

                result.Replies.Add(reply);

                // Add interval delay for continuous ping simulation
                if (i < options.PingCount - 1 && options.Interval > 0)
                {
                    Thread.Sleep(options.Interval);
                }
            }

            // Calculate statistics
            result.PacketsReceived = successCount;
            result.PacketsLost = options.PingCount - successCount;
            result.PacketLossPercentage = (result.PacketsLost * 100.0) / options.PingCount;
            result.Success = successCount > 0;

            if (rttValues.Count > 0)
            {
                result.MinRoundTripTime = rttValues.Min();
                result.MaxRoundTripTime = rttValues.Max();
                result.AvgRoundTripTime = (int)rttValues.Average();
                result.StandardDeviation = CalculateStandardDeviation(rttValues);
            }

            // Update ARP table if successful
            if (result.Success && device is INetworkContext networkContext)
            {
                UpdateArpTable(networkContext, options.Destination, GenerateMacAddress(options.Destination));
            }

            result.EndTime = DateTime.UtcNow;
            return result;
        }

        /// <summary>
        /// Execute a continuous ping operation
        /// </summary>
        public async IAsyncEnumerable<PingResultData> ExecuteContinuousPing(
            INetworkDevice device,
            string destination,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var options = new PingOptions
            {
                Destination = destination,
                PingCount = 1,
                Interval = 1000
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                yield return ExecutePingWithOptions(device, options);
                await Task.Delay(options.Interval, cancellationToken);
            }
        }

        /// <summary>
        /// Update interface counters for ping simulation
        /// </summary>
        public void UpdatePingCounters(INetworkDevice device, string destination, int pingCount, int packetSize)
        {
            try
            {
                var sourceInterface = GetPingSourceInterface(device);
                if (sourceInterface == null) return;

                // Calculate packet sizes with headers
                var icmpHeaderSize = 8;
                var ipHeaderSize = 20;
                var ethernetHeaderSize = 14;
                var totalPacketSize = packetSize + icmpHeaderSize + ipHeaderSize + ethernetHeaderSize;

                // Update TX/RX counters
                sourceInterface.TxPackets += pingCount;
                sourceInterface.RxPackets += pingCount;
                sourceInterface.TxBytes += pingCount * totalPacketSize;
                sourceInterface.RxBytes += pingCount * totalPacketSize;

                // Log activity
                if (device is IDeviceLogging logging)
                {
                    logging.AddLogEntry($"Ping to {destination}: {pingCount} packets sent/received via {sourceInterface.Name}");
                }
            }
            catch (Exception ex)
            {
                if (device is IDeviceLogging logging)
                {
                    logging.AddLogEntry($"Warning: Failed to update ping counters - {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Find the source interface for ping operations
        /// </summary>
        public dynamic GetPingSourceInterface(INetworkDevice device)
        {
            if (device is not IInterfaceManager interfaceManager)
                return null;

            var interfaces = interfaceManager.GetAllInterfaces();

            // Prefer the first interface with an IP address that's up
            var activeInterface = interfaces.Values
                .FirstOrDefault(i => !string.IsNullOrEmpty(i.IpAddress) && i.IsUp && !i.IsShutdown);

            if (activeInterface != null)
                return activeInterface;

            // Fallback to any interface that's up
            return interfaces.Values.FirstOrDefault(i => i.IsUp && !i.IsShutdown);
        }

        /// <summary>
        /// Find a specific source interface for ping operations
        /// </summary>
        public dynamic GetPingSourceInterface(INetworkDevice device, string interfaceName)
        {
            if (device is not IInterfaceManager interfaceManager)
                return null;

            var interfaces = interfaceManager.GetAllInterfaces();

            if (!interfaces.TryGetValue(interfaceName, out var sourceInterface))
                return null;

            // Verify interface is usable
            if (!sourceInterface.IsUp || sourceInterface.IsShutdown)
                return null;

            return sourceInterface;
        }

        /// <summary>
        /// Validate if the target IP address is valid
        /// </summary>
        public bool IsValidIpAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// Validate if the target hostname is valid
        /// </summary>
        public bool IsValidHostname(string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return false;

            // Basic hostname validation
            return hostname.Length <= 255 &&
                   hostname.Split('.').All(label =>
                       label.Length > 0 &&
                       label.Length <= 63 &&
                       label.All(c => char.IsLetterOrDigit(c) || c == '-') &&
                       !label.StartsWith('-') &&
                       !label.EndsWith('-'));
        }

        /// <summary>
        /// Resolve hostname to IP address
        /// </summary>
        public string? ResolveHostname(string hostname)
        {
            // In simulation, we'll use a simple mapping
            return hostname.ToLower() switch
            {
                "localhost" => "127.0.0.1",
                "router1" => "192.168.1.1",
                "router2" => "192.168.1.2",
                "switch1" => "192.168.1.10",
                "server1" => "192.168.1.100",
                _ => null
            };
        }

        /// <summary>
        /// Check if destination is reachable from device
        /// </summary>
        public bool IsDestinationReachable(INetworkDevice device, string destination)
        {
            // Check if destination is in the same network
            if (device is INetworkConnectivity connectivity)
            {
                var sourceInterface = GetPingSourceInterface(device);
                if (sourceInterface?.IpAddress != null)
                {
                    return IsInSameNetwork(sourceInterface.IpAddress, destination, "255.255.255.0");
                }
            }

            // Check routing table - simplified for now
            // In a real implementation, this would check the device's routing table
            // For simulation purposes, assume routes exist for common networks

            // Default: assume local network is reachable
            return destination.StartsWith("192.168.") ||
                   destination.StartsWith("10.") ||
                   destination.StartsWith("172.");
        }

        /// <summary>
        /// Calculate round-trip time simulation
        /// </summary>
        public int CalculateRoundTripTime(INetworkDevice device, string destination)
        {
            // Simulate RTT based on "distance"
            var baseRtt = 1;

            // Add latency based on destination
            if (destination.StartsWith("127."))
                return baseRtt; // Loopback is fastest

            if (IsInSameNetwork(GetPingSourceInterface(device)?.IpAddress ?? "192.168.1.1", destination, "255.255.255.0"))
                return baseRtt + _random.Next(0, 3); // Same network: 1-4ms

            // Different network or internet
            return baseRtt + _random.Next(5, 50); // 6-51ms
        }

        #region Helper Methods

        private bool IsInSameNetwork(string ip1, string ip2, string mask)
        {
            try
            {
                var addr1 = IPAddress.Parse(ip1);
                var addr2 = IPAddress.Parse(ip2);
                var subnet = IPAddress.Parse(mask);

                var bytes1 = addr1.GetAddressBytes();
                var bytes2 = addr2.GetAddressBytes();
                var maskBytes = subnet.GetAddressBytes();

                for (int i = 0; i < bytes1.Length; i++)
                {
                    if ((bytes1[i] & maskBytes[i]) != (bytes2[i] & maskBytes[i]))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private double CalculateStandardDeviation(List<int> values)
        {
            if (values.Count <= 1)
                return 0;

            double avg = values.Average();
            double sum = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sum / (values.Count - 1));
        }

        private void UpdateArpTable(INetworkContext context, string ipAddress, string macAddress)
        {
            // Update ARP table with discovered MAC address
            var arpEntry = new { IpAddress = ipAddress, MacAddress = macAddress, Age = 0 };

            // This would update the actual ARP table in the device
            // For now, just log it
            if (context is IDeviceLogging logging)
            {
                logging.AddLogEntry($"ARP: Added {ipAddress} -> {macAddress}");
            }
        }

        private string GenerateMacAddress(string ipAddress)
        {
            // Generate a consistent MAC address based on IP
            var hash = ipAddress.GetHashCode();
            var bytes = BitConverter.GetBytes(hash);
            return $"00:1A:{bytes[0]:X2}:{bytes[1]:X2}:{bytes[2]:X2}:{bytes[3]:X2}";
        }

        #endregion
    }
}