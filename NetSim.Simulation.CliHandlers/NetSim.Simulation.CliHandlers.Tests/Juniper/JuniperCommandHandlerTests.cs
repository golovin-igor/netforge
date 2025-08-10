using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Juniper
{
    public class JuniperCommandHandlerTests
    {
        [Fact]
        public async Task JuniperDeviceShouldStartInOperationalMode()
        {
            // Arrange & Act
            var device = new JuniperDevice("TestRouter");
            
            // Assert
            Assert.Equal("TestRouter> ", device.GetPrompt());
            Assert.Equal("operational", device.GetMode());
        }
        
        [Fact]
        public async Task ConfigureHandlerShouldEnterConfigMode()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            await device.ProcessCommandAsync("configure");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("configuration", device.GetCurrentMode());
        }

        [Fact]
        public async Task ConfigureHandlerShouldClearCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("set system host-name TestRouter2");
            await device.ProcessCommandAsync("exit");
            
            // Act
            await device.ProcessCommandAsync("configure");
            var output = await device.ProcessCommandAsync("show | compare");
            
            // Assert
            Assert.Contains("No changes", output);
            Assert.Equal("configuration", device.GetCurrentMode());
        }

        [Fact]
        public async Task ShowHandlerShouldDisplaySystemInfo()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show system");
            
            // Assert
            Assert.Contains("System information", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }

        [Fact]
        public async Task ShowHandlerShouldDisplayInterfaces()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show interfaces");
            
            // Assert
            Assert.Contains("Interface information", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }

        [Fact]
        public async Task ShowHandlerShouldDisplayVersion()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show version");
            
            // Assert
            Assert.Contains("Hostname: TestRouter", output);
            Assert.Contains("Model: vSRX", output);
            Assert.Contains("Junos:", output);
            Assert.Contains("JUNOS Software Release", output);
        }

        [Fact]
        public async Task ShowHandlerShouldDisplayConfiguration()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("set system host-name TestRouter2");
            
            // Act
            var output = await device.ProcessCommandAsync("show configuration");
            
            // Assert
            Assert.Contains("system {", output);
            Assert.Contains("host-name TestRouter2;", output);
            Assert.Equal("configuration", device.GetCurrentMode());
        }

        [Fact]
        public async Task ShowHandlerShouldDisplayCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("set system host-name TestRouter2");
            
            // Act
            var output = await device.ProcessCommandAsync("show | compare");
            
            // Assert
            Assert.Contains("+ host-name TestRouter2;", output);
            Assert.Equal("configuration", device.GetCurrentMode());
        }

        [Fact]
        public async Task ShowHandlerWhenNotInConfigModeShouldReturnError()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show configuration");
            
            // Assert
            Assert.Contains("Invalid mode", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }

        [Fact]
        public async Task ShowHandlerWithInvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show invalid");
            
            // Assert
            Assert.Contains("syntax error", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }

        [Fact]
        public async Task ShowHandlerWithIncompleteCommandShouldReturnError()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show");
            
            // Assert
            Assert.Contains("syntax error", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }
        
        [Fact]
        public async Task SetCommandShouldAddToCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var result = await device.ProcessCommandAsync("set system host-name MyRouter");
            
            // Assert
            Assert.Equal("TestRouter# ", result);
            var candidateConfig = device.GetCandidateConfig();
            Assert.Contains("set system host-name MyRouter", candidateConfig);
        }
        
        [Fact]
        public async Task SetInterfaceCommandShouldConfigureInterface()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var result1 = await device.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            var result2 = await device.ProcessCommandAsync("set interfaces ge-0/0/0 description \"LAN Interface\"");
            
            // Assert
            Assert.Equal("TestRouter# ", result1);
            Assert.Equal("TestRouter# ", result2);
            
            var candidateConfig = device.GetCandidateConfig();
            Assert.Contains("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24", candidateConfig);
            Assert.Contains("set interfaces ge-0/0/0 description \"LAN Interface\"", candidateConfig);
        }
        
        [Fact]
        public async Task SetVlanCommandShouldConfigureVlan()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var result = await device.ProcessCommandAsync("set vlans management vlan-id 100");
            
            // Assert
            Assert.Equal("TestRouter# ", result);
            
            var candidateConfig = device.GetCandidateConfig();
            Assert.Contains("set vlans management vlan-id 100", candidateConfig);
        }
        
        [Fact]
        public async Task CommitCommandShouldApplyConfiguration()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("set system host-name MyRouter");
            await device.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            
            // Act
            var result = await device.ProcessCommandAsync("commit");
            
            // Assert
            Assert.Contains("commit complete", result);
            
            // Verify configuration was applied
            Assert.Equal("MyRouter", device.GetHostname());
            var iface = device.GetInterface("ge-0/0/0");
            Assert.NotNull(iface);
            Assert.Equal("192.168.1.1", iface.IpAddress);
            Assert.Equal("255.255.255.0", iface.SubnetMask);
            
            // Candidate config should be cleared after commit
            var candidateConfig = device.GetCandidateConfig();
            Assert.Empty(candidateConfig);
        }
        
        [Fact]
        public async Task RollbackCommandShouldClearCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("set system host-name MyRouter");
            await device.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            
            // Act
            var result = await device.ProcessCommandAsync("rollback");
            
            // Assert
            Assert.Contains("rollback complete", result);
            
            // Candidate config should be cleared
            var candidateConfig = device.GetCandidateConfig();
            Assert.Empty(candidateConfig);
        }
        
        [Fact]
        public async Task ShowConfigurationShouldDisplayCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("set system host-name MyRouter");
            await device.ProcessCommandAsync("set vlans management vlan-id 100");
            
            // Act
            var result = await device.ProcessCommandAsync("show configuration");
            
            // Assert
            Assert.Contains("set system host-name MyRouter", result);
            Assert.Contains("set vlans management vlan-id 100", result);
            Assert.Contains("## Last changed:", result);
        }
        
        [Fact]
        public async Task ExitCommandShouldLeaveConfigurationMode()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var result = await device.ProcessCommandAsync("exit");
            
            // Assert
            Assert.Contains("Exiting configuration mode", result);
            Assert.Equal("TestRouter> ", device.GetPrompt());
            Assert.Equal("operational", device.GetMode());
        }
        
        [Fact]
        public async Task ShowVersionShouldDisplayJunosVersion()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var result = await device.ProcessCommandAsync("show version");
            
            // Assert
            Assert.Contains("Hostname: TestRouter", result);
            Assert.Contains("Model: vSRX", result);
            Assert.Contains("Junos: 20.4R3.8", result);
            Assert.Contains("JUNOS Software Release", result);
        }
        
        [Fact]
        public async Task ShowInterfacesShouldDisplayInterfaceInfo()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            await device.ProcessCommandAsync("set interfaces ge-0/0/0 description \"Test Interface\"");
            await device.ProcessCommandAsync("commit");
            await device.ProcessCommandAsync("exit");
            
            // Act
            var result = await device.ProcessCommandAsync("show interfaces ge-0/0/0");
            
            // Assert
            Assert.Contains("Physical interface: ge-0/0/0", result);
            Assert.Contains("Description: Test Interface", result);
            Assert.Contains("192.168.1.1/24", result);
        }
        
        [Fact]
        public async Task PingCommandShouldExecutePing()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var result = await device.ProcessCommandAsync("ping 8.8.8.8");
            
            // Assert
            Assert.Contains("ICMP Echos to 8.8.8.8", result);
            Assert.Contains("timeout is 2 seconds", result);
        }
        
        [Fact]
        public async Task CommandHistoryShouldWorkWithJuniper()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            await device.ProcessCommandAsync("show version");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("set system host-name MyRouter");
            var result = await device.ProcessCommandAsync("history");
            
            // Assert
            Assert.Contains("show version", result);
            Assert.Contains("configure", result);
            Assert.Contains("set system host-name MyRouter", result);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigureOspf()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/0.0");
            var output2 = await device.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface lo0.0 passive");
            var output3 = await device.ProcessCommandAsync("set routing-options router-id 1.1.1.1");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            Assert.Equal("TestRouter# ", output3);
            
            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Equal("1.1.1.1", ospfConfig.RouterId);
            Assert.Contains("ge-0/0/0.0", ospfConfig.Interfaces);
            Assert.Contains("lo0.0", ospfConfig.PassiveInterfaces);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigureBgp()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set routing-options autonomous-system 65001");
            var output2 = await device.ProcessCommandAsync("set protocols bgp group external-peers type external");
            var output3 = await device.ProcessCommandAsync("set protocols bgp group external-peers peer-as 65002");
            var output4 = await device.ProcessCommandAsync("set protocols bgp group external-peers neighbor 192.168.1.2");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            Assert.Equal("TestRouter# ", output3);
            Assert.Equal("TestRouter# ", output4);
            
            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Equal(65001, bgpConfig.LocalAs);
            Assert.Contains("external-peers", bgpConfig.Groups.Keys);
            Assert.Equal("external", bgpConfig.Groups["external-peers"].Type);
            Assert.Contains("192.168.1.2", bgpConfig.Groups["external-peers"].Neighbors.Keys);
            Assert.Equal(65002, bgpConfig.Groups["external-peers"].PeerAs);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigureIsIs()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set protocols isis interface ge-0/0/0.0");
            var output2 = await device.ProcessCommandAsync("set protocols isis interface lo0.0 passive");
            var output3 = await device.ProcessCommandAsync("set protocols isis level 1 disable");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            Assert.Equal("TestRouter# ", output3);
            
            var isisConfig = device.GetIsisConfiguration();
            Assert.NotNull(isisConfig);
            Assert.Contains("ge-0/0/0.0", isisConfig.Interfaces);
            Assert.Contains("lo0.0", isisConfig.PassiveInterfaces);
            Assert.False(isisConfig.Level1Enabled);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigureRip()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set protocols rip group rip-group neighbor ge-0/0/0.0");
            var output2 = await device.ProcessCommandAsync("set protocols rip group rip-group export rip-export");
            var output3 = await device.ProcessCommandAsync("set protocols rip authentication-type md5");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            Assert.Equal("TestRouter# ", output3);
            
            var ripConfig = device.GetRipConfiguration();
            Assert.NotNull(ripConfig);
            Assert.Contains("ge-0/0/0.0", ripConfig.Groups["rip-group"].Neighbors);
            Assert.Contains("rip-export", ripConfig.Groups["rip-group"].ExportPolicies);
            Assert.Equal("md5", ripConfig.AuthenticationType);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigureRouteRedistribution()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set protocols ospf export bgp-to-ospf");
            var output2 = await device.ProcessCommandAsync("set policy-options policy-statement bgp-to-ospf from protocol bgp");
            var output3 = await device.ProcessCommandAsync("set policy-options policy-statement bgp-to-ospf then accept");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            Assert.Equal("TestRouter# ", output3);
            
            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Contains("bgp-to-ospf", ospfConfig.ExportPolicies);
            
            var policies = device.GetRoutingPolicies();
            Assert.NotNull(policies);
            Assert.Contains("bgp-to-ospf", policies.Keys);
            Assert.Contains("from protocol bgp", policies["bgp-to-ospf"].Terms[0]);
            Assert.Contains("then accept", policies["bgp-to-ospf"].Terms[0]);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigureRoutingPolicies()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set policy-options policy-statement filter-bgp from route-filter 192.168.0.0/16 orlonger");
            var output2 = await device.ProcessCommandAsync("set policy-options policy-statement filter-bgp then community add bgp-community");
            var output3 = await device.ProcessCommandAsync("set policy-options policy-statement filter-bgp then accept");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            Assert.Equal("TestRouter# ", output3);
            
            var policies = device.GetRoutingPolicies();
            Assert.NotNull(policies);
            Assert.Contains("filter-bgp", policies.Keys);
            Assert.Contains("from route-filter 192.168.0.0/16 orlonger", policies["filter-bgp"].Terms[0]);
            Assert.Contains("then community add bgp-community", policies["filter-bgp"].Terms[0]);
            Assert.Contains("then accept", policies["filter-bgp"].Terms[0]);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigureCommunities()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set policy-options community bgp-community members 65001:100");
            var output2 = await device.ProcessCommandAsync("set policy-options community bgp-community members 65001:200");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            
            var communities = device.GetCommunities();
            Assert.NotNull(communities);
            Assert.Contains("bgp-community", communities.Keys);
            Assert.Contains("65001:100", communities["bgp-community"].Members);
            Assert.Contains("65001:200", communities["bgp-community"].Members);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigureAsPathGroups()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set policy-options as-path-group external-as path1 .*65002.*");
            var output2 = await device.ProcessCommandAsync("set policy-options as-path-group external-as path2 .*65003.*");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            
            var asPathGroups = device.GetAsPathGroups();
            Assert.NotNull(asPathGroups);
            Assert.Contains("external-as", asPathGroups.Keys);
            Assert.Contains(".*65002.*", asPathGroups["external-as"].Paths["path1"]);
            Assert.Contains(".*65003.*", asPathGroups["external-as"].Paths["path2"]);
        }

        [Fact]
        public async Task JuniperRouterShouldConfigurePrefixLists()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            
            // Act
            var output1 = await device.ProcessCommandAsync("set policy-options prefix-list filter-bgp 192.168.0.0/16");
            var output2 = await device.ProcessCommandAsync("set policy-options prefix-list filter-bgp 10.0.0.0/8");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("TestRouter# ", output1);
            Assert.Equal("TestRouter# ", output2);
            
            var prefixLists = device.GetPrefixLists();
            Assert.NotNull(prefixLists);
            Assert.Contains("filter-bgp", prefixLists.Keys);
            Assert.Contains("192.168.0.0/16", prefixLists["filter-bgp"].Prefixes);
            Assert.Contains("10.0.0.0/8", prefixLists["filter-bgp"].Prefixes);
        }

        [Fact]
        public async Task JuniperRouterShouldShowRoutingProtocols()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output1 = await device.ProcessCommandAsync("show ospf neighbor");
            var output2 = await device.ProcessCommandAsync("show bgp summary");
            var output3 = await device.ProcessCommandAsync("show isis adjacency");
            var output4 = await device.ProcessCommandAsync("show rip neighbor");
            
            // Assert
            Assert.Contains("OSPF Neighbor", output1);
            Assert.Contains("BGP Summary", output2);
            Assert.Contains("ISIS Adjacency", output3);
            Assert.Contains("RIP Neighbor", output4);
            Assert.Equal("TestRouter> ", device.GetPrompt());
        }
    }
} 
