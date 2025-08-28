using Xunit;
using NetForge.Simulation.CliHandlers.Dell;
using NetForge.Simulation.Devices;

namespace NetForge.Simulation.Tests.AliasTests
{
    public class DellAliasTests
    {
        [Theory]
        [InlineData("eth 1/1/1", "ethernet 1/1/1")]
        [InlineData("e 1/1/1", "ethernet 1/1/1")]
        [InlineData("eth 1/1/2", "ethernet 1/1/2")]
        [InlineData("e 1/2/1", "ethernet 1/2/1")]
        [InlineData("mgmt 1/1/1", "mgmt 1/1/1")]
        [InlineData("management 1/1/1", "mgmt 1/1/1")]
        [InlineData("ma 1/1/1", "mgmt 1/1/1")]
        [InlineData("po 1", "port-channel 1")]
        [InlineData("pc 1", "port-channel 1")]
        [InlineData("port 1", "port-channel 1")]
        [InlineData("portchannel 1", "port-channel 1")]
        [InlineData("vl 100", "vlan 100")]
        [InlineData("vlan 100", "vlan 100")]
        [InlineData("lo 0", "loopback 0")]
        [InlineData("loop 0", "loopback 0")]
        [InlineData("loopback 0", "loopback 0")]
        [InlineData("tu 1", "tunnel 1")]
        [InlineData("tunnel 1", "tunnel 1")]
        [InlineData("nu 0", "null 0")]
        [InlineData("null 0", "null 0")]
        [InlineData("te 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("ten 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("tengig 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("25g 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("25ge 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("40g 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("40ge 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("100g 1/1/1", "hundredgigabitethernet 1/1/1")]
        [InlineData("100ge 1/1/1", "hundredgigabitethernet 1/1/1")]
        public void ExpandInterfaceAlias_WithValidAliases_ReturnsExpandedName(string input, string expected)
        {
            var result = DellInterfaceAliasHandler.ExpandInterfaceAlias(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("ethernet 1/1/1", "e 1/1/1")]
        [InlineData("mgmt 1/1/1", "ma 1/1/1")]
        [InlineData("port-channel 1", "po 1")]
        [InlineData("vlan 100", "vl 100")]
        [InlineData("loopback 0", "lo 0")]
        [InlineData("tunnel 1", "tu 1")]
        [InlineData("null 0", "nu 0")]
        [InlineData("tengigabitethernet 1/1/1", "te 1/1/1")]
        [InlineData("twentyfivegigabitethernet 1/1/1", "25g 1/1/1")]
        [InlineData("fortygigabitethernet 1/1/1", "40g 1/1/1")]
        [InlineData("hundredgigabitethernet 1/1/1", "100g 1/1/1")]
        public void CompressInterfaceName_WithValidNames_ReturnsShortestAlias(string input, string expected)
        {
            var result = DellInterfaceAliasHandler.CompressInterfaceName(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("eth 1/1/1")]
        [InlineData("e 1/1/1")]
        [InlineData("ethernet 1/1/1")]
        [InlineData("mgmt 1/1/1")]
        [InlineData("management 1/1/1")]
        [InlineData("ma 1/1/1")]
        [InlineData("po 1")]
        [InlineData("pc 1")]
        [InlineData("port 1")]
        [InlineData("portchannel 1")]
        [InlineData("port-channel 1")]
        [InlineData("vl 100")]
        [InlineData("vlan 100")]
        [InlineData("lo 0")]
        [InlineData("loop 0")]
        [InlineData("loopback 0")]
        [InlineData("tu 1")]
        [InlineData("tunnel 1")]
        [InlineData("nu 0")]
        [InlineData("null 0")]
        [InlineData("te 1/1/1")]
        [InlineData("ten 1/1/1")]
        [InlineData("tengig 1/1/1")]
        [InlineData("tengigabit 1/1/1")]
        [InlineData("tengigabitethernet 1/1/1")]
        [InlineData("25g 1/1/1")]
        [InlineData("25ge 1/1/1")]
        [InlineData("twentyfive 1/1/1")]
        [InlineData("twentyfivegigabit 1/1/1")]
        [InlineData("twentyfivegigabitethernet 1/1/1")]
        [InlineData("40g 1/1/1")]
        [InlineData("40ge 1/1/1")]
        [InlineData("forty 1/1/1")]
        [InlineData("fortygigabit 1/1/1")]
        [InlineData("fortygigabitethernet 1/1/1")]
        [InlineData("100g 1/1/1")]
        [InlineData("100ge 1/1/1")]
        [InlineData("hundred 1/1/1")]
        [InlineData("hundredgigabit 1/1/1")]
        [InlineData("hundredgigabitethernet 1/1/1")]
        public void IsValidInterfaceName_WithValidNames_ReturnsTrue(string input)
        {
            var result = DellInterfaceAliasHandler.IsValidInterfaceName(input);
            Assert.True(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("invalid")]
        [InlineData("xyz123")]
        [InlineData("eth")]
        [InlineData("mgmt")]
        [InlineData("123")]
        [InlineData("eth ")]
        [InlineData("InvalidInterface")]
        public void IsValidInterfaceName_WithInvalidNames_ReturnsFalse(string input)
        {
            var result = DellInterfaceAliasHandler.IsValidInterfaceName(input);
            Assert.False(result);
        }

        [Theory]
        [InlineData("eth 1/1/1", "ethernet 1/1/1")]
        [InlineData("ethernet 1/1/1", "eth 1/1/1")]
        [InlineData("e 1/1/1", "ethernet 1/1/1")]
        [InlineData("ethernet 1/1/1", "e 1/1/1")]
        [InlineData("mgmt 1/1/1", "management 1/1/1")]
        [InlineData("management 1/1/1", "mgmt 1/1/1")]
        [InlineData("ma 1/1/1", "mgmt 1/1/1")]
        [InlineData("mgmt 1/1/1", "ma 1/1/1")]
        [InlineData("po 1", "port-channel 1")]
        [InlineData("port-channel 1", "po 1")]
        [InlineData("pc 1", "port-channel 1")]
        [InlineData("port-channel 1", "pc 1")]
        [InlineData("port 1", "portchannel 1")]
        [InlineData("portchannel 1", "port 1")]
        [InlineData("vl 100", "vlan 100")]
        [InlineData("vlan 100", "vl 100")]
        [InlineData("lo 0", "loopback 0")]
        [InlineData("loopback 0", "lo 0")]
        [InlineData("loop 0", "loopback 0")]
        [InlineData("loopback 0", "loop 0")]
        [InlineData("tu 1", "tunnel 1")]
        [InlineData("tunnel 1", "tu 1")]
        [InlineData("nu 0", "null 0")]
        [InlineData("null 0", "nu 0")]
        [InlineData("te 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("tengigabitethernet 1/1/1", "te 1/1/1")]
        [InlineData("ten 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("tengigabitethernet 1/1/1", "ten 1/1/1")]
        [InlineData("25g 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("twentyfivegigabitethernet 1/1/1", "25g 1/1/1")]
        [InlineData("25ge 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("twentyfivegigabitethernet 1/1/1", "25ge 1/1/1")]
        [InlineData("40g 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("fortygigabitethernet 1/1/1", "40g 1/1/1")]
        [InlineData("40ge 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("fortygigabitethernet 1/1/1", "40ge 1/1/1")]
        [InlineData("100g 1/1/1", "hundredgigabitethernet 1/1/1")]
        [InlineData("hundredgigabitethernet 1/1/1", "100g 1/1/1")]
        [InlineData("100ge 1/1/1", "hundredgigabitethernet 1/1/1")]
        [InlineData("hundredgigabitethernet 1/1/1", "100ge 1/1/1")]
        public void AreEquivalentInterfaceNames_WithEquivalentNames_ReturnsTrue(string name1, string name2)
        {
            var result = DellInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);
            Assert.True(result);
        }

        [Theory]
        [InlineData("eth 1/1/1", "mgmt 1/1/1")]
        [InlineData("eth 1/1/1", "po 1")]
        [InlineData("eth 1/1/1", "vlan 100")]
        [InlineData("po 1", "po 2")]
        [InlineData("vlan 100", "vlan 200")]
        [InlineData("lo 0", "lo 1")]
        [InlineData("ethernet 1/1/1", "ethernet 1/1/2")]
        [InlineData("mgmt 1/1/1", "mgmt 1/1/2")]
        [InlineData("te 1/1/1", "te 1/1/2")]
        [InlineData("25g 1/1/1", "25g 1/1/2")]
        [InlineData("40g 1/1/1", "40g 1/1/2")]
        [InlineData("100g 1/1/1", "100g 1/1/2")]
        public void AreEquivalentInterfaceNames_WithNonEquivalentNames_ReturnsFalse(string name1, string name2)
        {
            var result = DellInterfaceAliasHandler.AreEquivalentInterfaceNames(name1, name2);
            Assert.False(result);
        }

        [Theory]
        [InlineData("eth 1/1/1", new[] { "ethernet 1/1/1", "eth 1/1/1", "e 1/1/1" })]
        [InlineData("mgmt 1/1/1", new[] { "mgmt 1/1/1", "management 1/1/1", "ma 1/1/1" })]
        [InlineData("po 1", new[] { "port-channel 1", "po 1", "pc 1", "port 1", "portchannel 1" })]
        [InlineData("vlan 100", new[] { "vlan 100", "vl 100" })]
        [InlineData("loopback 0", new[] { "loopback 0", "lo 0", "loop 0" })]
        [InlineData("tunnel 1", new[] { "tunnel 1", "tu 1" })]
        [InlineData("null 0", new[] { "null 0", "nu 0" })]
        [InlineData("tengigabitethernet 1/1/1", new[] { "tengigabitethernet 1/1/1", "te 1/1/1", "ten 1/1/1", "tengig 1/1/1", "tengigabit 1/1/1" })]
        [InlineData("twentyfivegigabitethernet 1/1/1", new[] { "twentyfivegigabitethernet 1/1/1", "25g 1/1/1", "25ge 1/1/1", "twentyfive 1/1/1", "twentyfivegigabit 1/1/1" })]
        [InlineData("fortygigabitethernet 1/1/1", new[] { "fortygigabitethernet 1/1/1", "40g 1/1/1", "40ge 1/1/1", "forty 1/1/1", "fortygigabit 1/1/1" })]
        [InlineData("hundredgigabitethernet 1/1/1", new[] { "hundredgigabitethernet 1/1/1", "100g 1/1/1", "100ge 1/1/1", "hundred 1/1/1", "hundredgigabit 1/1/1" })]
        public void GetInterfaceAliases_WithValidInterface_ReturnsAllAliases(string interfaceName, string[] expectedAliases)
        {
            var result = DellInterfaceAliasHandler.GetInterfaceAliases(interfaceName);

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
            var result = DellInterfaceAliasHandler.GetInterfaceAliases(interfaceName);
            Assert.Equal(expectedAliases, result);
        }

        [Theory]
        [InlineData("eth 1/1/1", "ethernet")]
        [InlineData("e 1/1/1", "ethernet")]
        [InlineData("ethernet 1/1/1", "ethernet")]
        [InlineData("mgmt 1/1/1", "mgmt")]
        [InlineData("management 1/1/1", "mgmt")]
        [InlineData("ma 1/1/1", "mgmt")]
        [InlineData("po 1", "port-channel")]
        [InlineData("pc 1", "port-channel")]
        [InlineData("port 1", "port-channel")]
        [InlineData("portchannel 1", "port-channel")]
        [InlineData("port-channel 1", "port-channel")]
        [InlineData("vl 100", "vlan")]
        [InlineData("vlan 100", "vlan")]
        [InlineData("lo 0", "loopback")]
        [InlineData("loop 0", "loopback")]
        [InlineData("loopback 0", "loopback")]
        [InlineData("tu 1", "tunnel")]
        [InlineData("tunnel 1", "tunnel")]
        [InlineData("nu 0", "null")]
        [InlineData("null 0", "null")]
        [InlineData("te 1/1/1", "tengigabitethernet")]
        [InlineData("ten 1/1/1", "tengigabitethernet")]
        [InlineData("tengig 1/1/1", "tengigabitethernet")]
        [InlineData("tengigabit 1/1/1", "tengigabitethernet")]
        [InlineData("tengigabitethernet 1/1/1", "tengigabitethernet")]
        [InlineData("25g 1/1/1", "twentyfivegigabitethernet")]
        [InlineData("25ge 1/1/1", "twentyfivegigabitethernet")]
        [InlineData("twentyfive 1/1/1", "twentyfivegigabitethernet")]
        [InlineData("twentyfivegigabit 1/1/1", "twentyfivegigabitethernet")]
        [InlineData("twentyfivegigabitethernet 1/1/1", "twentyfivegigabitethernet")]
        [InlineData("40g 1/1/1", "fortygigabitethernet")]
        [InlineData("40ge 1/1/1", "fortygigabitethernet")]
        [InlineData("forty 1/1/1", "fortygigabitethernet")]
        [InlineData("fortygigabit 1/1/1", "fortygigabitethernet")]
        [InlineData("fortygigabitethernet 1/1/1", "fortygigabitethernet")]
        [InlineData("100g 1/1/1", "hundredgigabitethernet")]
        [InlineData("100ge 1/1/1", "hundredgigabitethernet")]
        [InlineData("hundred 1/1/1", "hundredgigabitethernet")]
        [InlineData("hundredgigabit 1/1/1", "hundredgigabitethernet")]
        [InlineData("hundredgigabitethernet 1/1/1", "hundredgigabitethernet")]
        public void GetInterfaceType_WithValidInterface_ReturnsCorrectType(string interfaceName, string expectedType)
        {
            var result = DellInterfaceAliasHandler.GetInterfaceType(interfaceName);
            Assert.Equal(expectedType, result);
        }

        [Theory]
        [InlineData("eth 1/1/1", "1/1/1")]
        [InlineData("ethernet 1/2/1", "1/2/1")]
        [InlineData("mgmt 1/1/1", "1/1/1")]
        [InlineData("po 1", "1")]
        [InlineData("po 10", "10")]
        [InlineData("vlan 100", "100")]
        [InlineData("vlan 200", "200")]
        [InlineData("lo 0", "0")]
        [InlineData("lo 1", "1")]
        [InlineData("tu 1", "1")]
        [InlineData("tu 10", "10")]
        [InlineData("nu 0", "0")]
        [InlineData("te 1/1/1", "1/1/1")]
        [InlineData("te 1/2/1", "1/2/1")]
        [InlineData("25g 1/1/1", "1/1/1")]
        [InlineData("25g 1/2/1", "1/2/1")]
        [InlineData("40g 1/1/1", "1/1/1")]
        [InlineData("40g 1/2/1", "1/2/1")]
        [InlineData("100g 1/1/1", "1/1/1")]
        [InlineData("100g 1/2/1", "1/2/1")]
        public void GetInterfaceNumber_WithValidInterface_ReturnsCorrectNumber(string interfaceName, string expectedNumber)
        {
            var result = DellInterfaceAliasHandler.GetInterfaceNumber(interfaceName);
            Assert.Equal(expectedNumber, result);
        }

        [Theory]
        [InlineData("eth 1/1/1", "ethernet 1/1/1")]
        [InlineData("e 1/1/1", "ethernet 1/1/1")]
        [InlineData("ethernet 1/1/1", "ethernet 1/1/1")]
        [InlineData("mgmt 1/1/1", "mgmt 1/1/1")]
        [InlineData("management 1/1/1", "mgmt 1/1/1")]
        [InlineData("ma 1/1/1", "mgmt 1/1/1")]
        [InlineData("po 1", "port-channel 1")]
        [InlineData("pc 1", "port-channel 1")]
        [InlineData("port 1", "port-channel 1")]
        [InlineData("portchannel 1", "port-channel 1")]
        [InlineData("port-channel 1", "port-channel 1")]
        [InlineData("vl 100", "vlan 100")]
        [InlineData("vlan 100", "vlan 100")]
        [InlineData("lo 0", "loopback 0")]
        [InlineData("loop 0", "loopback 0")]
        [InlineData("loopback 0", "loopback 0")]
        [InlineData("tu 1", "tunnel 1")]
        [InlineData("tunnel 1", "tunnel 1")]
        [InlineData("nu 0", "null 0")]
        [InlineData("null 0", "null 0")]
        [InlineData("te 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("ten 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("tengig 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("tengigabit 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("tengigabitethernet 1/1/1", "tengigabitethernet 1/1/1")]
        [InlineData("25g 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("25ge 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("twentyfive 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("twentyfivegigabit 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("twentyfivegigabitethernet 1/1/1", "twentyfivegigabitethernet 1/1/1")]
        [InlineData("40g 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("40ge 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("forty 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("fortygigabit 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("fortygigabitethernet 1/1/1", "fortygigabitethernet 1/1/1")]
        [InlineData("100g 1/1/1", "hundredgigabitethernet 1/1/1")]
        [InlineData("100ge 1/1/1", "hundredgigabitethernet 1/1/1")]
        [InlineData("hundred 1/1/1", "hundredgigabitethernet 1/1/1")]
        [InlineData("hundredgigabit 1/1/1", "hundredgigabitethernet 1/1/1")]
        [InlineData("hundredgigabitethernet 1/1/1", "hundredgigabitethernet 1/1/1")]
        public void GetCanonicalInterfaceName_WithValidInterface_ReturnsCanonicalName(string input, string expected)
        {
            var result = DellInterfaceAliasHandler.GetCanonicalInterfaceName(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExpandInterfaceAlias_WithNullOrEmpty_ReturnsOriginal()
        {
            Assert.Null(DellInterfaceAliasHandler.ExpandInterfaceAlias(null));
            Assert.Equal("", DellInterfaceAliasHandler.ExpandInterfaceAlias(""));
        }

        [Fact]
        public void CompressInterfaceName_WithNullOrEmpty_ReturnsOriginal()
        {
            Assert.Null(DellInterfaceAliasHandler.CompressInterfaceName(null));
            Assert.Equal("", DellInterfaceAliasHandler.CompressInterfaceName(""));
        }

        [Fact]
        public void AreEquivalentInterfaceNames_WithNullOrEmpty_ReturnsFalse()
        {
            Assert.False(DellInterfaceAliasHandler.AreEquivalentInterfaceNames(null, "eth 1/1/1"));
            Assert.False(DellInterfaceAliasHandler.AreEquivalentInterfaceNames("eth 1/1/1", null));
            Assert.False(DellInterfaceAliasHandler.AreEquivalentInterfaceNames("", "eth 1/1/1"));
            Assert.False(DellInterfaceAliasHandler.AreEquivalentInterfaceNames("eth 1/1/1", ""));
        }

        [Fact]
        public void Dell_DeviceIntegration_ShouldWork()
        {
            // Create a Dell device
            var device = new DellDevice("SW1");

            // Add interface using full name
            device.AddInterface("ethernet 1/1/1");

            // Should be able to find it using alias
            var iface1 = device.GetInterface("eth 1/1/1");
            var iface2 = device.GetInterface("e 1/1/1");
            var iface3 = device.GetInterface("ethernet 1/1/1");

            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.Same(iface1, iface2);
            Assert.Same(iface1, iface3);
        }

        [Fact]
        public void Dell_PortChannelAlias_ShouldWork()
        {
            // Create a Dell device
            var device = new DellDevice("SW1");

            // Add Port-Channel interface using full name
            device.AddInterface("port-channel 1");

            // Should be able to find it using alias
            var iface1 = device.GetInterface("po 1");
            var iface2 = device.GetInterface("pc 1");
            var iface3 = device.GetInterface("port 1");
            var iface4 = device.GetInterface("portchannel 1");
            var iface5 = device.GetInterface("port-channel 1");

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
        public void Dell_ManagementAlias_ShouldWork()
        {
            // Create a Dell device
            var device = new DellDevice("SW1");

            // Add Management interface using full name
            device.AddInterface("mgmt 1/1/1");

            // Should be able to find it using aliases
            var iface1 = device.GetInterface("mgmt 1/1/1");
            var iface2 = device.GetInterface("management 1/1/1");
            var iface3 = device.GetInterface("ma 1/1/1");

            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.Same(iface1, iface2);
            Assert.Same(iface1, iface3);
        }

        [Fact]
        public void Dell_VlanAlias_ShouldWork()
        {
            // Create a Dell device
            var device = new DellDevice("SW1");

            // Add VLAN interface using full name
            device.AddInterface("vlan 100");

            // Should be able to find it using alias
            var iface1 = device.GetInterface("vl 100");
            var iface2 = device.GetInterface("vlan 100");

            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.Same(iface1, iface2);
        }

        [Fact]
        public void Dell_LoopbackAlias_ShouldWork()
        {
            // Create a Dell device
            var device = new DellDevice("SW1");

            // Add Loopback interface using full name
            device.AddInterface("loopback 0");

            // Should be able to find it using aliases
            var iface1 = device.GetInterface("lo 0");
            var iface2 = device.GetInterface("loop 0");
            var iface3 = device.GetInterface("loopback 0");

            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.Same(iface1, iface2);
            Assert.Same(iface1, iface3);
        }

        [Fact]
        public void Dell_HighSpeedEthernetAlias_ShouldWork()
        {
            // Create a Dell device
            var device = new DellDevice("SW1");

            // Add high-speed ethernet interfaces
            device.AddInterface("tengigabitethernet 1/1/1");
            device.AddInterface("twentyfivegigabitethernet 1/1/2");
            device.AddInterface("fortygigabitethernet 1/1/3");
            device.AddInterface("hundredgigabitethernet 1/1/4");

            // Should be able to find them using aliases
            var iface1 = device.GetInterface("te 1/1/1");
            var iface2 = device.GetInterface("25g 1/1/2");
            var iface3 = device.GetInterface("40g 1/1/3");
            var iface4 = device.GetInterface("100g 1/1/4");

            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.NotNull(iface4);
        }

        [Fact]
        public void Dell_TunnelAndNullAlias_ShouldWork()
        {
            // Create a Dell device
            var device = new DellDevice("SW1");

            // Add tunnel and null interfaces
            device.AddInterface("tunnel 1");
            device.AddInterface("null 0");

            // Should be able to find them using aliases
            var iface1 = device.GetInterface("tu 1");
            var iface2 = device.GetInterface("tunnel 1");
            var iface3 = device.GetInterface("nu 0");
            var iface4 = device.GetInterface("null 0");

            Assert.NotNull(iface1);
            Assert.NotNull(iface2);
            Assert.NotNull(iface3);
            Assert.NotNull(iface4);
            Assert.Same(iface1, iface2);
            Assert.Same(iface3, iface4);
        }
    }
}
