using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.Configuration
{
    public class InterfaceConfigTests
    {
        [Fact]
        public void InterfaceConfig_Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var name = "GigabitEthernet0/0";
            var device = new CiscoDevice("Router1");

            // Act
            var config = new InterfaceConfig(name, device);

            // Assert
            Assert.Equal(name, config.Name);
            Assert.Equal("", config.Description);
            Assert.Null(config.IpAddress);
            Assert.Null(config.SubnetMask);
            Assert.True(config.IsUp);
            Assert.False(config.IsShutdown);
            Assert.Equal(1500, config.Mtu);
            Assert.Equal("auto", config.Duplex);
            Assert.Equal("auto", config.Speed);
            Assert.NotNull(config.MacAddress);
            Assert.Equal(1, config.VlanId);
            Assert.Equal("access", config.SwitchportMode);
        }

        [Fact]
        public void InterfaceConfig_WithoutDevice_ShouldWork()
        {
            // Arrange
            var name = "FastEthernet0/1";

            // Act
            var config = new InterfaceConfig(name, null);

            // Assert
            Assert.Equal(name, config.Name);
            Assert.NotNull(config.MacAddress);
            Assert.True(config.IsUp);
            Assert.False(config.IsShutdown);
        }

        [Fact]
        public void InterfaceConfig_IsShutdown_ShouldAffectStatus()
        {
            // Arrange
            var config = new InterfaceConfig("eth0", null);

            // Act & Assert - Initial state
            Assert.True(config.IsUp);
            Assert.False(config.IsShutdown);
            Assert.Equal("up", config.GetStatus());

            // Act & Assert - Shutdown
            config.IsShutdown = true;
            Assert.Equal("administratively down", config.GetStatus());
            Assert.False(config.IsUp); // Shutdown forces IsUp to false

            // Act & Assert - Clear shutdown (but need to manually bring up)
            config.IsShutdown = false;
            Assert.False(config.IsUp); // IsUp remains false after clearing shutdown
            Assert.Equal("down", config.GetStatus()); // Status is "down" not "up"

            // Act & Assert - Manually bring interface up
            config.IsUp = true;
            Assert.Equal("up", config.GetStatus());
        }

        [Fact]
        public void InterfaceConfig_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new InterfaceConfig("eth0", null);

            // Act & Assert
            config.IpAddress = "192.168.1.1";
            Assert.Equal("192.168.1.1", config.IpAddress);

            config.SubnetMask = "255.255.255.0";
            Assert.Equal("255.255.255.0", config.SubnetMask);

            config.Description = "LAN Interface";
            Assert.Equal("LAN Interface", config.Description);

            config.VlanId = 10;
            Assert.Equal(10, config.VlanId);

            config.SwitchportMode = "trunk";
            Assert.Equal("trunk", config.SwitchportMode);

            config.Mtu = 9000;
            Assert.Equal(9000, config.Mtu);

            config.Duplex = "full";
            Assert.Equal("full", config.Duplex);

            config.Speed = "1000";
            Assert.Equal("1000", config.Speed);
        }

        [Fact]
        public void InterfaceConfig_OspfProperties_CanBeConfigured()
        {
            // Arrange
            var config = new InterfaceConfig("eth0", null);

            // Act
            config.OspfEnabled = true;
            config.OspfProcessId = 100;
            config.OspfArea = 1;
            config.OspfCost = 50;
            config.OspfNetworkType = "point-to-point";

            // Assert
            Assert.True(config.OspfEnabled);
            Assert.Equal(100, config.OspfProcessId);
            Assert.Equal(1, config.OspfArea);
            Assert.Equal(50, config.OspfCost);
            Assert.Equal("point-to-point", config.OspfNetworkType);
        }

        [Fact]
        public void InterfaceConfig_StpProperties_CanBeConfigured()
        {
            // Arrange
            var config = new InterfaceConfig("eth0", null);

            // Act
            config.StpPortfast = true;
            config.StpBpduGuard = true;

            // Assert
            Assert.True(config.StpPortfast);
            Assert.True(config.StpBpduGuard);
        }

        [Fact]
        public void InterfaceConfig_ChannelProperties_CanBeConfigured()
        {
            // Arrange
            var config = new InterfaceConfig("eth0", null);

            // Act
            config.ChannelGroup = 1;
            config.ChannelMode = "active";

            // Assert
            Assert.Equal(1, config.ChannelGroup);
            Assert.Equal("active", config.ChannelMode);
        }

        [Fact]
        public void InterfaceConfig_PacketCounters_CanBeIncremented()
        {
            // Arrange
            var config = new InterfaceConfig("eth0", null);

            // Act
            config.RxPackets = 100;
            config.TxPackets = 50;
            config.RxBytes = 1024;
            config.TxBytes = 512;

            // Assert
            Assert.Equal(100, config.RxPackets);
            Assert.Equal(50, config.TxPackets);
            Assert.Equal(1024, config.RxBytes);
            Assert.Equal(512, config.TxBytes);
        }

        [Fact]
        public void InterfaceConfig_SetParentDevice_ShouldWork()
        {
            // Arrange
            var config = new InterfaceConfig("eth0", null);
            var device = new CiscoDevice("Switch1");

            // Act
            config.SetParentDevice(device);

            // Assert - Can't directly test private field, but method should not throw
            Assert.NotNull(config);
        }

        [Theory]
        [InlineData("GigabitEthernet0/0")]
        [InlineData("FastEthernet0/1")]
        [InlineData("eth0")]
        [InlineData("Vlan100")]
        public void InterfaceConfig_GeneratesMacFromName(string interfaceName)
        {
            // Act
            var config = new InterfaceConfig(interfaceName, null);

            // Assert
            Assert.NotNull(config.MacAddress);
            Assert.NotEmpty(config.MacAddress);
            Assert.Contains(":", config.MacAddress);
            Assert.True(config.MacAddress.Length >= 17); // MAC address format xx:xx:xx:xx:xx:xx
        }

        [Theory]
        [InlineData(true, false, "up")]
        [InlineData(false, true, "administratively down")]
        [InlineData(false, false, "down")]
        public void InterfaceConfig_GetStatus_ReturnsCorrectStatus(bool isUp, bool isShutdown, string expectedStatus)
        {
            // Arrange
            var config = new InterfaceConfig("eth0", null);

            // Act
            if (isShutdown)
                config.IsShutdown = true;
            else if (!isUp)
                config.IsUp = false;

            var status = config.GetStatus();

            // Assert
            Assert.Equal(expectedStatus, status);
        }
    }
}
