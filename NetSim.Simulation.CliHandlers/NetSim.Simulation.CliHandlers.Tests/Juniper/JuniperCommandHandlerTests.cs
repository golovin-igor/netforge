using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Juniper
{
    public class JuniperCommandHandlerTests
    {
        [Fact]
        public void JuniperDevice_ShouldStartInOperationalMode()
        {
            // Arrange & Act
            var device = new JuniperDevice("TestRouter");
            
            // Assert
            Assert.Equal("TestRouter> ", device.GetPrompt());
            Assert.Equal("operational", device.GetMode());
        }
        
        [Fact]
        public void ConfigureHandler_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            device.ProcessCommand("configure");
            
            // Assert
            Assert.Equal("TestRouter# ", device.GetPrompt());
            Assert.Equal("configuration", device.GetCurrentMode());
        }

        [Fact]
        public void ConfigureHandler_ShouldClearCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("set system host-name TestRouter2");
            device.ProcessCommand("exit");
            
            // Act
            device.ProcessCommand("configure");
            var output = device.ProcessCommand("show | compare");
            
            // Assert
            Assert.Contains("No changes", output);
            Assert.Equal("configuration", device.GetCurrentMode());
        }

        [Fact]
        public void ShowHandler_ShouldDisplaySystemInfo()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show system");
            
            // Assert
            Assert.Contains("System information", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }

        [Fact]
        public void ShowHandler_ShouldDisplayInterfaces()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show interfaces");
            
            // Assert
            Assert.Contains("Interface information", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }

        [Fact]
        public void ShowHandler_ShouldDisplayVersion()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show version");
            
            // Assert
            Assert.Contains("Hostname: TestRouter", output);
            Assert.Contains("Model: vSRX", output);
            Assert.Contains("Junos:", output);
            Assert.Contains("JUNOS Software Release", output);
        }

        [Fact]
        public void ShowHandler_ShouldDisplayConfiguration()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("set system host-name TestRouter2");
            
            // Act
            var output = device.ProcessCommand("show configuration");
            
            // Assert
            Assert.Contains("system {", output);
            Assert.Contains("host-name TestRouter2;", output);
            Assert.Equal("configuration", device.GetCurrentMode());
        }

        [Fact]
        public void ShowHandler_ShouldDisplayCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("set system host-name TestRouter2");
            
            // Act
            var output = device.ProcessCommand("show | compare");
            
            // Assert
            Assert.Contains("+ host-name TestRouter2;", output);
            Assert.Equal("configuration", device.GetCurrentMode());
        }

        [Fact]
        public void ShowHandler_WhenNotInConfigMode_ShouldReturnError()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show configuration");
            
            // Assert
            Assert.Contains("Invalid mode", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }

        [Fact]
        public void ShowHandler_WithInvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show invalid");
            
            // Assert
            Assert.Contains("syntax error", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }

        [Fact]
        public void ShowHandler_WithIncompleteCommand_ShouldReturnError()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show");
            
            // Assert
            Assert.Contains("syntax error", output);
            Assert.Equal("operational", device.GetCurrentMode());
        }
        
        [Fact]
        public void SetCommand_ShouldAddToCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var result = device.ProcessCommand("set system host-name MyRouter");
            
            // Assert
            Assert.Equal("TestRouter# ", result);
            var candidateConfig = device.GetCandidateConfig();
            Assert.Contains("set system host-name MyRouter", candidateConfig);
        }
        
        [Fact]
        public void SetInterfaceCommand_ShouldConfigureInterface()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var result1 = device.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            var result2 = device.ProcessCommand("set interfaces ge-0/0/0 description \"LAN Interface\"");
            
            // Assert
            Assert.Equal("TestRouter# ", result1);
            Assert.Equal("TestRouter# ", result2);
            
            var candidateConfig = device.GetCandidateConfig();
            Assert.Contains("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24", candidateConfig);
            Assert.Contains("set interfaces ge-0/0/0 description \"LAN Interface\"", candidateConfig);
        }
        
        [Fact]
        public void SetVlanCommand_ShouldConfigureVlan()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var result = device.ProcessCommand("set vlans management vlan-id 100");
            
            // Assert
            Assert.Equal("TestRouter# ", result);
            
            var candidateConfig = device.GetCandidateConfig();
            Assert.Contains("set vlans management vlan-id 100", candidateConfig);
        }
        
        [Fact]
        public void CommitCommand_ShouldApplyConfiguration()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("set system host-name MyRouter");
            device.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            
            // Act
            var result = device.ProcessCommand("commit");
            
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
        public void RollbackCommand_ShouldClearCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("set system host-name MyRouter");
            device.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            
            // Act
            var result = device.ProcessCommand("rollback");
            
            // Assert
            Assert.Contains("rollback complete", result);
            
            // Candidate config should be cleared
            var candidateConfig = device.GetCandidateConfig();
            Assert.Empty(candidateConfig);
        }
        
        [Fact]
        public void ShowConfiguration_ShouldDisplayCandidateConfig()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("set system host-name MyRouter");
            device.ProcessCommand("set vlans management vlan-id 100");
            
            // Act
            var result = device.ProcessCommand("show configuration");
            
            // Assert
            Assert.Contains("set system host-name MyRouter", result);
            Assert.Contains("set vlans management vlan-id 100", result);
            Assert.Contains("## Last changed:", result);
        }
        
        [Fact]
        public void ExitCommand_ShouldLeaveConfigurationMode()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var result = device.ProcessCommand("exit");
            
            // Assert
            Assert.Contains("Exiting configuration mode", result);
            Assert.Equal("TestRouter> ", device.GetPrompt());
            Assert.Equal("operational", device.GetMode());
        }
        
        [Fact]
        public void ShowVersion_ShouldDisplayJunosVersion()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var result = device.ProcessCommand("show version");
            
            // Assert
            Assert.Contains("Hostname: TestRouter", result);
            Assert.Contains("Model: vSRX", result);
            Assert.Contains("Junos: 20.4R3.8", result);
            Assert.Contains("JUNOS Software Release", result);
        }
        
        [Fact]
        public void ShowInterfaces_ShouldDisplayInterfaceInfo()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            device.ProcessCommand("set interfaces ge-0/0/0 description \"Test Interface\"");
            device.ProcessCommand("commit");
            device.ProcessCommand("exit");
            
            // Act
            var result = device.ProcessCommand("show interfaces ge-0/0/0");
            
            // Assert
            Assert.Contains("Physical interface: ge-0/0/0", result);
            Assert.Contains("Description: Test Interface", result);
            Assert.Contains("192.168.1.1/24", result);
        }
        
        [Fact]
        public void PingCommand_ShouldExecutePing()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var result = device.ProcessCommand("ping 8.8.8.8");
            
            // Assert
            Assert.Contains("ICMP Echos to 8.8.8.8", result);
            Assert.Contains("timeout is 2 seconds", result);
        }
        
        [Fact]
        public void CommandHistory_ShouldWorkWithJuniper()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            device.ProcessCommand("show version");
            device.ProcessCommand("configure");
            device.ProcessCommand("set system host-name MyRouter");
            var result = device.ProcessCommand("history");
            
            // Assert
            Assert.Contains("show version", result);
            Assert.Contains("configure", result);
            Assert.Contains("set system host-name MyRouter", result);
        }

        [Fact]
        public void JuniperRouter_ShouldConfigureOspf()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set protocols ospf area 0.0.0.0 interface ge-0/0/0.0");
            var output2 = device.ProcessCommand("set protocols ospf area 0.0.0.0 interface lo0.0 passive");
            var output3 = device.ProcessCommand("set routing-options router-id 1.1.1.1");
            
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
        public void JuniperRouter_ShouldConfigureBgp()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set routing-options autonomous-system 65001");
            var output2 = device.ProcessCommand("set protocols bgp group external-peers type external");
            var output3 = device.ProcessCommand("set protocols bgp group external-peers peer-as 65002");
            var output4 = device.ProcessCommand("set protocols bgp group external-peers neighbor 192.168.1.2");
            
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
        public void JuniperRouter_ShouldConfigureIsIs()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set protocols isis interface ge-0/0/0.0");
            var output2 = device.ProcessCommand("set protocols isis interface lo0.0 passive");
            var output3 = device.ProcessCommand("set protocols isis level 1 disable");
            
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
        public void JuniperRouter_ShouldConfigureRip()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set protocols rip group rip-group neighbor ge-0/0/0.0");
            var output2 = device.ProcessCommand("set protocols rip group rip-group export rip-export");
            var output3 = device.ProcessCommand("set protocols rip authentication-type md5");
            
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
        public void JuniperRouter_ShouldConfigureRouteRedistribution()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set protocols ospf export bgp-to-ospf");
            var output2 = device.ProcessCommand("set policy-options policy-statement bgp-to-ospf from protocol bgp");
            var output3 = device.ProcessCommand("set policy-options policy-statement bgp-to-ospf then accept");
            
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
        public void JuniperRouter_ShouldConfigureRoutingPolicies()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set policy-options policy-statement filter-bgp from route-filter 192.168.0.0/16 orlonger");
            var output2 = device.ProcessCommand("set policy-options policy-statement filter-bgp then community add bgp-community");
            var output3 = device.ProcessCommand("set policy-options policy-statement filter-bgp then accept");
            
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
        public void JuniperRouter_ShouldConfigureCommunities()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set policy-options community bgp-community members 65001:100");
            var output2 = device.ProcessCommand("set policy-options community bgp-community members 65001:200");
            
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
        public void JuniperRouter_ShouldConfigureAsPathGroups()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set policy-options as-path-group external-as path1 .*65002.*");
            var output2 = device.ProcessCommand("set policy-options as-path-group external-as path2 .*65003.*");
            
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
        public void JuniperRouter_ShouldConfigurePrefixLists()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output1 = device.ProcessCommand("set policy-options prefix-list filter-bgp 192.168.0.0/16");
            var output2 = device.ProcessCommand("set policy-options prefix-list filter-bgp 10.0.0.0/8");
            
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
        public void JuniperRouter_ShouldShowRoutingProtocols()
        {
            // Arrange
            var device = new JuniperDevice("TestRouter");
            
            // Act
            var output1 = device.ProcessCommand("show ospf neighbor");
            var output2 = device.ProcessCommand("show bgp summary");
            var output3 = device.ProcessCommand("show isis adjacency");
            var output4 = device.ProcessCommand("show rip neighbor");
            
            // Assert
            Assert.Contains("OSPF Neighbor", output1);
            Assert.Contains("BGP Summary", output2);
            Assert.Contains("ISIS Adjacency", output3);
            Assert.Contains("RIP Neighbor", output4);
            Assert.Equal("TestRouter> ", device.GetPrompt());
        }
    }
} 
