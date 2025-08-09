using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Alcatel
{
    public class AlcatelCommandHandlerTests
    {
        [Fact]
        public void AlcatelHandler_Configure_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("configure");
            
            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureSystemName_ShouldSetSystemName()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("system name NewRouter");
            
            // Assert
            Assert.Equal("A:NewRouter>config#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigurePort_ShouldEnterPortMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("port 1/1/1");
            
            // Assert
            Assert.Equal("port", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>port#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureRouterInterface_ShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("router interface system");
            
            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>interface#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureRouterOspf_ShouldEnterOspfMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("router");
            
            // Act
            var output = device.ProcessCommand("ospf");
            
            // Assert
            Assert.Equal("ospf", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>ospf#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureRouterBgp_ShouldEnterBgpMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("router");
            
            // Act
            var output = device.ProcessCommand("bgp");
            
            // Assert
            Assert.Equal("bgp", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>bgp#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_AdminDisplayConfig_ShouldShowConfiguration()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("admin display-config");
            
            // Assert
            Assert.Contains("Configuration", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ShowRouterRouteTable_ShouldDisplayRoutes()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show router route-table");
            
            // Assert
            Assert.Contains("Route Table", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ShowRouterArp_ShouldDisplayArpTable()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show router arp");
            
            // Assert
            Assert.Contains("ARP Table", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ShowRouterInterface_ShouldDisplayInterfaces()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show router interface");
            
            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_PingCommand_ShouldExecutePing()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("ping 8.8.8.8");
            
            // Assert
            Assert.Contains("ping", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_TracerouteCommand_ShouldExecuteTraceroute()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("traceroute 8.8.8.8");
            
            // Assert
            Assert.Contains("traceroute", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureVlan_ShouldEnterVlanMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("vlan 100");
            
            // Assert
            Assert.Equal("vlan", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>vlan#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ShowVlanInfo_ShouldDisplayVlanInfo()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show vlan info");
            
            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureRouterMpls_ShouldEnterMplsMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("router");
            
            // Act
            var output = device.ProcessCommand("mpls lsp test-lsp");
            
            // Assert
            Assert.Equal("mpls-lsp", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>mpls-lsp#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ShowRouterMplsLsp_ShouldDisplayMplsLsp()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show router mpls lsp");
            
            // Assert
            Assert.Contains("MPLS LSP", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureRouterLdp_ShouldEnterLdpMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("router");
            
            // Act
            var output = device.ProcessCommand("ldp");
            
            // Assert
            Assert.Equal("ldp", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>ldp#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ShowSystemTime_ShouldDisplayTime()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show system time");
            
            // Assert
            Assert.Contains("Time", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureSystemTimeNtp_ShouldConfigureNtp()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            device.ProcessCommand("system");
            
            // Act
            var output = device.ProcessCommand("time ntp server 192.168.1.1");
            
            // Assert
            Assert.Equal("A:TestRouter>config>system#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_AdminSave_ShouldSaveConfiguration()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("admin save");
            
            // Assert
            Assert.Contains("Configuration saved", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ShowSystemUptime_ShouldDisplayUptime()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show system uptime");
            
            // Assert
            Assert.Contains("Uptime", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ConfigureServiceVpls_ShouldEnterVplsMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand("service vpls 100");
            
            // Assert
            Assert.Equal("vpls", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>service>vpls#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_ShowServiceVpls_ShouldDisplayVplsInfo()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show service vpls 100");
            
            // Assert
            Assert.Contains("VPLS", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public void AlcatelHandler_WithInvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("invalid command");
            
            // Assert
            Assert.Contains("Invalid", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Theory]
        [InlineData("admin display-config")]
        [InlineData("show router route-table")]
        [InlineData("show router arp")]
        [InlineData("show router interface")]
        [InlineData("show vlan info")]
        [InlineData("show router mpls lsp")]
        [InlineData("show router ldp interface")]
        [InlineData("show system time")]
        [InlineData("show system uptime")]
        [InlineData("show router ospf neighbor")]
        [InlineData("show router bgp summary")]
        [InlineData("show router isis adjacency")]
        [InlineData("show router rip neighbor")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("traceroute 127.0.0.1")]
        public void AlcatelHandler_AllShowCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("enable"); // Enter privileged mode
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Theory]
        [InlineData("configure")]
        [InlineData("system name NewName")]
        [InlineData("port 1/1/1")]
        [InlineData("router interface system")]
        [InlineData("router ospf")]
        [InlineData("router bgp")]
        [InlineData("vlan 100")]
        [InlineData("service vpls 100")]
        public void AlcatelHandler_ConfigurationCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            if (!command.Equals("configure"))
            {
                device.ProcessCommand("configure");
            }
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("router isis")]
        [InlineData("router rip")]
        [InlineData("router multicast")]
        [InlineData("router pim")]
        [InlineData("router igmp")]
        [InlineData("filter ip-filter 1")]
        [InlineData("system security user test")]
        [InlineData("system snmp community public")]
        [InlineData("system login-control ssh")]
        public void AlcatelHandler_AdvancedCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            device.ProcessCommand("configure");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
        }

        [Theory]
        [InlineData("show router arp table")]
        [InlineData("show router arp statistics")]
        [InlineData("show router arp summary")]
        [InlineData("show router arp interface")]
        [InlineData("show router arp detail")]
        [InlineData("show router arp cache")]
        [InlineData("show router arp entry")]
        [InlineData("show router arp lookup")]
        [InlineData("show router arp resolve")]
        [InlineData("show router arp reachable")]
        [InlineData("show router arp connected")]
        public void AlcatelHandler_ArpCommands_ShouldWork(string command)
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }
    }
}
