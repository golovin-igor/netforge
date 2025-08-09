using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests for enable command aliases across different device types
    /// </summary>
    public class EnableAliasTests
    {
        [Fact]
        public void CiscoDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("R1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public void AristaDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("SW1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("SW1#", device.GetPrompt());
            Assert.Equal("SW1#", output);
        }

        [Fact]
        public void AniraDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new AniraDevice("R1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public void DellDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new DellDevice("SW1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("SW1#", device.GetPrompt());
            Assert.Equal("SW1#", output);
        }

        [Fact]
        public void MikroTikDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new MikroTikDevice("RB1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("RB1#", device.GetPrompt());
            Assert.Equal("RB1#", output);
        }

        [Fact]
        public void NokiaDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new NokiaDevice("R1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public void LinuxDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new LinuxDevice("R1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public void FortinetDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new FortinetDevice("FW1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("FW1#", device.GetPrompt());
            Assert.Equal("FW1#", output);
        }

        [Fact]
        public void F5Device_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new F5Device("LB1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("LB1#", device.GetPrompt());
            Assert.Equal("LB1#", output);
        }

        [Fact]
        public void ExtremeDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("SW1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("SW1#", device.GetPrompt());
            Assert.Equal("SW1#", output);
        }

        [Fact]
        public void HuaweiDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("R1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public void BroadcomDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new BroadcomDevice("SW1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("SW1#", device.GetPrompt());
            Assert.Equal("SW1#", output);
        }

        [Fact]
        public void AlcatelDevice_EnableAlias_ShouldWork()
        {
            // Arrange
            var device = new AlcatelDevice("R1");
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public void EnableAlias_WhenAlreadyPrivileged_ShouldReturnSuccess()
        {
            // Arrange
            var device = new CiscoDevice("R1");
            device.ProcessCommand("enable"); // Enter privileged mode first
            
            // Act
            var output = device.ProcessCommand("ena");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("R1#", device.GetPrompt());
            Assert.Equal("R1#", output);
        }

        [Fact]
        public void EnableAlias_ShouldBeEquivalentToFullCommand()
        {
            // Arrange
            var device1 = new CiscoDevice("R1");
            var device2 = new CiscoDevice("R2");
            
            // Act
            var output1 = device1.ProcessCommand("ena");
            var output2 = device2.ProcessCommand("enable");
            
            // Assert
            Assert.Equal(device1.GetCurrentMode(), device2.GetCurrentMode());
            Assert.Equal(device1.GetPrompt(), device2.GetPrompt());
            Assert.Equal(output1, output2);
        }
    }
} 