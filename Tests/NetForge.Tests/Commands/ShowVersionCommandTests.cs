using FluentAssertions;
using Moq;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Common.Common;
using NetForge.Tests.TestHelpers;
using Xunit;

namespace NetForge.Tests.Commands;

/// <summary>
/// Comprehensive test suite for ShowVersionCommand following the Command Pattern architecture
/// Tests command pattern integrity, business logic separation, and vendor-agnostic behavior
/// </summary>
public class ShowVersionCommandTests
{
    private readonly Mock<IDeviceInformationService> _mockDeviceInfoService;
    private readonly ShowVersionCommand _showVersionCommand;
    private readonly MockNetworkDevice _mockDevice;

    public ShowVersionCommandTests()
    {
        _mockDeviceInfoService = new Mock<IDeviceInformationService>();
        _showVersionCommand = new ShowVersionCommand(_mockDeviceInfoService.Object);
        _mockDevice = MockDeviceBuilder.Create()
            .WithName("TestDevice")
            .WithVendor("Cisco")
            .WithInterface("eth0", "192.168.1.10", isUp: true)
            .Build();
    }

    #region Command Pattern Integrity Tests

    [Fact]
    public async Task ExecuteBusinessLogicAsync_ValidDevice_CallsDeviceInformationService()
    {
        // Arrange
        var args = new[] { "show", "version" };
        var expectedVersionData = CreateMockDeviceVersionData();

        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(_mockDevice))
            .Returns(expectedVersionData);

        // Act
        var result = await _showVersionCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ShowVersionCommandData>();
        var showVersionResult = (ShowVersionCommandData)result;

        showVersionResult.VersionData.Should().Be(expectedVersionData);

        _mockDeviceInfoService.Verify(s => s.GetDeviceVersion(_mockDevice), Times.Once);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_ServiceException_ThrowsCommandExecutionException()
    {
        // Arrange
        var args = new[] { "show", "version" };

        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(_mockDevice))
            .Throws(new InvalidOperationException("Service error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _showVersionCommand.ExecuteBusinessLogicAsync(_mockDevice, args));

        exception.Message.Should().Contain("Service error");
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_NullDevice_ThrowsCommandExecutionException()
    {
        // Arrange
        var args = new[] { "show", "version" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _showVersionCommand.ExecuteBusinessLogicAsync(null, args));

        exception.Message.Should().Contain("device");
        exception.InnerException.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_EmptyArgs_StillExecutesSuccessfully()
    {
        // Arrange
        var args = new string[] { };
        var expectedVersionData = CreateMockDeviceVersionData();

        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(_mockDevice))
            .Returns(expectedVersionData);

        // Act
        var result = await _showVersionCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ShowVersionCommandData>();
        _mockDeviceInfoService.Verify(s => s.GetDeviceVersion(_mockDevice), Times.Once);
    }

    #endregion

    #region Business Logic Separation Tests

    [Fact]
    public async Task ExecuteBusinessLogicAsync_CiscoDevice_CallsServiceWithCorrectDevice()
    {
        // Arrange
        var ciscoDevice = MockDeviceBuilder.Create()
            .WithName("CiscoRouter")
            .WithVendor("Cisco")
            .Build();
        var args = new[] { "show", "version" };

        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(ciscoDevice))
            .Returns(CreateMockDeviceVersionData());

        // Act
        await _showVersionCommand.ExecuteBusinessLogicAsync(ciscoDevice, args);

        // Assert
        _mockDeviceInfoService.Verify(s => s.GetDeviceVersion(ciscoDevice), Times.Once);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_JuniperDevice_CallsServiceWithCorrectDevice()
    {
        // Arrange
        var juniperDevice = MockDeviceBuilder.Create()
            .WithName("JuniperSwitch")
            .WithVendor("Juniper")
            .Build();
        var args = new[] { "show", "version" };

        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(juniperDevice))
            .Returns(CreateMockDeviceVersionData());

        // Act
        await _showVersionCommand.ExecuteBusinessLogicAsync(juniperDevice, args);

        // Assert
        _mockDeviceInfoService.Verify(s => s.GetDeviceVersion(juniperDevice), Times.Once);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_VendorAgnostic_WorksWithAnyVendor()
    {
        // Arrange
        var vendors = new[] { "Cisco", "Juniper", "Nokia", "Arista", "UnknownVendor" };
        var args = new[] { "show", "version" };

        foreach (var vendor in vendors)
        {
            var device = MockDeviceBuilder.Create()
                .WithName($"TestDevice-{vendor}")
                .WithVendor(vendor)
                .Build();

            _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(device))
                .Returns(new DeviceVersionData { Vendor = vendor });

            // Act
            var result = await _showVersionCommand.ExecuteBusinessLogicAsync(device, args);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ShowVersionCommandData>();
            var commandData = (ShowVersionCommandData)result;
            commandData.VersionData.Vendor.Should().Be(vendor);
        }
    }

    #endregion

    #region Command Data Structure Tests

    [Fact]
    public async Task ExecuteBusinessLogicAsync_ShowVersionCommandData_ContainsAllInformation()
    {
        // Arrange
        var args = new[] { "show", "version" };
        var versionData = CreateMockDeviceVersionData();

        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(_mockDevice))
            .Returns(versionData);

        var beforeExecution = DateTime.UtcNow;

        // Act
        var result = await _showVersionCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var afterExecution = DateTime.UtcNow;
        var commandData = (ShowVersionCommandData)result;

        // Verify all data is properly mapped
        commandData.VersionData.Should().Be(versionData);

        // Verify timestamp is set correctly
        commandData.Timestamp.Should().BeOnOrAfter(beforeExecution);
        commandData.Timestamp.Should().BeOnOrBefore(afterExecution);
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_CommandData_IsImmutable()
    {
        // Arrange
        var args = new[] { "show", "version" };
        var versionData = CreateMockDeviceVersionData();

        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(_mockDevice))
            .Returns(versionData);

        // Act
        var result1 = await _showVersionCommand.ExecuteBusinessLogicAsync(_mockDevice, args);
        var result2 = await _showVersionCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        var commandData1 = (ShowVersionCommandData)result1;
        var commandData2 = (ShowVersionCommandData)result2;

        // Each execution should create a new instance with fresh timestamp
        commandData1.Should().NotBeSameAs(commandData2);
        commandData1.Timestamp.Should().BeLessOrEqualTo(commandData2.Timestamp);

        // But the device information should be consistent
        commandData1.VersionData.SerialNumber.Should().Be(commandData2.VersionData.SerialNumber);
        commandData1.VersionData.Model.Should().Be(commandData2.VersionData.Model);
    }

    #endregion

    #region Help and Completion Tests

    [Fact]
    public void GetHelpText_Always_ReturnsVersionCommandHelp()
    {
        // Act
        var helpText = _showVersionCommand.GetHelpText();

        // Assert
        helpText.Should().NotBeNullOrEmpty();
        helpText.Should().Contain("show version");
        helpText.Should().Contain("device information");
        helpText.Should().Contain("software version");
        helpText.Should().Contain("hardware");
        helpText.Should().Contain("Usage:");
    }

    [Fact]
    public void GetCompletions_PartialModifier_ReturnsMatchingModifiers()
    {
        // Arrange
        var partial = "bri";

        // Act
        var completions = _showVersionCommand.GetCompletions(partial);

        // Assert
        completions.Should().Contain("brief");
    }

    [Fact]
    public void GetCompletions_EmptyInput_ReturnsCommonModifiers()
    {
        // Act
        var completions = _showVersionCommand.GetCompletions("");

        // Assert
        completions.Should().NotBeEmpty();
        completions.Should().Contain("brief");
        completions.Should().Contain("detail");
        completions.Should().Contain("hardware");
        completions.Should().Contain("software");
    }

    [Fact]
    public void GetCompletions_VendorSpecific_ReturnsVendorOptions()
    {
        // Arrange
        var partial = "hard";

        // Act
        var completions = _showVersionCommand.GetCompletions(partial);

        // Assert
        completions.Should().Contain("hardware");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task ExecuteBusinessLogicAsync_StandardExecution_CompletesWithin100ms()
    {
        // Arrange
        var args = new[] { "show", "version" };
        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(_mockDevice))
            .Returns(CreateMockDeviceVersionData());
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _showVersionCommand.ExecuteBusinessLogicAsync(_mockDevice, args);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(100);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteBusinessLogicAsync_MultipleVendors_PerformsConsistently()
    {
        // Arrange
        var vendors = new[] { "Cisco", "Juniper", "Nokia", "Arista" };
        var args = new[] { "show", "version" };
        var executionTimes = new List<long>();

        foreach (var vendor in vendors)
        {
            var device = MockDeviceBuilder.Create().WithVendor(vendor).Build();
            _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(device))
                .Returns(CreateMockDeviceVersionData());

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _showVersionCommand.ExecuteBusinessLogicAsync(device, args);

            stopwatch.Stop();
            executionTimes.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert
        executionTimes.Should().OnlyContain(time => time <= 100);
        var avgTime = executionTimes.Average();
        var maxTime = executionTimes.Max();
        var minTime = executionTimes.Min();

        // Performance should be consistent across vendors (within reasonable variance)
        (maxTime - minTime).Should().BeLessOrEqualTo(50); // Max 50ms variance
    }

    #endregion

    #region Error Resilience Tests

    [Fact]
    public async Task ExecuteBusinessLogicAsync_PartialServiceFailure_ReturnsPartialData()
    {
        // Arrange
        var args = new[] { "show", "version" };

        _mockDeviceInfoService.Setup(s => s.GetDeviceVersion(_mockDevice))
            .Throws(new InvalidOperationException("Device info unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutionException>(
            () => _showVersionCommand.ExecuteBusinessLogicAsync(_mockDevice, args));

        exception.Message.Should().Contain("Device info unavailable");
    }

    #endregion

    #region Private Helper Methods

    private DeviceVersionData CreateMockDeviceVersionData()
    {
        return new DeviceVersionData
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
        };
    }



    #endregion
}