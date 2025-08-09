using System;
using System.Linq;
using NetSim.Simulation.CliHandlers.Huawei;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Huawei command aliases and interface name shortcuts.
    /// </summary>
    public class HuaweiAliasTests
    {
        [Fact]
        public void Huawei_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new HuaweiDevice("R1");
            var full = device.ProcessCommand("display current-configuration");
            var alias = device.ProcessCommand("disp curr");
            Assert.Equal(full, alias);
        }



        [Theory]
        [InlineData("gi0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("Gi0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("gig0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("gigabit0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("ge0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("GigabitEthernet0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("fa0/0/0", "FastEthernet0/0/0")]
        [InlineData("Fa0/0/0", "FastEthernet0/0/0")]
        [InlineData("fast0/0/0", "FastEthernet0/0/0")]
        [InlineData("fe0/0/0", "FastEthernet0/0/0")]
        [InlineData("FastEthernet0/0/0", "FastEthernet0/0/0")]
        [InlineData("te0/0/0", "TenGigabitEthernet0/0/0")]
        [InlineData("xge0/0/0", "TenGigabitEthernet0/0/0")]
        [InlineData("25ge0/0/0", "TwentyFiveGigabitEthernet0/0/0")]
        [InlineData("40ge0/0/0", "FortyGigabitEthernet0/0/0")]
        [InlineData("100ge0/0/0", "HundredGigabitEthernet0/0/0")]
        [InlineData("s0/0/0", "Serial0/0/0")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("Lo0", "Loopback0")]
        [InlineData("loop0", "Loopback0")]
        [InlineData("loopback0", "Loopback0")]
        [InlineData("Loopback0", "Loopback0")]
        [InlineData("vl100", "Vlanif100")]
        [InlineData("Vl100", "Vlanif100")]
        [InlineData("vlan100", "Vlanif100")]
        [InlineData("vlanif100", "Vlanif100")]
        [InlineData("Vlanif100", "Vlanif100")]
        [InlineData("tu1", "Tunnel1")]
        [InlineData("Tu1", "Tunnel1")]
        [InlineData("tunnel1", "Tunnel1")]
        [InlineData("Tunnel1", "Tunnel1")]
        [InlineData("m0/0/0", "MEth0/0/0")]
        [InlineData("meth0/0/0", "MEth0/0/0")]
        [InlineData("management0/0/0", "MEth0/0/0")]
        [InlineData("nu0", "Null0")]
        [InlineData("null0", "Null0")]
        [InlineData("Null0", "Null0")]
        [InlineData("gi0/0/0.100", "GigabitEthernet0/0/0.100")]
        [InlineData("vlanif100.200", "Vlanif100.200")]
        public void ExpandInterfaceAlias_WithValidAliases_ReturnsExpandedName(string input, string expected)
        {
            var result = HuaweiInterfaceAliasHandler.ExpandInterfaceAlias(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("GigabitEthernet0/0/0", "gi0/0/0")]
        [InlineData("FastEthernet0/0/0", "fa0/0/0")]
        [InlineData("TenGigabitEthernet0/0/0", "te0/0/0")]
        [InlineData("TwentyFiveGigabitEthernet0/0/0", "twe0/0/0")]
        [InlineData("FortyGigabitEthernet0/0/0", "fo0/0/0")]
        [InlineData("HundredGigabitEthernet0/0/0", "hu0/0/0")]
        [InlineData("Serial0/0/0", "s0/0/0")]
        [InlineData("Loopback0", "lo0")]
        [InlineData("Vlanif100", "vl100")]
        [InlineData("Tunnel1", "tu1")]
        [InlineData("MEth0/0/0", "m0/0/0")]
        [InlineData("Null0", "nu0")]
        [InlineData("Bridge1", "br1")]
        [InlineData("GigabitEthernet0/0/0.100", "gi0/0/0.100")]
        [InlineData("Vlanif100.200", "vl100.200")]
        public void CompressInterfaceName_WithValidNames_ReturnsShortestAlias(string input, string expected)
        {
            var result = HuaweiInterfaceAliasHandler.CompressInterfaceName(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("GigabitEthernet0/0/0")]
        [InlineData("gi0/0/0")]
        [InlineData("gig0/0/0")]
        [InlineData("gigabit0/0/0")]
        [InlineData("ge0/0/0")]
        [InlineData("FastEthernet0/0/0")]
        [InlineData("fa0/0/0")]
        [InlineData("fast0/0/0")]
        [InlineData("fe0/0/0")]
        [InlineData("TenGigabitEthernet0/0/0")]
        [InlineData("te0/0/0")]
        [InlineData("xge0/0/0")]
        [InlineData("25ge0/0/0")]
        [InlineData("40ge0/0/0")]
        [InlineData("100ge0/0/0")]
        [InlineData("Serial0/0/0")]
        [InlineData("s0/0/0")]
        [InlineData("Loopback0")]
        [InlineData("lo0")]
        [InlineData("loop0")]
        [InlineData("loopback0")]
        [InlineData("Vlanif100")]
        [InlineData("vl100")]
        [InlineData("vlan100")]
        [InlineData("vlanif100")]
        [InlineData("Tunnel1")]
        [InlineData("tu1")]
        [InlineData("tunnel1")]
        [InlineData("MEth0/0/0")]
        [InlineData("m0/0/0")]
        [InlineData("meth0/0/0")]
        [InlineData("management0/0/0")]
        [InlineData("Null0")]
        [InlineData("nu0")]
        [InlineData("null0")]
        [InlineData("Bridge1")]
        [InlineData("br1")]
        [InlineData("bridge1")]
        [InlineData("GigabitEthernet0/0/0.100")]
        [InlineData("gi0/0/0.100")]
        [InlineData("Vlanif100.200")]
        [InlineData("vl100.200")]
        public void IsValidInterfaceName_WithValidNames_ReturnsTrue(string input)
        {
            var result = HuaweiInterfaceAliasHandler.IsValidInterfaceName(input);
            Assert.True(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("InvalidInterface")]
        [InlineData("xyz123")]
        [InlineData("123")]
        [InlineData("GigabitEthernet")]
        [InlineData("gi")]
        [InlineData("1")]
        [InlineData("Interface1")]
        [InlineData("Port1")]
        [InlineData("Switch1")]
        [InlineData("Eth1")]  // Huawei doesn't use simple "Eth" naming
        public void IsValidInterfaceName_WithInvalidNames_ReturnsFalse(string input)
        {
            var result = HuaweiInterfaceAliasHandler.IsValidInterfaceName(input);
            Assert.False(result);
        }

        [Theory]
        [InlineData("GigabitEthernet0/0/0", "gi0/0/0")]
        [InlineData("gi0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("gig0/0/0", "gigabit0/0/0")]
        [InlineData("ge0/0/0", "gi0/0/0")]
        [InlineData("FastEthernet0/0/0", "fa0/0/0")]
        [InlineData("fa0/0/0", "FastEthernet0/0/0")]
        [InlineData("fast0/0/0", "fe0/0/0")]
        [InlineData("TenGigabitEthernet0/0/0", "te0/0/0")]
        [InlineData("te0/0/0", "TenGigabitEthernet0/0/0")]
        [InlineData("xge0/0/0", "te0/0/0")]
        [InlineData("Serial0/0/0", "s0/0/0")]
        [InlineData("s0/0/0", "Serial0/0/0")]
        [InlineData("Loopback0", "lo0")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("loop0", "loopback0")]
        [InlineData("Vlanif100", "vl100")]
        [InlineData("vl100", "Vlanif100")]
        [InlineData("vlan100", "vlanif100")]
        [InlineData("Tunnel1", "tu1")]
        [InlineData("tu1", "Tunnel1")]
        [InlineData("tunnel1", "Tunnel1")]
        [InlineData("MEth0/0/0", "m0/0/0")]
        [InlineData("m0/0/0", "MEth0/0/0")]
        [InlineData("meth0/0/0", "management0/0/0")]
        [InlineData("Null0", "nu0")]
        [InlineData("nu0", "Null0")]
        [InlineData("null0", "Null0")]
        [InlineData("Bridge1", "br1")]
        [InlineData("br1", "Bridge1")]
        [InlineData("bridge1", "Bridge1")]
        [InlineData("GigabitEthernet0/0/0.100", "gi0/0/0.100")]
        [InlineData("gi0/0/0.100", "GigabitEthernet0/0/0.100")]
        [InlineData("Vlanif100.200", "vl100.200")]
        [InlineData("vl100.200", "Vlanif100.200")]
        public void AreEquivalentInterfaceNames_WithEquivalentNames_ReturnsTrue(string name1, string name2)
        {
            var result = HuaweiInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);
            Assert.True(result);
        }

        [Theory]
        [InlineData("GigabitEthernet0/0/0", "FastEthernet0/0/0")]
        [InlineData("gi0/0/0", "fa0/0/0")]
        [InlineData("Loopback0", "Vlanif100")]
        [InlineData("lo0", "vl100")]
        [InlineData("Tunnel1", "Serial0/0/0")]
        [InlineData("tu1", "s0/0/0")]
        [InlineData("GigabitEthernet0/0/0", "GigabitEthernet0/0/1")]
        [InlineData("gi0/0/0", "gi0/0/1")]
        [InlineData("GigabitEthernet0/0/0.100", "GigabitEthernet0/0/0.200")]
        [InlineData("gi0/0/0.100", "gi0/0/0.200")]
        [InlineData("", "GigabitEthernet0/0/0")]
        [InlineData("GigabitEthernet0/0/0", "")]
        [InlineData("", "")]
        public void AreEquivalentInterfaceNames_WithNonEquivalentNames_ReturnsFalse(string name1, string name2)
        {
            var result = HuaweiInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);
            Assert.False(result);
        }

        [Theory]
        [InlineData("GigabitEthernet0/0/0", new[] { "GigabitEthernet0/0/0", "gi0/0/0", "gig0/0/0", "gigabit0/0/0", "gigabitethernet0/0/0", "ge0/0/0" })]
        [InlineData("FastEthernet0/0/0", new[] { "FastEthernet0/0/0", "fa0/0/0", "fast0/0/0", "fastethernet0/0/0", "fe0/0/0" })]
        [InlineData("TenGigabitEthernet0/0/0", new[] { "TenGigabitEthernet0/0/0", "te0/0/0", "ten0/0/0", "tengig0/0/0", "tengigabit0/0/0", "tengigabitethernet0/0/0", "xge0/0/0" })]
        [InlineData("Serial0/0/0", new[] { "Serial0/0/0", "s0/0/0", "ser0/0/0", "serial0/0/0" })]
        [InlineData("Loopback0", new[] { "Loopback0", "lo0", "loop0", "loopback0" })]
        [InlineData("Vlanif100", new[] { "Vlanif100", "vl100", "vlan100", "vlanif100" })]
        [InlineData("Tunnel1", new[] { "Tunnel1", "tu1", "tunnel1" })]
        [InlineData("MEth0/0/0", new[] { "MEth0/0/0", "m0/0/0", "meth0/0/0", "management0/0/0" })]
        [InlineData("Null0", new[] { "Null0", "nu0", "null0" })]
        [InlineData("Bridge1", new[] { "Bridge1", "br1", "bridge1" })]
        public void GetInterfaceAliases_WithValidInterface_ReturnsAllAliases(string interfaceName, string[] expectedAliases)
        {
            var aliases = HuaweiInterfaceAliasHandler.GetInterfaceAliases(interfaceName);
            
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
            var aliases = HuaweiInterfaceAliasHandler.GetInterfaceAliases(interfaceName);
            
            Assert.Equal(expectedAliases.Length, aliases.Count);
            foreach (var expectedAlias in expectedAliases)
            {
                Assert.Contains(expectedAlias, aliases, StringComparer.OrdinalIgnoreCase);
            }
        }

        [Theory]
        [InlineData("GigabitEthernet0/0/0", "GigabitEthernet")]
        [InlineData("gi0/0/0", "GigabitEthernet")]
        [InlineData("FastEthernet0/0/0", "FastEthernet")]
        [InlineData("fa0/0/0", "FastEthernet")]
        [InlineData("TenGigabitEthernet0/0/0", "TenGigabitEthernet")]
        [InlineData("te0/0/0", "TenGigabitEthernet")]
        [InlineData("xge0/0/0", "TenGigabitEthernet")]
        [InlineData("Serial0/0/0", "Serial")]
        [InlineData("s0/0/0", "Serial")]
        [InlineData("Loopback0", "Loopback")]
        [InlineData("lo0", "Loopback")]
        [InlineData("Vlanif100", "Vlanif")]
        [InlineData("vl100", "Vlanif")]
        [InlineData("Tunnel1", "Tunnel")]
        [InlineData("tu1", "Tunnel")]
        [InlineData("MEth0/0/0", "MEth")]
        [InlineData("m0/0/0", "MEth")]
        [InlineData("Null0", "Null")]
        [InlineData("nu0", "Null")]
        [InlineData("Bridge1", "Bridge")]
        [InlineData("br1", "Bridge")]
        public void GetInterfaceType_WithValidInterface_ReturnsCorrectType(string interfaceName, string expectedType)
        {
            var result = HuaweiInterfaceAliasHandler.GetInterfaceType(interfaceName);
            Assert.Equal(expectedType, result);
        }

        [Theory]
        [InlineData("GigabitEthernet0/0/0", "0/0/0")]
        [InlineData("gi0/0/0", "0/0/0")]
        [InlineData("FastEthernet0/0/1", "0/0/1")]
        [InlineData("fa0/0/1", "0/0/1")]
        [InlineData("TenGigabitEthernet1/0/0", "1/0/0")]
        [InlineData("te1/0/0", "1/0/0")]
        [InlineData("Serial2/0/0", "2/0/0")]
        [InlineData("s2/0/0", "2/0/0")]
        [InlineData("Loopback0", "0")]
        [InlineData("lo0", "0")]
        [InlineData("Vlanif100", "100")]
        [InlineData("vl100", "100")]
        [InlineData("Tunnel1", "1")]
        [InlineData("tu1", "1")]
        [InlineData("MEth0/0/1", "0/0/1")]
        [InlineData("m0/0/1", "0/0/1")]
        [InlineData("Null0", "0")]
        [InlineData("nu0", "0")]
        [InlineData("Bridge1", "1")]
        [InlineData("br1", "1")]
        [InlineData("GigabitEthernet0/0/0.100", "0/0/0.100")]
        [InlineData("gi0/0/0.100", "0/0/0.100")]
        [InlineData("Vlanif100.200", "100.200")]
        [InlineData("vl100.200", "100.200")]
        public void GetInterfaceNumber_WithValidInterface_ReturnsCorrectNumber(string interfaceName, string expectedNumber)
        {
            var result = HuaweiInterfaceAliasHandler.GetInterfaceNumber(interfaceName);
            Assert.Equal(expectedNumber, result);
        }

        [Theory]
        [InlineData("gi0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("fa0/0/0", "FastEthernet0/0/0")]
        [InlineData("te0/0/0", "TenGigabitEthernet0/0/0")]
        [InlineData("xge0/0/0", "TenGigabitEthernet0/0/0")]
        [InlineData("s0/0/0", "Serial0/0/0")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("vl100", "Vlanif100")]
        [InlineData("tu1", "Tunnel1")]
        [InlineData("m0/0/0", "MEth0/0/0")]
        [InlineData("nu0", "Null0")]
        [InlineData("br1", "Bridge1")]
        [InlineData("GigabitEthernet0/0/0", "GigabitEthernet0/0/0")]
        [InlineData("FastEthernet0/0/0", "FastEthernet0/0/0")]
        [InlineData("TenGigabitEthernet0/0/0", "TenGigabitEthernet0/0/0")]
        [InlineData("Serial0/0/0", "Serial0/0/0")]
        [InlineData("Loopback0", "Loopback0")]
        [InlineData("Vlanif100", "Vlanif100")]
        [InlineData("Tunnel1", "Tunnel1")]
        [InlineData("MEth0/0/0", "MEth0/0/0")]
        [InlineData("Null0", "Null0")]
        [InlineData("Bridge1", "Bridge1")]
        public void GetCanonicalInterfaceName_WithValidInterface_ReturnsCanonicalName(string input, string expected)
        {
            var result = HuaweiInterfaceAliasHandler.GetCanonicalInterfaceName(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Huawei_InterfaceAliasIntegration_ShouldWork()
        {
            var device = new HuaweiDevice("test-huawei");
            
            // Add interface using full name
            device.AddInterface("GigabitEthernet0/0/0");
            
            // Should be able to retrieve using alias
            var iface1 = device.GetInterface("gi0/0/0");
            var iface2 = device.GetInterface("gig0/0/0");
            var iface3 = device.GetInterface("ge0/0/0");
            var iface4 = device.GetInterface("GigabitEthernet0/0/0");
            
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
        public void Huawei_InterfaceAliasConfig_ShouldApplySettings()
        {
            var device = new HuaweiDevice("test-huawei");
            
            // Add interface using full name
            device.AddInterface("GigabitEthernet0/0/0");
            
            // Configure using alias
            var iface = device.GetInterface("gi0/0/0");
            Assert.NotNull(iface);
            iface.IpAddress = "192.168.1.1";
            iface.SubnetMask = "255.255.255.0";
            iface.Description = "Test interface";
            
            // Verify using different alias
            var verifyIface = device.GetInterface("ge0/0/0");
            Assert.NotNull(verifyIface);
            Assert.Equal("192.168.1.1", verifyIface.IpAddress);
            Assert.Equal("255.255.255.0", verifyIface.SubnetMask);
            Assert.Equal("Test interface", verifyIface.Description);
        }

        [Fact]
        public void Huawei_VlanifAlias_ShouldWork()
        {
            var device = new HuaweiDevice("test-huawei");
            
            // Add VLAN interface using full name
            device.AddInterface("Vlanif100");
            
            // Should be able to retrieve using alias
            var iface1 = device.GetInterface("vl100");
            var iface2 = device.GetInterface("vlan100");
            var iface3 = device.GetInterface("Vlanif100");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            
            // All should refer to the same interface
            Assert.Same(iface1, iface2);
            Assert.Same(iface2, iface3);
        }

        [Fact]
        public void Huawei_SubInterfaceAlias_ShouldWork()
        {
            var device = new HuaweiDevice("test-huawei");
            
            // Add sub-interface using full name
            device.AddInterface("GigabitEthernet0/0/0.100");
            
            // Should be able to retrieve using alias
            var iface1 = device.GetInterface("gi0/0/0.100");
            var iface2 = device.GetInterface("gig0/0/0.100");
            var iface3 = device.GetInterface("GigabitEthernet0/0/0.100");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            
            // All should refer to the same interface
            Assert.Same(iface1, iface2);
            Assert.Same(iface2, iface3);
        }
    }
}
