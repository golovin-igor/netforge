using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Factories;
using NetForge.Simulation.Common.CLI.Implementations;
using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.Integration
{
    public class VendorAgnosticArchitectureIntegrationTests
    {
        private readonly CiscoDevice _ciscoDevice;

        public VendorAgnosticArchitectureIntegrationTests()
        {
            _ciscoDevice = new CiscoDevice("CiscoRouter");
        }

        [Fact]
        public void VendorAgnosticArchitecture_FullFlow_ShouldWorkEndToEnd()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "show", "version" }, "show version");

            // Act - Test the full flow
            var vendorContext = context.VendorContext;
            var capabilities = vendorContext.Capabilities;
            var isInUserMode = vendorContext.IsInMode("user");
            var runningConfig = capabilities.GetRunningConfiguration();

            // Assert - Full architecture should work
            Assert.NotNull(vendorContext);
            Assert.NotNull(capabilities);
            Assert.True(isInUserMode);
            Assert.NotNull(runningConfig);
        }

        [Fact]
        public void VendorAgnosticArchitecture_DeviceWithoutVendorContext_ShouldUseDefaultContext()
        {
            // Arrange
            var genericDevice = new CiscoDevice("GenericDevice");
            var context = new CliContext(genericDevice, new[] { "show", "version" }, "show version");

            // Act
            var vendorContext = context.VendorContext;

            // Assert
            Assert.NotNull(vendorContext);
            Assert.IsType<DefaultVendorContext>(vendorContext);
        }

        [Fact]
        public void VendorAgnosticArchitecture_ModeTransitions_ShouldWorkCorrectly()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "configure", "terminal" }, "configure terminal");

            // Act - Test mode transitions
            var vendorContext = context.VendorContext;
            var capabilities = vendorContext.Capabilities;

            // Start in user mode
            var startMode = vendorContext.IsInMode("user");

            // Transition to privileged mode
            capabilities.SetDeviceMode("privileged");
            var privilegedMode = vendorContext.IsInMode("privileged");

            // Transition to config mode
            capabilities.SetDeviceMode("config");
            var configMode = vendorContext.IsInMode("config");

            // Assert
            Assert.True(startMode);
            Assert.True(privilegedMode);
            Assert.True(configMode);
        }

        [Fact]
        public void VendorAgnosticArchitecture_ErrorHandling_ShouldProvideVendorSpecificErrors()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "invalid", "command" }, "invalid command");

            // Act
            var vendorContext = context.VendorContext;
            var capabilities = vendorContext.Capabilities;
            var errorMessage = capabilities.GetVendorErrorMessage("invalid_command");

            // Assert
            Assert.NotNull(errorMessage);
            Assert.StartsWith("%", errorMessage); // Cisco error format
        }

        [Fact]
        public void VendorAgnosticArchitecture_InterfaceNameFormatting_ShouldUseVendorConventions()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "interface", "gi0/0" }, "interface gi0/0");

            // Act
            var vendorContext = context.VendorContext;
            var capabilities = vendorContext.Capabilities;
            var formattedName = capabilities.FormatInterfaceName("gi0/0");

            // Assert
            Assert.NotNull(formattedName);
            Assert.Equal("GigabitEthernet0/0", formattedName); // Cisco expansion
        }

        [Fact]
        public void VendorAgnosticArchitecture_CommandValidation_ShouldUseVendorRules()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "show", "version" }, "show version");

            // Act
            var vendorContext = context.VendorContext;
            var capabilities = vendorContext.Capabilities;
            var isValid = capabilities.ValidateVendorSyntax(context.CommandParts, context.FullCommand);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void VendorAgnosticArchitecture_FeatureSupport_ShouldReflectVendorCapabilities()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "ping", "1.1.1.1" }, "ping 1.1.1.1");

            // Act
            var vendorContext = context.VendorContext;
            var capabilities = vendorContext.Capabilities;
            var supportsPing = capabilities.SupportsFeature("ping");
            var supportsInvalidFeature = capabilities.SupportsFeature("invalid_feature");

            // Assert
            Assert.True(supportsPing);
            Assert.False(supportsInvalidFeature);
        }

        [Fact]
        public void VendorAgnosticArchitecture_ConfigurationAccess_ShouldBeVendorSpecific()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "show", "running-config" }, "show running-config");

            // Act
            var vendorContext = context.VendorContext;
            var capabilities = vendorContext.Capabilities;
            var runningConfig = capabilities.GetRunningConfiguration();
            var startupConfig = capabilities.GetStartupConfiguration();

            // Assert
            Assert.NotNull(runningConfig);
            Assert.NotNull(startupConfig);
        }

        [Fact]
        public void VendorAgnosticArchitecture_VendorContextFactory_ShouldWorkCorrectly()
        {
            // Arrange
            var testDevice = new CiscoDevice("TestRouter");

            // Act
            var vendorContext = VendorContextFactory.GetVendorContext(testDevice);

            // Assert
            Assert.NotNull(vendorContext);
            Assert.IsType<DefaultVendorContext>(vendorContext);
        }

        [Fact]
        public void VendorAgnosticArchitecture_ContextCaching_ShouldBeEfficient()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "show", "version" }, "show version");

            // Act - Access vendor context multiple times
            var vendorContext1 = context.VendorContext;
            var vendorContext2 = context.VendorContext;
            var vendorContext3 = context.VendorContext;

            // Assert - Should be the same instance (cached)
            Assert.Same(vendorContext1, vendorContext2);
            Assert.Same(vendorContext2, vendorContext3);
        }

        [Fact]
        public void VendorAgnosticArchitecture_HandlerWithMultipleVendors_ShouldWorkCorrectly()
        {
            // Arrange
            var ciscoDevice = new CiscoDevice("CiscoRouter");
            var testHandler = new TestMultiVendorHandler();

            // Act
            var ciscoContext = new CliContext(ciscoDevice, new[] { "test" }, "test");
            var ciscoResult = testHandler.Handle(ciscoContext);

            // Assert
            Assert.True(ciscoResult.Success);
            Assert.Contains("Cisco", ciscoResult.Output);
        }

        [Fact]
        public void VendorAgnosticArchitecture_VendorSpecificCommands_ShouldBeRestricted()
        {
            // Arrange
            var ciscoDevice = new CiscoDevice("CiscoRouter");
            var testHandler = new TestCiscoOnlyHandler();

            // Act
            var ciscoContext = new CliContext(ciscoDevice, new[] { "cisco-only" }, "cisco-only");
            var ciscoResult = testHandler.Handle(ciscoContext);

            // Assert
            Assert.True(ciscoResult.Success);
            Assert.Contains("Cisco only", ciscoResult.Output);
        }

        [Fact]
        public void VendorAgnosticArchitecture_BackwardCompatibility_ShouldBePreserved()
        {
            // Arrange
            var context = new CliContext(_ciscoDevice, new[] { "show", "version" }, "show version");

            // Act - Test that existing device methods still work
            var deviceName = context.Device.Name;
            var deviceMode = context.Device.GetCurrentMode();
            var deviceHostname = context.Device.GetHostname();

            // Assert
            Assert.Equal("CiscoRouter", deviceName);
            Assert.NotNull(deviceMode);
            Assert.NotNull(deviceHostname);
        }
    }

    /// <summary>
    /// Test handler that works with multiple vendors
    /// </summary>
    public class TestMultiVendorHandler : VendorAgnosticCliHandler
    {
        public TestMultiVendorHandler() : base("test", "Test command for multiple vendors")
        {
        }

    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            var vendorContext = GetVendorContext(context);
            var vendorName = vendorContext?.VendorName ?? "Unknown";

            return Success($"Command executed on {vendorName} device");
        }
    }

    /// <summary>
    /// Test handler that only works with Cisco devices
    /// </summary>
    public class TestCiscoOnlyHandler : VendorAgnosticCliHandler
    {
        public TestCiscoOnlyHandler() : base("cisco-only", "Command that only works on Cisco devices")
        {
        }

    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            var vendorCheck = RequireVendor(context, "Cisco");
            if (!vendorCheck.Success)
                return vendorCheck;

            return Success("Cisco only command executed successfully");
        }
    }

    /// <summary>
    /// Test handler that demonstrates error handling with vendor context
    /// </summary>
    public class TestErrorHandler : VendorAgnosticCliHandler
    {
        public TestErrorHandler() : base("error-test", "Test error handling")
        {
        }

    protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            var errorMessage = GetVendorError(context, "invalid_command");
            return Error(CliErrorType.InvalidCommand, errorMessage);
        }
    }
}
