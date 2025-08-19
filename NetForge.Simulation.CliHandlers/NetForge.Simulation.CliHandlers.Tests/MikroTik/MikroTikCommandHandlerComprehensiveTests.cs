using System.Globalization;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.MikroTik
{
    public class MikroTikCommandHandlerComprehensiveTests
    {
        [Fact]
        public async Task MikroTikHandlerSystemIdentitySetShouldSetIdentity()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/system identity set name=NewRouter");

            // Assert
            Assert.Equal("[NewRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerInterfacePrintShouldDisplayInterfaces()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/interface print");

            // Assert
            Assert.Contains("Interface", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerInterfaceEthernetSetShouldConfigureInterface()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/interface ethernet set ether1 name=wan");

            // Assert
            Assert.Contains("configured", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpAddressAddShouldAddIpAddress()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip address add address=192.168.1.1/24 interface=ether1");

            // Assert
            Assert.Contains("added", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpAddressPrintShouldDisplayAddresses()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip address print");

            // Assert
            Assert.Contains("Address", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpRouteAddShouldAddRoute()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip route add dst-address=10.0.0.0/8 gateway=192.168.1.1");

            // Assert
            Assert.Contains("route", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpRoutePrintShouldDisplayRoutes()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip route print");

            // Assert
            Assert.Contains("Route", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpArpPrintShouldDisplayArpTable()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip arp print");

            // Assert
            Assert.Contains("ARP", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerSystemLicensePrintShouldDisplayLicense()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/system license print");

            // Assert
            Assert.Contains("License", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerToolTracerouteShouldExecuteTraceroute()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/tool traceroute 8.8.8.8");

            // Assert
            Assert.Contains("traceroute", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpFirewallFilterAddShouldAddFirewallRule()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip firewall filter add chain=input action=accept");

            // Assert
            Assert.Contains("firewall", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpFirewallFilterPrintShouldDisplayFirewallRules()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip firewall filter print");

            // Assert
            Assert.Contains("Firewall", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpDhcpServerAddShouldAddDhcpServer()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip dhcp-server add interface=ether1 name=dhcp1");

            // Assert
            Assert.Contains("dhcp", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpDhcpServerPrintShouldDisplayDhcpServers()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip dhcp-server print");

            // Assert
            Assert.Contains("DHCP", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpDnsSetShouldConfigureDns()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip dns set servers=8.8.8.8");

            // Assert
            Assert.Contains("dns", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerSystemNtpClientSetShouldConfigureNtp()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/system ntp client set enabled=yes server=pool.ntp.org");

            // Assert
            Assert.Contains("ntp", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerInterfaceVlanAddShouldAddVlan()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/interface vlan add interface=ether1 vlan-id=100");

            // Assert
            Assert.Contains("vlan", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpOspfInstanceAddShouldAddOspfInstance()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip ospf instance add name=default");

            // Assert
            Assert.Contains("ospf", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerIpBgpInstanceAddShouldAddBgpInstance()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/ip bgp instance add name=default as=65001");

            // Assert
            Assert.Contains("bgp", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerInterfaceBridgeAddShouldAddBridge()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/interface bridge add name=bridge1");

            // Assert
            Assert.Contains("bridge", output.ToLower(CultureInfo.InvariantCulture));
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerSystemResourcePrintShouldDisplayResources()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync("/system resource print");

            // Assert
            Assert.Contains("Resource", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Theory]
        [InlineData("/system identity print")]
        [InlineData("/interface print")]
        [InlineData("/ip address print")]
        [InlineData("/ip route print")]
        [InlineData("/ip arp print")]
        [InlineData("/system license print")]
        [InlineData("/ip firewall filter print")]
        [InlineData("/ip dhcp-server print")]
        [InlineData("/ip dns print")]
        [InlineData("/system ntp client print")]
        [InlineData("/interface vlan print")]
        [InlineData("/ip ospf print")]
        [InlineData("/ip bgp print")]
        [InlineData("/interface bridge print")]
        [InlineData("/system resource print")]
        [InlineData("/system clock print")]
        [InlineData("/system upgrade print")]
        [InlineData("/system script print")]
        [InlineData("/ip hotspot print")]
        [InlineData("/ip ipsec print")]
        [InlineData("/ip snmp print")]
        [InlineData("/system logging print")]
        [InlineData("/interface wireless print")]
        [InlineData("/ip cloud print")]
        [InlineData("/system watchdog print")]
        [InlineData("/ip traffic-flow print")]
        [InlineData("/ip proxy print")]
        [InlineData("/ip upnp print")]
        public async Task MikroTikHandlerAllShowCommandsShouldHaveHandlers(string command)
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync(command);

            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Theory]
        [InlineData("/system identity set name=NewName")]
        [InlineData("/interface ethernet set ether1 name=wan")]
        [InlineData("/ip address add address=192.168.1.1/24 interface=ether1")]
        [InlineData("/ip route add dst-address=10.0.0.0/8 gateway=192.168.1.1")]
        [InlineData("/ip firewall filter add chain=input action=accept")]
        [InlineData("/ip dhcp-server add interface=ether1 name=dhcp1")]
        [InlineData("/ip dns set servers=8.8.8.8")]
        [InlineData("/system ntp client set enabled=yes")]
        [InlineData("/interface vlan add interface=ether1 vlan-id=100")]
        [InlineData("/ip ospf instance add name=default")]
        [InlineData("/ip bgp instance add name=default as=65001")]
        [InlineData("/interface bridge add name=bridge1")]
        [InlineData("/ip firewall nat add chain=srcnat action=masquerade")]
        [InlineData("/mpls ldp set enabled=yes")]
        [InlineData("/ip hotspot profile add name=hsprof1")]
        [InlineData("/ip ipsec policy add src-address=192.168.1.0/24 dst-address=10.0.0.0/8")]
        [InlineData("/ip snmp set enabled=yes")]
        [InlineData("/system logging add topics=info")]
        [InlineData("/interface wireless set wlan1 mode=ap-bridge")]
        [InlineData("/ip cloud set enabled=yes")]
        public async Task MikroTikHandlerConfigurationCommandsShouldWork(string command)
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync(command);

            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Theory]
        [InlineData("/ip route print detail")]
        [InlineData("/ip arp print detail")]
        [InlineData("/interface ethernet print detail")]
        [InlineData("/system resource print")]
        [InlineData("/ip firewall filter print detail")]
        [InlineData("/ip dhcp-server print detail")]
        [InlineData("/ip dns print detail")]
        [InlineData("/ip ospf print detail")]
        [InlineData("/ip bgp print detail")]
        [InlineData("/interface bridge print detail")]
        [InlineData("/ip hotspot print detail")]
        [InlineData("/ip ipsec print detail")]
        [InlineData("/ip snmp print detail")]
        [InlineData("/system logging print detail")]
        [InlineData("/interface wireless print detail")]
        public async Task MikroTikHandlerDetailedShowCommandsShouldWork(string command)
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync(command);

            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Theory]
        [InlineData("/ip route print vrf=main")]
        [InlineData("/ip arp print vrf=main")]
        [InlineData("/ip firewall filter print vrf=main")]
        [InlineData("/ip dhcp-server print vrf=main")]
        [InlineData("/ip ospf print vrf=main")]
        [InlineData("/ip bgp print vrf=main")]
        [InlineData("/ip ipsec print vrf=main")]
        [InlineData("/ip snmp print vrf=main")]
        public async Task MikroTikHandlerVrfCommandsShouldWork(string command)
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync(command);

            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Theory]
        [InlineData("/ip route print statistics")]
        [InlineData("/ip arp print statistics")]
        [InlineData("/interface ethernet print statistics")]
        [InlineData("/ip firewall filter print statistics")]
        [InlineData("/ip dhcp-server print statistics")]
        [InlineData("/ip ospf print statistics")]
        [InlineData("/ip bgp print statistics")]
        [InlineData("/interface bridge print statistics")]
        [InlineData("/ip ipsec print statistics")]
        [InlineData("/ip snmp print statistics")]
        [InlineData("/system logging print statistics")]
        [InlineData("/interface wireless print statistics")]
        public async Task MikroTikHandlerStatisticsCommandsShouldWork(string command)
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act
            var output = await device.ProcessCommandAsync(command);

            // Assert
            Assert.NotNull(output);
            Assert.DoesNotContain("bad command name", output);
            Assert.Equal("[TestRouter] > ", device.GetPrompt());
        }

        [Fact]
        public async Task MikroTikHandlerNestedVrfConfigurationCommandsShouldWork()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act & Assert for multiple VRF configuration commands
            var commands = new[]
            {
                "/ip route add vrf=test dst-address=10.0.0.0/8 gateway=192.168.1.1",
                "/ip arp add vrf=test address=192.168.1.1 mac-address=aa:bb:cc:dd:ee:ff",
                "/ip dhcp-server add vrf=test interface=ether1 name=dhcp-vrf",
                "/ip ospf instance add vrf=test name=ospf-vrf",
                "/ip bgp instance add vrf=test name=bgp-vrf as=65001"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("bad command name", output);
                Assert.Equal("[TestRouter] > ", device.GetPrompt());
            }
        }

        [Fact]
        public async Task MikroTikHandlerComplexFirewallConfigurationShouldWork()
        {
            // Arrange
            var device = new MikroTikDevice("TestRouter");

            // Act & Assert for complex firewall configuration
            var commands = new[]
            {
                "/ip firewall filter add chain=input action=accept",
                "/ip firewall filter add chain=forward action=drop",
                "/ip firewall nat add chain=srcnat action=masquerade",
                "/ip firewall mangle add chain=prerouting action=mark-connection",
                "/ip firewall mangle add chain=postrouting action=mark-packet"
            };

            foreach (var command in commands)
            {
                var output = await device.ProcessCommandAsync(command);
                Assert.NotNull(output);
                Assert.DoesNotContain("bad command name", output);
                Assert.Equal("[TestRouter] > ", device.GetPrompt());
            }
        }
    }
}
