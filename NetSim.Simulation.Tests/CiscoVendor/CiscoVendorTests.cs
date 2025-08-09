using NetSim.Simulation.CliHandlers.Cisco;
using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Interfaces;
using Xunit;

namespace NetSim.Simulation.Tests.CiscoVendor
{
    public class CiscoVendorContextTests
    {
        private readonly CiscoDevice _testDevice;
        private readonly CiscoVendorContext _ciscoContext;

        public CiscoVendorContextTests()
        {
            _testDevice = new CiscoDevice("CiscoRouter");
            _ciscoContext = new CiscoVendorContext(_testDevice);
        }

        [Fact]
        public void CiscoVendorContext_WhenCreated_ShouldHaveCorrectVendorName()
        {
            // Arrange & Act
            var context = new CiscoVendorContext(_testDevice);

            // Assert
            Assert.Equal("Cisco", context.VendorName);
        }

        [Fact]
        public void CiscoVendorContext_WhenCreated_ShouldHaveCiscoCapabilities()
        {
            // Arrange & Act
            var context = new CiscoVendorContext(_testDevice);

            // Assert
            Assert.NotNull(context.Capabilities);
            Assert.IsType<CiscoVendorCapabilities>(context.Capabilities);
        }

        [Fact]
        public void CiscoVendorContext_CandidateConfig_ShouldBeNull()
        {
            // Arrange & Act
            var context = new CiscoVendorContext(_testDevice);

            // Assert
            Assert.Null(context.CandidateConfig); // Cisco doesn't use candidate config
        }

        [Fact]
        public void CiscoVendorContext_IsInMode_ShouldReturnCorrectValue()
        {
            // Arrange
            _testDevice.SetMode("config");

            // Act & Assert
            Assert.True(_ciscoContext.IsInMode("config"));
            Assert.False(_ciscoContext.IsInMode("privileged"));
        }

        [Fact]
        public void CiscoVendorContext_Constructor_WithNullDevice_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CiscoVendorContext(null));
        }

        [Fact]
        public void CiscoVendorContext_GetModePrompt_ShouldReturnCiscoStylePrompt()
        {
            // Arrange
            _testDevice.SetMode("privileged");

            // Act
            var prompt = _ciscoContext.GetModePrompt();

            // Assert
            Assert.NotNull(prompt);
            Assert.Contains("#", prompt); // Cisco privileged mode prompt
        }

        [Fact]
        public void CiscoVendorContext_GetCommandHelp_ShouldReturnCiscoHelp()
        {
            // Act
            var help = _ciscoContext.GetCommandHelp("show");

            // Assert
            Assert.NotNull(help);
            Assert.Contains("show", help);
        }

        [Fact]
        public void CiscoVendorContext_PreprocessCommand_ShouldHandleCiscoAbbreviations()
        {
            // Act
            var processed = _ciscoContext.PreprocessCommand("sh ver");

            // Assert
            Assert.NotNull(processed);
            // Should expand abbreviations or maintain original format
        }
    }

    public class CiscoVendorCapabilitiesTests
    {
        private readonly CiscoDevice _testDevice;
        private readonly CiscoVendorCapabilities _ciscoCapabilities;

        public CiscoVendorCapabilitiesTests()
        {
            _testDevice = new CiscoDevice("CiscoRouter");
            _ciscoCapabilities = new CiscoVendorCapabilities(_testDevice);
        }

        [Fact]
        public void CiscoVendorCapabilities_Constructor_WithNullDevice_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CiscoVendorCapabilities(null));
        }

        [Fact]
        public void CiscoVendorCapabilities_GetRunningConfiguration_ShouldReturnConfiguration()
        {
            // Act
            var config = _ciscoCapabilities.GetRunningConfiguration();

            // Assert
            Assert.NotNull(config);
            Assert.IsType<string>(config);
        }

        [Fact]
        public void CiscoVendorCapabilities_GetStartupConfiguration_ShouldReturnConfiguration()
        {
            // Act
            var config = _ciscoCapabilities.GetStartupConfiguration();

            // Assert
            Assert.NotNull(config);
            Assert.IsType<string>(config);
        }

        [Fact]
        public void CiscoVendorCapabilities_SetDeviceMode_ShouldUpdateDeviceMode()
        {
            // Act
            _ciscoCapabilities.SetDeviceMode("config");

            // Assert
            Assert.Equal("config", _testDevice.GetCurrentMode());
        }

        [Fact]
        public void CiscoVendorCapabilities_GetCurrentMode_ShouldReturnDeviceMode()
        {
            // Arrange
            _testDevice.SetMode("privileged");

            // Act
            var mode = _ciscoCapabilities.GetCurrentMode();

            // Assert
            Assert.Equal("privileged", mode);
        }

        [Fact]
        public void CiscoVendorCapabilities_GetModeContext_ShouldReturnCiscoContext()
        {
            // Arrange
            _testDevice.SetMode("interface");
            _testDevice.SetCurrentInterface("GigabitEthernet0/0");

            // Act
            var context = _ciscoCapabilities.GetModeContext();

            // Assert
            Assert.NotNull(context);
            Assert.Equal("interface", context["mode"]);
            Assert.Equal("GigabitEthernet0/0", context["interface"]);
        }

        [Fact]
        public void CiscoVendorCapabilities_IsInMode_ShouldReturnCorrectValue()
        {
            // Arrange
            _testDevice.SetMode("config");

            // Act & Assert
            Assert.True(_ciscoCapabilities.IsInMode("config"));
            Assert.False(_ciscoCapabilities.IsInMode("user"));
        }

        [Fact]
        public void CiscoVendorCapabilities_ValidateCommand_ShouldValidateCiscoCommands()
        {
            // Act & Assert
            Assert.True(_ciscoCapabilities.ValidateCommand("show version", "user"));
            Assert.True(_ciscoCapabilities.ValidateCommand("configure terminal", "privileged"));
            Assert.True(_ciscoCapabilities.ValidateCommand("interface GigabitEthernet0/0", "config"));
            Assert.False(_ciscoCapabilities.ValidateCommand("invalid_command", "user"));
        }

        [Fact]
        public void CiscoVendorCapabilities_GetVendorErrorMessages_ShouldReturnCiscoErrorMessages()
        {
            // Act
            var errors = _ciscoCapabilities.GetVendorErrorMessages();

            // Assert
            Assert.NotNull(errors);
            Assert.Contains("incomplete_command", errors.Keys);
            Assert.Contains("invalid_command", errors.Keys);
            Assert.Equal("% Incomplete command.", errors["incomplete_command"]);
            Assert.Equal("% Invalid input detected at '^' marker.", errors["invalid_command"]);
        }

        [Fact]
        public void CiscoVendorCapabilities_FormatInterfaceName_ShouldExpandCiscoAbbreviations()
        {
            // Act & Assert
            Assert.Equal("GigabitEthernet0/0", _ciscoCapabilities.FormatInterfaceName("gi0/0"));
            Assert.Equal("FastEthernet0/1", _ciscoCapabilities.FormatInterfaceName("fa0/1"));
            Assert.Equal("TenGigabitEthernet0/0", _ciscoCapabilities.FormatInterfaceName("te0/0"));
            Assert.Equal("Loopback0", _ciscoCapabilities.FormatInterfaceName("lo0"));
        }

        [Fact]
        public void CiscoVendorCapabilities_GetCommandHistory_ShouldReturnHistoryFromDevice()
        {
            // Arrange
            _testDevice.ProcessCommand("show version");
            _testDevice.ProcessCommand("show interfaces");

            // Act
            var history = _ciscoCapabilities.GetCommandHistory();

            // Assert
            Assert.NotNull(history);
            Assert.IsType<List<string>>(history);
            Assert.Contains("show version", history);
            Assert.Contains("show interfaces", history);
        }

        [Theory]
        [InlineData("show", "user", true)]
        [InlineData("enable", "user", true)]
        [InlineData("configure", "privileged", true)]
        [InlineData("interface", "config", true)]
        [InlineData("ip", "interface", true)]
        [InlineData("configure", "user", false)]
        [InlineData("interface", "user", false)]
        public void CiscoVendorCapabilities_ValidateCommand_ShouldValidateByMode(string command, string mode, bool expected)
        {
            // Act
            var result = _ciscoCapabilities.ValidateCommand(command, mode);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CiscoVendorCapabilities_SetModeContext_ShouldUpdateDeviceContext()
        {
            // Act
            _ciscoCapabilities.SetModeContext("interface", "FastEthernet0/0");
            _ciscoCapabilities.SetModeContext("hostname", "CiscoTest");

            // Assert
            Assert.Equal("FastEthernet0/0", _testDevice.GetCurrentInterface());
            Assert.Equal("CiscoTest", _testDevice.GetHostname());
        }
    }

    public class CiscoHandlerRegistryTests
    {
        private readonly CiscoHandlerRegistry _registry;
        private readonly CiscoDevice _testDevice;

        public CiscoHandlerRegistryTests()
        {
            _registry = new CiscoHandlerRegistry();
            _testDevice = new CiscoDevice("CiscoRouter");
        }

        [Fact]
        public void CiscoHandlerRegistry_VendorName_ShouldBeCisco()
        {
            // Act
            var vendorName = _registry.VendorName;

            // Assert
            Assert.Equal("Cisco", vendorName);
        }

        [Fact]
        public void CiscoHandlerRegistry_Priority_ShouldBeHighPriority()
        {
            // Act
            var priority = _registry.Priority;

            // Assert
            Assert.Equal(200, priority);
        }

        [Fact]
        public void CiscoHandlerRegistry_CanHandle_ShouldRecognizeCiscoVariants()
        {
            // Act & Assert
            Assert.True(_registry.CanHandle("cisco"));
            Assert.True(_registry.CanHandle("Cisco"));
            Assert.True(_registry.CanHandle("CISCO"));
            Assert.True(_registry.CanHandle("cisco systems"));
            Assert.True(_registry.CanHandle("ios"));
            Assert.True(_registry.CanHandle("ios-xe"));
            Assert.True(_registry.CanHandle("nx-os"));
            Assert.False(_registry.CanHandle("juniper"));
            Assert.False(_registry.CanHandle("arista"));
        }

        [Fact]
        public void CiscoHandlerRegistry_GetSupportedDeviceTypes_ShouldReturnCiscoDeviceTypes()
        {
            // Act
            var deviceTypes = _registry.GetSupportedDeviceTypes();

            // Assert
            Assert.NotNull(deviceTypes);
            Assert.Contains("router", deviceTypes);
            Assert.Contains("switch", deviceTypes);
            Assert.Contains("catalyst", deviceTypes);
            Assert.Contains("nexus", deviceTypes);
            Assert.Contains("asr", deviceTypes);
            Assert.Contains("isr", deviceTypes);
        }

        [Fact]
        public void CiscoHandlerRegistry_CreateVendorContext_WithCiscoDevice_ShouldReturnCiscoContext()
        {
            // Act
            var context = _registry.CreateVendorContext(_testDevice);

            // Assert
            Assert.NotNull(context);
            Assert.IsType<CiscoVendorContext>(context);
            Assert.Equal("Cisco", context.VendorName);
        }

        [Fact]
        public void CiscoHandlerRegistry_CreateVendorContext_WithNullDevice_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _registry.CreateVendorContext(null));
        }

        [Fact]
        public void CiscoHandlerRegistry_Initialize_ShouldNotThrow()
        {
            // Act & Assert - Should not throw
            _registry.Initialize();
        }

        [Fact]
        public void CiscoHandlerRegistry_Cleanup_ShouldNotThrow()
        {
            // Act & Assert - Should not throw
            _registry.Cleanup();
        }

        [Fact]
        public void CiscoHandlerRegistry_ToString_ShouldReturnDescriptiveString()
        {
            // Act
            var description = _registry.ToString();

            // Assert
            Assert.NotNull(description);
            Assert.Contains("Cisco", description);
        }
    }

    public class CiscoVendorIntegrationTests
    {
        private readonly CiscoDevice _testDevice;
        private readonly CiscoHandlerRegistry _registry;

        public CiscoVendorIntegrationTests()
        {
            _testDevice = new CiscoDevice("CiscoRouter");
            _registry = new CiscoHandlerRegistry();
        }

        [Fact]
        public void CiscoVendor_EndToEndTest_ShouldWorkCorrectly()
        {
            // Arrange
            _registry.Initialize();
            var context = _registry.CreateVendorContext(_testDevice);

            // Act - Test vendor context functionality
            var vendorName = context.VendorName;
            var capabilities = context.Capabilities;
            var isInUserMode = context.IsInMode("user");

            // Assert
            Assert.Equal("Cisco", vendorName);
            Assert.NotNull(capabilities);
            Assert.True(isInUserMode); // Device starts in user mode
        }

        [Fact]
        public void CiscoVendor_ModeTransitions_ShouldWorkCorrectly()
        {
            // Arrange
            var context = _registry.CreateVendorContext(_testDevice);
            var capabilities = context.Capabilities;

            // Act - Test mode transitions
            capabilities.SetDeviceMode("privileged");
            var isInPrivileged = context.IsInMode("privileged");

            capabilities.SetDeviceMode("config");
            var isInConfig = context.IsInMode("config");

            // Assert
            Assert.True(isInPrivileged);
            Assert.True(isInConfig);
        }

        [Fact]
        public void CiscoVendor_ConfigurationAccess_ShouldWorkCorrectly()
        {
            // Arrange
            var context = _registry.CreateVendorContext(_testDevice);
            var capabilities = context.Capabilities;

            // Act
            var runningConfig = capabilities.GetRunningConfiguration();
            var startupConfig = capabilities.GetStartupConfiguration();

            // Assert
            Assert.NotNull(runningConfig);
            Assert.NotNull(startupConfig);
        }

        [Fact]
        public void CiscoVendor_ErrorHandling_ShouldProvideCorrectMessages()
        {
            // Arrange
            var context = _registry.CreateVendorContext(_testDevice);
            var capabilities = (CiscoVendorCapabilities)context.Capabilities;

            // Act
            var errorMessages = capabilities.GetVendorErrorMessages();

            // Assert
            Assert.NotNull(errorMessages);
            Assert.All(errorMessages.Values, msg => Assert.StartsWith("%", msg)); // Cisco errors start with %
        }
    }
} 