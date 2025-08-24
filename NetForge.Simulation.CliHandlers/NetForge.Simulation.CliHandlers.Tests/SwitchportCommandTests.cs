using NetForge.Simulation.Core.Devices;
using Xunit;
// For VlanConfig
// Assuming Cisco context for switchport

// Added for Xunit

namespace NetForge.Simulation.Tests.CliHandlers
{
    public class SwitchportCommandTests
    {
        [Fact]
        public async Task SwitchportModeAccessShouldConfigureAccessMode()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");

            // Act
            var result = await device.ProcessCommandAsync("switchport mode access");

            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal("access", iface.SwitchportMode);
            Assert.Contains("switchport mode access", device.ShowRunningConfig());
        }

        [Fact]
        public async Task SwitchportAccessVlanShouldAssignVlan()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("vlan 100");
            await device.ProcessCommandAsync("name TestVLAN");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");

            // Act
            var result = await device.ProcessCommandAsync("switchport access vlan 100");

            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal(100, iface.VlanId);
            Assert.Contains("switchport access vlan 100", device.ShowRunningConfig());
        }

        [Fact]
        public async Task SwitchportModeTrunkShouldConfigureTrunkMode()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");

            // Act
            var result = await device.ProcessCommandAsync("switchport mode trunk");

            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal("trunk", iface.SwitchportMode);
            Assert.Contains("switchport mode trunk", device.ShowRunningConfig());
        }

        [Fact]
        public async Task SwitchportTrunkEncapsulationShouldConfigureEncapsulation()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");

            // Act
            var result = await device.ProcessCommandAsync("switchport trunk encapsulation dot1q");

            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            Assert.Contains("switchport trunk encapsulation dot1q", device.ShowRunningConfig());
        }

        [Fact]
        public async Task SwitchportVoiceVlanShouldConfigureVoiceVlan()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");

            // Act
            var result = await device.ProcessCommandAsync("switchport voice vlan 200");

            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            Assert.Contains("switchport voice vlan 200", device.ShowRunningConfig());
        }

        [Fact]
        public async Task SwitchportBasicShouldEnableSwitchport()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");

            // Act
            var result = await device.ProcessCommandAsync("switchport");

            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal("access", iface.SwitchportMode); // Default to access mode
            Assert.Contains(" switchport", device.ShowRunningConfig());
        }

        [Fact]
        public async Task SwitchportAccessVlanNonExistentVlanShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");

            // Act
            var result = await device.ProcessCommandAsync("switchport access vlan 999");

            // Assert
            Assert.Equal("% VLAN not foundTestSwitch(config-if)#", result);
        }
    }
}
