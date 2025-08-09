using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Huawei
{
    public class HuaweiCommandHandlerComprehensiveTests
    {
        [Fact]
        public async Task HuaweiHandler_SystemName_ShouldSetSystemName()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act
            var output = await device.ProcessCommandAsync("sysname NewRouter");
            
            // Assert
            Assert.Equal("[NewRouter]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_DisplayCurrentConfiguration_ShouldShowConfig()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("sysname NewRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("display current-configuration");
            
            // Assert
            Assert.Contains("sysname NewRouter", output);
            Assert.Equal("[NewRouter]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_InterfaceLoopback_ShouldEnterLoopbackMode()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act
            var output = await device.ProcessCommandAsync("interface LoopBack0");
            
            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("[TestRouter-LoopBack0]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_IpAddress_ShouldConfigureInterface()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            
            // Act
            var output = await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            
            // Assert
            Assert.Equal("[TestRouter-GigabitEthernet0/0/0]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_UndoShutdown_ShouldEnableInterface()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            
            // Act
            var output = await device.ProcessCommandAsync("undo shutdown");
            
            // Assert
            Assert.Equal("[TestRouter-GigabitEthernet0/0/0]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_Vlan_ShouldEnterVlanMode()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act
            var output = await device.ProcessCommandAsync("vlan 100");
            
            // Assert
            Assert.Equal("vlan", device.GetCurrentMode());
            Assert.Equal("[TestRouter-vlan100]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_VlanDescription_ShouldSetVlanDescription()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("vlan 100");
            
            // Act
            var output = await device.ProcessCommandAsync("description TestVLAN");
            
            // Assert
            Assert.Equal("[TestRouter-vlan100]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_PortGroup_ShouldEnterPortGroupMode()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act
            var output = await device.ProcessCommandAsync("port-group 1");
            
            // Assert
            Assert.Equal("port-group", device.GetCurrentMode());
            Assert.Equal("[TestRouter-port-group-1]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_GroupMember_ShouldAddPortsToGroup()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("port-group 1");
            
            // Act
            var output = await device.ProcessCommandAsync("group-member GigabitEthernet0/0/1 to GigabitEthernet0/0/10");
            
            // Assert
            Assert.Equal("[TestRouter-port-group-1]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_DisplayVlan_ShouldDisplayVlans()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("display vlan");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_DisplayInterface_ShouldDisplayInterfaceInfo()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("display interface GigabitEthernet0/0/0");
            
            // Assert
            Assert.Contains("GigabitEthernet0/0/0", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_DisplayIpInterface_ShouldDisplayIpInfo()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("display ip interface brief");
            
            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_DisplayIpRouting_ShouldDisplayRoutes()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("display ip routing-table");
            
            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_DisplayArp_ShouldDisplayArpTable()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("display arp");
            
            // Assert
            Assert.Contains("ARP", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_DisplayMacAddress_ShouldDisplayMacTable()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("display mac-address");
            
            // Assert
            Assert.Contains("MAC", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandler_DisplayDevice_ShouldDisplayDeviceInfo()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("display device");
            
            // Assert
            Assert.Contains("Device", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Theory]
        [InlineData("display version")]
        [InlineData("display current-configuration")]
        [InlineData("display interface brief")]
        [InlineData("display ip interface brief")]
        [InlineData("display ip routing-table")]
        [InlineData("display arp")]
        [InlineData("display mac-address")]
        [InlineData("display vlan")]
        [InlineData("display device")]
        [InlineData("display ospf peer")]
        [InlineData("display bgp peer")]
        [InlineData("display isis peer")]
        [InlineData("display rip 1 neighbor")]
        [InlineData("display stp brief")]
        [InlineData("display cpu-usage")]
        [InlineData("display memory-usage")]
        [InlineData("display temperature")]
        [InlineData("display power")]
        [InlineData("display fan")]
        [InlineData("display alarm")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("tracert 127.0.0.1")]
        public async Task HuaweiHandler_AllDisplayCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Theory]
        [InlineData("system-view")]
        [InlineData("sysname NewName")]
        [InlineData("interface GigabitEthernet0/0/0")]
        [InlineData("ip address 192.168.1.1 255.255.255.0")]
        [InlineData("undo shutdown")]
        [InlineData("vlan 100")]
        [InlineData("description TestVLAN")]
        [InlineData("port-group 1")]
        [InlineData("ospf 1")]
        [InlineData("bgp 65001")]
        [InlineData("isis 1")]
        [InlineData("rip 1")]
        [InlineData("stp enable")]
        [InlineData("dhcp enable")]
        public async Task HuaweiHandler_ConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            if (!command.StartsWith("system-view"))
            {
                await device.ProcessCommandAsync("system-view");
            }
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("display interface GigabitEthernet0/0/0")]
        [InlineData("display ip routing-table protocol ospf")]
        [InlineData("display ip routing-table protocol bgp")]
        [InlineData("display arp dynamic")]
        [InlineData("display arp static")]
        [InlineData("display mac-address dynamic")]
        [InlineData("display mac-address static")]
        [InlineData("display vlan brief")]
        [InlineData("display stp instance 0")]
        [InlineData("display ospf lsdb")]
        [InlineData("display bgp routing-table")]
        [InlineData("display isis route")]
        [InlineData("display rip 1 route")]
        [InlineData("display dhcp server ip-in-use")]
        [InlineData("display snmp-agent community")]
        public async Task HuaweiHandler_DetailedDisplayCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Theory]
        [InlineData("description Test Interface")]
        [InlineData("ip address 192.168.1.1 255.255.255.0")]
        [InlineData("undo shutdown")]
        [InlineData("port link-type trunk")]
        [InlineData("port trunk allow-pass vlan 100 200")]
        [InlineData("port link-type access")]
        [InlineData("port default vlan 100")]
        [InlineData("duplex full")]
        [InlineData("speed 1000")]
        [InlineData("flow-control")]
        public async Task HuaweiHandler_InterfaceConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("[TestRouter-GigabitEthernet0/0/0]", device.GetPrompt());
        }

        [Theory]
        [InlineData("description TestVLAN")]
        [InlineData("name TestVLAN")]
        public async Task HuaweiHandler_VlanConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("vlan 100");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("[TestRouter-vlan100]", device.GetPrompt());
        }

        [Theory]
        [InlineData("router-id 1.1.1.1")]
        [InlineData("area 0")]
        [InlineData("network 192.168.1.0 0.0.0.255")]
        [InlineData("bandwidth-reference 1000")]
        [InlineData("default-route-advertise")]
        public async Task HuaweiHandler_OspfConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("ospf 1");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("peer 192.168.1.2 as-number 65002")]
        [InlineData("router-id 2.2.2.2")]
        [InlineData("ipv4-family unicast")]
        [InlineData("peer 192.168.1.2 enable")]
        [InlineData("peer 192.168.1.2 next-hop-local")]
        [InlineData("network 192.168.1.0 255.255.255.0")]
        public async Task HuaweiHandler_BgpConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("bgp 65001");
            if (command.StartsWith("peer 192.168.1.2 enable") || command.StartsWith("peer 192.168.1.2 next-hop") || command.StartsWith("network"))
            {
                await device.ProcessCommandAsync("ipv4-family unicast");
            }
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Fact]
        public async Task HuaweiHandler_ComplexOspfConfiguration_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act & Assert for complex OSPF configuration
            var commands = new[]
            {
                "ospf 1",
                "router-id 1.1.1.1",
                "area 0",
                "network 192.168.1.0 0.0.0.255",
                "network 10.0.0.0 0.255.255.255",
                "quit",
                "area 1",
                "network 172.16.0.0 0.0.255.255",
                "stub",
                "quit",
                "import-route bgp",
                "import-route isis 1",
                "import-route rip 1"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task HuaweiHandler_ComplexBgpConfiguration_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act & Assert for complex BGP configuration
            var commands = new[]
            {
                "bgp 65001",
                "router-id 2.2.2.2",
                "peer 192.168.1.2 as-number 65002",
                "peer 192.168.1.3 as-number 65001",
                "ipv4-family unicast",
                "peer 192.168.1.2 enable",
                "peer 192.168.1.3 enable",
                "peer 192.168.1.2 next-hop-local",
                "network 192.168.1.0 255.255.255.0",
                "network 10.0.0.0 255.0.0.0"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task HuaweiHandler_VlanAndPortConfiguration_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act & Assert for VLAN and port configuration
            var commands = new[]
            {
                "vlan batch 100 200 300",
                "vlan 100",
                "description Production",
                "quit",
                "vlan 200", 
                "description Development",
                "quit",
                "interface GigabitEthernet0/0/1",
                "port link-type trunk",
                "port trunk allow-pass vlan 100 200",
                "quit",
                "interface GigabitEthernet0/0/2",
                "port link-type access",
                "port default vlan 100"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task HuaweiHandler_RoutePolicyConfiguration_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act & Assert for route policy configuration
            var commands = new[]
            {
                "route-policy RP-BGP permit node 10",
                "if-match ip-prefix PL-NETWORKS",
                "apply cost 100",
                "apply community 65001:100",
                "quit",
                "route-policy RP-OSPF permit node 10",
                "if-match interface GigabitEthernet0/0/0",
                "apply tag 100",
                "quit",
                "ip ip-prefix PL-NETWORKS index 10 permit 192.168.0.0 16 greater-equal 16 less-equal 24"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task HuaweiHandler_SnmpConfiguration_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act & Assert for SNMP configuration
            var commands = new[]
            {
                "snmp-agent",
                "snmp-agent community read public",
                "snmp-agent community write private",
                "snmp-agent sys-info contact \"Network Admin\"",
                "snmp-agent sys-info location \"Data Center\"",
                "snmp-agent target-host trap address udp-domain 192.168.1.100 params securityname public",
                "snmp-agent trap enable"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
                Assert.Equal("[TestRouter]", device.GetPrompt());
            }
        }

        [Fact]
        public async Task HuaweiHandler_StpConfiguration_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act & Assert for STP configuration
            var commands = new[]
            {
                "stp enable",
                "stp mode rstp",
                "stp priority 4096",
                "stp root primary",
                "interface GigabitEthernet0/0/1",
                "stp port priority 128",
                "stp port cost 200000",
                "stp edged-port enable"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }

        [Fact]
        public async Task HuaweiHandler_DhcpConfiguration_ShouldWork()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            
            // Act & Assert for DHCP configuration
            var commands = new[]
            {
                "dhcp enable",
                "ip pool TestPool",
                "gateway-list 192.168.1.1",
                "network 192.168.1.0 mask 255.255.255.0",
                "dns-list 8.8.8.8 8.8.4.4",
                "lease day 1 hour 0 minute 0",
                "quit",
                "interface GigabitEthernet0/0/1",
                "dhcp select global"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("Invalid", output);
            }
        }
    }
}
