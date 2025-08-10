using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Cisco
{
    public class CiscoCommandHandlerCompleteTests
    {
        [Fact]
        public async Task RouterModeNetworkCommandShouldAddOspfNetwork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router ospf 1");
            
            // Act
            var output = await device.ProcessCommandAsync("network 10.0.0.0 0.0.0.255 area 0");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", output);
            var config = device.GetOspfConfig();
            Assert.NotNull(config);
            Assert.True(config.NetworkAreas.ContainsKey("10.0.0.0"));
        }
        
        [Fact]
        public async Task BgpNeighborCommandShouldConfigureBgpPeer()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router bgp 65001");
            
            // Act
            var output1 = await device.ProcessCommandAsync("neighbor 192.168.1.2 remote-as 65002");
            var output2 = await device.ProcessCommandAsync("neighbor 192.168.1.2 description ISP-Link");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            var bgp = device.GetBgpConfig();
            Assert.NotNull(bgp);
            Assert.Single(bgp.Neighbors);
            Assert.Equal("ISP-Link", bgp.Neighbors.Values.First().Description);
        }
        
        [Fact]
        public async Task AccessListCommandShouldCreateAcl()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("access-list 10 permit host 192.168.1.1");
            
            // Assert
            Assert.Equal("TestRouter(config)#", output);
            Assert.Contains("access-list 10 permit", device.ShowRunningConfig());
        }
        
        [Fact]
        public async Task SpanningTreeCommandShouldConfigureStp()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("spanning-tree mode rapid-pvst");
            var output2 = await device.ProcessCommandAsync("spanning-tree vlan 10 priority 24576");
            
            // Assert
            Assert.Equal("TestSwitch(config)#", output1);
            Assert.Equal("TestSwitch(config)#", output2);
            Assert.Contains("spanning-tree mode rapid-pvst", device.ShowRunningConfig());
            Assert.Contains("spanning-tree vlan 10 priority 24576", device.ShowRunningConfig());
        }
        
        [Fact]
        public async Task CdpCommandShouldConfigureCdp()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("cdp run");
            var output2 = await device.ProcessCommandAsync("cdp timer 90");
            var output3 = await device.ProcessCommandAsync("cdp holdtime 270");
            
            // Assert
            Assert.Equal("TestRouter(config)#", output1);
            Assert.Equal("TestRouter(config)#", output2);
            Assert.Equal("TestRouter(config)#", output3);
            
            // Test CDP status
            var cdpStatus = await device.ProcessCommandAsync("show cdp");
            Assert.Contains("Sending CDP packets every 90 seconds", cdpStatus);
            Assert.Contains("holdtime value of 270 seconds", cdpStatus);
        }
        
        [Fact]
        public async Task ShowCdpNeighborsShouldDisplayCdpInfo()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show cdp neighbors");
            
            // Assert
            Assert.Contains("Capability Codes:", output);
            Assert.Contains("Device ID", output);
            Assert.Contains("Total cdp entries displayed : 0", output);
            Assert.Contains("TestRouter>", output);
        }
        
        [Fact]
        public async Task ClearCommandsShouldClearVariousInfo()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output1 = await device.ProcessCommandAsync("clear counters");
            var output2 = await device.ProcessCommandAsync("clear ip route *");
            var output3 = await device.ProcessCommandAsync("clear cdp table");
            
            // Assert
            Assert.Equal("TestRouter#", output1);
            Assert.Equal("TestRouter#", output2);
            Assert.Equal("TestRouter#", output3);
        }
        
        [Fact]
        public async Task InterfaceCdpCommandShouldConfigureCdpOnInterface()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0");
            
            // Act
            var output = await device.ProcessCommandAsync("cdp enable");
            
            // Assert
            Assert.Equal("TestRouter(config-if)#", output);
            Assert.Contains(" cdp enable", device.ShowRunningConfig());
        }
        
        [Fact]
        public async Task IpAccessListCommandShouldCreateNamedAcl()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("ip access-list standard MGMT-ACCESS");
            var output2 = await device.ProcessCommandAsync("permit host 10.1.1.1");
            var output3 = await device.ProcessCommandAsync("deny any");
            var output4 = await device.ProcessCommandAsync("exit");
            
            // Assert
            Assert.Equal("TestRouter(config-std-nacl)#", output1);
            Assert.Equal("TestRouter(config-std-nacl)#", output2);
            Assert.Equal("TestRouter(config-std-nacl)#", output3);
            Assert.Equal("TestRouter(config)#", output4);
            Assert.Contains("ip access-list standard MGMT-ACCESS", device.ShowRunningConfig());
        }
        
        [Fact]
        public async Task RipVersionCommandShouldConfigureRipVersion()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router rip");
            
            // Act
            var output1 = await device.ProcessCommandAsync("version 2");
            var output2 = await device.ProcessCommandAsync("network 10.0.0.0");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            var rip = device.GetRipConfig();
            Assert.NotNull(rip);
            Assert.Equal(2, rip.Version);
            Assert.Contains("10.0.0.0", rip.Networks);
        }
        
        [Fact]
        public async Task ComplexConfigSequenceShouldWorkCorrectly()
        {
            // Arrange
            var device = new CiscoDevice("CoreSwitch");
            
            // Act - Build a complex configuration
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("hostname CoreSwitch01");
            await device.ProcessCommandAsync("vlan 100");
            await device.ProcessCommandAsync("name Management");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");
            await device.ProcessCommandAsync("switchport mode access");
            await device.ProcessCommandAsync("switchport access vlan 100");
            await device.ProcessCommandAsync("spanning-tree portfast");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("spanning-tree mode rapid-pvst");
            await device.ProcessCommandAsync("cdp run");
            await device.ProcessCommandAsync("ip route 0.0.0.0 0.0.0.0 10.1.1.1");
            
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
