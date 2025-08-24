using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Huawei
{
    public class HuaweiCommandHandlerTests
    {
        [Fact]
        public async Task HuaweiHandlerDisplayCommandShouldShowInfo()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("display version");

            // Assert
            Assert.Contains("Huawei Versatile Routing Platform", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandlerSystemViewCommandShouldEnterConfigMode()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("system-view");

            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("[TestRouter]", device.GetPrompt());
            Assert.Equal("[TestRouter]", output);
        }

        [Fact]
        public async Task HuaweiHandlerReturnCommandShouldExitConfigMode()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");

            // Act
            var output = await device.ProcessCommandAsync("return");

            // Assert
            Assert.Equal("operational", device.GetCurrentMode());
            Assert.Equal("<TestRouter>", device.GetPrompt());
            Assert.Equal("<TestRouter>", output);
        }

        [Fact]
        public async Task HuaweiHandlerPingCommandShouldExecutePing()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("ping 8.8.8.8");

            // Assert
            Assert.Contains("ping 8.8.8.8", output);
            Assert.Equal("[TestRouter]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandlerTracertCommandShouldExecuteTraceroute()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("tracert 8.8.8.8");

            // Assert
            Assert.Contains("tracert 8.8.8.8", output);
            Assert.Equal("[TestRouter]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandlerInterfaceCommandShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");

            // Act
            var output = await device.ProcessCommandAsync("interface GigabitEthernet0/0/0");

            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("[TestRouter-GigabitEthernet0/0/0]", device.GetPrompt());
            Assert.Equal("[TestRouter-GigabitEthernet0/0/0]", output);
        }

        [Fact]
        public async Task HuaweiHandlerQuitCommandShouldExitCurrentMode()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/0");

            // Act
            var output = await device.ProcessCommandAsync("quit");

            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("[TestRouter]", device.GetPrompt());
            Assert.Equal("[TestRouter]", output);
        }

        [Fact]
        public async Task HuaweiHandlerSaveCommandShouldSaveConfiguration()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("sysname TestRouter2");

            // Act
            var output = await device.ProcessCommandAsync("save");

            // Assert
            Assert.Contains("Configuration saved", output);
            Assert.Equal("[TestRouter2]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandlerDisplayCurrentConfigCommandShouldShowConfig()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("sysname TestRouter2");

            // Act
            var output = await device.ProcessCommandAsync("display current-configuration");

            // Assert
            Assert.Contains("sysname TestRouter2", output);
            Assert.Equal("[TestRouter2]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandlerUndoCommandShouldUndoConfiguration()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("sysname TestRouter2");

            // Act
            var output = await device.ProcessCommandAsync("undo sysname");

            // Assert
            Assert.Contains("Configuration undone", output);
            Assert.Equal("[TestRouter]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandlerWithInvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("invalid command");

            // Assert
            Assert.Contains("Invalid command", output);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiHandlerWithIncompleteCommandShouldReturnError()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("system-view ip");

            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Equal("[TestRouter]", device.GetPrompt());
        }

        [Fact]
        public async Task HuaweiRouterShouldConfigureOspf()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");

            // Act
            var output1 = await device.ProcessCommandAsync("ospf 1");
            var output2 = await device.ProcessCommandAsync("router-id 1.1.1.1");
            var output3 = await device.ProcessCommandAsync("area 0");
            var output4 = await device.ProcessCommandAsync("network 192.168.1.0 0.0.0.255");

            // Assert
            Assert.Equal("[TestRouter-ospf-1-area-0.0.0.0]", device.GetPrompt());
            Assert.Equal("[TestRouter-ospf-1]", output1);
            Assert.Equal("[TestRouter-ospf-1]", output2);
            Assert.Equal("[TestRouter-ospf-1-area-0.0.0.0]", output3);
            Assert.Equal("[TestRouter-ospf-1-area-0.0.0.0]", output4);

            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Equal("1.1.1.1", ospfConfig.RouterId);
            Assert.Contains("192.168.1.0 0.0.0.255", ospfConfig.Networks);
        }

        [Fact]
        public async Task HuaweiRouterShouldConfigureBgp()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");

            // Act
            var output1 = await device.ProcessCommandAsync("bgp 65001");
            var output2 = await device.ProcessCommandAsync("peer 192.168.1.2 as-number 65002");
            var output3 = await device.ProcessCommandAsync("ipv4-family unicast");
            var output4 = await device.ProcessCommandAsync("peer 192.168.1.2 enable");

            // Assert
            Assert.Equal("[TestRouter-bgp-ipv4]", device.GetPrompt());
            Assert.Equal("[TestRouter-bgp]", output1);
            Assert.Equal("[TestRouter-bgp]", output2);
            Assert.Equal("[TestRouter-bgp-ipv4]", output3);
            Assert.Equal("[TestRouter-bgp-ipv4]", output4);

            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Equal(65001, bgpConfig.LocalAs);
            Assert.Contains("192.168.1.2", bgpConfig.Peers.Keys);
            Assert.Equal(65002, bgpConfig.Peers["192.168.1.2"].RemoteAs);
        }



        [Fact]
        public async Task HuaweiRouterShouldConfigureRip()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");

            // Act
            var output1 = await device.ProcessCommandAsync("rip 1");
            var output2 = await device.ProcessCommandAsync("version 2");
            var output3 = await device.ProcessCommandAsync("network 192.168.1.0");
            var output4 = await device.ProcessCommandAsync("undo summary");

            // Assert
            Assert.Equal("[TestRouter-rip-1]", device.GetPrompt());
            Assert.Equal("[TestRouter-rip-1]", output1);
            Assert.Equal("[TestRouter-rip-1]", output2);
            Assert.Equal("[TestRouter-rip-1]", output3);
            Assert.Equal("[TestRouter-rip-1]", output4);

            var ripConfig = device.GetRipConfiguration();
            Assert.NotNull(ripConfig);
            Assert.Equal(2, ripConfig.Version);
            Assert.Contains("192.168.1.0", ripConfig.Networks);
            Assert.False(ripConfig.Summary);
        }

        [Fact]
        public async Task HuaweiRouterShouldConfigureRouteRedistribution()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("ospf 1");

            // Act
            var output1 = await device.ProcessCommandAsync("import-route bgp");
            var output2 = await device.ProcessCommandAsync("import-route isis 1");
            var output3 = await device.ProcessCommandAsync("import-route rip 1");

            // Assert
            Assert.Equal("[TestRouter-ospf-1]", device.GetPrompt());
            Assert.Equal("[TestRouter-ospf-1]", output1);
            Assert.Equal("[TestRouter-ospf-1]", output2);
            Assert.Equal("[TestRouter-ospf-1]", output3);

            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Contains("import-route bgp", ospfConfig.ImportRoutes);
            Assert.Contains("import-route isis 1", ospfConfig.ImportRoutes);
            Assert.Contains("import-route rip 1", ospfConfig.ImportRoutes);
        }

        [Fact]
        public async Task HuaweiRouterShouldConfigureRoutePolicies()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");

            // Act
            var output1 = await device.ProcessCommandAsync("route-policy RM-BGP permit node 10");
            var output2 = await device.ProcessCommandAsync("if-match ip-prefix PL-BGP");
            var output3 = await device.ProcessCommandAsync("apply cost 100");

            // Assert
            Assert.Equal("[TestRouter-route-policy-RM-BGP-10]", device.GetPrompt());
            Assert.Equal("[TestRouter-route-policy-RM-BGP-10]", output1);
            Assert.Equal("[TestRouter-route-policy-RM-BGP-10]", output2);
            Assert.Equal("[TestRouter-route-policy-RM-BGP-10]", output3);

            var routePolicies = device.GetRoutePolicies();
            Assert.NotNull(routePolicies);
            Assert.Contains("RM-BGP", routePolicies.Keys);
            Assert.Contains("if-match ip-prefix PL-BGP", routePolicies["RM-BGP"].Nodes["10"].ToString());
            Assert.Contains("apply cost 100", routePolicies["RM-BGP"].Nodes["10"].ToString());
        }

        [Fact]
        public async Task HuaweiRouterShouldConfigureIpPrefixLists()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");

            // Act
            var output1 = await device.ProcessCommandAsync("ip ip-prefix PL-BGP index 10 permit 192.168.0.0 16 greater-equal 16 less-equal 24");
            var output2 = await device.ProcessCommandAsync("ip ip-prefix PL-BGP index 20 deny 0.0.0.0 0");

            // Assert
            Assert.Equal("[TestRouter]", device.GetPrompt());
            Assert.Equal("[TestRouter]", output1);
            Assert.Equal("[TestRouter]", output2);

            var prefixLists = device.GetIpPrefixLists();
            Assert.NotNull(prefixLists);
            Assert.Contains("PL-BGP", prefixLists.Keys);
            Assert.Contains("permit 192.168.0.0 16 greater-equal 16 less-equal 24", prefixLists["PL-BGP"].Entries[0]);
            Assert.Contains("deny 0.0.0.0 0", prefixLists["PL-BGP"].Entries[1]);
        }

        [Fact]
        public async Task HuaweiRouterShouldConfigureBgpCommunities()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("bgp 65001");

            // Act
            var output1 = await device.ProcessCommandAsync("peer 192.168.1.2 advertise-community");
            var output2 = await device.ProcessCommandAsync("peer 192.168.1.2 advertise-ext-community");

            // Assert
            Assert.Equal("[TestRouter-bgp]", device.GetPrompt());
            Assert.Equal("[TestRouter-bgp]", output1);
            Assert.Equal("[TestRouter-bgp]", output2);

            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Contains("192.168.1.2", bgpConfig.Peers.Keys);
            Assert.True(bgpConfig.Peers["192.168.1.2"].AdvertiseCommunity);
            Assert.True(bgpConfig.Peers["192.168.1.2"].AdvertiseExtCommunity);
        }

        [Fact]
        public async Task HuaweiRouterShouldShowRoutingProtocols()
        {
            // Arrange
            var device = new HuaweiDevice("TestRouter");

            // Act
            var output1 = await device.ProcessCommandAsync("display ospf peer");
            var output2 = await device.ProcessCommandAsync("display bgp peer");
            var output3 = await device.ProcessCommandAsync("display isis peer");
            var output4 = await device.ProcessCommandAsync("display rip 1 neighbor");

            // Assert
            Assert.Contains("OSPF Process", output1);
            Assert.Contains("BGP local router ID", output2);
            Assert.Contains("ISIS Peer", output3);
            Assert.Contains("RIP neighbor", output4);
            Assert.Equal("<TestRouter>", device.GetPrompt());
        }
    }
}
