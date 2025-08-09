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
            
            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure terminal");
            await sw1.ProcessCommandAsync("vlan 10");
            await sw1.ProcessCommandAsync("name SALES");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await sw1.ProcessCommandAsync("switchport mode access");
            await sw1.ProcessCommandAsync("switchport access vlan 10");
            await sw1.ProcessCommandAsync("ip address 192.168.10.1 255.255.255.0");
            await sw1.ProcessCommandAsync("no shutdown");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("exit");

            await sw2.ProcessCommandAsync("enable");
            await sw2.ProcessCommandAsync("configure terminal");
            await sw2.ProcessCommandAsync("vlan 10");
            await sw2.ProcessCommandAsync("name SALES");
            await sw2.ProcessCommandAsync("exit");
            await sw2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await sw2.ProcessCommandAsync("switchport mode access");
            await sw2.ProcessCommandAsync("switchport access vlan 10");
            await sw2.ProcessCommandAsync("ip address 192.168.10.2 255.255.255.0");
            await sw2.ProcessCommandAsync("no shutdown");
            await sw2.ProcessCommandAsync("exit");
            await sw2.ProcessCommandAsync("exit");

            var vlanOutput = await sw1.ProcessCommandAsync("show vlan brief");
            Assert.Contains("10   SALES", vlanOutput);
            Assert.Contains("active    Gi0/0", vlanOutput);

            var pingOutput = await sw1.ProcessCommandAsync("ping 192.168.10.2");
            Assert.Contains("!!!!!", pingOutput);
        }

        [Fact]
        public async Task Cisco_ConfigureOspf_ShouldFormAdjacency()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.252");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router ospf 1");
            await r1.ProcessCommandAsync("router-id 1.1.1.1");
            await r1.ProcessCommandAsync("network 10.0.0.0 0.0.0.3 area 0");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 10.0.0.2 255.255.255.252");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router ospf 1");
            await r2.ProcessCommandAsync("router-id 2.2.2.2");
            await r2.ProcessCommandAsync("network 10.0.0.0 0.0.0.3 area 0");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50); 

            var ospfNeighbors = await r1.ProcessCommandAsync("show ip ospf neighbor");
            Assert.Contains("2.2.2.2", ospfNeighbors);
            Assert.Contains("FULL", ospfNeighbors);
        }

        [Fact]
        public async Task Cisco_ConfigureBgp_ShouldEstablishPeering()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.252");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router bgp 65001");
            await r1.ProcessCommandAsync("bgp router-id 1.1.1.1");
            await r1.ProcessCommandAsync("neighbor 10.0.0.2 remote-as 65002");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 10.0.0.2 255.255.255.252");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router bgp 65002");
            await r2.ProcessCommandAsync("bgp router-id 2.2.2.2");
            await r2.ProcessCommandAsync("neighbor 10.0.0.1 remote-as 65001");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var bgpSummary = await r1.ProcessCommandAsync("show ip bgp summary");
            Assert.Contains("10.0.0.2", bgpSummary);
            Assert.Contains("Established", bgpSummary);
        }

        [Fact]
        public async Task Cisco_ConfigureRip_ShouldExchangeRoutes()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("interface Loopback0");
            await r1.ProcessCommandAsync("ip address 1.1.1.1 255.255.255.255");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router rip");
            await r1.ProcessCommandAsync("version 2");
            await r1.ProcessCommandAsync("network 10.0.0.0");
            await r1.ProcessCommandAsync("network 1.0.0.0");
            await r1.ProcessCommandAsync("no auto-summary");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 10.0.0.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("interface Loopback0");
            await r2.ProcessCommandAsync("ip address 2.2.2.2 255.255.255.255");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router rip");
            await r2.ProcessCommandAsync("version 2");
            await r2.ProcessCommandAsync("network 10.0.0.0");
            await r2.ProcessCommandAsync("network 2.0.0.0");
            await r2.ProcessCommandAsync("no auto-summary");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var ripRoutes = await r1.ProcessCommandAsync("show ip route rip");
            Assert.Contains("2.0.0.0/8", ripRoutes);
            Assert.Contains("10.0.0.2", ripRoutes);
        }
        
        [Fact]
        public async Task Cisco_InterfaceShutdown_ShouldAffectConnectivity()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.252");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 10.0.0.2 255.255.255.252");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            Assert.Contains("Success rate is 100 percent", await r1.ProcessCommandAsync("ping 10.0.0.2"));

            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            Assert.Contains("administratively down", await r1.ProcessCommandAsync("show interfaces GigabitEthernet0/0"));
            Assert.Contains("Success rate is 0 percent", await r1.ProcessCommandAsync("ping 10.0.0.2"));
        }

        [Fact]
        public async Task Cisco_ConfigureAcl_ShouldBlockTraffic()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.252");
            await r1.ProcessCommandAsync("ip access-group 101 in");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");

            await r1.ProcessCommandAsync("access-list 101 deny icmp host 10.0.0.2 host 10.0.0.1");
            await r1.ProcessCommandAsync("access-list 101 permit ip any any");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit"); 

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 10.0.0.2 255.255.255.252");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            Assert.Contains("U.U.U", await r2.ProcessCommandAsync("ping 10.0.0.1"));
        }

        [Fact]
        public async Task Cisco_ConfigureStp_ShouldElectRoot()
        {
            var (network, sw1, sw2) = await SetupNetworkWithTwoDevicesAsync("SW1", "SW2");
            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure terminal");
            await sw1.ProcessCommandAsync("spanning-tree vlan 1 priority 4096");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("exit");

            await sw2.ProcessCommandAsync("enable");
            await sw2.ProcessCommandAsync("configure terminal");
            await sw2.ProcessCommandAsync("spanning-tree vlan 1 priority 8192");
            await sw2.ProcessCommandAsync("exit");
            await sw2.ProcessCommandAsync("exit");

            network.UpdateProtocols();
            await Task.Delay(50);
            
            var stpOutput = await sw1.ProcessCommandAsync("show spanning-tree");
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
            
            await sw1.ProcessCommandAsync("enable");
            await sw1.ProcessCommandAsync("configure terminal");
            await sw1.ProcessCommandAsync("interface GigabitEthernet0/1");
            await sw1.ProcessCommandAsync("channel-group 1 mode active");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("interface GigabitEthernet0/2");
            await sw1.ProcessCommandAsync("channel-group 1 mode active");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("interface Port-channel1");
            await sw1.ProcessCommandAsync("switchport mode trunk");
            await sw1.ProcessCommandAsync("exit");
            await sw1.ProcessCommandAsync("exit");

            var portChannelSummary = await sw1.ProcessCommandAsync("show etherchannel summary");
            Assert.Contains("Po1(SU)", portChannelSummary);
            Assert.Contains("Gi0/1(P)", portChannelSummary);
            Assert.Contains("Gi0/2(P)", portChannelSummary);
        }

        [Fact]
        public async Task Cisco_CdpNeighbor_ShouldDisplayCorrectly()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("cdp run");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("cdp enable");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("cdp run");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("cdp enable");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            network.UpdateProtocols(); 
            await Task.Delay(50);

            var cdpNeighbors = await r1.ProcessCommandAsync("show cdp neighbors");
            Assert.Contains("R2", cdpNeighbors);
            Assert.Contains("Gig 0/0", cdpNeighbors);
        }

        [Fact]
        public async Task Cisco_InvalidCommand_ShouldReturnError() 
        {
            var r1 = new CiscoDevice("R1");
            var output1 = await r1.ProcessCommandAsync("invalid command");
            Assert.Contains("Invalid input detected", output1);

            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure terminal");
            var output2 = await r1.ProcessCommandAsync("interface InvalidInterface");
            Assert.Contains("Invalid interface name", output2);
            await r1.ProcessCommandAsync("exit"); // Exit config mode
            await r1.ProcessCommandAsync("exit"); // Exit enable mode
        }

        [Fact]
        public async Task Cisco_ShowStaticRoute_ShouldDisplayRoute() 
        {
            var r1 = new CiscoDevice("R1");
            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("ip route 10.10.10.0 255.255.255.0 192.168.1.254");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            var routeOutput = await r1.ProcessCommandAsync("show ip route");
            Assert.Contains("10.10.10.0/24", routeOutput);
            Assert.Contains("192.168.1.254", routeOutput);
        }
    }
} 
