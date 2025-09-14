using FluentAssertions;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Formatters;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Tests.TestHelpers;
using Xunit;

namespace NetForge.Tests.Formatters;

/// <summary>
/// Cross-formatter consistency tests following the test strategy
/// Tests business logic consistency across different vendor formatters
/// </summary>
public class FormatterConsistencyTests
{
    private readonly CiscoPingFormatter _ciscoPingFormatter;
    private readonly JuniperPingFormatter _juniperPingFormatter;
    private readonly DefaultCommandFormatter _defaultFormatter;
    private readonly CiscoShowVersionFormatter _ciscoShowVersionFormatter;
    private readonly JuniperShowVersionFormatter _juniperShowVersionFormatter;
    private readonly DefaultShowVersionFormatter _defaultShowVersionFormatter;

    public FormatterConsistencyTests()
    {
        _ciscoPingFormatter = new CiscoPingFormatter();
        _juniperPingFormatter = new JuniperPingFormatter();
        _defaultFormatter = new DefaultCommandFormatter();
        _ciscoShowVersionFormatter = new CiscoShowVersionFormatter();
        _juniperShowVersionFormatter = new JuniperShowVersionFormatter();
        _defaultShowVersionFormatter = new DefaultShowVersionFormatter();
    }

    #region Business Logic Consistency Tests

    [Fact]
    public void SamePingData_DifferentFormatters_ProducesSameBusinessResults()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.1",
            Success = true,
            PacketsSent = 5,
            PacketsReceived = 4,
            PacketLossPercentage = 20,
            PacketSize = 64,
            MinRoundTripTime = 1,
            AvgRoundTripTime = 3,
            MaxRoundTripTime = 8,
            StandardDeviation = 2.5,
            Replies = CreateMixedPingReplies()
        };

        // Act
        var ciscoOutput = _ciscoPingFormatter.Format(pingData, "Cisco");
        var juniperOutput = _juniperPingFormatter.Format(pingData, "Juniper");
        var defaultOutput = _defaultFormatter.Format(pingData, "Default");

        // Assert
        // All outputs should contain the same core business data
        AssertContainsBusinessData(ciscoOutput, "192.168.1.1", 5, 4, 20);
        AssertContainsBusinessData(juniperOutput, "192.168.1.1", 5, 4, 20);
        AssertContainsBusinessData(defaultOutput, "192.168.1.1", 5, 4, 20);

        // All outputs should contain RTT information for successful packets
        AssertContainsRTTData(ciscoOutput, 1, 3, 8);
        AssertContainsRTTData(juniperOutput, 1, 3, 8);
        AssertContainsRTTData(defaultOutput, 1, 3, 8);
    }

    [Fact]
    public void SameVersionData_DifferentFormatters_ShowsSameDeviceInfo()
    {
        // Arrange
        var versionData = CreateConsistentShowVersionData();

        // Act
        var ciscoOutput = _ciscoShowVersionFormatter.Format(versionData, "Cisco");
        var juniperOutput = _juniperShowVersionFormatter.Format(versionData, "Juniper");
        var defaultOutput = _defaultShowVersionFormatter.Format(versionData, "Default");

        // Assert
        // All outputs should contain the same core device information
        AssertContainsDeviceInfo(ciscoOutput, "TestDevice", "Cisco", "15.2(4)E7", "FDO1234ABCD");
        AssertContainsDeviceInfo(juniperOutput, "TestDevice", "Cisco", "15.2(4)E7", "FDO1234ABCD");
        AssertContainsDeviceInfo(defaultOutput, "TestDevice", "Cisco", "15.2(4)E7", "FDO1234ABCD");

        // All outputs should contain uptime information
        AssertContainsUptimeInfo(ciscoOutput);
        AssertContainsUptimeInfo(juniperOutput);
        AssertContainsUptimeInfo(defaultOutput);
    }

    [Fact]
    public void ErrorConditions_AllFormatters_HandleErrorsConsistently()
    {
        // Arrange
        var failedPingData = new PingCommandData
        {
            Destination = "unreachable.host",
            Success = false,
            PacketsSent = 5,
            PacketsReceived = 0,
            PacketLossPercentage = 100,
            PacketSize = 64,
            ErrorMessage = "Host unreachable",
            Replies = CreateFailedPingReplies(5)
        };

        // Act
        var ciscoOutput = _ciscoPingFormatter.Format(failedPingData, "Cisco");
        var juniperOutput = _juniperPingFormatter.Format(failedPingData, "Juniper");
        var defaultOutput = _defaultFormatter.Format(failedPingData, "Default");

        // Assert
        // All formatters should indicate failure
        AssertIndicatesFailure(ciscoOutput, "unreachable.host", 100);
        AssertIndicatesFailure(juniperOutput, "unreachable.host", 100);
        AssertIndicatesFailure(defaultOutput, "unreachable.host", 100);

        // None should show RTT statistics for complete failures
        ciscoOutput.Should().NotContain("round-trip min/avg/max");
        juniperOutput.Should().NotContain("round-trip min/avg/max");
        defaultOutput.Should().NotContain("round-trip min/avg/max");
    }

    [Theory]
    [InlineData("Cisco")]
    [InlineData("Juniper")]
    [InlineData("Nokia")]
    [InlineData("Arista")]
    [InlineData("Default")]
    public void VendorName_AllFormatters_ReturnsCorrectVendorIdentification(string expectedVendor)
    {
        // Arrange
        var formatter = GetFormatterForVendor(expectedVendor);

        // Act
        var vendorName = formatter?.VendorName;

        // Assert
        if (formatter != null)
        {
            vendorName.Should().Be(expectedVendor);
        }
    }

    [Fact]
    public void CanFormat_AllFormatters_CorrectlyIdentifiesSupportedTypes()
    {
        // Arrange
        var pingData = new PingCommandData();
        var versionData = new ShowVersionCommandData();
        var unknownData = new object(); // Unsupported type

        var formatters = new IVendorCommandFormatter[]
        {
            _ciscoPingFormatter,
            _juniperPingFormatter,
            _ciscoShowVersionFormatter,
            _juniperShowVersionFormatter,
            _defaultFormatter,
            _defaultShowVersionFormatter
        };

        // Act & Assert
        foreach (var formatter in formatters)
        {
            var vendorName = formatter.VendorName;

            // Ping formatters should support PingCommandData
            if (formatter is CiscoPingFormatter or JuniperPingFormatter)
            {
                formatter.CanFormat(pingData, vendorName).Should().BeTrue();
                formatter.CanFormat(versionData, vendorName).Should().BeFalse();
            }
            // Show version formatters should support ShowVersionCommandData
            else if (formatter is CiscoShowVersionFormatter or JuniperShowVersionFormatter)
            {
                formatter.CanFormat(versionData, vendorName).Should().BeTrue();
                formatter.CanFormat(pingData, vendorName).Should().BeFalse();
            }
            // Default formatter should support both
            else if (formatter is DefaultCommandFormatter)
            {
                formatter.CanFormat(pingData, vendorName).Should().BeTrue();
                formatter.CanFormat(versionData, vendorName).Should().BeFalse(); // This is ping formatter
            }
            else if (formatter is DefaultShowVersionFormatter)
            {
                formatter.CanFormat(versionData, vendorName).Should().BeTrue();
                formatter.CanFormat(pingData, vendorName).Should().BeFalse(); // This is show version formatter
            }

            // No formatter should support unknown types
            formatter.CanFormat(unknownData, vendorName).Should().BeFalse();
        }
    }

    #endregion

    #region Data Preservation Tests

    [Fact]
    public void PingFormatters_PreserveAllCriticalData()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Destination = "192.168.1.100",
            ResolvedAddress = "192.168.1.100",
            Success = true,
            PacketsSent = 10,
            PacketsReceived = 8,
            PacketLossPercentage = 20,
            PacketSize = 1500,
            TimeoutSeconds = 5,
            MinRoundTripTime = 1,
            AvgRoundTripTime = 15,
            MaxRoundTripTime = 45,
            StandardDeviation = 12.5,
            SourceInterface = "eth0",
            SourceIpAddress = "192.168.1.10",
            Ttl = 64,
            Replies = CreateDetailedPingReplies()
        };

        // Act
        var ciscoOutput = _ciscoPingFormatter.Format(pingData, "Cisco");
        var juniperOutput = _juniperPingFormatter.Format(pingData, "Juniper");

        // Assert
        // Verify all critical data is preserved in both formats
        foreach (var output in new[] { ciscoOutput, juniperOutput })
        {
            output.Should().Contain("192.168.1.100"); // Destination
            output.Should().Contain("10"); // Packets sent
            output.Should().Contain("8"); // Packets received
            output.Should().Contain("20"); // Packet loss percentage
            output.Should().Contain("1500"); // Packet size (in some form)
            // RTT data should be present
            output.Should().MatchRegex(@"\b1\b.*\b15\b.*\b45\b"); // min/avg/max pattern
        }
    }

    [Fact]
    public void ShowVersionFormatters_PreserveAllCriticalData()
    {
        // Arrange
        var versionData = CreateDetailedShowVersionData();

        // Act
        var ciscoOutput = _ciscoShowVersionFormatter.Format(versionData, "Cisco");
        var juniperOutput = _juniperShowVersionFormatter.Format(versionData, "Juniper");

        // Assert
        // Verify all critical device data is preserved
        foreach (var output in new[] { ciscoOutput, juniperOutput })
        {
            output.Should().Contain("RouterDevice"); // Hostname
            output.Should().Contain("Cisco"); // Vendor
            output.Should().Contain("16.12.04"); // Software version
            output.Should().Contain("ABC1234DEF5"); // Serial number
            output.Should().Contain("PowerPC"); // Processor type
            output.Should().Contain("800"); // Processor speed
            // Memory information should be present
            output.Should().MatchRegex(@"\b4096\b|\b4.*GB\b"); // Total memory
        }
    }

    #endregion

    #region Vendor Style Consistency Tests

    [Fact]
    public void CiscoFormatters_MaintainCiscoStyleAcrossCommands()
    {
        // Arrange
        var pingData = new PingCommandData { Success = true, PacketsSent = 5, PacketsReceived = 5 };
        var versionData = CreateConsistentShowVersionData();

        // Act
        var pingOutput = _ciscoPingFormatter.Format(pingData, "Cisco");
        var versionOutput = _ciscoShowVersionFormatter.Format(versionData, "Cisco");

        // Assert
        // Both should use Cisco-specific terminology and style
        pingOutput.Should().Contain("Type escape sequence to abort"); // Cisco ping style
        versionOutput.Should().Contain("Cisco IOS Software"); // Cisco version style

        // Neither should use other vendors' style
        pingOutput.Should().NotContain("PING"); // JunOS style
        versionOutput.Should().NotContain("Hostname:"); // JunOS style
    }

    [Fact]
    public void JuniperFormatters_MaintainJunOSStyleAcrossCommands()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Success = true,
            PacketsSent = 3,
            PacketsReceived = 3,
            Replies = CreateSuccessfulReplies(3)
        };
        var versionData = CreateConsistentShowVersionData();

        // Act
        var pingOutput = _juniperPingFormatter.Format(pingData, "Juniper");
        var versionOutput = _juniperShowVersionFormatter.Format(versionData, "Juniper");

        // Assert
        // Both should use JunOS-specific terminology and style
        pingOutput.Should().Contain("PING"); // JunOS ping style
        pingOutput.Should().Contain("icmp_seq="); // JunOS sequence format
        versionOutput.Should().Contain("Hostname:"); // JunOS version style

        // Neither should use other vendors' style
        pingOutput.Should().NotContain("Type escape sequence to abort"); // Cisco style
        versionOutput.Should().NotContain("Cisco IOS Software"); // Cisco style
    }

    #endregion

    #region Performance Consistency Tests

    [Fact]
    public void AllFormatters_PerformConsistently()
    {
        // Arrange
        var pingData = new PingCommandData
        {
            Success = true,
            PacketsSent = 10,
            PacketsReceived = 10,
            Replies = CreateSuccessfulReplies(10)
        };

        var formatters = new IVendorCommandFormatter[]
        {
            _ciscoPingFormatter,
            _juniperPingFormatter,
            _defaultFormatter
        };

        // Act & Assert
        foreach (var formatter in formatters)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = formatter.Format(pingData, formatter.VendorName);
            stopwatch.Stop();

            result.Should().NotBeNull();
            stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(15); // Consistent performance threshold
        }
    }

    #endregion

    #region Private Helper Methods

    private List<PingReplyData> CreateMixedPingReplies()
    {
        return new List<PingReplyData>
        {
            new() { SequenceNumber = 1, Success = true, RoundTripTime = 1, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 },
            new() { SequenceNumber = 2, Success = false, ErrorType = "timeout", RoundTripTime = 0 },
            new() { SequenceNumber = 3, Success = true, RoundTripTime = 8, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 },
            new() { SequenceNumber = 4, Success = true, RoundTripTime = 2, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 },
            new() { SequenceNumber = 5, Success = true, RoundTripTime = 3, FromAddress = "192.168.1.1", BytesReceived = 64, Ttl = 64 }
        };
    }

    private List<PingReplyData> CreateFailedPingReplies(int count)
    {
        var replies = new List<PingReplyData>();
        for (int i = 1; i <= count; i++)
        {
            replies.Add(new PingReplyData
            {
                SequenceNumber = i,
                Success = false,
                ErrorType = "unreachable",
                RoundTripTime = 0
            });
        }
        return replies;
    }

    private List<PingReplyData> CreateSuccessfulReplies(int count)
    {
        var replies = new List<PingReplyData>();
        for (int i = 1; i <= count; i++)
        {
            replies.Add(new PingReplyData
            {
                SequenceNumber = i,
                Success = true,
                RoundTripTime = i,
                FromAddress = "192.168.1.1",
                BytesReceived = 64,
                Ttl = 64
            });
        }
        return replies;
    }

    private List<PingReplyData> CreateDetailedPingReplies()
    {
        return new List<PingReplyData>
        {
            new() { SequenceNumber = 1, Success = true, RoundTripTime = 1, FromAddress = "192.168.1.100", BytesReceived = 1500, Ttl = 64 },
            new() { SequenceNumber = 2, Success = true, RoundTripTime = 15, FromAddress = "192.168.1.100", BytesReceived = 1500, Ttl = 64 },
            new() { SequenceNumber = 3, Success = true, RoundTripTime = 45, FromAddress = "192.168.1.100", BytesReceived = 1500, Ttl = 64 },
            new() { SequenceNumber = 4, Success = true, RoundTripTime = 8, FromAddress = "192.168.1.100", BytesReceived = 1500, Ttl = 64 },
            new() { SequenceNumber = 5, Success = false, ErrorType = "timeout" },
            new() { SequenceNumber = 6, Success = true, RoundTripTime = 12, FromAddress = "192.168.1.100", BytesReceived = 1500, Ttl = 64 },
            new() { SequenceNumber = 7, Success = true, RoundTripTime = 20, FromAddress = "192.168.1.100", BytesReceived = 1500, Ttl = 64 },
            new() { SequenceNumber = 8, Success = true, RoundTripTime = 5, FromAddress = "192.168.1.100", BytesReceived = 1500, Ttl = 64 },
            new() { SequenceNumber = 9, Success = true, RoundTripTime = 25, FromAddress = "192.168.1.100", BytesReceived = 1500, Ttl = 64 },
            new() { SequenceNumber = 10, Success = false, ErrorType = "timeout" }
        };
    }

    private ShowVersionCommandData CreateConsistentShowVersionData()
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
                ConfigRegister = "0x010F"
            },
            SystemInfo = new DeviceSystemInfo
            {
                Hostname = "TestDevice",
                Uptime = TimeSpan.FromDays(45).Add(TimeSpan.FromHours(6)),
                RebootReason = "power cycle"
            },
            HardwareInfo = new DeviceHardwareInfo
            {
                ProcessorType = "PowerPC405",
                ProcessorSpeed = 266,
                ProcessorCount = 1
            },
            MemoryInfo = new DeviceMemoryInfo
            {
                TotalMemory = 1024 * 1024 * 1024, // 1GB
                UsedMemory = 512 * 1024 * 1024
            },
            StorageInfo = new DeviceStorageInfo
            {
                TotalFlash = 32 * 1024 * 1024, // 32MB
                FlashType = "Compact Flash"
            },
            BootInfo = new DeviceBootInfo
            {
                ConfigurationRegister = "0x010F"
            }
        };
    }

    private ShowVersionCommandData CreateDetailedShowVersionData()
    {
        return new ShowVersionCommandData
        {
            ExecutionTime = DateTime.UtcNow,
            DeviceVersion = new DeviceVersionData
            {
                Vendor = "Cisco",
                Model = "ISR 4321",
                SoftwareVersion = "16.12.04",
                SerialNumber = "ABC1234DEF5",
                Copyright = "Copyright (c) 1986-2023 by Cisco Systems, Inc."
            },
            SystemInfo = new DeviceSystemInfo
            {
                Hostname = "RouterDevice",
                Uptime = TimeSpan.FromDays(120),
                RebootReason = "reload"
            },
            HardwareInfo = new DeviceHardwareInfo
            {
                ProcessorType = "PowerPC",
                ProcessorSpeed = 800,
                ProcessorCount = 2
            },
            MemoryInfo = new DeviceMemoryInfo
            {
                TotalMemory = 4L * 1024 * 1024 * 1024, // 4GB
                UsedMemory = 2L * 1024 * 1024 * 1024
            },
            StorageInfo = new DeviceStorageInfo
            {
                TotalFlash = 8L * 1024 * 1024 * 1024, // 8GB
                FlashType = "eMMC"
            }
        };
    }

    private IVendorCommandFormatter? GetFormatterForVendor(string vendor)
    {
        return vendor switch
        {
            "Cisco" => _ciscoPingFormatter,
            "Juniper" => _juniperPingFormatter,
            "Default" => _defaultFormatter,
            _ => null
        };
    }

    private static void AssertContainsBusinessData(string output, string destination, int sent, int received, double lossPercentage)
    {
        output.Should().Contain(destination);
        output.Should().Contain(sent.ToString());
        output.Should().Contain(received.ToString());
        output.Should().Contain(lossPercentage.ToString("F0")); // Loss percentage as integer
    }

    private static void AssertContainsRTTData(string output, int min, int avg, int max)
    {
        // Should contain RTT statistics in some form
        var rttPattern = $@"\b{min}\b.*\b{avg}\b.*\b{max}\b";
        output.Should().MatchRegex(rttPattern);
    }

    private static void AssertContainsDeviceInfo(string output, string hostname, string vendor, string version, string serial)
    {
        output.Should().Contain(hostname);
        output.Should().Contain(vendor);
        output.Should().Contain(version);
        output.Should().Contain(serial);
    }

    private static void AssertContainsUptimeInfo(string output)
    {
        // Should contain some form of uptime information
        output.Should().MatchRegex(@"(\bday|\bweek|\bhour|\bminute|\buptime)");
    }

    private static void AssertIndicatesFailure(string output, string destination, double lossPercentage)
    {
        output.Should().Contain(destination);
        output.Should().Contain(lossPercentage.ToString("F0"));
        // Should indicate failure (0% success or 100% loss)
        output.Should().MatchRegex(@"(0 percent|100.*loss|0.*received)");
    }

    #endregion
}