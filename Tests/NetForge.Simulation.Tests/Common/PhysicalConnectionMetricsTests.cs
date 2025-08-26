using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using Xunit;

namespace NetForge.Simulation.Tests.Common
{
    public class PhysicalConnectionMetricsTests
    {
        [Fact]
        public void QualityScore_ConnectedStateWithPerfectMetrics_ShouldReturn100()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Connected,
                PacketLoss = 0.0,
                Latency = 1,
                ErrorCount = 0
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(100.0, score);
        }

        [Fact]
        public void QualityScore_DegradedStateWithPerfectMetrics_ShouldReturn100()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Degraded,
                PacketLoss = 0.0,
                Latency = 1,
                ErrorCount = 0
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(100.0, score);
        }

        [Fact]
        public void QualityScore_DisconnectedState_ShouldReturn0()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Disconnected,
                PacketLoss = 0.0,
                Latency = 1,
                ErrorCount = 0
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(0.0, score);
        }

        [Fact]
        public void QualityScore_FailedState_ShouldReturn0()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Failed,
                PacketLoss = 0.0,
                Latency = 1,
                ErrorCount = 0
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(0.0, score);
        }

        [Fact]
        public void QualityScore_WithPacketLoss_ShouldReduceScore()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Connected,
                PacketLoss = 5.0, // 5% packet loss
                Latency = 1,
                ErrorCount = 0
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(90.0, score); // 100 - (5 * 2) = 90
        }

        [Fact]
        public void QualityScore_WithHighLatency_ShouldReduceScore()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Connected,
                PacketLoss = 0.0,
                Latency = 21, // 20ms above baseline
                ErrorCount = 0
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(90.0, score); // 100 - (20 * 0.5) = 90
        }

        [Fact]
        public void QualityScore_WithErrors_ShouldReduceScore()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Connected,
                PacketLoss = 0.0,
                Latency = 1,
                ErrorCount = 50 // Should reduce by 5%
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(95.0, score); // 100 - min(50 * 0.1, 10) = 95
        }

        [Fact]
        public void QualityScore_WithManyErrors_ShouldCapAt10PercentReduction()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Connected,
                PacketLoss = 0.0,
                Latency = 1,
                ErrorCount = 200 // Should reduce by max 10%
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(90.0, score); // 100 - 10 = 90 (capped)
        }

        [Fact]
        public void QualityScore_WithCombinedDegradation_ShouldAccumulateReductions()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Connected,
                PacketLoss = 10.0, // -20 points
                Latency = 41,      // -20 points (40ms above baseline)
                ErrorCount = 100   // -10 points (capped)
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(50.0, score); // 100 - 20 - 20 - 10 = 50
        }

        [Fact]
        public void QualityScore_CannotGoBelowZero()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Connected,
                PacketLoss = 80.0, // -160 points
                Latency = 1000,    // -499.5 points
                ErrorCount = 1000  // -10 points (capped)
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(0.0, score); // Cannot go below 0
        }

        [Fact]
        public void PhysicalConnectionMetrics_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var metrics = new PhysicalConnectionMetrics();

            // Assert
            Assert.Equal("", metrics.ConnectionId);
            Assert.Equal(PhysicalConnectionState.Connected, metrics.State);
            Assert.Equal(PhysicalConnectionType.Ethernet, metrics.ConnectionType);
            Assert.Equal(0, metrics.Bandwidth);
            Assert.Equal(0, metrics.Latency);
            Assert.Equal(0.0, metrics.PacketLoss);
            Assert.Equal(0, metrics.MaxTransmissionUnit);
            Assert.Equal(0, metrics.ErrorCount);
        }

        [Fact]
        public void PhysicalConnectionMetrics_PropertyAssignment_ShouldWork()
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics();

            // Act
            metrics.ConnectionId = "test-connection";
            metrics.State = PhysicalConnectionState.Degraded;
            metrics.ConnectionType = PhysicalConnectionType.Fiber;
            metrics.Bandwidth = 1000;
            metrics.Latency = 5;
            metrics.PacketLoss = 1.5;
            metrics.MaxTransmissionUnit = 1500;
            metrics.ErrorCount = 3;

            // Assert
            Assert.Equal("test-connection", metrics.ConnectionId);
            Assert.Equal(PhysicalConnectionState.Degraded, metrics.State);
            Assert.Equal(PhysicalConnectionType.Fiber, metrics.ConnectionType);
            Assert.Equal(1000, metrics.Bandwidth);
            Assert.Equal(5, metrics.Latency);
            Assert.Equal(1.5, metrics.PacketLoss);
            Assert.Equal(1500, metrics.MaxTransmissionUnit);
            Assert.Equal(3, metrics.ErrorCount);
        }

        [Theory]
        [InlineData(0.0, 1, 0, 100.0)]
        [InlineData(1.0, 1, 0, 98.0)]
        [InlineData(0.0, 11, 0, 95.0)]
        [InlineData(0.0, 1, 10, 99.0)]
        [InlineData(2.5, 6, 5, 92.0)]
        public void QualityScore_WithVariousInputs_ShouldCalculateCorrectly(
            double packetLoss, int latency, int errorCount, double expectedScore)
        {
            // Arrange
            var metrics = new PhysicalConnectionMetrics
            {
                State = PhysicalConnectionState.Connected,
                PacketLoss = packetLoss,
                Latency = latency,
                ErrorCount = errorCount
            };

            // Act
            var score = metrics.QualityScore;

            // Assert
            Assert.Equal(expectedScore, score);
        }
    }
}
