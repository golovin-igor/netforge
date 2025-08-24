using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Aruba CLI aliases and interface naming variations.
    /// </summary>
    public class ArubaAliasTests
    {
        [Fact]
        public async Task Aruba_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new ArubaDevice("SW1");
            var full = await device.ProcessCommandAsync("show running-config");
            var alias = await device.ProcessCommandAsync("sh run");
            Assert.Equal(full, alias);
        }

        [Fact]
        public async Task Aruba_InterfaceAliasConfig_ShouldApplySettings()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("conf t");
            await device.ProcessCommandAsync("int Gig1/0/1");
            await device.ProcessCommandAsync("ip address 10.1.1.1 255.255.255.0");
            await device.ProcessCommandAsync("no shut");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("exit");

            var iface = device.GetInterface("GigabitEthernet1/0/1");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = await device.ProcessCommandAsync("show interface GigabitEthernet1/0/1");
            var alias = await device.ProcessCommandAsync("sh int 1/1/1");
            Assert.Equal(full, alias);
        }
    }
}
