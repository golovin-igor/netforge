using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Extreme
{
    public class ExtremeCommandHandlerComprehensiveTests
    {
        [Fact]
        public void ExtremeHandler_ConfigureSystemName_ShouldSetSystemName()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure system name NewSwitch");
            
            // Assert
            Assert.Equal("NewSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureVlan_ShouldCreateVlan()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure vlan test100");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureVlanIpAddress_ShouldSetVlanIp()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            device.ProcessCommand("configure vlan test100");
            
            // Act
            var output = device.ProcessCommand("configure vlan test100 ipaddress 192.168.1.1/24");
            
            // Assert
            Assert.Contains("IP address", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureVlanAddPorts_ShouldAddPorts()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            device.ProcessCommand("configure vlan test100");
            
            // Act
            var output = device.ProcessCommand("configure vlan test100 add ports 1-5");
            
            // Assert
            Assert.Contains("ports", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ShowConfiguration_ShouldDisplayConfig()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show configuration");
            
            // Assert
            Assert.Contains("Configuration", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ShowIproute_ShouldDisplayRoutes()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show iproute");
            
            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ShowIparp_ShouldDisplayArpTable()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show iparp");
            
            // Assert
            Assert.Contains("ARP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureIprouteAdd_ShouldAddRoute()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure iproute add 10.0.0.0/8 192.168.1.1");
            
            // Assert
            Assert.Contains("route", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_EnableOspf_ShouldEnableOspf()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("enable ospf");
            
            // Assert
            Assert.Contains("OSPF", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureOspfAddVlan_ShouldAddVlanToOspf()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            device.ProcessCommand("enable ospf");
            device.ProcessCommand("configure vlan test100");
            
            // Act
            var output = device.ProcessCommand("configure ospf add vlan test100 area 0.0.0.0");
            
            // Assert
            Assert.Contains("OSPF", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureBgp_ShouldConfigureBgp()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure bgp AS-number 65001");
            
            // Assert
            Assert.Contains("BGP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureBgpAddNeighbor_ShouldAddBgpNeighbor()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            device.ProcessCommand("configure bgp AS-number 65001");
            
            // Act
            var output = device.ProcessCommand("configure bgp add neighbor 192.168.1.2 remote-AS 65002");
            
            // Assert
            Assert.Contains("BGP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ShowVlan_ShouldDisplayVlans()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show vlan");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigurePortsDisplayString_ShouldSetDisplayString()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure ports 1 display-string \"Port 1\"");
            
            // Assert
            Assert.Contains("display", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ShowPortsInfo_ShouldDisplayPortInfo()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("show ports 1 info");
            
            // Assert
            Assert.Contains("Port", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureAccount_ShouldConfigureAccount()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure account admin test password secret");
            
            // Assert
            Assert.Contains("account", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureSnmp_ShouldConfigureSnmp()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure snmp add community public read-only");
            
            // Assert
            Assert.Contains("SNMP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureNtp_ShouldConfigureNtp()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure ntp server pool.ntp.org");
            
            // Assert
            Assert.Contains("NTP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_ConfigureMlag_ShouldConfigureMlag()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand("configure mlag peer peer1");
            
            // Assert
            Assert.Contains("MLAG", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_EnableIpforwarding_ShouldEnableIpForwarding()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            device.ProcessCommand("configure vlan test100");
            
            // Act
            var output = device.ProcessCommand("enable ipforwarding vlan test100");
            
            // Assert
            Assert.Contains("IP forwarding", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("show configuration")]
        [InlineData("show iproute")]
        [InlineData("show iparp")]
        [InlineData("show vlan")]
        [InlineData("show ports 1 info")]
        [InlineData("show accounts")]
        [InlineData("show snmp community")]
        [InlineData("show ntp status")]
        [InlineData("show log configuration")]
        [InlineData("show ssh2")]
        [InlineData("show access-list")]
        [InlineData("show mlag peer peer1")]
        [InlineData("show ipforwarding")]
        [InlineData("show ospf neighbor")]
        [InlineData("show bgp neighbor")]
        [InlineData("show rip interface")]
        [InlineData("show isis adjacency")]
        [InlineData("show pim neighbor")]
        [InlineData("show igmp group")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("traceroute 127.0.0.1")]
        public void ExtremeHandler_AllShowCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("configure system name NewName")]
        [InlineData("configure vlan test100")]
        [InlineData("configure vlan test100 ipaddress 192.168.1.1/24")]
        [InlineData("configure vlan test100 add ports 1-5")]
        [InlineData("configure iproute add 10.0.0.0/8 192.168.1.1")]
        [InlineData("enable ospf")]
        [InlineData("configure bgp AS-number 65001")]
        [InlineData("configure ports 1 display-string \"Port 1\"")]
        [InlineData("configure account admin test password secret")]
        [InlineData("configure snmp add community public read-only")]
        [InlineData("configure ntp server pool.ntp.org")]
        [InlineData("enable ssh2")]
        [InlineData("configure access-list test")]
        [InlineData("configure mlag peer peer1")]
        public void ExtremeHandler_ConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("show iparp statistics")]
        [InlineData("show iparp summary")]
        [InlineData("show iparp detail")]
        [InlineData("show iparp cache")]
        [InlineData("show iparp entry")]
        [InlineData("show iparp table")]
        [InlineData("show iparp lookup")]
        [InlineData("show iparp resolve")]
        [InlineData("show iparp reachable")]
        [InlineData("show iparp connected")]
        public void ExtremeHandler_ArpDetailCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("show iproute vrf TestVrf")]
        [InlineData("show iparp vrf TestVrf")]
        [InlineData("show ospf vrf TestVrf")]
        [InlineData("show bgp vrf TestVrf")]
        [InlineData("show rip vrf TestVrf")]
        [InlineData("show isis vrf TestVrf")]
        [InlineData("show pim vrf TestVrf")]
        [InlineData("show igmp vrf TestVrf")]
        public void ExtremeHandler_VrfCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            device.ProcessCommand("configure vrf TestVrf");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Theory]
        [InlineData("configure ospf area 0.0.0.0 stub")]
        [InlineData("configure bgp neighbor 192.168.1.2 maximum-prefix 1000")]
        [InlineData("configure rip split-horizon")]
        [InlineData("configure isis level 1 metric 10")]
        [InlineData("configure pim crp 224.0.1.39")]
        [InlineData("configure igmp snooping vlan test100")]
        [InlineData("configure ports 1 mtu 9000")]
        [InlineData("configure ports 1 speed 10000")]
        [InlineData("configure ports 1 description Test Port")]
        public void ExtremeHandler_AdvancedConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            // Pre-configure dependencies
            device.ProcessCommand("configure vlan test100");
            device.ProcessCommand("enable ospf");
            device.ProcessCommand("configure bgp AS-number 65001");
            device.ProcessCommand("configure bgp add neighbor 192.168.1.2 remote-AS 65002");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public void ExtremeHandler_VxlanConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act & Assert for VXLAN configuration
            var commands = new[]
            {
                "configure vxlan add vni 100 vlan test100",
                "configure vxlan vni 100 remote-ip 192.168.1.2",
                "configure vxlan vni 100 flood-list 192.168.1.3",
                "configure vxlan vni 100 learning enable"
            };

            device.ProcessCommand("configure vlan test100");
            
            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public void ExtremeHandler_PolicyConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act & Assert for policy configuration
            var commands = new[]
            {
                "configure policy profile 1",
                "configure ports 1 qosprofile 1",
                "configure policy access-list test deny",
                "configure policy access-list test permit",
                "configure policy access-list test match ip-source 192.168.1.0/24"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public void ExtremeHandler_SpanningTreeConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act & Assert for spanning tree configuration
            var commands = new[]
            {
                "configure stpd s0 mode mstp",
                "configure stpd s0 priority 4096",
                "configure stpd s0 port 1 cost 200000",
                "configure stpd s0 port 1 priority 128",
                "configure stpd s0 port 1 mode dot1d"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public void ExtremeHandler_DhcpConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            device.ProcessCommand("configure vlan test100");
            
            // Act & Assert for DHCP configuration
            var commands = new[]
            {
                "configure dhcp vlan test100 server 192.168.1.1",
                "configure dhcp vlan test100 lease-timer 86400",
                "configure dhcp vlan test100 gateway 192.168.1.1",
                "configure dhcp vlan test100 exclude 192.168.1.100",
                "configure dhcp vlan test100 dns-server 8.8.8.8",
                "configure dhcp vlan test100 netmask 255.255.255.0"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public void ExtremeHandler_ComplexMlagConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            device.ProcessCommand("configure vlan test100");
            
            // Act & Assert for complex MLAG configuration
            var commands = new[]
            {
                "configure mlag peer peer1",
                "configure mlag peer peer1 port 48 vlan test100",
                "configure mlag peer peer1 lacp-mac aa:bb:cc:dd:ee:ff",
                "configure mlag peer peer1 timeout 30",
                "configure mlag peer peer1 health-check",
                "configure mlag peer peer1 vlan test100",
                "configure mlag peer peer1 checkpoint"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public void ExtremeHandler_AdvancedPortConfiguration_ShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            
            // Act & Assert for advanced port configuration
            var commands = new[]
            {
                "configure ports 1 description \"Test Port\"",
                "configure ports 1 mtu 9000",
                "configure ports 1 speed 10000",
                "configure ports 1 auto-speed",
                "configure ports 1 auto-duplex",
                "configure ports 1 storm-control",
                "configure ports 1 loopback-detection",
                "configure ports 1 jumbo-frame",
                "configure ports 1 mirror",
                "configure ports 1 sharing",
                "configure ports 1 flow-control",
                "configure ports 1 energy-efficient-ethernet",
                "configure ports 1 auto-negotiation",
                "configure ports 1 bandwidth 1000000"
            };

            foreach (var command in commands)
            {
                var output = device.ProcessCommand(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }
    }
}
