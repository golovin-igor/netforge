using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Nokia command aliases and interface notation variations.
    /// </summary>
    public class NokiaAliasTests
    {
        [Fact]
        public async Task Nokia_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new NokiaDevice("R1");
            var full = await device.ProcessCommandAsync("show running-config");
            var alias = await device.ProcessCommandAsync("sh run");
            Assert.Equal(full, alias);
        }

        [Fact]
        public async Task Nokia_InterfaceNotationVariation_ShouldMatch()
        {
            var device = new NokiaDevice("R1");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface 1/1/1");
            await device.ProcessCommandAsync("address 10.1.1.1/24");
            await device.ProcessCommandAsync("no shutdown");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("exit");

            var iface = device.GetInterface("1/1/1");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = await device.ProcessCommandAsync("show interface 1/1/1");
            var alias = await device.ProcessCommandAsync("show interface 1/1/c1");
            Assert.Equal(full, alias);
        }
    }
}
