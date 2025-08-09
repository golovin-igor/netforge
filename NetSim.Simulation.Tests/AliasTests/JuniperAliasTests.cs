using System;
using System.Linq;
using Xunit;
using NetSim.Simulation.CliHandlers.Juniper;
using NetSim.Simulation.Devices;

namespace NetSim.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Juniper alias support for show commands and interface names.
    /// </summary>
    public class JuniperAliasTests
    {
        [Fact]
        public void Juniper_ShowConfigurationAlias_ShouldMatchFullCommand()
        {
            var device = new JuniperDevice("R1");
            var full = device.ProcessCommand("show configuration");
            var alias = device.ProcessCommand("sh conf");
            Assert.Equal(full, alias);
        }

        [Fact]
        public void Juniper_InterfaceNameCase_ShouldBeAccepted()
        {
            var device = new JuniperDevice("R1");
            device.ProcessCommand("configure");
            device.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.1.1.1/24");
            device.ProcessCommand("commit");

            var iface = device.GetInterface("ge-0/0/0");
            Assert.Equal("10.1.1.1", iface.IpAddress);

            var full = device.ProcessCommand("show interfaces ge-0/0/0");
            var alias = device.ProcessCommand("show interfaces GE-0/0/0");
            Assert.Equal(full, alias);
        }

        [Theory]
        [InlineData("ge-0/0/0", "GigabitEthernet-0/0/0")]
        [InlineData("ge-1/0/0", "GigabitEthernet-1/0/0")]
        [InlineData("xe-0/0/0", "TenGigabitEthernet-0/0/0")]
        [InlineData("xe-1/0/0", "TenGigabitEthernet-1/0/0")]
        [InlineData("et-0/0/0", "EthernetInterface-0/0/0")]
        [InlineData("et-1/0/0", "EthernetInterface-1/0/0")]
        [InlineData("ae0", "AggregatedEthernet0")]
        [InlineData("ae1", "AggregatedEthernet1")]
        [InlineData("ae10", "AggregatedEthernet10")]
        [InlineData("irb0", "IRB0")]
        [InlineData("irb1", "IRB1")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("lo1", "Loopback1")]
        [InlineData("me0", "Management0")]
        [InlineData("me1", "Management1")]
        [InlineData("fxp0", "Management0")]
        [InlineData("em0", "Management0")]
        [InlineData("vlan.100", "VLAN.100")]
        [InlineData("vlan.200", "VLAN.200")]
        [InlineData("reth0", "RedundantEthernet0")]
        [InlineData("reth1", "RedundantEthernet1")]
        [InlineData("gr-0/0/0", "GRE-0/0/0")]
        [InlineData("st0", "STunnel0")]
        [InlineData("100ge-0/0/0", "HundredGigabitEthernet-0/0/0")]
        [InlineData("25ge-0/0/0", "TwentyFiveGigabitEthernet-0/0/0")]
        [InlineData("40ge-0/0/0", "FortyGigabitEthernet-0/0/0")]
        public void ExpandInterfaceAlias_WithValidAliases_ReturnsExpandedName(string input, string expected)
        {
            var result = JuniperInterfaceAliasHandler.ExpandInterfaceAlias(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("GigabitEthernet-0/0/0", "ge-0/0/0")]
        [InlineData("TenGigabitEthernet-0/0/0", "xe-0/0/0")]
        [InlineData("EthernetInterface-0/0/0", "et-0/0/0")]
        [InlineData("AggregatedEthernet0", "ae0")]
        [InlineData("IRB0", "irb0")]
        [InlineData("Loopback0", "lo0")]
        [InlineData("Management0", "me0")]
        [InlineData("VLAN.100", "vlan100")]
        [InlineData("RedundantEthernet0", "reth0")]
        [InlineData("GRE-0/0/0", "gr-0/0/0")]
        [InlineData("STunnel0", "st0")]
        [InlineData("HundredGigabitEthernet-0/0/0", "100ge-0/0/0")]
        [InlineData("TwentyFiveGigabitEthernet-0/0/0", "25ge-0/0/0")]
        [InlineData("FortyGigabitEthernet-0/0/0", "40ge-0/0/0")]
        public void CompressInterfaceName_WithValidNames_ReturnsShortestAlias(string input, string expected)
        {
            var result = JuniperInterfaceAliasHandler.CompressInterfaceName(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("ge-0/0/0")]
        [InlineData("xe-0/0/0")]
        [InlineData("et-0/0/0")]
        [InlineData("ae0")]
        [InlineData("ae10")]
        [InlineData("irb0")]
        [InlineData("lo0")]
        [InlineData("me0")]
        [InlineData("fxp0")]
        [InlineData("em0")]
        [InlineData("vlan.100")]
        [InlineData("reth0")]
        [InlineData("gr-0/0/0")]
        [InlineData("st0")]
        [InlineData("100ge-0/0/0")]
        [InlineData("25ge-0/0/0")]
        [InlineData("40ge-0/0/0")]
        [InlineData("GigabitEthernet-0/0/0")]
        [InlineData("TenGigabitEthernet-0/0/0")]
        [InlineData("EthernetInterface-0/0/0")]
        [InlineData("AggregatedEthernet0")]
        [InlineData("IRB0")]
        [InlineData("Loopback0")]
        [InlineData("Management0")]
        [InlineData("VLAN.100")]
        [InlineData("RedundantEthernet0")]
        [InlineData("GRE-0/0/0")]
        [InlineData("STunnel0")]
        [InlineData("HundredGigabitEthernet-0/0/0")]
        [InlineData("TwentyFiveGigabitEthernet-0/0/0")]
        [InlineData("FortyGigabitEthernet-0/0/0")]
        public void IsValidInterfaceName_WithValidNames_ReturnsTrue(string input)
        {
            var result = JuniperInterfaceAliasHandler.IsValidInterfaceName(input);
            Assert.True(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("invalid")]
        [InlineData("xyz123")]
        [InlineData("ge")]
        [InlineData("xe")]
        [InlineData("123")]
        [InlineData("ge-")]
        [InlineData("xe-")]
        [InlineData("InvalidInterface")]
        public void IsValidInterfaceName_WithInvalidNames_ReturnsFalse(string input)
        {
            var result = JuniperInterfaceAliasHandler.IsValidInterfaceName(input);
            Assert.False(result);
        }

        [Theory]
        [InlineData("ge-0/0/0", "GigabitEthernet-0/0/0")]
        [InlineData("GigabitEthernet-0/0/0", "ge-0/0/0")]
        [InlineData("xe-0/0/0", "TenGigabitEthernet-0/0/0")]
        [InlineData("TenGigabitEthernet-0/0/0", "xe-0/0/0")]
        [InlineData("et-0/0/0", "EthernetInterface-0/0/0")]
        [InlineData("EthernetInterface-0/0/0", "et-0/0/0")]
        [InlineData("ae0", "AggregatedEthernet0")]
        [InlineData("AggregatedEthernet0", "ae0")]
        [InlineData("irb0", "IRB0")]
        [InlineData("IRB0", "irb0")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("Loopback0", "lo0")]
        [InlineData("me0", "Management0")]
        [InlineData("Management0", "me0")]
        [InlineData("fxp0", "Management0")]
        [InlineData("Management0", "fxp0")]
        [InlineData("em0", "Management0")]
        [InlineData("Management0", "em0")]
        [InlineData("vlan.100", "VLAN.100")]
        [InlineData("VLAN.100", "vlan.100")]
        [InlineData("reth0", "RedundantEthernet0")]
        [InlineData("RedundantEthernet0", "reth0")]
        [InlineData("gr-0/0/0", "GRE-0/0/0")]
        [InlineData("GRE-0/0/0", "gr-0/0/0")]
        [InlineData("st0", "STunnel0")]
        [InlineData("STunnel0", "st0")]
        [InlineData("100ge-0/0/0", "HundredGigabitEthernet-0/0/0")]
        [InlineData("HundredGigabitEthernet-0/0/0", "100ge-0/0/0")]
        [InlineData("25ge-0/0/0", "TwentyFiveGigabitEthernet-0/0/0")]
        [InlineData("TwentyFiveGigabitEthernet-0/0/0", "25ge-0/0/0")]
        [InlineData("40ge-0/0/0", "FortyGigabitEthernet-0/0/0")]
        [InlineData("FortyGigabitEthernet-0/0/0", "40ge-0/0/0")]
        public void AreEquivalentInterfaceNames_WithEquivalentNames_ReturnsTrue(string name1, string name2)
        {
            var result = JuniperInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);
            Assert.True(result);
        }

        [Theory]
        [InlineData("ge-0/0/0", "xe-0/0/0")]
        [InlineData("ge-0/0/0", "et-0/0/0")]
        [InlineData("ae0", "ae1")]
        [InlineData("irb0", "irb1")]
        [InlineData("lo0", "lo1")]
        [InlineData("me0", "me1")]
        [InlineData("ge-0/0/0", "ge-0/0/1")]
        [InlineData("xe-0/0/0", "xe-0/0/1")]
        [InlineData("AggregatedEthernet0", "AggregatedEthernet1")]
        [InlineData("IRB0", "IRB1")]
        [InlineData("Loopback0", "Loopback1")]
        [InlineData("Management0", "Management1")]
        [InlineData("VLAN.100", "VLAN.200")]
        [InlineData("RedundantEthernet0", "RedundantEthernet1")]
        [InlineData("GRE-0/0/0", "GRE-0/0/1")]
        [InlineData("STunnel0", "STunnel1")]
        [InlineData("100ge-0/0/0", "100ge-0/0/1")]
        [InlineData("25ge-0/0/0", "25ge-0/0/1")]
        [InlineData("40ge-0/0/0", "40ge-0/0/1")]
        public void AreEquivalentInterfaceNames_WithNonEquivalentNames_ReturnsFalse(string name1, string name2)
        {
            var result = JuniperInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);
            Assert.False(result);
        }

        [Theory]
        [InlineData("ge-0/0/0", new[] { "GigabitEthernet-0/0/0", "ge-0/0/0", "gig-0/0/0", "gigabit-0/0/0", "gigabitethernet-0/0/0" })]
        [InlineData("xe-0/0/0", new[] { "TenGigabitEthernet-0/0/0", "xe-0/0/0", "xge-0/0/0", "tengig-0/0/0", "tengigabit-0/0/0", "tengigabitethernet-0/0/0" })]
        [InlineData("et-0/0/0", new[] { "EthernetInterface-0/0/0", "et-0/0/0", "ether-0/0/0" })]
        [InlineData("ae0", new[] { "AggregatedEthernet0", "ae0", "agg0", "aggregated0", "aggregatedethernet0" })]
        [InlineData("irb0", new[] { "IRB0", "irb0" })]
        [InlineData("lo0", new[] { "Loopback0", "lo0", "loop0", "loopback0" })]
        [InlineData("me0", new[] { "Management0", "me0", "mgmt0", "management0", "fxp0", "em0" })]
        [InlineData("vlan.100", new[] { "VLAN.100", "vlan100", "vl100" })]
        [InlineData("reth0", new[] { "RedundantEthernet0", "reth0", "redundant0" })]
        [InlineData("gr-0/0/0", new[] { "GRE-0/0/0", "gr-0/0/0", "gre-0/0/0" })]
        [InlineData("st0", new[] { "STunnel0", "st0", "tunnel0" })]
        [InlineData("100ge-0/0/0", new[] { "HundredGigabitEthernet-0/0/0", "100ge-0/0/0", "hundred-0/0/0", "hundredgigabit-0/0/0", "hundredgigabitethernet-0/0/0" })]
        [InlineData("25ge-0/0/0", new[] { "TwentyFiveGigabitEthernet-0/0/0", "25ge-0/0/0" })]
        [InlineData("40ge-0/0/0", new[] { "FortyGigabitEthernet-0/0/0", "40ge-0/0/0", "forty-0/0/0", "fortygigabit-0/0/0", "fortygigabitethernet-0/0/0" })]
        public void GetInterfaceAliases_WithValidInterface_ReturnsAllAliases(string interfaceName, string[] expectedAliases)
        {
            var result = JuniperInterfaceAliasHandler.GetInterfaceAliases(interfaceName);
            
            // Check that all expected aliases are present
            foreach (var expected in expectedAliases)
            {
                Assert.Contains(expected, result, StringComparer.OrdinalIgnoreCase);
            }
        }

        [Theory]
        [InlineData("", new[] { "" })]
        [InlineData("invalid", new[] { "invalid" })]
        [InlineData("xyz123", new[] { "xyz123" })]
        public void GetInterfaceAliases_WithInvalidInterface_ReturnsOriginalOrEmpty(string interfaceName, string[] expectedAliases)
        {
            var result = JuniperInterfaceAliasHandler.GetInterfaceAliases(interfaceName);
            Assert.Equal(expectedAliases, result);
        }

        [Theory]
        [InlineData("ge-0/0/0", "GigabitEthernet")]
        [InlineData("xe-0/0/0", "TenGigabitEthernet")]
        [InlineData("et-0/0/0", "EthernetInterface")]
        [InlineData("ae0", "AggregatedEthernet")]
        [InlineData("irb0", "IRB")]
        [InlineData("lo0", "Loopback")]
        [InlineData("me0", "Management")]
        [InlineData("fxp0", "Management")]
        [InlineData("em0", "Management")]
        [InlineData("vlan.100", "VLAN")]
        [InlineData("reth0", "RedundantEthernet")]
        [InlineData("gr-0/0/0", "GRE")]
        [InlineData("st0", "STunnel")]
        [InlineData("100ge-0/0/0", "HundredGigabitEthernet")]
        [InlineData("25ge-0/0/0", "TwentyFiveGigabitEthernet")]
        [InlineData("40ge-0/0/0", "FortyGigabitEthernet")]
        public void GetInterfaceType_WithValidInterface_ReturnsCorrectType(string interfaceName, string expectedType)
        {
            var result = JuniperInterfaceAliasHandler.GetInterfaceType(interfaceName);
            Assert.Equal(expectedType, result);
        }

        [Theory]
        [InlineData("ge-0/0/0", "0/0/0")]
        [InlineData("xe-1/0/0", "1/0/0")]
        [InlineData("et-2/0/0", "2/0/0")]
        [InlineData("ae0", "0")]
        [InlineData("ae10", "10")]
        [InlineData("irb0", "0")]
        [InlineData("lo0", "0")]
        [InlineData("me0", "0")]
        [InlineData("fxp0", "0")]
        [InlineData("em0", "0")]
        [InlineData("vlan.100", "100")]
        [InlineData("reth0", "0")]
        [InlineData("gr-0/0/0", "0/0/0")]
        [InlineData("st0", "0")]
        [InlineData("100ge-0/0/0", "0/0/0")]
        [InlineData("25ge-1/0/0", "1/0/0")]
        [InlineData("40ge-2/0/0", "2/0/0")]
        public void GetInterfaceNumber_WithValidInterface_ReturnsCorrectNumber(string interfaceName, string expectedNumber)
        {
            var result = JuniperInterfaceAliasHandler.GetInterfaceNumber(interfaceName);
            Assert.Equal(expectedNumber, result);
        }

        [Theory]
        [InlineData("ge-0/0/0", "GigabitEthernet-0/0/0")]
        [InlineData("xe-0/0/0", "TenGigabitEthernet-0/0/0")]
        [InlineData("et-0/0/0", "EthernetInterface-0/0/0")]
        [InlineData("ae0", "AggregatedEthernet0")]
        [InlineData("irb0", "IRB0")]
        [InlineData("lo0", "Loopback0")]
        [InlineData("me0", "Management0")]
        [InlineData("fxp0", "Management0")]
        [InlineData("em0", "Management0")]
        [InlineData("vlan.100", "VLAN.100")]
        [InlineData("reth0", "RedundantEthernet0")]
        [InlineData("gr-0/0/0", "GRE-0/0/0")]
        [InlineData("st0", "STunnel0")]
        [InlineData("100ge-0/0/0", "HundredGigabitEthernet-0/0/0")]
        [InlineData("25ge-0/0/0", "TwentyFiveGigabitEthernet-0/0/0")]
        [InlineData("40ge-0/0/0", "FortyGigabitEthernet-0/0/0")]
        [InlineData("GigabitEthernet-0/0/0", "GigabitEthernet-0/0/0")]
        [InlineData("TenGigabitEthernet-0/0/0", "TenGigabitEthernet-0/0/0")]
        [InlineData("EthernetInterface-0/0/0", "EthernetInterface-0/0/0")]
        [InlineData("AggregatedEthernet0", "AggregatedEthernet0")]
        [InlineData("IRB0", "IRB0")]
        [InlineData("Loopback0", "Loopback0")]
        [InlineData("Management0", "Management0")]
        [InlineData("VLAN.100", "VLAN.100")]
        [InlineData("RedundantEthernet0", "RedundantEthernet0")]
        [InlineData("GRE-0/0/0", "GRE-0/0/0")]
        [InlineData("STunnel0", "STunnel0")]
        [InlineData("HundredGigabitEthernet-0/0/0", "HundredGigabitEthernet-0/0/0")]
        [InlineData("TwentyFiveGigabitEthernet-0/0/0", "TwentyFiveGigabitEthernet-0/0/0")]
        [InlineData("FortyGigabitEthernet-0/0/0", "FortyGigabitEthernet-0/0/0")]
        public void GetCanonicalInterfaceName_WithValidInterface_ReturnsCanonicalName(string input, string expected)
        {
            var result = JuniperInterfaceAliasHandler.GetCanonicalInterfaceName(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExpandInterfaceAlias_WithNullOrEmpty_ReturnsOriginal()
        {
            Assert.Null(JuniperInterfaceAliasHandler.ExpandInterfaceAlias(null));
            Assert.Equal("", JuniperInterfaceAliasHandler.ExpandInterfaceAlias(""));
        }

        [Fact]
        public void CompressInterfaceName_WithNullOrEmpty_ReturnsOriginal()
        {
            Assert.Null(JuniperInterfaceAliasHandler.CompressInterfaceName(null));
            Assert.Equal("", JuniperInterfaceAliasHandler.CompressInterfaceName(""));
        }

        [Fact]
        public void AreEquivalentInterfaceNames_WithNullOrEmpty_ReturnsFalse()
        {
            Assert.False(JuniperInterfaceAliasHandler.AreEquivalentInterfaceNames(null, "ge-0/0/0"));
            Assert.False(JuniperInterfaceAliasHandler.AreEquivalentInterfaceNames("ge-0/0/0", null));
            Assert.False(JuniperInterfaceAliasHandler.AreEquivalentInterfaceNames("", "ge-0/0/0"));
            Assert.False(JuniperInterfaceAliasHandler.AreEquivalentInterfaceNames("ge-0/0/0", ""));
        }

        [Fact]
        public void Juniper_DeviceIntegration_ShouldWork()
        {
            // Create a Juniper device
            var device = new JuniperDevice("R1");
            
            // Add interface using full name
            device.AddInterface("GigabitEthernet-0/0/0");
            
            // Should be able to find it using alias
            var iface1 = device.GetInterface("ge-0/0/0");
            var iface2 = device.GetInterface("gig-0/0/0");
            var iface3 = device.GetInterface("GigabitEthernet-0/0/0");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.Same(iface1, iface2);
            Assert.Same(iface1, iface3);
        }

        [Fact]
        public void Juniper_AggregatedEthernetAlias_ShouldWork()
        {
            // Create a Juniper device
            var device = new JuniperDevice("R1");
            
            // Add AE interface using full name
            device.AddInterface("AggregatedEthernet0");
            
            // Should be able to find it using alias
            var iface1 = device.GetInterface("ae0");
            var iface2 = device.GetInterface("agg0");
            var iface3 = device.GetInterface("AggregatedEthernet0");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.Same(iface1, iface2);
            Assert.Same(iface1, iface3);
        }

        [Fact]
        public void Juniper_IRBAlias_ShouldWork()
        {
            // Create a Juniper device
            var device = new JuniperDevice("R1");
            
            // Add IRB interface using full name
            device.AddInterface("IRB0");
            
            // Should be able to find it using alias
            var iface1 = device.GetInterface("irb0");
            var iface2 = device.GetInterface("IRB0");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.Same(iface1, iface2);
        }

        [Fact]
        public void Juniper_LoopbackAlias_ShouldWork()
        {
            // Create a Juniper device
            var device = new JuniperDevice("R1");
            
            // Add Loopback interface using full name
            device.AddInterface("Loopback0");
            
            // Should be able to find it using alias
            var iface1 = device.GetInterface("lo0");
            var iface2 = device.GetInterface("loop0");
            var iface3 = device.GetInterface("Loopback0");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.Same(iface1, iface2);
            Assert.Same(iface1, iface3);
        }

        [Fact]
        public void Juniper_ManagementAlias_ShouldWork()
        {
            // Create a Juniper device
            var device = new JuniperDevice("R1");
            
            // Add Management interface using full name
            device.AddInterface("Management0");
            
            // Should be able to find it using aliases
            var iface1 = device.GetInterface("me0");
            var iface2 = device.GetInterface("mgmt0");
            var iface3 = device.GetInterface("fxp0");
            var iface4 = device.GetInterface("em0");
            var iface5 = device.GetInterface("Management0");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.NotNull(iface4);
            Assert.NotNull(iface5);
            Assert.Same(iface1, iface2);
            Assert.Same(iface1, iface3);
            Assert.Same(iface1, iface4);
            Assert.Same(iface1, iface5);
        }

        [Fact]
        public void Juniper_HighSpeedEthernetAlias_ShouldWork()
        {
            // Create a Juniper device
            var device = new JuniperDevice("R1");
            
            // Add high-speed ethernet interfaces
            device.AddInterface("HundredGigabitEthernet-0/0/0");
            device.AddInterface("TwentyFiveGigabitEthernet-0/0/1");
            device.AddInterface("FortyGigabitEthernet-0/0/2");
            
            // Should be able to find them using aliases
            var iface1 = device.GetInterface("100ge-0/0/0");
            var iface2 = device.GetInterface("25ge-0/0/1");
            var iface3 = device.GetInterface("40ge-0/0/2");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
        }

        [Fact]
        public void Juniper_TunnelAlias_ShouldWork()
        {
            // Create a Juniper device
            var device = new JuniperDevice("R1");
            
            // Add tunnel interfaces
            device.AddInterface("GRE-0/0/0");
            device.AddInterface("STunnel0");
            
            // Should be able to find them using aliases
            var iface1 = device.GetInterface("gr-0/0/0");
            var iface2 = device.GetInterface("gre-0/0/0");
            var iface3 = device.GetInterface("st0");
            var iface4 = device.GetInterface("tunnel0");
            
            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.NotNull(iface4);
            Assert.Same(iface1, iface2);
            Assert.Same(iface3, iface4);
        }
    }
}
