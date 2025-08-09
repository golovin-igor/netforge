using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Aruba
{
    public class ArubaDeviceTests
    {
        // Test 1: Show Running-Configuration
        [Fact]
        public void Aruba_ShowRunningConfig_ShouldIncludeAllConfigurations()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("hostname Aruba-Core");
            device.ProcessCommand("vlan 10");
            device.ProcessCommand("name SALES");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface 1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("exit");
            device.ProcessCommand("router ospf");
            device.ProcessCommand("area 0.0.0.0 range 10.0.0.0 255.255.255.0");
            device.ProcessCommand("enable");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show running-config");
            Assert.Contains("hostname \"Aruba-Core\"", output);
            Assert.Contains("vlan 10", output);
            Assert.Contains("name \"SALES\"", output);
            Assert.Contains("ip address 10.0.0.1 255.255.255.0", output);
            Assert.Contains("router ospf", output);
        }

        // Test 2: Show IP Route
        [Fact]
        public void Aruba_ShowIpRoute_ShouldDisplayAllRouteTypes()
        {
            var device = new ArubaDevice("R1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface vlan 1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("exit");
            device.ProcessCommand("ip route 192.168.1.0 255.255.255.0 10.0.0.2");
            device.ProcessCommand("router ospf");
            device.ProcessCommand("area 0.0.0.0 range 10.0.0.0 255.255.255.0");
            device.ProcessCommand("enable");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show ip route");
            Assert.Contains("10.0.0.0/24", output);
            Assert.Contains("192.168.1.0/24", output);
            Assert.Contains("connected", output);
            Assert.Contains("static", output);
        }

        // Test 3: Ping
        [Fact]
        public void Aruba_Ping_ShouldShowSuccessAndFailure()
        {
            var device = new ArubaDevice("R1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface vlan 1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("exit");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("ping 10.0.0.2");
            Assert.Contains("5 packets transmitted, 5 packets received, 0.0% packet loss", output);
            
            output = device.ProcessCommand("ping 192.168.99.99");
            Assert.Contains("5 packets transmitted, 0 packets received, 100.0% packet loss", output);
        }

        // Test 4: Show Interface
        [Fact]
        public void Aruba_ShowInterface_ShouldDisplayInterfaceDetails()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface 1");
            device.ProcessCommand("name \"WAN Link\"");
            device.ProcessCommand("no disable");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface vlan 10");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show interfaces 1");
            Assert.Contains("Status and Counters", output);
            Assert.Contains("WAN Link", output);
            Assert.Contains("Link Status", output);
            
            output = device.ProcessCommand("show interfaces vlan 10");
            Assert.Contains("VLAN", output);
            Assert.Contains("10.0.0.1", output);
        }

        // Test 5: Configure Mode
        [Fact]
        public void Aruba_Configure_ShouldEnterConfigMode()
        {
            var device = new ArubaDevice("SW1");
            var output = device.ProcessCommand("enable");
            Assert.Contains("SW1#", output);
            
            output = device.ProcessCommand("configure");
            Assert.Contains("SW1(config)#", output);
            
            output = device.ProcessCommand("interface 1");
            Assert.Contains("SW1(eth-1)#", output);
            
            output = device.ProcessCommand("exit");
            Assert.Contains("SW1(config)#", output);
        }

        // Test 6: Show VLAN
        [Fact]
        public void Aruba_ShowVlan_ShouldDisplayVlanConfiguration()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("vlan 10");
            device.ProcessCommand("name SALES");
            device.ProcessCommand("exit");
            device.ProcessCommand("vlan 20");
            device.ProcessCommand("name MARKETING");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface 1");
            device.ProcessCommand("untagged vlan 10");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show vlan");
            Assert.Contains("10", output);
            Assert.Contains("SALES", output);
            Assert.Contains("20", output);
            Assert.Contains("MARKETING", output);
            Assert.Contains("Port-based", output);
        }

        // Test 7: Show OSPF Neighbor
        [Fact]
        public void Aruba_ShowOspfNeighbor_ShouldDisplayNeighbors()
        {
            var device = new ArubaDevice("R1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("router ospf");
            device.ProcessCommand("router-id 1.1.1.1");
            device.ProcessCommand("area 0.0.0.0 range 10.0.0.0 255.255.255.0");
            device.ProcessCommand("enable");
            device.ProcessCommand("exit");
            
            // Simulate neighbor
            // TODO: Update this when command handler architecture is complete
            // var ospfConfig = device.GetOspfConfig();
            // if (ospfConfig != null)
            // {
            //     ospfConfig.AddNeighbor("2.2.2.2", "10.0.0.2", "vlan 1", "DR");
            // }
            
            var output = device.ProcessCommand("show ip ospf neighbor");
            Assert.Contains("2.2.2.2", output);
            Assert.Contains("10.0.0.2", output);
            Assert.Contains("DR", output);
        }

        // Test 8: Interface Configuration
        [Fact]
        public void Aruba_InterfaceConfiguration_ShouldApplySettings()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface 1");
            device.ProcessCommand("name \"Server Connection\"");
            device.ProcessCommand("speed 1000");
            device.ProcessCommand("duplex full");
            device.ProcessCommand("no disable");
            device.ProcessCommand("untagged vlan 10");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface vlan 10");
            device.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show running-config");
            Assert.Contains("interface 1", output);
            Assert.Contains("name \"Server Connection\"", output);
            Assert.Contains("untagged vlan 10", output);
            Assert.Contains("interface vlan 10", output);
            Assert.Contains("ip address 192.168.1.1", output);
        }

        // Test 9: Show BGP Summary
        [Fact]
        public void Aruba_ShowBgpSummary_ShouldDisplayPeerStatus()
        {
            var device = new ArubaDevice("R1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("router bgp 65001");
            device.ProcessCommand("router-id 1.1.1.1");
            device.ProcessCommand("neighbor 172.16.0.2 remote-as 65002");
            device.ProcessCommand("network 10.0.0.0/24");
            device.ProcessCommand("enable");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show ip bgp summary");
            Assert.Contains("BGP Peer Information", output);
            Assert.Contains("172.16.0.2", output);
            Assert.Contains("65002", output);
            Assert.Contains("65001", output);
        }

        // Test 10: Shutdown / No Shutdown
        [Fact]
        public void Aruba_ShutdownNoShutdown_ShouldToggleInterface()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface 1");
            device.ProcessCommand("no disable");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show interfaces brief");
            Assert.Contains("1", output);
            Assert.Contains("Up", output);
            
            device.ProcessCommand("interface 1");
            device.ProcessCommand("disable");
            device.ProcessCommand("exit");
            
            output = device.ProcessCommand("show interfaces brief");
            Assert.Contains("Down", output);
        }

        // Test 11: Show Version
        [Fact]
        public void Aruba_ShowVersion_ShouldDisplaySystemInfo()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            var output = device.ProcessCommand("show version");
            
            Assert.Contains("Image stamp:", output);
            Assert.Contains("Boot Image:", output);
            Assert.Contains("Version information", output);
        }

        // Test 12: Show ARP
        [Fact]
        public void Aruba_ShowArp_ShouldDisplayArpTable()
        {
            var device = new ArubaDevice("R1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface vlan 1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("exit");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show ip arp");
            Assert.Contains("IP ARP table", output);
            Assert.Contains("IP Address", output);
            Assert.Contains("MAC Address", output);
            Assert.Contains("10.0.0.1", output);
        }

        // Test 13: Show MAC Address-Table
        [Fact]
        public void Aruba_ShowMacAddressTable_ShouldDisplayMacEntries()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            var output = device.ProcessCommand("show mac-address");
            
            Assert.Contains("Status and Counters - Port Address Table", output);
            Assert.Contains("MAC Address", output);
            Assert.Contains("Port", output);
            Assert.Contains("VLAN", output);
        }

        // Test 14: Show IP Interface Brief
        [Fact]
        public void Aruba_ShowInterfaceBrief_ShouldDisplaySummary()
        {
            var device = new ArubaDevice("R1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface vlan 1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface vlan 10");
            device.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show ip interface brief");
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
        public void Aruba_WriteMemory_ShouldSaveConfiguration()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("hostname Aruba-Test");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("write memory");
            Assert.DoesNotContain("Invalid", output);
            
            output = device.ProcessCommand("copy running-config startup-config");
            Assert.DoesNotContain("Invalid", output);
        }

        // Test 16: Show Spanning-Tree
        [Fact]
        public void Aruba_ShowSpanningTree_ShouldDisplayStpInfo()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("spanning-tree mode rapid-pvst");
            device.ProcessCommand("spanning-tree priority 4096");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show spanning-tree");
            Assert.Contains("Multiple Spanning Trees", output);
            Assert.Contains("CST", output);
            Assert.Contains("Root ID", output);
            Assert.Contains("Priority", output);
            Assert.Contains("4096", output);
        }

        // Test 17: Show Access-Lists
        [Fact]
        public void Aruba_ShowAccessLists_ShouldDisplayAcls()
        {
            var device = new ArubaDevice("R1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("ip access-list standard BLOCK_NETWORK");
            device.ProcessCommand("deny 192.168.1.0 0.0.0.255");
            device.ProcessCommand("permit any");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show running-config");
            Assert.Contains("ip access-list standard BLOCK_NETWORK", output);
            Assert.Contains("deny", output);
            Assert.Contains("permit any", output);
        }

        // Test 18: Show Trunk (Port-Channel)
        [Fact]
        public void Aruba_ShowTrunk_ShouldDisplayLagStatus()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface 1");
            device.ProcessCommand("trunk 1 lacp");
            device.ProcessCommand("exit");
            device.ProcessCommand("interface 2");
            device.ProcessCommand("trunk 1 lacp");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show trunk");
            Assert.Contains("Load Balancing Method", output);
            Assert.Contains("Port", output);
            Assert.Contains("Type", output);
            Assert.Contains("Group", output);
            Assert.Contains("lacp", output);
        }

        // Test 19: Reload
        [Fact]
        public void Aruba_Reload_ShouldPromptForConfirmation()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            var output = device.ProcessCommand("reload");
            
            Assert.Contains("This command will reboot the device", output);
            Assert.Contains("Continue", output);
        }

        // Test 20: Show Logging
        [Fact]
        public void Aruba_ShowLogging_ShouldDisplayLogEntries()
        {
            var device = new ArubaDevice("SW1");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure");
            device.ProcessCommand("interface 1");
            device.ProcessCommand("disable");
            device.ProcessCommand("no disable");
            device.ProcessCommand("exit");
            
            var output = device.ProcessCommand("show logging");
            Assert.Contains("Event Log", output);
            Assert.Contains("port 1", output);
            Assert.Contains("System coldstart", output);
        }
    }
} 
