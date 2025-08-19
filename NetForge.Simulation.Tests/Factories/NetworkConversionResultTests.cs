using NetForge.Simulation.Common;
using NetForge.Simulation.Factories;
using Xunit;

namespace NetForge.Simulation.Tests.Factories
{
    public class NetworkConversionResultTests
    {
        [Fact]
        public void NetworkConversionResult_DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var result = new NetworkConversionResult();

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Network);
            Assert.Equal("", result.Summary);
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
            Assert.NotNull(result.Warnings);
            Assert.Empty(result.Warnings);
            Assert.NotNull(result.ConvertedDevices);
            Assert.Empty(result.ConvertedDevices);
            Assert.NotNull(result.ConvertedConnections);
            Assert.Empty(result.ConvertedConnections);
            Assert.NotNull(result.FailedDevices);
            Assert.Empty(result.FailedDevices);
            Assert.NotNull(result.FailedConnections);
            Assert.Empty(result.FailedConnections);
        }

        [Fact]
        public void NetworkConversionResult_Success_CanBeSetAndRetrieved()
        {
            // Arrange
            var result = new NetworkConversionResult();

            // Act
            result.Success = true;

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void NetworkConversionResult_Network_CanBeSetAndRetrieved()
        {
            // Arrange
            var result = new NetworkConversionResult();
            var network = new Network();

            // Act
            result.Network = network;

            // Assert
            Assert.Same(network, result.Network);
        }

        [Fact]
        public void NetworkConversionResult_Summary_CanBeSetAndRetrieved()
        {
            // Arrange
            var result = new NetworkConversionResult();
            var summary = "Conversion completed successfully";

            // Act
            result.Summary = summary;

            // Assert
            Assert.Equal(summary, result.Summary);
        }

        [Fact]
        public void NetworkConversionResult_Errors_CanBeAdded()
        {
            // Arrange
            var result = new NetworkConversionResult();
            var error1 = "Device not found";
            var error2 = "Invalid configuration";

            // Act
            result.Errors.Add(error1);
            result.Errors.Add(error2);

            // Assert
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(error1, result.Errors);
            Assert.Contains(error2, result.Errors);
        }

        [Fact]
        public void NetworkConversionResult_Warnings_CanBeAdded()
        {
            // Arrange
            var result = new NetworkConversionResult();
            var warning1 = "Default settings applied";
            var warning2 = "Interface configuration missing";

            // Act
            result.Warnings.Add(warning1);
            result.Warnings.Add(warning2);

            // Assert
            Assert.Equal(2, result.Warnings.Count);
            Assert.Contains(warning1, result.Warnings);
            Assert.Contains(warning2, result.Warnings);
        }

        [Fact]
        public void NetworkConversionResult_ConvertedDevices_CanBeAdded()
        {
            // Arrange
            var result = new NetworkConversionResult();
            var originalId1 = "dev1";
            var convertedName1 = "Router1";
            var originalId2 = "dev2";
            var convertedName2 = "Switch1";

            // Act
            result.ConvertedDevices.Add(originalId1, convertedName1);
            result.ConvertedDevices.Add(originalId2, convertedName2);

            // Assert
            Assert.Equal(2, result.ConvertedDevices.Count);
            Assert.Equal(convertedName1, result.ConvertedDevices[originalId1]);
            Assert.Equal(convertedName2, result.ConvertedDevices[originalId2]);
        }

        [Fact]
        public void NetworkConversionResult_ConvertedConnections_CanBeAdded()
        {
            // Arrange
            var result = new NetworkConversionResult();
            var connectionId1 = "conn1";
            var connectionId2 = "conn2";

            // Act
            result.ConvertedConnections.Add(connectionId1);
            result.ConvertedConnections.Add(connectionId2);

            // Assert
            Assert.Equal(2, result.ConvertedConnections.Count);
            Assert.Contains(connectionId1, result.ConvertedConnections);
            Assert.Contains(connectionId2, result.ConvertedConnections);
        }

        [Fact]
        public void NetworkConversionResult_FailedDevices_CanBeAdded()
        {
            // Arrange
            var result = new NetworkConversionResult();
            var deviceId1 = "dev1";
            var error1 = "Unsupported device type";
            var deviceId2 = "dev2";
            var error2 = "Configuration error";

            // Act
            result.FailedDevices.Add(deviceId1, error1);
            result.FailedDevices.Add(deviceId2, error2);

            // Assert
            Assert.Equal(2, result.FailedDevices.Count);
            Assert.Equal(error1, result.FailedDevices[deviceId1]);
            Assert.Equal(error2, result.FailedDevices[deviceId2]);
        }

        [Fact]
        public void NetworkConversionResult_FailedConnections_CanBeAdded()
        {
            // Arrange
            var result = new NetworkConversionResult();
            var connectionId1 = "conn1";
            var error1 = "Interface not found";
            var connectionId2 = "conn2";
            var error2 = "Incompatible connection type";

            // Act
            result.FailedConnections.Add(connectionId1, error1);
            result.FailedConnections.Add(connectionId2, error2);

            // Assert
            Assert.Equal(2, result.FailedConnections.Count);
            Assert.Equal(error1, result.FailedConnections[connectionId1]);
            Assert.Equal(error2, result.FailedConnections[connectionId2]);
        }

        [Fact]
        public void NetworkConversionResult_ComplexScenario_ShouldHandleAllCollections()
        {
            // Arrange
            var result = new NetworkConversionResult
            {
                Success = true,
                Network = new Network(),
                Summary = "Partially successful conversion"
            };

            // Act
            result.Errors.Add("Minor configuration issue");
            result.Warnings.Add("Using default settings");
            result.ConvertedDevices.Add("original1", "converted1");
            result.ConvertedConnections.Add("connection1");
            result.FailedDevices.Add("failed1", "Unsupported type");
            result.FailedConnections.Add("failed_conn1", "Invalid interface");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Network);
            Assert.Equal("Partially successful conversion", result.Summary);
            Assert.Single(result.Errors);
            Assert.Single(result.Warnings);
            Assert.Single(result.ConvertedDevices);
            Assert.Single(result.ConvertedConnections);
            Assert.Single(result.FailedDevices);
            Assert.Single(result.FailedConnections);
        }

        [Fact]
        public void NetworkConversionResult_CollectionProperties_ShouldBeIndependent()
        {
            // Arrange
            var result1 = new NetworkConversionResult();
            var result2 = new NetworkConversionResult();

            // Act
            result1.Errors.Add("Error in result1");
            result2.Errors.Add("Error in result2");

            // Assert
            Assert.Single(result1.Errors);
            Assert.Single(result2.Errors);
            Assert.NotEqual(result1.Errors[0], result2.Errors[0]);
        }

        [Fact]
        public void NetworkConversionResult_DefaultValues_ShouldNotBeNull()
        {
            // Arrange & Act
            var result = new NetworkConversionResult();

            // Assert - All collections should be initialized and not null
            Assert.NotNull(result.Errors);
            Assert.NotNull(result.Warnings);
            Assert.NotNull(result.ConvertedDevices);
            Assert.NotNull(result.ConvertedConnections);
            Assert.NotNull(result.FailedDevices);
            Assert.NotNull(result.FailedConnections);
            Assert.NotNull(result.Summary);
        }
    }
} 
