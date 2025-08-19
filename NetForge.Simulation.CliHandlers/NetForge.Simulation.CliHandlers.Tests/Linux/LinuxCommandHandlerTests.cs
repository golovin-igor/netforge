using System.Globalization;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Linux
{
    public class LinuxCommandHandlerTests
    {
        [Fact]
        public async Task LinuxHandlerIpAddrShowShouldDisplayInterfaces()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ip addr show");
            
            // Assert
            Assert.Contains("interface", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerIpLinkShowShouldDisplayLinkInfo()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ip link show");
            
            // Assert
            Assert.Contains("link", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerIpRouteShowShouldDisplayRoutes()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ip route show");
            
            // Assert
            Assert.Contains("route", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerIpAddrAddShouldConfigureInterface()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ip addr add 192.168.1.10/24 dev eth0");
            
            // Assert
            Assert.Contains("address configured", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerIpLinkSetShouldSetInterfaceState()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ip link set eth0 up");
            
            // Assert
            Assert.Contains("interface state", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerIpRouteAddShouldAddRoute()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ip route add 10.0.0.0/8 via 192.168.1.1");
            
            // Assert
            Assert.Contains("route added", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerArpCommandShouldShowArpTable()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("arp -n");
            
            // Assert
            Assert.Contains("arp", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerPingCommandShouldExecutePing()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ping 8.8.8.8");
            
            // Assert
            Assert.Contains("ping", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerTracerouteCommandShouldExecuteTraceroute()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("traceroute 8.8.8.8");
            
            // Assert
            Assert.Contains("traceroute", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerTcpdumpCommandShouldStartCapture()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("tcpdump -i eth0");
            
            // Assert
            Assert.Contains("tcpdump", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerNetstatCommandShouldShowNetworkInfo()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("netstat -i");
            
            // Assert
            Assert.Contains("interface", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerEthtoolCommandShouldShowInterfaceInfo()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ethtool eth0");
            
            // Assert
            Assert.Contains("ethtool", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerSsCommandShouldShowSockets()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ss -t");
            
            // Assert
            Assert.Contains("socket", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerNmapCommandShouldScanNetwork()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("nmap 192.168.1.1");
            
            // Assert
            Assert.Contains("nmap", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerIptablesCommandShouldManageFirewall()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("iptables -L");
            
            // Assert
            Assert.Contains("iptables", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerRouteCommandShouldManageRoutes()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("route add -net 10.0.0.0 netmask 255.0.0.0 gw 192.168.1.1");
            
            // Assert
            Assert.Contains("route", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerIfconfigCommandShouldConfigureInterface()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("ifconfig eth0 up");
            
            // Assert
            Assert.Contains("interface", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public async Task LinuxHandlerWithInvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync("invalid_command");
            
            // Assert
            Assert.Contains("command not found", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Theory]
        [InlineData("ip addr show")]
        [InlineData("ip link show")]
        [InlineData("ip route show")]
        [InlineData("ip neigh show")]
        [InlineData("arp -n")]
        [InlineData("ping 127.0.0.1")]
        [InlineData("traceroute 127.0.0.1")]
        [InlineData("tcpdump -i lo")]
        [InlineData("netstat -i")]
        [InlineData("ethtool eth0")]
        [InlineData("ss -t")]
        [InlineData("ss -u")]
        [InlineData("nmap 127.0.0.1")]
        [InlineData("iptables -L")]
        [InlineData("route show")]
        [InlineData("ifconfig")]
        public async Task LinuxHandlerAllBasicCommandsShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("command not found", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Theory]
        [InlineData("ip addr add 192.168.1.10/24 dev eth0")]
        [InlineData("ip link set eth0 up")]
        [InlineData("ip route add 10.0.0.0/8 via 192.168.1.1")]
        [InlineData("ip neigh add 192.168.1.1 lladdr aa:bb:cc:dd:ee:ff dev eth0")]
        [InlineData("arp -s 192.168.1.1 aa:bb:cc:dd:ee:ff")]
        [InlineData("iptables -A INPUT -p tcp --dport 22 -j ACCEPT")]
        [InlineData("route add -net 10.0.0.0 netmask 255.0.0.0 gw 192.168.1.1")]
        [InlineData("ifconfig eth0 192.168.1.10 netmask 255.255.255.0")]
        public async Task LinuxHandlerConfigurationCommandsShouldModifyState(string command)
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("command not found", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Theory]
        [InlineData("ip addr show dev eth0")]
        [InlineData("ip route show cache")]
        [InlineData("ip route show table main")]
        [InlineData("netstat -r")]
        [InlineData("netstat -s")]
        [InlineData("netstat -tuln")]
        [InlineData("ethtool -k eth0")]
        [InlineData("ss -a")]
        [InlineData("ss -l")]
        [InlineData("nmap -sS 127.0.0.1")]
        [InlineData("iptables -t nat -L")]
        public async Task LinuxHandlerShowCommandsShouldDisplayInformation(string command)
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = await device.ProcessCommandAsync(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("command not found", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("TestServer$", device.GetPrompt());
        }
    }
}
