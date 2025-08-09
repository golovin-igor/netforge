using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Dell
{
    public class DellCommandHandlerComprehensiveTests
    {
        [Fact]
        public void DellHandler_ConfigureTerminal_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure terminal");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_Hostname_ShouldSetHostname()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("hostname NewSwitch");
            
            // Assert
            Assert.Equal("NewSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_ShowRunningConfig_ShouldDisplayConfig()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show running-configuration");
            
            // Assert
            Assert.Contains("Current configuration", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_ShowIpInterface_ShouldDisplayInterfaces()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show ip interface brief");
            
            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_ShowIpRoute_ShouldDisplayRoutes()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show ip route");
            
            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_ShowArp_ShouldDisplayArpTable()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show arp");
            
            // Assert
            Assert.Contains("ARP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_InterfaceEthernet_ShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("interface ethernet 1/1/1");
            
            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-if-eth1/1/1)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_IpAddress_ShouldConfigureInterface()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface ethernet 1/1/1");
            
            // Act
            var output = device.ProcessCommand("ip address 192.168.1.1/24");
            
            // Assert
            Assert.Equal("TestSwitch(config-if-eth1/1/1)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_NoShutdown_ShouldEnableInterface()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface ethernet 1/1/1");
            
            // Act
            var output = device.ProcessCommand("no shutdown");
            
            // Assert
            Assert.Equal("TestSwitch(config-if-eth1/1/1)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_IpRoute_ShouldConfigureRoute()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("ip route 10.0.0.0/8 192.168.1.1");
            
            // Assert
            Assert.Equal("TestSwitch(config)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_RouterOspf_ShouldEnterOspfMode()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("router ospf 1");
            
            // Assert
            Assert.Equal("router", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-router)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_RouterBgp_ShouldEnterBgpMode()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("router bgp 65001");
            
            // Assert
            Assert.Equal("router", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-router)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_Vlan_ShouldEnterVlanMode()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("vlan 100");
            
            // Assert
            Assert.Equal("vlan", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-vlan)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_ShowVersion_ShouldDisplayVersion()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show version");
            
            // Assert
            Assert.Contains("Version", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_ShowVlan_ShouldDisplayVlans()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show vlan");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_ShowSpanningTree_ShouldDisplayStp()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show spanning-tree mst");
            
            // Assert
            Assert.Contains("Spanning Tree", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_IpVrf_ShouldConfigureVrf()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("ip vrf TestVrf");
            
            // Assert
            Assert.Equal("vrf", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-vrf)#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_ShowEvpn_ShouldDisplayEvpnInfo()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show evpn");
            
            // Assert
            Assert.Contains("EVPN", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void DellHandler_VltDomain_ShouldConfigureVlt()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("vlt domain 1");
            
            // Assert
            Assert.Equal("vlt", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-vlt-domain)#", device.GetPrompt());
        }

        [Theory]
        [InlineData("show running-configuration")]
        [InlineData("show ip interface brief")]
        [InlineData("show ip route")]
        [InlineData("show arp")]
        [InlineData("show version")]
        [InlineData("show vlan")]
        [InlineData("show spanning-tree mst")]
        [InlineData("show ip vrf")]
        [InlineData("show evpn")]
        [InlineData("show ip ospf neighbor")]
        [InlineData("show ip bgp summary")]
        [InlineData("show interface ethernet 1/1/1")]
        [InlineData("show mac address-table")]
        [InlineData("show ip dhcp binding")]
        [InlineData("show port-channel summary")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("traceroute 127.0.0.1")]
        public void DellHandler_AllShowCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable"); // Enter privileged mode
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("configure terminal")]
        [InlineData("hostname NewName")]
        [InlineData("interface ethernet 1/1/1")]
        [InlineData("ip address 192.168.1.1/24")]
        [InlineData("no shutdown")]
        [InlineData("ip route 10.0.0.0/8 192.168.1.1")]
        [InlineData("router ospf 1")]
        [InlineData("router bgp 65001")]
        [InlineData("vlan 100")]
        [InlineData("ip vrf TestVrf")]
        [InlineData("vlt domain 1")]
        [InlineData("evpn enable")]
        public void DellHandler_ConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            if (!command.StartsWith("configure terminal"))
            {
                device.ProcessCommand("configure terminal");
            }
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("show ip route summary")]
        [InlineData("show ip route detail")]
        [InlineData("show ip route all")]
        [InlineData("show arp summary")]
        [InlineData("show arp detail")]
        [InlineData("show arp statistics")]
        [InlineData("show interface description")]
        [InlineData("show interface mtu")]
        [InlineData("show interface speed")]
        [InlineData("show interface duplex")]
        [InlineData("show ip ospf interface")]
        [InlineData("show ip bgp detail")]
        [InlineData("show ip rip database")]
        [InlineData("show ip multicast")]
        [InlineData("show ip dhcp server")]
        [InlineData("show port-security")]
        public void DellHandler_DetailedShowCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable"); // Enter privileged mode
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("show ip route vrf TestVrf")]
        [InlineData("show arp vrf TestVrf")]
        [InlineData("show ip ospf vrf TestVrf")]
        [InlineData("show ip bgp vrf TestVrf")]
        [InlineData("show ip multicast vrf TestVrf")]
        [InlineData("show ip dhcp server vrf TestVrf")]
        [InlineData("show interface ethernet 1/1/1 vrf TestVrf")]
        public void DellHandler_VrfCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable"); // Enter privileged mode
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("ip vrf TestVrf");
            device.ProcessCommand("exit");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("show ip route ospf")]
        [InlineData("show ip route bgp")]
        [InlineData("show ip route static")]
        [InlineData("show ip route evpn")]
        [InlineData("show ip route connected")]
        [InlineData("show ip route ospf summary")]
        [InlineData("show ip route bgp summary")]
        [InlineData("show ip route static summary")]
        [InlineData("show ip route evpn summary")]
        public void DellHandler_RoutingProtocolCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("enable"); // Enter privileged mode
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("ip arp gratuitous")]
        [InlineData("ip arp timeout 300")]
        [InlineData("ip arp static")]
        [InlineData("ip multicast-routing")]
        [InlineData("ip dhcp pool TestPool")]
        [InlineData("port-security enable")]
        [InlineData("ip source-guard enable")]
        [InlineData("ip dhcp snooping")]
        [InlineData("aaa authentication login local")]
        public void DellHandler_AdvancedConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Fact]
        public void DellHandler_EvpnVniConfiguration_ShouldWork()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act & Assert for EVPN VNI configuration
            var commands = new[]
            {
                "evpn enable",
                "evpn vni 100",
                "evpn vni 100 type vlan",
                "ip route evpn",
                "ip bgp evpn"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public void DellHandler_VltConfiguration_ShouldWork()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act & Assert for VLT configuration
            var commands = new[]
            {
                "vlt domain 1",
                "vlt domain 1 peer-link port-channel 1",
                "vlt domain 1 backup destination 192.168.1.2"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public void DellHandler_ComplexVrfConfiguration_ShouldWork()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            
            // Act & Assert for complex VRF configuration
            var commands = new[]
            {
                "ip vrf TestVrf",
                "ip route vrf TestVrf 10.0.0.0/8 192.168.1.1",
                "ip ospf vrf TestVrf",
                "ip bgp vrf TestVrf",
                "ip multicast vrf TestVrf",
                "ip dhcp server vrf TestVrf"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public void DellHandler_InterfaceAdvancedConfiguration_ShouldWork()
        {
            // Arrange
            var device = new DellDevice("TestSwitch");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("interface ethernet 1/1/1");
            
            // Act & Assert for advanced interface configuration
            var commands = new[]
            {
                "ip address 192.168.1.1/24",
                "no shutdown",
                "mtu 9000",
                "description Test Interface",
                "speed 10000",
                "duplex full",
                "flowcontrol"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }
    }
}
