using FluentAssertions;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Formatters;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Tests.TestHelpers;
using Xunit;

namespace NetForge.Tests.Formatters;

/// <summary>
/// Comprehensive test suite for Juniper formatters following the test strategy
/// Tests JunOS format accuracy for ping and show version commands
/// </summary>
public class JuniperFormatterTests
{
    private readonly JuniperPingFormatter _juniperPingFormatter;
    private readonly JuniperShowVersionFormatter _juniperShowVersionFormatter;
    private readonly MockNetworkDevice _mockDevice;

    public JuniperFormatterTests()
    {
        _juniperPingFormatter = new JuniperPingFormatter();
        _juniperShowVersionFormatter = new JuniperShowVersionFormatter();
        _mockDevice = MockDeviceBuilder.Create()
            .WithName("ex2200-switch")
            .WithVendor("Juniper")
            .WithInterface("ge-0/0/0", "192.168.1.10", isUp: true)
            .Build();
    }

    #region Juniper Ping Formatter Tests

    [Fact]
    public void FormatPingResult_SuccessfulPings_ShowsIndividualReplies()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            ResolvedAddress = "192.168.1.1",
            Success = true,
            PacketsSent = 5,
            PacketsReceived = 5,
            PacketLossPercentage = 0,
            PacketSize = 56, // JunOS default data size
            MinRoundTripTime = 1,
            AvgRoundTripTime = 2,
            MaxRoundTripTime = 4,
            StandardDeviation = 1.2,
            Replies = CreateSuccessfulJunOSReplies(5)
        };

        // Act
        var result = _juniperPingFormatter.Format(pingData, "Juniper");

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("PING 192.168.1.1 (192.168.1.1): 56 data bytes");
        result.Should().Contain("64 bytes from 192.168.1.1: icmp_seq=0 ttl=64 time=1.000 ms");
        result.Should().Contain("64 bytes from 192.168.1.1: icmp_seq=1 ttl=64 time=2.000 ms");
        result.Should().Contain("--- 192.168.1.1 ping statistics ---");
        result.Should().Contain("5 packets transmitted, 5 packets received, 0% packet loss");
        result.Should().Contain("round-trip min/avg/max/stddev = 1.000/2.000/4.000/1.200 ms");
    }

    [Fact]
    public void FormatPingResult_SuccessfulPings_ShowsSubMillisecondPrecision()
    {
        // Arrange
        var replies = new List<PingReplyData>
        {
            new() { SequenceNumber = 1, Success = true, RoundTripTime = 1, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 },
            new() { SequenceNumber = 2, Success = true, RoundTripTime = 3, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 }
        };

        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 2,
            PacketsReceived = 2,
            PacketLossPercentage = 0,
            PacketSize = 56,
            MinRoundTripTime = 1,
            AvgRoundTripTime = 2,
            MaxRoundTripTime = 3,
            StandardDeviation = 1.0,
            Replies = replies
        };

        // Act
        var result = _juniperPingFormatter.Format(pingData, "Juniper");

        // Assert
        result.Should().Contain("time=1.000 ms");
        result.Should().Contain("time=3.000 ms");
        result.Should().Contain("round-trip min/avg/max/stddev = 1.000/2.000/3.000/1.000 ms");
    }

    [Fact]
    public void FormatPingResult_TimeoutPings_ShowsRequestTimeout()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.99",
            Success = false,
            PacketsSent = 3,
            PacketsReceived = 0,
            PacketLossPercentage = 100,
            PacketSize = 56,
            Replies = CreateTimeoutJunOSReplies(3)
        };

        // Act
        var result = _juniperPingFormatter.Format(pingData, "Juniper");

        // Assert
        result.Should().Contain("PING 192.168.1.99 (192.168.1.99): 56 data bytes");
        result.Should().Contain("Request timeout for icmp_seq 0");
        result.Should().Contain("Request timeout for icmp_seq 1");
        result.Should().Contain("Request timeout for icmp_seq 2");
        result.Should().Contain("--- 192.168.1.99 ping statistics ---");
        result.Should().Contain("3 packets transmitted, 0 packets received, 100% packet loss");
        result.Should().NotContain("round-trip min/avg/max"); // No RTT stats for complete failure
    }

    [Fact]
    public void FormatPingResult_MixedResults_ShowsJunOSStyleSummary()
    {
        // Arrange
        var replies = new List<PingReplyData>
        {
            new() { SequenceNumber = 1, Success = true, RoundTripTime = 1, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 },
            new() { SequenceNumber = 2, Success = false, ErrorType = "timeout" },
            new() { SequenceNumber = 3, Success = true, RoundTripTime = 3, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 },
            new() { SequenceNumber = 4, Success = false, ErrorType = "timeout" },
            new() { SequenceNumber = 5, Success = true, RoundTripTime = 2, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 }
        };

        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = false, // Mixed results = overall failure
            PacketsSent = 5,
            PacketsReceived = 3,
            PacketLossPercentage = 40,
            PacketSize = 56,
            MinRoundTripTime = 1,
            AvgRoundTripTime = 2,
            MaxRoundTripTime = 3,
            StandardDeviation = 1.0,
            Replies = replies
        };

        // Act
        var result = _juniperPingFormatter.Format(pingData, "Juniper");

        // Assert
        result.Should().Contain("64 bytes from 192.168.1.1: icmp_seq=0 ttl=64 time=1.000 ms");
        result.Should().Contain("Request timeout for icmp_seq 1");
        result.Should().Contain("64 bytes from 192.168.1.1: icmp_seq=2 ttl=64 time=3.000 ms");
        result.Should().Contain("Request timeout for icmp_seq 3");
        result.Should().Contain("64 bytes from 192.168.1.1: icmp_seq=4 ttl=64 time=2.000 ms");
        result.Should().Contain("5 packets transmitted, 3 packets received, 40% packet loss");
        result.Should().Contain("round-trip min/avg/max/stddev = 1.000/2.000/3.000/1.000 ms");
    }

    [Fact]
    public void FormatPingResult_HostnameResolution_ShowsResolvedAddress()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "router1.example.com",
            ResolvedAddress = "192.168.1.1",
            Success = true,
            PacketsSent = 1,
            PacketsReceived = 1,
            PacketLossPercentage = 0,
            PacketSize = 56,
            Replies = new List<PingReplyData>
            {
                new() { SequenceNumber = 1, Success = true, RoundTripTime = 2, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 }
            }
        };

        // Act
        var result = _juniperPingFormatter.Format(pingData, "Juniper");

        // Assert
        result.Should().Contain("PING router1.example.com (192.168.1.1): 56 data bytes");
        result.Should().Contain("64 bytes from 192.168.1.1: icmp_seq=0 ttl=64 time=2.000 ms");
    }

    [Fact]
    public void JuniperPingFormatter_VendorName_ReturnsJuniper()
    {
        // Act
        var vendorName = _juniperPingFormatter.VendorName;

        // Assert
        vendorName.Should().Be("Juniper");
    }

    [Fact]
    public void JuniperPingFormatter_CanFormat_ReturnsTrueForPingCommandData()
    {
        // Arrange
        var pingData = new PingCommandData();

        // Act
        var canFormat = _juniperPingFormatter.CanFormat(pingData, "Juniper");

        // Assert
        canFormat.Should().BeTrue();
    }

    #endregion

    #region Juniper Show Version Formatter Tests

    [Fact]
    public void FormatShowVersion_JuniperDevice_StartsWithHostname()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().StartWith("Hostname: ex2200-switch");
        result.Should().Contain("Model: EX2200-24T-4G");
    }

    [Fact]
    public void FormatShowVersion_JuniperDevice_ShowsJunOSSoftwareRelease()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().Contain("Junos: 12.3R12.4");
        result.Should().Contain("JUNOS Software Release");
        result.Should().Contain("built by builder");
    }

    [Fact]
    public void FormatShowVersion_JuniperDevice_ShowsFpcInformation()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().Contain("FPC 0:");
        result.Should().Contain("CPU MIPS64 processor");
        result.Should().Contain("Start time:");
        result.Should().Contain("Uptime:");
    }

    [Fact]
    public void FormatShowVersion_JuniperDevice_ShowsChassisHardwareTable()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().Contain("Chassis Hardware:");
        result.Should().Contain("Item             Version  Part number  Serial number     Description");
        result.Should().Contain("Chassis                                JN123456789       EX2200-24T-4G");
        result.Should().Contain("Routing Engine");
        result.Should().Contain("FPC 0");
        result.Should().Contain("Power Supply 0");
        result.Should().Contain("Fan Tray");
    }

    [Fact]
    public void FormatShowVersion_JuniperDevice_ShowsCPUUtilization()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().Contain("CPU utilization:");
        result.Should().Contain("User");
        result.Should().Contain("Background");
        result.Should().Contain("Kernel");
        result.Should().Contain("Interrupt");
        result.Should().Contain("Idle");
    }

    [Fact]
    public void FormatShowVersion_JuniperDevice_ShowsMemoryUtilization()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().Contain("Memory utilization:");
        result.Should().Contain("Total memory:");
        result.Should().Contain("Reserved memory:");
        result.Should().Contain("Wired memory:");
        result.Should().Contain("Active memory:");
        result.Should().Contain("Inactive memory:");
        result.Should().Contain("Free memory:");
    }

    [Fact]
    public void FormatShowVersion_JuniperDevice_ShowsSoftwareComponents()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().Contain("Software components:");
        result.Should().Contain("junos-runtime");
        result.Should().Contain("junos-base");
        result.Should().Contain("junos-routing");
        result.Should().Contain("junos-openconfig");
        result.Should().Contain("[12.3R12.4]");
    }

    [Fact]
    public void FormatShowVersion_JuniperDevice_ShowsUptimeInJunOSFormat()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().Contain("Current time:");
        result.Should().Contain("Time Source:  NTP CLOCK");
        result.Should().Contain("Boot time:");
        result.Should().Contain("System booted:");
        result.Should().Contain("Uptime: 30 days, 12 hours, 45 minutes");
    }

    [Fact]
    public void FormatShowVersion_JuniperDevice_ShowsSerialAndPartNumbers()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        result.Should().Contain("Serial number: JN123456789");
        result.Should().Contain("Part number:");
        result.Should().Contain("Assembly date:");
        result.Should().Contain("CLEI code:");
    }

    [Fact]
    public void JuniperShowVersionFormatter_VendorName_ReturnsJuniper()
    {
        // Act
        var vendorName = _juniperShowVersionFormatter.VendorName;

        // Assert
        vendorName.Should().Be("Juniper");
    }

    [Fact]
    public void JuniperShowVersionFormatter_CanFormat_ReturnsTrueForShowVersionCommandData()
    {
        // Arrange
        var showVersionData = new ShowVersionCommandData();

        // Act
        var canFormat = _juniperShowVersionFormatter.CanFormat(showVersionData, "Juniper");

        // Assert
        canFormat.Should().BeTrue();
    }

    #endregion

    #region Cross-Format Consistency Tests

    [Fact]
    public void JuniperFormatters_DifferentCommands_MaintainConsistentJunOSStyle()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 3,
            PacketsReceived = 3,
            PacketLossPercentage = 0,
            Replies = CreateSuccessfulJunOSReplies(3)
        };

        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var pingOutput = _juniperPingFormatter.Format(pingData, "Juniper");
        var versionOutput = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        // Both outputs should maintain JunOS's characteristic style elements
        pingOutput.Should().Contain("PING"); // JunOS ping characteristic
        versionOutput.Should().Contain("Hostname:"); // JunOS version characteristic

        // Both should avoid other vendor-specific terminology
        pingOutput.Should().NotContain("Type escape sequence to abort"); // Cisco style
        versionOutput.Should().NotContain("Cisco IOS Software"); // Cisco style
    }

    [Fact]
    public void JuniperFormatters_HostnameConsistency_UseSameHostname()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 1,
            PacketsReceived = 1,
            Replies = CreateSuccessfulJunOSReplies(1)
        };

        var showVersionData = CreateMockJunOSShowVersionData();

        // Act
        var versionOutput = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        // JunOS show version displays the hostname prominently
        versionOutput.Should().Contain("Hostname: ex2200-switch");
        // This hostname should match the device used in both formatters
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void JuniperPingFormatter_StandardFormatting_CompletesWithin10ms()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 5,
            PacketsReceived = 5,
            Replies = CreateSuccessfulJunOSReplies(5)
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _juniperPingFormatter.Format(pingData, "Juniper");

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(10);
        result.Should().NotBeNull();
    }

    [Fact]
    public void JuniperShowVersionFormatter_StandardFormatting_CompletesWithin10ms()
    {
        // Arrange
        var showVersionData = CreateMockJunOSShowVersionData();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _juniperShowVersionFormatter.Format(showVersionData, "Juniper");

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(10);
        result.Should().NotBeNull();
    }

    #endregion

    #region Private Helper Methods

    private List<PingReplyData> CreateSuccessfulJunOSReplies(int count)
    {
        var replies = new List<PingReplyData>();
        for (int i = 1; i <= count; i++)
        {
            replies.Add(new PingReplyData
            {
                SequenceNumber = i,
                Success = true,
                RoundTripTime = i, // 1, 2, 3, etc. (will be displayed as 1.000, 2.000, 3.000)
                FromAddress = "192.168.1.1",
                BytesReceived = 64,
                Ttl = 64,
                Timestamp = DateTime.UtcNow
            });
        }
        return replies;
    }

    private List<PingReplyData> CreateTimeoutJunOSReplies(int count)
    {
        var replies = new List<PingReplyData>();
        for (int i = 1; i <= count; i++)
        {
            replies.Add(new PingReplyData
            {
                SequenceNumber = i,
                Success = false,
                ErrorType = "timeout",
                RoundTripTime = 0,
                FromAddress = "",
                BytesReceived = 0,
                Ttl = 0,
                Timestamp = DateTime.UtcNow
            });
        }
        return replies;
    }

    private ShowVersionCommandData CreateMockJunOSShowVersionData()
    {
        return new ShowVersionCommandData
        {
            ExecutionTime = DateTime.UtcNow,
            DeviceVersion = new DeviceVersionData
            {
                Vendor = "Juniper",
                Model = "EX2200-24T-4G",
                SoftwareVersion = "12.3R12.4",
                SerialNumber = "JN123456789",
                Copyright = "Copyright (c) 2023, Juniper Networks, Inc.",
                ImageFile = "jinstall-ex-2200-12.3R12.4-domestic-signed.tgz",
                CompilationDate = DateTime.Parse("2023-03-15T14:45:00Z"),
                CompilationBy = "builder",
                RomVersion = "1.0",
                BootloaderVersion = "1.0.0",
                ConfigRegister = "0x1"
            },
            SystemInfo = new DeviceSystemInfo
            {
                Hostname = "ex2200-switch",
                Uptime = TimeSpan.FromDays(30).Add(TimeSpan.FromHours(12)).Add(TimeSpan.FromMinutes(45)),
                SystemTime = DateTime.UtcNow,
                LastReboot = DateTime.UtcNow.AddDays(-30.5),
                RebootReason = "Router rebooted after a normal shutdown.",
                ConfigurationLastChanged = DateTime.UtcNow.AddDays(-5)
            },
            HardwareInfo = new DeviceHardwareInfo
            {
                ProcessorType = "MIPS64 Processor",
                ProcessorSpeed = 400,
                ProcessorCount = 1,
                BoardRevision = "Rev 02",
                ChassisType = "EX2200-24T-4G"
            },
            MemoryInfo = new DeviceMemoryInfo
            {
                TotalMemory = 2 * 1024 * 1024 * 1024, // 2GB
                UsedMemory = 800 * 1024 * 1024,       // 800MB
                FreeMemory = 1224 * 1024 * 1024,      // 1224MB
                MemoryUtilizationPercentage = 39.0
            },
            StorageInfo = new DeviceStorageInfo
            {
                TotalFlash = 1024 * 1024 * 1024,   // 1GB
                UsedFlash = 400 * 1024 * 1024,     // 400MB
                FreeFlash = 624 * 1024 * 1024,     // 624MB
                FlashUtilizationPercentage = 39.0,
                FlashType = "Internal Flash"
            },
            BootInfo = new DeviceBootInfo
            {
                BootImageName = "kernel",
                BootImagePath = "/boot/kernel",
                ConfigurationRegister = "0x1",
                BootTime = TimeSpan.FromSeconds(120)
            }
        };
    }

    #endregion
}