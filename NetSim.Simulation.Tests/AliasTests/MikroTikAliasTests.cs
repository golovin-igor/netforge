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
        public void MikroTik_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new MikroTikDevice("RB1");
            var full = device.ProcessCommand("/export");
            var alias = device.ProcessCommand("export");
            Assert.Equal(full, alias);
        }

        [Fact]
        public void MikroTik_InterfaceName_ShouldConfigureProperly()
        {
            var device = new MikroTikDevice("RB1");
            device.ProcessCommand("/interface ethernet set ether1 disabled=no");
            device.ProcessCommand("/ip address add address=10.1.1.1/24 interface=ether1");

            var iface = device.GetInterface("ether1");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = device.ProcessCommand("/interface print detail where name=ether1");
            var alias = device.ProcessCommand("interface print detail where name=ether1");
            Assert.Equal(full, alias);
        }
    }
}
