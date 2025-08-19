using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests for enable command aliases across different device types
    /// </summary>
    public class EnableAliasTests
    {
        [Fact]
        public async Task CiscoDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("R1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public async Task AristaDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("SW1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("SW1#", device.GetPrompt());
            Assert.Equal("SW1#", output);
        }

        [Fact]
        public async Task AniraDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new AniraDevice("R1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public async Task DellDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new DellDevice("SW1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("SW1#", device.GetPrompt());
            Assert.Equal("SW1#", output);
        }

        [Fact]
        public async Task MikroTikDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new MikroTikDevice("RB1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("RB1#", device.GetPrompt());
            Assert.Equal("RB1#", output);
        }

        [Fact]
        public async Task NokiaDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new NokiaDevice("R1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public async Task LinuxDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new LinuxDevice("R1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public async Task FortinetDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new FortinetDevice("FW1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("FW1#", device.GetPrompt());
            Assert.Equal("FW1#", output);
        }

        [Fact]
        public async Task F5Device_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new F5Device("LB1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("LB1#", device.GetPrompt());
            Assert.Equal("LB1#", output);
        }

        [Fact]
        public async Task ExtremeDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("SW1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("SW1#", device.GetPrompt());
            Assert.Equal("SW1#", output);
        }

        [Fact]
        public async Task HuaweiDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("R1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public async Task BroadcomDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new BroadcomDevice("SW1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("SW1#", device.GetPrompt());
            Assert.Equal("SW1#", output);
        }

        [Fact]
        public async Task AlcatelDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new AlcatelDevice("R1");
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public async Task EnableAlias_WhenAlreadyPrivileged_ShouldReturnSuccess()
        {
            // Arrange
            var device = new CiscoDevice("R1");
            await device.ProcessCommandAsync("enable"); // Enter privileged mode first
            
            // Act
            var output = await device.ProcessCommandAsync("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public async Task EnableAlias_ShouldBeEquivalentToFullCommand()
        {
            // Arrange
            var device1 = new CiscoDevice("R1");
            var device2 = new CiscoDevice("R2");
            
            // Act
            var output1 = await device1.ProcessCommandAsync("ena");
            var output2 = await device2.ProcessCommandAsync("enable");
            
            // Assert
            Assert.Equal(device1.GetCurrentMode(), device2.GetCurrentMode());
            Assert.Equal(device1.GetPrompt(), device2.GetPrompt());
            Assert.Equal(output1, output2);
        }
    }
} 
