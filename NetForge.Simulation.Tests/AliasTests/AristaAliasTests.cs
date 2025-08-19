using System;
using System.Linq;
using NetForge.Simulation.CliHandlers.Arista;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Arista command aliases and interface short names.
    /// </summary>
    public class AristaAliasTests
    {
        [Theory]
        [InlineData("et1", "Ethernet1")]
        [InlineData("Et1", "Ethernet1")]
        [InlineData("eth1", "Ethernet1")]
        [InlineData("ethernet1", "Ethernet1")]
        [InlineData("Ethernet1", "Ethernet1")]
        [InlineData("ma1", "Management1")]
        [InlineData("Ma1", "Management1")]
        [InlineData("mgmt1", "Management1")]
        [InlineData("management1", "Management1")]
        [InlineData("Management1", "Management1")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("Lo0", "Loopback0")]
        [InlineData("loop0", "Loopback0")]
        [InlineData("loopback0", "Loopback0")]
        [InlineData("Loopback0", "Loopback0")]
        [InlineData("vl100", "Vlan100")]
        [InlineData("Vl100", "Vlan100")]
        [InlineData("vlan100", "Vlan100")]
        [InlineData("Vlan100", "Vlan100")]
        [InlineData("po1", "Port-Channel1")]
        [InlineData("Po1", "Port-Channel1")]
        [InlineData("port1", "Port-Channel1")]
        [InlineData("port-channel1", "Port-Channel1")]
        [InlineData("Port-Channel1", "Port-Channel1")]
        [InlineData("tu1", "Tunnel1")]
        [InlineData("Tu1", "Tunnel1")]
        [InlineData("tunnel1", "Tunnel1")]
        [InlineData("Tunnel1", "Tunnel1")]
        [InlineData("et1.100", "Ethernet1.100")]
        [InlineData("mgmt1.200", "Management1.200")]
        public void ExpandInterfaceAlias_WithValidAliases_ReturnsExpandedName(string input, string expected)
        {
            var result = AristaInterfaceAliasHandler.ExpandInterfaceAlias(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Ethernet1", "et1")]
        [InlineData("Management1", "ma1")]
        [InlineData("Loopback0", "lo0")]
        [InlineData("Vlan100", "vl100")]
        [InlineData("Port-Channel1", "po1")]
        [InlineData("Tunnel1", "tu1")]
        [InlineData("Ethernet1.100", "et1.100")]
        [InlineData("Management1.200", "ma1.200")]
        public void CompressInterfaceName_WithValidNames_ReturnsShortestAlias(string input, string expected)
        {
            var result = AristaInterfaceAliasHandler.CompressInterfaceName(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Ethernet1")]
        [InlineData("et1")]
        [InlineData("eth1")]
        [InlineData("ethernet1")]
        [InlineData("Management1")]
        [InlineData("ma1")]
        [InlineData("mgmt1")]
        [InlineData("management1")]
        [InlineData("Loopback0")]
        [InlineData("lo0")]
        [InlineData("loop0")]
        [InlineData("loopback0")]
        [InlineData("Vlan100")]
        [InlineData("vl100")]
        [InlineData("vlan100")]
        [InlineData("Port-Channel1")]
        [InlineData("po1")]
        [InlineData("port1")]
        [InlineData("port-channel1")]
        [InlineData("Tunnel1")]
        [InlineData("tu1")]
        [InlineData("tunnel1")]
        [InlineData("Ethernet1.100")]
        [InlineData("et1.100")]
        [InlineData("Management1.200")]
        [InlineData("ma1.200")]
        public void IsValidInterfaceName_WithValidNames_ReturnsTrue(string input)
        {
            var result = AristaInterfaceAliasHandler.IsValidInterfaceName(input);
            Assert.True(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("InvalidInterface")]
        [InlineData("xyz123")]
        [InlineData("123")]
        [InlineData("Ethernet")]
        [InlineData("et")]
        [InlineData("1")]
        [InlineData("Interface1")]
        [InlineData("Port1")]
        [InlineData("Switch1")]
        public void IsValidInterfaceName_WithInvalidNames_ReturnsFalse(string input)
        {
            var result = AristaInterfaceAliasHandler.IsValidInterfaceName(input);
            Assert.False(result);
        }

        [Theory]
        [InlineData("Ethernet1", "et1")]
        [InlineData("et1", "Ethernet1")]
        [InlineData("eth1", "ethernet1")]
        [InlineData("Management1", "ma1")]
        [InlineData("ma1", "Management1")]
        [InlineData("mgmt1", "management1")]
        [InlineData("Loopback0", "lo0")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("loop0", "loopback0")]
        [InlineData("Vlan100", "vl100")]
        [InlineData("vl100", "Vlan100")]
        [InlineData("vlan100", "Vlan100")]
        [InlineData("Port-Channel1", "po1")]
        [InlineData("po1", "Port-Channel1")]
        [InlineData("port1", "port-channel1")]
        [InlineData("Tunnel1", "tu1")]
        [InlineData("tu1", "Tunnel1")]
        [InlineData("tunnel1", "Tunnel1")]
        [InlineData("Ethernet1.100", "et1.100")]
        [InlineData("et1.100", "Ethernet1.100")]
        [InlineData("Management1.200", "ma1.200")]
        [InlineData("ma1.200", "Management1.200")]
        public void AreEquivalentInterfaceNames_WithEquivalentNames_ReturnsTrue(string name1, string name2)
        {
            var result = AristaInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);
            Assert.True(result);
        }

        [Theory]
        [InlineData("Ethernet1", "Management1")]
        [InlineData("et1", "ma1")]
        [InlineData("Loopback0", "Vlan100")]
        [InlineData("lo0", "vl100")]
        [InlineData("Port-Channel1", "Tunnel1")]
        [InlineData("po1", "tu1")]
        [InlineData("Ethernet1", "Ethernet2")]
        [InlineData("et1", "et2")]
        [InlineData("Ethernet1.100", "Ethernet1.200")]
        [InlineData("et1.100", "et1.200")]
        [InlineData("", "Ethernet1")]
        [InlineData("Ethernet1", "")]
        [InlineData("", "")]
        public void AreEquivalentInterfaceNames_WithNonEquivalentNames_ReturnsFalse(string name1, string name2)
        {
            var result = AristaInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);
            Assert.False(result);
        }

        [Theory]
        [InlineData("Ethernet1", new[] { "Ethernet1", "et1", "eth1", "ethernet1" })]
        [InlineData("Management1", new[] { "Management1", "ma1", "mgmt1", "management1" })]
        [InlineData("Loopback0", new[] { "Loopback0", "lo0", "loop0", "loopback0" })]
        [InlineData("Vlan100", new[] { "Vlan100", "vl100", "vlan100" })]
        [InlineData("Port-Channel1", new[] { "Port-Channel1", "po1", "port1", "port-channel1" })]
        [InlineData("Tunnel1", new[] { "Tunnel1", "tu1", "tunnel1" })]
        public void GetInterfaceAliases_WithValidInterface_ReturnsAllAliases(string interfaceName, string[] expectedAliases)
        {
            var aliases = AristaInterfaceAliasHandler.GetInterfaceAliases(interfaceName);
            
            Assert.Equal(expectedAliases.Length, aliases.Count);
            foreach (var expectedAlias in expectedAliases)
            {
                Assert.Contains(expectedAlias, aliases, StringComparer.OrdinalIgnoreCase);
            }
        }

        [Theory]
        [InlineData("InvalidInterface", new[] { "InvalidInterface" })]
        [InlineData("", new string[0])]
        public void GetInterfaceAliases_WithInvalidInterface_ReturnsOriginalOrEmpty(string interfaceName, string[] expectedAliases)
        {
            var aliases = AristaInterfaceAliasHandler.GetInterfaceAliases(interfaceName);
            
            Assert.Equal(expectedAliases.Length, aliases.Count);
            foreach (var expectedAlias in expectedAliases)
            {
                Assert.Contains(expectedAlias, aliases, StringComparer.OrdinalIgnoreCase);
            }
        }

        [Theory]
        [InlineData("Ethernet1", "Ethernet")]
        [InlineData("et1", "Ethernet")]
        [InlineData("Management1", "Management")]
        [InlineData("ma1", "Management")]
        [InlineData("Loopback0", "Loopback")]
        [InlineData("lo0", "Loopback")]
        [InlineData("Vlan100", "Vlan")]
        [InlineData("vl100", "Vlan")]
        [InlineData("Port-Channel1", "Port-Channel")]
        [InlineData("po1", "Port-Channel")]
        [InlineData("Tunnel1", "Tunnel")]
        [InlineData("tu1", "Tunnel")]
        public void GetInterfaceType_WithValidInterface_ReturnsCorrectType(string interfaceName, string expectedType)
        {
            var result = AristaInterfaceAliasHandler.GetInterfaceType(interfaceName);
            Assert.Equal(expectedType, result);
        }

        [Theory]
        [InlineData("Ethernet1", "1")]
        [InlineData("et1", "1")]
        [InlineData("Management1", "1")]
        [InlineData("ma1", "1")]
        [InlineData("Loopback0", "0")]
        [InlineData("lo0", "0")]
        [InlineData("Vlan100", "100")]
        [InlineData("vl100", "100")]
        [InlineData("Port-Channel1", "1")]
        [InlineData("po1", "1")]
        [InlineData("Tunnel1", "1")]
        [InlineData("tu1", "1")]
        [InlineData("Ethernet1.100", "1.100")]
        [InlineData("et1.100", "1.100")]
        public void GetInterfaceNumber_WithValidInterface_ReturnsCorrectNumber(string interfaceName, string expectedNumber)
        {
            var result = AristaInterfaceAliasHandler.GetInterfaceNumber(interfaceName);
            Assert.Equal(expectedNumber, result);
        }

        [Theory]
        [InlineData("et1", "Ethernet1")]
        [InlineData("ma1", "Management1")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("vl100", "Vlan100")]
        [InlineData("po1", "Port-Channel1")]
        [InlineData("tu1", "Tunnel1")]
        [InlineData("Ethernet1", "Ethernet1")]
        [InlineData("Management1", "Management1")]
        [InlineData("Loopback0", "Loopback0")]
        [InlineData("Vlan100", "Vlan100")]
        [InlineData("Port-Channel1", "Port-Channel1")]
        [InlineData("Tunnel1", "Tunnel1")]
        public void GetCanonicalInterfaceName_WithValidInterface_ReturnsCanonicalName(string input, string expected)
        {
            var result = AristaInterfaceAliasHandler.GetCanonicalInterfaceName(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Arista_InterfaceAliasIntegration_ShouldWork()
        {
            var device = new AristaDevice("test-arista");
            
            // Add interface using full name
            device.AddInterface("Ethernet1");
            
            // Should be able to retrieve using alias
            var iface1 = device.GetInterface("et1");
            var iface2 = device.GetInterface("eth1");
            var iface3 = device.GetInterface("ethernet1");
            var iface4 = device.GetInterface("Ethernet1");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.NotNull(iface4);
            
            // All should refer to the same interface
            Assert.Same(iface1, iface2);
            Assert.Same(iface2, iface3);
            Assert.Same(iface3, iface4);
        }

        [Fact]
        public void Arista_InterfaceAliasConfig_ShouldApplySettings()
        {
            var device = new AristaDevice("test-arista");
            
            // Add interface using full name
            device.AddInterface("Ethernet1");
            
            // Configure using alias
            var iface = device.GetInterface("et1");
            Assert.NotNull(iface);
            iface.IpAddress = "192.168.1.1";
            iface.SubnetMask = "255.255.255.0";
            iface.Description = "Test interface";
            
            // Verify using different alias
            var verifyIface = device.GetInterface("eth1");
            Assert.NotNull(verifyIface);
            Assert.Equal("192.168.1.1", verifyIface.IpAddress);
            Assert.Equal("255.255.255.0", verifyIface.SubnetMask);
            Assert.Equal("Test interface", verifyIface.Description);
        }

        [Fact]
        public async Task Arista_ShowInterfaceAlias_ShouldWork()
        {
            var device = new AristaDevice("test-arista");
            
            // Add interface using full name
            device.AddInterface("Ethernet1");
            var iface = device.GetInterface("Ethernet1");
            iface.IpAddress = "192.168.1.1";
            iface.SubnetMask = "255.255.255.0";
            iface.IsUp = true;
            
            // Test show command with alias
            var result = await device.ProcessCommandAsync("show int et1");
            Assert.Contains("Ethernet1", result);
            Assert.Contains("192.168.1.1", result);
        }
    }
}
