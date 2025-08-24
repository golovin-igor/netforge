using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Extreme
{
    public class ExtremeCommandHandlerComprehensiveTests
    {
        [Fact]
        public async Task ExtremeHandlerConfigureSystemNameShouldSetSystemName()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure system name NewSwitch");

            // Assert
            Assert.Equal("NewSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureVlanShouldCreateVlan()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure vlan test100");

            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureVlanIpAddressShouldSetVlanIp()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            await device.ProcessCommandAsync("configure vlan test100");

            // Act
            var output = await device.ProcessCommandAsync("configure vlan test100 ipaddress 192.168.1.1/24");

            // Assert
            Assert.Contains("IP address", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureVlanAddPortsShouldAddPorts()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            await device.ProcessCommandAsync("configure vlan test100");

            // Act
            var output = await device.ProcessCommandAsync("configure vlan test100 add ports 1-5");

            // Assert
            Assert.Contains("ports", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerShowConfigurationShouldDisplayConfig()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("show configuration");

            // Assert
            Assert.Contains("Configuration", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerShowIprouteShouldDisplayRoutes()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("show iproute");

            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerShowIparpShouldDisplayArpTable()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("show iparp");

            // Assert
            Assert.Contains("ARP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureIprouteAddShouldAddRoute()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure iproute add 10.0.0.0/8 192.168.1.1");

            // Assert
            Assert.Contains("route", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerEnableOspfShouldEnableOspf()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("enable ospf");

            // Assert
            Assert.Contains("OSPF", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureOspfAddVlanShouldAddVlanToOspf()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            await device.ProcessCommandAsync("enable ospf");
            await device.ProcessCommandAsync("configure vlan test100");

            // Act
            var output = await device.ProcessCommandAsync("configure ospf add vlan test100 area 0.0.0.0");

            // Assert
            Assert.Contains("OSPF", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureBgpShouldConfigureBgp()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure bgp AS-number 65001");

            // Assert
            Assert.Contains("BGP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureBgpAddNeighborShouldAddBgpNeighbor()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            await device.ProcessCommandAsync("configure bgp AS-number 65001");

            // Act
            var output = await device.ProcessCommandAsync("configure bgp add neighbor 192.168.1.2 remote-AS 65002");

            // Assert
            Assert.Contains("BGP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerShowVlanShouldDisplayVlans()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("show vlan");

            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigurePortsDisplayStringShouldSetDisplayString()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure ports 1 display-string \"Port 1\"");

            // Assert
            Assert.Contains("display", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerShowPortsInfoShouldDisplayPortInfo()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("show ports 1 info");

            // Assert
            Assert.Contains("Port", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureAccountShouldConfigureAccount()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure account admin test password secret");

            // Assert
            Assert.Contains("account", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureSnmpShouldConfigureSnmp()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure snmp add community public read-only");

            // Assert
            Assert.Contains("SNMP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureNtpShouldConfigureNtp()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure ntp server pool.ntp.org");

            // Assert
            Assert.Contains("NTP", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerConfigureMlagShouldConfigureMlag()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync("configure mlag peer peer1");

            // Assert
            Assert.Contains("MLAG", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerEnableIpforwardingShouldEnableIpForwarding()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            await device.ProcessCommandAsync("configure vlan test100");

            // Act
            var output = await device.ProcessCommandAsync("enable ipforwarding vlan test100");

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
        public async Task ExtremeHandlerAllShowCommandsShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync(command);

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
        public async Task ExtremeHandlerConfigurationCommandsShouldWork(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync(command);

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
        public async Task ExtremeHandlerArpDetailCommandsShouldWork(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");

            // Act
            var output = await device.ProcessCommandAsync(command);

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
        public async Task ExtremeHandlerVrfCommandsShouldWork(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            await device.ProcessCommandAsync("configure vrf TestVrf");

            // Act
            var output = await device.ProcessCommandAsync(command);

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
        public async Task ExtremeHandlerAdvancedConfigurationCommandsShouldWork(string command)
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            // Pre-configure dependencies
            await device.ProcessCommandAsync("configure vlan test100");
            await device.ProcessCommandAsync("enable ospf");
            await device.ProcessCommandAsync("configure bgp AS-number 65001");
            await device.ProcessCommandAsync("configure bgp add neighbor 192.168.1.2 remote-AS 65002");

            // Act
            var output = await device.ProcessCommandAsync(command);

            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("TestSwitch#", device.GetPrompt());
        }

        [Fact]
        public async Task ExtremeHandlerVxlanConfigurationShouldWork()
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

            await device.ProcessCommandAsync("configure vlan test100");

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public async Task ExtremeHandlerPolicyConfigurationShouldWork()
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
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public async Task ExtremeHandlerSpanningTreeConfigurationShouldWork()
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
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public async Task ExtremeHandlerDhcpConfigurationShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            await device.ProcessCommandAsync("configure vlan test100");

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
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public async Task ExtremeHandlerComplexMlagConfigurationShouldWork()
        {
            // Arrange
            var device = new ExtremeDevice("TestSwitch");
            await device.ProcessCommandAsync("configure vlan test100");

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
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }

        [Fact]
        public async Task ExtremeHandlerAdvancedPortConfigurationShouldWork()
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
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("TestSwitch#", device.GetPrompt());
            }
        }
    }
}
