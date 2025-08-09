using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Cisco
{
    public class CiscoDeviceTests
    {
        private async Task<(Network, CiscoDevice, CiscoDevice)> SetupNetworkWithTwoDevicesAsync(string r1Name = "R1", string r2Name = "R2", string r1Interface = "GigabitEthernet0/0", string r2Interface = "GigabitEthernet0/0")
        {
            var network = new Network();
            var r1 = new CiscoDevice(r1Name);
            var r2 = new CiscoDevice(r2Name);
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync(r1Name, r1Interface, r2Name, r2Interface);
            return (network, r1, r2);
        }

        [Fact]
        public async Task Cisco_ConfigureVlanAndPing_ShouldSucceed()
        {
            var (network, sw1, sw2) = await SetupNetworkWithTwoDevicesAsync("SW1", "SW2");
            
            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure terminal");
            sw1.ProcessCommand("vlan 10");
            sw1.ProcessCommand("name SALES");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("interface GigabitEthernet0/0");
            sw1.ProcessCommand("switchport mode access");
            sw1.ProcessCommand("switchport access vlan 10");
            sw1.ProcessCommand("ip address 192.168.10.1 255.255.255.0");
            sw1.ProcessCommand("no shutdown");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("exit");

            sw2.ProcessCommand("enable");
            sw2.ProcessCommand("configure terminal");
            sw2.ProcessCommand("vlan 10");
            sw2.ProcessCommand("name SALES");
            sw2.ProcessCommand("exit");
            sw2.ProcessCommand("interface GigabitEthernet0/0");
            sw2.ProcessCommand("switchport mode access");
            sw2.ProcessCommand("switchport access vlan 10");
            sw2.ProcessCommand("ip address 192.168.10.2 255.255.255.0");
            sw2.ProcessCommand("no shutdown");
            sw2.ProcessCommand("exit");
            sw2.ProcessCommand("exit");

            var vlanOutput = sw1.ProcessCommand("show vlan brief");
            Assert.Contains("10   SALES", vlanOutput);
            Assert.Contains("active    Gi0/0", vlanOutput);

            var pingOutput = sw1.ProcessCommand("ping 192.168.10.2");
            Assert.Contains("!!!!!", pingOutput);
        }

        [Fact]
        public async Task Cisco_ConfigureOspf_ShouldFormAdjacency()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure terminal");
            r1.ProcessCommand("interface GigabitEthernet0/0");
            r1.ProcessCommand("ip address 10.0.0.1 255.255.255.252");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router ospf 1");
            r1.ProcessCommand("router-id 1.1.1.1");
            r1.ProcessCommand("network 10.0.0.0 0.0.0.3 area 0");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure terminal");
            r2.ProcessCommand("interface GigabitEthernet0/0");
            r2.ProcessCommand("ip address 10.0.0.2 255.255.255.252");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router ospf 1");
            r2.ProcessCommand("router-id 2.2.2.2");
            r2.ProcessCommand("network 10.0.0.0 0.0.0.3 area 0");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50); 

            var ospfNeighbors = r1.ProcessCommand("show ip ospf neighbor");
            Assert.Contains("2.2.2.2", ospfNeighbors);
            Assert.Contains("FULL", ospfNeighbors);
        }

        [Fact]
        public async Task Cisco_ConfigureBgp_ShouldEstablishPeering()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure terminal");
            r1.ProcessCommand("interface GigabitEthernet0/0");
            r1.ProcessCommand("ip address 10.0.0.1 255.255.255.252");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router bgp 65001");
            r1.ProcessCommand("bgp router-id 1.1.1.1");
            r1.ProcessCommand("neighbor 10.0.0.2 remote-as 65002");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure terminal");
            r2.ProcessCommand("interface GigabitEthernet0/0");
            r2.ProcessCommand("ip address 10.0.0.2 255.255.255.252");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router bgp 65002");
            r2.ProcessCommand("bgp router-id 2.2.2.2");
            r2.ProcessCommand("neighbor 10.0.0.1 remote-as 65001");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var bgpSummary = r1.ProcessCommand("show ip bgp summary");
            Assert.Contains("10.0.0.2", bgpSummary);
            Assert.Contains("Established", bgpSummary);
        }

        [Fact]
        public async Task Cisco_ConfigureRip_ShouldExchangeRoutes()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure terminal");
            r1.ProcessCommand("interface GigabitEthernet0/0");
            r1.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("interface Loopback0");
            r1.ProcessCommand("ip address 1.1.1.1 255.255.255.255");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router rip");
            r1.ProcessCommand("version 2");
            r1.ProcessCommand("network 10.0.0.0");
            r1.ProcessCommand("network 1.0.0.0");
            r1.ProcessCommand("no auto-summary");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure terminal");
            r2.ProcessCommand("interface GigabitEthernet0/0");
            r2.ProcessCommand("ip address 10.0.0.2 255.255.255.0");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("interface Loopback0");
            r2.ProcessCommand("ip address 2.2.2.2 255.255.255.255");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router rip");
            r2.ProcessCommand("version 2");
            r2.ProcessCommand("network 10.0.0.0");
            r2.ProcessCommand("network 2.0.0.0");
            r2.ProcessCommand("no auto-summary");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var ripRoutes = r1.ProcessCommand("show ip route rip");
            Assert.Contains("2.0.0.0/8", ripRoutes);
            Assert.Contains("10.0.0.2", ripRoutes);
        }
        
        [Fact]
        public async Task Cisco_InterfaceShutdown_ShouldAffectConnectivity()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure terminal");
            r1.ProcessCommand("interface GigabitEthernet0/0");
            r1.ProcessCommand("ip address 10.0.0.1 255.255.255.252");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure terminal");
            r2.ProcessCommand("interface GigabitEthernet0/0");
            r2.ProcessCommand("ip address 10.0.0.2 255.255.255.252");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");

            Assert.Contains("Success rate is 100 percent", r1.ProcessCommand("ping 10.0.0.2"));

            r1.ProcessCommand("configure terminal");
            r1.ProcessCommand("interface GigabitEthernet0/0");
            r1.ProcessCommand("shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            Assert.Contains("administratively down", r1.ProcessCommand("show interfaces GigabitEthernet0/0"));
            Assert.Contains("Success rate is 0 percent", r1.ProcessCommand("ping 10.0.0.2"));
        }

        [Fact]
        public async Task Cisco_ConfigureAcl_ShouldBlockTraffic()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure terminal");
            r1.ProcessCommand("interface GigabitEthernet0/0");
            r1.ProcessCommand("ip address 10.0.0.1 255.255.255.252");
            r1.ProcessCommand("ip access-group 101 in");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");

            r1.ProcessCommand("access-list 101 deny icmp host 10.0.0.2 host 10.0.0.1");
            r1.ProcessCommand("access-list 101 permit ip any any");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit"); 

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure terminal");
            r2.ProcessCommand("interface GigabitEthernet0/0");
            r2.ProcessCommand("ip address 10.0.0.2 255.255.255.252");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");

            Assert.Contains("U.U.U", r2.ProcessCommand("ping 10.0.0.1"));
        }

        [Fact]
        public async Task Cisco_ConfigureStp_ShouldElectRoot()
        {
            var (network, sw1, sw2) = await SetupNetworkWithTwoDevicesAsync("SW1", "SW2");
            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure terminal");
            sw1.ProcessCommand("spanning-tree vlan 1 priority 4096");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("exit");

            sw2.ProcessCommand("enable");
            sw2.ProcessCommand("configure terminal");
            sw2.ProcessCommand("spanning-tree vlan 1 priority 8192");
            sw2.ProcessCommand("exit");
            sw2.ProcessCommand("exit");

            network.UpdateProtocols();
            await Task.Delay(50);
            
            var stpOutput = sw1.ProcessCommand("show spanning-tree");
            Assert.Contains("This bridge is the root", stpOutput);
        }

        [Fact]
        public async Task Cisco_ConfigurePortChannel_ShouldShowMembers()
        {
            var network = new Network();
            var sw1 = new CiscoDevice("SW1");
            var sw2 = new CiscoDevice("SW2");
            await network.AddDeviceAsync(sw1);
            await network.AddDeviceAsync(sw2);
            await network.AddLinkAsync("SW1", "GigabitEthernet0/1", "SW2", "GigabitEthernet0/1");
            await network.AddLinkAsync("SW1", "GigabitEthernet0/2", "SW2", "GigabitEthernet0/2");
            
            sw1.ProcessCommand("enable");
            sw1.ProcessCommand("configure terminal");
            sw1.ProcessCommand("interface GigabitEthernet0/1");
            sw1.ProcessCommand("channel-group 1 mode active");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("interface GigabitEthernet0/2");
            sw1.ProcessCommand("channel-group 1 mode active");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("interface Port-channel1");
            sw1.ProcessCommand("switchport mode trunk");
            sw1.ProcessCommand("exit");
            sw1.ProcessCommand("exit");

            var portChannelSummary = sw1.ProcessCommand("show etherchannel summary");
            Assert.Contains("Po1(SU)", portChannelSummary);
            Assert.Contains("Gi0/1(P)", portChannelSummary);
            Assert.Contains("Gi0/2(P)", portChannelSummary);
        }

        [Fact]
        public async Task Cisco_CdpNeighbor_ShouldDisplayCorrectly()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure terminal");
            r1.ProcessCommand("cdp run");
            r1.ProcessCommand("interface GigabitEthernet0/0");
            r1.ProcessCommand("cdp enable");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure terminal");
            r2.ProcessCommand("cdp run");
            r2.ProcessCommand("interface GigabitEthernet0/0");
            r2.ProcessCommand("cdp enable");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");

            network.UpdateProtocols(); 
            await Task.Delay(50);

            var cdpNeighbors = r1.ProcessCommand("show cdp neighbors");
            Assert.Contains("R2", cdpNeighbors);
            Assert.Contains("Gig 0/0", cdpNeighbors);
        }

        [Fact]
        public void Cisco_InvalidCommand_ShouldReturnError() 
        {
            var r1 = new CiscoDevice("R1");
            var output1 = r1.ProcessCommand("invalid command");
            Assert.Contains("Invalid input detected", output1);

            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure terminal");
            var output2 = r1.ProcessCommand("interface InvalidInterface");
            Assert.Contains("Invalid interface name", output2);
            r1.ProcessCommand("exit"); // Exit config mode
            r1.ProcessCommand("exit"); // Exit enable mode
        }

        [Fact]
        public void Cisco_ShowStaticRoute_ShouldDisplayRoute() 
        {
            var r1 = new CiscoDevice("R1");
            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure terminal");
            r1.ProcessCommand("ip route 10.10.10.0 255.255.255.0 192.168.1.254");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            var routeOutput = r1.ProcessCommand("show ip route");
            Assert.Contains("10.10.10.0/24", routeOutput);
            Assert.Contains("192.168.1.254", routeOutput);
        }
    }
} 
