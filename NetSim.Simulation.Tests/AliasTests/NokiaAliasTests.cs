using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Nokia command aliases and interface notation variations.
    /// </summary>
    public class NokiaAliasTests
    {
        [Fact]
        public void Nokia_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new NokiaDevice("R1");
            var full = device.ProcessCommand("show running-config");
            var alias = device.ProcessCommand("sh run");
            Assert.Equal(full, alias);
        }

        [Fact]
        public void Nokia_InterfaceNotationVariation_ShouldMatch()
        {
            var device = new NokiaDevice("R1");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface 1/1/1");
            device.ProcessCommand("address 10.1.1.1/24");
            device.ProcessCommand("no shutdown");
            device.ProcessCommand("exit");
            device.ProcessCommand("exit");

            var iface = device.GetInterface("1/1/1");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = device.ProcessCommand("show interface 1/1/1");
            var alias = device.ProcessCommand("show interface 1/1/c1");
            Assert.Equal(full, alias);
        }
    }
}
