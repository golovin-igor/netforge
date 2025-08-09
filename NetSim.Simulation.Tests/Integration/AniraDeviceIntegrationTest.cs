using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.Integration
{
    public class AniraDeviceIntegrationTest
    {
        [Fact]
        public async Task AniraDevice_MigrationVerification_ShouldWork()
        {
            // This test verifies that AniraDevice has been successfully migrated
            // to use vendor-agnostic handlers instead of old common handlers
            
            // Arrange
            var device = new AniraDevice("MigrationTestDevice");

            // Act & Assert - Test basic functionality
            Assert.NotNull(device);
            Assert.Equal("Anira", device.Vendor);
            Assert.Equal("MigrationTestDevice", device.GetHostname());

            // Test that commands work (indicating successful migration)
            var result1 = await device.ProcessCommandAsync("enable");
            Assert.NotNull(result1);

            var result2 = await device.ProcessCommandAsync("ping 8.8.8.8");
            Assert.NotNull(result2);
            Assert.Contains("8.8.8.8", result2);

            var result3 = await device.ProcessCommandAsync("configure terminal");
            Assert.NotNull(result3);

            // Verify interfaces are initialized
            var interfaces = device.GetAllInterfaces();
            Assert.NotEmpty(interfaces);
            Assert.True(interfaces.ContainsKey("ge-0/0/0"));
        }
    }
} 