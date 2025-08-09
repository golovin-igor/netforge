using NetSim.Simulation.Configuration;
using Xunit;

namespace NetSim.Simulation.Tests.Configuration
{
    public class PortChannelTests
    {
        [Fact]
        public void PortChannel_Constructor_ShouldSetDefaultValues()
        {
            // Arrange
            int channelId = 5;

            // Act
            var portChannel = new PortChannel(channelId);

            // Assert
            Assert.Equal(channelId, portChannel.Id);
            Assert.Equal("", portChannel.Description);
            Assert.True(portChannel.IsUp);
            Assert.NotNull(portChannel.MemberPorts);
            Assert.Empty(portChannel.MemberPorts);
            Assert.NotNull(portChannel.MemberInterfaces);
            Assert.Empty(portChannel.MemberInterfaces);
            Assert.Equal("on", portChannel.Mode);
            Assert.Equal("lacp", portChannel.Protocol);
        }

        [Fact]
        public void PortChannel_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act & Assert
            portChannel.Id = 15;
            Assert.Equal(15, portChannel.Id);

            portChannel.Description = "Test Port Channel";
            Assert.Equal("Test Port Channel", portChannel.Description);

            portChannel.IsUp = false;
            Assert.False(portChannel.IsUp);

            portChannel.Mode = "active";
            Assert.Equal("active", portChannel.Mode);

            portChannel.Protocol = "pagp";
            Assert.Equal("pagp", portChannel.Protocol);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PortChannel_IsUp_CanBeToggled(bool isUp)
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.IsUp = isUp;

            // Assert
            Assert.Equal(isUp, portChannel.IsUp);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        public void PortChannel_Id_AcceptsVariousValues(int id)
        {
            // Act
            var portChannel = new PortChannel(id);

            // Assert
            Assert.Equal(id, portChannel.Id);
        }

        [Theory]
        [InlineData("on")]
        [InlineData("active")]
        [InlineData("passive")]
        [InlineData("auto")]
        [InlineData("desirable")]
        public void PortChannel_Mode_AcceptsStandardModes(string mode)
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.Mode = mode;

            // Assert
            Assert.Equal(mode, portChannel.Mode);
        }

        [Theory]
        [InlineData("lacp")]
        [InlineData("pagp")]
        [InlineData("static")]
        [InlineData("none")]
        public void PortChannel_Protocol_AcceptsStandardProtocols(string protocol)
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.Protocol = protocol;

            // Assert
            Assert.Equal(protocol, portChannel.Protocol);
        }

        [Fact]
        public void PortChannel_MemberPorts_CanBeModified()
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.MemberPorts.Add("Gi0/1");
            portChannel.MemberPorts.Add("Gi0/2");

            // Assert
            Assert.Equal(2, portChannel.MemberPorts.Count);
            Assert.Contains("Gi0/1", portChannel.MemberPorts);
            Assert.Contains("Gi0/2", portChannel.MemberPorts);
        }

        [Fact]
        public void PortChannel_MemberInterfaces_CanBeModified()
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.MemberInterfaces.Add("GigabitEthernet0/1");
            portChannel.MemberInterfaces.Add("GigabitEthernet0/2");
            portChannel.MemberInterfaces.Add("GigabitEthernet0/3");

            // Assert
            Assert.Equal(3, portChannel.MemberInterfaces.Count);
            Assert.Contains("GigabitEthernet0/1", portChannel.MemberInterfaces);
            Assert.Contains("GigabitEthernet0/2", portChannel.MemberInterfaces);
            Assert.Contains("GigabitEthernet0/3", portChannel.MemberInterfaces);
        }

        [Fact]
        public void PortChannel_MemberPorts_CanBeRemoved()
        {
            // Arrange
            var portChannel = new PortChannel(1);
            portChannel.MemberPorts.Add("Gi0/1");
            portChannel.MemberPorts.Add("Gi0/2");

            // Act
            portChannel.MemberPorts.Remove("Gi0/1");

            // Assert
            Assert.Single(portChannel.MemberPorts);
            Assert.DoesNotContain("Gi0/1", portChannel.MemberPorts);
            Assert.Contains("Gi0/2", portChannel.MemberPorts);
        }

        [Fact]
        public void PortChannel_MemberInterfaces_CanBeRemoved()
        {
            // Arrange
            var portChannel = new PortChannel(1);
            portChannel.MemberInterfaces.Add("GigabitEthernet0/1");
            portChannel.MemberInterfaces.Add("GigabitEthernet0/2");

            // Act
            portChannel.MemberInterfaces.Remove("GigabitEthernet0/1");

            // Assert
            Assert.Single(portChannel.MemberInterfaces);
            Assert.DoesNotContain("GigabitEthernet0/1", portChannel.MemberInterfaces);
            Assert.Contains("GigabitEthernet0/2", portChannel.MemberInterfaces);
        }

        [Fact]
        public void PortChannel_MemberCollections_CanBeCleared()
        {
            // Arrange
            var portChannel = new PortChannel(1);
            portChannel.MemberPorts.Add("Gi0/1");
            portChannel.MemberPorts.Add("Gi0/2");
            portChannel.MemberInterfaces.Add("GigabitEthernet0/1");
            portChannel.MemberInterfaces.Add("GigabitEthernet0/2");

            // Act
            portChannel.MemberPorts.Clear();
            portChannel.MemberInterfaces.Clear();

            // Assert
            Assert.Empty(portChannel.MemberPorts);
            Assert.Empty(portChannel.MemberInterfaces);
        }

        [Fact]
        public void PortChannel_MultipleInstances_ShouldBeIndependent()
        {
            // Arrange & Act
            var channel1 = new PortChannel(1);
            var channel2 = new PortChannel(2);

            channel1.Description = "First Channel";
            channel1.IsUp = true;
            channel1.Mode = "active";
            channel1.Protocol = "lacp";
            channel1.MemberPorts.Add("Gi0/1");

            channel2.Description = "Second Channel";
            channel2.IsUp = false;
            channel2.Mode = "passive";
            channel2.Protocol = "pagp";
            channel2.MemberPorts.Add("Gi0/3");

            // Assert
            Assert.Equal(1, channel1.Id);
            Assert.Equal(2, channel2.Id);
            Assert.Equal("First Channel", channel1.Description);
            Assert.Equal("Second Channel", channel2.Description);
            Assert.True(channel1.IsUp);
            Assert.False(channel2.IsUp);
            Assert.Equal("active", channel1.Mode);
            Assert.Equal("passive", channel2.Mode);
            Assert.Equal("lacp", channel1.Protocol);
            Assert.Equal("pagp", channel2.Protocol);
            Assert.Single(channel1.MemberPorts);
            Assert.Single(channel2.MemberPorts);
            Assert.Contains("Gi0/1", channel1.MemberPorts);
            Assert.Contains("Gi0/3", channel2.MemberPorts);
        }

        [Fact]
        public void PortChannel_MemberCollections_AreInitializedAsEmptyLists()
        {
            // Arrange & Act
            var portChannel = new PortChannel(1);

            // Assert
            Assert.NotNull(portChannel.MemberPorts);
            Assert.IsType<List<string>>(portChannel.MemberPorts);
            Assert.Empty(portChannel.MemberPorts);
            
            Assert.NotNull(portChannel.MemberInterfaces);
            Assert.IsType<List<string>>(portChannel.MemberInterfaces);
            Assert.Empty(portChannel.MemberInterfaces);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Port Channel 1")]
        [InlineData("LAG Group")]
        [InlineData("Trunk 1-2")]
        [InlineData(null)]
        public void PortChannel_Description_AcceptsAnyString(string description)
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.Description = description;

            // Assert
            Assert.Equal(description, portChannel.Description);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("unknown")]
        [InlineData("custom-mode")]
        public void PortChannel_Mode_AcceptsAnyString(string mode)
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.Mode = mode;

            // Assert
            Assert.Equal(mode, portChannel.Mode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("unknown")]
        [InlineData("custom-protocol")]
        public void PortChannel_Protocol_AcceptsAnyString(string protocol)
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.Protocol = protocol;

            // Assert
            Assert.Equal(protocol, portChannel.Protocol);
        }

        [Fact]
        public void PortChannel_AllProperties_CanBeSetSimultaneously()
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.Id = 99;
            portChannel.Description = "Test Channel";
            portChannel.IsUp = false;
            portChannel.Mode = "passive";
            portChannel.Protocol = "pagp";
            portChannel.MemberPorts.Add("Gi0/1");
            portChannel.MemberPorts.Add("Gi0/2");
            portChannel.MemberInterfaces.Add("GigabitEthernet0/1");
            portChannel.MemberInterfaces.Add("GigabitEthernet0/2");

            // Assert
            Assert.Equal(99, portChannel.Id);
            Assert.Equal("Test Channel", portChannel.Description);
            Assert.False(portChannel.IsUp);
            Assert.Equal("passive", portChannel.Mode);
            Assert.Equal("pagp", portChannel.Protocol);
            Assert.Equal(2, portChannel.MemberPorts.Count);
            Assert.Equal(2, portChannel.MemberInterfaces.Count);
            Assert.Contains("Gi0/1", portChannel.MemberPorts);
            Assert.Contains("Gi0/2", portChannel.MemberPorts);
            Assert.Contains("GigabitEthernet0/1", portChannel.MemberInterfaces);
            Assert.Contains("GigabitEthernet0/2", portChannel.MemberInterfaces);
        }

        [Fact]
        public void PortChannel_ModificationAfterCreation_ShouldWork()
        {
            // Arrange
            var portChannel = new PortChannel(5);
            var originalId = portChannel.Id;
            var originalDescription = portChannel.Description;
            var originalIsUp = portChannel.IsUp;
            var originalMode = portChannel.Mode;
            var originalProtocol = portChannel.Protocol;

            // Act
            portChannel.Id = originalId + 10;
            portChannel.Description = "Modified Description";
            portChannel.IsUp = !originalIsUp;
            portChannel.Mode = "passive";
            portChannel.Protocol = "pagp";

            // Assert
            Assert.NotEqual(originalId, portChannel.Id);
            Assert.NotEqual(originalDescription, portChannel.Description);
            Assert.NotEqual(originalIsUp, portChannel.IsUp);
            Assert.NotEqual(originalMode, portChannel.Mode);
            Assert.NotEqual(originalProtocol, portChannel.Protocol);
        }

        [Fact]
        public void PortChannel_MixedPortTypes_ShouldWork()
        {
            // Arrange
            var portChannel = new PortChannel(1);

            // Act
            portChannel.MemberPorts.Add("Gi0/1");
            portChannel.MemberPorts.Add("Fa0/1");
            portChannel.MemberPorts.Add("Te0/1");
            
            portChannel.MemberInterfaces.Add("GigabitEthernet0/1");
            portChannel.MemberInterfaces.Add("FastEthernet0/1");
            portChannel.MemberInterfaces.Add("TenGigabitEthernet0/1");

            // Assert
            Assert.Equal(3, portChannel.MemberPorts.Count);
            Assert.Equal(3, portChannel.MemberInterfaces.Count);
            Assert.Contains("Gi0/1", portChannel.MemberPorts);
            Assert.Contains("Fa0/1", portChannel.MemberPorts);
            Assert.Contains("Te0/1", portChannel.MemberPorts);
            Assert.Contains("GigabitEthernet0/1", portChannel.MemberInterfaces);
            Assert.Contains("FastEthernet0/1", portChannel.MemberInterfaces);
            Assert.Contains("TenGigabitEthernet0/1", portChannel.MemberInterfaces);
        }
    }
} 