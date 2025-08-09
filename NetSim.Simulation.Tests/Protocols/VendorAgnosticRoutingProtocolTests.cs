using NetSim.Simulation.Devices;
using NetSim.Simulation.Common;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    public class VendorAgnosticRoutingProtocolTests
    {
        [Fact]
        public void CiscoDevice_OspfConfiguration_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");

            // Act
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            var result1 = device.ProcessCommand("router ospf 1");
            var result2 = device.ProcessCommand("router-id 1.1.1.1");
            var result3 = device.ProcessCommand("network 192.168.1.0 0.0.0.255 area 0");

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
        public void CiscoDevice_BgpConfiguration_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");

            // Act
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            var result1 = device.ProcessCommand("router bgp 65001");
            var result2 = device.ProcessCommand("neighbor 192.168.1.2 remote-as 65002");
            var result3 = device.ProcessCommand("address-family ipv4 unicast");
            var result4 = device.ProcessCommand("neighbor 192.168.1.2 activate");

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
        public void CiscoDevice_EigrpConfiguration_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");

            // Act
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            var result1 = device.ProcessCommand("router eigrp 100");
            var result2 = device.ProcessCommand("network 192.168.1.0 0.0.0.255");
            var result3 = device.ProcessCommand("no auto-summary");

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
        public void CiscoDevice_RipConfiguration_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");

            // Act
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            var result1 = device.ProcessCommand("router rip");
            var result2 = device.ProcessCommand("version 2");
            var result3 = device.ProcessCommand("network 192.168.1.0");
            var result4 = device.ProcessCommand("no auto-summary");

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
        public void CiscoDevice_ShowRoutingProtocols_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            device.ProcessCommand("enable");

            // Act
            var ospfResult = device.ProcessCommand("show ip ospf");
            var bgpResult = device.ProcessCommand("show ip bgp summary");
            var eigrpResult = device.ProcessCommand("show ip eigrp neighbors");
            var ripResult = device.ProcessCommand("show ip rip database");

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
        public void CiscoDevice_RouteRedistribution_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("router ospf 1");

            // Act
            var result1 = device.ProcessCommand("redistribute bgp 65001 subnets");
            var result2 = device.ProcessCommand("redistribute eigrp 100 subnets");
            var result3 = device.ProcessCommand("redistribute rip subnets");

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
        public void CiscoDevice_RouteMaps_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");

            // Act
            var result1 = device.ProcessCommand("route-map RM-BGP permit 10");
            var result2 = device.ProcessCommand("match ip address prefix-list PL-BGP");
            var result3 = device.ProcessCommand("set metric 100");

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
        public void CiscoDevice_PrefixLists_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");

            // Act
            var result1 = device.ProcessCommand("ip prefix-list PL-BGP seq 10 permit 192.168.0.0/16 le 24");
            var result2 = device.ProcessCommand("ip prefix-list PL-BGP seq 20 deny 0.0.0.0/0");

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
        public void CiscoDevice_BgpRoutePolicies_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("router bgp 65001");

            // Act
            var result1 = device.ProcessCommand("neighbor 192.168.1.2 route-map RM-BGP in");
            var result2 = device.ProcessCommand("neighbor 192.168.1.2 route-map RM-BGP out");

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
        public void CiscoDevice_BgpCommunities_ShouldUseVendorAgnosticHandlers()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("router bgp 65001");

            // Act
            var result1 = device.ProcessCommand("neighbor 192.168.1.2 send-community");
            var result2 = device.ProcessCommand("neighbor 192.168.1.2 send-community extended");

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
        public void RoutingProtocols_ShouldWorkAcrossMultipleVendors()
        {
            // Arrange
            var ciscoDevice = new CiscoDevice("CiscoRouter");
            var juniperDevice = new JuniperDevice("JuniperRouter");

            // Act - Configure OSPF on both devices
            ciscoDevice.ProcessCommand("enable");
            ciscoDevice.ProcessCommand("configure terminal");
            var ciscoOspfResult = ciscoDevice.ProcessCommand("router ospf 1");

            juniperDevice.ProcessCommand("configure");
            var juniperOspfResult = juniperDevice.ProcessCommand("set protocols ospf area 0.0.0.0 interface ge-0/0/0");

            // Assert
            Assert.NotNull(ciscoOspfResult);
            Assert.NotNull(juniperOspfResult);

            // Both devices should support OSPF configuration regardless of vendor
            Assert.Contains("(config-router)#", ciscoOspfResult);
            // Juniper uses different syntax, but the command should be processed
        }

        [Fact]
        public void RoutingProtocol_ErrorHandling_ShouldBeConsistent()
        {
            // Arrange
            var device = new CiscoDevice("CiscoRouter");
            device.ProcessCommand("enable");

            // Act - Try routing commands without being in config mode
            var ospfResult = device.ProcessCommand("router ospf 1");
            var bgpResult = device.ProcessCommand("router bgp 65001");

            // Assert
            Assert.NotNull(ospfResult);
            Assert.NotNull(bgpResult);

            // Should return appropriate error messages
            Assert.Contains("Invalid", ospfResult);
            Assert.Contains("Invalid", bgpResult);
        }
    }
} 