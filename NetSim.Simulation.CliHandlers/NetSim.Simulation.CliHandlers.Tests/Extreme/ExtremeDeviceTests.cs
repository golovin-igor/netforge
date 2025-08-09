using NetSim.Simulation.Devices;
using NetSim.Simulation.Common;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Extreme
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
        public void Extreme_ShowConfiguration_ShouldIncludeAllSettings()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("configure snmp sysName \"ExtremeCore\"");
            device.ProcessCommand("create vlan \"SALES\"");
            device.ProcessCommand("configure vlan SALES tag 10");
            device.ProcessCommand("configure vlan SALES add ports 1-5");
            device.ProcessCommand("configure ipaddress SALES 10.0.0.1/24");
            device.ProcessCommand("enable ospf");
            device.ProcessCommand("configure ospf add vlan SALES area 0.0.0.0");
            
            var output = device.ProcessCommand("show configuration");
            Assert.Contains("configure snmp sysName \"ExtremeCore\"", output);
            Assert.Contains("create vlan \"SALES\"", output);
            Assert.Contains("configure vlan \"SALES\" tag 10", output);
            Assert.Contains("configure ospf add vlan SALES", output);
        }

        // Test 2: Show IP Route
        [Fact]
        public void Extreme_ShowIpRoute_ShouldDisplayAllRoutes()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("create vlan \"Mgmt\"");
            device.ProcessCommand("configure ipaddress Mgmt 10.0.0.1/24");
            device.ProcessCommand("configure iproute add 192.168.1.0/24 10.0.0.2");
            device.ProcessCommand("enable ospf");
            
            var output = device.ProcessCommand("show iproute");
            Assert.Contains("10.0.0.0/24", output);
            Assert.Contains("192.168.1.0/24", output);
            Assert.Contains("UC", output); // Connected
            Assert.Contains("UG", output); // Gateway/Static
        }

        // Test 3: Ping
        [Fact]
        public async Task Extreme_Ping_ShouldShowSuccessAndFailure()
        {
            var (network, sw1, sw2) = await SetupNetworkWithTwoDevicesAsync();
            
            // Configure first device
            var result1 = sw1.ProcessCommand("create vlan \"Test\"");
            System.Console.WriteLine($"DEBUG: Create VLAN result SW1: '{result1}'");
            
            var result2 = sw1.ProcessCommand("configure vlan Test add ports 1");  // Add connected interface to VLAN
            System.Console.WriteLine($"DEBUG: Add ports result SW1: '{result2}'");
            
            var result3 = sw1.ProcessCommand("configure ipaddress Test 10.0.0.1/24");
            System.Console.WriteLine($"DEBUG: IP address result SW1: '{result3}'");
            
            // Configure second device
            var result4 = sw2.ProcessCommand("create vlan \"Test\"");
            System.Console.WriteLine($"DEBUG: Create VLAN result SW2: '{result4}'");
            
            var result5 = sw2.ProcessCommand("configure vlan Test add ports 1");  // Add connected interface to VLAN
            System.Console.WriteLine($"DEBUG: Add ports result SW2: '{result5}'");
            
            var result6 = sw2.ProcessCommand("configure ipaddress Test 10.0.0.2/24");
            System.Console.WriteLine($"DEBUG: IP address result SW2: '{result6}'");
            
            // Debug: Check device configuration
            System.Console.WriteLine($"DEBUG: SW1 VLANs: {string.Join(", ", sw1.GetAllVlans().Select(v => $"{v.Key}:{v.Value.Name}"))}");
            System.Console.WriteLine($"DEBUG: SW2 VLANs: {string.Join(", ", sw2.GetAllVlans().Select(v => $"{v.Key}:{v.Value.Name}"))}");
            System.Console.WriteLine($"DEBUG: SW1 interfaces: {string.Join(", ", sw1.GetAllInterfaces().Select(i => $"{i.Key}:{i.Value.IpAddress}:{i.Value.VlanId}"))}");
            System.Console.WriteLine($"DEBUG: SW2 interfaces: {string.Join(", ", sw2.GetAllInterfaces().Select(i => $"{i.Key}:{i.Value.IpAddress}:{i.Value.VlanId}"))}");
            System.Console.WriteLine($"DEBUG: SW1 routing table: {string.Join(", ", sw1.GetRoutingTable().Select(r => $"{r.Network}/{r.Mask}->{r.NextHop}"))}");
            System.Console.WriteLine($"DEBUG: SW2 routing table: {string.Join(", ", sw2.GetRoutingTable().Select(r => $"{r.Network}/{r.Mask}->{r.NextHop}"))}");
            
            var output = sw1.ProcessCommand("ping 10.0.0.2");
            
            // Debug: Print the full output to understand what's happening
            System.Console.WriteLine($"DEBUG: Full ping output: '{output}'");
            
            Assert.Contains("4 packets transmitted, 4 packets received", output);
            
            output = sw1.ProcessCommand("ping 192.168.99.99");
            Assert.Contains("4 packets transmitted, 0 packets received, 100% loss", output);
        }

        // Test 4: Show Ports Information
        [Fact]
        public void Extreme_ShowPorts_ShouldDisplayPortDetails()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("configure ports 1 display-string \"WAN Link\"");
            device.ProcessCommand("enable ports 1");
            
            var output = device.ProcessCommand("show ports 1 information detail");
            Assert.Contains("Port:", output);
            Assert.Contains("WAN Link", output);
            Assert.Contains("Link State:", output);
            Assert.Contains("Active", output);
        }

        // Test 5: Configure Commands
        [Fact]
        public void Extreme_Configure_ShouldModifySettings()
        {
            var device = new ExtremeDevice("SW1");
            var output = device.ProcessCommand("configure snmp sysName \"TestSwitch\"");
            Assert.Contains("* TestSwitch.1 #", output);
            
            output = device.ProcessCommand("create vlan \"TestVlan\"");
            Assert.Contains("* TestSwitch.1 #", output);
            Assert.DoesNotContain("Error", output);
        }

        // Test 6: Show VLAN
        [Fact]
        public void Extreme_ShowVlan_ShouldDisplayVlanInfo()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("create vlan \"SALES\" tag 10");
            device.ProcessCommand("create vlan \"MARKETING\" tag 20");
            device.ProcessCommand("configure vlan SALES add ports 1-5");
            device.ProcessCommand("configure vlan MARKETING add ports 6-10");
            
            var output = device.ProcessCommand("show vlan");
            Assert.Contains("SALES", output);
            Assert.Contains("10", output);
            Assert.Contains("MARKETING", output);
            Assert.Contains("20", output);
            
            output = device.ProcessCommand("show vlan detail");
            Assert.Contains("Ports:", output);
            Assert.Contains("Untag:", output);
        }

        // Test 7: Show OSPF Neighbor
        [Fact]
        public void Extreme_ShowOspfNeighbor_ShouldDisplayNeighbors()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("enable ospf");
            device.ProcessCommand("configure ospf routerid 1.1.1.1");
            device.ProcessCommand("create vlan \"OSPF_VLAN\"");
            device.ProcessCommand("configure ipaddress OSPF_VLAN 10.0.0.1/24");
            device.ProcessCommand("configure ospf add vlan OSPF_VLAN area 0.0.0.0");
            
            // Simulate neighbor
            // TODO: Update this when command handler architecture is complete
            // var ospfConfig = device.GetOspfConfig();
            // if (ospfConfig != null)
            // {
            //     ospfConfig.AddNeighbor("2.2.2.2", "10.0.0.2", "vlan 10", "Full");
            // }
            
            var output = device.ProcessCommand("show ospf neighbor");
            Assert.Contains("2.2.2.2", output);
            Assert.Contains("10.0.0.2", output);
            Assert.Contains("FULL", output);
        }

        // Test 8: Configure IP Address
        [Fact]
        public void Extreme_ConfigureIpAddress_ShouldSetIp()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("create vlan \"Management\"");
            device.ProcessCommand("configure ipaddress Management 192.168.1.1/24");
            
            var output = device.ProcessCommand("show ipconfig ipv4");
            Assert.Contains("Management", output);
            Assert.Contains("192.168.1.1/24", output);
            Assert.Contains("255.255.255.0", output);
        }

        // Test 9: Show BGP Neighbor
        [Fact]
        public void Extreme_ShowBgpNeighbor_ShouldDisplayPeers()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("enable bgp");
            device.ProcessCommand("configure bgp as-number 65001");
            device.ProcessCommand("configure bgp routerid 1.1.1.1");
            device.ProcessCommand("configure bgp neighbor 172.16.0.2 remote-as 65002");
            device.ProcessCommand("configure bgp add network 10.0.0.0/24");
            
            var output = device.ProcessCommand("show bgp neighbor");
            Assert.Contains("172.16.0.2", output);
            Assert.Contains("65002", output);
            Assert.Contains("BGP Peer Table", output);
        }

        // Test 10: Enable/Disable Ports
        [Fact]
        public void Extreme_EnableDisablePorts_ShouldToggleStatus()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("enable ports 1");
            
            var output = device.ProcessCommand("show ports 1 information");
            Assert.Contains("Port", output);
            Assert.DoesNotContain("D", output); // Not disabled
            
            device.ProcessCommand("disable ports 1");
            output = device.ProcessCommand("show ports 1 information");
            Assert.Contains("X", output); // Disabled flag
        }

        // Test 11: Show Version
        [Fact]
        public void Extreme_ShowVersion_ShouldDisplaySystemInfo()
        {
            var device = new ExtremeDevice("SW1");
            var output = device.ProcessCommand("show version");
            
            Assert.Contains("Switch", output);
            Assert.Contains("SysName", output);
            Assert.Contains("Platform", output);
            Assert.Contains("Software Version", output);
        }

        // Test 12: Show IP ARP
        [Fact]
        public void Extreme_ShowIpArp_ShouldDisplayArpTable()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("create vlan \"Test\"");
            device.ProcessCommand("configure ipaddress Test 10.0.0.1/24");
            
            var output = device.ProcessCommand("show iparp");
            Assert.Contains("Destination", output);
            Assert.Contains("Mac", output);
            Assert.Contains("VLAN", output);
            Assert.Contains("10.0.0.1", output);
            Assert.Contains("aa:bb:cc", output);
        }

        // Test 13: Show FDB (MAC Address Table)
        [Fact]
        public void Extreme_ShowFdb_ShouldDisplayMacEntries()
        {
            var device = new ExtremeDevice("SW1");
            var output = device.ProcessCommand("show fdb");
            
            Assert.Contains("MAC", output);
            Assert.Contains("VLAN Name", output);
            Assert.Contains("Port", output);
            Assert.Contains("aa:bb:cc:00:01:00", output);
            Assert.Contains("aa:bb:cc:00:02:00", output);
        }

        // Test 14: Show Ports Information Brief
        [Fact]
        public void Extreme_ShowPortsBrief_ShouldDisplaySummary()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("enable ports 1-5");
            device.ProcessCommand("disable ports 6-10");
            
            var output = device.ProcessCommand("show ports information");
            Assert.Contains("Port", output);
            Assert.Contains("Display", output);
            Assert.Contains("VLAN Name", output);
            Assert.Contains("Port", output);
            Assert.Contains("State", output);
            Assert.Contains("Link", output);
        }

        // Test 15: Save Configuration
        [Fact]
        public void Extreme_SaveConfiguration_ShouldPersistConfig()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("configure snmp sysName \"TestSave\"");
            
            var output = device.ProcessCommand("save configuration");
            Assert.Contains("Do you want to save configuration", output);
            Assert.Contains("primary.cfg", output);
        }

        // Test 16: Show STPD
        [Fact]
        public void Extreme_ShowStpd_ShouldDisplaySpanningTree()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("configure stpd s0 priority 4096");
            
            var output = device.ProcessCommand("show stpd");
            Assert.Contains("Name", output);
            Assert.Contains("Mode", output);
            Assert.Contains("State", output);
            Assert.Contains("Priority", output);
            Assert.Contains("4096", output);
        }

        // Test 17: Create Access-List
        [Fact]
        public void Extreme_CreateAccessList_ShouldAddAcl()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("create access-list BLOCK_NETWORK");
            
            var output = device.ProcessCommand("show configuration");
            Assert.Contains("create access-list BLOCK_NETWORK", output);
        }

        // Test 18: Show Sharing (Port-Channel)
        [Fact]
        public void Extreme_ShowSharing_ShouldDisplayLag()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("enable sharing 1 grouping 1,2");
            device.ProcessCommand("enable sharing 3 grouping 3,4");
            
            var output = device.ProcessCommand("show sharing");
            Assert.Contains("Load Sharing Monitor", output);
            Assert.Contains("Master", output);
            Assert.Contains("Type", output);
            Assert.Contains("1", output);
            Assert.Contains("2", output);
        }

        // Test 19: Reboot
        [Fact]
        public void Extreme_Reboot_ShouldPromptConfirmation()
        {
            var device = new ExtremeDevice("SW1");
            var output = device.ProcessCommand("reboot");
            
            Assert.Contains("Are you sure you want to reboot", output);
            Assert.Contains("(y/N)", output);
        }

        // Test 20: Show Log
        [Fact]
        public void Extreme_ShowLog_ShouldDisplayLogEntries()
        {
            var device = new ExtremeDevice("SW1");
            device.ProcessCommand("enable ports 1");
            device.ProcessCommand("disable ports 1");
            
            var output = device.ProcessCommand("show log");
            Assert.Contains("Port 1", output);
            Assert.Contains("link", output);
            Assert.Contains("System started", output);
        }
    }
} 
