using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Arista
{
    public class AristaOperationsTests
    {
        [Fact]
        public async Task Arista_ConfigureVlanAndPing_ShouldSucceed()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("vlan 10");
            sw1.ProcessCommand("name SALES");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("interface Ethernet1");
            sw1.ProcessCommand("switchport mode access");
            sw1.ProcessCommand("switchport access vlan 10");
            sw1.ProcessCommand("no switchport");
            sw1.ProcessCommand("ip address 192.168.1.1/24");
            sw1.ProcessCommand("no shutdown");
            sw1.ProcessCommand("exit");

            sw2.ProcessCommand("enable");
            sw2.ProcessCommand("configure");
            sw2.ProcessCommand("vlan 10");
            sw2.ProcessCommand("name SALES");
            sw2.ProcessCommand("exit");
            sw2.ProcessCommand("interface Ethernet1");
            sw2.ProcessCommand("switchport mode access");
            sw2.ProcessCommand("switchport access vlan 10");
            sw2.ProcessCommand("no switchport");
            sw2.ProcessCommand("ip address 192.168.1.2/24");
            sw2.ProcessCommand("no shutdown");
            sw2.ProcessCommand("exit");

            var vlanOutput = sw1.ProcessCommand("show vlan");
            Assert.Contains("10   SALES", vlanOutput);
            Assert.Contains("active", vlanOutput);

            var pingOutput = sw1.ProcessCommand("ping 192.168.1.2");
            Assert.Contains("Success rate is 100 percent (5/5)", pingOutput);
            Assert.Contains("!!!!!", pingOutput);
        }

        [Fact]
        public async Task Arista_InterfaceShutdown_ShouldStopTraffic()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("interface Ethernet1");
            sw1.ProcessCommand("no switchport");
            sw1.ProcessCommand("ip address 192.168.1.1/24");
            sw1.ProcessCommand("no shutdown");
            sw1.ProcessCommand("exit");

            sw2.ProcessCommand("enable");
            sw2.ProcessCommand("configure");
            sw2.ProcessCommand("interface Ethernet1");
            sw2.ProcessCommand("no switchport");
            sw2.ProcessCommand("ip address 192.168.1.2/24");
            sw2.ProcessCommand("no shutdown");
            sw2.ProcessCommand("exit");

            var pingBefore = sw1.ProcessCommand("ping 192.168.1.2");
            Assert.Contains("Success rate is 100 percent", pingBefore);

            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("interface Ethernet1");
            sw1.ProcessCommand("shutdown");
            sw1.ProcessCommand("exit");

            var ifaceOutput = sw1.ProcessCommand("show interface Ethernet1");
            Assert.Contains("administratively down", ifaceOutput);

            var pingAfter = sw1.ProcessCommand("ping 192.168.1.2");
            Assert.Contains("Success rate is 0 percent", pingAfter);
        }

        [Fact]
        public async Task Arista_ConfigureAcl_ShouldBlockPing()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("interface Ethernet1");
            sw1.ProcessCommand("no switchport");
            sw1.ProcessCommand("ip address 192.168.1.1/24");
            sw1.ProcessCommand("no shutdown");
            sw1.ProcessCommand("exit");

            sw2.ProcessCommand("enable");
            sw2.ProcessCommand("configure");
            sw2.ProcessCommand("interface Ethernet1");
            sw2.ProcessCommand("no switchport");
            sw2.ProcessCommand("ip address 192.168.1.2/24");
            sw2.ProcessCommand("no shutdown");
            sw2.ProcessCommand("exit");

            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("ip access-list standard BLOCK-PING");
            sw1.ProcessCommand("deny 192.168.1.1 0.0.0.0");
            sw1.ProcessCommand("permit any");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("exit");

            var pingOutput = sw1.ProcessCommand("ping 192.168.1.2");
            Assert.Contains("Destination host unreachable", pingOutput);
        }

        [Fact]
        public async Task Arista_ConfigureStp_ShouldSetPriority()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("spanning-tree mode rapid-pvst");
            sw1.ProcessCommand("spanning-tree vlan 1 priority 4096");
            sw1.ProcessCommand("exit");

            sw2.ProcessCommand("enable");
            sw2.ProcessCommand("configure");
            sw2.ProcessCommand("spanning-tree mode rapid-pvst");
            sw2.ProcessCommand("spanning-tree vlan 1 priority 8192");
            sw2.ProcessCommand("exit");

            var stpOutput = sw1.ProcessCommand("show spanning-tree");
            Assert.Contains("This bridge is the root", stpOutput);
            Assert.Contains("4096", stpOutput);
        }

        [Fact]
        public void Arista_ConfigurePortChannel_ShouldCreateLag()
        {
            var sw1 = new AristaDevice("SW1");
            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("interface Ethernet1");
            sw1.ProcessCommand("channel-group 1 mode active");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("interface Ethernet2");
            sw1.ProcessCommand("channel-group 1 mode active");
            sw1.ProcessCommand("exit");

            var configOutput = sw1.ProcessCommand("show running-config");
            Assert.Contains("channel-group 1 mode active", configOutput);
            Assert.Contains("interface Port-Channel1", configOutput);
        }

        [Fact]
        public void Arista_InvalidCommand_ShouldReturnError()
        {
            var sw1 = new AristaDevice("SW1");
            var output1 = sw1.ProcessCommand("invalid command");
            Assert.Contains("Invalid input", output1);

            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            var output2 = sw1.ProcessCommand("interface InvalidInterface");
            Assert.Contains("Invalid interface", output2);

            sw1.ProcessCommand("vlan");
            Assert.Contains("Invalid input", sw1.ProcessCommand("vlan"));
        }

        [Fact]
        public void Arista_ShowCommands_ShouldReturnProperOutput()
        {
            var sw1 = new AristaDevice("SW1");
            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("hostname TestSwitch");
            sw1.ProcessCommand("interface Ethernet1");
            sw1.ProcessCommand("no switchport");
            sw1.ProcessCommand("ip address 10.0.0.1/24");
            sw1.ProcessCommand("description WAN Link");
            sw1.ProcessCommand("speed 1000");
            sw1.ProcessCommand("duplex full");
            sw1.ProcessCommand("exit");

            var runConfig = sw1.ProcessCommand("show running-config");
            Assert.Contains("hostname TestSwitch", runConfig);
            Assert.Contains("interface Ethernet1", runConfig);
            Assert.Contains("ip address 10.0.0.1/24", runConfig);
            Assert.Contains("description WAN Link", runConfig);
            Assert.Contains("speed 1000", runConfig);
            Assert.Contains("duplex full", runConfig);

            var ipBrief = sw1.ProcessCommand("show ip interface brief");
            Assert.Contains("Ethernet1", ipBrief);
            Assert.Contains("10.0.0.1/24", ipBrief);
            Assert.Contains("up", ipBrief);

            var ifaceDetail = sw1.ProcessCommand("show interface Ethernet1");
            Assert.Contains("Ethernet1 is up", ifaceDetail);
            Assert.Contains("Internet address is 10.0.0.1/24", ifaceDetail);
            Assert.Contains("Description: WAN Link", ifaceDetail);
            Assert.Contains("Full-duplex, 1Gb/s", ifaceDetail);

            var versionOutput = sw1.ProcessCommand("show version");
            Assert.Contains("Arista vEOS", versionOutput);
            Assert.Contains("EOS-4.24.0F", versionOutput);
        }

        [Fact]
        public async Task Arista_InterfaceCounters_ShouldIncrement()
        {
            var network = new Network();
            var sw1 = new AristaDevice("SW1");
            var sw2 = new AristaDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "Ethernet1", "SW2", "Ethernet1");

            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("interface Ethernet1");
            sw1.ProcessCommand("no switchport");
            sw1.ProcessCommand("ip address 192.168.1.1/24");
            sw1.ProcessCommand("no shutdown");
            sw1.ProcessCommand("exit");

            sw2.ProcessCommand("enable");
            sw2.ProcessCommand("configure");
            sw2.ProcessCommand("interface Ethernet1");
            sw2.ProcessCommand("no switchport");
            sw2.ProcessCommand("ip address 192.168.1.2/24");
            sw2.ProcessCommand("no shutdown");
            sw2.ProcessCommand("exit");

            sw1.ProcessCommand("ping 192.168.1.2");

            var sw1IfaceOutput = sw1.ProcessCommand("show interface Ethernet1");
            Assert.Contains("5 packets output, 500 bytes", sw1IfaceOutput);

            var sw2IfaceOutput = sw2.ProcessCommand("show interface Ethernet1");
            Assert.Contains("5 packets input, 500 bytes", sw2IfaceOutput);
        }

        [Fact]
        public void Arista_VlanList_ShouldSupportCommaSeparated()
        {
            var sw1 = new AristaDevice("SW1");
            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("vlan 10,20,30");
            sw1.ProcessCommand("exit");

            var vlanOutput = sw1.ProcessCommand("show vlan");
            Assert.Contains("10", vlanOutput);
            Assert.Contains("20", vlanOutput);
            Assert.Contains("30", vlanOutput);
        }

        [Fact]
        public void Arista_InterfaceStatus_ShouldShowProperFormat()
        {
            var sw1 = new AristaDevice("SW1");
            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure");
            sw1.ProcessCommand("interface Ethernet1");
            sw1.ProcessCommand("switchport mode access");
            sw1.ProcessCommand("switchport access vlan 10");
            sw1.ProcessCommand("exit");

            var statusOutput = sw1.ProcessCommand("show interface status");
            Assert.Contains("Port", statusOutput);
            Assert.Contains("Status", statusOutput);
            Assert.Contains("Vlan", statusOutput);
            Assert.Contains("Ethernet1", statusOutput);
            Assert.Contains("connected", statusOutput);
        }
    }
}

