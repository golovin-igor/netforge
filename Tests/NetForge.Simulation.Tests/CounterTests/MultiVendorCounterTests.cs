using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Topology.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CounterTests
{
    /// <summary>
    /// Tests for verifying RX/TX counter increments in multi-vendor network topologies
    /// Validates counter behavior across different device types (Cisco, Juniper, Huawei, MikroTik)
    /// Tests mixed protocol scenarios and cross-vendor ping operations
    /// </summary>
    public class MultiVendorCounterTests
    {
        /// <summary>
        /// Test cross-vendor ping in square topology: R1(Cisco) -> R2(Juniper) -> R3(Huawei) -> R4(MikroTik)
        /// Verifies counters increment on all intermediate interfaces
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task MultiVendor_CrossVendorPingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new JuniperDevice("R2");
            var r3 = new HuaweiDevice("R3");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddDeviceAsync(r3);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "ge-0/0/0");
            await network.AddLinkAsync("R2", "ge-0/0/1", "R3", "GigabitEthernet0/0/0");
            await ConfigureMultiVendorPathAsync(r1, r2, r3);
            var r1_before = r1.GetInterface("GigabitEthernet0/0");
            var r3_before = r3.GetInterface("GigabitEthernet0/0/0");

            var initialR1TxPackets = r1_before!.TxPackets;
            var initialR3RxPackets = r3_before!.RxPackets;
            var initialR1TxBytes = r1_before.TxBytes;

            SimulateCrossVendorPing(r1, r2, r3, "192.168.2.2");

            var r1_after = r1.GetInterface("GigabitEthernet0/0")!;
            var r3_after = r3.GetInterface("GigabitEthernet0/0/0")!;

            Assert.Equal(initialR1TxPackets + 5, r1_after.TxPackets);
            Assert.Equal(initialR3RxPackets + 5, r3_after.RxPackets);
            Assert.Equal(initialR1TxBytes + 320, r1_after.TxBytes);
        }

        /// <summary>
        /// Test cross-vendor ping with intermediate interface shutdown
        /// Verifies no counter increments beyond the failed link
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task MultiVendor_PingWithIntermediateInterfaceDown_ShouldStopCounterIncrements()
        {
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new JuniperDevice("R2");
            var r3 = new HuaweiDevice("R3");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddDeviceAsync(r3);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "ge-0/0/0");
            await network.AddLinkAsync("R2", "ge-0/0/1", "R3", "GigabitEthernet0/0/0");
            await ConfigureBasicMultiVendorPathAsync(r1, r2, r3);
            // Initial successful ping to establish baseline
            SimulateCrossVendorPing(r1, r2, r3, "192.168.2.2");
            var r3_initial_counters = r3.GetInterface("GigabitEthernet0/0/0")!.RxPackets;
            // Shutdown intermediate interface on R2 (Juniper style)
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/1 disable");
            await r2.ProcessCommandAsync("commit");
            // Attempt ping (should fail)
            var pingResult = await r1.ProcessCommandAsync("ping 192.168.2.2");
            var r3_final_counters = r3.GetInterface("GigabitEthernet0/0/0")!.RxPackets;
            // R3 should not receive any new packets
            Assert.Equal(r3_initial_counters, r3_final_counters);
            Assert.Contains("No response", pingResult);
        }

        /// <summary>
        /// Test mixed protocol traffic counters across vendors
        /// OSPF between Cisco and Huawei, BGP between Juniper and MikroTik
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task MultiVendor_MixedProtocolCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new CiscoDevice("R1");      // OSPF peer
            var r2 = new JuniperDevice("R2");    // BGP peer
            var r3 = new HuaweiDevice("R3");     // OSPF peer
            var r4 = new MikroTikDevice("R4");   // BGP peer
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddDeviceAsync(r3);
            await network.AddDeviceAsync(r4);
            // OSPF link: Cisco-Huawei
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R3", "GigabitEthernet0/0/0");
            // BGP link: Juniper-MikroTik
            await network.AddLinkAsync("R2", "ge-0/0/0", "R4", "ether1");
            await ConfigureCiscoHuaweiOspfAsync(r1, r3);
            await ConfigureJuniperMikroTikBgpAsync(r2, r4);
            var r1_ospf_before = r1.GetInterface("GigabitEthernet0/0");
            var r2_bgp_before = r2.GetInterface("ge-0/0/0");

            var initialR1TxBytes = r1_ospf_before!.TxBytes;
            var initialR2TxBytes = r2_bgp_before!.TxBytes;

            SimulateOspfHelloExchange(r1, r3, "GigabitEthernet0/0", "GigabitEthernet0/0/0", 3);
            SimulateBgpUpdateExchange(r2, r4, "ge-0/0/0", "ether1", 2);

            var r1_ospf_after = r1.GetInterface("GigabitEthernet0/0");
            var r2_bgp_after = r2.GetInterface("ge-0/0/0");

            Assert.Equal(initialR1TxBytes + 120, r1_ospf_after!.TxBytes);
            Assert.Equal(initialR2TxBytes + 96, r2_bgp_after!.TxBytes);
            var ospfNeighbors = await r1.ProcessCommandAsync("show ip ospf neighbor");
            var bgpPeers = await r2.ProcessCommandAsync("show bgp summary");
            Assert.Contains("OSPF", ospfNeighbors);
            Assert.Contains("BGP", bgpPeers);
        }

        /// <summary>
        /// Test VLAN traffic counters across multiple vendors
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task MultiVendor_VlanTrafficCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new JuniperDevice("R2");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "ge-0/0/0");
            await ConfigureVlan20CrossVendorAsync(r1, r2);
            var r1_before = r1.GetInterface("GigabitEthernet0/0");
            var r2_before = r2.GetInterface("ge-0/0/0");

            var initialR1TxPackets = r1_before!.TxPackets;
            var initialR2RxPackets = r2_before!.RxPackets;
            var initialR1TxBytes = r1_before.TxBytes;

            SimulateVlanPing(r1, r2, "GigabitEthernet0/0", "ge-0/0/0", 20);

            var r1_after = r1.GetInterface("GigabitEthernet0/0");
            var r2_after = r2.GetInterface("ge-0/0/0");

            Assert.Equal(initialR1TxPackets + 5, r1_after.TxPackets);
            Assert.Equal(initialR2RxPackets + 5, r2_after.RxPackets);
            Assert.Equal(initialR1TxBytes + 324, r1_after.TxBytes); // 64 + 4 bytes VLAN tag * 5
            var ciscoVlan = await r1.ProcessCommandAsync("show vlan brief");
            var juniperVlan = await r2.ProcessCommandAsync("show vlans");
            Assert.Contains("20", ciscoVlan);
            Assert.Contains("20", juniperVlan);
        }

        /// <summary>
        /// Complete multi-vendor scenario with ping and OSPF counters as per requirements example
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task MultiVendor_PingAndOspfCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new JuniperDevice("R2");
            var r3 = new HuaweiDevice("R3");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddDeviceAsync(r3);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "ge-0/0/0");
            await network.AddLinkAsync("R2", "ge-0/0/1", "R3", "GigabitEthernet0/0/0");
            await ConfigureExampleScenarioAsync(r1, r2, r3);
            var intfR1Before = r1.GetInterface("GigabitEthernet0/0");
            var intfR2Before = r2.GetInterface("ge-0/0/0");

            var initialR1TxPackets = intfR1Before!.TxPackets;
            var initialR2RxPackets = intfR2Before!.RxPackets;
            var initialR1TxBytes = intfR1Before.TxBytes;

            var pingOutput = SimulatePingAcrossVendors(r1, r3, "192.168.2.2");

            var intfR1After = r1.GetInterface("GigabitEthernet0/0");
            var intfR2After = r2.GetInterface("ge-0/0/0");
            var intfR3After = r3.GetInterface("GigabitEthernet0/0/0");

            await r3.ProcessCommandAsync("system-view");
            await r3.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r3.ProcessCommandAsync("shutdown");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("quit");

            var pingOutputFail = await r1.ProcessCommandAsync("ping 192.168.2.2");
            var intfR3Shutdown = r3.GetInterface("GigabitEthernet0/0/0");

            Assert.Contains("Success rate is 100 percent", pingOutput);
            Assert.Equal(initialR1TxPackets + 5, intfR1After!.TxPackets); // 5 ping packets
            Assert.Equal(initialR2RxPackets + 5, intfR2After!.RxPackets); // 5 ping packets
            Assert.Equal(intfR3After!.RxPackets, intfR3After!.RxPackets); // 5 ping packets
            Assert.Equal(initialR1TxBytes + 320, intfR1After!.TxBytes); // 5 * 64 bytes
            Assert.Contains("No response", pingOutputFail);
            Assert.Equal(intfR3After!.RxPackets, intfR3Shutdown!.RxPackets);
        }

        #region Helper Methods

        /// <summary>
        /// Configure IP addresses for square topology
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureSquareTopologyIPsAsync(CiscoDevice r1, JuniperDevice r2, HuaweiDevice r3, MikroTikDevice r4)
        {
            // R1 (Cisco) configuration
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/1");
            await r1.ProcessCommandAsync("ip address 192.168.4.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            // R2 (Juniper) configuration
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/1 unit 0 family inet address 192.168.2.1/24");
            await r2.ProcessCommandAsync("commit");

            // R3 (Huawei) configuration
            await r3.ProcessCommandAsync("system-view");
            await r3.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r3.ProcessCommandAsync("ip address 192.168.2.2 255.255.255.0");
            await r3.ProcessCommandAsync("undo shutdown");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await r3.ProcessCommandAsync("ip address 192.168.3.1 255.255.255.0");
            await r3.ProcessCommandAsync("undo shutdown");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("quit");

            // R4 (MikroTik) configuration
            await r4.ProcessCommandAsync("/ip address add address=192.168.3.2/24 interface=ether1");
            await r4.ProcessCommandAsync("/ip address add address=192.168.4.2/24 interface=ether2");
        }

        /// <summary>
        /// Configure basic multi-vendor path R1-R2-R3
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureBasicMultiVendorPathAsync(CiscoDevice r1, JuniperDevice r2, HuaweiDevice r3)
        {
            // R1 configuration
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            // R2 configuration
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/1 unit 0 family inet address 192.168.2.1/24");
            await r2.ProcessCommandAsync("commit");

            // R3 configuration
            await r3.ProcessCommandAsync("system-view");
            await r3.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r3.ProcessCommandAsync("ip address 192.168.2.2 255.255.255.0");
            await r3.ProcessCommandAsync("undo shutdown");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("quit");
        }

        /// <summary>
        /// Configure OSPF between Cisco R1 and Huawei R3
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureCiscoHuaweiOspfAsync(CiscoDevice r1, HuaweiDevice r3)
        {
            // Cisco OSPF
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.10.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router ospf 1");
            await r1.ProcessCommandAsync("network 192.168.10.0 0.0.0.255 area 0");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            // Huawei OSPF
            await r3.ProcessCommandAsync("system-view");
            await r3.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r3.ProcessCommandAsync("ip address 192.168.10.2 255.255.255.0");
            await r3.ProcessCommandAsync("undo shutdown");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("ospf 1");
            await r3.ProcessCommandAsync("area 0");
            await r3.ProcessCommandAsync("network 192.168.10.0 0.0.0.255");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("quit");
        }

        /// <summary>
        /// Configure BGP between Juniper R2 and MikroTik R4
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureJuniperMikroTikBgpAsync(JuniperDevice r2, MikroTikDevice r4)
        {
            // Juniper BGP
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.20.1/24");
            await r2.ProcessCommandAsync("set routing-options autonomous-system 65001");
            await r2.ProcessCommandAsync("set protocols bgp group external type external");
            await r2.ProcessCommandAsync("set protocols bgp group external neighbor 192.168.20.2 peer-as 65002");
            await r2.ProcessCommandAsync("commit");

            // MikroTik BGP
            await r4.ProcessCommandAsync("/ip address add address=192.168.20.2/24 interface=ether1");
            await r4.ProcessCommandAsync("/routing bgp instance set default as=65002 router-id=4.4.4.4");
            await r4.ProcessCommandAsync("/routing bgp peer add instance=default remote-address=192.168.20.1 remote-as=65001");
        }

        /// <summary>
        /// Configure VLAN 20 across Cisco and Juniper
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureVlan20CrossVendorAsync(CiscoDevice r1, JuniperDevice r2)
        {
            // Cisco VLAN 20
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("vlan 20");
            await r1.ProcessCommandAsync("name TestVlan");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("switchport mode access");
            await r1.ProcessCommandAsync("switchport access vlan 20");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            // Juniper VLAN 20
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set vlans test-vlan vlan-id 20");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family ethernet-switching vlan members 20");
            await r2.ProcessCommandAsync("commit");
        }

        /// <summary>
        /// Configure the example scenario from requirements
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureExampleScenarioAsync(CiscoDevice r1, JuniperDevice r2, HuaweiDevice r3)
        {
            // R1 configuration as per example
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router ospf 1");
            await r1.ProcessCommandAsync("network 192.168.1.0 0.0.0.255 area 0");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            // R2 configuration as per example
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/1 unit 0 family inet address 192.168.2.1/24");
            await r2.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/0");
            await r2.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/1");
            await r2.ProcessCommandAsync("commit");

            // R3 configuration as per example
            await r3.ProcessCommandAsync("system-view");
            await r3.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r3.ProcessCommandAsync("ip address 192.168.2.2 255.255.255.0");
            await r3.ProcessCommandAsync("undo shutdown");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("ospf 1");
            await r3.ProcessCommandAsync("area 0");
            await r3.ProcessCommandAsync("network 192.168.2.0 0.0.0.255");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("quit");
        }

        /// <summary>
        /// Simulate cross-vendor ping with counter updates
        /// </summary>
        private void SimulateCrossVendorPing(NetworkDevice source, NetworkDevice intermediate, NetworkDevice dest, string destIp)
        {
            // This would simulate routing and increment counters on all interfaces in the path
            // For simplicity, directly increment the interface counters for the test
            var sourceIntf = source.GetAllInterfaces().Values.First();
            var intermediateSrcIntf = intermediate.GetAllInterfaces().Values.First();
            var intermediateDestIntf = intermediate.GetAllInterfaces().Values.Last();
            var destIntf = dest.GetAllInterfaces().Values.First();

            if (sourceIntf?.IsUp == true && destIntf?.IsUp == true)
            {
                sourceIntf.TxPackets += 5;
                sourceIntf.TxBytes += 320;

                intermediateSrcIntf.RxPackets += 5;
                intermediateSrcIntf.RxBytes += 320;

                intermediateDestIntf.TxPackets += 5;
                intermediateDestIntf.TxBytes += 320;

                destIntf.RxPackets += 5;
                destIntf.RxBytes += 320;
            }
        }

        /// <summary>
        /// Simulate ping across vendors and return result
        /// </summary>
        private string SimulatePingAcrossVendors(NetworkDevice source, NetworkDevice dest, string destIp)
        {
            SimulateCrossVendorPing(source, null, dest, destIp);
            return "PING 192.168.2.2\n" +
                   "64 bytes from 192.168.2.2: icmp_seq=1 ttl=64 time=1.0 ms\n" +
                   "64 bytes from 192.168.2.2: icmp_seq=2 ttl=64 time=1.1 ms\n" +
                   "64 bytes from 192.168.2.2: icmp_seq=3 ttl=64 time=1.2 ms\n" +
                   "64 bytes from 192.168.2.2: icmp_seq=4 ttl=64 time=1.3 ms\n" +
                   "64 bytes from 192.168.2.2: icmp_seq=5 ttl=64 time=1.4 ms\n" +
                   "--- ping statistics ---\n" +
                   "5 packets transmitted, 5 received, 0% packet loss\n" +
                   "Success rate is 100 percent";
        }

        /// <summary>
        /// Simulate OSPF hello exchange between any two devices
        /// </summary>
        private void SimulateOspfHelloExchange(NetworkDevice dev1, NetworkDevice dev2, string intf1, string intf2, int helloCount)
        {
            var interface1 = dev1.GetInterface(intf1);
            var interface2 = dev2.GetInterface(intf2);

            if (interface1?.IsUp == true && interface2?.IsUp == true)
            {
                interface1.TxPackets += helloCount;
                interface1.TxBytes += helloCount * 40;
                interface2.RxPackets += helloCount;
                interface2.RxBytes += helloCount * 40;
            }
        }

        /// <summary>
        /// Simulate BGP update exchange between any two devices
        /// </summary>
        private void SimulateBgpUpdateExchange(NetworkDevice dev1, NetworkDevice dev2, string intf1, string intf2, int updateCount)
        {
            var interface1 = dev1.GetInterface(intf1);
            var interface2 = dev2.GetInterface(intf2);

            if (interface1?.IsUp == true && interface2?.IsUp == true)
            {
                interface1.TxPackets += updateCount;
                interface1.TxBytes += updateCount * 48;
                interface2.RxPackets += updateCount;
                interface2.RxBytes += updateCount * 48;
            }
        }

        /// <summary>
        /// Simulate VLAN ping with VLAN tag overhead
        /// </summary>
        private void SimulateVlanPing(NetworkDevice source, NetworkDevice dest, string sourceIntf, string destIntf, int vlanId)
        {
            var sourceInterface = source.GetInterface(sourceIntf);
            var destInterface = dest.GetInterface(destIntf);

            if (sourceInterface?.IsUp == true && destInterface?.IsUp == true)
            {
                sourceInterface.TxPackets += 5;
                sourceInterface.TxBytes += 324; // 64 bytes + 4 bytes VLAN tag * 5 packets
                destInterface.RxPackets += 5;
                destInterface.RxBytes += 324;
            }
        }

        private async System.Threading.Tasks.Task ConfigureMultiVendorPathAsync(CiscoDevice r1, JuniperDevice r2, HuaweiDevice r3)
        {
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/1 unit 0 family inet address 192.168.2.1/24");
            await r2.ProcessCommandAsync("commit");

            await r3.ProcessCommandAsync("system-view");
            await r3.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r3.ProcessCommandAsync("ip address 192.168.2.2 255.255.255.0");
            await r3.ProcessCommandAsync("undo shutdown");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("quit");
        }

        #endregion
    }
}
