using System;
using System.Linq;
using NetForge.Simulation.CliHandlers.Cisco;
using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Cisco command and interface aliases.
    /// </summary>
    public class CiscoAliasTests
    {
        [Fact]
        public async Task Cisco_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new CiscoDevice("R1");
            var full = await device.ProcessCommandAsync("show running-config");
            var alias = await device.ProcessCommandAsync("sh run");

            Assert.Equal(full, alias);
        }

        [Fact]
        public async Task Cisco_InterfaceAliasConfig_ShouldApplySettings()
        {
            var device = new CiscoDevice("R1");

            await device.ProcessCommandAsync("conf t");
            await device.ProcessCommandAsync("int Gi0/0");
            await device.ProcessCommandAsync("ip address 10.1.1.1 255.255.255.0");
            await device.ProcessCommandAsync("no shut");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("exit");

            var iface = device.GetInterface("GigabitEthernet0/0");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = await device.ProcessCommandAsync("show interface GigabitEthernet0/0");
            var alias = await device.ProcessCommandAsync("sh int Gi0/0");
            Assert.Equal(full, alias);
        }

        [Theory]
        [InlineData("gi0/1", "GigabitEthernet0/1")]
        [InlineData("Gi0/1", "GigabitEthernet0/1")]
        [InlineData("gig0/1", "GigabitEthernet0/1")]
        [InlineData("gigabit0/1", "GigabitEthernet0/1")]
        [InlineData("GigabitEthernet0/1", "GigabitEthernet0/1")]
        [InlineData("fa0/1", "FastEthernet0/1")]
        [InlineData("Fa0/1", "FastEthernet0/1")]
        [InlineData("fast0/1", "FastEthernet0/1")]
        [InlineData("FastEthernet0/1", "FastEthernet0/1")]
        [InlineData("eth0/1", "Ethernet0/1")]
        [InlineData("Ethernet0/1", "Ethernet0/1")]
        [InlineData("te0/1", "TenGigabitEthernet0/1")]
        [InlineData("ten0/1", "TenGigabitEthernet0/1")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("loop0", "Loopback0")]
        [InlineData("Loopback0", "Loopback0")]
        [InlineData("vl100", "Vlan100")]
        [InlineData("vlan100", "Vlan100")]
        [InlineData("po1", "Port-channel1")]
        [InlineData("port1", "Port-channel1")]
        [InlineData("Port-channel1", "Port-channel1")]
        [InlineData("se0/0/0", "Serial0/0/0")]
        [InlineData("Serial0/0/0", "Serial0/0/0")]
        [InlineData("tu0", "Tunnel0")]
        [InlineData("tunnel0", "Tunnel0")]
        public void ExpandInterfaceAlias_WithValidAliases_ReturnsCorrectExpansion(string input, string expected)
        {
            // Act
            var result = CiscoInterfaceAliasHandler.ExpandInterfaceAlias(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("GigabitEthernet0/1", "gi0/1")]
        [InlineData("FastEthernet0/1", "fa0/1")]
        [InlineData("Ethernet0/1", "eth0/1")]
        [InlineData("TenGigabitEthernet0/1", "te0/1")]
        [InlineData("Loopback0", "lo0")]
        [InlineData("Vlan100", "vl100")]
        [InlineData("Port-channel1", "po1")]
        [InlineData("Serial0/0/0", "se0/0/0")]
        [InlineData("Tunnel0", "tu0")]
        public void CompressInterfaceName_WithValidNames_ReturnsShortestAlias(string input, string expected)
        {
            // Act
            var result = CiscoInterfaceAliasHandler.CompressInterfaceName(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("gi0/1")]
        [InlineData("GigabitEthernet0/1")]
        [InlineData("fa0/1")]
        [InlineData("FastEthernet0/1")]
        [InlineData("eth0/1")]
        [InlineData("te0/1")]
        [InlineData("lo0")]
        [InlineData("vl100")]
        [InlineData("po1")]
        [InlineData("se0/0/0")]
        [InlineData("tu0")]
        [InlineData("gi1/0/1")]
        [InlineData("fa0/1.100")]
        [InlineData("gi0/1.500")]
        public void IsValidInterfaceName_WithValidNames_ReturnsTrue(string input)
        {
            // Act
            var result = CiscoInterfaceAliasHandler.IsValidInterfaceName(input);

            // Assert
            Assert.True(result, $"Expected '{input}' to be valid");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid")]
        [InlineData("xyz0/1")]
        [InlineData("gi")]
        [InlineData("gi/1")]
        [InlineData("gi0/")]
        [InlineData("gi0/a")]
        [InlineData("gi-1/0")]
        [InlineData("gi0/1/2/3/4")]
        public void IsValidInterfaceName_WithInvalidNames_ReturnsFalse(string input)
        {
            // Act
            var result = CiscoInterfaceAliasHandler.IsValidInterfaceName(input);

            // Assert
            Assert.False(result, $"Expected '{input}' to be invalid");
        }

        [Theory]
        [InlineData("gi0/1", "GigabitEthernet0/1")]
        [InlineData("Gi0/1", "gigabitethernet0/1")]
        [InlineData("gig0/1", "GigabitEthernet0/1")]
        [InlineData("fa0/1", "FastEthernet0/1")]
        [InlineData("po1", "Port-channel1")]
        [InlineData("lo0", "Loopback0")]
        public void AreEquivalentInterfaceNames_WithAliases_ReturnsTrue(string name1, string name2)
        {
            // Act
            var result = CiscoInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);

            // Assert
            Assert.True(result, $"Expected '{name1}' and '{name2}' to be equivalent");
        }

        [Theory]
        [InlineData("gi0/1", "fa0/1")]
        [InlineData("gi0/1", "gi0/2")]
        [InlineData("lo0", "lo1")]
        [InlineData("po1", "po2")]
        [InlineData("gi0/1", "gi1/1")]
        public void AreEquivalentInterfaceNames_WithDifferentInterfaces_ReturnsFalse(string name1, string name2)
        {
            // Act
            var result = CiscoInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);

            // Assert
            Assert.False(result, $"Expected '{name1}' and '{name2}' to be different");
        }

        [Fact]
        public void GetInterfaceAliases_WithGigabitEthernet_ReturnsAllAliases()
        {
            // Arrange
            var interfaceName = "GigabitEthernet0/1";

            // Act
            var aliases = CiscoInterfaceAliasHandler.GetInterfaceAliases(interfaceName);

            // Assert
            Assert.Contains("GigabitEthernet0/1", aliases);
            Assert.Contains("gi0/1", aliases);
            Assert.Contains("gig0/1", aliases);
            Assert.Contains("gigabit0/1", aliases);
            Assert.Contains("gigabiteth0/1", aliases);
            Assert.True(aliases.Count >= 5, "Should contain multiple aliases");
        }

        [Fact]
        public void GetInterfaceAliases_WithShortAlias_ReturnsAllAliases()
        {
            // Arrange
            var interfaceName = "gi0/1";

            // Act
            var aliases = CiscoInterfaceAliasHandler.GetInterfaceAliases(interfaceName);

            // Assert
            Assert.Contains("GigabitEthernet0/1", aliases);
            Assert.Contains("gi0/1", aliases);
            Assert.Contains("gig0/1", aliases);
            Assert.True(aliases.Count >= 3, "Should contain multiple aliases");
        }

        [Theory]
        [InlineData("GigabitEthernet0/1", "GigabitEthernet")]
        [InlineData("gi0/1", "GigabitEthernet")]
        [InlineData("FastEthernet0/1", "FastEthernet")]
        [InlineData("fa0/1", "FastEthernet")]
        [InlineData("Loopback0", "Loopback")]
        [InlineData("lo0", "Loopback")]
        [InlineData("Port-channel1", "Port-channel")]
        [InlineData("po1", "Port-channel")]
        public void GetInterfaceType_WithVariousInterfaces_ReturnsCorrectType(string interfaceName, string expectedType)
        {
            // Act
            var result = CiscoInterfaceAliasHandler.GetInterfaceType(interfaceName);

            // Assert
            Assert.Equal(expectedType, result);
        }

        [Theory]
        [InlineData("GigabitEthernet0/1", "0/1")]
        [InlineData("gi1/0/2", "1/0/2")]
        [InlineData("FastEthernet0/1.100", "0/1.100")]
        [InlineData("Loopback0", "0")]
        [InlineData("Port-channel1", "1")]
        [InlineData("Vlan100", "100")]
        public void GetInterfaceNumber_WithVariousInterfaces_ReturnsCorrectNumber(string interfaceName, string expectedNumber)
        {
            // Act
            var result = CiscoInterfaceAliasHandler.GetInterfaceNumber(interfaceName);

            // Assert
            Assert.Equal(expectedNumber, result);
        }

        [Fact]
        public void CiscoDevice_GetInterface_WithAlias_ReturnsCorrectInterface()
        {
            // Arrange
            var device = new CiscoDevice("R1");
            device.AddInterface("GigabitEthernet0/1");

            // Act
            var interfaceByFullName = device.GetInterface("GigabitEthernet0/1");
            var interfaceByAlias = device.GetInterface("gi0/1");
            var interfaceByShortAlias = device.GetInterface("Gi0/1");

            // Assert
            Assert.NotNull(interfaceByFullName);
            Assert.NotNull(interfaceByAlias);
            Assert.NotNull(interfaceByShortAlias);
            Assert.Same(interfaceByFullName, interfaceByAlias);
            Assert.Same(interfaceByFullName, interfaceByShortAlias);
            Assert.Equal("GigabitEthernet0/1", interfaceByAlias.Name);
        }

        [Fact]
        public async Task CiscoDevice_InterfaceCommand_WithAlias_EntersCorrectInterface()
        {
            // Arrange
            var device = new CiscoDevice("R1");

            // Act
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            var result = await device.ProcessCommandAsync("interface gi0/1");

            // Assert
            Assert.Equal("R1(config-if)#", device.GetPrompt().Trim());
            Assert.Equal("GigabitEthernet0/1", device.GetCurrentInterface());

            // Verify the interface was created with canonical name
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal("GigabitEthernet0/1", iface.Name);
        }

        [Fact]
        public async Task CiscoDevice_InterfaceCommand_WithVariousAliases_CreatesOneInterface()
        {
            // Arrange
            var device = new CiscoDevice("R1");

            // Act
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface gi0/1");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface gig0/1");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface GigabitEthernet0/1");

            // Assert
            var interfaces = device.GetAllInterfaces();
            var gigInterfaces = interfaces.Where(kvp => kvp.Key.Contains("GigabitEthernet0/1")).ToList();

            Assert.Single(gigInterfaces);
            Assert.Equal("GigabitEthernet0/1", gigInterfaces.First().Key);
        }

        [Fact]
        public async Task CiscoDevice_IpAddressCommand_WithAlias_ConfiguresCorrectInterface()
        {
            // Arrange
            var device = new CiscoDevice("R1");

            // Act
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("interface gi0/1");
            await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");

            // Assert
            var iface = device.GetInterface("GigabitEthernet0/1");
            Assert.NotNull(iface);
            Assert.Equal("192.168.1.1", iface.IpAddress);
            Assert.Equal("255.255.255.0", iface.SubnetMask);

            // Verify we can access the same interface via alias
            var ifaceViaAlias = device.GetInterface("gi0/1");
            Assert.Same(iface, ifaceViaAlias);
        }

        [Theory]
        [InlineData("gi0/1", "fa0/1", "te0/1", "lo0", "po1")]
        public async Task CiscoDevice_MultipleInterfaceTypes_WithAliases_AllWorkCorrectly(params string[] interfaceAliases)
        {
            // Arrange
            var device = new CiscoDevice("R1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");

            // Act & Assert
            foreach (var alias in interfaceAliases)
            {
                var result = await device.ProcessCommandAsync($"interface {alias}");
                Assert.Equal("R1(config-if)#", device.GetPrompt().Trim());

                var canonicalName = CiscoInterfaceAliasHandler.ExpandInterfaceAlias(alias);
                Assert.Equal(canonicalName, device.GetCurrentInterface());

                await device.ProcessCommandAsync("exit");
            }
        }

        [Fact]
        public void GetCanonicalInterfaceName_WithVariousInputs_ReturnsExpectedResults()
        {
            // Test cases
            var testCases = new[]
            {
                ("gi0/1", "GigabitEthernet0/1"),
                ("GigabitEthernet0/1", "GigabitEthernet0/1"),
                ("fa0/1", "FastEthernet0/1"),
                ("lo0", "Loopback0"),
                ("", ""),
                ("   ", "   "),
                ("invalid", "invalid")
            };

            foreach (var (input, expected) in testCases)
            {
                // Act
                var result = CiscoInterfaceAliasHandler.GetCanonicalInterfaceName(input);

                // Assert
                Assert.Equal(expected, result);
            }
        }
    }
}
