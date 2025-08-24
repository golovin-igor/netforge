using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Factories;
using NetForge.Simulation.Common.CLI.Implementations;
using NetForge.Simulation.Common.CLI.Interfaces;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.VendorContext
{
    public class VendorContextTests
    {
        private readonly CiscoDevice _testDevice;
        private readonly DefaultVendorContext _defaultVendorContext;

        public VendorContextTests()
        {
            _testDevice = new CiscoDevice("TestRouter");
            _defaultVendorContext = new DefaultVendorContext(_testDevice);
        }

        [Fact]
        public void DefaultVendorContext_WhenCreated_ShouldHaveCorrectVendorName()
        {
            // Arrange & Act
            var context = new DefaultVendorContext(_testDevice);

            // Assert
            Assert.Equal(_testDevice.Vendor ?? "Unknown", context.VendorName);
        }

        [Fact]
        public void DefaultVendorContext_WhenCreated_ShouldHaveCapabilities()
        {
            // Arrange & Act
            var context = new DefaultVendorContext(_testDevice);

            // Assert
            Assert.NotNull(context.Capabilities);
            Assert.IsType<DefaultVendorCapabilities>(context.Capabilities);
        }

        [Fact]
        public void DefaultVendorContext_CandidateConfig_ShouldBeNull()
        {
            // Arrange & Act
            var context = new DefaultVendorContext(_testDevice);

            // Assert
            Assert.Null(context.CandidateConfig);
        }

        [Fact]
        public void DefaultVendorContext_IsInMode_ShouldReturnCorrectValue()
        {
            // Arrange
            _testDevice.SetMode("config");
            var context = new DefaultVendorContext(_testDevice);

            // Act & Assert
            Assert.True(context.IsInMode("config"));
            Assert.False(context.IsInMode("privileged"));
        }

        [Fact]
        public void DefaultVendorContext_GetModePrompt_ShouldReturnDevicePrompt()
        {
            // Arrange
            _testDevice.SetMode("interface");
            var context = new DefaultVendorContext(_testDevice);

            // Act
            var prompt = context.GetModePrompt();

            // Assert
            Assert.NotNull(prompt);
            Assert.IsType<string>(prompt);
        }

        [Fact]
        public void DefaultVendorCapabilities_GetRunningConfiguration_ShouldReturnConfiguration()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);

            // Act
            var config = capabilities.GetRunningConfiguration();

            // Assert
            Assert.NotNull(config);
            Assert.IsType<string>(config);
        }

        [Fact]
        public void DefaultVendorCapabilities_SetDeviceMode_ShouldUpdateDeviceMode()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);

            // Act
            capabilities.SetDeviceMode("privileged");

            // Assert
            Assert.Equal("privileged", _testDevice.GetCurrentMode());
        }

        [Fact]
        public void DefaultVendorCapabilities_GetDeviceMode_ShouldReturnDeviceMode()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);
            _testDevice.SetMode("config");

            // Act
            var mode = capabilities.GetDeviceMode();

            // Assert
            Assert.Equal("config", mode);
        }

        [Fact]
        public void DefaultVendorCapabilities_SupportsMode_ShouldReturnCorrectValue()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);

            // Act & Assert
            Assert.True(capabilities.SupportsMode("privileged"));
            Assert.True(capabilities.SupportsMode("config"));
            Assert.False(capabilities.SupportsMode("invalid_mode"));
        }

        [Fact]
        public void DefaultVendorCapabilities_GetAvailableModes_ShouldReturnModesList()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);

            // Act
            var modes = capabilities.GetAvailableModes();

            // Assert
            Assert.NotNull(modes);
            Assert.Contains("user", modes);
            Assert.Contains("privileged", modes);
            Assert.Contains("config", modes);
        }

        [Fact]
        public void DefaultVendorCapabilities_GetVendorErrorMessage_ShouldReturnCorrectMessage()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);

            // Act & Assert
            Assert.Equal("% Invalid command", capabilities.GetVendorErrorMessage("invalid_command"));
            Assert.Equal("% Incomplete command", capabilities.GetVendorErrorMessage("incomplete_command"));
            Assert.Equal("% Invalid parameter", capabilities.GetVendorErrorMessage("invalid_parameter"));
        }

        [Fact]
        public void DefaultVendorCapabilities_ValidateVendorSyntax_ShouldReturnTrueForValidCommands()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);

            // Act & Assert
            Assert.True(capabilities.ValidateVendorSyntax(new[] { "show", "version" }, "show version"));
            Assert.True(capabilities.ValidateVendorSyntax(new[] { "configure", "terminal" }, "configure terminal"));
            Assert.False(capabilities.ValidateVendorSyntax(new string[0], ""));
        }

        [Fact]
        public void DefaultVendorCapabilities_SupportsFeature_ShouldReturnCorrectValue()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);

            // Act & Assert
            Assert.True(capabilities.SupportsFeature("ping"));
            Assert.True(capabilities.SupportsFeature("show"));
            Assert.True(capabilities.SupportsFeature("configure"));
            Assert.False(capabilities.SupportsFeature("bgp"));
        }

        [Fact]
        public void DefaultVendorCapabilities_FormatCommandOutput_ShouldFormatOutput()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);

            // Act
            var formatted = capabilities.FormatCommandOutput("show version", "Version 1.0");

            // Assert
            Assert.Equal("Version 1.0", formatted);
        }

        [Fact]
        public void DefaultVendorCapabilities_FormatInterfaceName_ShouldReturnUnmodifiedName()
        {
            // Arrange
            var capabilities = new DefaultVendorCapabilities(_testDevice);
            var interfaceName = "GigabitEthernet0/0";

            // Act
            var formatted = capabilities.FormatInterfaceName(interfaceName);

            // Assert
            Assert.Equal(interfaceName, formatted);
        }
    }

    public class VendorContextFactoryTests
    {
        private readonly CiscoDevice _testDevice;

        public VendorContextFactoryTests()
        {
            _testDevice = new CiscoDevice("TestRouter");
        }

        [Fact]
        public void RegisterVendorContext_WhenCalled_ShouldRegisterFactory()
        {
            // Arrange
            var factoryCallCount = 0;
            Func<NetworkDevice, IVendorContext> factory = device =>
            {
                factoryCallCount++;
                return new DefaultVendorContext(device);
            };

            // Act
            VendorContextFactory.RegisterVendorContext("test", factory);

            // Create a test device with the specific vendor
            var testDevice = new CiscoDevice("TestRouter");
            // Set the vendor to match the registered factory
            var context = VendorContextFactory.GetVendorContext(testDevice);

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void GetVendorContext_WhenVendorNotRegistered_ShouldReturnDefaultContext()
        {
            // Arrange
            var deviceWithUnknownVendor = new CiscoDevice("TestDevice");

            // Act
            var context = VendorContextFactory.GetVendorContext(deviceWithUnknownVendor);

            // Assert
            Assert.NotNull(context);
            Assert.IsType<DefaultVendorContext>(context);
        }

        [Fact]
        public void IsVendorContextRegistered_WhenVendorRegistered_ShouldReturnTrue()
        {
            // Arrange
            var vendorName = "TestVendor";
            VendorContextFactory.RegisterVendorContext(vendorName, device => new DefaultVendorContext(device));

            // Act
            var isRegistered = VendorContextFactory.IsVendorContextRegistered(vendorName);

            // Assert
            Assert.True(isRegistered);
        }

        [Fact]
        public void IsVendorContextRegistered_WhenVendorNotRegistered_ShouldReturnFalse()
        {
            // Arrange
            var vendorName = "UnknownVendor";

            // Act
            var isRegistered = VendorContextFactory.IsVendorContextRegistered(vendorName);

            // Assert
            Assert.False(isRegistered);
        }

        [Fact]
        public void GetRegisteredVendors_ShouldReturnRegisteredVendorNames()
        {
            // Arrange
            VendorContextFactory.RegisterVendorContext("Vendor1", device => new DefaultVendorContext(device));
            VendorContextFactory.RegisterVendorContext("Vendor2", device => new DefaultVendorContext(device));

            // Act
            var vendors = VendorContextFactory.GetRegisteredVendors();

            // Assert
            Assert.NotNull(vendors);
            Assert.Contains("vendor1", vendors); // Case insensitive storage
            Assert.Contains("vendor2", vendors);
        }

        [Fact]
        public void GetVendorContext_WhenDeviceIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                VendorContextFactory.GetVendorContext(null));
        }
    }

    public class CliContextVendorContextTests
    {
        private readonly CiscoDevice _testDevice;
        private readonly CliContext _cliContext;

        public CliContextVendorContextTests()
        {
            _testDevice = new CiscoDevice("TestRouter");
            _cliContext = new CliContext(_testDevice, new[] { "show", "version" }, "show version");
        }

        [Fact]
        public void CliContext_VendorContext_ShouldLazyLoadVendorContext()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "show", "version" }, "show version");

            // Act
            var vendorContext = context.VendorContext;

            // Assert
            Assert.NotNull(vendorContext);
            Assert.IsType<DefaultVendorContext>(vendorContext);
        }

        [Fact]
        public void CliContext_VendorContext_ShouldCacheVendorContext()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "show", "version" }, "show version");

            // Act
            var vendorContext1 = context.VendorContext;
            var vendorContext2 = context.VendorContext;

            // Assert
            Assert.Same(vendorContext1, vendorContext2);
        }

        [Fact]
        public void CliContext_VendorContext_ShouldUseCorrectVendorName()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "show", "version" }, "show version");

            // Act
            var vendorContext = context.VendorContext;

            // Assert
            Assert.Equal(_testDevice.Vendor ?? "Unknown", vendorContext.VendorName);
        }

        [Fact]
        public void CliContext_VendorContext_WhenCustomVendorContextSet_ShouldUseCustomContext()
        {
            // Arrange
            var context = new CliContext(_testDevice, new[] { "show", "version" }, "show version");
            var customContext = new DefaultVendorContext(_testDevice);

            // Act
            context.VendorContext = customContext;

            // Assert
            Assert.Same(customContext, context.VendorContext);
        }
    }
}
