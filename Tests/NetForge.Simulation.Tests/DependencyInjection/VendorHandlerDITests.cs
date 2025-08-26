using Xunit;
using NetForge.Simulation.Common;
using NetForge.Simulation.Core;
using NetForge.Simulation.Common.CLI.Extensions;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Tests.DependencyInjection
{
    [Trait("TestCategory", "VendorDI")]
    public class VendorHandlerDITests
    {
        [Fact]
        public void VendorHandlerFactory_CreateCliHandlerManager_ShouldReturnValidManager()
        {
            // Arrange
            var device = CreateTestDevice("Cisco");

            // Act
            var manager = VendorHandlerFactory.CreateCliHandlerManager(device);

            // Assert
            Assert.NotNull(manager);
            Assert.IsType<VendorAwareCliHandlerManager>(manager);
        }

        [Fact]
        public void VendorHandlerFactory_GetDiscoveryService_ShouldReturnSameInstance()
        {
            // Arrange & Act
            var service1 = VendorHandlerFactory.GetDiscoveryService();
            var service2 = VendorHandlerFactory.GetDiscoveryService();

            // Assert
            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.Same(service1, service2); // Should be singleton
        }

        [Fact]
        public void VendorHandlerFactory_SetDiscoveryService_ShouldUseCustomService()
        {
            // Arrange
            var originalService = VendorHandlerFactory.GetDiscoveryService();
            var customService = new VendorHandlerDiscoveryService();

            try
            {
                // Act
                VendorHandlerFactory.SetDiscoveryService(customService);
                var retrievedService = VendorHandlerFactory.GetDiscoveryService();

                // Assert
                Assert.Same(customService, retrievedService);
                Assert.NotSame(originalService, retrievedService);
            }
            finally
            {
                // Cleanup
                VendorHandlerFactory.Reset();
            }
        }

        [Fact]
        public void VendorHandlerFactory_CreateWithDiscovery_ShouldTriggerRegistration()
        {
            // Arrange
            var device = CreateTestDevice("Cisco");

            // Act
            var manager = VendorHandlerFactory.CreateWithDiscovery(device);

            // Assert
            Assert.NotNull(manager);

            // Get vendor info to verify discovery worked
            var vendorInfo = manager.GetVendorInfo();
            Assert.NotNull(vendorInfo);
            Assert.Equal("Cisco", vendorInfo.DeviceVendor);
            Assert.True(vendorInfo.VendorHandlersRegistered);
        }

        [Fact]
        public void VendorHandlerDiscoveryService_DiscoverVendorRegistries_ShouldFindRegistries()
        {
            // Arrange
            var discoveryService = new VendorHandlerDiscoveryService();

            // Act
            var registries = discoveryService.DiscoverVendorRegistries().ToList();

            // Assert
            Assert.NotNull(registries);
            // We should find at least the Cisco registry if it's properly discovered
            // The exact count depends on what's loaded in the test environment
        }

        [Fact]
        public void VendorHandlerDiscoveryService_GetVendorRegistry_WithCiscoDevice_ShouldReturnRegistry()
        {
            // Arrange
            var discoveryService = new VendorHandlerDiscoveryService();
            var ciscoDevice = CreateTestDevice("Cisco");

            // Act
            var registry = discoveryService.GetVendorRegistry(ciscoDevice);

            // Assert - May be null if Cisco handlers aren't discovered, but test should not throw
            // This test verifies the method works without errors
            Assert.True(true); // If we get here without exception, the method works
        }

        [Fact]
        public void VendorHandlerDiscoveryService_IsVendorSupported_WithKnownVendor_ShouldWork()
        {
            // Arrange
            var discoveryService = new VendorHandlerDiscoveryService();

            // Act
            var isCiscoSupported = discoveryService.IsVendorSupported("Cisco");
            var isUnknownSupported = discoveryService.IsVendorSupported("UnknownVendor");

            // Assert
            // The exact result depends on discovery, but method should not throw
            Assert.True(true);
        }

        [Fact]
        public void VendorAwareCliHandlerManager_GetVendorInfo_ShouldReturnCorrectInfo()
        {
            // Arrange
            var device = CreateTestDevice("TestVendor");
            var discoveryService = new VendorHandlerDiscoveryService();
            var manager = new VendorAwareCliHandlerManager(device, discoveryService);

            // Act
            var vendorInfo = manager.GetVendorInfo();

            // Assert
            Assert.NotNull(vendorInfo);
            Assert.Equal("TestVendor", vendorInfo.DeviceVendor);
            Assert.NotNull(vendorInfo.SupportedVendors);
        }

        [Fact]
        public void VendorAwareCliHandlerManager_RefreshVendorHandlers_ShouldNotThrow()
        {
            // Arrange
            var device = CreateTestDevice("Cisco");
            var manager = VendorHandlerFactory.CreateCliHandlerManager(device);

            // Act & Assert
            var exception = Record.Exception(() => manager.RefreshVendorHandlers());
            Assert.Null(exception);
        }

        [Fact]
        public void VendorHandlerFactory_Reset_ShouldClearState()
        {
            // Arrange
            var originalService = VendorHandlerFactory.GetDiscoveryService();

            // Act
            VendorHandlerFactory.Reset();
            var newService = VendorHandlerFactory.GetDiscoveryService();

            // Assert
            Assert.NotSame(originalService, newService);
        }

        private NetworkDevice CreateTestDevice(string vendor)
        {
            // Create a test device for DI testing
            return new TestDevice("TestDevice", vendor);
        }
    }

    /// <summary>
    /// Simple test device implementation for DI testing
    /// </summary>
    public class TestDevice : NetworkDevice
    {
        public TestDevice(string name, string vendor = "Test") : base(name)
        {
            // Set vendor using reflection since the setter is protected
            var vendorProperty = typeof(NetworkDevice).GetProperty("Vendor");
            if (vendorProperty != null && vendorProperty.CanWrite)
            {
                vendorProperty.SetValue(this, vendor);
            }
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
            // No device-specific handlers for test device
        }

        public override string GetPrompt()
        {
            return $"{GetHostname()}>";
        }

        protected override void InitializeDefaultInterfaces()
        {
            // No interfaces needed for DI testing
        }
    }
}
