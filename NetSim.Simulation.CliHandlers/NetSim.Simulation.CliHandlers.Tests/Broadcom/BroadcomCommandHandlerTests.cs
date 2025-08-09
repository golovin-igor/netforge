using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Broadcom
{
    public class BroadcomCommandHandlerTests
    {
        [Fact]
        public async Task BroadcomHandler_ConfigureTerminal_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("configure terminal");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_Hostname_ShouldSetHostname()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("hostname NewSwitch");
            
            // Assert
            Assert.Equal("NewSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowRunningConfig_ShouldDisplayConfig()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show running-config");
            
            // Assert
            Assert.Contains("Current configuration", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowIpInterface_ShouldDisplayInterfaces()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show ip interface brief");
            
            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowIpRoute_ShouldDisplayRoutes()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show ip route");
            
            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowArp_ShouldDisplayArpTable()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show arp");
            
            // Assert
            Assert.Contains("ARP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_InterfaceEthernet_ShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("interface ethernet 1/0/1");
            
            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-if)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_IpAddress_ShouldConfigureInterface()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface ethernet 1/0/1");
            
            // Act
            var output = await device.ProcessCommandAsync("ip address 192.168.1.1/24");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_NoShutdown_ShouldEnableInterface()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface ethernet 1/0/1");
            
            // Act
            var output = await device.ProcessCommandAsync("no shutdown");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_IpRoute_ShouldConfigureRoute()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("ip route 10.0.0.0/8 192.168.1.1");
            
            // Assert
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_RouterOspf_ShouldEnterOspfMode()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("router ospf 1");
            
            // Assert
            Assert.Equal("router", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-router)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_RouterBgp_ShouldEnterBgpMode()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("router bgp 65001");
            
            // Assert
            Assert.Equal("router", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-router)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_Vlan_ShouldEnterVlanMode()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("vlan 100");
            
            // Assert
            Assert.Equal("vlan", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-vlan)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_PingCommand_ShouldExecutePing()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("ping 8.8.8.8");
            
            // Assert
            Assert.Contains("ping", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_TracerouteCommand_ShouldExecuteTraceroute()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("traceroute 8.8.8.8");
            
            // Assert
            Assert.Contains("traceroute", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowVersion_ShouldDisplayVersion()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show version");
            
            // Assert
            Assert.Contains("Version", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowVlan_ShouldDisplayVlans()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show vlan brief");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_SwitchportMode_ShouldConfigureSwitchport()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface ethernet 1/0/1");
            
            // Act
            var output = await device.ProcessCommandAsync("switchport mode access");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_SwitchportAccess_ShouldConfigureAccessVlan()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface ethernet 1/0/1");
            
            // Act
            var output = await device.ProcessCommandAsync("switchport access vlan 100");
            
            // Assert
            Assert.Equal("TestSwitch(config-if)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowMacAddressTable_ShouldDisplayMacTable()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show mac address-table");
            
            // Assert
            Assert.Contains("MAC", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowSpanningTree_ShouldDisplayStp()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show spanning-tree");
            
            // Assert
            Assert.Contains("Spanning Tree", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_MlagConfiguration_ShouldEnterMlagMode()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("mlag configuration");
            
            // Assert
            Assert.Equal("mlag", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-mlag)#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_BashCommand_ShouldExecuteLinuxCommand()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("bash ls");
            
            // Assert
            Assert.Contains("bash", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_ShowEvpn_ShouldDisplayEvpnInfo()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show evpn");
            
            // Assert
            Assert.Contains("EVPN", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task BroadcomHandler_WithInvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("invalid command");
            
            // Assert
            Assert.Contains("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("show running-config")]
        [InlineData("show ip interface brief")]
        [InlineData("show ip route")]
        [InlineData("show arp")]
        [InlineData("show version")]
        [InlineData("show vlan brief")]
        [InlineData("show mac address-table")]
        [InlineData("show spanning-tree")]
        [InlineData("show mlag")]
        [InlineData("show evpn")]
        [InlineData("show ip ospf neighbor")]
        [InlineData("show ip bgp summary")]
        [InlineData("show interface ethernet 1/0/1")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("traceroute 127.0.0.1")]
        public async Task BroadcomHandler_AllShowCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("enable"); // Enter privileged mode
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("configure terminal")]
        [InlineData("hostname NewName")]
        [InlineData("interface ethernet 1/0/1")]
        [InlineData("ip address 192.168.1.1/24")]
        [InlineData("no shutdown")]
        [InlineData("ip route 10.0.0.0/8 192.168.1.1")]
        [InlineData("router ospf 1")]
        [InlineData("router bgp 65001")]
        [InlineData("vlan 100")]
        [InlineData("switchport mode access")]
        [InlineData("mlag configuration")]
        public async Task BroadcomHandler_ConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            if (!command.StartsWith("configure terminal"))
            {
                await device.ProcessCommandAsync("configure terminal");
            }
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("ip arp gratuitous")]
        [InlineData("ip arp timeout 300")]
        [InlineData("ip arp static")]
        [InlineData("ip vrf TestVrf")]
        [InlineData("ip multicast-routing")]
        [InlineData("ip dhcp pool TestPool")]
        [InlineData("evpn enable")]
        [InlineData("port-security enable")]
        public async Task BroadcomHandler_AdvancedCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new BroadcomDevice("TestSwitch");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }
    }
}
