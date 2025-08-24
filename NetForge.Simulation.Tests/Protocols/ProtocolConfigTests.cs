using NetForge.Simulation.Common.Protocols;
using Xunit;

namespace NetForge.Simulation.Tests.Protocols
{
    public class RipConfigTests
    {
        [Fact]
        public void RipConfig_DefaultConstructor_ShouldSetDefaultValues()
        {
            // Act
            var config = new RipConfig();

            // Assert
            Assert.Equal(2, config.Version);
            Assert.NotNull(config.Networks);
            Assert.Empty(config.Networks);
            Assert.NotNull(config.Neighbors);
            Assert.Empty(config.Neighbors);
            Assert.False(config.AutoSummary);
            Assert.False(config.Summary);
            Assert.NotNull(config.Groups);
            Assert.Empty(config.Groups);
            Assert.Equal("none", config.AuthenticationType);
            Assert.True(config.IsEnabled);
        }

        [Fact]
        public void RipConfig_ConstructorWithVersion_ShouldSetVersion()
        {
            // Act
            var config = new RipConfig(1);

            // Assert
            Assert.Equal(1, config.Version);
            Assert.True(config.IsEnabled);
        }

        [Fact]
        public void RipConfig_Version_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new RipConfig();

            // Act
            config.Version = 1;

            // Assert
            Assert.Equal(1, config.Version);
        }

        [Fact]
        public void RipConfig_Networks_CanBeAdded()
        {
            // Arrange
            var config = new RipConfig();

            // Act
            config.Networks.Add("192.168.1.0");
            config.Networks.Add("10.0.0.0");

            // Assert
            Assert.Equal(2, config.Networks.Count);
            Assert.Contains("192.168.1.0", config.Networks);
            Assert.Contains("10.0.0.0", config.Networks);
        }

        [Fact]
        public void RipConfig_Neighbors_CanBeAdded()
        {
            // Arrange
            var config = new RipConfig();
            var neighbor = new RipNeighbor("192.168.1.1", "Gi0/0");

            // Act
            config.Neighbors.Add(neighbor);

            // Assert
            Assert.Single(config.Neighbors);
            Assert.Equal("192.168.1.1", config.Neighbors[0].IpAddress);
            Assert.Equal("Gi0/0", config.Neighbors[0].Interface);
        }

        [Fact]
        public void RipConfig_Groups_CanBeAdded()
        {
            // Arrange
            var config = new RipConfig();
            var group = new RipGroup("test-group");

            // Act
            config.Groups.Add("test-group", group);

            // Assert
            Assert.Single(config.Groups);
            Assert.Equal("test-group", config.Groups["test-group"].Name);
        }

        [Fact]
        public void RipConfig_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new RipConfig();

            // Act
            config.AutoSummary = true;
            config.Summary = true;
            config.AuthenticationType = "md5";
            config.IsEnabled = false;

            // Assert
            Assert.True(config.AutoSummary);
            Assert.True(config.Summary);
            Assert.Equal("md5", config.AuthenticationType);
            Assert.False(config.IsEnabled);
        }
    }

    public class RipGroupTests
    {
        [Fact]
        public void RipGroup_Constructor_ShouldSetName()
        {
            // Act
            var group = new RipGroup("test-group");

            // Assert
            Assert.Equal("test-group", group.Name);
            Assert.NotNull(group.Members);
            Assert.Empty(group.Members);
            Assert.NotNull(group.ImportPolicies);
            Assert.Empty(group.ImportPolicies);
            Assert.NotNull(group.ExportPolicies);
            Assert.Empty(group.ExportPolicies);
            Assert.NotNull(group.Neighbors);
            Assert.Empty(group.Neighbors);
        }

        [Fact]
        public void RipGroup_Collections_CanBePopulated()
        {
            // Arrange
            var group = new RipGroup("test-group");

            // Act
            group.Members.Add("member1");
            group.ImportPolicies.Add("import-policy1");
            group.ExportPolicies.Add("export-policy1");
            group.Neighbors.Add("192.168.1.1");

            // Assert
            Assert.Single(group.Members);
            Assert.Single(group.ImportPolicies);
            Assert.Single(group.ExportPolicies);
            Assert.Single(group.Neighbors);
            Assert.Equal("member1", group.Members[0]);
            Assert.Equal("import-policy1", group.ImportPolicies[0]);
            Assert.Equal("export-policy1", group.ExportPolicies[0]);
            Assert.Equal("192.168.1.1", group.Neighbors[0]);
        }
    }

    public class RipNeighborTests
    {
        [Fact]
        public void RipNeighbor_Constructor_ShouldSetIpAddressAndInterface()
        {
            // Act
            var neighbor = new RipNeighbor("192.168.1.1", "Gi0/0");

            // Assert
            Assert.Equal("192.168.1.1", neighbor.IpAddress);
            Assert.Equal("Gi0/0", neighbor.Interface);
        }

        [Fact]
        public void RipNeighbor_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var neighbor = new RipNeighbor("192.168.1.1", "Gi0/0");

            // Act & Assert - Properties are settable
            Assert.Equal("192.168.1.1", neighbor.IpAddress);
            Assert.Equal("Gi0/0", neighbor.Interface);
        }
    }

    public class IgrpConfigTests
    {
        [Fact]
        public void IgrpConfig_Constructor_ShouldSetAsNumberAndDefaults()
        {
            // Act
            var config = new IgrpConfig(100);

            // Assert
            Assert.Equal(100, config.AsNumber);
            Assert.Equal("", config.RouterId);
            Assert.NotNull(config.Networks);
            Assert.Empty(config.Networks);
            Assert.True(config.AutoSummary);
            Assert.NotNull(config.Neighbors);
            Assert.Empty(config.Neighbors);
            Assert.NotNull(config.Metrics);
            Assert.Empty(config.Metrics);
            Assert.NotNull(config.Redistribution);
            Assert.Empty(config.Redistribution);
            Assert.True(config.IsEnabled);
            Assert.Equal(1544, config.Bandwidth);
            Assert.Equal(20000, config.Delay);
            Assert.Equal(255, config.Reliability);
            Assert.Equal(1, config.Load);
            Assert.Equal(1500, config.Mtu);
        }

        [Fact]
        public void IgrpConfig_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new IgrpConfig(100);

            // Act
            config.RouterId = "1.1.1.1";
            config.AutoSummary = false;
            config.IsEnabled = false;
            config.Bandwidth = 10000;
            config.Delay = 5000;
            config.Reliability = 200;
            config.Load = 5;
            config.Mtu = 9000;

            // Assert
            Assert.Equal("1.1.1.1", config.RouterId);
            Assert.False(config.AutoSummary);
            Assert.False(config.IsEnabled);
            Assert.Equal(10000, config.Bandwidth);
            Assert.Equal(5000, config.Delay);
            Assert.Equal(200, config.Reliability);
            Assert.Equal(5, config.Load);
            Assert.Equal(9000, config.Mtu);
        }

        [Fact]
        public void IgrpConfig_Collections_CanBePopulated()
        {
            // Arrange
            var config = new IgrpConfig(100);
            var neighbor = new IgrpNeighbor("192.168.1.1", 100, "Gi0/0");

            // Act
            config.Networks.Add("192.168.1.0");
            config.Neighbors.Add(neighbor);
            config.Metrics.Add("delay", 1000);
            config.Redistribution.Add("ospf");

            // Assert
            Assert.Single(config.Networks);
            Assert.Single(config.Neighbors);
            Assert.Single(config.Metrics);
            Assert.Single(config.Redistribution);
            Assert.Equal("192.168.1.0", config.Networks[0]);
            Assert.Equal("192.168.1.1", config.Neighbors[0].IpAddress);
            Assert.Equal(1000, config.Metrics["delay"]);
            Assert.Equal("ospf", config.Redistribution[0]);
        }
    }

    public class IgrpNeighborTests
    {
        [Fact]
        public void IgrpNeighbor_Constructor_ShouldSetRequiredProperties()
        {
            // Act
            var neighbor = new IgrpNeighbor("192.168.1.1", 100, "Gi0/0");

            // Assert
            Assert.Equal("192.168.1.1", neighbor.IpAddress);
            Assert.Equal(100, neighbor.AsNumber);
            Assert.Equal("Gi0/0", neighbor.Interface);
            Assert.Equal("Up", neighbor.State);
            Assert.Equal(0, neighbor.Metric);
            Assert.Equal(1544, neighbor.Bandwidth);
            Assert.Equal(20000, neighbor.Delay);
            Assert.Equal(255, neighbor.Reliability);
            Assert.Equal(1, neighbor.Load);
            Assert.Equal(1500, neighbor.Mtu);
        }

        [Fact]
        public void IgrpNeighbor_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var neighbor = new IgrpNeighbor("192.168.1.1", 100, "Gi0/0");

            // Act
            neighbor.State = "Down";
            neighbor.Metric = 1000;
            neighbor.Bandwidth = 100000;
            neighbor.Delay = 1000;
            neighbor.Reliability = 200;
            neighbor.Load = 10;
            neighbor.Mtu = 9000;

            // Assert
            Assert.Equal("Down", neighbor.State);
            Assert.Equal(1000, neighbor.Metric);
            Assert.Equal(100000, neighbor.Bandwidth);
            Assert.Equal(1000, neighbor.Delay);
            Assert.Equal(200, neighbor.Reliability);
            Assert.Equal(10, neighbor.Load);
            Assert.Equal(9000, neighbor.Mtu);
        }
    }

    public class IsIsConfigTests
    {
        [Fact]
        public void IsIsConfig_DefaultConstructor_ShouldSetDefaultValues()
        {
            // Act
            var config = new IsIsConfig();

            // Assert
            Assert.Equal("", config.NetworkEntity);
            Assert.Equal("level-2", config.IsType);
            Assert.Equal("level-2", config.IsLevel);
            Assert.Equal("level-2", config.LevelCapability);
            Assert.NotNull(config.Interfaces);
            Assert.Empty(config.Interfaces);
            Assert.NotNull(config.Areas);
            Assert.Empty(config.Areas);
            Assert.True(config.IsEnabled);
            Assert.NotNull(config.PassiveInterfaces);
            Assert.Empty(config.PassiveInterfaces);
            Assert.False(config.Level1Enabled);
        }

        [Fact]
        public void IsIsConfig_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var config = new IsIsConfig();

            // Act
            config.NetworkEntity = "49.0001.1921.6800.1001.00";
            config.IsType = "level-1";
            config.IsLevel = "level-1";
            config.LevelCapability = "level-1";
            config.IsEnabled = false;
            config.Level1Enabled = true;

            // Assert
            Assert.Equal("49.0001.1921.6800.1001.00", config.NetworkEntity);
            Assert.Equal("level-1", config.IsType);
            Assert.Equal("level-1", config.IsLevel);
            Assert.Equal("level-1", config.LevelCapability);
            Assert.False(config.IsEnabled);
            Assert.True(config.Level1Enabled);
        }

        [Fact]
        public void IsIsConfig_Collections_CanBePopulated()
        {
            // Arrange
            var config = new IsIsConfig();
            var interface1 = new IsIsInterface("Gi0/0");

            // Act
            config.Interfaces.Add("Gi0/0", interface1);
            config.Areas.Add("49.0001");
            config.PassiveInterfaces.Add("Gi0/1");

            // Assert
            Assert.Single(config.Interfaces);
            Assert.Single(config.Areas);
            Assert.Single(config.PassiveInterfaces);
            Assert.Equal("Gi0/0", config.Interfaces["Gi0/0"].Name);
            Assert.Equal("49.0001", config.Areas[0]);
            Assert.Equal("Gi0/1", config.PassiveInterfaces[0]);
        }
    }

    public class IsIsInterfaceTests
    {
        [Fact]
        public void IsIsInterface_Constructor_ShouldSetNameAndDefaults()
        {
            // Act
            var interface1 = new IsIsInterface("Gi0/0");

            // Assert
            Assert.Equal("Gi0/0", interface1.Name);
            Assert.Equal("point-to-point", interface1.Type);
            Assert.False(interface1.Passive);
            Assert.Equal(64, interface1.Priority);
        }

        [Fact]
        public void IsIsInterface_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var interface1 = new IsIsInterface("Gi0/0");

            // Act
            interface1.Type = "broadcast";
            interface1.Passive = true;
            interface1.Priority = 100;

            // Assert
            Assert.Equal("broadcast", interface1.Type);
            Assert.True(interface1.Passive);
            Assert.Equal(100, interface1.Priority);
        }
    }
}
