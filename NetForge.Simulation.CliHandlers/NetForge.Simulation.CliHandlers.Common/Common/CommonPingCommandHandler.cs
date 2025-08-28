using System.Net;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.CommonHandlers
{
    /// <summary>
    /// Common ping command handler
    /// </summary>
    public class CommonPingCommandHandler : BaseCliHandler
    {
        public CommonPingCommandHandler() : base("ping", "Send ICMP echo requests")
        {
        }

        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, "% Incomplete command. Usage: ping <destination>\n");
            }

            var destination = context.CommandParts[1];

            // Validate IP address
            if (!IPAddress.TryParse(destination, out _))
            {
                return Error(CliErrorType.InvalidParameter, "% Invalid IP address\n");
            }

            // Update interface counters before executing ping
            var pingCount = 5; // Default ping count
            var packetSize = 64; // Default packet size
            UpdatePingCounters(context, destination, pingCount, packetSize);

            return Success(context.Device.ExecutePing(destination));
        }

        /// <summary>
        /// Update interface counters for ping simulation
        /// </summary>
        private void UpdatePingCounters(CliContext context, string destination, int pingCount, int packetSize)
        {
            try
            {
                var device = context.Device;
                if (device == null) return;

                // Find the source interface for ping (use first available interface with IP)
                var sourceInterface = GetPingSourceInterface(device as NetworkDevice);
                if (sourceInterface == null) return;

                // Each ping involves:
                // - 1 outgoing ICMP echo request (TX)
                // - 1 incoming ICMP echo reply (RX)
                var icmpHeaderSize = 8; // ICMP header size
                var ipHeaderSize = 20; // IP header size
                var ethernetHeaderSize = 14; // Ethernet header size
                var totalPacketSize = packetSize + icmpHeaderSize + ipHeaderSize + ethernetHeaderSize;

                // Update counters
                sourceInterface.TxPackets += pingCount; // Outgoing requests
                sourceInterface.RxPackets += pingCount; // Incoming replies
                sourceInterface.TxBytes += pingCount * totalPacketSize; // Outgoing bytes
                sourceInterface.RxBytes += pingCount * totalPacketSize; // Incoming bytes

                // Log the ping activity
                var networkDevice = device as NetworkDevice;
                networkDevice?.AddLogEntry($"Ping to {destination}: {pingCount} packets sent/received via {sourceInterface.Name}");
            }
            catch (Exception ex)
            {
                // Don't fail the ping command if counter update fails
                var networkDevice = context.Device as NetworkDevice;
                networkDevice?.AddLogEntry($"Warning: Failed to update ping counters - {ex.Message}");
            }
        }

        /// <summary>
        /// Find the interface to use as the source for ping
        /// </summary>
        private dynamic GetPingSourceInterface(NetworkDevice device)
        {
            var interfaces = device.GetAllInterfaces();

            // Prefer the first interface with an IP address that's up
            var activeInterface = interfaces.Values
                .FirstOrDefault(i => !string.IsNullOrEmpty(i.IpAddress) && i.IsUp && !i.IsShutdown);

            if (activeInterface != null)
                return activeInterface;

            // Fallback to any interface that's up
            return interfaces.Values.FirstOrDefault(i => i.IsUp && !i.IsShutdown);
        }
    }
}


