using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Cisco
{
    public class CiscoCommandHandlerTests
    {
        [Fact]
        public async Task EnableHandler_ShouldEnterPrivilegedMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("enable");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public async Task EnableHandler_WithAlias_ShouldWork()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("en");
            
            // Assert
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output);
        }

        [Fact]
        public async Task EnableHandler_WhenAlreadyPrivileged_ShouldReturnSuccess()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable"); // Enter privileged mode first
            
            // Act
            var output = await device.ProcessCommandAsync("enable");
            
            // Assert
            Assert.Contains("Already in privileged mode", output);
            Assert.Equal("privileged", device.GetCurrentMode());
        }

        [Fact]
        public async Task DisableHandler_ShouldExitPrivilegedMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable"); // Enter privileged mode first
            
            // Act
            var output = await device.ProcessCommandAsync("disable");
            
            // Assert
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("TestRouter>", device.GetPrompt());
            Assert.Equal("TestRouter>", output);
        }

        [Fact]
        public async Task DisableHandler_WhenNotPrivileged_ShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("disable");
            
            // Assert
            Assert.Contains("Not in privileged mode", output);
            Assert.Equal("user", device.GetCurrentMode());
        }

        [Fact]
        public async Task ClearCdpHandler_ShouldClearCdpInfo()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("clear cdp");
            
            // Assert
            Assert.Contains("CDP information cleared", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task ClearCdpTableHandler_ShouldClearCdpTable()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("clear cdp table");
            
            // Assert
            Assert.Contains("CDP table cleared", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task ClearCdpCountersHandler_ShouldClearCdpCounters()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("clear cdp counters");
            
            // Assert
            Assert.Contains("CDP counters cleared", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task ClearCdpHandler_WhenNotPrivileged_ShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("clear cdp");
            
            // Assert
            Assert.Contains("Invalid mode", output);
            Assert.Equal("user", device.GetCurrentMode());
        }

        [Fact]
        public async Task ClearCdpHandler_WithInvalidSubcommand_ShouldReturnError()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output = await device.ProcessCommandAsync("clear cdp invalid");
            
            // Assert
            Assert.Contains("CDP information cleared", output);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigureOspf()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("router ospf 1");
            var output2 = await device.ProcessCommandAsync("router-id 1.1.1.1");
            var output3 = await device.ProcessCommandAsync("network 192.168.1.0 0.0.0.255 area 0");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            Assert.Equal("TestRouter(config-router)#", output3);
            
            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Equal("1.1.1.1", ospfConfig.RouterId);
            Assert.Contains("192.168.1.0 0.0.0.255 area 0", ospfConfig.Networks);
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigureBgp()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("router bgp 65001");
            var output2 = await device.ProcessCommandAsync("neighbor 192.168.1.2 remote-as 65002");
            var output3 = await device.ProcessCommandAsync("address-family ipv4 unicast");
            var output4 = await device.ProcessCommandAsync("neighbor 192.168.1.2 activate");
            
            // Assert
            Assert.Equal("TestRouter(config-router-af)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            Assert.Equal("TestRouter(config-router-af)#", output3);
            Assert.Equal("TestRouter(config-router-af)#", output4);
            
            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Equal(65001, bgpConfig.LocalAs);
            Assert.Contains("192.168.1.2", bgpConfig.Neighbors.Keys);
            Assert.Equal(65002, bgpConfig.Neighbors["192.168.1.2"].RemoteAs);
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigureEigrp()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("router eigrp 100");
            var output2 = await device.ProcessCommandAsync("network 192.168.1.0 0.0.0.255");
            var output3 = await device.ProcessCommandAsync("no auto-summary");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            Assert.Equal("TestRouter(config-router)#", output3);
            
            var eigrpConfig = device.GetEigrpConfiguration();
            Assert.NotNull(eigrpConfig);
            Assert.Equal(100, eigrpConfig.AsNumber);
            Assert.Contains("192.168.1.0 0.0.0.255", eigrpConfig.Networks);
            Assert.False(eigrpConfig.AutoSummary);
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigureRip()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("router rip");
            var output2 = await device.ProcessCommandAsync("version 2");
            var output3 = await device.ProcessCommandAsync("network 192.168.1.0");
            var output4 = await device.ProcessCommandAsync("no auto-summary");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            Assert.Equal("TestRouter(config-router)#", output3);
            Assert.Equal("TestRouter(config-router)#", output4);
            
            var ripConfig = device.GetRipConfiguration();
            Assert.NotNull(ripConfig);
            Assert.Equal(2, ripConfig.Version);
            Assert.Contains("192.168.1.0", ripConfig.Networks);
            Assert.False(ripConfig.AutoSummary);
        }

        [Fact]
        public async Task CiscoRouter_ShouldShowRoutingProtocols()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            
            // Act
            var output1 = await device.ProcessCommandAsync("show ip ospf");
            var output2 = await device.ProcessCommandAsync("show ip bgp summary");
            var output3 = await device.ProcessCommandAsync("show ip eigrp neighbors");
            var output4 = await device.ProcessCommandAsync("show ip rip database");
            
            // Assert
            Assert.Contains("OSPF Process", output1);
            Assert.Contains("BGP router identifier", output2);
            Assert.Contains("EIGRP-IPv4 Neighbors", output3);
            Assert.Contains("RIP database", output4);
            Assert.Equal("TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigureRouteRedistribution()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router ospf 1");
            
            // Act
            var output1 = await device.ProcessCommandAsync("redistribute bgp 65001 subnets");
            var output2 = await device.ProcessCommandAsync("redistribute eigrp 100 subnets");
            var output3 = await device.ProcessCommandAsync("redistribute rip subnets");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            Assert.Equal("TestRouter(config-router)#", output3);
            
            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Contains("redistribute bgp 65001 subnets", ospfConfig.Redistribution);
            Assert.Contains("redistribute eigrp 100 subnets", ospfConfig.Redistribution);
            Assert.Contains("redistribute rip subnets", ospfConfig.Redistribution);
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigureRouteMaps()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("route-map RM-BGP permit 10");
            var output2 = await device.ProcessCommandAsync("match ip address prefix-list PL-BGP");
            var output3 = await device.ProcessCommandAsync("set metric 100");
            
            // Assert
            Assert.Equal("TestRouter(config-route-map)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-route-map)#", output1);
            Assert.Equal("TestRouter(config-route-map)#", output2);
            Assert.Equal("TestRouter(config-route-map)#", output3);
            
            var routeMaps = device.GetRouteMaps();
            Assert.NotNull(routeMaps);
            Assert.Contains("RM-BGP", routeMaps.Keys);
            Assert.Contains("match ip address prefix-list PL-BGP", routeMaps["RM-BGP"].Statements[0]);
            Assert.Contains("set metric 100", routeMaps["RM-BGP"].Statements[1]);
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigurePrefixLists()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output1 = await device.ProcessCommandAsync("ip prefix-list PL-BGP seq 10 permit 192.168.0.0/16 le 24");
            var output2 = await device.ProcessCommandAsync("ip prefix-list PL-BGP seq 20 deny 0.0.0.0/0");
            
            // Assert
            Assert.Equal("TestRouter(config)#", device.GetPrompt());
            Assert.Equal("TestRouter(config)#", output1);
            Assert.Equal("TestRouter(config)#", output2);
            
            var prefixLists = device.GetPrefixLists();
            Assert.NotNull(prefixLists);
            Assert.Contains("PL-BGP", prefixLists.Keys);
            Assert.Contains("permit 192.168.0.0/16 le 24", prefixLists["PL-BGP"].Entries[0]);
            Assert.Contains("deny 0.0.0.0/0", prefixLists["PL-BGP"].Entries[1]);
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigureRoutePolicies()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router bgp 65001");
            
            // Act
            var output1 = await device.ProcessCommandAsync("neighbor 192.168.1.2 route-map RM-BGP in");
            var output2 = await device.ProcessCommandAsync("neighbor 192.168.1.2 route-map RM-BGP out");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            
            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Contains("192.168.1.2", bgpConfig.Neighbors.Keys);
            Assert.Equal("RM-BGP", bgpConfig.Neighbors["192.168.1.2"].RouteMapIn);
            Assert.Equal("RM-BGP", bgpConfig.Neighbors["192.168.1.2"].RouteMapOut);
        }

        [Fact]
        public async Task CiscoRouter_ShouldConfigureBgpCommunities()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("router bgp 65001");
            
            // Act
            var output1 = await device.ProcessCommandAsync("neighbor 192.168.1.2 send-community");
            var output2 = await device.ProcessCommandAsync("neighbor 192.168.1.2 send-community extended");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", device.GetPrompt());
            Assert.Equal("TestRouter(config-router)#", output1);
            Assert.Equal("TestRouter(config-router)#", output2);
            
            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Contains("192.168.1.2", bgpConfig.Neighbors.Keys);
            Assert.True(bgpConfig.Neighbors["192.168.1.2"].SendCommunity);
            Assert.True(bgpConfig.Neighbors["192.168.1.2"].SendCommunityExtended);
        }
    }
} 
