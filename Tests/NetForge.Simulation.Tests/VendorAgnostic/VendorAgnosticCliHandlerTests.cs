using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Implementations;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.VendorAgnostic
{
    public class VendorAgnosticCliHandlerTests
    {
        private readonly CiscoDevice _testDevice = new CiscoDevice("TestRouter");
        private readonly TestVendorAgnosticHandler _testHandler = new();

        [Fact]
        public void VendorAgnosticCliHandler_GetVendorContext_ShouldReturnCorrectContext()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            var vendorContext = _testHandler.GetVendorContextPublic(context);

            // Assert
            Assert.NotNull(vendorContext);
        }

        [Fact]
        public void VendorAgnosticCliHandler_IsVendor_ShouldReturnCorrectValue()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act & Assert
            Assert.True(_testHandler.IsVendorPublic(context, "Cisco"));
            Assert.False(_testHandler.IsVendorPublic(context, "Juniper"));
        }

        [Fact]
        public void VendorAgnosticCliHandler_RequireVendor_ShouldReturnErrorForWrongVendor()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            var result = _testHandler.RequireVendorPublic(context, "Juniper");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("not supported", result.Output);
        }

        [Fact]
        public void VendorAgnosticCliHandler_GetRunningConfig_ShouldReturnConfiguration()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            var config = _testHandler.GetRunningConfigPublic(context);

            // Assert
            Assert.NotNull(config);
        }

        [Fact]
        public void VendorAgnosticCliHandler_SetMode_ShouldUpdateDeviceMode()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            _testHandler.SetModePublic(context, "config");

            // Assert
            Assert.Equal("config", _testDevice.GetCurrentMode());
        }

        [Fact]
        public void VendorAgnosticCliHandler_IsInMode_ShouldReturnCorrectValue()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");
            _testDevice.SetMode("config");

            // Act & Assert
            Assert.True(_testHandler.IsInModePublic(context, "config"));
            Assert.False(_testHandler.IsInModePublic(context, "user"));
        }

        [Fact]
        public void VendorAgnosticCliHandler_GetVendorError_ShouldReturnVendorSpecificError()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            var error = _testHandler.GetVendorErrorPublic(context, "invalid_command");

            // Assert
            Assert.NotNull(error);
            Assert.Contains("%", error); // Should contain vendor-specific error formatting
        }

        [Fact]
        public void VendorAgnosticCliHandler_FormatInterfaceName_ShouldFormatCorrectly()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            var formatted = _testHandler.FormatInterfaceNamePublic(context, "gi0/0");

            // Assert
            Assert.NotNull(formatted);
        }

        [Fact]
        public void VendorAgnosticCliHandler_ValidateVendorSyntax_ShouldValidateCorrectly()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "show", "version" }, "show version");

            // Act
            var isValid = _testHandler.ValidateVendorSyntaxPublic(context, "show version");

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void VendorAgnosticCliHandler_SupportsFeature_ShouldValidateFeatures()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act & Assert
            Assert.True(_testHandler.SupportsFeaturePublic(context, "ping"));
            Assert.True(_testHandler.SupportsFeaturePublic(context, "show"));
            Assert.False(_testHandler.SupportsFeaturePublic(context, "bgp"));
        }

        [Fact]
        public void VendorAgnosticCliHandler_GetStartupConfig_ShouldReturnStartupConfiguration()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            var config = _testHandler.GetStartupConfigPublic(context);

            // Assert
            Assert.NotNull(config);
        }

        [Fact]
        public void VendorAgnosticCliHandler_FormatOutput_ShouldFormatCorrectly()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            var formatted = _testHandler.FormatOutputPublic(context, "show version", "Version 1.0");

            // Assert
            Assert.NotNull(formatted);
        }

        [Fact]
        public void VendorAgnosticCliHandler_ErrorResult_ShouldCreateErrorWithVendorFormatting()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "test" }, "test");

            // Act
            var result = _testHandler.CreateErrorResult(context, CliErrorType.InvalidCommand, "Test error");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Test error", result.Output);
        }

        [Fact]
        public void VendorAgnosticCliHandler_SuccessResult_ShouldCreateSuccess()
        {
            // Act
            var result = _testHandler.CreateSuccessResult("Test output");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Test output", result.Output);
        }
    }

    /// <summary>
    /// Test implementation of VendorAgnosticCliHandler to expose protected methods for testing
    /// </summary>
    public class TestVendorAgnosticHandler() : VendorAgnosticCliHandler("test", "Test handler for testing vendor-agnostic functionality")
    {
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            return Success("Test command executed");
        }

        // Public methods to expose protected functionality for testing
        public IVendorContext? GetVendorContextPublic(CliContext context) => GetVendorContext(context);
        public bool IsVendorPublic(CliContext context, string vendorName) => IsVendor(context, vendorName);
        public CliResult RequireVendorPublic(CliContext context, string vendorName) => RequireVendor(context, vendorName);
        public string GetRunningConfigPublic(CliContext context) => GetRunningConfig(context);
        public void SetModePublic(CliContext context, string mode) => SetMode(context, mode);
        public bool IsInModePublic(CliContext context, string mode) => IsInMode(context, mode);
        public string GetVendorErrorPublic(CliContext context, string errorType) => GetVendorError(context, errorType);
        public string FormatInterfaceNamePublic(CliContext context, string interfaceName) => FormatInterfaceName(context, interfaceName);
        public bool ValidateVendorSyntaxPublic(CliContext context, string command) => ValidateVendorSyntax(context, command);
        public bool SupportsFeaturePublic(CliContext context, string feature) => SupportsFeature(context, feature);
        public string GetStartupConfigPublic(CliContext context) => GetStartupConfig(context);
        public string FormatOutputPublic(CliContext context, string command, object? data) => FormatOutput(context, command, data);

        public CliResult CreateErrorResult(CliContext context, CliErrorType errorType, string message)
        {
            return Error(errorType, message);
        }

        public CliResult CreateSuccessResult(string output)
        {
            return Success(output);
        }
    }

    public class VendorRegistryBaseTests
    {
        private readonly TestVendorRegistry _testRegistry = new();

        [Fact]
        public void VendorHandlerRegistryBase_VendorName_ShouldReturnCorrectName()
        {
            // Act
            var vendorName = _testRegistry.VendorName;

            // Assert
            Assert.Equal("TestVendor", vendorName);
        }

        [Fact]
        public void VendorHandlerRegistryBase_Priority_ShouldReturnCorrectPriority()
        {
            // Act
            var priority = _testRegistry.Priority;

            // Assert
            Assert.Equal(150, priority);
        }

        [Fact]
        public void VendorHandlerRegistryBase_CanHandle_ShouldBeCaseInsensitive()
        {
            // Act & Assert
            Assert.True(_testRegistry.CanHandle("testvendor"));
            Assert.True(_testRegistry.CanHandle("TestVendor"));
            Assert.True(_testRegistry.CanHandle("TESTVENDOR"));
            Assert.False(_testRegistry.CanHandle("OtherVendor"));
        }

        [Fact]
        public void VendorHandlerRegistryBase_GetSupportedDeviceTypes_ShouldReturnDefaultTypes()
        {
            // Act
            var deviceTypes = _testRegistry.GetSupportedDeviceTypes();

            // Assert
            Assert.NotNull(deviceTypes);
            Assert.Contains("router", deviceTypes);
            Assert.Contains("switch", deviceTypes);
            Assert.Contains("firewall", deviceTypes);
        }

        [Fact]
        public void VendorHandlerRegistryBase_Initialize_ShouldNotThrow()
        {
            // Act & Assert - Should not throw
            _testRegistry.Initialize();
        }

        [Fact]
        public void VendorHandlerRegistryBase_Cleanup_ShouldNotThrow()
        {
            // Act & Assert - Should not throw
            _testRegistry.Cleanup();
        }

        [Fact]
        public void VendorHandlerRegistryBase_ToString_ShouldReturnDescriptiveString()
        {
            // Act
            var description = _testRegistry.ToString();

            // Assert
            Assert.NotNull(description);
            Assert.Contains("TestVendor", description);
            Assert.Contains("v1.0.0", description);
        }
    }

    /// <summary>
    /// Test implementation of VendorHandlerRegistryBase for testing
    /// </summary>
    public class TestVendorRegistry : VendorHandlerRegistryBase
    {
        public override string VendorName => "TestVendor";
        public override int Priority => 150;

        public override void RegisterHandlers(CliHandlerManager manager)
        {
            // Test implementation - register a test handler
            manager.RegisterHandler(new TestVendorAgnosticHandler());
        }

        public override IVendorContext CreateVendorContext(INetworkDevice device)
        {
            if (device is NetworkDevice networkDevice)
            {
                return new DefaultVendorContext(networkDevice);
            }
            throw new ArgumentException("Device type not supported");
        }

        public override IEnumerable<string> GetSupportedDeviceTypes()
        {
            return new[] { "router", "switch", "firewall", "test-device" };
        }

        public override bool CanHandle(string vendorName)
        {
            return "TestVendor".Equals(vendorName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
