using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Alcatel
{
    public class AlcatelCommandHandlerTests
    {
        [Fact]
        public async Task AlcatelHandlerConfigureShouldEnterConfigMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("configure");

            // Assert
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureSystemNameShouldSetSystemName()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");

            // Act
            var output = await device.ProcessCommandAsync("system name NewRouter");

            // Assert
            Assert.Equal("A:NewRouter>config#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigurePortShouldEnterPortMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");

            // Act
            var output = await device.ProcessCommandAsync("port 1/1/1");

            // Assert
            Assert.Equal("port", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>port#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureRouterInterfaceShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");

            // Act
            var output = await device.ProcessCommandAsync("router interface system");

            // Assert
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>interface#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureRouterOspfShouldEnterOspfMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router");

            // Act
            var output = await device.ProcessCommandAsync("ospf");

            // Assert
            Assert.Equal("ospf", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>ospf#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureRouterBgpShouldEnterBgpMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router");

            // Act
            var output = await device.ProcessCommandAsync("bgp");

            // Assert
            Assert.Equal("bgp", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>bgp#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerAdminDisplayConfigShouldShowConfiguration()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("admin display-config");

            // Assert
            Assert.Contains("Configuration", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerShowRouterRouteTableShouldDisplayRoutes()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show router route-table");

            // Assert
            Assert.Contains("Route Table", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerShowRouterArpShouldDisplayArpTable()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show router arp");

            // Assert
            Assert.Contains("ARP Table", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerShowRouterInterfaceShouldDisplayInterfaces()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show router interface");

            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerPingCommandShouldExecutePing()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("ping 8.8.8.8");

            // Assert
            Assert.Contains("ping", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerTracerouteCommandShouldExecuteTraceroute()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("traceroute 8.8.8.8");

            // Assert
            Assert.Contains("traceroute", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureVlanShouldEnterVlanMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");

            // Act
            var output = await device.ProcessCommandAsync("vlan 100");

            // Assert
            Assert.Equal("vlan", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>vlan#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerShowVlanInfoShouldDisplayVlanInfo()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show vlan info");

            // Assert
            Assert.Contains("VLAN", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureRouterMplsShouldEnterMplsMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router");

            // Act
            var output = await device.ProcessCommandAsync("mpls lsp test-lsp");

            // Assert
            Assert.Equal("mpls-lsp", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>mpls-lsp#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerShowRouterMplsLspShouldDisplayMplsLsp()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show router mpls lsp");

            // Assert
            Assert.Contains("MPLS LSP", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureRouterLdpShouldEnterLdpMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router");

            // Act
            var output = await device.ProcessCommandAsync("ldp");

            // Assert
            Assert.Equal("ldp", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>router>ldp#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerShowSystemTimeShouldDisplayTime()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show system time");

            // Assert
            Assert.Contains("Time", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureSystemTimeNtpShouldConfigureNtp()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("system");

            // Act
            var output = await device.ProcessCommandAsync("time ntp server 192.168.1.1");

            // Assert
            Assert.Equal("A:TestRouter>config>system#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerAdminSaveShouldSaveConfiguration()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("admin save");

            // Assert
            Assert.Contains("Configuration saved", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerShowSystemUptimeShouldDisplayUptime()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show system uptime");

            // Assert
            Assert.Contains("Uptime", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerConfigureServiceVplsShouldEnterVplsMode()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");

            // Act
            var output = await device.ProcessCommandAsync("service vpls 100");

            // Assert
            Assert.Equal("vpls", device.GetCurrentMode());
            Assert.Equal("A:TestRouter>config>service>vpls#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerShowServiceVplsShouldDisplayVplsInfo()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("show service vpls 100");

            // Assert
            Assert.Contains("VPLS", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }

        [Fact]
        public async Task AlcatelHandlerWithInvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("invalid command");

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
        public async Task AlcatelHandlerAllShowCommandsShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("enable"); // Enter privileged mode

            // Act
            var output = await device.ProcessCommandAsync(command);

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
        public async Task AlcatelHandlerConfigurationCommandsShouldWork(string command)
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            if (!command.Equals("configure", StringComparison.OrdinalIgnoreCase))
            {
                await device.ProcessCommandAsync("configure");
            }

            // Act
            var output = await device.ProcessCommandAsync(command);

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
        public async Task AlcatelHandlerAdvancedCommandsShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");
            await device.ProcessCommandAsync("configure");

            // Act
            var output = await device.ProcessCommandAsync(command);

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
        public async Task AlcatelHandlerArpCommandsShouldWork(string command)
        {
            // Arrange
            var device = new AlcatelDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync(command);

            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("Invalid", output);
            Assert.Equal("A:TestRouter#", device.GetPrompt());
        }
    }
}
