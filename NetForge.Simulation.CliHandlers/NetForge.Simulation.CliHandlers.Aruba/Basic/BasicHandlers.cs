using System.Text;
using System.Net;
using NetForge.Simulation.Common;

namespace NetForge.Simulation.CliHandlers.Aruba.Basic
{
    public static class BasicHandlers
    {
        /// <summary>
        /// Aruba ping command handler with interface counter increments
        /// </summary>
        public class ArubaPingHandler : VendorAgnosticCliHandler
        {
            public ArubaPingHandler() : base("ping", "Send ICMP echo requests") { }

            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "ping", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: ping <destination>");

                var destination = args[1];
                
                // Validate IP address
                if (!IsValidIpAddress(destination))
                {
                    return Error(CliErrorType.InvalidParameter, $"Invalid IP address: {destination}");
                }
                
                // Parse ping options for Aruba
                var pingCount = 5; // Default Aruba ping count
                var packetSize = 32; // Default Aruba ping packet size
                
                // Look for count option
                for (int i = 2; i < args.Length - 1; i++)
                {
                    if (args[i].Equals("count", StringComparison.OrdinalIgnoreCase) && int.TryParse(args[i + 1], out int count))
                    {
                        pingCount = count;
                        break;
                    }
                }
                
                // Update interface counters before executing ping
                UpdatePingCounters(context, destination, pingCount, packetSize);
                
                // Simulate Aruba ping output
                var output = new StringBuilder();
                output.AppendLine($"PING {destination} from {GetSourceInterface(context)}: 32(60) bytes of data.");
                
                for (int i = 1; i <= pingCount; i++)
                {
                    var time = Random.Shared.NextDouble() * 10; // Random response time
                    output.AppendLine($"64 bytes from {destination}: icmp_seq={i} ttl=64 time={time:F1} ms");
                }
                
                output.AppendLine();
                output.AppendLine($"--- {destination} ping statistics ---");
                output.AppendLine($"{pingCount} packets transmitted, {pingCount} received, 0% packet loss");
                output.AppendLine($"round-trip min/avg/max = 0.5/5.2/9.8 ms");

                return Success(output.ToString());
            }
            
            private bool IsValidIpAddress(string ip)
            {
                return IPAddress.TryParse(ip, out _);
            }
            
            private string GetSourceInterface(CliContext context)
            {
                var device = context.Device as NetworkDevice;
                var interfaces = device?.GetAllInterfaces();
                var activeInterface = interfaces?.Values
                    .FirstOrDefault(i => !string.IsNullOrEmpty(i.IpAddress) && i.IsUp && !i.IsShutdown);
                return activeInterface?.IpAddress ?? "unknown";
            }
            
            /// <summary>
            /// Update interface counters for ping simulation
            /// </summary>
            private void UpdatePingCounters(CliContext context, string destination, int pingCount, int packetSize)
            {
                try
                {
                    var device = context.Device as NetworkDevice;
                    if (device == null) return;
                    
                    // Find the source interface for ping
                    var sourceInterface = GetPingSourceInterface(device);
                    if (sourceInterface == null) return;
                    
                    // Each ping involves TX request + RX reply
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
                    device.AddLogEntry($"Ping to {destination}: {pingCount} packets sent/received via {sourceInterface.Name}");
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

        /// <summary>
        /// Aruba hostname command handler
        /// </summary>
        public class ArubaHostnameHandler : VendorAgnosticCliHandler
        {
            public ArubaHostnameHandler() : base("hostname", "Set system hostname") { }

            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "hostname", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "config"))
                    return Error(CliErrorType.InvalidMode, "Command only available in configuration mode");

                var args = context.CommandParts;
                if (args.Length < 2)
                    return Error(CliErrorType.InvalidParameter, "Usage: hostname <name>");

                var newHostname = args[1];
                
                // Validate hostname
                if (string.IsNullOrWhiteSpace(newHostname) || newHostname.Length > 63)
                    return Error(CliErrorType.InvalidParameter, "Invalid hostname");

                var vendorCaps = GetVendorCapabilities(context);
                if (vendorCaps?.SetHostname(newHostname) == true)
                {
                    return Success("");
                }

                return Error(CliErrorType.InvalidParameter, "Failed to set hostname");
            }
        }

        /// <summary>
        /// Aruba write command handler
        /// </summary>
        public class ArubaWriteHandler : VendorAgnosticCliHandler
        {
            public ArubaWriteHandler() : base("write", "Save configuration") { }

            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "write", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                var vendorCaps = GetVendorCapabilities(context);
                if (vendorCaps?.SaveConfiguration() == true)
                {
                    return Success("Configuration saved to flash");
                }

                return Error(CliErrorType.InvalidCommand, "Failed to save configuration");
            }
        }

        /// <summary>
        /// Aruba reload command handler  
        /// </summary>
        public class ArubaReloadHandler : VendorAgnosticCliHandler
        {
            public ArubaReloadHandler() : base("reload", "Restart the system") { }

            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "reload", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                if (!IsInMode(context, "privileged"))
                    return Error(CliErrorType.InvalidMode, "Command only available in privileged mode");

                return Success("This command will reboot the device\nContinue [y/n]?");
            }
        }

        /// <summary>
        /// Aruba enable command handler
        /// </summary>
        public class ArubaEnableHandler : VendorAgnosticCliHandler
        {
            public ArubaEnableHandler() : base("enable", "Enter privileged mode") { }

            public override bool CanHandle(CliContext context)
            {
                return context.CommandParts.Length >= 1 &&
                       string.Equals(context.CommandParts[0], "enable", StringComparison.OrdinalIgnoreCase);
            }

            protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
            {
                if (!IsVendor(context, "Aruba"))
                    return RequireVendor(context, "Aruba");

                try
                {
                    SetMode(context, "privileged");
                    return Success("");
                }
                catch
                {
                    return Error(CliErrorType.InvalidMode, "Failed to enter privileged mode");
                }
            }
        }
    }
} 
