using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests Aruba CLI aliases and interface naming variations.
    /// </summary>
    public class ArubaAliasTests
    {
        [Fact]
        public void Aruba_ShowRunAlias_ShouldMatchFullCommand()
        {
            var device = new ArubaDevice("SW1");
            var full = device.ProcessCommand("show running-config");
            var alias = device.ProcessCommand("sh run");
            Assert.Equal(full, alias);
        }

        [Fact]
        public void Aruba_InterfaceAliasConfig_ShouldApplySettings()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("conf t");
            device.ProcessCommand("int Gig1/0/1");
            device.ProcessCommand("ip address 10.1.1.1 255.255.255.0");
            device.ProcessCommand("no shut");
            device.ProcessCommand("exit");
            device.ProcessCommand("exit");

            var iface = device.GetInterface("GigabitEthernet1/0/1");
            Assert.Equal("10.1.1.1", iface.IpAddress);
            Assert.False(iface.IsShutdown);

            var full = device.ProcessCommand("show interface GigabitEthernet1/0/1");
            var alias = device.ProcessCommand("sh int 1/1/1");
            Assert.Equal(full, alias);
        }
    }
}
