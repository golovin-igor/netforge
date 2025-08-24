using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Huawei
{
    public class HuaweiDeviceDisplayTests
    {
        [Fact]
        public async Task HuaweiDisplayCurrentConfigurationShouldIncludeAllSettings()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("sysname Huawei-Core");
            await device.ProcessCommandAsync("vlan 10");
            await device.ProcessCommandAsync("description SALES");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("ospf 1");
            await device.ProcessCommandAsync("area 0.0.0.0");
            await device.ProcessCommandAsync("network 10.0.0.0 0.0.0.255");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display current-configuration");
            Assert.Contains("sysname Huawei-Core", output);
            Assert.Contains("vlan 10", output);
            Assert.Contains("description SALES", output);
            Assert.Contains("ip address 10.0.0.1 255.255.255.0", output);
            Assert.Contains("ospf 1", output);
            Assert.Contains("network 10.0.0.0 0.0.0.255", output);
        }

        [Fact]
        public async Task HuaweiDisplayIpRoutingTableShouldShowAllRoutes()
        {
            var device = new HuaweiDevice("R1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("undo shutdown");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("ip route-static 192.168.1.0 255.255.255.0 10.0.0.2");
            await device.ProcessCommandAsync("ospf 1");
            await device.ProcessCommandAsync("area 0");
            await device.ProcessCommandAsync("network 10.0.0.0 0.0.0.255");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display ip routing-table");
            Assert.Contains("10.0.0.0/24", output);
            Assert.Contains("192.168.1.0/24", output);
            Assert.Contains("Direct", output);
            Assert.Contains("Static", output);
        }

        [Fact]
        public async Task HuaweiDisplayInterfaceShouldShowInterfaceDetails()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("description WAN Link");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("undo shutdown");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display interface GigabitEthernet0/0/1");
            Assert.Contains("GigabitEthernet0/0/1 current state : UP", output);
            Assert.Contains("Line protocol current state : UP", output);
            Assert.Contains("Description:WAN Link", output);
            Assert.Contains("Internet Address is 10.0.0.1/24", output);
        }

        [Fact]
        public async Task HuaweiDisplayVlanShouldShowVlanConfiguration()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("vlan 10");
            await device.ProcessCommandAsync("description SALES");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("vlan 20");
            await device.ProcessCommandAsync("description MARKETING");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("port link-type access");
            await device.ProcessCommandAsync("port default vlan 10");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display vlan");
            Assert.Contains("10", output);
            Assert.Contains("SALES", output);
            Assert.Contains("20", output);
            Assert.Contains("MARKETING", output);
            Assert.Contains("common", output);
        }

        [Fact]
        public async Task HuaweiDisplayOspfPeerShouldShowNeighbors()
        {
            var device = new HuaweiDevice("R1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("ospf 1");
            await device.ProcessCommandAsync("router-id 1.1.1.1");
            await device.ProcessCommandAsync("area 0");
            await device.ProcessCommandAsync("network 10.0.0.0 0.0.0.255");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display ospf peer");
            Assert.Contains("2.2.2.2", output);
            Assert.Contains("10.0.0.2", output);
            Assert.Contains("Full", output);
            Assert.Contains("GigabitEthernet0/0/1", output);
        }

        [Fact]
        public async Task HuaweiDisplayBgpPeerShouldShowPeerStatus()
        {
            var device = new HuaweiDevice("R1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("bgp 65001");
            await device.ProcessCommandAsync("router-id 1.1.1.1");
            await device.ProcessCommandAsync("peer 172.16.0.2 as-number 65002");
            await device.ProcessCommandAsync("network 10.0.0.0 24");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display bgp peer");
            Assert.Contains("BGP local router ID", output);
            Assert.Contains("1.1.1.1", output);
            Assert.Contains("Local AS number : 65001", output);
            Assert.Contains("172.16.0.2", output);
            Assert.Contains("65002", output);
        }

        [Fact]
        public async Task HuaweiDisplayVersionShouldShowSystemInfo()
        {
            var device = new HuaweiDevice("SW1");
            var output = await device.ProcessCommandAsync("display version");

            Assert.Contains("Huawei Versatile Routing Platform Software", output);
            Assert.Contains("VRP (R) software", output);
            Assert.Contains("HUAWEI", output);
            Assert.Contains("Uptime", output);
        }

        [Fact]
        public async Task HuaweiDisplayArpShouldShowArpTable()
        {
            var device = new HuaweiDevice("R1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("undo shutdown");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display arp");
            Assert.Contains("IP ADDRESS", output);
            Assert.Contains("MAC ADDRESS", output);
            Assert.Contains("VPN-Instance", output);
            Assert.Contains("10.0.0.1", output);
        }

        [Fact]
        public async Task HuaweiDisplayMacAddressShouldShowMacTable()
        {
            var device = new HuaweiDevice("SW1");
            var output = await device.ProcessCommandAsync("display mac-address");

            Assert.Contains("MAC Address", output);
            Assert.Contains("VLAN", output);
            Assert.Contains("Learned-From", output);
            Assert.Contains("Type", output);
            Assert.Contains("dynamic", output);
        }

        [Fact]
        public async Task HuaweiDisplayInterfaceBriefShouldShowSummary()
        {
            var device = new HuaweiDevice("R1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("undo shutdown");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/2");
            await device.ProcessCommandAsync("shutdown");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display interface brief");
            Assert.Contains("Interface", output);
            Assert.Contains("PHY", output);
            Assert.Contains("Protocol", output);
            Assert.Contains("GigabitEthernet0/0/1", output);
            Assert.Contains("up", output);
            Assert.Contains("GigabitEthernet0/0/2", output);
            Assert.Contains("down", output);
        }

        [Fact]
        public async Task HuaweiDisplayStpShouldShowSpanningTree()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("stp mode rstp");
            await device.ProcessCommandAsync("stp priority 4096");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display stp");
            Assert.Contains("Protocol Status", output);
            Assert.Contains("Bridge ID", output);
            Assert.Contains("Priority", output);
            Assert.Contains("4096", output);
        }

        [Fact]
        public async Task HuaweiDisplayEthTrunkShouldShowLagStatus()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface Eth-Trunk1");
            await device.ProcessCommandAsync("mode lacp");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("eth-trunk 1");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/2");
            await device.ProcessCommandAsync("eth-trunk 1");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display eth-trunk");
            Assert.Contains("Eth-Trunk1", output);
            Assert.Contains("LAG ID", output);
            Assert.Contains("WorkingMode", output);
            Assert.Contains("LACP", output);
            Assert.Contains("GigabitEthernet0/0/1", output);
            Assert.Contains("GigabitEthernet0/0/2", output);
        }

        [Fact]
        public async Task HuaweiDisplayLogbufferShouldShowLogEntries()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("shutdown");
            await device.ProcessCommandAsync("undo shutdown");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display logbuffer");
            Assert.Contains("%IFNET", output);
            Assert.Contains("GigabitEthernet0/0/1", output);
            Assert.Contains("state", output);
        }
    }
}

