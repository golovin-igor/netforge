using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Aruba
{
    public class ArubaDeviceTests
    {
        // Test 1: Show Running-Configuration
        [Fact]
        public async Task ArubaShowRunningConfigShouldIncludeAllConfigurations()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("hostname Aruba-Core");
            await device.ProcessCommandAsync("vlan 10");
            await device.ProcessCommandAsync("name SALES");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface 1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("router ospf");
            await device.ProcessCommandAsync("area 0.0.0.0 range 10.0.0.0 255.255.255.0");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show running-config");
            Assert.Contains("hostname \"Aruba-Core\"", output);
            Assert.Contains("vlan 10", output);
            Assert.Contains("name \"SALES\"", output);
            Assert.Contains("ip address 10.0.0.1 255.255.255.0", output);
            Assert.Contains("router ospf", output);
        }

        // Test 2: Show IP Route
        [Fact]
        public async Task ArubaShowIpRouteShouldDisplayAllRouteTypes()
        {
            var device = new ArubaDevice("R1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface vlan 1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("ip route 192.168.1.0 255.255.255.0 10.0.0.2");
            await device.ProcessCommandAsync("router ospf");
            await device.ProcessCommandAsync("area 0.0.0.0 range 10.0.0.0 255.255.255.0");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show ip route");
            Assert.Contains("10.0.0.0/24", output);
            Assert.Contains("192.168.1.0/24", output);
            Assert.Contains("connected", output);
            Assert.Contains("static", output);
        }

        // Test 3: Ping
        [Fact]
        public async Task ArubaPingShouldShowSuccessAndFailure()
        {
            var device = new ArubaDevice("R1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface vlan 1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("ping 10.0.0.2");
            Assert.Contains("5 packets transmitted, 5 packets received, 0.0% packet loss", output);

            output = await device.ProcessCommandAsync("ping 192.168.99.99");
            Assert.Contains("5 packets transmitted, 0 packets received, 100.0% packet loss", output);
        }

        // Test 4: Show Interface
        [Fact]
        public async Task ArubaShowInterfaceShouldDisplayInterfaceDetails()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface 1");
            await device.ProcessCommandAsync("name \"WAN Link\"");
            await device.ProcessCommandAsync("no disable");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface vlan 10");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show interfaces 1");
            Assert.Contains("Status and Counters", output);
            Assert.Contains("WAN Link", output);
            Assert.Contains("Link Status", output);

            output = await device.ProcessCommandAsync("show interfaces vlan 10");
            Assert.Contains("VLAN", output);
            Assert.Contains("10.0.0.1", output);
        }

        // Test 5: Configure Mode
        [Fact]
        public async Task ArubaConfigureShouldEnterConfigMode()
        {
            var device = new ArubaDevice("SW1");
            var output = await device.ProcessCommandAsync("enable");
            Assert.Contains("SW1#", output);

            output = await device.ProcessCommandAsync("configure");
            Assert.Contains("SW1(config)#", output);

            output = await device.ProcessCommandAsync("interface 1");
            Assert.Contains("SW1(eth-1)#", output);

            output = await device.ProcessCommandAsync("exit");
            Assert.Contains("SW1(config)#", output);
        }

        // Test 6: Show VLAN
        [Fact]
        public async Task ArubaShowVlanShouldDisplayVlanConfiguration()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("vlan 10");
            await device.ProcessCommandAsync("name SALES");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("vlan 20");
            await device.ProcessCommandAsync("name MARKETING");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface 1");
            await device.ProcessCommandAsync("untagged vlan 10");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show vlan");
            Assert.Contains("10", output);
            Assert.Contains("SALES", output);
            Assert.Contains("20", output);
            Assert.Contains("MARKETING", output);
            Assert.Contains("Port-based", output);
        }

        // Test 7: Show OSPF Neighbor
        [Fact]
        public async Task ArubaShowOspfNeighborShouldDisplayNeighbors()
        {
            var device = new ArubaDevice("R1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router ospf");
            await device.ProcessCommandAsync("router-id 1.1.1.1");
            await device.ProcessCommandAsync("area 0.0.0.0 range 10.0.0.0 255.255.255.0");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("exit");

            // Simulate neighbor
            // TODO: Update this when command handler architecture is complete
            // var ospfConfig = device.GetOspfConfig();
            // if (ospfConfig != null)
            // {
            //     ospfConfig.AddNeighbor("2.2.2.2", "10.0.0.2", "vlan 1", "DR");
            // }

            var output = await device.ProcessCommandAsync("show ip ospf neighbor");
            Assert.Contains("2.2.2.2", output);
            Assert.Contains("10.0.0.2", output);
            Assert.Contains("DR", output);
        }

        // Test 8: Interface Configuration
        [Fact]
        public async Task ArubaInterfaceConfigurationShouldApplySettings()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface 1");
            await device.ProcessCommandAsync("name \"Server Connection\"");
            await device.ProcessCommandAsync("speed 1000");
            await device.ProcessCommandAsync("duplex full");
            await device.ProcessCommandAsync("no disable");
            await device.ProcessCommandAsync("untagged vlan 10");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface vlan 10");
            await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show running-config");
            Assert.Contains("interface 1", output);
            Assert.Contains("name \"Server Connection\"", output);
            Assert.Contains("untagged vlan 10", output);
            Assert.Contains("interface vlan 10", output);
            Assert.Contains("ip address 192.168.1.1", output);
        }

        // Test 9: Show BGP Summary
        [Fact]
        public async Task ArubaShowBgpSummaryShouldDisplayPeerStatus()
        {
            var device = new ArubaDevice("R1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("router bgp 65001");
            await device.ProcessCommandAsync("router-id 1.1.1.1");
            await device.ProcessCommandAsync("neighbor 172.16.0.2 remote-as 65002");
            await device.ProcessCommandAsync("network 10.0.0.0/24");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show ip bgp summary");
            Assert.Contains("BGP Peer Information", output);
            Assert.Contains("172.16.0.2", output);
            Assert.Contains("65002", output);
            Assert.Contains("65001", output);
        }

        // Test 10: Shutdown / No Shutdown
        [Fact]
        public async Task ArubaShutdownNoShutdownShouldToggleInterface()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface 1");
            await device.ProcessCommandAsync("no disable");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show interfaces brief");
            Assert.Contains("1", output);
            Assert.Contains("Up", output);

            await device.ProcessCommandAsync("interface 1");
            await device.ProcessCommandAsync("disable");
            await device.ProcessCommandAsync("exit");

            output = await device.ProcessCommandAsync("show interfaces brief");
            Assert.Contains("Down", output);
        }

        // Test 11: Show Version
        [Fact]
        public async Task ArubaShowVersionShouldDisplaySystemInfo()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            var output = await device.ProcessCommandAsync("show version");

            Assert.Contains("Image stamp:", output);
            Assert.Contains("Boot Image:", output);
            Assert.Contains("Version information", output);
        }

        // Test 12: Show ARP
        [Fact]
        public async Task ArubaShowArpShouldDisplayArpTable()
        {
            var device = new ArubaDevice("R1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface vlan 1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show ip arp");
            Assert.Contains("IP ARP table", output);
            Assert.Contains("IP Address", output);
            Assert.Contains("MAC Address", output);
            Assert.Contains("10.0.0.1", output);
        }

        // Test 13: Show MAC Address-Table
        [Fact]
        public async Task ArubaShowMacAddressTableShouldDisplayMacEntries()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            var output = await device.ProcessCommandAsync("show mac-address");

            Assert.Contains("Status and Counters - Port Address Table", output);
            Assert.Contains("MAC Address", output);
            Assert.Contains("Port", output);
            Assert.Contains("VLAN", output);
        }

        // Test 14: Show IP Interface Brief
        [Fact]
        public async Task ArubaShowInterfaceBriefShouldDisplaySummary()
        {
            var device = new ArubaDevice("R1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface vlan 1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface vlan 10");
            await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show ip interface brief");
            Assert.Contains("Internet (IP) Service", output);
            Assert.Contains("Interface", output);
            Assert.Contains("IP Address", output);
            Assert.Contains("vlan 1", output);
            Assert.Contains("10.0.0.1", output);
            Assert.Contains("vlan 10", output);
            Assert.Contains("192.168.1.1", output);
        }

        // Test 15: Write Memory
        [Fact]
        public async Task ArubaWriteMemoryShouldSaveConfiguration()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("hostname Aruba-Test");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("write memory");
            Assert.DoesNotContain("Invalid", output);

            output = await device.ProcessCommandAsync("copy running-config startup-config");
            Assert.DoesNotContain("Invalid", output);
        }

        // Test 16: Show Spanning-Tree
        [Fact]
        public async Task ArubaShowSpanningTreeShouldDisplayStpInfo()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("spanning-tree mode rapid-pvst");
            await device.ProcessCommandAsync("spanning-tree priority 4096");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show spanning-tree");
            Assert.Contains("Multiple Spanning Trees", output);
            Assert.Contains("CST", output);
            Assert.Contains("Root ID", output);
            Assert.Contains("Priority", output);
            Assert.Contains("4096", output);
        }

        // Test 17: Show Access-Lists
        [Fact]
        public async Task ArubaShowAccessListsShouldDisplayAcls()
        {
            var device = new ArubaDevice("R1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("ip access-list standard BLOCK_NETWORK");
            await device.ProcessCommandAsync("deny 192.168.1.0 0.0.0.255");
            await device.ProcessCommandAsync("permit any");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show running-config");
            Assert.Contains("ip access-list standard BLOCK_NETWORK", output);
            Assert.Contains("deny", output);
            Assert.Contains("permit any", output);
        }

        // Test 18: Show Trunk (Port-Channel)
        [Fact]
        public async Task ArubaShowTrunkShouldDisplayLagStatus()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface 1");
            await device.ProcessCommandAsync("trunk 1 lacp");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("interface 2");
            await device.ProcessCommandAsync("trunk 1 lacp");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show trunk");
            Assert.Contains("Load Balancing Method", output);
            Assert.Contains("Port", output);
            Assert.Contains("Type", output);
            Assert.Contains("Group", output);
            Assert.Contains("lacp", output);
        }

        // Test 19: Reload
        [Fact]
        public async Task ArubaReloadShouldPromptForConfirmation()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            var output = await device.ProcessCommandAsync("reload");

            Assert.Contains("This command will reboot the device", output);
            Assert.Contains("Continue", output);
        }

        // Test 20: Show Logging
        [Fact]
        public async Task ArubaShowLoggingShouldDisplayLogEntries()
        {
            var device = new ArubaDevice("SW1");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure");
            await device.ProcessCommandAsync("interface 1");
            await device.ProcessCommandAsync("disable");
            await device.ProcessCommandAsync("no disable");
            await device.ProcessCommandAsync("exit");

            var output = await device.ProcessCommandAsync("show logging");
            Assert.Contains("Event Log", output);
            Assert.Contains("port 1", output);
            Assert.Contains("System coldstart", output);
        }
    }
}
