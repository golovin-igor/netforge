using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Arista
{
    public class AristaCommandHandlerComprehensiveTests
    {
        [Fact]
        public async Task AristaHandler_ConfigureTerminal_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("configure terminal");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_Hostname_ShouldSetHostname()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("hostname NewSwitch");
            
            // Assert
            Assert.Equal("NewSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowRunningConfig_ShouldDisplayConfig()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show running-config");
            
            // Assert
            Assert.Contains("Current configuration", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowInterfaces_ShouldDisplayInterfaces()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show interfaces");
            
            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowIpRoute_ShouldDisplayRoutes()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show ip route");
            
            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowArp_ShouldDisplayArpTable()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show arp");
            
            // Assert
            Assert.Contains("ARP", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_InterfaceEthernet_ShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("interface Ethernet1");
            
            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-if-Et1)#", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_IpAddress_ShouldConfigureInterface()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface Ethernet1");
            
            // Act
            var output = await device.ProcessCommandAsync("ip address 192.168.1.1/24");
            
            // Assert
            Assert.Equal("TestSwitch(config-if-Et1)#", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_NoShutdown_ShouldEnableInterface()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface Ethernet1");
            
            // Act
            var output = await device.ProcessCommandAsync("no shutdown");
            
            // Assert
            Assert.Equal("TestSwitch(config-if-Et1)#", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_RouterOspf_ShouldEnterOspfMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("router ospf 1");
            
            // Assert
            Assert.Equal("router", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-router-ospf)#", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_RouterBgp_ShouldEnterBgpMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("router bgp 65001");
            
            // Assert
            Assert.Equal("router", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-router-bgp)#", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_Vlan_ShouldEnterVlanMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("vlan 100");
            
            // Assert
            Assert.Equal("vlan", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-vlan-100)#", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowVlan_ShouldDisplayVlans()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show vlan");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowSpanningTree_ShouldDisplayStp()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show spanning-tree");
            
            // Assert
            Assert.Contains("Spanning Tree", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowMlag_ShouldDisplayMlagInfo()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show mlag");
            
            // Assert
            Assert.Contains("MLAG", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowVxlan_ShouldDisplayVxlanInfo()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show vxlan");
            
            // Assert
            Assert.Contains("VXLAN", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task AristaHandler_ShowBgpEvpn_ShouldDisplayEvpnInfo()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show bgp evpn");
            
            // Assert
            Assert.Contains("BGP EVPN", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Theory]
        [InlineData("show running-config")]
        [InlineData("show interfaces")]
        [InlineData("show ip route")]
        [InlineData("show arp")]
        [InlineData("show version")]
        [InlineData("show vlan")]
        [InlineData("show spanning-tree")]
        [InlineData("show mlag")]
        [InlineData("show vxlan")]
        [InlineData("show bgp evpn")]
        [InlineData("show ip ospf neighbor")]
        [InlineData("show ip bgp summary")]
        [InlineData("show interfaces status")]
        [InlineData("show mac address-table")]
        [InlineData("show lldp neighbors")]
        [InlineData("show port-channel")]
        [InlineData("show clock")]
        [InlineData("show inventory")]
        [InlineData("show environment")]
        [InlineData("show system")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("traceroute 127.0.0.1")]
        public async Task AristaHandler_AllShowCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Theory]
        [InlineData("enable")]
        [InlineData("configure terminal")]
        [InlineData("hostname NewName")]
        [InlineData("interface Ethernet1")]
        [InlineData("ip address 192.168.1.1/24")]
        [InlineData("no shutdown")]
        [InlineData("router ospf 1")]
        [InlineData("router bgp 65001")]
        [InlineData("vlan 100")]
        [InlineData("ip route 10.0.0.0/8 192.168.1.1")]
        [InlineData("mlag configuration")]
        [InlineData("interface vxlan1")]
        [InlineData("spanning-tree mode mstp")]
        public async Task AristaHandler_ConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            if (!command.StartsWith("enable") && !command.StartsWith("configure terminal"))
            {
                await device.ProcessCommandAsync("enable");
                await device.ProcessCommandAsync("configure terminal");
            }
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("show interfaces description")]
        [InlineData("show interfaces counters")]
        [InlineData("show interfaces transceiver")]
        [InlineData("show ip route detail")]
        [InlineData("show ip route summary")]
        [InlineData("show arp summary")]
        [InlineData("show vlan brief")]
        [InlineData("show spanning-tree detail")]
        [InlineData("show mlag detail")]
        [InlineData("show vxlan vtep")]
        [InlineData("show bgp evpn summary")]
        [InlineData("show ip ospf database")]
        [InlineData("show ip bgp neighbors")]
        [InlineData("show mac address-table dynamic")]
        [InlineData("show lldp neighbors detail")]
        [InlineData("show port-channel summary")]
        public async Task AristaHandler_DetailedShowCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Theory]
        [InlineData("show ip route vrf management")]
        [InlineData("show arp vrf management")]
        [InlineData("show ip ospf vrf management")]
        [InlineData("show ip bgp vrf management")]
        [InlineData("show interfaces vrf management")]
        public async Task AristaHandler_VrfCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("vrf instance management");
            await device.ProcessCommandAsync("exit");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Theory]
        [InlineData("interface port-channel 1")]
        [InlineData("port-channel load-balance src-dst-ip")]
        [InlineData("lacp mode active")]
        [InlineData("channel-group 1 mode active")]
        [InlineData("mlag configuration")]
        [InlineData("mlag domain-id Test")]
        [InlineData("mlag local-interface Vlan4094")]
        [InlineData("mlag peer-address 192.168.1.2")]
        [InlineData("mlag peer-link Port-Channel1")]
        public async Task AristaHandler_MlagConfiguration_ShouldWork(string command)
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("interface vxlan1")]
        [InlineData("vxlan source-interface Loopback0")]
        [InlineData("vxlan udp-port 4789")]
        [InlineData("vxlan vlan 100 vni 10100")]
        [InlineData("vxlan flood vtep 192.168.1.2")]
        [InlineData("evpn")]
        [InlineData("neighbor 192.168.1.2 activate")]
        [InlineData("advertise-all-vni")]
        public async Task AristaHandler_VxlanEvpnConfiguration_ShouldWork(string command)
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            if (command.StartsWith("neighbor") || command.StartsWith("advertise"))
            {
                await device.ProcessCommandAsync("router bgp 65001");
                await device.ProcessCommandAsync("address-family evpn");
            }
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Fact]
        public async Task AristaHandler_ComplexMlagConfiguration_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for complex MLAG configuration
            var commands = new[]
            {
                "mlag configuration",
                "domain-id MLAG1",
                "local-interface Vlan4094",
                "peer-address 192.168.255.2",
                "peer-link Port-Channel1",
                "reload-delay mlag 300",
                "reload-delay non-mlag 330"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task AristaHandler_VxlanConfiguration_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for VXLAN configuration
            var commands = new[]
            {
                "interface vxlan1",
                "vxlan source-interface Loopback0",
                "vxlan udp-port 4789",
                "vxlan vlan 100 vni 10100",
                "vxlan vlan 200 vni 10200",
                "vxlan flood vtep 192.168.1.2 192.168.1.3"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task AristaHandler_BgpEvpnConfiguration_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for BGP EVPN configuration
            var commands = new[]
            {
                "router bgp 65001",
                "neighbor 192.168.1.2 remote-as 65001",
                "neighbor 192.168.1.2 update-source Loopback0",
                "neighbor 192.168.1.2 send-community extended",
                "address-family evpn",
                "neighbor 192.168.1.2 activate",
                "advertise-all-vni"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task AristaHandler_SpanningTreeConfiguration_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for spanning tree configuration
            var commands = new[]
            {
                "spanning-tree mode mstp",
                "spanning-tree mst 0 priority 4096",
                "spanning-tree mst configuration",
                "name REGION1",
                "revision 1",
                "instance 1 vlan 100-200",
                "instance 2 vlan 300-400"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task AristaHandler_RouteMapConfiguration_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for route map configuration
            var commands = new[]
            {
                "route-map RMAP-IN permit 10",
                "match ip address prefix-list PL-NETWORKS",
                "set metric 100",
                "set community 65001:100",
                "route-map RMAP-OUT permit 10",
                "match community COMM-LIST",
                "set local-preference 200"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task AristaHandler_AccessListConfiguration_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for access list configuration
            var commands = new[]
            {
                "ip access-list extended ACL-TEST",
                "permit tcp 192.168.1.0/24 any eq 80",
                "permit tcp 192.168.1.0/24 any eq 443",
                "deny ip any any",
                "ip prefix-list PL-NETWORKS permit 192.168.0.0/16 le 24",
                "ip prefix-list PL-NETWORKS deny 0.0.0.0/0"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task AristaHandler_InterfaceAdvancedConfiguration_ShouldWork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface Ethernet1");
            
            // Act & Assert for advanced interface configuration
            var commands = new[]
            {
                "description Test Interface",
                "mtu 9000",
                "speed forced 10000full",
                "flow-control send on",
                "flow-control receive on",
                "storm-control broadcast level 10",
                "storm-control multicast level 10",
                "storm-control unknown-unicast level 10",
                "load-interval 30"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch(config-if-Et1)#", device.GetPrompt());
            }
        }
    }
}
