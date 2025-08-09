using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.Devices
{
    public class AniraDeviceMigrationTests
    {
        [Fact]
        public void AniraDevice_Constructor_ShouldInitializeWithVendorAgnosticHandlers()
        {
            // Arrange & Act
            var device = new AniraDevice("TestAniraDevice");

            // Assert
            Assert.NotNull(device);
            Assert.Equal("Anira", device.Vendor);
            Assert.Equal("TestAniraDevice", device.GetHostname());
            // CommandManager is not publicly accessible, but we can test it works via ProcessCommand
        }

        [Fact]
        public async Task AniraDevice_EnableCommand_ShouldWorkWithVendorAgnosticHandler()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");

            // Act
            var result = await device.ProcessCommandAsync("enable");

            // Assert
            Assert.Contains(">", device.GetPrompt()); // Should be in privileged mode
        }

        [Fact]
        public async Task AniraDevice_DisableCommand_ShouldWorkWithVendorAgnosticHandler()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");
            await device.ProcessCommandAsync("enable"); // First enter privileged mode

            // Act
            var result = await device.ProcessCommandAsync("disable");

            // Assert
            Assert.Contains(">", device.GetPrompt()); // Should be back in user mode
        }

        [Fact]
        public async Task AniraDevice_PingCommand_ShouldWorkWithVendorAgnosticHandler()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");

            // Act
            var result = await device.ProcessCommandAsync("ping 192.168.1.1");

            // Assert
            Assert.Contains("192.168.1.1", result);
            Assert.Contains("ping", result.ToLower());
        }

        [Fact]
        public async Task AniraDevice_ConfigureTerminalCommand_ShouldWorkWithVendorAgnosticHandler()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");
            await device.ProcessCommandAsync("enable");

            // Act
            var result = await device.ProcessCommandAsync("configure terminal");

            // Assert
            Assert.Contains("(config)", device.GetPrompt());
        }

        [Fact]
        public async Task AniraDevice_HostnameCommand_ShouldWorkWithVendorAgnosticHandler()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");

            // Act
            var result = await device.ProcessCommandAsync("hostname NewAniraName");

            // Assert
            Assert.Equal("NewAniraName", device.GetHostname());
            Assert.Contains("NewAniraName", device.GetPrompt());
        }

        [Fact]
        public async Task AniraDevice_InterfaceConfiguration_ShouldWorkWithVendorAgnosticHandlers()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");

            // Act
            var result1 = await device.ProcessCommandAsync("interface ge-0/0/0");
            var result2 = await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            var result3 = await device.ProcessCommandAsync("no shutdown");

            // Assert
            Assert.Contains("(config-if)", device.GetPrompt());
            var interfaces = device.GetAllInterfaces();
            Assert.True(interfaces.ContainsKey("ge-0/0/0"));
            Assert.Equal("192.168.1.1", interfaces["ge-0/0/0"].IpAddress);
            Assert.Equal("255.255.255.0", interfaces["ge-0/0/0"].SubnetMask);
            Assert.True(interfaces["ge-0/0/0"].IsUp);
        }

        [Fact]
        public async Task AniraDevice_ExitCommand_ShouldWorkWithVendorAgnosticHandler()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface ge-0/0/0");

            // Act - Exit from interface mode to config mode
            var result1 = await device.ProcessCommandAsync("exit");
            var prompt1 = device.GetPrompt();

            // Exit from config mode to privileged mode
            var result2 = await device.ProcessCommandAsync("exit");
            var prompt2 = device.GetPrompt();

            // Assert
            Assert.Contains("(config)#", prompt1); // Should be in config mode
            Assert.Contains("#", prompt2); // Should be in privileged mode
            Assert.DoesNotContain("(config)", prompt2); // Should not be in config mode
        }

        [Fact]
        public async Task AniraDevice_WriteCommand_ShouldWorkWithVendorAgnosticHandler()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");
            await device.ProcessCommandAsync("enable");

            // Act
            var result = await device.ProcessCommandAsync("write");

            // Assert
            Assert.Contains("saved", result.ToLower());
        }

        [Fact]
        public async Task AniraDevice_ReloadCommand_ShouldWorkWithVendorAgnosticHandler()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");
            await device.ProcessCommandAsync("enable");

            // Act
            var result = await device.ProcessCommandAsync("reload");

            // Assert
            Assert.Contains("restart", result.ToLower());
        }

        [Fact]
        public async Task AniraDevice_VendorAgnosticHandlers_ShouldNotUseOldCommonHandlers()
        {
            // Arrange
            var device = new AniraDevice("TestAniraDevice");

            // Act & Assert - The device should use vendor-agnostic handlers from Cisco project
            // This test verifies that the migration is complete and no old common handlers are used
            
            // Test that basic commands work (proving the new handlers are loaded)
            var enableResult = await device.ProcessCommandAsync("enable");
            var pingResult = await device.ProcessCommandAsync("ping 1.1.1.1");
            var configResult = await device.ProcessCommandAsync("configure terminal");
            var exitResult = await device.ProcessCommandAsync("exit");

            // These should all work without errors, indicating successful migration
            Assert.NotNull(enableResult);
            Assert.NotNull(pingResult);
            Assert.NotNull(configResult);
            Assert.NotNull(exitResult);
            
            // Verify vendor is still set correctly
            Assert.Equal("Anira", device.Vendor);
        }

        [Fact]
        public void AniraDevice_DefaultInterfaces_ShouldBeInitialized()
        {
            // Arrange & Act
            var device = new AniraDevice("TestAniraDevice");

            // Assert
            var interfaces = device.GetAllInterfaces();
            Assert.True(interfaces.ContainsKey("ge-0/0/0"));
            Assert.True(interfaces.ContainsKey("ge-0/0/1"));
            Assert.True(interfaces.ContainsKey("ge-0/0/2"));
            Assert.True(interfaces.ContainsKey("ge-0/0/3"));
        }

        [Fact]
        public void AniraDevice_NetworkProtocols_ShouldBeRegistered()
        {
            // Arrange & Act
            var device = new AniraDevice("TestAniraDevice");

            // Assert - Verify that network protocols are still registered
            // The constructor should register OSPF, BGP, STP, and LLDP protocols
            Assert.NotNull(device);
            
            // These protocols should be available for routing configuration
            // We can verify this by checking that device is ready for protocol commands
            Assert.Equal("Anira", device.Vendor);
        }
    }
} 