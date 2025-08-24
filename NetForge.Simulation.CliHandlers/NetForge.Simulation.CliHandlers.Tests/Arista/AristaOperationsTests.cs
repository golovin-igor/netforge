using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Arista
{
    public class AristaOperationsTests
    {
        [Fact]
        public async Task AristaConfigureVlanAndPingShouldSucceed()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("vlan 10");
            await sw1.ProcessCommandAsync("name SALES");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("interface Ethernet1");
            await sw1.ProcessCommandAsync("switchport mode access");
            await sw1.ProcessCommandAsync("switchport access vlan 10");
            await sw1.ProcessCommandAsync("no switchport");
            await sw1.ProcessCommandAsync("ip address 192.168.1.1/24");
            await sw1.ProcessCommandAsync("no shutdown");
            await sw1.ProcessCommandAsync("exit");

            await sw2.ProcessCommandAsync("enable");
            await sw2.ProcessCommandAsync("configure");
            await sw2.ProcessCommandAsync("vlan 10");
            await sw2.ProcessCommandAsync("name SALES");
            await sw2.ProcessCommandAsync("exit");
            await sw2.ProcessCommandAsync("interface Ethernet1");
            await sw2.ProcessCommandAsync("switchport mode access");
            await sw2.ProcessCommandAsync("switchport access vlan 10");
            await sw2.ProcessCommandAsync("no switchport");
            await sw2.ProcessCommandAsync("ip address 192.168.1.2/24");
            await sw2.ProcessCommandAsync("no shutdown");
            await sw2.ProcessCommandAsync("exit");

            var vlanOutput = await sw1.ProcessCommandAsync("show vlan");
            Assert.Contains("10   SALES", vlanOutput);
            Assert.Contains("active", vlanOutput);

            var pingOutput = await sw1.ProcessCommandAsync("ping 192.168.1.2");
            Assert.Contains("Success rate is 100 percent (5/5)", pingOutput);
            Assert.Contains("!!!!!", pingOutput);
        }

        [Fact]
        public async Task AristaInterfaceShutdownShouldStopTraffic()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("interface Ethernet1");
            await sw1.ProcessCommandAsync("no switchport");
            await sw1.ProcessCommandAsync("ip address 192.168.1.1/24");
            await sw1.ProcessCommandAsync("no shutdown");
            await sw1.ProcessCommandAsync("exit");

            await sw2.ProcessCommandAsync("enable");
            await sw2.ProcessCommandAsync("configure");
            await sw2.ProcessCommandAsync("interface Ethernet1");
            await sw2.ProcessCommandAsync("no switchport");
            await sw2.ProcessCommandAsync("ip address 192.168.1.2/24");
            await sw2.ProcessCommandAsync("no shutdown");
            await sw2.ProcessCommandAsync("exit");

            var pingBefore = await sw1.ProcessCommandAsync("ping 192.168.1.2");
            Assert.Contains("Success rate is 100 percent", pingBefore);

            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("interface Ethernet1");
            await sw1.ProcessCommandAsync("shutdown");
            await sw1.ProcessCommandAsync("exit");

            var ifaceOutput = await sw1.ProcessCommandAsync("show interface Ethernet1");
            Assert.Contains("administratively down", ifaceOutput);

            var pingAfter = await sw1.ProcessCommandAsync("ping 192.168.1.2");
            Assert.Contains("Success rate is 0 percent", pingAfter);
        }

        [Fact]
        public async Task AristaConfigureAclShouldBlockPing()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("interface Ethernet1");
            await sw1.ProcessCommandAsync("no switchport");
            await sw1.ProcessCommandAsync("ip address 192.168.1.1/24");
            await sw1.ProcessCommandAsync("no shutdown");
            await sw1.ProcessCommandAsync("exit");

            await sw2.ProcessCommandAsync("enable");
            await sw2.ProcessCommandAsync("configure");
            await sw2.ProcessCommandAsync("interface Ethernet1");
            await sw2.ProcessCommandAsync("no switchport");
            await sw2.ProcessCommandAsync("ip address 192.168.1.2/24");
            await sw2.ProcessCommandAsync("no shutdown");
            await sw2.ProcessCommandAsync("exit");

            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("ip access-list standard BLOCK-PING");
            await sw1.ProcessCommandAsync("deny 192.168.1.1 0.0.0.0");
            await sw1.ProcessCommandAsync("permit any");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("exit");

            var pingOutput = await sw1.ProcessCommandAsync("ping 192.168.1.2");
            Assert.Contains("Destination host unreachable", pingOutput);
        }

        [Fact]
        public async Task AristaConfigureStpShouldSetPriority()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("spanning-tree mode rapid-pvst");
            await sw1.ProcessCommandAsync("spanning-tree vlan 1 priority 4096");
            await sw1.ProcessCommandAsync("exit");

            await sw2.ProcessCommandAsync("enable");
            await sw2.ProcessCommandAsync("configure");
            await sw2.ProcessCommandAsync("spanning-tree mode rapid-pvst");
            await sw2.ProcessCommandAsync("spanning-tree vlan 1 priority 8192");
            await sw2.ProcessCommandAsync("exit");

            var stpOutput = await sw1.ProcessCommandAsync("show spanning-tree");
            Assert.Contains("This bridge is the root", stpOutput);
            Assert.Contains("4096", stpOutput);
        }

        [Fact]
        public async Task AristaConfigurePortChannelShouldCreateLag()
        {
            var sw1 = new AristaDevice("SW1");
            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("interface Ethernet1");
            await sw1.ProcessCommandAsync("channel-group 1 mode active");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("interface Ethernet2");
            await sw1.ProcessCommandAsync("channel-group 1 mode active");
            await sw1.ProcessCommandAsync("exit");

            var configOutput = await sw1.ProcessCommandAsync("show running-config");
            Assert.Contains("channel-group 1 mode active", configOutput);
            Assert.Contains("interface Port-Channel1", configOutput);
        }

        [Fact]
        public async Task AristaInvalidCommandShouldReturnError()
        {
            var sw1 = new AristaDevice("SW1");
            var output1 = await sw1.ProcessCommandAsync("invalid command");
            Assert.Contains("Invalid input", output1);

            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            var output2 = await sw1.ProcessCommandAsync("interface InvalidInterface");
            Assert.Contains("Invalid interface", output2);

            await sw1.ProcessCommandAsync("vlan");
            Assert.Contains("Invalid input", await sw1.ProcessCommandAsync("vlan"));
        }

        [Fact]
        public async Task AristaShowCommandsShouldReturnProperOutput()
        {
            var sw1 = new AristaDevice("SW1");
            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("hostname TestSwitch");
            await sw1.ProcessCommandAsync("interface Ethernet1");
            await sw1.ProcessCommandAsync("no switchport");
            await sw1.ProcessCommandAsync("ip address 10.0.0.1/24");
            await sw1.ProcessCommandAsync("description WAN Link");
            await sw1.ProcessCommandAsync("speed 1000");
            await sw1.ProcessCommandAsync("duplex full");
            await sw1.ProcessCommandAsync("exit");

            var runConfig = await sw1.ProcessCommandAsync("show running-config");
            Assert.Contains("hostname TestSwitch", runConfig);
            Assert.Contains("interface Ethernet1", runConfig);
            Assert.Contains("ip address 10.0.0.1/24", runConfig);
            Assert.Contains("description WAN Link", runConfig);
            Assert.Contains("speed 1000", runConfig);
            Assert.Contains("duplex full", runConfig);

            var ipBrief = await sw1.ProcessCommandAsync("show ip interface brief");
            Assert.Contains("Ethernet1", ipBrief);
            Assert.Contains("10.0.0.1/24", ipBrief);
            Assert.Contains("up", ipBrief);

            var ifaceDetail = await sw1.ProcessCommandAsync("show interface Ethernet1");
            Assert.Contains("Ethernet1 is up", ifaceDetail);
            Assert.Contains("Internet address is 10.0.0.1/24", ifaceDetail);
            Assert.Contains("Description: WAN Link", ifaceDetail);
            Assert.Contains("Full-duplex, 1Gb/s", ifaceDetail);

            var versionOutput = await sw1.ProcessCommandAsync("show version");
            Assert.Contains("Arista vEOS", versionOutput);
            Assert.Contains("EOS-4.24.0F", versionOutput);
        }

        [Fact]
        public async Task AristaInterfaceCountersShouldIncrement()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("interface Ethernet1");
            await sw1.ProcessCommandAsync("no switchport");
            await sw1.ProcessCommandAsync("ip address 192.168.1.1/24");
            await sw1.ProcessCommandAsync("no shutdown");
            await sw1.ProcessCommandAsync("exit");

            await sw2.ProcessCommandAsync("enable");
            await sw2.ProcessCommandAsync("configure");
            await sw2.ProcessCommandAsync("interface Ethernet1");
            await sw2.ProcessCommandAsync("no switchport");
            await sw2.ProcessCommandAsync("ip address 192.168.1.2/24");
            await sw2.ProcessCommandAsync("no shutdown");
            await sw2.ProcessCommandAsync("exit");

            await sw1.ProcessCommandAsync("ping 192.168.1.2");

            var sw1IfaceOutput = await sw1.ProcessCommandAsync("show interface Ethernet1");
            Assert.Contains("5 packets output, 500 bytes", sw1IfaceOutput);

            var sw2IfaceOutput = await sw2.ProcessCommandAsync("show interface Ethernet1");
            Assert.Contains("5 packets input, 500 bytes", sw2IfaceOutput);
        }

        [Fact]
        public async Task AristaVlanListShouldSupportCommaSeparated()
        {
            var sw1 = new AristaDevice("SW1");
            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("vlan 10,20,30");
            await sw1.ProcessCommandAsync("exit");

            var vlanOutput = await sw1.ProcessCommandAsync("show vlan");
            Assert.Contains("10", vlanOutput);
            Assert.Contains("20", vlanOutput);
            Assert.Contains("30", vlanOutput);
        }

        [Fact]
        public async Task AristaInterfaceStatusShouldShowProperFormat()
        {
            var sw1 = new AristaDevice("SW1");
            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure");
            await sw1.ProcessCommandAsync("interface Ethernet1");
            await sw1.ProcessCommandAsync("switchport mode access");
            await sw1.ProcessCommandAsync("switchport access vlan 10");
            await sw1.ProcessCommandAsync("exit");

            var statusOutput = await sw1.ProcessCommandAsync("show interface status");
            Assert.Contains("Port", statusOutput);
            Assert.Contains("Status", statusOutput);
            Assert.Contains("Vlan", statusOutput);
            Assert.Contains("Ethernet1", statusOutput);
            Assert.Contains("connected", statusOutput);
        }
    }
}

