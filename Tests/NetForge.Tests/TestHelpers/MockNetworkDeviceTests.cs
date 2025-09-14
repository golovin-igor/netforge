using FluentAssertions;
using NetForge.Tests.TestHelpers;

namespace NetForge.Tests.TestHelpers;

/// <summary>
/// Tests for the MockNetworkDevice implementation
/// </summary>
public class MockNetworkDeviceTests
{
    [Fact]
    public void MockNetworkDevice_WithBasicProperties_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var device = MockDeviceBuilder.Create()
            .WithName("TestRouter")
            .WithVendor("Cisco")
            .Build();

        // Assert
        device.Name.Should().Be("TestRouter");
        device.Vendor.Should().Be("Cisco");
        device.DeviceName.Should().Be("TestRouter");
        device.DeviceType.Should().Be("Router");
        device.GetHostname().Should().Be("TestRouter");
        device.DeviceId.Should().NotBeNullOrEmpty();
        device.DeviceId.Should().Contain("Cisco-TestRouter");
    }

    [Fact]
    public void MockNetworkDevice_WithEthernetInterface_CreatesInterfaceCorrectly()
    {
        // Arrange & Act
        var device = MockDeviceBuilder.Create()
            .WithName("Switch1")
            .WithVendor("Juniper")
            .WithEthernetInterface("ge-0/0/0", "192.168.1.10")
            .Build();

        // Assert
        var iface = device.GetInterface("ge-0/0/0");
        iface.Should().NotBeNull();
        iface.Name.Should().Be("ge-0/0/0");
        iface.IpAddress.Should().Be("192.168.1.10");
        iface.InterfaceType.Should().Be("Ethernet");
        iface.IsUp.Should().BeTrue();
        iface.IsShutdown.Should().BeFalse();
        iface.IsOperational.Should().BeTrue();
        iface.MacAddress.Should().NotBeNullOrEmpty();
        iface.Mtu.Should().Be(1500);
    }

    [Fact]
    public void MockNetworkDevice_WithLoopbackInterface_CreatesLoopbackCorrectly()
    {
        // Arrange & Act
        var device = MockDeviceBuilder.Create()
            .WithName("Router1")
            .WithLoopbackInterface("lo0", "10.0.0.1")
            .Build();

        // Assert
        var loopback = device.GetInterface("lo0");
        loopback.Should().NotBeNull();
        loopback.Name.Should().Be("lo0");
        loopback.IpAddress.Should().Be("10.0.0.1");
        loopback.SubnetMask.Should().Be("255.0.0.0");
        loopback.InterfaceType.Should().Be("Loopback");
        loopback.MacAddress.Should().Be("00:00:00:00:00:00");
    }

    [Fact]
    public void MockNetworkDevice_NetworkConnectivity_SimulatesReachabilityCorrectly()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("TestDevice")
            .WithInterface("eth0", "192.168.1.1")
            .Build();

        // Act & Assert - Reachable destinations
        device.CanReach("192.168.1.2").Should().BeTrue();
        device.CanReach("8.8.8.8").Should().BeTrue();
        device.CanReach("localhost").Should().BeTrue();
        device.CanReach("127.0.0.1").Should().BeTrue();

        // Act & Assert - Unreachable destinations
        device.CanReach("192.168.999.1").Should().BeFalse();
        device.CanReach("unreachable.example.com").Should().BeFalse();
        device.CanReach("10.0.0.0").Should().BeFalse();
        device.CanReach("").Should().BeFalse();
    }

    [Fact]
    public void MockNetworkDevice_LatencyCalculation_ReturnsRealisticLatencies()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("TestDevice")
            .WithInterface("eth0", "192.168.1.1")
            .Build();

        // Act
        var localhostLatency = device.CalculateLatency("127.0.0.1");
        var localNetworkLatency = device.CalculateLatency("192.168.1.5");
        var remoteLatency = device.CalculateLatency("8.8.8.8");
        var unreachableLatency = device.CalculateLatency("192.168.999.1");

        // Assert
        localhostLatency.Should().BeLessThan(TimeSpan.FromMilliseconds(2));
        localNetworkLatency.Should().BeLessThan(TimeSpan.FromMilliseconds(10));
        remoteLatency.Should().BeGreaterThan(TimeSpan.FromMilliseconds(15)).And.BeLessThan(TimeSpan.FromMilliseconds(150));
        unreachableLatency.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(5000));
    }

    [Fact]
    public void MockNetworkDevice_ActiveInterface_ReturnsCorrectInterface()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithInterface("eth0", "192.168.1.1", isUp: false) // Down
            .WithInterface("eth1", "192.168.1.2", isShutdown: true) // Shutdown
            .WithInterface("eth2", "192.168.1.3", isUp: true, isShutdown: false) // Active
            .Build();

        // Act
        var activeInterface = device.GetActiveInterface();
        var hasActive = device.HasActiveInterface();

        // Assert
        hasActive.Should().BeTrue();
        activeInterface.Should().NotBeNull();
        activeInterface!.Name.Should().Be("eth2");
        activeInterface.IsOperational.Should().BeTrue();
    }

    [Fact]
    public void MockNetworkDevice_InterfaceCounters_UpdateCorrectly()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithInterface("eth0", "192.168.1.1")
            .Build();

        var iface = device.GetInterface("eth0");
        var initialTxBytes = iface.TxBytes;
        var initialRxBytes = iface.RxBytes;

        // Act
        device.UpdateInterfaceCounters("eth0", 1000, 500);

        // Assert
        iface.TxBytes.Should().Be(initialTxBytes + 1000);
        iface.RxBytes.Should().Be(initialRxBytes + 500);
    }

    [Fact]
    public void MockNetworkDevice_InterfaceActivity_SimulatesCorrectly()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithInterface("eth0", "192.168.1.1")
            .Build();

        var iface = device.GetInterface("eth0");
        var initialTxPackets = iface.TxPackets;
        var initialTxBytes = iface.TxBytes;

        // Act
        iface.SimulateActivity(5, 128); // 5 packets of 128 bytes each

        // Assert
        iface.TxPackets.Should().Be(initialTxPackets + 5);
        iface.TxBytes.Should().Be(initialTxBytes + (5 * 128));
        iface.LastActivity.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MockNetworkDevice_SystemSettings_WorkCorrectly()
    {
        // Arrange
        var device = MockDeviceBuilder.Create().Build();

        // Act
        device.SetSystemSetting("version", "15.2(4)E7");
        device.SetSystemSetting("model", "2960");

        // Assert
        device.GetSystemSetting("version").Should().Be("15.2(4)E7");
        device.GetSystemSetting("model").Should().Be("2960");
        device.GetSystemSetting("nonexistent").Should().BeNull();
    }

    [Fact]
    public void MockNetworkDevice_LogEntries_TrackCorrectly()
    {
        // Arrange
        var device = MockDeviceBuilder.Create().Build();

        // Act
        device.AddLogEntry("Interface eth0 up");
        device.AddLogEntry("BGP neighbor established");

        // Assert
        var logs = device.GetLogEntries();
        logs.Should().HaveCount(2);
        logs[0].Should().Contain("Interface eth0 up");
        logs[1].Should().Contain("BGP neighbor established");
        logs.All(log => log.Contains(DateTime.Now.ToString("yyyy-MM-dd"))).Should().BeTrue();
    }

    [Fact]
    public void MockNetworkDevice_HostnameOperations_WorkCorrectly()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("OriginalName")
            .Build();

        // Act
        device.SetHostname("NewHostname");

        // Assert
        device.GetHostname().Should().Be("NewHostname");
        device.Name.Should().Be("OriginalName"); // Name should remain unchanged
        device.DeviceName.Should().Be("OriginalName"); // DeviceName should remain unchanged
    }

    [Fact]
    public void MockInterface_StatisticsSummary_ReturnsCorrectFormat()
    {
        // Arrange
        var iface = new MockInterface
        {
            Name = "eth0",
            TxPackets = 1000,
            TxBytes = 64000,
            RxPackets = 500,
            RxBytes = 32000
        };

        // Act
        var summary = iface.GetStatisticsSummary();

        // Assert
        summary.Should().Be("TX: 1000 packets (64000 bytes), RX: 500 packets (32000 bytes)");
    }

    [Fact]
    public void MockInterface_MacAddressGeneration_GeneratesValidMacAddress()
    {
        // Arrange & Act
        var iface = new MockInterface();

        // Assert
        iface.MacAddress.Should().NotBeNullOrEmpty();
        iface.MacAddress.Should().MatchRegex(@"^[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}$");

        // Check that it's locally administered and unicast
        var firstByte = Convert.ToByte(iface.MacAddress.Substring(0, 2), 16);
        (firstByte & 0x02).Should().Be(0x02); // Locally administered bit set
        (firstByte & 0x01).Should().Be(0x00); // Unicast (multicast bit clear)
    }
}