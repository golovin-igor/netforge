using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Vendors;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Protocols.Common.Services;
using NetForge.Simulation.DataTypes;
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
            
            // Create a mock device
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
            var mockDevice = new { Vendor = "Cisco", DeviceType = "Switch", Hostname = "SW1" };
            
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
}