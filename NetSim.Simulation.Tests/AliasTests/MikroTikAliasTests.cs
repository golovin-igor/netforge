using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests MikroTik CLI alias commands and interface naming.
    /// </summary>
    public class MikroTikAliasTests
    {
        [Fact]
        public async Task MikroTik_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new MikroTikDevice("RB1");
            var full = await device.ProcessCommandAsync("/export");
            var alias = await device.ProcessCommandAsync("export");
            Assert.Equal(full, alias);
        }

        [Fact]
        public async Task MikroTik_InterfaceName_ShouldConfigureProperly()
        {
            var device = new MikroTikDevice("RB1");
            await device.ProcessCommandAsync("/interface ethernet set ether1 disabled=no");
            await device.ProcessCommandAsync("/ip address add address=10.1.1.1/24 interface=ether1");

            var iface = device.GetInterface("ether1");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = await device.ProcessCommandAsync("/interface print detail where name=ether1");
            var alias = await device.ProcessCommandAsync("interface print detail where name=ether1");
            Assert.Equal(full, alias);
        }
    }
}
