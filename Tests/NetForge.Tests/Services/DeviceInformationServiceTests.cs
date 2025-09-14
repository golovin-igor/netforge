using FluentAssertions;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Common.Common;
using NetForge.Tests.TestHelpers;
using Xunit;

namespace NetForge.Tests.Services;

/// <summary>
/// Comprehensive test suite for DeviceInformationService following the test strategy
/// Tests device information accuracy, vendor-specific behavior, and data consistency
/// </summary>
public class DeviceInformationServiceTests
{
    private readonly IDeviceInformationService _deviceInfoService;

    public DeviceInformationServiceTests()
    {
        _deviceInfoService = new DeviceInformationService();
    }

    #region Device Information Accuracy Tests

    [Fact]
    public void GetDeviceVersion_CiscoDevice_ReturnsCiscoSpecificInformation()
    {
        // Arrange
        var ciscoDevice = MockDeviceBuilder.Create()
            .WithName("CiscoSwitch01")
            .WithVendor("Cisco")
            .Build();

        // Act
        var result = _deviceInfoService.GetDeviceVersion(ciscoDevice);

        // Assert
        result.Should().NotBeNull();
        result.Vendor.Should().Be("Cisco");
        result.Model.Should().Contain("Catalyst");
        result.SoftwareVersion.Should().MatchRegex(@"\d+\.\d+.*"); // Version pattern
        result.SerialNumber.Should().NotBeNullOrEmpty();
        result.SerialNumber.Should().StartWith("FDO"); // Cisco serial pattern
        result.Copyright.Should().Contain("Cisco Systems");
    }

    [Fact]
    public void GetDeviceVersion_JuniperDevice_ReturnsJuniperSpecificInformation()
    {
        // Arrange
        var juniperDevice = MockDeviceBuilder.Create()
            .WithName("JuniperSwitch01")
            .WithVendor("Juniper")
            .Build();

        // Act
        var result = _deviceInfoService.GetDeviceVersion(juniperDevice);

        // Assert
        result.Should().NotBeNull();
        result.Vendor.Should().Be("Juniper");
        result.Model.Should().Contain("EX");
        result.SoftwareVersion.Should().MatchRegex(@"\d+\.\d+R\d+.*"); // JunOS version pattern
        result.SerialNumber.Should().NotBeNullOrEmpty();
        result.SerialNumber.Should().MatchRegex(@"[A-Z]{2}\d{10}"); // Juniper serial pattern
        result.Copyright.Should().Contain("Juniper Networks");
    }

    [Fact]
    public void GetDeviceVersion_NokiaDevice_ReturnsNokiaSpecificInformation()
    {
        // Arrange
        var nokiaDevice = MockDeviceBuilder.Create()
            .WithName("NokiaRouter01")
            .WithVendor("Nokia")
            .Build();

        // Act
        var result = _deviceInfoService.GetDeviceVersion(nokiaDevice);

        // Assert
        result.Should().NotBeNull();
        result.Vendor.Should().Be("Nokia");
        result.Model.Should().Contain("7750");
        result.SoftwareVersion.Should().MatchRegex(@"B-\d+\.\d+.*"); // Nokia version pattern
        result.Copyright.Should().Contain("Nokia");
    }

    [Fact]
    public void GetDeviceVersion_UnknownVendor_ReturnsGenericInformation()
    {
        // Arrange
        var genericDevice = MockDeviceBuilder.Create()
            .WithName("GenericDevice01")
            .WithVendor("UnknownVendor")
            .Build();

        // Act
        var result = _deviceInfoService.GetDeviceVersion(genericDevice);

        // Assert
        result.Should().NotBeNull();
        result.Vendor.Should().Be("UnknownVendor");
        result.Model.Should().Be("Generic Network Device");
        result.SoftwareVersion.Should().Be("1.0.0");
        result.SerialNumber.Should().NotBeNullOrEmpty();
        result.Copyright.Should().Contain("Network Device");
    }

    [Fact]
    public void GetSystemInfo_ValidDevice_ReturnsAccurateSystemInfo()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("TestDevice")
            .WithVendor("Cisco")
            .Build();

        // Act
        var result = _deviceInfoService.GetSystemInfo(device);

        // Assert
        result.Should().NotBeNull();
        result.Hostname.Should().Be("TestDevice");
        result.Uptime.Should().BeGreaterThan(TimeSpan.Zero);
        result.SystemTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.LastReboot.Should().BeBefore(DateTime.UtcNow);
        result.RebootReason.Should().NotBeNullOrEmpty();
        result.ConfigurationLastChanged.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void GetHardwareInfo_ValidDevice_ReturnsRealisticHardwareSpecs()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithVendor("Cisco")
            .Build();

        // Act
        var result = _deviceInfoService.GetHardwareInfo(device);

        // Assert
        result.Should().NotBeNull();
        result.ProcessorType.Should().NotBeNullOrEmpty();
        result.ProcessorSpeed.Should().BeGreaterThan(0);
        result.ProcessorCount.Should().BeGreaterThan(0);
        result.BoardRevision.Should().NotBeNullOrEmpty();
        result.ChassisType.Should().NotBeNullOrEmpty();
        result.PowerSupplies.Should().NotBeEmpty();
        result.Fans.Should().NotBeEmpty();
        result.TemperatureSensors.Should().NotBeEmpty();
    }

    [Fact]
    public void GetUptime_ConsistentDevice_ReturnsSameUptimeAcrossRuns()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("ConsistentDevice")
            .WithVendor("Cisco")
            .Build();

        // Act
        var uptime1 = _deviceInfoService.GetSystemInfo(device).Uptime;
        Thread.Sleep(100); // Small delay
        var uptime2 = _deviceInfoService.GetSystemInfo(device).Uptime;

        // Assert
        // Uptime should be consistent for the same device (simulated)
        uptime1.Should().BeCloseTo(uptime2, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetMemoryInfo_HighEndDevice_ReturnsLargerMemoryAllocation()
    {
        // Arrange
        var highEndDevice = MockDeviceBuilder.Create()
            .WithVendor("Nokia") // Nokia typically has higher memory
            .Build();

        var basicDevice = MockDeviceBuilder.Create()
            .WithVendor("MikroTik") // MikroTik typically has lower memory
            .Build();

        // Act
        var highEndMemory = _deviceInfoService.GetMemoryInfo(highEndDevice);
        var basicMemory = _deviceInfoService.GetMemoryInfo(basicDevice);

        // Assert
        highEndMemory.TotalMemory.Should().BeGreaterThan(basicMemory.TotalMemory);
        highEndMemory.FreeMemory.Should().BeGreaterThan(basicMemory.FreeMemory);
        highEndMemory.UsedMemory.Should().BeGreaterThan(basicMemory.UsedMemory);
    }

    [Fact]
    public void GetStorageInfo_VendorSpecific_ReturnsCorrectFlashType()
    {
        // Arrange
        var ciscoDevice = MockDeviceBuilder.Create().WithVendor("Cisco").Build();
        var juniperDevice = MockDeviceBuilder.Create().WithVendor("Juniper").Build();

        // Act
        var ciscoStorage = _deviceInfoService.GetStorageInfo(ciscoDevice);
        var juniperStorage = _deviceInfoService.GetStorageInfo(juniperDevice);

        // Assert
        ciscoStorage.FlashType.Should().Be("Compact Flash");
        juniperStorage.FlashType.Should().Be("Internal Flash");

        ciscoStorage.TotalFlash.Should().BeGreaterThan(0);
        ciscoStorage.FreeFlash.Should().BeGreaterThan(0);
        ciscoStorage.UsedFlash.Should().Be(ciscoStorage.TotalFlash - ciscoStorage.FreeFlash);

        juniperStorage.TotalFlash.Should().BeGreaterThan(0);
        juniperStorage.FreeFlash.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetBootInfo_VendorSpecific_ReturnsCorrectBootSequence()
    {
        // Arrange
        var ciscoDevice = MockDeviceBuilder.Create().WithVendor("Cisco").Build();

        // Act
        var result = _deviceInfoService.GetBootInfo(ciscoDevice);

        // Assert
        result.Should().NotBeNull();
        result.BootImageName.Should().NotBeNullOrEmpty();
        result.BootImagePath.Should().NotBeNullOrEmpty();
        result.ConfigurationRegister.Should().NotBeNullOrEmpty();
        result.BootTime.Should().BeGreaterThan(TimeSpan.Zero);
        result.BootOrder.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateSerialNumber_SameDevice_ReturnsConsistentSerial()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("TestDevice123")
            .WithVendor("Cisco")
            .Build();

        // Act
        var serial1 = _deviceInfoService.GetDeviceVersion(device).SerialNumber;
        var serial2 = _deviceInfoService.GetDeviceVersion(device).SerialNumber;

        // Assert
        serial1.Should().Be(serial2); // Same device should have consistent serial
        serial1.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Vendor-Specific Behavior Tests

    [Theory]
    [InlineData("Cisco", "Catalyst")]
    [InlineData("Juniper", "EX")]
    [InlineData("Nokia", "7750")]
    [InlineData("Arista", "DCS")]
    [InlineData("Extreme", "X")]
    public void GetDeviceModel_VendorSpecific_ReturnsExpectedModel(string vendor, string expectedModelPrefix)
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor(vendor).Build();

        // Act
        var result = _deviceInfoService.GetDeviceVersion(device);

        // Assert
        result.Model.Should().Contain(expectedModelPrefix);
    }

    [Theory]
    [InlineData("Cisco", @"\d+\.\d+.*")]
    [InlineData("Juniper", @"\d+\.\d+R\d+.*")]
    [InlineData("Nokia", @"B-\d+\.\d+.*")]
    [InlineData("Arista", @"\d+\.\d+\.\d+.*")]
    public void GetSoftwareVersion_VendorSpecific_MatchesExpectedPattern(string vendor, string expectedPattern)
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor(vendor).Build();

        // Act
        var result = _deviceInfoService.GetDeviceVersion(device);

        // Assert
        result.SoftwareVersion.Should().MatchRegex(expectedPattern);
    }

    [Theory]
    [InlineData("Cisco", "PowerPC")]
    [InlineData("Juniper", "MIPS")]
    [InlineData("Nokia", "x86")]
    [InlineData("Arista", "x86_64")]
    public void GetProcessorType_VendorSpecific_ReturnsExpectedProcessor(string vendor, string expectedProcessor)
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor(vendor).Build();

        // Act
        var result = _deviceInfoService.GetHardwareInfo(device);

        // Assert
        result.ProcessorType.Should().Contain(expectedProcessor);
    }

    [Theory]
    [InlineData("Nokia", 8.0)] // High-end multiplier
    [InlineData("F5", 6.0)]    // High-end multiplier
    [InlineData("Cisco", 4.0)] // Standard multiplier
    [InlineData("MikroTik", 1.0)] // Basic multiplier
    public void GetMemoryMultiplier_VendorSpecific_ReturnsExpectedMultiplier(string vendor, double expectedMinMultiplier)
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor(vendor).Build();

        // Act
        var memoryInfo = _deviceInfoService.GetMemoryInfo(device);

        // Assert
        // Base memory is typically 256MB, so we can calculate the effective multiplier
        var baseMemory = 256 * 1024 * 1024; // 256MB in bytes
        var effectiveMultiplier = (double)memoryInfo.TotalMemory / baseMemory;
        effectiveMultiplier.Should().BeGreaterOrEqualTo(expectedMinMultiplier);
    }

    #endregion

    #region Data Consistency Tests

    [Fact]
    public void GetDeviceVersion_MultipleCallsSameDevice_ReturnsConsistentData()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("ConsistentDevice")
            .WithVendor("Cisco")
            .Build();

        // Act
        var result1 = _deviceInfoService.GetDeviceVersion(device);
        var result2 = _deviceInfoService.GetDeviceVersion(device);

        // Assert
        result1.SerialNumber.Should().Be(result2.SerialNumber);
        result1.Model.Should().Be(result2.Model);
        result1.SoftwareVersion.Should().Be(result2.SoftwareVersion);
        result1.Vendor.Should().Be(result2.Vendor);
    }

    [Fact]
    public void GetSystemInfo_MultipleCallsSameDevice_HasConsistentUptime()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("UptimeDevice")
            .Build();

        // Act
        var info1 = _deviceInfoService.GetSystemInfo(device);
        Thread.Sleep(50);
        var info2 = _deviceInfoService.GetSystemInfo(device);

        // Assert
        info1.LastReboot.Should().Be(info2.LastReboot); // Boot time should be consistent
        info1.Hostname.Should().Be(info2.Hostname);
    }

    [Fact]
    public void GetHardwareInfo_MultipleCallsSameDevice_ReturnsIdenticalSpecs()
    {
        // Arrange
        var device = MockDeviceBuilder.Create()
            .WithName("HardwareDevice")
            .WithVendor("Juniper")
            .Build();

        // Act
        var hw1 = _deviceInfoService.GetHardwareInfo(device);
        var hw2 = _deviceInfoService.GetHardwareInfo(device);

        // Assert
        hw1.ProcessorType.Should().Be(hw2.ProcessorType);
        hw1.ProcessorSpeed.Should().Be(hw2.ProcessorSpeed);
        hw1.ProcessorCount.Should().Be(hw2.ProcessorCount);
        hw1.BoardRevision.Should().Be(hw2.BoardRevision);
        hw1.ChassisType.Should().Be(hw2.ChassisType);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void GetDeviceVersion_NullDevice_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => _deviceInfoService.GetDeviceVersion(null);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("device");
    }

    [Fact]
    public void GetSystemInfo_NullDevice_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => _deviceInfoService.GetSystemInfo(null);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("device");
    }

    [Fact]
    public void GetHardwareInfo_NullDevice_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => _deviceInfoService.GetHardwareInfo(null);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("device");
    }

    [Fact]
    public void GetMemoryInfo_NullDevice_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => _deviceInfoService.GetMemoryInfo(null);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("device");
    }

    [Fact]
    public void GetStorageInfo_NullDevice_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => _deviceInfoService.GetStorageInfo(null);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("device");
    }

    [Fact]
    public void GetBootInfo_NullDevice_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => _deviceInfoService.GetBootInfo(null);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("device");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void GetDeviceVersion_StandardCall_CompletesWithin50ms()
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor("Cisco").Build();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _deviceInfoService.GetDeviceVersion(device);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(50);
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetSystemInfo_StandardCall_CompletesWithin25ms()
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor("Juniper").Build();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = _deviceInfoService.GetSystemInfo(device);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(25);
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetAllDeviceInfo_CombinedCalls_CompletesWithin100ms()
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor("Nokia").Build();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var version = _deviceInfoService.GetDeviceVersion(device);
        var system = _deviceInfoService.GetSystemInfo(device);
        var hardware = _deviceInfoService.GetHardwareInfo(device);
        var memory = _deviceInfoService.GetMemoryInfo(device);
        var storage = _deviceInfoService.GetStorageInfo(device);
        var boot = _deviceInfoService.GetBootInfo(device);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(100);
        version.Should().NotBeNull();
        system.Should().NotBeNull();
        hardware.Should().NotBeNull();
        memory.Should().NotBeNull();
        storage.Should().NotBeNull();
        boot.Should().NotBeNull();
    }

    #endregion

    #region Memory and Storage Validation Tests

    [Fact]
    public void GetMemoryInfo_ValidDevice_ReturnsLogicalMemoryValues()
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor("Cisco").Build();

        // Act
        var result = _deviceInfoService.GetMemoryInfo(device);

        // Assert
        result.TotalMemory.Should().BeGreaterThan(0);
        result.UsedMemory.Should().BeGreaterThan(0);
        result.FreeMemory.Should().BeGreaterThan(0);
        result.UsedMemory.Should().BeLessOrEqualTo(result.TotalMemory);
        result.FreeMemory.Should().BeLessOrEqualTo(result.TotalMemory);
        (result.UsedMemory + result.FreeMemory).Should().Be(result.TotalMemory);
        result.MemoryUtilizationPercentage.Should().BeInRange(0, 100);
    }

    [Fact]
    public void GetStorageInfo_ValidDevice_ReturnsLogicalStorageValues()
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor("Juniper").Build();

        // Act
        var result = _deviceInfoService.GetStorageInfo(device);

        // Assert
        result.TotalFlash.Should().BeGreaterThan(0);
        result.UsedFlash.Should().BeGreaterOrEqualTo(0);
        result.FreeFlash.Should().BeGreaterThan(0);
        result.UsedFlash.Should().BeLessOrEqualTo(result.TotalFlash);
        result.FreeFlash.Should().BeLessOrEqualTo(result.TotalFlash);
        (result.UsedFlash + result.FreeFlash).Should().Be(result.TotalFlash);
        result.FlashUtilizationPercentage.Should().BeInRange(0, 100);
    }

    [Fact]
    public void GetHardwareInfo_ValidDevice_ReturnsReasonableProcessorSpecs()
    {
        // Arrange
        var device = MockDeviceBuilder.Create().WithVendor("Arista").Build();

        // Act
        var result = _deviceInfoService.GetHardwareInfo(device);

        // Assert
        result.ProcessorSpeed.Should().BeInRange(100, 5000); // 100MHz to 5GHz reasonable range
        result.ProcessorCount.Should().BeInRange(1, 16); // 1 to 16 cores reasonable
        result.PowerSupplies.Should().NotBeEmpty();
        result.PowerSupplies.Should().OnlyContain(ps => !string.IsNullOrEmpty(ps));
        result.Fans.Should().NotBeEmpty();
        result.Fans.Should().OnlyContain(fan => !string.IsNullOrEmpty(fan));
        result.TemperatureSensors.Should().NotBeEmpty();
        result.TemperatureSensors.Should().OnlyContain(sensor => !string.IsNullOrEmpty(sensor));
    }

    #endregion

    #region Realistic Hardware Simulation Tests

    [Fact]
    public void GetDeviceVersion_DifferentDevicesSameVendor_HaveDifferentSerials()
    {
        // Arrange
        var device1 = MockDeviceBuilder.Create().WithName("Device1").WithVendor("Cisco").Build();
        var device2 = MockDeviceBuilder.Create().WithName("Device2").WithVendor("Cisco").Build();

        // Act
        var version1 = _deviceInfoService.GetDeviceVersion(device1);
        var version2 = _deviceInfoService.GetDeviceVersion(device2);

        // Assert
        version1.SerialNumber.Should().NotBe(version2.SerialNumber);
        version1.SerialNumber.Should().StartWith("FDO"); // Cisco pattern
        version2.SerialNumber.Should().StartWith("FDO"); // Cisco pattern
    }

    [Fact]
    public void GetSystemInfo_RebootReasonVariety_ShowsReasonableReasons()
    {
        // Arrange & Act
        var reasons = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            var device = MockDeviceBuilder.Create().WithName($"Device{i}").Build();
            var systemInfo = _deviceInfoService.GetSystemInfo(device);
            reasons.Add(systemInfo.RebootReason);
        }

        // Assert
        reasons.Should().NotBeEmpty();
        reasons.Should().ContainMatch("*power*"); // Should have power-related reasons
        reasons.Should().HaveCountGreaterThan(1); // Should have variety in reasons
    }

    #endregion
}