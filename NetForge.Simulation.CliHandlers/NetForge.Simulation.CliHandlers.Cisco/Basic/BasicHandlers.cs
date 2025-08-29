using System.Text;
using System.Linq;
using System;
using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.CliHandlers.Cisco.Basic
{
    /// <summary>
    /// Cisco ping command handler
    /// </summary>
    public class PingCommandHandler() : VendorAgnosticCliHandler("ping", "Send ping packets")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            var targetIp = context.CommandParts[1];

            // Validate IP address format
            if (!IsValidIpAddress(targetIp))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid IP address: {targetIp}");
            }

            // Simulate ping and update interface counters
            var pingCount = 5;
            var packetSize = 100; // Default Cisco ping packet size
            UpdatePingCounters(context, pingCount, packetSize);

            // Simulate ping result
            var output = new StringBuilder();
            output.AppendLine($"Type escape sequence to abort.");
            output.AppendLine($"Sending {pingCount}, {packetSize}-byte ICMP Echos to {targetIp}, timeout is 2 seconds:");
            output.AppendLine("!!!!!");
            output.AppendLine($"Success rate is 100 percent ({pingCount}/{pingCount}), round-trip min/avg/max = 1/1/4 ms");
            output.AppendLine($"{pingCount} packets transmitted, {pingCount} packets received, 0% packet loss");

            return Success(output.ToString());
        }

        private bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }

        /// <summary>
        /// Update interface counters for ping simulation
        /// </summary>
        private void UpdatePingCounters(ICliContext context, int pingCount, int packetSize)
        {
            try
            {
                var device = context.Device;
                if (device == null) return;

                // Find the source interface for ping (use first available interface with IP)
                var sourceInterface = GetPingSourceInterface(device);
                if (sourceInterface == null) return;

                // Each ping involves:
                // - 1 outgoing ICMP echo request (TX)
                // - 1 incoming ICMP echo reply (RX)
                var totalPackets = pingCount * 2; // Request + Reply
                var icmpHeaderSize = 8; // ICMP header size
                var ipHeaderSize = 20; // IP header size
                var totalPacketSize = packetSize + icmpHeaderSize + ipHeaderSize;
                var totalBytes = totalPackets * totalPacketSize;

                // Update counters
                sourceInterface.TxPackets += pingCount; // Outgoing requests
                sourceInterface.RxPackets += pingCount; // Incoming replies
                sourceInterface.TxBytes += pingCount * totalPacketSize; // Outgoing bytes
                sourceInterface.RxBytes += pingCount * totalPacketSize; // Incoming bytes

                // Log the ping activity
                var networkDevice = device;
                networkDevice?.AddLogEntry($"Ping to {context.CommandParts[1]}: {pingCount} packets sent/received via {sourceInterface.Name}");
            }
            catch (Exception ex)
            {
                // Don't fail the ping command if counter update fails
                var networkDevice = context.Device;
                networkDevice?.AddLogEntry($"Warning: Failed to update ping counters - {ex.Message}");
            }
        }

        /// <summary>
        /// Find the interface to use as the source for ping
        /// </summary>
        private dynamic GetPingSourceInterface(INetworkDevice device)
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
    /// Cisco traceroute command handler
    /// </summary>
    public class TracerouteCommandHandler : VendorAgnosticCliHandler
    {
        public TracerouteCommandHandler() : base("traceroute", "Trace route to destination")
        {
            AddAlias("tracert");
            AddAlias("trace");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            var targetIp = context.CommandParts[1];

            // Validate IP address format
            if (!IsValidIpAddress(targetIp))
            {
                return Error(CliErrorType.InvalidParameter,
                    $"% Invalid IP address: {targetIp}");
            }

            // Simulate traceroute
            var output = new StringBuilder();
            output.AppendLine($"Type escape sequence to abort.");
            output.AppendLine($"Tracing the route to {targetIp}");
            output.AppendLine($"VRF info: (vrf in name/id, vrf out name/id)");
            output.AppendLine($"  1 192.168.1.1 4 msec 4 msec 4 msec");
            output.AppendLine($"  2 10.0.0.1 8 msec 8 msec 8 msec");
            output.AppendLine($"  3 {targetIp} 12 msec *  16 msec");

            return Success(output.ToString());
        }

        private bool IsValidIpAddress(string ip)
        {
            return System.Net.IPAddress.TryParse(ip, out _);
        }
    }

    /// <summary>
    /// Cisco write command handler
    /// </summary>
    public class WriteCommandHandler : VendorAgnosticCliHandler
    {
        public WriteCommandHandler() : base("write", "Write configuration to memory")
        {
            AddAlias("wr");
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires privileged mode");
            }

            // Handle sub-commands
            if (context.CommandParts.Length > 1)
            {
                var subCommand = context.CommandParts[1];

                return subCommand switch
                {
                    "memory" => HandleWriteMemory(context),
                    "terminal" => HandleWriteTerminal(context),
                    "erase" => HandleWriteErase(context),
                    _ => Error(CliErrorType.InvalidCommand,
                        GetVendorError(context, "invalid_command"))
                };
            }

            // Default to write memory
            return HandleWriteMemory(context);
        }

        private CliResult HandleWriteMemory(ICliContext context)
        {
            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                // Get current running configuration
                var runningConfig = GetRunningConfig(context);

                // Log configuration save
                device.AddLogEntry("Configuration saved to startup-config");

                return Success("Building configuration...\n[OK]");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error saving configuration: {ex.Message}");
            }
        }

        private CliResult HandleWriteTerminal(ICliContext context)
        {
            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                // Generate and display current running configuration
                var runningConfig = GetRunningConfig(context);
                return Success(runningConfig);
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error displaying configuration: {ex.Message}");
            }
        }

        private CliResult HandleWriteErase(ICliContext context)
        {
            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                // Log startup configuration erase
                device.AddLogEntry("Startup configuration erased");

                return Success("Erasing the nvram filesystem will remove all configuration files!\n[OK]");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error erasing configuration: {ex.Message}");
            }
        }


    }

    /// <summary>
    /// Cisco reload command handler
    /// </summary>
    public class ReloadCommandHandler() : VendorAgnosticCliHandler("reload", "Reload the device")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires privileged mode");
            }

            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                var output = new StringBuilder();
                output.AppendLine("Proceed with reload? [confirm]");
                output.AppendLine("System configuration has been modified. Save? [yes/no]:");
                output.AppendLine("Building configuration...");
                output.AppendLine("[OK]");
                output.AppendLine("Reload requested by console. Reload Reason: Reload Command.");

                // Log device reload
                device.AddLogEntry("Device reload initiated");

                // Reset interface states
                var interfaces = device.GetAllInterfaces();
                foreach (var iface in interfaces.Values)
                {
                    // Reset interface counters
                    iface.RxBytes = 0;
                    iface.TxBytes = 0;
                    iface.RxPackets = 0;
                    iface.TxPackets = 0;
                }

                device.AddLogEntry("Device reload completed");

                return Success(output.ToString());
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error during reload: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Cisco history command handler
    /// </summary>
    public class HistoryCommandHandler() : VendorAgnosticCliHandler("history", "Display command history")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            // Get command history
            var history = GetCommandHistory(context);

            var output = new StringBuilder();
            for (int i = 0; i < history.Count; i++)
            {
                output.AppendLine($"{i + 1,3}  {history[i]}");
            }

            return Success(output.ToString());
        }
    }

    /// <summary>
    /// Cisco copy command handler
    /// </summary>
    public class CopyCommandHandler() : VendorAgnosticCliHandler("copy", "Copy configuration or files")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires privileged mode");
            }

            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need source and destination");
            }

            var source = context.CommandParts[1];
            var destination = context.CommandParts[2];

            return (source, destination) switch
            {
                ("running-config", "startup-config") => HandleCopyRunningToStartup(context),
                ("startup-config", "running-config") => HandleCopyStartupToRunning(context),
                ("running-config", "tftp") => HandleCopyRunningToTftp(context),
                ("tftp", "running-config") => HandleCopyTftpToRunning(context),
                _ => Error(CliErrorType.InvalidCommand,
                    $"% Invalid copy operation: {source} to {destination}")
            };
        }

        private CliResult HandleCopyRunningToStartup(ICliContext context)
        {
            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                device.AddLogEntry("Running configuration copied to startup configuration");

                return Success("Destination filename [startup-config]? \nBuilding configuration...\n[OK]");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error copying running-config to startup-config: {ex.Message}");
            }
        }

        private CliResult HandleCopyStartupToRunning(ICliContext context)
        {
            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                device.AddLogEntry("Startup configuration loaded to running configuration");

                return Success("Destination filename [running-config]? \nLoading configuration...\n[OK]");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error copying startup-config to running-config: {ex.Message}");
            }
        }

        private CliResult HandleCopyRunningToTftp(ICliContext context)
        {
            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                var runningConfig = GetRunningConfig(context);
                var configSize = System.Text.Encoding.UTF8.GetByteCount(runningConfig);

                var output = new StringBuilder();
                output.AppendLine("Address or name of remote host []? 192.168.1.100");
                output.AppendLine($"Destination filename [{device.Name}-config]? ");
                output.AppendLine($"Writing {device.Name}-config !!! [{configSize} bytes]");
                output.AppendLine("[OK]");

                device.AddLogEntry($"Running configuration backed up to TFTP server (simulated) - {configSize} bytes");

                return Success(output.ToString());
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error copying running-config to TFTP: {ex.Message}");
            }
        }

        private CliResult HandleCopyTftpToRunning(ICliContext context)
        {
            try
            {
                var device = context.Device;
                if (device == null)
                {
                    return Error(CliErrorType.ExecutionError,
                        "% Device not available");
                }

                var output = new StringBuilder();
                output.AppendLine("Address or name of remote host []? 192.168.1.100");
                output.AppendLine($"Source filename []? {device.Name}-config");
                output.AppendLine("Destination filename [running-config]? ");
                output.AppendLine($"Accessing tftp://192.168.1.100/{device.Name}-config...");
                output.AppendLine($"Loading {device.Name}-config from 192.168.1.100 (via GigabitEthernet0/0): !");
                output.AppendLine("[OK - 1234 bytes]");

                device.AddLogEntry("Configuration loaded from TFTP server (simulated)");

                return Success(output.ToString());
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError,
                    $"% Error copying TFTP to running-config: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Cisco clear command handler
    /// </summary>
    public class ClearCommandHandler() : VendorAgnosticCliHandler("clear", "Clear counters and statistics")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            if (!IsVendor(context, "Cisco"))
            {
                return RequireVendor(context, "Cisco");
            }

            if (!IsInMode(context, "privileged"))
            {
                return Error(CliErrorType.InvalidMode,
                    "% This command requires privileged mode");
            }

            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand,
                    GetVendorError(context, "incomplete_command"));
            }

            var subCommand = context.CommandParts[1];

            return subCommand switch
            {
                "counters" => HandleClearCounters(context),
                "arp" => HandleClearArp(context),
                "ip" => HandleClearIp(context),
                "interface" => HandleClearInterface(context),
                "cdp" => HandleClearCdp(context),
                _ => Error(CliErrorType.InvalidCommand,
                    GetVendorError(context, "invalid_command"))
            };
        }

        private CliResult HandleClearCounters(ICliContext context)
        {
            // Simulate clearing interface counters
            return Success("Clear \"show interface\" counters on all interfaces [confirm]y");
        }

        private CliResult HandleClearArp(ICliContext context)
        {
            // Simulate clearing ARP table
            return Success("");
        }

        private CliResult HandleClearIp(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command");
            }

            var ipSubCommand = context.CommandParts[2];

            return ipSubCommand switch
            {
                "route" => HandleClearIpRoute(context),
                "bgp" => HandleClearIpBgp(context),
                "ospf" => HandleClearIpOspf(context),
                _ => Error(CliErrorType.InvalidCommand,
                    GetVendorError(context, "invalid_command"))
            };
        }

        private CliResult HandleClearIpRoute(ICliContext context)
        {
            // Simulate clearing IP routing table
            return Success("");
        }

        private CliResult HandleClearIpBgp(ICliContext context)
        {
            // Simulate clearing BGP sessions
            return Success("");
        }

        private CliResult HandleClearIpOspf(ICliContext context)
        {
            // Simulate clearing OSPF process
            return Success("");
        }

        private CliResult HandleClearInterface(ICliContext context)
        {
            if (context.CommandParts.Length < 3)
            {
                return Error(CliErrorType.IncompleteCommand,
                    "% Incomplete command - need interface name");
            }

            var interfaceName = string.Join(" ", context.CommandParts.Skip(2));

            // Simulate clearing interface statistics
            return Success("");
        }

        private CliResult HandleClearCdp(ICliContext context)
        {
            // Check for subcommands
            if (context.CommandParts.Length > 2)
            {
                var cdpSubCommand = context.CommandParts[2];

                return cdpSubCommand switch
                {
                    "table" => Success("CDP table cleared"),
                    "counters" => Success("CDP counters cleared"),
                    _ => Success("CDP information cleared") // For invalid subcommands, still clear CDP info
                };
            }

            // Default CDP clear
            return Success("CDP information cleared");
        }
    }
}
