using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Huawei
{
    public class HuaweiDeviceDisplayTests
    {
        [Fact]
        public void Huawei_DisplayCurrentConfiguration_ShouldIncludeAllSettings()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("sysname Huawei-Core");
            device.ProcessCommand("vlan 10");
            device.ProcessCommand("description SALES");
            device.ProcessCommand("quit");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("quit");
            device.ProcessCommand("ospf 1");
            device.ProcessCommand("area 0.0.0.0");
            device.ProcessCommand("network 10.0.0.0 0.0.0.255");
            device.ProcessCommand("quit");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display current-configuration");
            Assert.Contains("sysname Huawei-Core", output);
            Assert.Contains("vlan 10", output);
            Assert.Contains("description SALES", output);
            Assert.Contains("ip address 10.0.0.1 255.255.255.0", output);
            Assert.Contains("ospf 1", output);
            Assert.Contains("network 10.0.0.0 0.0.0.255", output);
        }

        [Fact]
        public void Huawei_DisplayIpRoutingTable_ShouldShowAllRoutes()
        {
            var device = new HuaweiDevice("R1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("undo shutdown");
            device.ProcessCommand("quit");
            device.ProcessCommand("ip route-static 192.168.1.0 255.255.255.0 10.0.0.2");
            device.ProcessCommand("ospf 1");
            device.ProcessCommand("area 0");
            device.ProcessCommand("network 10.0.0.0 0.0.0.255");
            device.ProcessCommand("quit");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display ip routing-table");
            Assert.Contains("10.0.0.0/24", output);
            Assert.Contains("192.168.1.0/24", output);
            Assert.Contains("Direct", output);
            Assert.Contains("Static", output);
        }

        [Fact]
        public void Huawei_DisplayInterface_ShouldShowInterfaceDetails()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("description WAN Link");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("undo shutdown");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display interface GigabitEthernet0/0/1");
            Assert.Contains("GigabitEthernet0/0/1 current state : UP", output);
            Assert.Contains("Line protocol current state : UP", output);
            Assert.Contains("Description:WAN Link", output);
            Assert.Contains("Internet Address is 10.0.0.1/24", output);
        }

        [Fact]
        public void Huawei_DisplayVlan_ShouldShowVlanConfiguration()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("vlan 10");
            device.ProcessCommand("description SALES");
            device.ProcessCommand("quit");
            device.ProcessCommand("vlan 20");
            device.ProcessCommand("description MARKETING");
            device.ProcessCommand("quit");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("port link-type access");
            device.ProcessCommand("port default vlan 10");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display vlan");
            Assert.Contains("10", output);
            Assert.Contains("SALES", output);
            Assert.Contains("20", output);
            Assert.Contains("MARKETING", output);
            Assert.Contains("common", output);
        }

        [Fact]
        public void Huawei_DisplayOspfPeer_ShouldShowNeighbors()
        {
            var device = new HuaweiDevice("R1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("ospf 1");
            device.ProcessCommand("router-id 1.1.1.1");
            device.ProcessCommand("area 0");
            device.ProcessCommand("network 10.0.0.0 0.0.0.255");
            device.ProcessCommand("quit");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display ospf peer");
            Assert.Contains("2.2.2.2", output);
            Assert.Contains("10.0.0.2", output);
            Assert.Contains("Full", output);
            Assert.Contains("GigabitEthernet0/0/1", output);
        }

        [Fact]
        public void Huawei_DisplayBgpPeer_ShouldShowPeerStatus()
        {
            var device = new HuaweiDevice("R1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("bgp 65001");
            device.ProcessCommand("router-id 1.1.1.1");
            device.ProcessCommand("peer 172.16.0.2 as-number 65002");
            device.ProcessCommand("network 10.0.0.0 24");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display bgp peer");
            Assert.Contains("BGP local router ID", output);
            Assert.Contains("1.1.1.1", output);
            Assert.Contains("Local AS number : 65001", output);
            Assert.Contains("172.16.0.2", output);
            Assert.Contains("65002", output);
        }

        [Fact]
        public void Huawei_DisplayVersion_ShouldShowSystemInfo()
        {
            var device = new HuaweiDevice("SW1");
            var output = device.ProcessCommand("display version");

            Assert.Contains("Huawei Versatile Routing Platform Software", output);
            Assert.Contains("VRP (R) software", output);
            Assert.Contains("HUAWEI", output);
            Assert.Contains("Uptime", output);
        }

        [Fact]
        public void Huawei_DisplayArp_ShouldShowArpTable()
        {
            var device = new HuaweiDevice("R1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("undo shutdown");
            device.ProcessCommand("quit");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display arp");
            Assert.Contains("IP ADDRESS", output);
            Assert.Contains("MAC ADDRESS", output);
            Assert.Contains("VPN-Instance", output);
            Assert.Contains("10.0.0.1", output);
        }

        [Fact]
        public void Huawei_DisplayMacAddress_ShouldShowMacTable()
        {
            var device = new HuaweiDevice("SW1");
            var output = device.ProcessCommand("display mac-address");

            Assert.Contains("MAC Address", output);
            Assert.Contains("VLAN", output);
            Assert.Contains("Learned-From", output);
            Assert.Contains("Type", output);
            Assert.Contains("dynamic", output);
        }

        [Fact]
        public void Huawei_DisplayInterfaceBrief_ShouldShowSummary()
        {
            var device = new HuaweiDevice("R1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("undo shutdown");
            device.ProcessCommand("quit");
            device.ProcessCommand("interface GigabitEthernet0/0/2");
            device.ProcessCommand("shutdown");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display interface brief");
            Assert.Contains("Interface", output);
            Assert.Contains("PHY", output);
            Assert.Contains("Protocol", output);
            Assert.Contains("GigabitEthernet0/0/1", output);
            Assert.Contains("up", output);
            Assert.Contains("GigabitEthernet0/0/2", output);
            Assert.Contains("down", output);
        }

        [Fact]
        public void Huawei_DisplayStp_ShouldShowSpanningTree()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("stp mode rstp");
            device.ProcessCommand("stp priority 4096");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display stp");
            Assert.Contains("Protocol Status", output);
            Assert.Contains("Bridge ID", output);
            Assert.Contains("Priority", output);
            Assert.Contains("4096", output);
        }

        [Fact]
        public void Huawei_DisplayEthTrunk_ShouldShowLagStatus()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface Eth-Trunk1");
            device.ProcessCommand("mode lacp");
            device.ProcessCommand("quit");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("eth-trunk 1");
            device.ProcessCommand("quit");
            device.ProcessCommand("interface GigabitEthernet0/0/2");
            device.ProcessCommand("eth-trunk 1");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display eth-trunk");
            Assert.Contains("Eth-Trunk1", output);
            Assert.Contains("LAG ID", output);
            Assert.Contains("WorkingMode", output);
            Assert.Contains("LACP", output);
            Assert.Contains("GigabitEthernet0/0/1", output);
            Assert.Contains("GigabitEthernet0/0/2", output);
        }

        [Fact]
        public void Huawei_DisplayLogbuffer_ShouldShowLogEntries()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("shutdown");
            device.ProcessCommand("undo shutdown");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display logbuffer");
            Assert.Contains("%IFNET", output);
            Assert.Contains("GigabitEthernet0/0/1", output);
            Assert.Contains("state", output);
        }
    }
}

