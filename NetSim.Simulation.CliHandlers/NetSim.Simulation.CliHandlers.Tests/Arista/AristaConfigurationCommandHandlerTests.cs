using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Arista
{
    public class AristaConfigurationCommandHandlerTests
    {
        [Fact]
        public void Configure_ShouldEnterConfigurationMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");

            // Act
            var result = device.ProcessCommand("configure");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("config", device.GetMode());
            Assert.Contains("TestSwitch(config)#", result);
        }

        [Fact]
        public void Hostname_InConfigMode_ShouldChangeHostname()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("hostname NewSwitch");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewSwitch", device.GetHostname());
        }

        [Fact]
        public void Interface_ShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("interface Ethernet1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("interface", device.GetMode());
            Assert.Equal("Ethernet1", device.GetCurrentInterface());
            Assert.Contains("TestSwitch(config-if-Ethernet1)#", result);
        }

        [Fact]
        public void Vlan_ShouldEnterVlanMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("vlan 100");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("vlan", device.GetMode());
        }

        [Fact]
        public void RouterOspf_ShouldEnterOspfMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("router ospf 1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router", device.GetMode());
            Assert.Equal("ospf", device.GetCurrentRouterProtocol());
            Assert.NotNull(device.GetOspfConfig());
        }

        [Fact]
        public void RouterBgp_ShouldEnterBgpMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("router bgp 65001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router", device.GetMode());
            Assert.Equal("bgp", device.GetCurrentRouterProtocol());
            Assert.NotNull(device.GetBgpConfig());
            Assert.Equal(65001, device.GetBgpConfig().LocalAs);
        }

        [Fact]
        public void RouterRip_ShouldEnterRipMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("router rip");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router", device.GetMode());
            Assert.Equal("rip", device.GetCurrentRouterProtocol());
            Assert.NotNull(device.GetRipConfig());
        }

        [Fact]
        public void IpRoute_ShouldCreateStaticRoute()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("ip route 10.0.0.0 255.255.255.0 192.168.1.1");

            // Assert
            Assert.NotNull(result);
            var routes = device.GetRoutingTable();
            Assert.Contains(routes, r => r.Network == "10.0.0.0" && r.NextHop == "192.168.1.1");
        }

        [Fact]
        public void IpAccessListStandard_ShouldEnterAclMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("ip access-list standard TestACL");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("acl", device.GetMode());
            foreach (var log in device.GetLogEntries())
            {
                System.Diagnostics.Debug.WriteLine(log);
            }
        }

        [Fact]
        public void IpAccessListExtended_ShouldEnterAclMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("ip access-list extended TestACL");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("acl", device.GetMode());
        }

        // Interface configuration tests
        [Fact]
        public void IpAddress_InInterfaceMode_ShouldConfigureInterface()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface Ethernet1");

            // Act
            var result = device.ProcessCommand("ip address 192.168.1.1 255.255.255.0");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal("192.168.1.1", eth1.IpAddress);
            Assert.Equal("255.255.255.0", eth1.SubnetMask);
        }

        [Fact]
        public void Shutdown_InInterfaceMode_ShouldShutdownInterface()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface Ethernet1");

            // Act
            var result = device.ProcessCommand("shutdown");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.True(eth1.IsShutdown);
            Assert.False(eth1.IsUp);
        }

        [Fact]
        public void NoShutdown_InInterfaceMode_ShouldEnableInterface()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface Ethernet1");
            device.ProcessCommand("shutdown");

            // Act
            var result = device.ProcessCommand("no shutdown");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.False(eth1.IsShutdown);
            Assert.True(eth1.IsUp);
        }

        [Fact]
        public void Description_InInterfaceMode_ShouldSetDescription()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface Ethernet1");

            // Act
            var result = device.ProcessCommand("description LAN Interface");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal("LAN Interface", eth1.Description);
        }

        [Fact]
        public void SwitchportModeAccess_InInterfaceMode_ShouldSetAccessMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface Ethernet1");

            // Act
            var result = device.ProcessCommand("switchport mode access");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal("access", eth1.SwitchportMode);
        }

        [Fact]
        public void SwitchportAccessVlan_InInterfaceMode_ShouldAssignVlan()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("vlan 100");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface Ethernet1");

            // Act
            var result = device.ProcessCommand("switchport access vlan 100");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal(100, eth1.VlanId);
        }

        [Fact]
        public void SwitchportModeTrunk_InInterfaceMode_ShouldSetTrunkMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface Ethernet1");

            // Act
            var result = device.ProcessCommand("switchport mode trunk");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetInterfaces();
            var eth1 = interfaces.FirstOrDefault(i => i.Name == "Ethernet1");
            Assert.NotNull(eth1);
            Assert.Equal("trunk", eth1.SwitchportMode);
        }

        // VLAN configuration tests
        [Fact]
        public void VlanName_InVlanMode_ShouldSetVlanName()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("vlan 100");

            // Act
            var result = device.ProcessCommand("name TestVLAN");

            // Assert
            Assert.NotNull(result);
        }

        // Router configuration tests
        [Fact]
        public void NetworkOspf_InRouterMode_ShouldAdvertiseNetwork()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("router ospf 1");

            // Act
            var result = device.ProcessCommand("network 10.0.0.0 255.255.255.0 area 0");

            // Assert
            Assert.NotNull(result);
            var ospfConfig = device.GetOspfConfig();
            Assert.NotNull(ospfConfig);
            Assert.True(ospfConfig.NetworkAreas.ContainsKey("10.0.0.0"));
        }

        [Fact]
        public void RouterId_InOspfMode_ShouldSetRouterId()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("router ospf 1");

            // Act
            var result = device.ProcessCommand("router-id 1.1.1.1");

            // Assert
            Assert.NotNull(result);
            var ospfConfig = device.GetOspfConfig();
            Assert.NotNull(ospfConfig);
            Assert.Equal("1.1.1.1", ospfConfig.RouterId);
        }

        [Fact]
        public void BgpNeighborRemoteAs_InBgpMode_ShouldAddNeighbor()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("router bgp 65001");

            // Act
            var result = device.ProcessCommand("neighbor 192.168.1.2 remote-as 65002");

            // Assert
            Assert.NotNull(result);
            var bgpConfig = device.GetBgpConfig();
            Assert.NotNull(bgpConfig);
            Assert.Contains(bgpConfig.Neighbors.Values, n => n.IpAddress == "192.168.1.2" && n.RemoteAs == 65002);
        }

        [Fact]
        public void BgpNeighborDescription_InBgpMode_ShouldSetDescription()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("router bgp 65001");
            device.ProcessCommand("neighbor 192.168.1.2 remote-as 65002");

            // Act
            var result = device.ProcessCommand("neighbor 192.168.1.2 description Peer Router");

            // Assert
            Assert.NotNull(result);
            var bgpConfig = device.GetBgpConfig();
            Assert.NotNull(bgpConfig);
            var neighbor = bgpConfig.Neighbors.Values.FirstOrDefault(n => n.IpAddress == "192.168.1.2");
            Assert.NotNull(neighbor);
            Assert.Equal("Peer Router", neighbor.Description);
        }

        [Fact]
        public void RipVersion_InRipMode_ShouldSetVersion()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("router rip");

            // Act
            var result = device.ProcessCommand("version 2");

            // Assert
            Assert.NotNull(result);
            var ripConfig = device.GetRipConfig();
            Assert.NotNull(ripConfig);
            Assert.Equal(2, ripConfig.Version);
        }

        [Fact]
        public void BgpRouterId_InBgpMode_ShouldSetRouterId()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("router bgp 65001");

            // Act
            var result = device.ProcessCommand("bgp router-id 2.2.2.2");

            // Assert
            Assert.NotNull(result);
            var bgpConfig = device.GetBgpConfig();
            Assert.NotNull(bgpConfig);
            Assert.Equal("2.2.2.2", bgpConfig.RouterId);
        }

        // ACL configuration tests
        [Fact]
        public void Permit_InAclMode_ShouldAddAclEntry()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("ip access-list standard TestACL");

            // Act
            var result = device.ProcessCommand("permit any");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Deny_InAclMode_ShouldAddAclEntry()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("ip access-list standard TestACL");

            // Act
            var result = device.ProcessCommand("deny 192.168.1.0 0.0.0.255");

            // Assert
            Assert.NotNull(result);
        }

        // Mode transition tests
        [Fact]
        public void Exit_FromInterfaceMode_ShouldReturnToConfigMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface Ethernet1");
            Assert.Equal("interface", device.GetMode());

            // Act
            var result = device.ProcessCommand("exit");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("config", device.GetMode());
        }

        [Fact]
        public void End_FromAnyConfigMode_ShouldReturnToPrivilegedMode()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface Ethernet1");
            Assert.Equal("interface", device.GetMode());

            // Act
            var result = device.ProcessCommand("end");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("privileged", device.GetMode());
        }

        [Fact]
        public void InvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new AristaDevice("TestSwitch");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");

            // Act
            var result = device.ProcessCommand("invalid command");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Invalid input", result);
        }
    }
} 
