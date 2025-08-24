using NetForge.Simulation.Core.Devices;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Extreme
{
    public class ExtremeDeviceTests
    {
        private async Task<(Network, ExtremeDevice, ExtremeDevice)> SetupNetworkWithTwoDevicesAsync(string dev1Name = "SW1", string dev2Name = "SW2")
        {
            var network = new Network();
            var dev1 = new ExtremeDevice(dev1Name);
            var dev2 = new ExtremeDevice(dev2Name);
            await network.AddDeviceAsync(dev1);
            await network.AddDeviceAsync(dev2);
            await network.AddLinkAsync(dev1Name, "1", dev2Name, "1");
            return (network, dev1, dev2);
        }

        // Test 1: Show Running-Configuration
        [Fact]
        public async Task ExtremeShowConfigurationShouldIncludeAllSettings()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("configure snmp sysName \"ExtremeCore\"");
            await device.ProcessCommandAsync("create vlan \"SALES\"");
            await device.ProcessCommandAsync("configure vlan SALES tag 10");
            await device.ProcessCommandAsync("configure vlan SALES add ports 1-5");
            await device.ProcessCommandAsync("configure ipaddress SALES 10.0.0.1/24");
            await device.ProcessCommandAsync("enable ospf");
            await device.ProcessCommandAsync("configure ospf add vlan SALES area 0.0.0.0");

            var output = await device.ProcessCommandAsync("show configuration");
            Assert.Contains("configure snmp sysName \"ExtremeCore\"", output);
            Assert.Contains("create vlan \"SALES\"", output);
            Assert.Contains("configure vlan \"SALES\" tag 10", output);
            Assert.Contains("configure ospf add vlan SALES", output);
        }

        // Test 2: Show IP Route
        [Fact]
        public async Task ExtremeShowIpRouteShouldDisplayAllRoutes()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("create vlan \"Mgmt\"");
            await device.ProcessCommandAsync("configure ipaddress Mgmt 10.0.0.1/24");
            await device.ProcessCommandAsync("configure iproute add 192.168.1.0/24 10.0.0.2");
            await device.ProcessCommandAsync("enable ospf");

            var output = await device.ProcessCommandAsync("show iproute");
            Assert.Contains("10.0.0.0/24", output);
            Assert.Contains("192.168.1.0/24", output);
            Assert.Contains("UC", output); // Connected
            Assert.Contains("UG", output); // Gateway/Static
        }

        // Test 3: Ping
        [Fact]
        public async Task ExtremePingShouldShowSuccessAndFailure()
        {
            var (network, sw1, sw2) = await SetupNetworkWithTwoDevicesAsync();

            // Configure first device
            var result1 = await sw1.ProcessCommandAsync("create vlan \"Test\"");
            System.Console.WriteLine($"DEBUG: Create VLAN result SW1: '{result1}'");

            var result2 = await sw1.ProcessCommandAsync("configure vlan Test add ports 1");  // Add connected interface to VLAN
            System.Console.WriteLine($"DEBUG: Add ports result SW1: '{result2}'");

            var result3 = await sw1.ProcessCommandAsync("configure ipaddress Test 10.0.0.1/24");
            System.Console.WriteLine($"DEBUG: IP address result SW1: '{result3}'");

            // Configure second device
            var result4 = await sw2.ProcessCommandAsync("create vlan \"Test\"");
            System.Console.WriteLine($"DEBUG: Create VLAN result SW2: '{result4}'");

            var result5 = await sw2.ProcessCommandAsync("configure vlan Test add ports 1");  // Add connected interface to VLAN
            System.Console.WriteLine($"DEBUG: Add ports result SW2: '{result5}'");

            var result6 = await sw2.ProcessCommandAsync("configure ipaddress Test 10.0.0.2/24");
            System.Console.WriteLine($"DEBUG: IP address result SW2: '{result6}'");

            // Debug: Check device configuration
            System.Console.WriteLine($"DEBUG: SW1 VLANs: {string.Join(", ", sw1.GetAllVlans().Select(v => $"{v.Key}:{v.Value.Name}"))}");
            System.Console.WriteLine($"DEBUG: SW2 VLANs: {string.Join(", ", sw2.GetAllVlans().Select(v => $"{v.Key}:{v.Value.Name}"))}");
            System.Console.WriteLine($"DEBUG: SW1 interfaces: {string.Join(", ", sw1.GetAllInterfaces().Select(i => $"{i.Key}:{i.Value.IpAddress}:{i.Value.VlanId}"))}");
            System.Console.WriteLine($"DEBUG: SW2 interfaces: {string.Join(", ", sw2.GetAllInterfaces().Select(i => $"{i.Key}:{i.Value.IpAddress}:{i.Value.VlanId}"))}");
            System.Console.WriteLine($"DEBUG: SW1 routing table: {string.Join(", ", sw1.GetRoutingTable().Select(r => $"{r.Network}/{r.Mask}->{r.NextHop}"))}");
            System.Console.WriteLine($"DEBUG: SW2 routing table: {string.Join(", ", sw2.GetRoutingTable().Select(r => $"{r.Network}/{r.Mask}->{r.NextHop}"))}");

            var output = await sw1.ProcessCommandAsync("ping 10.0.0.2");

            // Debug: Print the full output to understand what's happening
            System.Console.WriteLine($"DEBUG: Full ping output: '{output}'");

            Assert.Contains("4 packets transmitted, 4 packets received", output);

            output = await sw1.ProcessCommandAsync("ping 192.168.99.99");
            Assert.Contains("4 packets transmitted, 0 packets received, 100% loss", output);
        }

        // Test 4: Show Ports Information
        [Fact]
        public async Task ExtremeShowPortsShouldDisplayPortDetails()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("configure ports 1 display-string \"WAN Link\"");
            await device.ProcessCommandAsync("enable ports 1");

            var output = await device.ProcessCommandAsync("show ports 1 information detail");
            Assert.Contains("Port:", output);
            Assert.Contains("WAN Link", output);
            Assert.Contains("Link State:", output);
            Assert.Contains("Active", output);
        }

        // Test 5: Configure Commands
        [Fact]
        public async Task ExtremeConfigureShouldModifySettings()
        {
            var device = new ExtremeDevice("SW1");
            var output = await device.ProcessCommandAsync("configure snmp sysName \"TestSwitch\"");
            Assert.Contains("* TestSwitch.1 #", output);

            output = await device.ProcessCommandAsync("create vlan \"TestVlan\"");
            Assert.Contains("* TestSwitch.1 #", output);
            Assert.DoesNotContain("Error", output);
        }

        // Test 6: Show VLAN
        [Fact]
        public async Task ExtremeShowVlanShouldDisplayVlanInfo()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("create vlan \"SALES\" tag 10");
            await device.ProcessCommandAsync("create vlan \"MARKETING\" tag 20");
            await device.ProcessCommandAsync("configure vlan SALES add ports 1-5");
            await device.ProcessCommandAsync("configure vlan MARKETING add ports 6-10");

            var output = await device.ProcessCommandAsync("show vlan");
            Assert.Contains("SALES", output);
            Assert.Contains("10", output);
            Assert.Contains("MARKETING", output);
            Assert.Contains("20", output);

            output = await device.ProcessCommandAsync("show vlan detail");
            Assert.Contains("Ports:", output);
            Assert.Contains("Untag:", output);
        }

        // Test 7: Show OSPF Neighbor
        [Fact]
        public async Task ExtremeShowOspfNeighborShouldDisplayNeighbors()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("enable ospf");
            await device.ProcessCommandAsync("configure ospf routerid 1.1.1.1");
            await device.ProcessCommandAsync("create vlan \"OSPF_VLAN\"");
            await device.ProcessCommandAsync("configure ipaddress OSPF_VLAN 10.0.0.1/24");
            await device.ProcessCommandAsync("configure ospf add vlan OSPF_VLAN area 0.0.0.0");

            // Simulate neighbor
            // TODO: Update this when command handler architecture is complete
            // var ospfConfig = device.GetOspfConfig();
            // if (ospfConfig != null)
            // {
            //     ospfConfig.AddNeighbor("2.2.2.2", "10.0.0.2", "vlan 10", "Full");
            // }

            var output = await device.ProcessCommandAsync("show ospf neighbor");
            Assert.Contains("2.2.2.2", output);
            Assert.Contains("10.0.0.2", output);
            Assert.Contains("FULL", output);
        }

        // Test 8: Configure IP Address
        [Fact]
        public async Task ExtremeConfigureIpAddressShouldSetIp()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("create vlan \"Management\"");
            await device.ProcessCommandAsync("configure ipaddress Management 192.168.1.1/24");

            var output = await device.ProcessCommandAsync("show ipconfig ipv4");
            Assert.Contains("Management", output);
            Assert.Contains("192.168.1.1/24", output);
            Assert.Contains("255.255.255.0", output);
        }

        // Test 9: Show BGP Neighbor
        [Fact]
        public async Task ExtremeShowBgpNeighborShouldDisplayPeers()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("enable bgp");
            await device.ProcessCommandAsync("configure bgp as-number 65001");
            await device.ProcessCommandAsync("configure bgp routerid 1.1.1.1");
            await device.ProcessCommandAsync("configure bgp neighbor 172.16.0.2 remote-as 65002");
            await device.ProcessCommandAsync("configure bgp add network 10.0.0.0/24");

            var output = await device.ProcessCommandAsync("show bgp neighbor");
            Assert.Contains("172.16.0.2", output);
            Assert.Contains("65002", output);
            Assert.Contains("BGP Peer Table", output);
        }

        // Test 10: Enable/Disable Ports
        [Fact]
        public async Task ExtremeEnableDisablePortsShouldToggleStatus()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("enable ports 1");

            var output = await device.ProcessCommandAsync("show ports 1 information");
            Assert.Contains("Port", output);
            Assert.DoesNotContain("D", output); // Not disabled

            await device.ProcessCommandAsync("disable ports 1");
            output = await device.ProcessCommandAsync("show ports 1 information");
            Assert.Contains("X", output); // Disabled flag
        }

        // Test 11: Show Version
        [Fact]
        public async Task ExtremeShowVersionShouldDisplaySystemInfo()
        {
            var device = new ExtremeDevice("SW1");
            var output = await device.ProcessCommandAsync("show version");

            Assert.Contains("Switch", output);
            Assert.Contains("SysName", output);
            Assert.Contains("Platform", output);
            Assert.Contains("Software Version", output);
        }

        // Test 12: Show IP ARP
        [Fact]
        public async Task ExtremeShowIpArpShouldDisplayArpTable()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("create vlan \"Test\"");
            await device.ProcessCommandAsync("configure ipaddress Test 10.0.0.1/24");

            var output = await device.ProcessCommandAsync("show iparp");
            Assert.Contains("Destination", output);
            Assert.Contains("Mac", output);
            Assert.Contains("VLAN", output);
            Assert.Contains("10.0.0.1", output);
            Assert.Contains("aa:bb:cc", output);
        }

        // Test 13: Show FDB (MAC Address Table)
        [Fact]
        public async Task ExtremeShowFdbShouldDisplayMacEntries()
        {
            var device = new ExtremeDevice("SW1");
            var output = await device.ProcessCommandAsync("show fdb");

            Assert.Contains("MAC", output);
            Assert.Contains("VLAN Name", output);
            Assert.Contains("Port", output);
            Assert.Contains("aa:bb:cc:00:01:00", output);
            Assert.Contains("aa:bb:cc:00:02:00", output);
        }

        // Test 14: Show Ports Information Brief
        [Fact]
        public async Task ExtremeShowPortsBriefShouldDisplaySummary()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("enable ports 1-5");
            await device.ProcessCommandAsync("disable ports 6-10");

            var output = await device.ProcessCommandAsync("show ports information");
            Assert.Contains("Port", output);
            Assert.Contains("Display", output);
            Assert.Contains("VLAN Name", output);
            Assert.Contains("Port", output);
            Assert.Contains("State", output);
            Assert.Contains("Link", output);
        }

        // Test 15: Save Configuration
        [Fact]
        public async Task ExtremeSaveConfigurationShouldPersistConfig()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("configure snmp sysName \"TestSave\"");

            var output = await device.ProcessCommandAsync("save configuration");
            Assert.Contains("Do you want to save configuration", output);
            Assert.Contains("primary.cfg", output);
        }

        // Test 16: Show STPD
        [Fact]
        public async Task ExtremeShowStpdShouldDisplaySpanningTree()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("configure stpd s0 priority 4096");

            var output = await device.ProcessCommandAsync("show stpd");
            Assert.Contains("Name", output);
            Assert.Contains("Mode", output);
            Assert.Contains("State", output);
            Assert.Contains("Priority", output);
            Assert.Contains("4096", output);
        }

        // Test 17: Create Access-List
        [Fact]
        public async Task ExtremeCreateAccessListShouldAddAcl()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("create access-list BLOCK_NETWORK");

            var output = await device.ProcessCommandAsync("show configuration");
            Assert.Contains("create access-list BLOCK_NETWORK", output);
        }

        // Test 18: Show Sharing (Port-Channel)
        [Fact]
        public async Task ExtremeShowSharingShouldDisplayLag()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("enable sharing 1 grouping 1,2");
            await device.ProcessCommandAsync("enable sharing 3 grouping 3,4");

            var output = await device.ProcessCommandAsync("show sharing");
            Assert.Contains("Load Sharing Monitor", output);
            Assert.Contains("Master", output);
            Assert.Contains("Type", output);
            Assert.Contains("1", output);
            Assert.Contains("2", output);
        }

        // Test 19: Reboot
        [Fact]
        public async Task ExtremeRebootShouldPromptConfirmation()
        {
            var device = new ExtremeDevice("SW1");
            var output = await device.ProcessCommandAsync("reboot");

            Assert.Contains("Are you sure you want to reboot", output);
            Assert.Contains("(y/N)", output);
        }

        // Test 20: Show Log
        [Fact]
        public async Task ExtremeShowLogShouldDisplayLogEntries()
        {
            var device = new ExtremeDevice("SW1");
            await device.ProcessCommandAsync("enable ports 1");
            await device.ProcessCommandAsync("disable ports 1");

            var output = await device.ProcessCommandAsync("show log");
            Assert.Contains("Port 1", output);
            Assert.Contains("link", output);
            Assert.Contains("System started", output);
        }
    }
}
