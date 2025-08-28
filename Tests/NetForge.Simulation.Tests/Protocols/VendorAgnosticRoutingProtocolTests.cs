using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.Protocols
{
    public class VendorAgnosticRoutingProtocolTests
    {
        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_OspfConfiguration_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");

            // Act
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            var result1 = await device.ProcessCommandAsync("router ospf 1");
            var result2 = await device.ProcessCommandAsync("router-id 1.1.1.1");
            var result3 = await device.ProcessCommandAsync("network 192.168.1.0 0.0.0.255 area 0");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.Contains("(config-router)#", result1);
            Assert.Contains("(config-router)#", result2);
            Assert.Contains("(config-router)#", result3);

            // Verify OSPF configuration was applied
            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Equal("1.1.1.1", ospfConfig.RouterId);
            Assert.Contains("192.168.1.0 0.0.0.255 area 0", ospfConfig.Networks);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_BgpConfiguration_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");

            // Act
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            var result1 = await device.ProcessCommandAsync("router bgp 65001");
            var result2 = await device.ProcessCommandAsync("neighbor 192.168.1.2 remote-as 65002");
            var result3 = await device.ProcessCommandAsync("address-family ipv4 unicast");
            var result4 = await device.ProcessCommandAsync("neighbor 192.168.1.2 activate");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.NotNull(result4);

            // Verify BGP configuration was applied
            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Equal(65001, bgpConfig.LocalAs);
            Assert.Contains("192.168.1.2", bgpConfig.Neighbors.Keys);
            Assert.Equal(65002, bgpConfig.Neighbors["192.168.1.2"].RemoteAs);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_EigrpConfiguration_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");

            // Act
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            var result1 = await device.ProcessCommandAsync("router eigrp 100");
            var result2 = await device.ProcessCommandAsync("network 192.168.1.0 0.0.0.255");
            var result3 = await device.ProcessCommandAsync("no auto-summary");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.Contains("(config-router)#", result1);

            // Verify EIGRP configuration was applied
            var eigrpConfig = device.GetEigrpConfiguration();
            Assert.NotNull(eigrpConfig);
            Assert.Equal(100, eigrpConfig.AsNumber);
            Assert.Contains("192.168.1.0 0.0.0.255", eigrpConfig.Networks);
            Assert.False(eigrpConfig.AutoSummary);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_RipConfiguration_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");

            // Act
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            var result1 = await device.ProcessCommandAsync("router rip");
            var result2 = await device.ProcessCommandAsync("version 2");
            var result3 = await device.ProcessCommandAsync("network 192.168.1.0");
            var result4 = await device.ProcessCommandAsync("no auto-summary");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.NotNull(result4);
            Assert.Contains("(config-router)#", result1);

            // Verify RIP configuration was applied
            var ripConfig = device.GetRipConfiguration();
            Assert.NotNull(ripConfig);
            Assert.Equal(2, ripConfig.Version);
            Assert.Contains("192.168.1.0", ripConfig.Networks);
            Assert.False(ripConfig.AutoSummary);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_ShowRoutingProtocols_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            await device.ProcessCommandAsync("enable");

            // Act
            var ospfResult = await device.ProcessCommandAsync("show ip ospf");
            var bgpResult = await device.ProcessCommandAsync("show ip bgp summary");
            var eigrpResult = await device.ProcessCommandAsync("show ip eigrp neighbors");
            var ripResult = await device.ProcessCommandAsync("show ip rip database");

            // Assert
            Assert.NotNull(ospfResult);
            Assert.NotNull(bgpResult);
            Assert.NotNull(eigrpResult);
            Assert.NotNull(ripResult);

            Assert.Contains("OSPF Process", ospfResult);
            Assert.Contains("BGP router identifier", bgpResult);
            Assert.Contains("EIGRP-IPv4 Neighbors", eigrpResult);
            Assert.Contains("RIP database", ripResult);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_RouteRedistribution_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router ospf 1");

            // Act
            var result1 = await device.ProcessCommandAsync("redistribute bgp 65001 subnets");
            var result2 = await device.ProcessCommandAsync("redistribute eigrp 100 subnets");
            var result3 = await device.ProcessCommandAsync("redistribute rip subnets");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.Contains("(config-router)#", result1);

            // Verify redistribution was configured
            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Contains("redistribute bgp 65001 subnets", ospfConfig.Redistribution);
            Assert.Contains("redistribute eigrp 100 subnets", ospfConfig.Redistribution);
            Assert.Contains("redistribute rip subnets", ospfConfig.Redistribution);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_RouteMaps_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");

            // Act
            var result1 = await device.ProcessCommandAsync("route-map RM-BGP permit 10");
            var result2 = await device.ProcessCommandAsync("match ip address prefix-list PL-BGP");
            var result3 = await device.ProcessCommandAsync("set metric 100");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.Contains("(config-route-map)#", result1);

            // Verify route map was configured
            var routeMaps = device.GetRouteMaps();
            Assert.NotNull(routeMaps);
            Assert.Contains("RM-BGP", routeMaps.Keys);
            Assert.Contains("match ip address prefix-list PL-BGP", routeMaps["RM-BGP"].Statements[0]);
            Assert.Contains("set metric 100", routeMaps["RM-BGP"].Statements[1]);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_PrefixLists_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");

            // Act
            var result1 = await device.ProcessCommandAsync("ip prefix-list PL-BGP seq 10 permit 192.168.0.0/16 le 24");
            var result2 = await device.ProcessCommandAsync("ip prefix-list PL-BGP seq 20 deny 0.0.0.0/0");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Contains("(config)#", result1);

            // Verify prefix list was configured
            var prefixLists = device.GetPrefixLists();
            Assert.NotNull(prefixLists);
            Assert.Contains("PL-BGP", prefixLists.Keys);
            Assert.Contains("permit 192.168.0.0/16 le 24", prefixLists["PL-BGP"].Entries[0]);
            Assert.Contains("deny 0.0.0.0/0", prefixLists["PL-BGP"].Entries[1]);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_BgpRoutePolicies_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router bgp 65001");

            // Act
            var result1 = await device.ProcessCommandAsync("neighbor 192.168.1.2 route-map RM-BGP in");
            var result2 = await device.ProcessCommandAsync("neighbor 192.168.1.2 route-map RM-BGP out");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Contains("(config-router)#", result1);

            // Verify route policy was configured
            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Contains("192.168.1.2", bgpConfig.Neighbors.Keys);
            Assert.Equal("RM-BGP", bgpConfig.Neighbors["192.168.1.2"].RouteMapIn);
            Assert.Equal("RM-BGP", bgpConfig.Neighbors["192.168.1.2"].RouteMapOut);
        }

        [Fact]
        public async System.Threading.Tasks.Task CiscoDevice_BgpCommunities_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router bgp 65001");

            // Act
            var result1 = await device.ProcessCommandAsync("neighbor 192.168.1.2 send-community");
            var result2 = await device.ProcessCommandAsync("neighbor 192.168.1.2 send-community extended");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Contains("(config-router)#", result1);

            // Verify BGP community configuration
            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Contains("192.168.1.2", bgpConfig.Neighbors.Keys);
            Assert.True(bgpConfig.Neighbors["192.168.1.2"].SendCommunity);
            Assert.True(bgpConfig.Neighbors["192.168.1.2"].SendCommunityExtended);
        }

        [Fact]
        public async System.Threading.Tasks.Task RoutingProtocols_ShouldWorkAcrossMultipleVendors()
        {
            // Arrange
            var ciscoDevice = new CiscoDevice("CiscoRouter");
            var juniperDevice = new JuniperDevice("JuniperRouter");

            // Act - Configure OSPF on both devices
            await ciscoDevice.ProcessCommandAsync("enable");
            await ciscoDevice.ProcessCommandAsync("configure terminal");
            var ciscoOspfResult = await ciscoDevice.ProcessCommandAsync("router ospf 1");

            await juniperDevice.ProcessCommandAsync("configure");
            var juniperOspfResult = await juniperDevice.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/0");

            // Assert
            Assert.NotNull(ciscoOspfResult);
            Assert.NotNull(juniperOspfResult);

            // Both devices should support OSPF configuration regardless of vendor
            Assert.Contains("(config-router)#", ciscoOspfResult);
            // Juniper uses different syntax, but the command should be processed
        }

        [Fact]
        public async System.Threading.Tasks.Task RoutingProtocol_ErrorHandling_ShouldBeConsistent()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            await device.ProcessCommandAsync("enable");

            // Act - Try routing commands without being in config mode
            var ospfResult = await device.ProcessCommandAsync("router ospf 1");
            var bgpResult = await device.ProcessCommandAsync("router bgp 65001");

            // Assert
            Assert.NotNull(ospfResult);
            Assert.NotNull(bgpResult);

            // Should return appropriate error messages
            Assert.Contains("Invalid", ospfResult);
            Assert.Contains("Invalid", bgpResult);
        }
    }
}
