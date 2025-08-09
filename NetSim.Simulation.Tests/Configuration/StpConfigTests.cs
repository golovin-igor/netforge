using NetSim.Simulation.Configuration;
using Xunit;

namespace NetSim.Simulation.Tests.Configuration
{
    public class StpConfigTests
    {
        [Fact]
        public void StpConfig_DefaultConstructor_ShouldSetDefaultValues()
        {
            // Act
            var stpConfig = new StpConfig();

            // Assert
            Assert.True(stpConfig.IsEnabled);
            Assert.Equal(32768, stpConfig.DefaultPriority);
            Assert.Equal("mstp", stpConfig.Mode);
            Assert.Equal("", stpConfig.BridgeId);
            Assert.False(stpConfig.IsRoot);
            Assert.NotNull(stpConfig.VlanPriorities);
            Assert.Empty(stpConfig.VlanPriorities);
        }

        [Fact]
        public void StpConfig_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act & Assert
            stpConfig.IsEnabled = false;
            Assert.False(stpConfig.IsEnabled);

            stpConfig.DefaultPriority = 4096;
            Assert.Equal(4096, stpConfig.DefaultPriority);

            stpConfig.Mode = "rapid-pvst";
            Assert.Equal("rapid-pvst", stpConfig.Mode);

            stpConfig.BridgeId = "0050.56c0.0001";
            Assert.Equal("0050.56c0.0001", stpConfig.BridgeId);

            stpConfig.IsRoot = true;
            Assert.True(stpConfig.IsRoot);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StpConfig_IsEnabled_CanBeToggled(bool enabled)
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.IsEnabled = enabled;

            // Assert
            Assert.Equal(enabled, stpConfig.IsEnabled);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StpConfig_IsRoot_CanBeToggled(bool isRoot)
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.IsRoot = isRoot;

            // Assert
            Assert.Equal(isRoot, stpConfig.IsRoot);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4096)]
        [InlineData(8192)]
        [InlineData(16384)]
        [InlineData(32768)]
        [InlineData(61440)]
        public void StpConfig_DefaultPriority_AcceptsValidValues(int priority)
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.DefaultPriority = priority;

            // Assert
            Assert.Equal(priority, stpConfig.DefaultPriority);
        }

        [Theory]
        [InlineData("mstp")]
        [InlineData("pvst")]
        [InlineData("rapid-pvst")]
        [InlineData("rstp")]
        [InlineData("ieee")]
        public void StpConfig_Mode_AcceptsStandardModes(string mode)
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.Mode = mode;

            // Assert
            Assert.Equal(mode, stpConfig.Mode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("0050.56c0.0001")]
        [InlineData("00:50:56:C0:00:01")]
        [InlineData("0050.56c0.0001.8000")]
        [InlineData("auto")]
        public void StpConfig_BridgeId_AcceptsVariousFormats(string bridgeId)
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.BridgeId = bridgeId;

            // Assert
            Assert.Equal(bridgeId, stpConfig.BridgeId);
        }

        [Fact]
        public void StpConfig_VlanPriorities_CanBeModified()
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.VlanPriorities[10] = 4096;
            stpConfig.VlanPriorities[20] = 8192;

            // Assert
            Assert.Equal(2, stpConfig.VlanPriorities.Count);
            Assert.Equal(4096, stpConfig.VlanPriorities[10]);
            Assert.Equal(8192, stpConfig.VlanPriorities[20]);
        }

        [Fact]
        public void StpConfig_GetPriority_WithDefaultVlan_ShouldReturnDefaultPriority()
        {
            // Arrange
            var stpConfig = new StpConfig();
            stpConfig.DefaultPriority = 16384;

            // Act
            var priority = stpConfig.GetPriority(1);

            // Assert
            Assert.Equal(16384, priority);
        }

        [Fact]
        public void StpConfig_GetPriority_WithSpecificVlan_ShouldReturnVlanPriority()
        {
            // Arrange
            var stpConfig = new StpConfig();
            stpConfig.VlanPriorities[100] = 4096;

            // Act
            var priority = stpConfig.GetPriority(100);

            // Assert
            Assert.Equal(4096, priority);
        }

        [Fact]
        public void StpConfig_SetPriority_ShouldAddVlanPriority()
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.SetPriority(50, 8192);

            // Assert
            Assert.True(stpConfig.VlanPriorities.ContainsKey(50));
            Assert.Equal(8192, stpConfig.VlanPriorities[50]);
            Assert.Equal(8192, stpConfig.GetPriority(50));
        }

        [Fact]
        public void StpConfig_SetPriority_ShouldOverrideExistingVlanPriority()
        {
            // Arrange
            var stpConfig = new StpConfig();
            stpConfig.SetPriority(10, 4096);

            // Act
            stpConfig.SetPriority(10, 16384);

            // Assert
            Assert.Equal(16384, stpConfig.VlanPriorities[10]);
            Assert.Equal(16384, stpConfig.GetPriority(10));
        }

        [Fact]
        public void StpConfig_GetPriority_WithMultipleVlans_ShouldReturnCorrectPriorities()
        {
            // Arrange
            var stpConfig = new StpConfig();
            stpConfig.DefaultPriority = 32768;
            stpConfig.SetPriority(10, 4096);
            stpConfig.SetPriority(20, 8192);

            // Act & Assert
            Assert.Equal(4096, stpConfig.GetPriority(10));
            Assert.Equal(8192, stpConfig.GetPriority(20));
            Assert.Equal(32768, stpConfig.GetPriority(30)); // Non-configured VLAN
        }

        [Fact]
        public void StpConfig_VlanPriorities_IsInitializedAsEmptyDictionary()
        {
            // Arrange & Act
            var stpConfig = new StpConfig();

            // Assert
            Assert.NotNull(stpConfig.VlanPriorities);
            Assert.IsType<Dictionary<int, int>>(stpConfig.VlanPriorities);
            Assert.Empty(stpConfig.VlanPriorities);
        }

        [Fact]
        public void StpConfig_MultipleInstances_ShouldBeIndependent()
        {
            // Arrange & Act
            var config1 = new StpConfig
            {
                IsEnabled = true,
                DefaultPriority = 4096,
                Mode = "rapid-pvst",
                BridgeId = "0050.56c0.0001",
                IsRoot = true
            };

            var config2 = new StpConfig
            {
                IsEnabled = false,
                DefaultPriority = 8192,
                Mode = "pvst",
                BridgeId = "0050.56c0.0002",
                IsRoot = false
            };

            config1.SetPriority(10, 2048);
            config2.SetPriority(20, 16384);

            // Assert
            Assert.True(config1.IsEnabled);
            Assert.False(config2.IsEnabled);
            Assert.Equal(4096, config1.DefaultPriority);
            Assert.Equal(8192, config2.DefaultPriority);
            Assert.Equal("rapid-pvst", config1.Mode);
            Assert.Equal("pvst", config2.Mode);
            Assert.True(config1.IsRoot);
            Assert.False(config2.IsRoot);
            Assert.Equal(2048, config1.GetPriority(10));
            Assert.Equal(16384, config2.GetPriority(20));
            Assert.Equal(4096, config1.GetPriority(20)); // Default priority for config1
            Assert.Equal(8192, config2.GetPriority(10)); // Default priority for config2
        }

        [Theory]
        [InlineData(1, 4096)]
        [InlineData(100, 8192)]
        [InlineData(4094, 16384)]
        [InlineData(0, 32768)]
        [InlineData(-1, 61440)] // Should handle edge cases
        public void StpConfig_SetAndGetPriority_WithVariousVlanIds(int vlanId, int priority)
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.SetPriority(vlanId, priority);
            var retrievedPriority = stpConfig.GetPriority(vlanId);

            // Assert
            Assert.Equal(priority, retrievedPriority);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("custom-mode")]
        [InlineData("unknown")]
        public void StpConfig_Mode_AcceptsAnyString(string mode)
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.Mode = mode;

            // Assert
            Assert.Equal(mode, stpConfig.Mode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("custom-bridge-id")]
        public void StpConfig_BridgeId_AcceptsAnyString(string bridgeId)
        {
            // Arrange
            var stpConfig = new StpConfig();

            // Act
            stpConfig.BridgeId = bridgeId;

            // Assert
            Assert.Equal(bridgeId, stpConfig.BridgeId);
        }
    }
} 