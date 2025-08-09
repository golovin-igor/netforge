using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.MikroTik
{
    public class MikroTikDeviceTests
    {
        private async Task<(Network, MikroTikDevice, MikroTikDevice)> SetupNetworkWithTwoDevicesAsync()
        {
            var network = new Network();
            var r1 = new MikroTikDevice("R1");
            var r2 = new MikroTikDevice("R2");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "ether1", "R2", "ether1");
            return (network, r1, r2);
        }

        // Test 1: Export Configuration
        [Fact]
        public async Task MikroTik_Export_ShouldShowFullConfiguration()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/system identity set name=\"MikroTik-Core\"");
            await device.ProcessCommandAsync("/interface vlan add vlan-id=10 interface=bridge name=vlan10");
            await device.ProcessCommandAsync("/ip address add address=10.0.0.1/24 interface=ether1");
            await device.ProcessCommandAsync("/routing ospf network add network=10.0.0.0/24 area=backbone");
            
            var output = await device.ProcessCommandAsync("/export");
            Assert.Contains("/system identity set name=\"MikroTik-Core\"", output);
            Assert.Contains("/interface vlan add vlan-id=10", output);
            Assert.Contains("/ip address add address=10.0.0.1/24", output);
            Assert.Contains("/routing ospf network add", output);
        }

        // Test 2: IP Route Print
        [Fact]
        public async Task MikroTik_IpRoutePrint_ShouldDisplayAllRoutes()
        {
            var device = new MikroTikDevice("R1");
            await device.ProcessCommandAsync("/ip address add address=10.0.0.1/24 interface=ether1");
            await device.ProcessCommandAsync("/ip route add dst-address=192.168.1.0/24 gateway=10.0.0.2");
            await device.ProcessCommandAsync("/routing ospf instance set 0 router-id=1.1.1.1");
            await device.ProcessCommandAsync("/routing ospf network add network=10.0.0.0/24 area=backbone");
            
            var output = await device.ProcessCommandAsync("/ip route print");
            Assert.Contains("10.0.0.0/24", output);
            Assert.Contains("192.168.1.0/24", output);
            Assert.Contains("ADC", output); // Connected
            Assert.Contains("AS", output); // Static
        }

        // Test 3: Ping
        [Fact]
        public async Task MikroTik_Ping_ShouldShowSuccessAndFailure()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();

            await r1.ProcessCommandAsync("/ip address add address=192.168.1.1/24 interface=ether1");
            await r2.ProcessCommandAsync("/ip address add address=192.168.1.2/24 interface=ether1");

            var output = await r1.ProcessCommandAsync("/ping 192.168.1.2");
            Assert.Contains("sent=5 received=5 packet-loss=0%", output);
            
            output = await r1.ProcessCommandAsync("/ping 192.168.99.99");
            Assert.Contains("sent=5 received=0 packet-loss=100%", output);
        }

        // Test 4: Interface Print
        [Fact]
        public async Task MikroTik_InterfacePrint_ShouldShowInterfaceDetails()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/interface ethernet set ether1 comment=\"WAN Link\"");
            await device.ProcessCommandAsync("/ip address add address=10.0.0.1/24 interface=ether1");
            await device.ProcessCommandAsync("/interface ethernet enable ether1");
            
            var output = await device.ProcessCommandAsync("/interface print");
            Assert.Contains("ether1", output);
            Assert.Contains("R", output); // Running flag
            Assert.Contains("ether", output); // Type
            
            output = await device.ProcessCommandAsync("/interface ethernet monitor ether1");
            Assert.Contains("name: ether1", output);
            Assert.Contains("status: link-ok", output);
        }

        // Test 5: System Identity
        [Fact]
        public async Task MikroTik_SystemIdentity_ShouldChangeHostname()
        {
            var device = new MikroTikDevice("MT1");
            var output = await device.ProcessCommandAsync("/system identity set name=\"RouterTest\"");
            Assert.Contains("[RouterTest] >", output);
            
            output = await device.ProcessCommandAsync("/system identity print");
            Assert.Contains("name: RouterTest", output);
        }

        // Test 6: VLAN Configuration
        [Fact]
        public async Task MikroTik_VlanConfiguration_ShouldCreateAndDisplay()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/interface vlan add vlan-id=10 interface=bridge name=SALES");
            await device.ProcessCommandAsync("/interface vlan add vlan-id=20 interface=bridge name=MARKETING");
            await device.ProcessCommandAsync("/interface bridge port add interface=ether1 bridge=bridge pvid=10");
            
            var output = await device.ProcessCommandAsync("/interface vlan print");
            Assert.Contains("10", output);
            Assert.Contains("SALES", output);
            Assert.Contains("20", output);
            Assert.Contains("MARKETING", output);
        }

        // Test 7: OSPF Neighbor
        [Fact]
        public async Task MikroTik_OspfNeighbor_ShouldShowNeighbors()
        {
            var device = new MikroTikDevice("R1");
            await device.ProcessCommandAsync("/routing ospf instance set 0 router-id=1.1.1.1");
            await device.ProcessCommandAsync("/routing ospf network add network=10.0.0.0/24 area=backbone");
            
            // The output will show configured OSPF but no neighbors without actual neighbor discovery
            var output = await device.ProcessCommandAsync("/routing ospf neighbor print");
            Assert.Contains("NEIGHBOR", output);
            Assert.Contains("STATE", output);
            // In a real environment, neighbors would appear here
        }

        // Test 8: IP Address Configuration
        [Fact]
        public async Task MikroTik_IpAddressConfiguration_ShouldApplySettings()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/interface ethernet set 0 comment=\"Server Connection\"");
            await device.ProcessCommandAsync("/ip address add address=192.168.1.1/24 interface=ether1");
            await device.ProcessCommandAsync("/interface vlan add vlan-id=10 interface=bridge name=vlan10");
            await device.ProcessCommandAsync("/ip address add address=10.10.10.1/24 interface=vlan10");
            
            var output = await device.ProcessCommandAsync("/ip address print");
            Assert.Contains("192.168.1.1/24", output);
            Assert.Contains("ether1", output);
            Assert.Contains("10.10.10.1/24", output);
            Assert.Contains("vlan10", output);
        }

        // Test 9: BGP Peer
        [Fact]
        public async Task MikroTik_BgpPeer_ShouldShowPeerStatus()
        {
            var device = new MikroTikDevice("R1");
            await device.ProcessCommandAsync("/routing bgp instance set default as=65001 router-id=1.1.1.1");
            await device.ProcessCommandAsync("/routing bgp peer add remote-address=172.16.0.2 remote-as=65002");
            await device.ProcessCommandAsync("/routing bgp network add network=10.0.0.0/24");
            
            var output = await device.ProcessCommandAsync("/routing bgp peer print");
            Assert.Contains("172.16.0.2", output);
            Assert.Contains("65002", output);
            Assert.Contains("default", output);
        }

        // Test 10: Enable/Disable Interface
        [Fact]
        public async Task MikroTik_EnableDisableInterface_ShouldToggleStatus()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/ip address add address=10.0.0.1/24 interface=ether1");
            await device.ProcessCommandAsync("/interface ethernet enable ether1");
            
            var output = await device.ProcessCommandAsync("/interface print");
            Assert.Contains("R", output); // Running
            Assert.Contains("ether1", output);
            
            await device.ProcessCommandAsync("/interface ethernet disable ether1");
            output = await device.ProcessCommandAsync("/interface print");
            Assert.Contains("X", output); // Disabled
        }

        // Test 11: System Resource Print
        [Fact]
        public async Task MikroTik_SystemResourcePrint_ShouldShowSystemInfo()
        {
            var device = new MikroTikDevice("MT1");
            var output = await device.ProcessCommandAsync("/system resource print");
            
            Assert.Contains("uptime:", output);
            Assert.Contains("version:", output);
            Assert.Contains("cpu:", output);
            Assert.Contains("cpu-count:", output);
            Assert.Contains("free-memory:", output);
            Assert.Contains("total-memory:", output);
            Assert.Contains("board-name:", output);
        }

        // Test 12: IP ARP Print
        [Fact]
        public async Task MikroTik_IpArpPrint_ShouldShowArpTable()
        {
            var device = new MikroTikDevice("R1");
            await device.ProcessCommandAsync("/ip address add address=10.0.0.1/24 interface=ether1");
            
            var output = await device.ProcessCommandAsync("/ip arp print");
            Assert.Contains("ADDRESS", output);
            Assert.Contains("MAC-ADDRESS", output);
            Assert.Contains("INTERFACE", output);
            Assert.Contains("10.0.0.1", output);
        }

        // Test 13: Interface Bridge Host Print (MAC Table)
        [Fact]
        public async Task MikroTik_BridgeHostPrint_ShouldShowMacTable()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/interface bridge add name=bridge1");
            await device.ProcessCommandAsync("/interface bridge port add interface=ether1 bridge=bridge1");
            
            var output = await device.ProcessCommandAsync("/interface bridge host print");
            Assert.Contains("MAC-ADDRESS", output);
            Assert.Contains("INTERFACE", output);
            Assert.Contains("BRIDGE", output);
        }

        // Test 14: IP Address Print (Brief)
        [Fact]
        public async Task MikroTik_IpAddressPrintBrief_ShouldShowSummary()
        {
            var device = new MikroTikDevice("R1");
            await device.ProcessCommandAsync("/ip address add address=10.0.0.1/24 interface=ether1");
            await device.ProcessCommandAsync("/ip address add address=192.168.1.1/24 interface=ether2");
            await device.ProcessCommandAsync("/interface ethernet disable ether2");
            
            var output = await device.ProcessCommandAsync("/ip address print");
            Assert.Contains("ADDRESS", output);
            Assert.Contains("NETWORK", output);
            Assert.Contains("INTERFACE", output);
            Assert.Contains("10.0.0.1/24", output);
            Assert.Contains("192.168.1.1/24", output);
        }

        // Test 15: File Print (Save Configuration)
        [Fact]
        public async Task MikroTik_FilePrint_ShouldShowSavedFiles()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/system identity set name=\"SaveTest\"");
            
            var output = await device.ProcessCommandAsync("/file print");
            Assert.Contains("NAME", output);
            Assert.Contains("TYPE", output);
            Assert.Contains("SIZE", output);
            Assert.Contains("CREATION-TIME", output);
        }

        // Test 16: Interface Bridge Settings (STP)
        [Fact]
        public async Task MikroTik_BridgeSettings_ShouldShowSpanningTree()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/interface bridge add name=bridge1 priority=0x1000 protocol-mode=rstp");
            
            var output = await device.ProcessCommandAsync("/interface bridge print");
            Assert.Contains("bridge1", output);
            Assert.Contains("rstp", output);
            
            output = await device.ProcessCommandAsync("/interface bridge port print");
            Assert.Contains("INTERFACE", output);
            Assert.Contains("BRIDGE", output);
        }

        // Test 17: IP Firewall Filter (Access Lists)
        [Fact]
        public async Task MikroTik_FirewallFilter_ShouldShowAcls()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("/ip firewall filter add chain=input action=drop src-address=192.168.1.0/24");
            await r1.ProcessCommandAsync("/ip firewall filter add chain=input action=accept");
            
            var output = await r1.ProcessCommandAsync("/ip firewall filter print");
            Assert.Contains("chain=input", output);
            Assert.Contains("action=drop", output);
            Assert.Contains("src-address=192.168.1.0/24", output);
            Assert.Contains("action=accept", output);
        }

        // Test 18: Interface Bonding (LAG)
        [Fact]
        public async Task MikroTik_InterfaceBonding_ShouldShowLag()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/interface bonding add name=bond1 mode=802.3ad slaves=ether1,ether2");
            
            var output = await device.ProcessCommandAsync("/interface bonding print");
            Assert.Contains("bond1", output);
            Assert.Contains("802.3ad", output);
            Assert.Contains("ether1,ether2", output);
        }

        // Test 19: System Reboot
        [Fact]
        public async Task MikroTik_SystemReboot_ShouldPromptConfirmation()
        {
            var device = new MikroTikDevice("MT1");
            var output = await device.ProcessCommandAsync("/system reboot");
            
            Assert.Contains("Reboot, yes?", output);
            Assert.Contains("[y/N]", output);
        }

        // Test 20: Log Print
        [Fact]
        public async Task MikroTik_LogPrint_ShouldShowLogEntries()
        {
            var device = new MikroTikDevice("MT1");
            await device.ProcessCommandAsync("/interface ethernet disable ether1");
            await device.ProcessCommandAsync("/interface ethernet enable ether1");
            
            var output = await device.ProcessCommandAsync("/log print");
            Assert.Contains("system,info", output);
            Assert.Contains("interface,info", output);
            Assert.Contains("ether1", output);
        }
    }
} 
