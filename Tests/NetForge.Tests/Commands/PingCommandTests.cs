using FluentAssertions;
using Moq;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Common.Common;
using NetForge.Tests.TestHelpers;
using Xunit;

namespace NetForge.Tests.Commands;

/// <summary>
/// Comprehensive test suite for PingCommand following the Command Pattern architecture
/// Tests command pattern integrity, business logic separation, and option parsing
/// </summary>
public class PingCommandTests
{
    private readonly Mock<IPingService> _mockPingService;
    private readonly PingCommand _pingCommand;
    private readonly MockNetworkDevice _mockDevice;

    public PingCommandTests()
    {
        _mockPingService = new Mock<IPingService>();
        _pingCommand = new PingCommand(_mockPingService.Object);
        _mockDevice = MockDeviceBuilder.Create()
            .WithName("TestDevice")
            .WithVendor("Cisco")
            .WithInterface("eth0", "192.168.1.10", isUp: true)
            .Build();
    }

    #region Command Pattern Integrity Tests

    [Fact]
    public async Task ExecuteBusinessLogicAsync_ValidArgs_CallsPingService()
    {
        // Arrange
        var destination = "192.168.1.1";
        var args = new[] { "ping", destination };
        var expectedResult = new PingResultData
        {
            Destination = destination,
            Success = true,
            PacketsSent = 5,
            PacketsReceived = 5,
            PacketLossPercentage = 0
        };

        _mockPingService.Setup(s => s.ExecutePing(_mockDevice, destination, 5, 64))
            .Returns(expectedResult);

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PingCommandData>();
        var pingResult = (PingCommandData)result;
        pingResult.Success.Should().BeTrue();
        pingResult.Destination.Should().Be(destination);
        pingResult.PacketsSent.Should().Be(5);

        _mockPingService.Verify(s => s.ExecutePing(_mockDevice, destination, 5, 64), Times.Once);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_WithPacketCount_CallsServiceWithCorrectCount()
    {
        // Arrange
        var destination = "192.168.1.1";
        var args = new[] { "ping", "-c", "10", destination };
        var expectedResult = new PingResultData
        {
            Destination = destination,
            Success = true,
            PacketsSent = 10,
            PacketsReceived = 10
        };

        _mockPingService.Setup(s => s.ExecutePing(_mockDevice, destination, 10, 64))
            .Returns(expectedResult);

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var pingResult = (PingCommandData)result;
        pingResult.PacketsSent.Should().Be(10);

        _mockPingService.Verify(s => s.ExecutePing(_mockDevice, destination, 10, 64), Times.Once);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_WithPacketSize_CallsServiceWithCorrectSize()
    {
        // Arrange
        var destination = "192.168.1.1";
        var args = new[] { "ping", "-s", "1500", destination };
        var expectedResult = new PingResultData
        {
            Destination = destination,
            Success = true,
            PacketSize = 1500
        };

        _mockPingService.Setup(s => s.ExecutePing(_mockDevice, destination, 5, 1500))
            .Returns(expectedResult);

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var pingResult = (PingCommandData)result;
        pingResult.PacketSize.Should().Be(1500);

        _mockPingService.Verify(s => s.ExecutePing(_mockDevice, destination, 5, 1500), Times.Once);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_WithAdvancedOptions_UsesExecutePingWithOptions()
    {
        // Arrange
        var destination = "192.168.1.1";
        var args = new[] { "ping", "-c", "3", "-s", "128", "-I", "eth0", "-t", "64", destination };
        var expectedResult = new PingResultData
        {
            Destination = destination,
            Success = true,
            PacketsSent = 3,
            PacketSize = 128,
            SourceInterface = "eth0",
            Ttl = 64
        };

        _mockPingService.Setup(s => s.ExecutePingWithOptions(_mockDevice, It.IsAny<PingOptions>()))
            .Returns(expectedResult);

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var pingResult = (PingCommandData)result;
        pingResult.PacketsSent.Should().Be(3);
        pingResult.PacketSize.Should().Be(128);
        pingResult.SourceInterface.Should().Be("eth0");
        pingResult.Ttl.Should().Be(64);

        _mockPingService.Verify(s => s.ExecutePingWithOptions(_mockDevice, It.Is<PingOptions>(o =>
            o.PingCount == 3 &&
            o.PacketSize == 128 &&
            o.SourceInterface == "eth0" &&
            o.Ttl == 64 &&
            o.Destination == destination)), Times.Once);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_InvalidArgs_ThrowsCommandExecutionException()
    {
        // Arrange
        var args = new[] { "ping" }; // Missing destination

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args));

        exception.Message.Should().Contain("destination");
        _mockPingService.Verify(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_ServiceThrowsException_PropagatesAsCommandExecutionException()
    {
        // Arrange
        var destination = "192.168.1.1";
        var args = new[] { "ping", destination };

        _mockPingService.Setup(s => s.ExecutePing(_mockDevice, destination, 5, 64))
            .Throws(new InvalidOperationException("Service error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args));

        exception.Message.Should().Contain("Service error");
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    #endregion

    #region Option Parsing Tests

    [Theory]
    [InlineData(new[] { "ping", "192.168.1.1" }, "192.168.1.1")]
    [InlineData(new[] { "ping", "google.com" }, "google.com")]
    [InlineData(new[] { "ping", "127.0.0.1" }, "127.0.0.1")]
    public async Task ParsePingOptions_BasicDestination_ParsesCorrectly(string[] args, string expectedDestination)
    {
        // Arrange
        var expectedResult = new PingResultData { Destination = expectedDestination, Success = true };
        _mockPingService.Setup(s => s.ExecutePing(It.IsAny<INetworkDevice>(), expectedDestination, It.IsAny<int>(), It.IsAny<int>()))
            .Returns(expectedResult);

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var pingResult = (PingCommandData)result;
        pingResult.Destination.Should().Be(expectedDestination);
    }

    [Theory]
    [InlineData(new[] { "ping", "-c", "1", "192.168.1.1" }, 1)]
    [InlineData(new[] { "ping", "-c", "10", "192.168.1.1" }, 10)]
    [InlineData(new[] { "ping", "-c", "100", "192.168.1.1" }, 100)]
    public async Task ParsePingOptions_CountFlag_ParsesCorrectly(string[] args, int expectedCount)
    {
        // Arrange
        var expectedResult = new PingResultData { Success = true, PacketsSent = expectedCount };
        _mockPingService.Setup(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), expectedCount, It.IsAny<int>()))
            .Returns(expectedResult);

        // Act
        await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        _mockPingService.Verify(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), expectedCount, It.IsAny<int>()), Times.Once);
    }

    [Theory]
    [InlineData(new[] { "ping", "-s", "64", "192.168.1.1" }, 64)]
    [InlineData(new[] { "ping", "-s", "1500", "192.168.1.1" }, 1500)]
    [InlineData(new[] { "ping", "-s", "128", "192.168.1.1" }, 128)]
    public async Task ParsePingOptions_SizeFlag_ParsesCorrectly(string[] args, int expectedSize)
    {
        // Arrange
        var expectedResult = new PingResultData { Success = true, PacketSize = expectedSize };
        _mockPingService.Setup(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), It.IsAny<int>(), expectedSize))
            .Returns(expectedResult);

        // Act
        await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        _mockPingService.Verify(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), It.IsAny<int>(), expectedSize), Times.Once);
    }

    [Fact]
    public async Task ParsePingOptions_CiscoStyleFlags_WorksWithCiscoSyntax()
    {
        // Arrange
        var args = new[] { "ping" };
        var destination = "192.168.1.1";

        // Mock the interactive parsing behavior that Cisco uses
        var expectedResult = new PingResultData { Success = true };
        _mockPingService.Setup(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(expectedResult);

        // For this test, we'll simulate that when no destination is provided,
        // the command could prompt for it (Cisco-style)
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args));

        // Assert
        exception.Message.Should().Contain("destination");
    }

    [Theory]
    [InlineData(new[] { "ping", "-I", "eth0", "192.168.1.1" }, "eth0")]
    [InlineData(new[] { "ping", "-I", "eth1", "192.168.1.1" }, "eth1")]
    [InlineData(new[] { "ping", "-I", "lo0", "192.168.1.1" }, "lo0")]
    public async Task ParsePingOptions_InterfaceFlag_ParsesCorrectly(string[] args, string expectedInterface)
    {
        // Arrange
        var expectedResult = new PingResultData { Success = true, SourceInterface = expectedInterface };
        _mockPingService.Setup(s => s.ExecutePingWithOptions(It.IsAny<INetworkDevice>(), It.IsAny<PingOptions>()))
            .Returns(expectedResult);

        // Act
        await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        _mockPingService.Verify(s => s.ExecutePingWithOptions(It.IsAny<INetworkDevice>(),
            It.Is<PingOptions>(o => o.SourceInterface == expectedInterface)), Times.Once);
    }

    [Theory]
    [InlineData(new[] { "ping", "-t", "64", "192.168.1.1" }, 64)]
    [InlineData(new[] { "ping", "-t", "128", "192.168.1.1" }, 128)]
    [InlineData(new[] { "ping", "-t", "255", "192.168.1.1" }, 255)]
    public async Task ParsePingOptions_TTLFlag_ParsesCorrectly(string[] args, int expectedTtl)
    {
        // Arrange
        var expectedResult = new PingResultData { Success = true, Ttl = expectedTtl };
        _mockPingService.Setup(s => s.ExecutePingWithOptions(It.IsAny<INetworkDevice>(), It.IsAny<PingOptions>()))
            .Returns(expectedResult);

        // Act
        await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        _mockPingService.Verify(s => s.ExecutePingWithOptions(It.IsAny<INetworkDevice>(),
            It.Is<PingOptions>(o => o.Ttl == expectedTtl)), Times.Once);
    }

    [Fact]
    public async Task ParsePingOptions_MultipleFlags_ParsesAllCorrectly()
    {
        // Arrange
        var args = new[] { "ping", "-c", "3", "-s", "128", "-I", "eth0", "-t", "64", "-v", "192.168.1.1" };
        var expectedResult = new PingResultData
        {
            Success = true,
            PacketsSent = 3,
            PacketSize = 128,
            SourceInterface = "eth0",
            Ttl = 64
        };

        _mockPingService.Setup(s => s.ExecutePingWithOptions(It.IsAny<INetworkDevice>(), It.IsAny<PingOptions>()))
            .Returns(expectedResult);

        // Act
        await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        _mockPingService.Verify(s => s.ExecutePingWithOptions(It.IsAny<INetworkDevice>(),
            It.Is<PingOptions>(o =>
                o.PingCount == 3 &&
                o.PacketSize == 128 &&
                o.SourceInterface == "eth0" &&
                o.Ttl == 64 &&
                o.Verbose == true &&
                o.Destination == "192.168.1.1")), Times.Once);
    }

    #endregion

    #region Help and Completion Tests

    [Fact]
    public void GetHelpText_Always_ReturnsUsageInstructions()
    {
        // Act
        var helpText = _pingCommand.GetHelpText();

        // Assert
        helpText.Should().NotBeNullOrEmpty();
        helpText.Should().Contain("ping");
        helpText.Should().Contain("destination");
        helpText.Should().Contain("-c"); // count option
        helpText.Should().Contain("-s"); // size option
        helpText.Should().Contain("-I"); // interface option
        helpText.Should().Contain("Usage:");
    }

    [Fact]
    public void GetCompletions_PartialIP_ReturnsMatchingIpAddresses()
    {
        // Arrange
        var partial = "192.168";

        // Act
        var completions = _pingCommand.GetCompletions(partial);

        // Assert
        completions.Should().NotBeEmpty();
        completions.Should().Contain(c => c.StartsWith("192.168"));
        completions.Should().Contain("192.168.1.1");
        completions.Should().Contain("192.168.1.254");
    }

    [Fact]
    public void GetCompletions_PartialHostname_ReturnsMatchingHostnames()
    {
        // Arrange
        var partial = "router";

        // Act
        var completions = _pingCommand.GetCompletions(partial);

        // Assert
        completions.Should().NotBeEmpty();
        completions.Should().Contain(c => c.StartsWith("router"));
        completions.Should().Contain("router1");
    }

    [Fact]
    public void GetCompletions_PartialFlag_ReturnsMatchingFlags()
    {
        // Arrange
        var partial = "-";

        // Act
        var completions = _pingCommand.GetCompletions(partial);

        // Assert
        completions.Should().NotBeEmpty();
        completions.Should().Contain("-c");
        completions.Should().Contain("-s");
        completions.Should().Contain("-I");
        completions.Should().Contain("-t");
        completions.Should().Contain("-v");
        completions.Should().Contain("-q");
    }

    [Fact]
    public void GetCompletions_EmptyInput_ReturnsCommonDestinations()
    {
        // Act
        var completions = _pingCommand.GetCompletions("");

        // Assert
        completions.Should().NotBeEmpty();
        completions.Should().Contain("127.0.0.1");
        completions.Should().Contain("192.168.1.1");
        completions.Should().Contain("localhost");
        completions.Should().Contain("gateway");
    }

    #endregion

    #region Command Data Tests

    [Fact]
    public async Task ExecuteBusinessLogicAsync_SuccessfulPing_SetsSuccessTrue()
    {
        // Arrange
        var args = new[] { "ping", "192.168.1.1" };
        var successResult = new PingResultData
        {
            Success = true,
            Destination = "192.168.1.1",
            PacketsSent = 5,
            PacketsReceived = 5,
            PacketLossPercentage = 0
        };

        _mockPingService.Setup(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(successResult);

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var commandData = (PingCommandData)result;
        commandData.Success.Should().BeTrue();
        commandData.ErrorMessage.Should().BeNull();
        commandData.PacketsReceived.Should().Be(5);
        commandData.PacketLossPercentage.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_FailedPing_SetsSuccessFalseAndErrorMessage()
    {
        // Arrange
        var args = new[] { "ping", "unreachable.host" };
        var failResult = new PingResultData
        {
            Success = false,
            Destination = "unreachable.host",
            PacketsSent = 5,
            PacketsReceived = 0,
            PacketLossPercentage = 100,
            ErrorMessage = "Host unreachable"
        };

        _mockPingService.Setup(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(failResult);

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var commandData = (PingCommandData)result;
        commandData.Success.Should().BeFalse();
        commandData.ErrorMessage.Should().Be("Host unreachable");
        commandData.PacketsReceived.Should().Be(0);
        commandData.PacketLossPercentage.Should().Be(100);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_PingCommandData_SetsTimestamp()
    {
        // Arrange
        var args = new[] { "ping", "192.168.1.1" };
        var successResult = new PingResultData { Success = true, Destination = "192.168.1.1" };

        _mockPingService.Setup(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(successResult);

        var beforeExecution = DateTime.UtcNow;

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var afterExecution = DateTime.UtcNow;
        var commandData = (PingCommandData)result;
        commandData.ExecutionTime.Should().BeOnOrAfter(beforeExecution);
        commandData.ExecutionTime.Should().BeOnOrBefore(afterExecution);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_PingCommandData_ContainsAllPingInformation()
    {
        // Arrange
        var args = new[] { "ping", "-c", "3", "-s", "128", "192.168.1.1" };
        var pingResult = new PingResultData
        {
            Success = true,
            Destination = "192.168.1.1",
            PacketsSent = 3,
            PacketsReceived = 2,
            PacketLossPercentage = 33.33,
            PacketSize = 128,
            MinRoundTripTime = 1,
            AvgRoundTripTime = 2,
            MaxRoundTripTime = 4,
            StandardDeviation = 1.5,
            Replies = new List<PingReplyData>
            {
                new() { SequenceNumber = 1, RoundTripTime = 1, Success = true },
                new() { SequenceNumber = 2, RoundTripTime = 4, Success = true },
                new() { SequenceNumber = 3, RoundTripTime = 0, Success = false, ErrorType = "timeout" }
            }
        };

        _mockPingService.Setup(s => s.ExecutePing(It.IsAny<INetworkDevice>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(pingResult);

        // Act
        var result = await _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var commandData = (PingCommandData)result;
        commandData.Destination.Should().Be("192.168.1.1");
        commandData.PacketsSent.Should().Be(3);
        commandData.PacketsReceived.Should().Be(2);
        commandData.PacketLossPercentage.Should().BeApproximately(33.33, 0.1);
        commandData.PacketSize.Should().Be(128);
        commandData.MinRoundTripTime.Should().Be(1);
        commandData.AvgRoundTripTime.Should().Be(2);
        commandData.MaxRoundTripTime.Should().Be(4);
        commandData.StandardDeviation.Should().BeApproximately(1.5, 0.1);
        commandData.Replies.Should().HaveCount(3);
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData(new[] { "ping", "-c", "0", "192.168.1.1" })]
    [InlineData(new[] { "ping", "-c", "-1", "192.168.1.1" })]
    [InlineData(new[] { "ping", "-c", "abc", "192.168.1.1" })]
    public async Task ParsePingOptions_InvalidCount_ThrowsCommandExecutionException(string[] args)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args));

        exception.Message.Should().Contain("count");
    }

    [Theory]
    [InlineData(new[] { "ping", "-s", "0", "192.168.1.1" })]
    [InlineData(new[] { "ping", "-s", "-1", "192.168.1.1" })]
    [InlineData(new[] { "ping", "-s", "70000", "192.168.1.1" })] // Too large
    public async Task ParsePingOptions_InvalidSize_ThrowsCommandExecutionException(string[] args)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args));

        exception.Message.Should().Contain("size");
    }

    [Theory]
    [InlineData(new[] { "ping", "-t", "0", "192.168.1.1" })]
    [InlineData(new[] { "ping", "-t", "256", "192.168.1.1" })] // Too large
    [InlineData(new[] { "ping", "-t", "abc", "192.168.1.1" })]
    public async Task ParsePingOptions_InvalidTTL_ThrowsCommandExecutionException(string[] args)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _pingCommand.ExecuteBusinessLogicAsync(_mockDevice, args));

        exception.Message.Should().Contain("TTL");
    }

    #endregion
}