using System.Globalization;
using System.Text;
using NetForge.Interfaces.Cli;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Common.Protocols;
using NetForge.Simulation.Common.Security;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.DataTypes.Cli;
using NetForge.Simulation.Protocols.Common.Events;

namespace NetForge.Simulation.Topology.Devices
{
    /// <summary>
    /// Base class for all network device implementations
    /// </summary>
    public abstract class NetworkDevice : ICommandProcessor, INetworkDevice
    {
        public string Name { get; protected set; }
        public string Vendor { get; protected init; }

        public INetwork? ParentNetwork { get; set; }

        /// <summary>
        /// Contains the unique identifier for this device in the network, that we import from the generated network.
        /// </summary>
        public string DeviceId { get; set; }

        public bool IsNvramLoaded { get; set; }

        // Configuration and state
        protected readonly DeviceConfiguration RunningConfig = new();
        protected readonly Dictionary<string, IInterfaceConfig> Interfaces = new();
        protected readonly Dictionary<int, VlanConfig> Vlans = new();
        protected readonly List<Route> RoutingTable = [];
        protected readonly Dictionary<int, AccessList> AccessLists = new();
        protected readonly Dictionary<int, PortChannel> PortChannels = new();
        protected readonly Dictionary<string, string> SystemSettings = new();
        protected readonly List<string> LogEntries = [];

        // Protocol configurations
        protected OspfConfig OspfConfig;
        protected BgpConfig BgpConfig;
        protected RipConfig RipConfig;
        protected EigrpConfig EigrpConfig;
        protected StpConfig StpConfig = new();
        protected IgrpConfig IgrpConfig;
        protected VrrpConfig VrrpConfig;
        protected HsrpConfig HsrpConfig;
        protected CdpConfig CdpConfig;
        protected LldpConfig LldpConfig;
        protected object TelnetConfig;
        protected object SshConfig;
        protected object SnmpConfig;
        protected object HttpConfig;
        protected readonly Dictionary<string, RoutingPolicy> RoutingPolicies = new();
        protected readonly Dictionary<string, PrefixList> PrefixLists = new();
        protected readonly Dictionary<string, BgpCommunity> BgpCommunities = new();
        protected readonly Dictionary<string, RouteMap> RouteMaps = new();
        protected readonly Dictionary<string, AsPathGroup> AsPathGroups = new();
        protected readonly Dictionary<string, string> ArpTable = new();

        // Registered network protocols
        private readonly List<IDeviceProtocol> _protocols = [];

        // Enhanced protocol service (lazily initialized)
        private IProtocolService _protocolService;

        // CLI state
        protected DeviceMode CurrentMode = DeviceMode.User;
        protected string CurrentInterface = "";
        protected string Hostname;

        protected readonly ICliHandlerManager CommandManager;
        protected readonly CommandHistory CommandHistory;

        // Event for log entry additions, useful for testing and real-time log monitoring
        public event Action<string> LogEntryAdded;

        protected NetworkDevice(string name)
        {
            Name = name;
            Hostname = name;
            InitializeDefaultInterfaces();

            // Initialize command handler manager
            //initialize commandManager through IoC or factory pattern
            //CommandManager = new CliHandlerManager(this);

            RegisterCommonHandlers();
            RegisterDeviceSpecificHandlers();

            // Initialize command history
            CommandHistory = new CommandHistory(1000); // Max 1000 commands
        }

        /// <summary>
        /// Auto-register protocols for this device vendor using the new plugin-based discovery service
        /// This method should be called after the Vendor property is set
        /// NOTE: This is a placeholder - actual implementation will be in device-specific projects
        /// </summary>
        protected virtual void AutoRegisterProtocols()
        {
            AddLogEntry("AutoRegisterProtocols called - implementation should be provided by device-specific project");
        }

        /// <summary>
        /// Get the enhanced protocol service for this device
        /// </summary>
        /// <returns>Protocol service instance</returns>
        public IProtocolService GetProtocolService()
        {
            if (_protocolService == null)
            {
                // Try to create enhanced protocol service if available
                try
                {
                    // More robust approach to loading the enhanced service
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var protocolsCommonAssembly = assemblies.FirstOrDefault(a =>
                        a.GetName().Name == "NetForge.Simulation.Core.Protocols.Common");

                    if (protocolsCommonAssembly != null)
                    {
                        var enhancedServiceType = protocolsCommonAssembly.GetType(
                            "NetForge.Simulation.Core.Protocols.Common.Services.NetworkDeviceProtocolService");

                        if (enhancedServiceType != null)
                        {
                            _protocolService = (IProtocolService)Activator.CreateInstance(enhancedServiceType, this);
                        }
                    }
                }
                catch
                {
                    // Fall back to basic protocol service if enhanced version not available
                }

            }

            return _protocolService;
        }

        /// <summary>
        /// Get the device name for protocol service compatibility
        /// </summary>
        public string DeviceName => Name;

        /// <summary>
        /// Get the device type for protocol service compatibility
        /// </summary>
        public string DeviceType => Vendor;


        /// <summary>
        /// Process a command and return the output
        /// </summary>
    // Synchronous ProcessCommand removed. Use ProcessCommandAsync instead.

        /// <summary>
        /// Asynchronously process a command and return the output
        /// </summary>
        public virtual async Task<string> ProcessCommandAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return GetPrompt();

            var result = "";
            var success = false;
            var originalCommand = command;

            try
            {
                // Check if this is a history recall command
                if (command.StartsWith("!"))
                {
                    var recalledCommand = CommandHistory.ProcessRecallCommand(command);
                    if (recalledCommand != null)
                    {
                        result = $"Recalled: {recalledCommand}\n";
                        command = recalledCommand;
                    }
                    else
                    {
                        result = "% Invalid history reference\n";
                        success = false;
                        result += GetPrompt();
                        return result;
                    }
                }

                // Process the command
                if (CommandManager != null)
                {
                    var cmdResult = await CommandManager.ProcessCommandAsync(command);
                    success = cmdResult.Success;

                    if (cmdResult.Success)
                    {
                        result += cmdResult.Output;
                    }
                    else
                    {
                        result += cmdResult.Output;
                        if (cmdResult.Suggestions?.Length > 0)
                        {
                            result += $"\nDid you mean one of these?: {string.Join(", ", cmdResult.Suggestions)}";
                        }
                    }
                }
                else
                {
                    // If no command manager, return error
                    result += "% Invalid input detected at '^' marker.\n";
                    success = false;
                }
            }
            catch (Exception ex)
            {
                result = $"% Error processing command: {ex.Message}\n";
                success = false;
            }
            finally
            {
                // Add command to history (but not if it was a recall command)
                if (!originalCommand.StartsWith("!"))
                {
                    CommandHistory.AddCommand(command, CurrentMode.ToModeString(), success);
                }
            }

            // Always append prompt to output
            if (!result.EndsWith(GetPrompt()))
            {
                // Ensure there's a line break before the prompt if the result doesn't end with one
                if (!string.IsNullOrEmpty(result) && !result.EndsWith("\n") && !result.EndsWith("\r\n"))
                {
                    result += "\n";
                }
                result += GetPrompt();
            }

            return result;
        }

        /// <summary>
        /// Get the command history for this device
        /// </summary>
        public CommandHistory GetCommandHistory()
        {
            return CommandHistory;
        }

        /// <summary>
        /// Initialize default interfaces for the device
        /// </summary>
        protected abstract void InitializeDefaultInterfaces();

        /// <summary>
        /// Get the current CLI prompt
        /// </summary>
        public abstract string GetPrompt();

        /// <summary>
        /// Implementation of ICommandProcessor.GetCurrentPrompt
        /// </summary>
        public string GetCurrentPrompt()
        {
            return GetPrompt();
        }

        /// <summary>
        /// Public accessors for command handlers
        /// </summary>
        public Dictionary<string, IInterfaceConfig> GetAllInterfaces() => Interfaces;

        public Dictionary<int, VlanConfig> GetAllVlans() => Vlans;
        public List<Route> GetRoutingTable() => RoutingTable;
        public Dictionary<int, AccessList> GetAccessLists() => AccessLists;
        public Dictionary<int, PortChannel> GetPortChannels() => PortChannels;
        public Dictionary<string, string> GetSystemSettings() => SystemSettings;
        public List<string> GetLogEntries() => LogEntries;
        public Dictionary<string, string> GetArpTable() => ArpTable;
        public string GetArpTableOutput() => BuildArpTableOutput();

        public OspfConfig? GetOspfConfiguration() => OspfConfig;
        public BgpConfig? GetBgpConfiguration() => BgpConfig;
        public RipConfig? GetRipConfiguration() => RipConfig;
        public EigrpConfig? GetEigrpConfiguration() => EigrpConfig;
        public StpConfig GetStpConfiguration() => StpConfig;

        public IgrpConfig? GetIgrpConfiguration() => IgrpConfig;
        public VrrpConfig? GetVrrpConfiguration() => VrrpConfig;
        public HsrpConfig? GetHsrpConfiguration() => HsrpConfig;
        public CdpConfig? GetCdpConfiguration() => CdpConfig;
        public LldpConfig? GetLldpConfiguration() => LldpConfig;

        public void SetOspfConfiguration(OspfConfig config)
        {
            bool wasNull = OspfConfig == null;
            OspfConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.OSPF,
                wasNull ? "OSPF configuration initialized" : "OSPF configuration updated"));
        }

        public void SetBgpConfiguration(BgpConfig config)
        {
            bool wasNull = BgpConfig == null;
            BgpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.BGP,
                wasNull ? "BGP configuration initialized" : "BGP configuration updated"));
        }

        public void SetRipConfiguration(RipConfig config)
        {
            bool wasNull = RipConfig == null;
            RipConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.RIP,
                wasNull ? "RIP configuration initialized" : "RIP configuration updated"));
        }

        public void SetEigrpConfiguration(EigrpConfig config)
        {
            bool wasNull = EigrpConfig == null;
            EigrpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.EIGRP,
                wasNull ? "EIGRP configuration initialized" : "EIGRP configuration updated"));
        }

        public void SetStpConfiguration(StpConfig config)
        {
            // StpConfig is rarely null due to default initialization, but check for substantive changes if needed.
            StpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.STP, "STP configuration updated"));
        }



        public void SetIgrpConfiguration(IgrpConfig config)
        {
            bool wasNull = IgrpConfig == null;
            IgrpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.IGRP,
                wasNull ? "IGRP configuration initialized" : "IGRP configuration updated"));
        }

        public void SetVrrpConfiguration(VrrpConfig config)
        {
            bool wasNull = VrrpConfig == null;
            VrrpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.VRRP,
                wasNull ? "VRRP configuration initialized" : "VRRP configuration updated"));
        }

        public void SetHsrpConfiguration(HsrpConfig config)
        {
            bool wasNull = HsrpConfig == null;
            HsrpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.HSRP,
                wasNull ? "HSRP configuration initialized" : "HSRP configuration updated"));
        }

        public void SetCdpConfiguration(CdpConfig config)
        {
            bool wasNull = CdpConfig == null;
            CdpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.CDP,
                wasNull ? "CDP configuration initialized" : "CDP configuration updated"));
        }

        public void SetLldpConfiguration(LldpConfig config)
        {
            bool wasNull = LldpConfig == null;
            LldpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.LLDP,
                wasNull ? "LLDP configuration initialized" : "LLDP configuration updated"));
        }

        public object GetTelnetConfiguration() => TelnetConfig;
        public void SetTelnetConfiguration(object config)
        {
            bool wasNull = TelnetConfig == null;
            TelnetConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.TELNET,
                wasNull ? "Telnet configuration initialized" : "Telnet configuration updated"));
        }

        public object GetSshConfiguration() => SshConfig;
        public void SetSshConfiguration(object config)
        {
            bool wasNull = SshConfig == null;
            SshConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.SSH,
                wasNull ? "SSH configuration initialized" : "SSH configuration updated"));
        }

        public object GetSnmpConfiguration() => SnmpConfig;
        public void SetSnmpConfiguration(object config)
        {
            bool wasNull = SnmpConfig == null;
            SnmpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.SNMP,
                wasNull ? "SNMP configuration initialized" : "SNMP configuration updated"));
        }

        public object GetHttpConfiguration() => HttpConfig;
        public void SetHttpConfiguration(object config)
        {
            bool wasNull = HttpConfig == null;
            HttpConfig = config;
            ParentNetwork?.EventBus?.PublishAsync(new ProtocolConfigChangedEventArgs(Name, NetworkProtocolType.HTTP,
                wasNull ? "HTTP configuration initialized" : "HTTP configuration updated"));
        }

        public string GetHostname() => Hostname;
        public void SetHostname(string name) => Hostname = name;

        public string GetCurrentMode() => CurrentMode.ToModeString();
        public void SetCurrentMode(string mode) => CurrentMode = DeviceModeExtensions.FromModeString(mode);

        /// <summary>
        /// Get the current device mode as a strongly typed enum
        /// </summary>
        public DeviceMode GetCurrentModeEnum() => CurrentMode;

        /// <summary>
        /// Set the current device mode using strongly typed enum
        /// </summary>
        public void SetCurrentModeEnum(DeviceMode mode) => CurrentMode = mode;

        public string GetCurrentInterface() => CurrentInterface;
        public void SetCurrentInterface(string iface) => CurrentInterface = iface;

        /// <summary>
        /// Set the device mode (e.g., configure, interface, etc.)
        /// </summary>
        public virtual void SetMode(string mode) => CurrentMode = DeviceModeExtensions.FromModeString(mode);

        /// <summary>
        /// Set the device mode using strongly typed enum
        /// </summary>
        public virtual void SetModeEnum(DeviceMode mode) => CurrentMode = mode;

        /// <summary>
        /// Public method wrappers for protected methods
        /// </summary>
        public string GetNetworkAddress(string ip, string mask) => GetNetwork(ip, mask);

        public void ForceUpdateConnectedRoutes() => UpdateConnectedRoutes();
        public string ExecutePing(string destination) => SimulatePing(destination);
        public bool CheckIpInNetwork(string ip, string network, string mask) => IsIpInNetwork(ip, network, mask);

        /// <summary>
        /// Add routes to routing table
        /// </summary>
        public void AddRoute(Route route)
        {
            RoutingTable.Add(route);
        }

        /// <summary>
        /// Remove routes from routing table
        /// </summary>
        public void RemoveRoute(Route route)
        {
            RoutingTable.Remove(route);
        }

        /// <summary>
        /// Clear all routes of a specific protocol
        /// </summary>
        public void ClearRoutesByProtocol(string protocol)
        {
            RoutingTable.RemoveAll(r => r.Protocol == protocol);
        }

        /// <summary>
        /// Update connected routes based on physical connectivity
        /// Enhanced version that respects physical connections
        /// </summary>
        protected void UpdateConnectedRoutes()
        {
            // Remove old connected routes
            RoutingTable.RemoveAll(r => r.Protocol == "Connected");

            // Add new connected routes only for physically connected interfaces
            foreach (var iface in Interfaces.Values)
            {
                if (!string.IsNullOrEmpty(iface.IpAddress) &&
                    !iface.IsShutdown &&
                    IsInterfacePhysicallyConnected(iface.Name))
                {
                    var network = GetNetwork(iface.IpAddress, iface.SubnetMask);
                    var route = new Route(network, iface.SubnetMask, "0.0.0.0", iface.Name, "Connected");
                    route.Metric = 0;
                    RoutingTable.Add(route);
                }
            }
        }

        /// <summary>
        /// Calculate network address from IP and mask
        /// </summary>
        protected string GetNetwork(string ip, string mask)
        {
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(mask))
                return "0.0.0.0";

            var ipParts = ip.Split('.').Select(int.Parse).ToArray();
            var maskParts = mask.Split('.').Select(int.Parse).ToArray();
            var networkParts = new int[4];

            for (int i = 0; i < 4; i++)
            {
                networkParts[i] = ipParts[i] & maskParts[i];
            }

            return string.Join(".", networkParts);
        }

        /// <summary>
        /// Convert subnet mask to CIDR notation
        /// </summary>
        public int MaskToCidr(string mask)
        {
            if (string.IsNullOrEmpty(mask)) return 0;
            var parts = mask.Split('.').Select(int.Parse).ToArray();
            int cidr = 0;
            foreach (var part in parts)
            {
                for (int i = 7; i >= 0; i--)
                {
                    if ((part & (1 << i)) != 0) cidr++;
                }
            }

            return cidr;
        }

        /// <summary>
        /// Convert CIDR notation to subnet mask
        /// </summary>
        protected string CidrToMask(int cidr)
        {
            uint mask = 0xFFFFFFFF << (32 - cidr);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        /// <summary>
        /// Check if an IP address is in a network
        /// </summary>
        protected bool IsIpInNetwork(string ip, string network, string mask)
        {
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(network) || string.IsNullOrEmpty(mask))
                return false;

            var ipParts = ip.Split('.').Select(int.Parse).ToArray();
            var networkParts = network.Split('.').Select(int.Parse).ToArray();
            var maskParts = mask.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < 4; i++)
            {
                if ((ipParts[i] & maskParts[i]) != (networkParts[i] & maskParts[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get interface by name
        /// </summary>
        public virtual IInterfaceConfig? GetInterface(string name)
        {
            return Interfaces.GetValueOrDefault(name);
        }

        /// <summary>
        /// Get VLAN by ID
        /// </summary>
        public VlanConfig? GetVlan(int id)
        {
            return Vlans.GetValueOrDefault(id);
        }

        /// <summary>
        /// Get access list by number
        /// </summary>
        public AccessList? GetAccessList(int number)
        {
            return AccessLists.GetValueOrDefault(number);
        }

        /// <summary>
        /// Get port channel by number
        /// </summary>
        public PortChannel? GetPortChannel(int number)
        {
            return PortChannels.GetValueOrDefault(number) ;
        }

        /// <summary>
        /// Get system setting by name
        /// </summary>
        public string? GetSystemSetting(string name)
        {
            return SystemSettings.GetValueOrDefault(name);
        }

        /// <summary>
        /// Set system setting
        /// </summary>
        public void SetSystemSetting(string name, string value)
        {
            SystemSettings[name] = value;
        }

        /// <summary>
        /// Add log entry
        /// </summary>
        public void AddLogEntry(string entry)
        {
            string fullEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {entry}";
            LogEntries.Add(fullEntry);
            LogEntryAdded?.Invoke(fullEntry); // Raise the event
        }

        /// <summary>
        /// Clear log entries
        /// </summary>
        public void ClearLog()
        {
            LogEntries.Clear();
        }

        /// <summary>
        /// Register common command handlers
        /// </summary>
        protected virtual void RegisterCommonHandlers()
        {
        }

        /// <summary>
        /// Register device-specific command handlers
        /// </summary>
        protected abstract void RegisterDeviceSpecificHandlers();

        /// <summary>
        /// Enhanced ping simulation that considers physical connectivity
        /// </summary>
        protected virtual string SimulatePing(string destination)
        {
            if (ParentNetwork == null)
                return "% Network not initialized";

            if (ParentNetwork.FindDeviceByIp(destination) is not NetworkDevice destDevice)
                return "% Destination host unreachable";

            // Find outgoing interface for the destination
            string outgoingInterface = FindOutgoingInterface(destination);
            if (string.IsNullOrEmpty(outgoingInterface))
                return "% No route to destination";

            // Check if the outgoing interface is shutdown
            var outgoingInterfaceConfig = GetInterface(outgoingInterface);
            if (outgoingInterfaceConfig == null || outgoingInterfaceConfig.IsShutdown)
                return BuildPingFailureOutput(destination, "No response");

            // Check if the outgoing interface has physical connectivity
            if (!IsInterfacePhysicallyConnected(outgoingInterface))
                return "% Interface physically disconnected";

            // Check for ACL blocking ICMP traffic on destination device
            var destInterface = destDevice.Interfaces.Values.FirstOrDefault(i => i.IpAddress == destination);
            if (destInterface != null && IsDestinationIcmpBlocked(destDevice, destInterface.Name, destination))
                return BuildPingFailureOutput(destination, "No response");

            // Check destination interface status
            if (destInterface == null || destInterface.IsShutdown)
                return BuildPingFailureOutput(destination, "No response");

            // Get physical connection metrics for realistic ping simulation
            var metrics = GetPhysicalConnectionMetrics(outgoingInterface);
            if (metrics == null)
                return "% Physical connection unavailable";

            var sb = new StringBuilder();
            sb.Append($"PING {destination}").AppendLine();

            // Simulate pings with realistic latency and potential packet loss
            int successCount = 0;

            for (int i = 0; i < 5; i++)
            {
                var result = TestPhysicalConnectivity(outgoingInterface, 64); // Standard ping packet size
                if (result.Success)
                {
                    successCount++;
                    sb.Append($"64 bytes from {destination}: icmp_seq={i + 1} ttl=64 time={result.ActualLatency}.{i} ms").AppendLine();

                    // Increment interface counters for successful pings
                    if (outgoingInterfaceConfig != null)
                    {
                        outgoingInterfaceConfig.TxPackets++;
                        outgoingInterfaceConfig.TxBytes += 64; // Standard ping packet size
                    }

                    // Increment RX counters on destination device
                    if (destInterface != null)
                    {
                        destInterface.RxPackets++;
                        destInterface.RxBytes += 64;
                    }
                }
                else
                {
                    sb.Append($"Request timeout for icmp_seq {i + 1}").AppendLine();
                }
            }

            sb.AppendLine();
            sb.Append("--- ping statistics ---").AppendLine();
            double packetLoss = ((5.0 - successCount) / 5.0) * 100;
            sb.AppendFormat(CultureInfo.InvariantCulture, "5 packets transmitted, {0} received, {1:F1}% packet loss", successCount, packetLoss).AppendLine();

            if (successCount > 0)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "round-trip min/avg/max = {0}.0/{1}.0/{2}.0 ms", metrics.Latency, metrics.Latency + 1, metrics.Latency + 2).AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Build ping failure output message
        /// </summary>
        protected virtual string BuildPingFailureOutput(string destination, string reason)
        {
            var sb = new StringBuilder();
            sb.Append($"PING {destination}").AppendLine();
            for (int i = 0; i < 5; i++)
            {
                sb.Append($"{reason} for icmp_seq {i + 1}").AppendLine();
            }
            sb.AppendLine();
            sb.Append("--- ping statistics ---").AppendLine();
            sb.Append("5 packets transmitted, 0 received, 100.0% packet loss").AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Check if ICMP traffic is blocked by ACL
        /// </summary>
        protected virtual bool IsIcmpBlocked(string interfaceName, string destination)
        {
            var iface = GetInterface(interfaceName);
            if (iface?.IncomingAccessList != null)
            {
                var acl = GetAccessList(iface.IncomingAccessList.Value);
                if (acl != null)
                {
                    foreach (var entry in acl.Entries)
                    {
                        if (entry.Protocol.ToLowerInvariant() == "icmp" && entry.Action == "deny")
                        {
                            // Check if this ACL entry applies to the destination
                            if (entry.DestAddress == "any" ||
                                entry.DestAddress == destination ||
                                (entry.DestAddress.StartsWith("host ") && entry.DestAddress.Substring(5) == destination))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Add a static route
        /// </summary>
        public virtual void AddStaticRoute(string network, string mask, string nextHop, int metric)
        {
            var route = new Route(network, mask, nextHop, "", "Static");
            route.Metric = metric;
            RoutingTable.Add(route);
        }

        /// <summary>
        /// Remove a static route
        /// </summary>
        public virtual void RemoveStaticRoute(string network, string mask)
        {
            var route = RoutingTable.FirstOrDefault(r =>
                r.Protocol == "Static" &&
                r.Network == network &&
                r.Mask == mask);

            if (route != null)
                RoutingTable.Remove(route);
        }

        /// <summary>
        /// Update ARP table
        /// </summary>
        protected virtual void UpdateArpTable(string destIp)
        {
            if (ArpTable.ContainsKey(destIp)) return;
            if (ParentNetwork?.FindDeviceByIp(destIp) is not NetworkDevice destDevice) return;
            // Find dest interface
            var destIface = destDevice.Interfaces.Values.FirstOrDefault(i => i.IpAddress == destIp);
            if (destIface == null) return;
            // For simplicity, assume ARP broadcast reaches
            ArpTable[destIp] = destIface.MacAddress;
            // Update remote side with our interface mac
            var outIfaceName = FindOutgoingInterface(destIp);
            if (!string.IsNullOrEmpty(outIfaceName))
            {
                var localMac = Interfaces[outIfaceName].MacAddress;
                destDevice.ArpTable[Interfaces[outIfaceName].IpAddress] = localMac;
            }
        }

        protected string BuildArpTableOutput()
        {
            var sb = new StringBuilder();
            sb.Append("Protocol  Address         Age (min)  Hardware Addr   Interface").AppendLine();
            foreach (var entry in ArpTable)
            {
                sb.Append($"Internet  {entry.Key,-15}  -         {entry.Value,-13}  -").AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Registers a network protocol implementation with the device.
        /// </summary>
        /// <param name="protocol">The protocol to register.</param>
        public void RegisterProtocol(IDeviceProtocol protocol)
        {
            if (protocol != null && !_protocols.Any(p => p.Type == protocol.Type))
            {
                _protocols.Add(protocol);
                protocol.Initialize(this); // Initialize protocol with device context
                if (ParentNetwork?.EventBus != null)
                {
                    protocol.SubscribeToEvents(ParentNetwork.EventBus, this);
                    AddLogEntry($"Protocol {protocol.Type} registered, initialized, and subscribed to events.");
                }
                else
                {
                    // This case might happen if a device is created but not yet added to a network.
                    // Protocols can subscribe later if the EventBus becomes available, or handle this gracefully.
                    AddLogEntry($"Protocol {protocol.Type} registered and initialized. EventBus not available for subscription yet.");
                }
            }
            else if (protocol != null)
            {
                AddLogEntry($"Protocol {protocol.Type} is already registered.");
            }
        }

        /// <summary>
        /// Updates the state of all registered network protocols.
        /// </summary>
        public virtual async Task UpdateAllProtocolStates()
        {
            AddLogEntry("Updating all protocol states...");
            foreach (var protocol in _protocols)
            {
                try
                {
                    await protocol.UpdateState(this);
                    AddLogEntry($"Protocol {protocol.Type} state updated.");
                }
                catch (Exception ex)
                {
                    AddLogEntry($"Error updating protocol {protocol.Type}: {ex.Message}");
                    // Optionally, handle or log the exception more formally
                }
            }
        }

        /// <summary>
        /// Find outgoing interface for a destination IP
        /// </summary>
        protected string FindOutgoingInterface(string destIp)
        {
            // Simplified: just find the first route that matches
            // In a real scenario, this would involve a more complex lookup
            // considering metrics, longest prefix match, etc.
            foreach (var route in RoutingTable.OrderByDescending(r => MaskToCidr(r.Mask))) // Prioritize more specific routes
            {
                if (IsIpInNetwork(destIp, route.Network, route.Mask))
                {
                    if (!string.IsNullOrEmpty(route.Interface))
                        return route.Interface; // Direct route via interface

                    // If next hop is specified, recurse (or find interface for next hop)
                    // This is a simplification; real routing is more complex.
                    if (!string.IsNullOrEmpty(route.NextHop) && route.NextHop != "0.0.0.0")
                    {
                        // This could lead to recursion if not handled carefully.
                        // For now, assume next hop is directly connected or requires another lookup.
                        // A better approach would be to find the interface FOR the next hop.
                        // Let's assume for now the next hop is on a connected network.
                        foreach (var localIface in Interfaces.Values)
                        {
                            if (!string.IsNullOrEmpty(localIface.IpAddress) && IsIpInNetwork(route.NextHop, GetNetwork(localIface.IpAddress, localIface.SubnetMask), localIface.SubnetMask))
                            {
                                return localIface.Name;
                            }
                        }
                    }
                }
            }

            return null; // No route found
        }

        /// <summary>
        /// Get physical connections for a specific interface
        /// </summary>
        public List<PhysicalConnection> GetPhysicalConnectionsForInterface(string interfaceName)
        {
            if (ParentNetwork == null)
                return new List<PhysicalConnection>();

            return ParentNetwork.GetPhysicalConnectionsForInterface(Name, interfaceName);
        }

        /// <summary>
        /// Get all operational physical connections for this device
        /// </summary>
        public List<PhysicalConnection> GetOperationalPhysicalConnections()
        {
            if (ParentNetwork == null)
                return new List<PhysicalConnection>();

            return ParentNetwork.GetAllPhysicalConnections()
                .Where(conn => (conn.Device1Name == Name || conn.Device2Name == Name) && conn.IsOperational)
                .ToList();
        }

        /// <summary>
        /// Check if an interface has operational physical connectivity
        /// </summary>
        public bool IsInterfacePhysicallyConnected(string interfaceName)
        {
            if (ParentNetwork == null)
                return false;

            return ParentNetwork.IsInterfaceConnected(Name, interfaceName);
        }

        /// <summary>
        /// Get physical connection quality metrics for an interface
        /// </summary>
        public PhysicalConnectionMetrics? GetPhysicalConnectionMetrics(string interfaceName)
        {
            var connections = GetPhysicalConnectionsForInterface(interfaceName);
            var operationalConnection = connections.FirstOrDefault(c => c.IsOperational);

            if (operationalConnection == null)
                return null;

            return new PhysicalConnectionMetrics
            {
                ConnectionId = operationalConnection.Id,
                State = operationalConnection.State,
                Bandwidth = operationalConnection.Bandwidth,
                Latency = operationalConnection.Latency,
                PacketLoss = operationalConnection.PacketLoss,
                MaxTransmissionUnit = operationalConnection.MaxTransmissionUnit,
                ErrorCount = operationalConnection.ErrorCount,
                ConnectionType = operationalConnection.ConnectionType
            };
        }

        /// <summary>
        /// Test physical connectivity by simulating packet transmission
        /// </summary>
        public PhysicalTransmissionResult TestPhysicalConnectivity(string interfaceName, int packetSize = 1500)
        {
            var connections = GetPhysicalConnectionsForInterface(interfaceName);
            var operationalConnection = connections.FirstOrDefault(c => c.IsOperational);

            if (operationalConnection == null)
                return new PhysicalTransmissionResult
                {
                    Success = false,
                    Reason = "No operational physical connection"
                };

            return operationalConnection.SimulateTransmission(packetSize);
        }

        /// <summary>
        /// Get remote device and interface connected to a local interface
        /// </summary>
        public (INetworkDevice device, string interfaceName)? GetConnectedDevice(string localInterfaceName)
        {
            if (ParentNetwork == null)
                return null;

            var connections = GetPhysicalConnectionsForInterface(localInterfaceName);
            var operationalConnection = connections.FirstOrDefault(c => c.IsOperational);

            if (operationalConnection == null)
                return null;

            var remoteEnd = operationalConnection.GetRemoteEnd(Name, localInterfaceName);
            if (!remoteEnd.HasValue)
                return null;

            var remoteDevice = ParentNetwork.GetDevice(remoteEnd.Value.deviceName);
            if (remoteDevice == null)
                return null;

            return (remoteDevice, remoteEnd.Value.interfaceName);
        }

        /// <summary>
        /// Check if protocols should consider this interface for routing/switching decisions
        /// This method respects physical connectivity - protocols should only use interfaces
        /// that have operational physical connections
        /// </summary>
        public bool ShouldInterfaceParticipateInProtocols(string interfaceName)
        {
            var interfaceConfig = GetInterface(interfaceName);
            if (interfaceConfig == null || interfaceConfig.IsShutdown)
                return false;

            // Interface must have operational physical connectivity to participate in protocols
            return IsInterfacePhysicallyConnected(interfaceName);
        }

        /// <summary>
        /// Just set the running configuration directly, no commands processed.
        /// </summary>
        /// <param name="config"></param>
        public void SetRunningConfig(string config)
        {
            this.RunningConfig.Import(config);
        }

        /// <summary>
        /// Check if ICMP traffic is blocked by ACL on the destination device
        /// </summary>
        protected virtual bool IsDestinationIcmpBlocked(INetworkDevice destDevice, string destInterfaceName, string destination)
        {
            var destInterface = destDevice.GetInterface(destInterfaceName);
            if (destInterface?.IncomingAccessList != null)
            {
                var acl = destDevice.GetAccessList(destInterface.IncomingAccessList.Value);
                if (acl != null)
                {
                    foreach (var entry in acl.Entries)
                    {
                        if (entry.Protocol.ToLowerInvariant() == "icmp" && entry.Action == "deny")
                        {
                            // Check if this ACL entry applies to the destination
                            if (entry.DestAddress == "any" ||
                                entry.DestAddress == destination ||
                                (entry.DestAddress.StartsWith("host ") && entry.DestAddress.Substring(5) == destination))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Subscribe all registered protocols to events
        /// This should be called when a device is added to a network
        /// </summary>
        public void SubscribeProtocolsToEvents()
        {
            if (ParentNetwork?.EventBus != null)
            {
                foreach (var protocol in _protocols)
                {
                    protocol.SubscribeToEvents(ParentNetwork.EventBus, this);
                    AddLogEntry($"Protocol {protocol.Type} subscribed to events.");
                }
            }
        }

        /// <summary>
        /// Get all registered network protocols
        /// </summary>
        /// <returns>Read-only collection of registered protocols</returns>
        public IReadOnlyList<IDeviceProtocol> GetRegisteredProtocols()
        {
            return _protocols.AsReadOnly();
        }

        /// <summary>
        /// Clear the command history
        /// </summary>
        public void ClearCommandHistory()
        {
            CommandHistory.Clear();
        }
    }
}
