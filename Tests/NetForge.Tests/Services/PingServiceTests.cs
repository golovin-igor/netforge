using FluentAssertions;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Common.Common;
using NetForge.Tests.TestHelpers;
using Xunit;

namespace NetForge.Tests.Services;

/// <summary>
/// Comprehensive test suite for PingService following the test strategy
/// Tests business logic validation, network simulation accuracy, and vendor-agnostic behavior
/// </summary>
public class PingServiceTests
{
    private readonly IPingService _pingService;
    private readonly MockNetworkDevice _mockDevice;

    public PingServiceTests()
    {
        _pingService = new PingService();
        _mockDevice = MockDeviceBuilder.Create()
            .WithName("TestRouter1")
            .WithVendor("Cisco")
            .WithInterface("eth0", "192.168.1.10", isUp: true)
            .Build();
    }

    #region Business Logic Validation Tests

    [Fact]
    public void ExecutePing_ValidDestination_ReturnsSuccessfulPingResult()
    {
        // Arrange
        var destination = "192.168.1.1";
        var expectedPacketCount = 5;
        var expectedPacketSize = 64;

        // Act
        var result = _pingService.ExecutePing(_mockDevice, destination, expectedPacketCount, expectedPacketSize);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Destination.Should().Be(destination);
        result.PacketsSent.Should().Be(expectedPacketCount);
        result.PacketSize.Should().Be(expectedPacketSize);
        result.Replies.Should().HaveCount(expectedPacketCount);
        result.PacketsReceived.Should().BeGreaterThan(0);
        result.PacketLossPercentage.Should().BeLessOrEqualTo(100);
        result.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.EndTime.Should().BeAfter(result.StartTime);
    }

    [Fact]
    public void ExecutePing_InvalidDestination_ReturnsFailedPingResult()
    {
        // Arrange
        var invalidDestination = "invalid.hostname.that.should.not.resolve";

        // Act
        var result = _pingService.ExecutePing(_mockDevice, invalidDestination);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Cannot resolve hostname");
        result.PacketsReceived.Should().Be(0);
        result.PacketLossPercentage.Should().Be(100);
        result.Destination.Should().Be(invalidDestination);
    }

    [Fact]
    public void ExecutePing_UnreachableDestination_ReturnsTimeoutResult()
    {
        // Arrange
        var unreachableDestination = "192.168.99.99"; // Simulated unreachable network

        // Act
        var result = _pingService.ExecutePing(_mockDevice, unreachableDestination);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.PacketLossPercentage.Should().BeGreaterThan(80); // High packet loss for unreachable
        result.Replies.Should().OnlyContain(r => !r.Success || r.ErrorType == "timeout");
    }

    [Fact]
    public void ExecutePing_CustomPacketSize_UsesCorrectPacketSize()
    {
        // Arrange
        var destination = "192.168.1.1";
        var customPacketSize = 1500;
        var packetCount = 3;

        // Act
        var result = _pingService.ExecutePing(_mockDevice, destination, packetCount, customPacketSize);

        // Assert
        result.Should().NotBeNull();
        result.PacketSize.Should().Be(customPacketSize);
        result.PacketsSent.Should().Be(packetCount);
        result.Replies.Should().HaveCount(packetCount);
        result.Replies.Should().OnlyContain(r => r.BytesReceived == customPacketSize);
    }

    [Fact]
    public void ExecutePing_CustomPacketCount_SendsCorrectNumberOfPackets()
    {
        // Arrange
        var destination = "192.168.1.1";
        var customPacketCount = 10;

        // Act
        var result = _pingService.ExecutePing(_mockDevice, destination, customPacketCount);

        // Assert
        result.Should().NotBeNull();
        result.PacketsSent.Should().Be(customPacketCount);
        result.Replies.Should().HaveCount(customPacketCount);
        for (int i = 0; i < customPacketCount; i++)
        {
            result.Replies[i].SequenceNumber.Should().Be(i + 1);
        }
    }

    [Fact]
    public void ExecutePingWithOptions_AllOptions_AppliesAllOptionsCorrectly()
    {
        // Arrange
        var options = new PingOptions
        {
            Destination = "192.168.1.1",
            PacketSize = 1500,
            PingCount = 3,
            TimeoutSeconds = 5,
            SourceInterface = "eth0",
            Ttl = 64,
            DontFragment = true,
            Verbose = true
        };

        // Act
        var result = _pingService.ExecutePingWithOptions(_mockDevice, options);

        // Assert
        result.Should().NotBeNull();
        result.Destination.Should().Be(options.Destination);
        result.PacketSize.Should().Be(options.PacketSize);
        result.PacketsSent.Should().Be(options.PingCount);
        result.TimeoutSeconds.Should().Be(options.TimeoutSeconds);
        result.SourceInterface.Should().Be(options.SourceInterface);
        result.Ttl.Should().Be(options.Ttl);
        result.Replies.Should().HaveCount(options.PingCount);
        result.Replies.Should().OnlyContain(r => r.BytesReceived == options.PacketSize);
    }

    [Fact]
    public async Task ExecuteContinuousPing_CancellationToken_StopsCorrectly()
    {
        // Arrange
        var destination = "192.168.1.1";
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var results = new List<PingResultData>();

        // Act
        await foreach (var result in _pingService.ExecuteContinuousPing(_mockDevice, destination, cts.Token))
        {
            results.Add(result);
            if (results.Count >= 10) break; // Safety limit
        }

        // Assert
        results.Should().NotBeEmpty();
        results.Should().HaveCountLessOrEqualTo(10);
        results.Should().OnlyContain(r => r.Destination == destination);
        results.Should().OnlyContain(r => r.PacketsSent == 1); // Each continuous ping sends 1 packet
    }

    #endregion

    #region Network Simulation Accuracy Tests

    [Fact]
    public void CalculateRoundTripTime_SameNetwork_ReturnsLowLatency()
    {
        // Arrange
        var sameNetworkDestination = "192.168.1.100";

        // Act
        var rtt = _pingService.CalculateRoundTripTime(_mockDevice, sameNetworkDestination);

        // Assert
        rtt.Should().BeInRange(1, 4); // Same network should be 1-4ms
    }

    [Fact]
    public void CalculateRoundTripTime_RemoteNetwork_ReturnsHigherLatency()
    {
        // Arrange
        var remoteDestination = "8.8.8.8";

        // Act
        var rtt = _pingService.CalculateRoundTripTime(_mockDevice, remoteDestination);

        // Assert
        rtt.Should().BeInRange(6, 51); // Remote network should be 6-51ms
    }

    [Fact]
    public void CalculateRoundTripTime_LocalHost_Returns1msRTT()
    {
        // Arrange
        var localHost = "127.0.0.1";

        // Act
        var rtt = _pingService.CalculateRoundTripTime(_mockDevice, localHost);

        // Assert
        rtt.Should().Be(1); // Localhost should always be 1ms
    }

    [Fact]
    public void UpdatePingCounters_ValidDevice_UpdatesInterfaceCounters()
    {
        // Arrange
        var destination = "192.168.1.1";
        var packetCount = 5;
        var packetSize = 64;
        var interface = _mockDevice.GetInterface("eth0");
        var initialTxPackets = interface.TxPackets;
        var initialRxPackets = interface.RxPackets;
        var initialTxBytes = interface.TxBytes;
        var initialRxBytes = interface.RxBytes;

        // Act
        _pingService.UpdatePingCounters(_mockDevice, destination, packetCount, packetSize);

        // Assert
        interface.TxPackets.Should().Be(initialTxPackets + packetCount);
        interface.RxPackets.Should().Be(initialRxPackets + packetCount);
        interface.TxBytes.Should().BeGreaterThan(initialTxBytes);
        interface.RxBytes.Should().BeGreaterThan(initialRxBytes);
        // ICMP header + IP header + data
        var expectedPacketSize = packetSize + 28; // 20 bytes IP + 8 bytes ICMP
        interface.TxBytes.Should().Be(initialTxBytes + (packetCount * expectedPacketSize));
    }

    [Fact]
    public void UpdatePingCounters_MultiplePings_AccumulatesCorrectly()
    {
        // Arrange
        var destination = "192.168.1.1";
        var interface = _mockDevice.GetInterface("eth0");
        var initialTxPackets = interface.TxPackets;

        // Act
        _pingService.UpdatePingCounters(_mockDevice, destination, 3, 64);
        _pingService.UpdatePingCounters(_mockDevice, destination, 2, 64);

        // Assert
        interface.TxPackets.Should().Be(initialTxPackets + 5); // 3 + 2
    }

    #endregion

    #region Validation Tests

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("0.0.0.0", true)]
    [InlineData("256.1.1.1", false)]
    [InlineData("192.168.1", false)]
    [InlineData("invalid-ip", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidIpAddress_VariousInputs_ValidatesCorrectly(string ipAddress, bool expectedValid)
    {
        // Act
        var result = _pingService.IsValidIpAddress(ipAddress);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("localhost", true)]
    [InlineData("router1.example.com", true)]
    [InlineData("switch-1", true)]
    [InlineData("host.sub.domain.com", true)]
    [InlineData("a", true)]
    [InlineData("123-invalid-start", false)]
    [InlineData("toolong" + "x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x.x", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("host..domain", false)] // Double dot
    [InlineData(".startwithdot", false)] // Start with dot
    [InlineData("endwithdot.", false)] // End with dot
    public void IsValidHostname_VariousInputs_ValidatesCorrectly(string hostname, bool expectedValid)
    {
        // Act
        var result = _pingService.IsValidHostname(hostname);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("localhost", "127.0.0.1")]
    [InlineData("router1", "192.168.1.1")]
    [InlineData("switch1", "192.168.1.10")]
    [InlineData("gateway", "192.168.1.254")]
    [InlineData("unknown-host", null)]
    [InlineData("nonexistent.example.com", null)]
    public void ResolveHostname_KnownAndUnknownHosts_ResolvesCorrectly(string hostname, string expectedIp)
    {
        // Act
        var result = _pingService.ResolveHostname(hostname);

        // Assert
        result.Should().Be(expectedIp);
    }

    [Theory]
    [InlineData("192.168.1.1", true)]   // Same network as device (192.168.1.10)
    [InlineData("10.0.0.1", true)]      // Different network but reachable
    [InlineData("172.16.1.1", true)]    // Private network, reachable
    [InlineData("8.8.8.8", false)]      // External network, not reachable in test
    [InlineData("invalid-ip", false)]   // Invalid IP
    [InlineData("192.168.99.99", false)] // Unreachable network
    public void IsDestinationReachable_VariousDestinations_ReturnsExpectedResult(string destination, bool expectedReachable)
    {
        // Act
        var result = _pingService.IsDestinationReachable(_mockDevice, destination);

        // Assert
        result.Should().Be(expectedReachable);
    }

    #endregion

    #region Interface Selection Tests

    [Fact]
    public void GetPingSourceInterface_HasActiveInterface_ReturnsCorrectInterface()
    {
        // Arrange - device already has eth0 interface from constructor

        // Act
        var result = _pingService.GetPingSourceInterface(_mockDevice);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("eth0");
        result.IpAddress.Should().Be("192.168.1.10");
        result.IsUp.Should().BeTrue();
    }

    [Fact]
    public void GetPingSourceInterface_MultipleInterfaces_ReturnsFirstActiveInterface()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithInterface("eth0", "192.168.1.10", isUp: false)
            .WithInterface("eth1", "10.0.0.10", isUp: true)
            .WithInterface("eth2", "172.16.1.10", isUp: true)
            .Build();

        // Act
        var result = _pingService.GetPingSourceInterface(device);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("eth1"); // First active interface
        result.IpAddress.Should().Be("10.0.0.10");
        result.IsUp.Should().BeTrue();
    }

    [Fact]
    public void GetPingSourceInterface_SpecificInterface_ReturnsRequestedInterface()
    {
        // Arrange
        var specificInterface = "eth1";
        var device = MockDeviceBuilder.Create()
            .WithInterface("eth0", "192.168.1.10", isUp: true)
            .WithInterface("eth1", "10.0.0.10", isUp: true)
            .Build();

        // Act
        var result = _pingService.GetPingSourceInterface(device, specificInterface);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("eth1");
        result.IpAddress.Should().Be("10.0.0.10");
    }

    [Fact]
    public void GetPingSourceInterface_InterfaceDown_ReturnsNull()
    {
        // Arrange
        var downInterface = "eth0";
        var device = MockDeviceBuilder.Create()
            .WithInterface("eth0", "192.168.1.10", isUp: false)
            .Build();

        // Act
        var result = _pingService.GetPingSourceInterface(device, downInterface);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetPingSourceInterface_InterfaceNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentInterface = "eth99";

        // Act
        var result = _pingService.GetPingSourceInterface(_mockDevice, nonExistentInterface);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetPingSourceInterface_NoActiveInterfaces_ReturnsNull()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithInterface("eth0", "192.168.1.10", isUp: false)
            .WithInterface("eth1", "10.0.0.10", isUp: false)
            .Build();

        // Act
        var result = _pingService.GetPingSourceInterface(device);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void ExecutePing_NullDevice_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => _pingService.ExecutePing(null, "192.168.1.1");
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("device");
    }

    [Fact]
    public void ExecutePingWithOptions_NullOptions_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => _pingService.ExecutePingWithOptions(_mockDevice, null);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void ExecutePing_NullDestination_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => _pingService.ExecutePing(_mockDevice, null);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("destination");
    }

    [Fact]
    public void ExecutePing_EmptyDestination_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => _pingService.ExecutePing(_mockDevice, "");
        act.Should().Throw<ArgumentException>()
            .WithParameterName("destination");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ExecutePing_InvalidPacketCount_ThrowsArgumentException(int invalidPacketCount)
    {
        // Act & Assert
        Action act = () => _pingService.ExecutePing(_mockDevice, "192.168.1.1", invalidPacketCount);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("pingCount");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)] // Too large
    public void ExecutePing_InvalidPacketSize_ThrowsArgumentException(int invalidPacketSize)
    {
        // Act & Assert
        Action act = () => _pingService.ExecutePing(_mockDevice, "192.168.1.1", 5, invalidPacketSize);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("packetSize");
    }

    [Fact]
    public void UpdatePingCounters_NullDevice_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => _pingService.UpdatePingCounters(null, "192.168.1.1", 5, 64);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("device");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void ExecutePing_StandardOptions_CompletesWithin50ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _pingService.ExecutePing(_mockDevice, "192.168.1.1");

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(50);
        result.Should().NotBeNull();
    }

    [Fact]
    public void ExecutePing_LargePacketCount_CompletesReasonably()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _pingService.ExecutePing(_mockDevice, "192.168.1.1", 100);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(200); // Should still be fast for simulation
        result.Should().NotBeNull();
        result.PacketsSent.Should().Be(100);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public void ExecutePing_SequenceNumbers_AreSequential()
    {
        // Act
        var result = _pingService.ExecutePing(_mockDevice, "192.168.1.1", 5);

        // Assert
        result.Replies.Should().HaveCount(5);
        for (int i = 0; i < 5; i++)
        {
            result.Replies[i].SequenceNumber.Should().Be(i + 1);
        }
    }

    [Fact]
    public void ExecutePing_Timestamps_AreRealistic()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        var result = _pingService.ExecutePing(_mockDevice, "192.168.1.1", 3);

        // Assert
        var endTime = DateTime.UtcNow;
        result.StartTime.Should().BeOnOrAfter(startTime);
        result.EndTime.Should().BeOnOrBefore(endTime);
        result.EndTime.Should().BeOnOrAfter(result.StartTime);

        foreach (var reply in result.Replies)
        {
            reply.Timestamp.Should().BeOnOrAfter(result.StartTime);
            reply.Timestamp.Should().BeOnOrBefore(result.EndTime);
        }
    }

    [Fact]
    public void ExecutePing_PacketLossCalculation_IsAccurate()
    {
        // Arrange & Act
        var result = _pingService.ExecutePing(_mockDevice, "192.168.1.1", 10);

        // Assert
        var expectedLoss = result.PacketsSent - result.PacketsReceived;
        var expectedPercentage = (double)expectedLoss / result.PacketsSent * 100;
        result.PacketLossPercentage.Should().BeApproximately(expectedPercentage, 0.1);
    }

    #endregion
}