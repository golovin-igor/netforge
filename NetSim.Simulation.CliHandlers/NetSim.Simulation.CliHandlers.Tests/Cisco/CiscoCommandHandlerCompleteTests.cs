using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Cisco
{
    public class CiscoCommandHandlerCompleteTests
    {
        [Fact]
        public void RouterModeNetworkCommand_ShouldAddOspfNetwork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("router ospf 1");
            
            // Act
            var output = device.ProcessCommand("network 10.0.0.0 0.0.0.255 area 0");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", output);
            var config = device.GetOspfConfig();
            Assert.NotNull(config);
            Assert.True(config.NetworkAreas.ContainsKey("10.0.0.0"));
        }
        
        [Fact]
        public void BgpNeighborCommand_ShouldConfigureBgpPeer()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("router bgp 65001");
            
            // Act
            var output1 = device.ProcessCommand("neighbor 192.168.1.2 remote-as 65002");
            var output2 = device.ProcessCommand("neighbor 192.168.1.2 description ISP-Link");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            var bgp = device.GetBgpConfig();
            Assert.NotNull(bgp);
            Assert.Single(bgp.Neighbors);
            Assert.Equal("ISP-Link", bgp.Neighbors.Values.First().Description);
        }
        
        [Fact]
        public void AccessListCommand_ShouldCreateAcl()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("access-list 10 permit host 192.168.1.1");
            
            // Assert
            Assert.Equal("TestRouter(config)#", output);
            Assert.Contains("access-list 10 permit", device.ShowRunningConfig());
        }
        
        [Fact]
        public void SpanningTreeCommand_ShouldConfigureStp()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output1 = device.ProcessCommand("spanning-tree mode rapid-pvst");
            var output2 = device.ProcessCommand("spanning-tree vlan 10 priority 24576");
            
            // Assert
            Assert.Equal("TestSwitch(config)#", output1);
            Assert.Equal("TestSwitch(config)#", output2);
            Assert.Contains("spanning-tree mode rapid-pvst", device.ShowRunningConfig());
            Assert.Contains("spanning-tree vlan 10 priority 24576", device.ShowRunningConfig());
        }
        
        [Fact]
        public void CdpCommand_ShouldConfigureCdp()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output1 = device.ProcessCommand("cdp run");
            var output2 = device.ProcessCommand("cdp timer 90");
            var output3 = device.ProcessCommand("cdp holdtime 270");
            
            // Assert
            Assert.Equal("TestRouter(config)#", output1);
            Assert.Equal("TestRouter(config)#", output2);
            Assert.Equal("TestRouter(config)#", output3);
            
            // Test CDP status
            var cdpStatus = device.ProcessCommand("show cdp");
            Assert.Contains("Sending CDP packets every 90 seconds", cdpStatus);
            Assert.Contains("holdtime value of 270 seconds", cdpStatus);
        }
        
        [Fact]
        public void ShowCdpNeighbors_ShouldDisplayCdpInfo()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show cdp neighbors");
            
            // Assert
            Assert.Contains("Capability Codes:", output);
            Assert.Contains("Device ID", output);
            Assert.Contains("Total cdp entries displayed : 0", output);
            Assert.Contains("TestRouter>", output);
        }
        
        [Fact]
        public void ClearCommands_ShouldClearVariousInfo()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            
            // Act
            var output1 = device.ProcessCommand("clear counters");
            var output2 = device.ProcessCommand("clear ip route *");
            var output3 = device.ProcessCommand("clear cdp table");
            
            // Assert
            Assert.Equal("TestRouter#", output1);
            Assert.Equal("TestRouter#", output2);
            Assert.Equal("TestRouter#", output3);
        }
        
        [Fact]
        public void InterfaceCdpCommand_ShouldConfigureCdpOnInterface()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Act
            var output = device.ProcessCommand("cdp enable");
            
            // Assert
            Assert.Equal("TestRouter(config-if)#", output);
            Assert.Contains(" cdp enable", device.ShowRunningConfig());
        }
        
        [Fact]
        public void IpAccessListCommand_ShouldCreateNamedAcl()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output1 = device.ProcessCommand("ip access-list standard MGMT-ACCESS");
            var output2 = device.ProcessCommand("permit host 10.1.1.1");
            var output3 = device.ProcessCommand("deny any");
            var output4 = device.ProcessCommand("exit");
            
            // Assert
            Assert.Equal("TestRouter(config-std-nacl)#", output1);
            Assert.Equal("TestRouter(config-std-nacl)#", output2);
            Assert.Equal("TestRouter(config-std-nacl)#", output3);
            Assert.Equal("TestRouter(config)#", output4);
            Assert.Contains("ip access-list standard MGMT-ACCESS", device.ShowRunningConfig());
        }
        
        [Fact]
        public void RipVersionCommand_ShouldConfigureRipVersion()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("router rip");
            
            // Act
            var output1 = device.ProcessCommand("version 2");
            var output2 = device.ProcessCommand("network 10.0.0.0");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            var rip = device.GetRipConfig();
            Assert.NotNull(rip);
            Assert.Equal(2, rip.Version);
            Assert.Contains("10.0.0.0", rip.Networks);
        }
        
        [Fact]
        public void ComplexConfigSequence_ShouldWorkCorrectly()
        {
            // Arrange
            var device = new CiscoDevice("CoreSwitch");
            
            // Act - Build a complex configuration
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("hostname CoreSwitch01");
            device.ProcessCommand("vlan 100");
            device.ProcessCommand("name Management");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface GigabitEthernet0/1");
            device.ProcessCommand("switchport mode access");
            device.ProcessCommand("switchport access vlan 100");
            device.ProcessCommand("spanning-tree portfast");
            device.ProcessCommand("exit");
            device.ProcessCommand("spanning-tree mode rapid-pvst");
            device.ProcessCommand("cdp run");
            device.ProcessCommand("ip route 0.0.0.0 0.0.0.0 10.1.1.1");
            
            // Assert
            var config = device.ShowRunningConfig();
            Assert.Contains("hostname CoreSwitch01", config);
            Assert.Contains("vlan 100", config);
            Assert.Contains("name Management", config);
            Assert.Contains("switchport mode access", config);
            Assert.Contains("switchport access vlan 100", config);
            Assert.Contains("spanning-tree portfast", config);
            Assert.Contains("spanning-tree mode rapid-pvst", config);
            Assert.Contains("cdp run", config);
            Assert.Contains("ip route 0.0.0.0 0.0.0.0 10.1.1.1", config);
        }
    }
} 
