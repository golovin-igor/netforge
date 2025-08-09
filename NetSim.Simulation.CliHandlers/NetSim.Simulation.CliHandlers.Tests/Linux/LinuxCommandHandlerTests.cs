using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Linux
{
    public class LinuxCommandHandlerTests
    {
        [Fact]
        public void LinuxHandler_IpAddrShow_ShouldDisplayInterfaces()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ip addr show");
            
            // Assert
            Assert.Contains("interface", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_IpLinkShow_ShouldDisplayLinkInfo()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ip link show");
            
            // Assert
            Assert.Contains("link", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_IpRouteShow_ShouldDisplayRoutes()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ip route show");
            
            // Assert
            Assert.Contains("route", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_IpAddrAdd_ShouldConfigureInterface()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ip addr add 192.168.1.10/24 dev eth0");
            
            // Assert
            Assert.Contains("address configured", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_IpLinkSet_ShouldSetInterfaceState()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ip link set eth0 up");
            
            // Assert
            Assert.Contains("interface state", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_IpRouteAdd_ShouldAddRoute()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ip route add 10.0.0.0/8 via 192.168.1.1");
            
            // Assert
            Assert.Contains("route added", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_ArpCommand_ShouldShowArpTable()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("arp -n");
            
            // Assert
            Assert.Contains("arp", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_PingCommand_ShouldExecutePing()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ping 8.8.8.8");
            
            // Assert
            Assert.Contains("ping", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_TracerouteCommand_ShouldExecuteTraceroute()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("traceroute 8.8.8.8");
            
            // Assert
            Assert.Contains("traceroute", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_TcpdumpCommand_ShouldStartCapture()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("tcpdump -i eth0");
            
            // Assert
            Assert.Contains("tcpdump", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_NetstatCommand_ShouldShowNetworkInfo()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("netstat -i");
            
            // Assert
            Assert.Contains("interface", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_EthtoolCommand_ShouldShowInterfaceInfo()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ethtool eth0");
            
            // Assert
            Assert.Contains("ethtool", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_SsCommand_ShouldShowSockets()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ss -t");
            
            // Assert
            Assert.Contains("socket", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_NmapCommand_ShouldScanNetwork()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("nmap 192.168.1.1");
            
            // Assert
            Assert.Contains("nmap", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_IptablesCommand_ShouldManageFirewall()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("iptables -L");
            
            // Assert
            Assert.Contains("iptables", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_RouteCommand_ShouldManageRoutes()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("route add -net 10.0.0.0 netmask 255.0.0.0 gw 192.168.1.1");
            
            // Assert
            Assert.Contains("route", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_IfconfigCommand_ShouldConfigureInterface()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("ifconfig eth0 up");
            
            // Assert
            Assert.Contains("interface", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }

        [Fact]
        public void LinuxHandler_WithInvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand("invalid_command");
            
            // Assert
            Assert.Contains("command not found", output.ToLower());
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
        public void LinuxHandler_AllBasicCommands_ShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("command not found", output.ToLower());
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
        public void LinuxHandler_ConfigurationCommands_ShouldModifyState(string command)
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("command not found", output.ToLower());
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
        public void LinuxHandler_ShowCommands_ShouldDisplayInformation(string command)
        {
            // Arrange
            var device = new LinuxDevice("TestServer");
            
            // Act
            var output = device.ProcessCommand(command);
            
            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("command not found", output.ToLower());
            Assert.Equal("TestServer$", device.GetPrompt());
        }
    }
}
