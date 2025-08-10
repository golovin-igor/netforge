using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Cisco
{
    public class CiscoCommandHandlerComprehensiveTests
    {
        [Fact]
        public async Task CiscoHandlerConfigureTerminalShouldEnterConfigMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("configure terminal");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerHostnameShouldSetHostname()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("hostname NewRouter");
            
            // Assert
            Assert.Equal("NewRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerShowRunningConfigShouldDisplayConfig()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show running-config");
            
            // Assert
            Assert.Contains("Current configuration", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerShowVersionShouldDisplayVersion()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show version");
            
            // Assert
            Assert.Contains("Cisco IOS", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerShowInterfacesShouldDisplayInterfaces()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show interfaces");
            
            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerShowIpRouteShouldDisplayRoutes()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show ip route");
            
            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerShowArpShouldDisplayArpTable()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show arp");
            
            // Assert
            Assert.Contains("Protocol", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerInterfaceGigabitEthernetShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("interface GigabitEthernet0/0");
            
            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("TestRouter(config-if)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerIpAddressShouldConfigureInterface()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0");
            
            // Act
            var output = await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            
            // Assert
            Assert.Equal("TestRouter(config-if)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerNoShutdownShouldEnableInterface()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0");
            
            // Act
            var output = await device.ProcessCommandAsync("no shutdown");
            
            // Assert
            Assert.Equal("TestRouter(config-if)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerIpRouteShouldConfigureStaticRoute()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("ip route 10.0.0.0 255.0.0.0 192.168.1.1");
            
            // Assert
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerVlanShouldEnterVlanMode()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("vlan 100");
            
            // Assert
            Assert.Equal("vlan", device.GetCurrentMode());
            Assert.Equal("TestSwitch(config-vlan)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerVlanNameShouldSetVlanName()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("vlan 100");
            
            // Act
            var output = await device.ProcessCommandAsync("name TestVLAN");
            
            // Assert
            Assert.Equal("TestSwitch(config-vlan)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerShowVlanShouldDisplayVlans()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show vlan");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerShowSpanningTreeShouldDisplayStp()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            
            // Act
            var output = await device.ProcessCommandAsync("show spanning-tree");
            
            // Assert
            Assert.Contains("Spanning tree", output);
            Assert.Equal("TestSwitch>", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerShowCdpNeighborsShouldDisplayCdpInfo()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show cdp neighbors");
            
            // Assert
            Assert.Contains("Device ID", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Theory]
        [InlineData("show running-config")]
        [InlineData("show version")]
        [InlineData("show interfaces")]
        [InlineData("show ip route")]
        [InlineData("show arp")]
        [InlineData("show vlan")]
        [InlineData("show spanning-tree")]
        [InlineData("show cdp neighbors")]
        [InlineData("show ip ospf neighbor")]
        [InlineData("show ip bgp summary")]
        [InlineData("show ip eigrp neighbors")]
        [InlineData("show ip rip database")]
        [InlineData("show interfaces status")]
        [InlineData("show mac address-table")]
        [InlineData("show ip protocols")]
        [InlineData("show clock")]
        [InlineData("show processes")]
        [InlineData("show memory")]
        [InlineData("show flash")]
        [InlineData("show inventory")]
        [InlineData("show environment")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("traceroute 127.0.0.1")]
        public async Task CiscoHandlerAllShowCommandsShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Theory]
        [InlineData("enable")]
        [InlineData("configure terminal")]
        [InlineData("hostname NewName")]
        [InlineData("interface GigabitEthernet0/0")]
        [InlineData("ip address 192.168.1.1 255.255.255.0")]
        [InlineData("no shutdown")]
        [InlineData("ip route 10.0.0.0 255.0.0.0 192.168.1.1")]
        [InlineData("router ospf 1")]
        [InlineData("router bgp 65001")]
        [InlineData("router eigrp 100")]
        [InlineData("router rip")]
        [InlineData("vlan 100")]
        [InlineData("name TestVLAN")]
        [InlineData("spanning-tree mode rapid-pvst")]
        public async Task CiscoHandlerConfigurationCommandsShouldWork(string command)
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
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
        [InlineData("show ip route detail")]
        [InlineData("show ip route summary")]
        [InlineData("show arp statistics")]
        [InlineData("show vlan brief")]
        [InlineData("show spanning-tree detail")]
        [InlineData("show cdp neighbors detail")]
        [InlineData("show ip ospf database")]
        [InlineData("show ip bgp neighbors")]
        [InlineData("show ip eigrp topology")]
        [InlineData("show ip rip neighbor")]
        [InlineData("show mac address-table dynamic")]
        [InlineData("show ip access-lists")]
        [InlineData("show ip prefix-list")]
        [InlineData("show route-map")]
        public async Task CiscoHandlerDetailedShowCommandsShouldWork(string command)
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Theory]
        [InlineData("show ip route vrf management")]
        [InlineData("show arp vrf management")]
        [InlineData("show ip ospf vrf management")]
        [InlineData("show ip bgp vrf management")]
        [InlineData("show interfaces vrf management")]
        public async Task CiscoHandlerVrfCommandsShouldWork(string command)
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("vrf definition management");
            await device.ProcessCommandAsync("exit");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }

        [Theory]
        [InlineData("description Test Interface")]
        [InlineData("ip address 192.168.1.1 255.255.255.0")]
        [InlineData("no shutdown")]
        [InlineData("duplex full")]
        [InlineData("speed 1000")]
        [InlineData("cdp enable")]
        [InlineData("ip helper-address 192.168.1.100")]
        [InlineData("ip directed-broadcast")]
        [InlineData("ip proxy-arp")]
        [InlineData("ip redirects")]
        [InlineData("ip unreachables")]
        [InlineData("keepalive 10")]
        [InlineData("bandwidth 1000000")]
        public async Task CiscoHandlerInterfaceConfigurationCommandsShouldWork(string command)
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestRouter(config-if)#", device.GetPrompt());
        }

        [Theory]
        [InlineData("name TestVLAN")]
        [InlineData("state active")]
        [InlineData("shutdown")]
        [InlineData("no shutdown")]
        public async Task CiscoHandlerVlanConfigurationCommandsShouldWork(string command)
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("vlan 100");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch(config-vlan)#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoHandlerComplexOspfConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for complex OSPF configuration
            var commands = new[]
            {
                "router ospf 1",
                "router-id 1.1.1.1",
                "network 192.168.1.0 0.0.0.255 area 0",
                "network 10.0.0.0 0.255.255.255 area 0",
                "area 1 stub",
                "area 1 default-cost 100",
                "passive-interface default",
                "no passive-interface GigabitEthernet0/0",
                "redistribute bgp 65001 subnets",
                "redistribute eigrp 100 subnets",
                "redistribute rip subnets",
                "default-information originate"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task CiscoHandlerComplexBgpConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for complex BGP configuration
            var commands = new[]
            {
                "router bgp 65001",
                "router-id 2.2.2.2",
                "neighbor 192.168.1.2 remote-as 65002",
                "neighbor 192.168.1.3 remote-as 65001",
                "neighbor 192.168.1.2 update-source Loopback0",
                "neighbor 192.168.1.2 send-community",
                "neighbor 192.168.1.2 send-community extended",
                "address-family ipv4 unicast",
                "neighbor 192.168.1.2 activate",
                "neighbor 192.168.1.3 activate",
                "network 192.168.1.0 mask 255.255.255.0",
                "network 10.0.0.0 mask 255.0.0.0",
                "redistribute ospf 1",
                "redistribute eigrp 100"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task CiscoHandlerVlanAndSwitchportConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for VLAN and switchport configuration
            var commands = new[]
            {
                "vlan 100",
                "name Production",
                "exit",
                "vlan 200",
                "name Development", 
                "exit",
                "interface GigabitEthernet0/1",
                "switchport mode trunk",
                "switchport trunk allowed vlan 100,200",
                "exit",
                "interface GigabitEthernet0/2",
                "switchport mode access",
                "switchport access vlan 100"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task CiscoHandlerAccessListConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for access list configuration
            var commands = new[]
            {
                "access-list 100 permit tcp 192.168.1.0 0.0.0.255 any eq 80",
                "access-list 100 permit tcp 192.168.1.0 0.0.0.255 any eq 443",
                "access-list 100 deny ip any any",
                "ip access-list extended WEB-TRAFFIC",
                "permit tcp 192.168.1.0 0.0.0.255 any eq 80",
                "permit tcp 192.168.1.0 0.0.0.255 any eq 443",
                "deny ip any any",
                "exit",
                "ip prefix-list PL-NETWORKS seq 10 permit 192.168.0.0/16 le 24",
                "ip prefix-list PL-NETWORKS seq 20 deny 0.0.0.0/0"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task CiscoHandlerRouteMapConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for route map configuration
            var commands = new[]
            {
                "route-map RMAP-IN permit 10",
                "match ip address prefix-list PL-NETWORKS",
                "set metric 100",
                "set community 65001:100",
                "set local-preference 200",
                "exit",
                "route-map RMAP-OUT permit 10",
                "match community COMM-LIST",
                "set metric-type type-1",
                "set tag 100"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task CiscoHandlerSpanningTreeConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for spanning tree configuration
            var commands = new[]
            {
                "spanning-tree mode rapid-pvst",
                "spanning-tree vlan 1-100 priority 4096",
                "spanning-tree vlan 101-200 priority 8192",
                "spanning-tree portfast bpduguard default",
                "spanning-tree uplinkfast",
                "spanning-tree backbonefast",
                "interface GigabitEthernet0/1",
                "spanning-tree port-priority 128",
                "spanning-tree cost 200000",
                "spanning-tree portfast"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task CiscoHandlerVrfConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for VRF configuration
            var commands = new[]
            {
                "vrf definition MGMT",
                "rd 65001:100",
                "route-target export 65001:100",
                "route-target import 65001:100",
                "address-family ipv4",
                "exit-address-family",
                "exit",
                "interface GigabitEthernet0/0",
                "vrf forwarding MGMT",
                "ip address 192.168.100.1 255.255.255.0"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task CiscoHandlerSnmpConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for SNMP configuration
            var commands = new[]
            {
                "snmp-server community public RO",
                "snmp-server community private RW",
                "snmp-server host 192.168.1.100 public",
                "snmp-server host 192.168.1.101 private",
                "snmp-server enable traps",
                "snmp-server contact \"Network Admin\"",
                "snmp-server location \"Data Center\"",
                "snmp-server chassis-id TestRouter"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestRouter(config)#", device.GetPrompt());
            }
        }

        [Fact]
        public async Task CiscoHandlerNtpConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for NTP configuration
            var commands = new[]
            {
                "ntp server 192.168.1.1",
                "ntp server 192.168.1.2 prefer",
                "ntp authenticate",
                "ntp authentication-key 1 md5 secret",
                "ntp trusted-key 1",
                "clock timezone EST -5",
                "clock summer-time EDT recurring"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestRouter(config)#", device.GetPrompt());
            }
        }

        [Fact]
        public async Task CiscoHandlerLoggingConfigurationShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act & Assert for logging configuration
            var commands = new[]
            {
                "logging 192.168.1.100",
                "logging 192.168.1.101",
                "logging facility local7",
                "logging trap informational",
                "logging source-interface Loopback0",
                "no logging console",
                "logging buffered 32768",
                "service timestamps log datetime msec"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestRouter(config)#", device.GetPrompt());
            }
        }
    }
}
