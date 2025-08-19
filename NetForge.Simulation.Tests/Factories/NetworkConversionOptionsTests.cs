using NetForge.Simulation.Factories;
using Xunit;

namespace NetForge.Simulation.Tests.Factories
{
    public class NetworkConversionOptionsTests
    {
        [Fact]
        public void NetworkConversionOptions_DefaultConstructor_ShouldSetDefaultValues()
        {
            // Act
            var options = new NetworkConversionOptions();

            // Assert
            Assert.True(options.ApplyNvramConfiguration);
            Assert.True(options.ConfigureDefaultInterfaceSettings);
            Assert.True(options.EnableProtocolInitialization);
            Assert.True(options.EnableOspf);
            Assert.True(options.EnableBgp);
            Assert.True(options.EnableRip);
            Assert.True(options.UpdateConnectedRoutes);
        }

        [Fact]
        public void NetworkConversionOptions_ApplyNvramConfiguration_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new NetworkConversionOptions();

            // Act
            options.ApplyNvramConfiguration = false;

            // Assert
            Assert.False(options.ApplyNvramConfiguration);
        }

        [Fact]
        public void NetworkConversionOptions_ConfigureDefaultInterfaceSettings_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new NetworkConversionOptions();

            // Act
            options.ConfigureDefaultInterfaceSettings = false;

            // Assert
            Assert.False(options.ConfigureDefaultInterfaceSettings);
        }

        [Fact]
        public void NetworkConversionOptions_EnableProtocolInitialization_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new NetworkConversionOptions();

            // Act
            options.EnableProtocolInitialization = false;

            // Assert
            Assert.False(options.EnableProtocolInitialization);
        }

        [Fact]
        public void NetworkConversionOptions_EnableOspf_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new NetworkConversionOptions();

            // Act
            options.EnableOspf = false;

            // Assert
            Assert.False(options.EnableOspf);
        }

        [Fact]
        public void NetworkConversionOptions_EnableBgp_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new NetworkConversionOptions();

            // Act
            options.EnableBgp = false;

            // Assert
            Assert.False(options.EnableBgp);
        }

        [Fact]
        public void NetworkConversionOptions_EnableRip_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new NetworkConversionOptions();

            // Act
            options.EnableRip = false;

            // Assert
            Assert.False(options.EnableRip);
        }

        [Fact]
        public void NetworkConversionOptions_UpdateConnectedRoutes_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new NetworkConversionOptions();

            // Act
            options.UpdateConnectedRoutes = false;

            // Assert
            Assert.False(options.UpdateConnectedRoutes);
        }

        [Fact]
        public void NetworkConversionOptions_CreateForTopologyImport_ShouldReturnCorrectConfiguration()
        {
            // Act
            var options = NetworkConversionOptions.CreateForTopologyImport();

            // Assert
            Assert.True(options.ApplyNvramConfiguration);
            Assert.True(options.ConfigureDefaultInterfaceSettings);
            Assert.False(options.EnableProtocolInitialization); // Let NVRAM config handle protocols
            Assert.True(options.UpdateConnectedRoutes);
            Assert.True(options.ValidateConfigurations);
            Assert.True(options.PreserveOriginalIds);
            Assert.True(options.GenerateMissingMacAddresses);
            Assert.True(options.EstablishConnectionsAsOperational);
            Assert.True(options.VerboseLogging);
        }

        [Fact]
        public void NetworkConversionOptions_AllPropertiesCanBeSetToTrue()
        {
            // Arrange
            var options = new NetworkConversionOptions
            {
                ApplyNvramConfiguration = true,
                ConfigureDefaultInterfaceSettings = true,
                EnableProtocolInitialization = true,
                EnableOspf = true,
                EnableBgp = true,
                EnableRip = true,
                UpdateConnectedRoutes = true
            };

            // Assert
            Assert.True(options.ApplyNvramConfiguration);
            Assert.True(options.ConfigureDefaultInterfaceSettings);
            Assert.True(options.EnableProtocolInitialization);
            Assert.True(options.EnableOspf);
            Assert.True(options.EnableBgp);
            Assert.True(options.EnableRip);
            Assert.True(options.UpdateConnectedRoutes);
        }

        [Fact]
        public void NetworkConversionOptions_AllPropertiesCanBeSetToFalse()
        {
            // Arrange
            var options = new NetworkConversionOptions
            {
                ApplyNvramConfiguration = false,
                ConfigureDefaultInterfaceSettings = false,
                EnableProtocolInitialization = false,
                EnableOspf = false,
                EnableBgp = false,
                EnableRip = false,
                UpdateConnectedRoutes = false
            };

            // Assert
            Assert.False(options.ApplyNvramConfiguration);
            Assert.False(options.ConfigureDefaultInterfaceSettings);
            Assert.False(options.EnableProtocolInitialization);
            Assert.False(options.EnableOspf);
            Assert.False(options.EnableBgp);
            Assert.False(options.EnableRip);
            Assert.False(options.UpdateConnectedRoutes);
        }

        [Fact]
        public void NetworkConversionOptions_ProtocolFlags_WorkIndependently()
        {
            // Arrange
            var options = new NetworkConversionOptions
            {
                EnableOspf = true,
                EnableBgp = false,
                EnableRip = true
            };

            // Assert
            Assert.True(options.EnableOspf);
            Assert.False(options.EnableBgp);
            Assert.True(options.EnableRip);
        }

        [Fact]
        public void NetworkConversionOptions_ProtocolInitializationDisabled_ProtocolFlagsStillWork()
        {
            // Arrange
            var options = new NetworkConversionOptions
            {
                EnableProtocolInitialization = false,
                EnableOspf = true,
                EnableBgp = true,
                EnableRip = true
            };

            // Assert
            Assert.False(options.EnableProtocolInitialization);
            Assert.True(options.EnableOspf);
            Assert.True(options.EnableBgp);
            Assert.True(options.EnableRip);
        }

        [Fact]
        public void NetworkConversionOptions_ConfigurationOptions_WorkIndependently()
        {
            // Arrange
            var options = new NetworkConversionOptions
            {
                ApplyNvramConfiguration = false,
                ConfigureDefaultInterfaceSettings = true,
                UpdateConnectedRoutes = false
            };

            // Assert
            Assert.False(options.ApplyNvramConfiguration);
            Assert.True(options.ConfigureDefaultInterfaceSettings);
            Assert.False(options.UpdateConnectedRoutes);
        }

        [Fact]
        public void NetworkConversionOptions_CreateForTopologyImport_ShouldCreateNewInstance()
        {
            // Act
            var options1 = NetworkConversionOptions.CreateForTopologyImport();
            var options2 = NetworkConversionOptions.CreateForTopologyImport();

            // Assert
            Assert.NotSame(options1, options2);
            
            // Modify one instance and verify the other is unaffected
            options1.EnableOspf = false;
            Assert.False(options1.EnableOspf);
            Assert.True(options2.EnableOspf);
        }

        [Theory]
        [InlineData(true, true, true, true, true, true, true)]
        [InlineData(false, false, false, false, false, false, false)]
        [InlineData(true, false, true, false, true, false, true)]
        [InlineData(false, true, false, true, false, true, false)]
        public void NetworkConversionOptions_VariousConfigurations_ShouldWorkCorrectly(
            bool applyNvram, bool configureInterfaces, bool enableProtocols,
            bool enableOspf, bool enableBgp, bool enableRip, bool updateRoutes)
        {
            // Arrange & Act
            var options = new NetworkConversionOptions
            {
                ApplyNvramConfiguration = applyNvram,
                ConfigureDefaultInterfaceSettings = configureInterfaces,
                EnableProtocolInitialization = enableProtocols,
                EnableOspf = enableOspf,
                EnableBgp = enableBgp,
                EnableRip = enableRip,
                UpdateConnectedRoutes = updateRoutes
            };

            // Assert
            Assert.Equal(applyNvram, options.ApplyNvramConfiguration);
            Assert.Equal(configureInterfaces, options.ConfigureDefaultInterfaceSettings);
            Assert.Equal(enableProtocols, options.EnableProtocolInitialization);
            Assert.Equal(enableOspf, options.EnableOspf);
            Assert.Equal(enableBgp, options.EnableBgp);
            Assert.Equal(enableRip, options.EnableRip);
            Assert.Equal(updateRoutes, options.UpdateConnectedRoutes);
        }
    }
} 
