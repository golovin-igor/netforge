using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Fortinet CLI aliases and interface naming.
    /// </summary>
    public class FortinetAliasTests
    {
        [Fact]
        public async Task Fortinet_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new FortinetDevice("FW1");
            var full = await device.ProcessCommandAsync("show running-config");
            var alias = await device.ProcessCommandAsync("sh run");
            Assert.Equal(full, alias);
        }

        [Fact]
        public async Task Fortinet_InterfaceAliasConfig_ShouldApplySettings()
        {
            var device = new FortinetDevice("FW1");
            await device.ProcessCommandAsync("config system interface");
            await device.ProcessCommandAsync("edit port1");
            await device.ProcessCommandAsync("set ip 10.1.1.1 255.255.255.0");
            await device.ProcessCommandAsync("set allowaccess ping");
            await device.ProcessCommandAsync("next");
            await device.ProcessCommandAsync("end");

            var iface = device.GetInterface("port1");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = await device.ProcessCommandAsync("show interface port1");
            var alias = await device.ProcessCommandAsync("show interface port1");
            Assert.Equal(full, alias);
        }
    }
}
