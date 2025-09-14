using FluentAssertions;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Formatters;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Tests.TestHelpers;
using Xunit;

namespace NetForge.Tests.Formatters;

/// <summary>
/// Comprehensive test suite for Cisco formatters following the test strategy
/// Tests Cisco IOS format accuracy for ping and show version commands
/// </summary>
public class CiscoFormatterTests
{
    private readonly CiscoPingFormatter _ciscoPingFormatter;
    private readonly CiscoShowVersionFormatter _ciscoShowVersionFormatter;
    private readonly MockNetworkDevice _mockDevice;

    public CiscoFormatterTests()
    {
        _ciscoPingFormatter = new CiscoPingFormatter();
        _ciscoShowVersionFormatter = new CiscoShowVersionFormatter();
        _mockDevice = MockDeviceBuilder.Create()
            .WithName("CiscoSwitch01")
            .WithVendor("Cisco")
            .WithInterface("GigabitEthernet0/1", "192.168.1.10", isUp: true)
            .Build();
    }

    #region Cisco Ping Formatter Tests

    [Fact]
    public void FormatPingResult_SuccessfulPings_ReturnsExclamationMarks()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 5,
            PacketsReceived = 5,
            PacketLossPercentage = 0,
            PacketSize = 100,
            TimeoutSeconds = 2,
            MinRoundTripTime = 1,
            AvgRoundTripTime = 2,
            MaxRoundTripTime = 4,
            Replies = CreateSuccessfulReplies(5)
        };

        // Act
        var result = _ciscoPingFormatter.Format(pingData, "Cisco");

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Type escape sequence to abort");
        result.Should().Contain("Sending 5, 100-byte ICMP Echos to 192.168.1.1, timeout is 2 seconds:");
        result.Should().Contain("!!!!!");
        result.Should().Contain("Success rate is 100 percent (5/5)");
        result.Should().Contain("round-trip min/avg/max = 1/2/4 ms");
    }

    [Fact]
    public void FormatPingResult_TimeoutPings_ReturnsDots()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.99",
            Success = false,
            PacketsSent = 5,
            PacketsReceived = 0,
            PacketLossPercentage = 100,
            PacketSize = 64,
            TimeoutSeconds = 2,
            Replies = CreateTimeoutReplies(5)
        };

        // Act
        var result = _ciscoPingFormatter.Format(pingData, "Cisco");

        // Assert
        result.Should().Contain("Type escape sequence to abort");
        result.Should().Contain("Sending 5, 64-byte ICMP Echos to 192.168.1.99, timeout is 2 seconds:");
        result.Should().Contain(".....");
        result.Should().Contain("Success rate is 0 percent (0/5)");
        result.Should().NotContain("round-trip min/avg/max"); // No RTT stats for timeouts
    }

    [Fact]
    public void FormatPingResult_UnreachablePings_ReturnsU()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "10.0.0.99",
            Success = false,
            PacketsSent = 5,
            PacketsReceived = 0,
            PacketLossPercentage = 100,
            PacketSize = 64,
            Replies = CreateUnreachableReplies(5)
        };

        // Act
        var result = _ciscoPingFormatter.Format(pingData, "Cisco");

        // Assert
        result.Should().Contain("UUUUU");
        result.Should().Contain("Success rate is 0 percent (0/5)");
    }

    [Fact]
    public void FormatPingResult_MixedResults_ReturnsCorrectPattern()
    {
        // Arrange
        var replies = new List<PingReplyData>
        {
            new() { SequenceNumber = 1, Success = true, RoundTripTime = 1 },
            new() { SequenceNumber = 2, Success = false, ErrorType = "timeout" },
            new() { SequenceNumber = 3, Success = true, RoundTripTime = 2 },
            new() { SequenceNumber = 4, Success = false, ErrorType = "unreachable" },
            new() { SequenceNumber = 5, Success = true, RoundTripTime = 3 }
        };

        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = false, // Mixed results = overall failure
            PacketsSent = 5,
            PacketsReceived = 3,
            PacketLossPercentage = 40,
            PacketSize = 64,
            MinRoundTripTime = 1,
            AvgRoundTripTime = 2,
            MaxRoundTripTime = 3,
            Replies = replies
        };

        // Act
        var result = _ciscoPingFormatter.Format(pingData, "Cisco");

        // Assert
        result.Should().Contain("!.!U!");
        result.Should().Contain("Success rate is 60 percent (3/5)");
        result.Should().Contain("round-trip min/avg/max = 1/2/3 ms");
    }

    [Fact]
    public void FormatPingResult_SuccessRate100_ShowsCorrectStatistics()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 10,
            PacketsReceived = 10,
            PacketLossPercentage = 0,
            PacketSize = 1500,
            MinRoundTripTime = 1,
            AvgRoundTripTime = 5,
            MaxRoundTripTime = 12,
            Replies = CreateSuccessfulReplies(10)
        };

        // Act
        var result = _ciscoPingFormatter.Format(pingData, "Cisco");

        // Assert
        result.Should().Contain("Sending 10, 1500-byte ICMP Echos");
        result.Should().Contain("Success rate is 100 percent (10/10)");
        result.Should().Contain("round-trip min/avg/max = 1/5/12 ms");
        result.Should().Contain("!!!!!!!!!!"); // 10 exclamation marks
    }

    [Fact]
    public void FormatPingResult_SuccessRate80_ShowsCorrectPercentage()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = false,
            PacketsSent = 5,
            PacketsReceived = 4,
            PacketLossPercentage = 20,
            PacketSize = 64,
            Replies = new List<PingReplyData>
            {
                new() { SequenceNumber = 1, Success = true },
                new() { SequenceNumber = 2, Success = true },
                new() { SequenceNumber = 3, Success = true },
                new() { SequenceNumber = 4, Success = true },
                new() { SequenceNumber = 5, Success = false, ErrorType = "timeout" }
            }
        };

        // Act
        var result = _ciscoPingFormatter.Format(pingData, "Cisco");

        // Assert
        result.Should().Contain("Success rate is 80 percent (4/5)");
        result.Should().Contain("!!!!.");
    }

    [Fact]
    public void CiscoPingFormatter_VendorName_ReturnsCisco()
    {
        // Act
        var vendorName = _ciscoPingFormatter.VendorName;

        // Assert
        vendorName.Should().Be("Cisco");
    }

    [Fact]
    public void CiscoPingFormatter_CanFormat_ReturnsTrueForPingCommandData()
    {
        // Arrange
        var pingData = new PingCommandData();

        // Act
        var canFormat = _ciscoPingFormatter.CanFormat(pingData, "Cisco");

        // Assert
        canFormat.Should().BeTrue();
    }

    [Fact]
    public void CiscoPingFormatter_CanFormat_ReturnsFalseForNonPingData()
    {
        // Arrange
        var showVersionData = new ShowVersionCommandData();

        // Act
        var canFormat = _ciscoPingFormatter.CanFormat(showVersionData, "Cisco");

        // Assert
        canFormat.Should().BeFalse();
    }

    #endregion

    #region Cisco Show Version Formatter Tests

    [Fact]
    public void FormatShowVersion_CiscoDevice_StartsWithCiscoIOSSoftware()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().StartWith("Cisco IOS Software");
        result.Should().Contain("Catalyst 2960-24TT-L Software");
        result.Should().Contain("Version 15.2(4)E7, RELEASE SOFTWARE (fc2)");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ContainsTechnicalSupportLine()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("Technical Support: http://www.cisco.com/techsupport");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ContainsCopyrightNotice()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("Copyright (c) 1986-2023 by Cisco Systems, Inc.");
        result.Should().Contain("Compiled");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ShowsProcessorInformation()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("PowerPC405 CPU");
        result.Should().Contain("processor (revision");
        result.Should().Contain("with 1024K/512K bytes of memory");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ShowsConfigurationRegister()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("Configuration register is 0x010F");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ShowsSystemSerialNumber()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("System serial number: FDO1234ABCD");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ShowsUptimeInCiscoFormat()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("CiscoSwitch01 uptime is");
        result.Should().Contain("weeks,");
        result.Should().Contain("days,");
        result.Should().Contain("hours,");
        result.Should().Contain("minutes");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ShowsSwitchPortsTable()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("Switch Ports Model              SW Version            SW Image");
        result.Should().Contain("*    1 24    WS-C2960-24TT-L    15.2(4)E7             C2960-LANBASEK9-M");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ShowsFlashMemoryInformation()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("bytes of flash-simulated non-volatile configuration memory");
        result.Should().Contain("Base ethernet MAC Address");
        result.Should().Contain("Motherboard assembly number");
        result.Should().Contain("Power supply part number");
    }

    [Fact]
    public void FormatShowVersion_CiscoDevice_ShowsImageFileInformation()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        result.Should().Contain("System image file is \"flash:c2960-lanbasek9-mz.152-4.E7.bin\"");
        result.Should().Contain("Last reload reason:");
    }

    [Fact]
    public void CiscoShowVersionFormatter_VendorName_ReturnsCisco()
    {
        // Act
        var vendorName = _ciscoShowVersionFormatter.VendorName;

        // Assert
        vendorName.Should().Be("Cisco");
    }

    [Fact]
    public void CiscoShowVersionFormatter_CanFormat_ReturnsTrueForShowVersionCommandData()
    {
        // Arrange
        var showVersionData = new ShowVersionCommandData();

        // Act
        var canFormat = _ciscoShowVersionFormatter.CanFormat(showVersionData, "Cisco");

        // Assert
        canFormat.Should().BeTrue();
    }

    [Fact]
    public void CiscoShowVersionFormatter_CanFormat_ReturnsFalseForNonShowVersionData()
    {
        // Arrange
        var pingData = new PingCommandData();

        // Act
        var canFormat = _ciscoShowVersionFormatter.CanFormat(pingData, "Cisco");

        // Assert
        canFormat.Should().BeFalse();
    }

    #endregion

    #region Cross-Format Consistency Tests

    [Fact]
    public void CiscoFormatters_DifferentCommands_MaintainConsistentVendorStyle()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 5,
            PacketsReceived = 5,
            PacketLossPercentage = 0
        };

        var showVersionData = CreateMockShowVersionData();

        // Act
        var pingOutput = _ciscoPingFormatter.Format(pingData, "Cisco");
        var versionOutput = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        // Both outputs should maintain Cisco's characteristic style elements
        pingOutput.Should().Contain("Type escape sequence to abort"); // Cisco ping characteristic
        versionOutput.Should().Contain("Cisco IOS Software"); // Cisco version characteristic

        // Both should avoid other vendor-specific terminology
        pingOutput.Should().NotContain("PING"); // JunOS style
        versionOutput.Should().NotContain("hostname"); // JunOS style
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void CiscoPingFormatter_StandardFormatting_CompletesWithin10ms()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 5,
            PacketsReceived = 5,
            Replies = CreateSuccessfulReplies(5)
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _ciscoPingFormatter.Format(pingData, "Cisco");

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(10);
        result.Should().NotBeNull();
    }

    [Fact]
    public void CiscoShowVersionFormatter_StandardFormatting_CompletesWithin10ms()
    {
        // Arrange
        var showVersionData = CreateMockShowVersionData();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _ciscoShowVersionFormatter.Format(showVersionData, "Cisco");

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(10);
        result.Should().NotBeNull();
    }

    #endregion

    #region Private Helper Methods

    private List<PingReplyData> CreateSuccessfulReplies(int count)
    {
        var replies = new List<PingReplyData>();
        for (int i = 1; i <= count; i++)
        {
            replies.Add(new PingReplyData
            {
                SequenceNumber = i,
                Success = true,
                RoundTripTime = i, // 1, 2, 3, etc.
                FromAddress = "192.168.1.1",
                BytesReceived = 64,
                Ttl = 64,
                Timestamp = DateTime.UtcNow
            });
        }
        return replies;
    }

    private List<PingReplyData> CreateTimeoutReplies(int count)
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

    private List<PingReplyData> CreateUnreachableReplies(int count)
    {
        var replies = new List<PingReplyData>();
        for (int i = 1; i <= count; i++)
        {
            replies.Add(new PingReplyData
            {
                SequenceNumber = i,
                Success = false,
                ErrorType = "unreachable",
                RoundTripTime = 0,
                FromAddress = "",
                BytesReceived = 0,
                Ttl = 0,
                Timestamp = DateTime.UtcNow
            });
        }
        return replies;
    }

    private ShowVersionCommandData CreateMockShowVersionData()
    {
        return new ShowVersionCommandData
        {
            ExecutionTime = DateTime.UtcNow,
            DeviceVersion = new DeviceVersionData
            {
                Vendor = "Cisco",
                Model = "Catalyst 2960-24TT-L",
                SoftwareVersion = "15.2(4)E7",
                SerialNumber = "FDO1234ABCD",
                Copyright = "Copyright (c) 1986-2023 by Cisco Systems, Inc.",
                ImageFile = "c2960-lanbasek9-mz.152-4.E7.bin",
                CompilationDate = DateTime.Parse("2023-03-15T10:30:00Z"),
                CompilationBy = "prod_rel_team",
                RomVersion = "15.0(2r)B",
                BootloaderVersion = "12.2(58r)SE2",
                ConfigRegister = "0x010F"
            },
            SystemInfo = new DeviceSystemInfo
            {
                Hostname = "CiscoSwitch01",
                Uptime = TimeSpan.FromDays(30).Add(TimeSpan.FromHours(12)).Add(TimeSpan.FromMinutes(45)),
                SystemTime = DateTime.UtcNow,
                LastReboot = DateTime.UtcNow.AddDays(-30.5),
                RebootReason = "power cycle",
                ConfigurationLastChanged = DateTime.UtcNow.AddDays(-5)
            },
            HardwareInfo = new DeviceHardwareInfo
            {
                ProcessorType = "PowerPC405",
                ProcessorSpeed = 266,
                ProcessorCount = 1,
                BoardRevision = "Rev 1.0",
                ChassisType = "Switch"
            },
            MemoryInfo = new DeviceMemoryInfo
            {
                TotalMemory = 1024 * 1024 * 1024, // 1GB
                UsedMemory = 512 * 1024 * 1024,   // 512MB
                FreeMemory = 512 * 1024 * 1024,   // 512MB
                MemoryUtilizationPercentage = 50.0
            },
            StorageInfo = new DeviceStorageInfo
            {
                TotalFlash = 32 * 1024 * 1024,   // 32MB
                UsedFlash = 16 * 1024 * 1024,    // 16MB
                FreeFlash = 16 * 1024 * 1024,    // 16MB
                FlashUtilizationPercentage = 50.0,
                FlashType = "Compact Flash"
            },
            BootInfo = new DeviceBootInfo
            {
                BootImageName = "c2960-lanbasek9-mz.152-4.E7.bin",
                BootImagePath = "flash:c2960-lanbasek9-mz.152-4.E7.bin",
                ConfigurationRegister = "0x010F",
                BootTime = TimeSpan.FromSeconds(45)
            }
        };
    }

    #endregion
}