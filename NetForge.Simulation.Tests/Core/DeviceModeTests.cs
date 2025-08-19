using NetForge.Simulation.Core;
using Xunit;

namespace NetForge.Simulation.Tests.Core
{
    public class DeviceModeTests
    {
        [Fact]
        public void ToModeString_ShouldReturnCorrectStringForBasicModes()
        {
            // Arrange & Act & Assert
            Assert.Equal("user", DeviceMode.User.ToModeString());
            Assert.Equal("privileged", DeviceMode.Privileged.ToModeString());
            Assert.Equal("config", DeviceMode.Config.ToModeString());
            Assert.Equal("interface", DeviceMode.Interface.ToModeString());
            Assert.Equal("vlan", DeviceMode.Vlan.ToModeString());
            Assert.Equal("router", DeviceMode.Router.ToModeString());
            Assert.Equal("bgp", DeviceMode.RouterBgp.ToModeString());
            Assert.Equal("ospf", DeviceMode.RouterOspf.ToModeString());
            Assert.Equal("rip", DeviceMode.RouterRip.ToModeString());
            Assert.Equal("acl", DeviceMode.Acl.ToModeString());
        }

        [Fact]
        public void ToModeString_ShouldReturnCorrectStringForJuniperModes()
        {
            // Arrange & Act & Assert
            Assert.Equal("operational", DeviceMode.Operational.ToModeString());
            Assert.Equal("configuration", DeviceMode.Configuration.ToModeString());
        }

        [Fact]
        public void ToModeString_ShouldReturnCorrectStringForFortinetModes()
        {
            // Arrange & Act & Assert
            Assert.Equal("global", DeviceMode.Global.ToModeString());
            Assert.Equal("global_config", DeviceMode.GlobalConfig.ToModeString());
            Assert.Equal("system_if", DeviceMode.SystemInterface.ToModeString());
            Assert.Equal("router_ospf", DeviceMode.RouterOspfFortinet.ToModeString());
            Assert.Equal("router_bgp", DeviceMode.RouterBgpFortinet.ToModeString());
            Assert.Equal("bgp_neighbor", DeviceMode.BgpNeighbor.ToModeString());
            Assert.Equal("bgp_neighbor_edit", DeviceMode.BgpNeighborEdit.ToModeString());
            Assert.Equal("bgp_network", DeviceMode.BgpNetwork.ToModeString());
            Assert.Equal("bgp_network_edit", DeviceMode.BgpNetworkEdit.ToModeString());
            Assert.Equal("router_rip", DeviceMode.RouterRipFortinet.ToModeString());
            Assert.Equal("router_static", DeviceMode.RouterStatic.ToModeString());
            Assert.Equal("static_route_edit", DeviceMode.StaticRouteEdit.ToModeString());
            Assert.Equal("firewall", DeviceMode.Firewall.ToModeString());
        }

        [Fact]
        public void ToModeString_ShouldReturnCorrectStringForNokiaModes()
        {
            // Arrange & Act & Assert
            Assert.Equal("admin", DeviceMode.Admin.ToModeString());
        }

        [Fact]
        public void FromModeString_ShouldReturnCorrectModeForBasicStrings()
        {
            // Arrange & Act & Assert
            Assert.Equal(DeviceMode.User, DeviceModeExtensions.FromModeString("user"));
            Assert.Equal(DeviceMode.Privileged, DeviceModeExtensions.FromModeString("privileged"));
            Assert.Equal(DeviceMode.Config, DeviceModeExtensions.FromModeString("config"));
            Assert.Equal(DeviceMode.Interface, DeviceModeExtensions.FromModeString("interface"));
            Assert.Equal(DeviceMode.Vlan, DeviceModeExtensions.FromModeString("vlan"));
            Assert.Equal(DeviceMode.Router, DeviceModeExtensions.FromModeString("router"));
            Assert.Equal(DeviceMode.RouterBgp, DeviceModeExtensions.FromModeString("bgp"));
            Assert.Equal(DeviceMode.RouterOspf, DeviceModeExtensions.FromModeString("ospf"));
            Assert.Equal(DeviceMode.RouterRip, DeviceModeExtensions.FromModeString("rip"));
            Assert.Equal(DeviceMode.Acl, DeviceModeExtensions.FromModeString("acl"));
        }

        [Fact]
        public void FromModeString_ShouldReturnUserForInvalidString()
        {
            // Arrange & Act & Assert
            Assert.Equal(DeviceMode.User, DeviceModeExtensions.FromModeString("invalid"));
            Assert.Equal(DeviceMode.User, DeviceModeExtensions.FromModeString(""));
            Assert.Equal(DeviceMode.User, DeviceModeExtensions.FromModeString(null));
            Assert.Equal(DeviceMode.User, DeviceModeExtensions.FromModeString("PRIVILEGED")); // Case sensitive
        }

        [Fact]
        public void IsRouterMode_ShouldReturnTrueForRouterModes()
        {
            // Arrange & Act & Assert
            Assert.True(DeviceMode.Router.IsRouterMode());
            Assert.True(DeviceMode.RouterBgp.IsRouterMode());
            Assert.True(DeviceMode.RouterOspf.IsRouterMode());
            Assert.True(DeviceMode.RouterRip.IsRouterMode());
            Assert.True(DeviceMode.RouterOspfFortinet.IsRouterMode());
            Assert.True(DeviceMode.RouterBgpFortinet.IsRouterMode());
            Assert.True(DeviceMode.RouterRipFortinet.IsRouterMode());
            Assert.True(DeviceMode.RouterStatic.IsRouterMode());
        }

        [Fact]
        public void IsRouterMode_ShouldReturnFalseForNonRouterModes()
        {
            // Arrange & Act & Assert
            Assert.False(DeviceMode.User.IsRouterMode());
            Assert.False(DeviceMode.Privileged.IsRouterMode());
            Assert.False(DeviceMode.Config.IsRouterMode());
            Assert.False(DeviceMode.Interface.IsRouterMode());
            Assert.False(DeviceMode.Vlan.IsRouterMode());
            Assert.False(DeviceMode.Acl.IsRouterMode());
            Assert.False(DeviceMode.Operational.IsRouterMode());
            Assert.False(DeviceMode.Configuration.IsRouterMode());
            Assert.False(DeviceMode.Admin.IsRouterMode());
        }

        [Fact]
        public void IsConfigurationMode_ShouldReturnTrueForConfigModes()
        {
            // Arrange & Act & Assert
            Assert.True(DeviceMode.Config.IsConfigurationMode());
            Assert.True(DeviceMode.Interface.IsConfigurationMode());
            Assert.True(DeviceMode.Vlan.IsConfigurationMode());
            Assert.True(DeviceMode.Router.IsConfigurationMode());
            Assert.True(DeviceMode.RouterBgp.IsConfigurationMode());
            Assert.True(DeviceMode.RouterOspf.IsConfigurationMode());
            Assert.True(DeviceMode.RouterRip.IsConfigurationMode());
            Assert.True(DeviceMode.Acl.IsConfigurationMode());
            Assert.True(DeviceMode.Configuration.IsConfigurationMode()); // Juniper
            Assert.True(DeviceMode.GlobalConfig.IsConfigurationMode()); // Fortinet
        }

        [Fact]
        public void IsConfigurationMode_ShouldReturnFalseForNonConfigModes()
        {
            // Arrange & Act & Assert
            Assert.False(DeviceMode.User.IsConfigurationMode());
            Assert.False(DeviceMode.Privileged.IsConfigurationMode());
            Assert.False(DeviceMode.Operational.IsConfigurationMode()); // Juniper operational
            Assert.False(DeviceMode.Admin.IsConfigurationMode()); // Nokia admin
            Assert.False(DeviceMode.Global.IsConfigurationMode()); // Fortinet global (not config)
        }

        [Theory]
        [InlineData("user", DeviceMode.User)]
        [InlineData("privileged", DeviceMode.Privileged)]
        [InlineData("config", DeviceMode.Config)]
        [InlineData("interface", DeviceMode.Interface)]
        [InlineData("vlan", DeviceMode.Vlan)]
        [InlineData("router", DeviceMode.Router)]
        [InlineData("bgp", DeviceMode.RouterBgp)]
        [InlineData("ospf", DeviceMode.RouterOspf)]
        [InlineData("rip", DeviceMode.RouterRip)]
        [InlineData("acl", DeviceMode.Acl)]
        public void ToModeString_AndFromModeString_ShouldBeReversible(string modeString, DeviceMode mode)
        {
            // Act & Assert
            Assert.Equal(modeString, mode.ToModeString());
            Assert.Equal(mode, DeviceModeExtensions.FromModeString(modeString));
        }

        [Fact]
        public void DeviceMode_AllEnumValues_ShouldBeValid()
        {
            // Arrange
            var allModes = Enum.GetValues<DeviceMode>();

            // Act & Assert - Ensure all enum values can be converted to strings
            foreach (var mode in allModes)
            {
                var modeString = mode.ToModeString();
                Assert.NotNull(modeString);
                Assert.NotEmpty(modeString);
                Assert.NotEqual("unknown", modeString); // Should not return default "unknown"
            }
        }

        [Fact]
        public void DeviceMode_VendorSpecificModes_ShouldExist()
        {
            // Act & Assert - Verify all vendor-specific modes are defined
            var allModes = Enum.GetValues<DeviceMode>();

            // Basic Cisco-style modes
            Assert.Contains(DeviceMode.User, allModes);
            Assert.Contains(DeviceMode.Privileged, allModes);
            Assert.Contains(DeviceMode.Config, allModes);

            // Juniper modes
            Assert.Contains(DeviceMode.Operational, allModes);
            Assert.Contains(DeviceMode.Configuration, allModes);

            // Nokia modes
            Assert.Contains(DeviceMode.Admin, allModes);

            // Fortinet modes
            Assert.Contains(DeviceMode.Global, allModes);
            Assert.Contains(DeviceMode.GlobalConfig, allModes);
            Assert.Contains(DeviceMode.SystemInterface, allModes);
            Assert.Contains(DeviceMode.RouterOspfFortinet, allModes);
            Assert.Contains(DeviceMode.RouterBgpFortinet, allModes);
            Assert.Contains(DeviceMode.BgpNeighbor, allModes);
            Assert.Contains(DeviceMode.BgpNeighborEdit, allModes);
            Assert.Contains(DeviceMode.BgpNetwork, allModes);
            Assert.Contains(DeviceMode.BgpNetworkEdit, allModes);
            Assert.Contains(DeviceMode.RouterRipFortinet, allModes);
            Assert.Contains(DeviceMode.RouterStatic, allModes);
            Assert.Contains(DeviceMode.StaticRouteEdit, allModes);
            Assert.Contains(DeviceMode.Firewall, allModes);
        }

        [Theory]
        [InlineData("system_if", DeviceMode.SystemInterface)]
        [InlineData("router_ospf", DeviceMode.RouterOspfFortinet)]
        [InlineData("router_bgp", DeviceMode.RouterBgpFortinet)]
        [InlineData("bgp_neighbor", DeviceMode.BgpNeighbor)]
        [InlineData("bgp_neighbor_edit", DeviceMode.BgpNeighborEdit)]
        [InlineData("bgp_network", DeviceMode.BgpNetwork)]
        [InlineData("bgp_network_edit", DeviceMode.BgpNetworkEdit)]
        [InlineData("router_rip", DeviceMode.RouterRipFortinet)]
        [InlineData("router_static", DeviceMode.RouterStatic)]
        [InlineData("static_route_edit", DeviceMode.StaticRouteEdit)]
        [InlineData("firewall", DeviceMode.Firewall)]
        public void FortinetModes_ShouldConvertCorrectly(string expectedString, DeviceMode mode)
        {
            // Act
            var result = mode.ToModeString();

            // Assert
            Assert.Equal(expectedString, result);
        }

        [Theory]
        [InlineData("UNKNOWN")]
        [InlineData("")]
        [InlineData("invalid_mode")]
        [InlineData("12345")]
        [InlineData("@#$%")]
        public void FromModeString_WithInvalidInput_ShouldDefaultToUser(string invalidInput)
        {
            // Act
            var result = DeviceModeExtensions.FromModeString(invalidInput);

            // Assert
            Assert.Equal(DeviceMode.User, result);
        }

        [Fact]
        public void FromModeString_WithNullInput_ShouldDefaultToUser()
        {
            // Act
            var result = DeviceModeExtensions.FromModeString(null);

            // Assert
            Assert.Equal(DeviceMode.User, result);
        }
    }
} 
