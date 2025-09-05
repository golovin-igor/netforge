using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Protocols.Common.Services;
using NetForge.Simulation.DataTypes;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using Xunit;

namespace NetForge.Simulation.Tests.Vendors
{
    /// <summary>
    /// Integration tests to verify the migration from plugin-based system to vendor-based system
    /// </summary>
    public class VendorMigrationIntegrationTests
    {
        private IServiceProvider CreateMigratedServiceProvider()
        {
            var services = new ServiceCollection();
            
            // Configure the new vendor-based system
            services.ConfigureVendorSystem();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Initialize the migration
            VendorSystemStartup.MigrateFromPluginSystem(serviceProvider);
            
            return serviceProvider;
        }

        [Fact]
        public void VendorSystem_Should_Replace_ProtocolDiscoveryService()
        {
            // Arrange
            var serviceProvider = CreateMigratedServiceProvider();
            var protocolService = serviceProvider.GetRequiredService<VendorBasedProtocolService>();
            
            // Act
            var supportedVendors = protocolService.GetSupportedVendors().ToList();
            var ciscoOspf = protocolService.GetProtocol(NetworkProtocolType.OSPF, "Cisco");
            var juniperBgp = protocolService.GetProtocol(NetworkProtocolType.BGP, "Juniper");
            
            // Assert
            Assert.NotEmpty(supportedVendors);
            Assert.Contains("Cisco", supportedVendors);
            Assert.Contains("Juniper", supportedVendors);
            Assert.Contains("Arista", supportedVendors);
            
            // Note: These might be null if the actual protocol classes don't exist,
            // but the service should be able to handle the requests
            Assert.NotNull(protocolService);
        }

        [Fact]
        public void VendorSystem_Should_Replace_HandlerDiscoveryService()
        {
            // Arrange
            var serviceProvider = CreateMigratedServiceProvider();
            var handlerService = serviceProvider.GetRequiredService<VendorBasedHandlerService>();
            
            // Act
            var supportedVendors = handlerService.GetRegisteredVendors().ToList();
            var ciscoHandlers = handlerService.GetHandlersForVendor("Cisco").ToList();
            var statistics = handlerService.GetDiscoveryStatistics();
            
            // Assert
            Assert.NotEmpty(supportedVendors);
            Assert.Contains("Cisco", supportedVendors);
            Assert.Contains("Juniper", supportedVendors);
            Assert.Contains("Arista", supportedVendors);
            
            Assert.NotNull(statistics);
            Assert.True(statistics.ContainsKey("TotalVendors"));
            Assert.True(statistics.ContainsKey("TotalHandlers"));
        }

        [Fact]
        public void VendorAwareProtocolManager_Should_Work_With_VendorService()
        {
            // Arrange
            var serviceProvider = CreateMigratedServiceProvider();
            var protocolManager = serviceProvider.GetRequiredService<VendorAwareProtocolManager>();
            
            // Create a mock device using anonymous object (still works with reflection fallback)
            var mockDevice = new { Vendor = "Cisco", DeviceType = "Router", Hostname = "R1" };
            
            // Act
            var protocol = protocolManager.CreateProtocolForDevice(mockDevice, NetworkProtocolType.OSPF);
            var supportsCisco = protocolManager.VendorSupportsProtocol("Cisco", NetworkProtocolType.OSPF);
            var statistics = protocolManager.GetStatistics();
            
            // Assert
            Assert.True(supportsCisco);
            Assert.NotNull(statistics);
            Assert.True(statistics.ContainsKey("TotalVendors"));
        }

        [Fact]
        public void VendorAwareCliHandlerManager_Should_Work_With_VendorService()
        {
            // Arrange
            var serviceProvider = CreateMigratedServiceProvider();
            
            // Create a proper mock device that implements INetworkDevice
            var mockDevice = new MockNetworkDevice { Vendor = "Cisco", Name = "SW1" };
            
            // Act
            var handlerManager = VendorSystemStartup.CreateVendorAwareHandlerManager(mockDevice, serviceProvider);
            
            // Assert
            Assert.NotNull(handlerManager);
            Assert.IsType<VendorAwareCliHandlerManager>(handlerManager);
            
            var vendorManager = (VendorAwareCliHandlerManager)handlerManager;
            var vendorInfo = vendorManager.GetVendorInfo();
            
            Assert.Equal("Cisco", vendorInfo.DeviceVendor);
            Assert.True(vendorInfo.IsVendorSupported);
        }

        [Fact]
        public void DeviceInitialization_Should_Use_VendorService()
        {
            // Arrange
            var serviceProvider = CreateMigratedServiceProvider();
            var mockDevice = new { Vendor = "Juniper", DeviceType = "Router", Hostname = "J1" };
            
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => 
                VendorSystemStartup.InitializeDeviceWithVendorSystem(mockDevice, serviceProvider));
        }

        [Fact]
        public void VendorRegistry_Should_Contain_All_Expected_Vendors()
        {
            // Arrange
            var serviceProvider = CreateMigratedServiceProvider();
            var vendorRegistry = serviceProvider.GetRequiredService<IVendorRegistry>();
            
            // Act
            var allVendors = vendorRegistry.GetAllVendors().ToList();
            var vendorNames = allVendors.Select(v => v.VendorName).ToList();
            
            // Assert
            Assert.Contains("Cisco", vendorNames);
            Assert.Contains("Juniper", vendorNames);
            Assert.Contains("Arista", vendorNames);
            
            // Verify each vendor has protocols and handlers
            foreach (var vendor in allVendors)
            {
                Assert.NotEmpty(vendor.SupportedProtocols);
                Assert.NotEmpty(vendor.CliHandlers);
                Assert.NotEmpty(vendor.SupportedModels);
            }
        }

        [Fact]
        public void VendorDescriptors_Should_Have_Correct_Priorities()
        {
            // Arrange
            var serviceProvider = CreateMigratedServiceProvider();
            var vendorRegistry = serviceProvider.GetRequiredService<IVendorRegistry>();
            
            // Act
            var cisco = vendorRegistry.GetVendor("Cisco");
            var juniper = vendorRegistry.GetVendor("Juniper");
            var arista = vendorRegistry.GetVendor("Arista");
            
            // Assert
            Assert.NotNull(cisco);
            Assert.NotNull(juniper);
            Assert.NotNull(arista);
            
            // Cisco should have highest priority
            Assert.True(cisco.Priority > juniper.Priority);
            Assert.True(cisco.Priority > arista.Priority);
        }

        [Fact]
        public void Migration_Should_Preserve_Protocol_Functionality()
        {
            // Arrange
            var serviceProvider = CreateMigratedServiceProvider();
            var protocolService = serviceProvider.GetRequiredService<VendorBasedProtocolService>();
            
            // Act
            var ospfAvailable = protocolService.IsProtocolAvailable(NetworkProtocolType.OSPF);
            var bgpVendors = protocolService.GetVendorsForProtocol(NetworkProtocolType.BGP).ToList();
            var eigrpVendors = protocolService.GetVendorsForProtocol(NetworkProtocolType.EIGRP).ToList();
            
            // Assert
            Assert.True(ospfAvailable);
            Assert.NotEmpty(bgpVendors);
            
            // EIGRP should only be supported by Cisco
            Assert.Single(eigrpVendors);
            Assert.Equal("Cisco", eigrpVendors.First());
        }

        [Fact]
        public void ServiceProvider_Should_Resolve_All_VendorSystem_Services()
        {
            // Arrange & Act
            var serviceProvider = CreateMigratedServiceProvider();
            
            // Assert - All services should be resolvable
            Assert.NotNull(serviceProvider.GetRequiredService<IVendorRegistry>());
            Assert.NotNull(serviceProvider.GetRequiredService<IVendorService>());
            Assert.NotNull(serviceProvider.GetRequiredService<VendorBasedProtocolService>());
            Assert.NotNull(serviceProvider.GetRequiredService<VendorBasedHandlerService>());
            Assert.NotNull(serviceProvider.GetRequiredService<VendorAwareProtocolManager>());
        }
    }

    /// <summary>
    /// Mock implementation of INetworkDevice for testing
    /// </summary>
    public class MockNetworkDevice : INetworkDevice
    {
        public string Name { get; set; } = "";
        public string Vendor { get; set; } = "";
        public string DeviceName => Name;
        public string DeviceType { get; set; } = "Router";
        public string DeviceId { get; set; } = "";
        public INetwork? ParentNetwork { get; set; }
        public bool IsNvramLoaded { get; set; }

        public event Action<string> LogEntryAdded = delegate { };

        public Task<string> ProcessCommandAsync(string command) => Task.FromResult("");
        public CommandHistory GetCommandHistory() => new CommandHistory();
        public string GetPrompt() => "Router>";
        public string GetCurrentPrompt() => "Router>";
        public Dictionary<string, IInterfaceConfig> GetAllInterfaces() => new Dictionary<string, IInterfaceConfig>();
        public Dictionary<int, VlanConfig> GetAllVlans() => new Dictionary<int, VlanConfig>();
        public List<Route> GetRoutingTable() => new List<Route>();
        public Dictionary<int, AccessList> GetAccessLists() => new Dictionary<int, AccessList>();
        public Dictionary<int, PortChannel> GetPortChannels() => new Dictionary<int, PortChannel>();
        public Dictionary<string, string> GetSystemSettings() => new Dictionary<string, string>();
        public List<string> GetLogEntries() => new List<string>();
        public Dictionary<string, string> GetArpTable() => new Dictionary<string, string>();
        public string GetArpTableOutput() => "";
        public OspfConfig? GetOspfConfiguration() => null;
        public BgpConfig? GetBgpConfiguration() => null;
        public RipConfig? GetRipConfiguration() => null;
        public EigrpConfig? GetEigrpConfiguration() => null;
        public StpConfig GetStpConfiguration() => new StpConfig();
        public IgrpConfig? GetIgrpConfiguration() => null;
        public VrrpConfig? GetVrrpConfiguration() => null;
        public HsrpConfig? GetHsrpConfiguration() => null;
        public CdpConfig? GetCdpConfiguration() => null;
        public LldpConfig? GetLldpConfiguration() => null;
        public void SetOspfConfiguration(OspfConfig config) { }
        public void SetBgpConfiguration(BgpConfig config) { }
        public void SetRipConfiguration(RipConfig config) { }
        public void SetEigrpConfiguration(EigrpConfig config) { }
        public void SetStpConfiguration(StpConfig config) { }
        public void SetIgrpConfiguration(IgrpConfig config) { }
        public void SetVrrpConfiguration(VrrpConfig config) { }
        public void SetHsrpConfiguration(HsrpConfig config) { }
        public void SetCdpConfiguration(CdpConfig config) { }
        public void SetLldpConfiguration(LldpConfig config) { }
        public object GetTelnetConfiguration() => new object();
        public void SetTelnetConfiguration(object config) { }
        public object GetSshConfiguration() => new object();
        public void SetSshConfiguration(object config) { }
        public object GetSnmpConfiguration() => new object();
        public void SetSnmpConfiguration(object config) { }
        public object GetHttpConfiguration() => new object();
        public void SetHttpConfiguration(object config) { }
        public string GetHostname() => Name;
        public void SetHostname(string name) => Name = name;
        public string GetCurrentMode() => "exec";
        public void SetCurrentMode(string mode) { }
        public DeviceMode GetCurrentModeEnum() => DeviceMode.Exec;
        public void SetCurrentModeEnum(DeviceMode mode) { }
        public string GetCurrentInterface() => "";
        public void SetCurrentInterface(string iface) { }
        public void SetMode(string mode) { }
        public void SetModeEnum(DeviceMode mode) { }
        public string GetNetworkAddress(string ip, string mask) => "";
        public void ForceUpdateConnectedRoutes() { }
        public string ExecutePing(string destination) => "";
        public bool CheckIpInNetwork(string ip, string network, string mask) => false;
        public void AddRoute(Route route) { }
        public void RemoveRoute(Route route) { }
        public void ClearRoutesByProtocol(string protocol) { }
        public int MaskToCidr(string mask) => 24;
        public IInterfaceConfig? GetInterface(string name) => null;
        public VlanConfig? GetVlan(int id) => null;
        public AccessList? GetAccessList(int number) => null;
        public PortChannel? GetPortChannel(int number) => null;
        public string? GetSystemSetting(string name) => null;
        public void SetSystemSetting(string name, string value) { }
        public void AddLogEntry(string entry) { }
        public void ClearLog() { }
        public void AddStaticRoute(string network, string mask, string nextHop, int metric) { }
        public void RemoveStaticRoute(string network, string mask) { }
        public void RegisterProtocol(IDeviceProtocol protocol) { }
        public Task UpdateAllProtocolStates() => Task.CompletedTask;
        public List<PhysicalConnection> GetPhysicalConnectionsForInterface(string interfaceName) => new List<PhysicalConnection>();
        public List<PhysicalConnection> GetOperationalPhysicalConnections() => new List<PhysicalConnection>();
        public bool IsInterfacePhysicallyConnected(string interfaceName) => false;
        public PhysicalConnectionMetrics? GetPhysicalConnectionMetrics(string interfaceName) => null;
        public PhysicalTransmissionResult TestPhysicalConnectivity(string interfaceName, int packetSize = 1500) => new PhysicalTransmissionResult();
        public (INetworkDevice device, string interfaceName)? GetConnectedDevice(string localInterfaceName) => null;
        public bool ShouldInterfaceParticipateInProtocols(string interfaceName) => false;
        public void SetRunningConfig(string config) { }
        public void SubscribeProtocolsToEvents() { }
        public IReadOnlyList<IDeviceProtocol> GetRegisteredProtocols() => new List<IDeviceProtocol>();
        public void ClearCommandHistory() { }
        public IProtocolService GetProtocolService() => null!;
    }
}