using NetSim.Simulation.Devices;
using Xunit;
// For VlanConfig
// Assuming Cisco context for switchport

// Added for Xunit

namespace NetSim.Simulation.Tests.CliHandlers
{
    public class SwitchportCommandTests
    {
        [Fact]
        public void SwitchportModeAccess_ShouldConfigureAccessMode()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface GigabitEthernet0/1");
            
            // Act
            var result = device.ProcessCommand("switchport mode access");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal("access", iface.SwitchportMode);
            Assert.Contains("switchport mode access", device.ShowRunningConfig());
        }
        
        [Fact]
        public void SwitchportAccessVlan_ShouldAssignVlan()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("vlan 100");
            device.ProcessCommand("name TestVLAN");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface GigabitEthernet0/1");
            
            // Act
            var result = device.ProcessCommand("switchport access vlan 100");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal(100, iface.VlanId);
            Assert.Contains("switchport access vlan 100", device.ShowRunningConfig());
        }
        
        [Fact]
        public void SwitchportModeTrunk_ShouldConfigureTrunkMode()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface GigabitEthernet0/1");
            
            // Act
            var result = device.ProcessCommand("switchport mode trunk");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal("trunk", iface.SwitchportMode);
            Assert.Contains("switchport mode trunk", device.ShowRunningConfig());
        }
        
        [Fact]
        public void SwitchportTrunkEncapsulation_ShouldConfigureEncapsulation()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface GigabitEthernet0/1");
            
            // Act
            var result = device.ProcessCommand("switchport trunk encapsulation dot1q");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            Assert.Contains("switchport trunk encapsulation dot1q", device.ShowRunningConfig());
        }
        
        [Fact]
        public void SwitchportVoiceVlan_ShouldConfigureVoiceVlan()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface GigabitEthernet0/1");
            
            // Act
            var result = device.ProcessCommand("switchport voice vlan 200");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            Assert.Contains("switchport voice vlan 200", device.ShowRunningConfig());
        }
        
        [Fact]
        public void SwitchportBasic_ShouldEnableSwitchport()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface GigabitEthernet0/1");
            
            // Act
            var result = device.ProcessCommand("switchport");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", result);
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal("access", iface.SwitchportMode); // Default to access mode
            Assert.Contains(" switchport", device.ShowRunningConfig());
        }
        
        [Fact]
        public void SwitchportAccessVlan_NonExistentVlan_ShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface GigabitEthernet0/1");
            
            // Act
            var result = device.ProcessCommand("switchport access vlan 999");
            
            // Assert
            Assert.Equal("% VLAN not foundTestSwitch(config-if)#", result);
        }
    }
} 
