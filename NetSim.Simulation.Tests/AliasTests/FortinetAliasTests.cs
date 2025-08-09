using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Fortinet CLI aliases and interface naming.
    /// </summary>
    public class FortinetAliasTests
    {
        [Fact]
        public void Fortinet_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new FortinetDevice("FW1");
            var full = device.ProcessCommand("show running-config");
            var alias = device.ProcessCommand("sh run");
            Assert.Equal(full, alias);
        }

        [Fact]
        public void Fortinet_InterfaceAliasConfig_ShouldApplySettings()
        {
            var device = new FortinetDevice("FW1");
            device.ProcessCommand("config system interface");
            device.ProcessCommand("edit port1");
            device.ProcessCommand("set ip 10.1.1.1 255.255.255.0");
            device.ProcessCommand("set allowaccess ping");
            device.ProcessCommand("next");
            device.ProcessCommand("end");

            var iface = device.GetInterface("port1");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = device.ProcessCommand("show interface port1");
            var alias = device.ProcessCommand("show interface port1");
            Assert.Equal(full, alias);
        }
    }
}
