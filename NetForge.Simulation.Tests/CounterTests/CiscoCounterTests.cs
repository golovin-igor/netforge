using NetForge.Simulation.Common;
using NetForge.Simulation.Devices;
using Xunit;
using System;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Tests.CounterTests
{
    /// <summary>
    /// Tests for verifying RX/TX counter increments on Cisco devices
    /// Validates packet/byte counters for ping, OSPF, BGP, RIP protocols
    /// Tests interface up/down conditions and ACL filtering effects
    /// </summary>
    public class CiscoCounterTests
    {
        /// <summary>
        /// Test ping counter increments - verifies 5 packets, 320 bytes (64 bytes * 5)
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task Cisco_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            var intfR1Before = r1.GetInterface("GigabitEthernet0/0");
            var intfR2Before = r2.GetInterface("GigabitEthernet0/0");
            var txPacketsBefore = intfR1Before.TxPackets;
            var rxPacketsBefore = intfR2Before.RxPackets;
            var txBytesBefore = intfR1Before.TxBytes;

            SimulatePingWithCounters(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0");

            var intfR1After = r1.GetInterface("GigabitEthernet0/0");
            var intfR2After = r2.GetInterface("GigabitEthernet0/0");

            Assert.Equal(txPacketsBefore + 5, intfR1After.TxPackets);
            Assert.Equal(rxPacketsBefore + 5, intfR2After.RxPackets);
            Assert.Equal(txBytesBefore + 320, intfR1After.TxBytes);
        }

        /// <summary>
        /// Test ping with interface shutdown - no counter increments should occur
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task Cisco_PingWithInterfaceShutdown_ShouldNotIncrementCounters()
        {
            // Arrange
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            // Configure interfaces
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            // Initial successful ping to establish baseline
            SimulatePingWithCounters(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0");
            
            // Get counters after initial ping
            var initialCounters = r2.GetInterface("GigabitEthernet0/0").RxPackets;

            // Shutdown destination interface
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            // Act - Attempt ping to shutdown interface
            var pingResult = await r1.ProcessCommandAsync("ping 192.168.1.2");

            // Assert
            var finalCounters = r2.GetInterface("GigabitEthernet0/0").RxPackets;
            
            // Counters should not increment when interface is down
            Assert.Equal(initialCounters, finalCounters);
            Assert.Contains("No response", pingResult);

            // Re-enable interface and verify counters resume
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            SimulatePingWithCounters(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0");
            var resumedCounters = r2.GetInterface("GigabitEthernet0/0").RxPackets;
            Assert.Equal(initialCounters + 5, resumedCounters);
        }

        /// <summary>
        /// Test OSPF hello packet counters - 40 bytes per hello packet
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task Cisco_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            // Arrange
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            // Configure interfaces and OSPF
            ConfigureOspfDevices(r1, r2);

            // Get initial counters
            var intfR1Before = r1.GetInterface("GigabitEthernet0/0");
            var intfR2Before = r2.GetInterface("GigabitEthernet0/0");
            var initialTxBytes = intfR1Before.TxBytes;
            var initialRxBytes = intfR2Before.RxBytes;

            // Act - Simulate OSPF hello exchange
            SimulateOspfHelloExchange(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0", 3);

            // Assert
            var intfR1After = r1.GetInterface("GigabitEthernet0/0");
            var intfR2After = r2.GetInterface("GigabitEthernet0/0");

            // Verify OSPF hello counters (3 hello packets, 40 bytes each)
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes); // 3 * 40 bytes
            Assert.Equal(initialRxBytes + 120, intfR2After.RxBytes);

            // Verify OSPF neighbors are established
            var ospfNeighbors = await r1.ProcessCommandAsync("show ip ospf neighbor");
            Assert.Contains("192.168.1.2", ospfNeighbors);
        }

        /// <summary>
        /// Test BGP update message counters - 48 bytes per update
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task Cisco_BgpUpdateCounters_ShouldIncrementCorrectly()
        {
            // Arrange
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            // Configure BGP peers
            await ConfigureBgpPeers(r1, r2, 65001, 65002);

            // Get initial counters
            var intfR1Before = r1.GetInterface("GigabitEthernet0/0");
            var initialTxBytes = intfR1Before.TxBytes;

            // Act - Simulate BGP update exchange
            SimulateBgpUpdateExchange(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0", 2);

            // Assert
            var intfR1After = r1.GetInterface("GigabitEthernet0/0");
            
            // Verify BGP update counters (2 update messages, 48 bytes each)
            Assert.Equal(initialTxBytes + 96, intfR1After.TxBytes); // 2 * 48 bytes

            // Verify BGP peering
            var bgpSummary = await r1.ProcessCommandAsync("show ip bgp summary");
            Assert.Contains("192.168.1.2", bgpSummary);
        }

        /// <summary>
        /// Test RIP advertisement counters - 32 bytes per advertisement
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task Cisco_RipAdvertisementCounters_ShouldIncrementCorrectly()
        {
            // Arrange
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            // Configure RIP
            await ConfigureRipDevices(r1, r2);

            // Get initial counters
            var intfR1Before = r1.GetInterface("GigabitEthernet0/0");
            var initialTxBytes = intfR1Before.TxBytes;

            // Act - Simulate RIP advertisement exchange
            SimulateRipAdvertisementExchange(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0", 2);

            // Assert
            var intfR1After = r1.GetInterface("GigabitEthernet0/0");
            
            // Verify RIP advertisement counters (2 advertisements, 32 bytes each)
            Assert.Equal(initialTxBytes + 64, intfR1After.TxBytes); // 2 * 32 bytes

            // Verify RIP routes
            var ripRoutes = await r1.ProcessCommandAsync("show ip route rip");
            Assert.Contains("R", ripRoutes); // RIP routes marked with 'R'
        }

        /// <summary>
        /// Test ACL blocking - no counter increments when traffic is blocked
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task Cisco_AclBlockingCounters_ShouldNotIncrementWhenBlocked()
        {
            // Arrange
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            // Configure interfaces
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");

            // Apply ACL to block ping
            await r2.ProcessCommandAsync("access-list 101 deny icmp any host 192.168.1.2");
            await r2.ProcessCommandAsync("access-list 101 permit ip any any");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip access-group 101 in");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            // Get initial counters
            var initialRxPackets = r2.GetInterface("GigabitEthernet0/0").RxPackets;

            // Act - Attempt ping (should be blocked)
            var pingResult = await r1.ProcessCommandAsync("ping 192.168.1.2");
            
            // Assert
            var finalRxPackets = r2.GetInterface("GigabitEthernet0/0").RxPackets;
            
            // Counters should not increment when ACL blocks traffic
            Assert.Equal(initialRxPackets, finalRxPackets);
            Assert.Contains("Request timeout", pingResult);

            // Remove ACL and verify counters resume
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("no ip access-group 101 in");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");

            SimulatePingWithCounters(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0");
            var resumedRxPackets = r2.GetInterface("GigabitEthernet0/0").RxPackets;
            Assert.Equal(initialRxPackets + 5, resumedRxPackets);
        }

        /// <summary>
        /// Test multi-protocol counters - OSPF and BGP running simultaneously
        /// </summary>
        [Fact]
        public async System.Threading.Tasks.Task Cisco_MultiProtocolCounters_ShouldAccumulateCorrectly()
        {
            // Arrange
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            // Configure both OSPF and BGP
            ConfigureOspfDevices(r1, r2);
            await ConfigureBgpPeers(r1, r2, 65001, 65002);

            // Get initial counters
            var intfR1Before = r1.GetInterface("GigabitEthernet0/0");
            var initialTxBytes = intfR1Before.TxBytes;

            // Act - Simulate both OSPF and BGP traffic
            SimulateOspfHelloExchange(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0", 2);
            SimulateBgpUpdateExchange(r1, r2, "GigabitEthernet0/0", "GigabitEthernet0/0", 1);

            // Assert
            var intfR1After = r1.GetInterface("GigabitEthernet0/0");
            
            // Verify cumulative counters (2 OSPF hellos @ 40 bytes + 1 BGP update @ 48 bytes)
            Assert.Equal(initialTxBytes + 128, intfR1After.TxBytes); // 80 + 48 bytes

            // Verify both protocols are active
            var ospfNeighbors = await r1.ProcessCommandAsync("show ip ospf neighbor");
            var bgpSummary = await r1.ProcessCommandAsync("show ip bgp summary");
            Assert.Contains("192.168.1.2", ospfNeighbors);
            Assert.Contains("192.168.1.2", bgpSummary);
        }

        #region Helper Methods

        /// <summary>
        /// Simulate ping with counter updates
        /// </summary>
        private void SimulatePingWithCounters(CiscoDevice source, CiscoDevice dest, 
            string sourceIntf, string destIntf)
        {
            var sourceInterface = source.GetInterface(sourceIntf);
            var destInterface = dest.GetInterface(destIntf);

            if (sourceInterface != null && destInterface != null && 
                sourceInterface.IsUp && destInterface.IsUp)
            {
                sourceInterface.TxPackets += 5;
                sourceInterface.TxBytes += 320;
                destInterface.RxPackets += 5;
                destInterface.RxBytes += 320;
            }
        }

        /// <summary>
        /// Configure OSPF on both devices
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureOspfDevices(CiscoDevice r1, CiscoDevice r2)
        {
            // R1 OSPF configuration
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router ospf 1");
            await r1.ProcessCommandAsync("network 192.168.1.0 0.0.0.255 area 0");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            // R2 OSPF configuration
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router ospf 1");
            await r2.ProcessCommandAsync("network 192.168.1.0 0.0.0.255 area 0");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");
        }

        /// <summary>
        /// Configure BGP peers
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureBgpPeers(CiscoDevice r1, CiscoDevice r2, int as1, int as2)
        {
            // R1 BGP configuration
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync($"router bgp {as1}");
            await r1.ProcessCommandAsync($"neighbor 192.168.1.2 remote-as {as2}");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            // R2 BGP configuration
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync($"router bgp {as2}");
            await r2.ProcessCommandAsync($"neighbor 192.168.1.1 remote-as {as1}");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");
        }

        /// <summary>
        /// Configure RIP on both devices
        /// </summary>
        private async System.Threading.Tasks.Task ConfigureRipDevices(CiscoDevice r1, CiscoDevice r2)
        {
            // R1 RIP configuration
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router rip");
            await r1.ProcessCommandAsync("network 192.168.1.0");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            // R2 RIP configuration
            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router rip");
            await r2.ProcessCommandAsync("network 192.168.1.0");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");
        }

        /// <summary>
        /// Simulate OSPF hello packet exchange
        /// </summary>
        private void SimulateOspfHelloExchange(CiscoDevice r1, CiscoDevice r2, 
            string r1Intf, string r2Intf, int helloCount)
        {
            var r1Interface = r1.GetInterface(r1Intf);
            var r2Interface = r2.GetInterface(r2Intf);

            if (r1Interface != null && r2Interface != null && 
                r1Interface.IsUp && r2Interface.IsUp)
            {
                // Each hello is 40 bytes
                r1Interface.TxPackets += helloCount;
                r1Interface.TxBytes += helloCount * 40;
                r2Interface.RxPackets += helloCount;
                r2Interface.RxBytes += helloCount * 40;
            }
        }

        /// <summary>
        /// Simulate BGP update message exchange
        /// </summary>
        private void SimulateBgpUpdateExchange(CiscoDevice r1, CiscoDevice r2, 
            string r1Intf, string r2Intf, int updateCount)
        {
            var r1Interface = r1.GetInterface(r1Intf);
            var r2Interface = r2.GetInterface(r2Intf);

            if (r1Interface != null && r2Interface != null && 
                r1Interface.IsUp && r2Interface.IsUp)
            {
                // Each BGP update is 48 bytes
                r1Interface.TxPackets += updateCount;
                r1Interface.TxBytes += updateCount * 48;
                r2Interface.RxPackets += updateCount;
                r2Interface.RxBytes += updateCount * 48;
            }
        }

        /// <summary>
        /// Simulate RIP advertisement exchange
        /// </summary>
        private void SimulateRipAdvertisementExchange(CiscoDevice r1, CiscoDevice r2, 
            string r1Intf, string r2Intf, int advCount)
        {
            var r1Interface = r1.GetInterface(r1Intf);
            var r2Interface = r2.GetInterface(r2Intf);

            if (r1Interface != null && r2Interface != null && 
                r1Interface.IsUp && r2Interface.IsUp)
            {
                // Each RIP advertisement is 32 bytes
                r1Interface.TxPackets += advCount;
                r1Interface.TxBytes += advCount * 32;
                r2Interface.RxPackets += advCount;
                r2Interface.RxBytes += advCount * 32;
            }
        }

        #endregion
    }
} 
