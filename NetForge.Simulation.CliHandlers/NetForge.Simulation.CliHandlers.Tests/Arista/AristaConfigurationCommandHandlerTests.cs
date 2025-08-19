using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Arista
{
    public class AristaConfigurationCommandHandlerTests
    {
        [Fact]
        public async Task ConfigureShouldEnterConfigurationMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");

            // Act
            var result = await device.ProcessCommandAsync("configure");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("config", device.GetMode());
            Assert.Contains("TestSwitch(config)#", result);
        }

        [Fact]
        public async Task HostnameInConfigModeShouldChangeHostname()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("hostname NewSwitch");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewSwitch", device.GetHostname());
        }

        [Fact]
        public async Task InterfaceShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("interface Ethernet1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("interface", device.GetMode());
            Assert.Equal("Ethernet1", device.GetCurrentInterface());
            Assert.Contains("TestSwitch(config-if-Ethernet1)#", result);
        }

        [Fact]
        public async Task VlanShouldEnterVlanMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("vlan 100");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("vlan", device.GetMode());
        }

        [Fact]
        public async Task RouterOspfShouldEnterOspfMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("router ospf 1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router", device.GetMode());
            Assert.Equal("ospf", device.GetCurrentRouterProtocol());
            Assert.NotNull(device.GetOspfConfig());
        }

        [Fact]
        public async Task RouterBgpShouldEnterBgpMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("router bgp 65001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router", device.GetMode());
            Assert.Equal("bgp", device.GetCurrentRouterProtocol());
            Assert.NotNull(device.GetBgpConfig());
            Assert.Equal(65001, device.GetBgpConfig().LocalAs);
        }

        [Fact]
        public async Task RouterRipShouldEnterRipMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("router rip");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router", device.GetMode());
            Assert.Equal("rip", device.GetCurrentRouterProtocol());
            Assert.NotNull(device.GetRipConfig());
        }

        [Fact]
        public async Task IpRouteShouldCreateStaticRoute()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("ip route 10.0.0.0 255.255.255.0 192.168.1.1");

            // Assert
            Assert.NotNull(result);
            var routes = device.GetRoutingTable();
            Assert.Contains(routes, r => r.Network == "10.0.0.0" && r.NextHop == "192.168.1.1");
        }

        [Fact]
        public async Task IpAccessListStandardShouldEnterAclMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("ip access-list standard TestACL");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("acl", device.GetMode());
            foreach (var log in device.GetLogEntries())
            {
                System.Diagnostics.Debug.WriteLine(log);
            }
        }

        [Fact]
        public async Task IpAccessListExtendedShouldEnterAclMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("ip access-list extended TestACL");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("acl", device.GetMode());
        }

        // Interface configuration tests
        [Fact]
        public async Task IpAddressInInterfaceModeShouldConfigureInterface()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface Ethernet1");

            // Act
            var result = await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal("192.168.1.1", eth1.IpAddress);
            Assert.Equal("255.255.255.0", eth1.SubnetMask);
        }

        [Fact]
        public async Task ShutdownInInterfaceModeShouldShutdownInterface()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface Ethernet1");

            // Act
            var result = await device.ProcessCommandAsync("shutdown");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.True(eth1.IsShutdown);
            Assert.False(eth1.IsUp);
        }

        [Fact]
        public async Task NoShutdownInInterfaceModeShouldEnableInterface()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface Ethernet1");
            await device.ProcessCommandAsync("shutdown");

            // Act
            var result = await device.ProcessCommandAsync("no shutdown");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.False(eth1.IsShutdown);
            Assert.True(eth1.IsUp);
        }

        [Fact]
        public async Task DescriptionInInterfaceModeShouldSetDescription()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface Ethernet1");

            // Act
            var result = await device.ProcessCommandAsync("description LAN Interface");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal("LAN Interface", eth1.Description);
        }

        [Fact]
        public async Task SwitchportModeAccessInInterfaceModeShouldSetAccessMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface Ethernet1");

            // Act
            var result = await device.ProcessCommandAsync("switchport mode access");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal("access", eth1.SwitchportMode);
        }

        [Fact]
        public async Task SwitchportAccessVlanInInterfaceModeShouldAssignVlan()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("vlan 100");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface Ethernet1");

            // Act
            var result = await device.ProcessCommandAsync("switchport access vlan 100");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal(100, eth1.VlanId);
        }

        [Fact]
        public async Task SwitchportModeTrunkInInterfaceModeShouldSetTrunkMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface Ethernet1");

            // Act
            var result = await device.ProcessCommandAsync("switchport mode trunk");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal("trunk", eth1.SwitchportMode);
        }

        // VLAN configuration tests
        [Fact]
        public async Task VlanNameInVlanModeShouldSetVlanName()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("vlan 100");

            // Act
            var result = await device.ProcessCommandAsync("name TestVLAN");

            // Assert
            Assert.NotNull(result);
        }

        // Router configuration tests
        [Fact]
        public async Task NetworkOspfInRouterModeShouldAdvertiseNetwork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router ospf 1");

            // Act
            var result = await device.ProcessCommandAsync("network 10.0.0.0 255.255.255.0 area 0");

            // Assert
            Assert.NotNull(result);
            var ospfConfig = device.GetOspfConfig();
            Assert.NotNull(ospfConfig);
            Assert.True(ospfConfig.NetworkAreas.ContainsKey("10.0.0.0"));
        }

        [Fact]
        public async Task RouterIdInOspfModeShouldSetRouterId()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router ospf 1");

            // Act
            var result = await device.ProcessCommandAsync("router-id 1.1.1.1");

            // Assert
            Assert.NotNull(result);
            var ospfConfig = device.GetOspfConfig();
            Assert.NotNull(ospfConfig);
            Assert.Equal("1.1.1.1", ospfConfig.RouterId);
        }

        [Fact]
        public async Task BgpNeighborRemoteAsInBgpModeShouldAddNeighbor()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router bgp 65001");

            // Act
            var result = await device.ProcessCommandAsync("neighbor 192.168.1.2 remote-as 65002");

            // Assert
            Assert.NotNull(result);
            var bgpConfig = device.GetBgpConfig();
            Assert.NotNull(bgpConfig);
            Assert.Contains(bgpConfig.Neighbors.Values, n => n.IpAddress == "192.168.1.2" && n.RemoteAs == 65002);
        }

        [Fact]
        public async Task BgpNeighborDescriptionInBgpModeShouldSetDescription()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router bgp 65001");
            await device.ProcessCommandAsync("neighbor 192.168.1.2 remote-as 65002");

            // Act
            var result = await device.ProcessCommandAsync("neighbor 192.168.1.2 description Peer Router");

            // Assert
            Assert.NotNull(result);
            var bgpConfig = device.GetBgpConfig();
            Assert.NotNull(bgpConfig);
            var neighbor = bgpConfig.Neighbors.Values.FirstOrDefault(n => n.IpAddress == "192.168.1.2");
            Assert.NotNull(neighbor);
            Assert.Equal("Peer Router", neighbor.Description);
        }

        [Fact]
        public async Task RipVersionInRipModeShouldSetVersion()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router rip");

            // Act
            var result = await device.ProcessCommandAsync("version 2");

            // Assert
            Assert.NotNull(result);
            var ripConfig = device.GetRipConfig();
            Assert.NotNull(ripConfig);
            Assert.Equal(2, ripConfig.Version);
        }

        [Fact]
        public async Task BgpRouterIdInBgpModeShouldSetRouterId()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router bgp 65001");

            // Act
            var result = await device.ProcessCommandAsync("bgp router-id 2.2.2.2");

            // Assert
            Assert.NotNull(result);
            var bgpConfig = device.GetBgpConfig();
            Assert.NotNull(bgpConfig);
            Assert.Equal("2.2.2.2", bgpConfig.RouterId);
        }

        // ACL configuration tests
        [Fact]
        public async Task PermitInAclModeShouldAddAclEntry()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("ip access-list standard TestACL");

            // Act
            var result = await device.ProcessCommandAsync("permit any");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DenyInAclModeShouldAddAclEntry()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("ip access-list standard TestACL");

            // Act
            var result = await device.ProcessCommandAsync("deny 192.168.1.0 0.0.0.255");

            // Assert
            Assert.NotNull(result);
        }

        // Mode transition tests
        [Fact]
        public async Task ExitFromInterfaceModeShouldReturnToConfigMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface Ethernet1");
            Assert.Equal("interface", device.GetMode());

            // Act
            var result = await device.ProcessCommandAsync("exit");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("config", device.GetMode());
        }

        [Fact]
        public async Task EndFromAnyConfigModeShouldReturnToPrivilegedMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface Ethernet1");
            Assert.Equal("interface", device.GetMode());

            // Act
            var result = await device.ProcessCommandAsync("end");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("privileged", device.GetMode());
        }

        [Fact]
        public async Task InvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");

            // Act
            var result = await device.ProcessCommandAsync("invalid command");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Invalid input", result);
        }
    }
} 
