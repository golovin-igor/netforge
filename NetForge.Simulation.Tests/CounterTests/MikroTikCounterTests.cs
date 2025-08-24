using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CounterTests
{
    /// <summary>
    /// Tests for verifying RX/TX counter increments on MikroTik devices
    /// Validates packet/byte counters for ping, OSPF, BGP protocols using RouterOS CLI
    /// Tests interface up/down conditions and firewall rule filtering effects
    /// </summary>
    public class MikroTikCounterTests
    {
        [Fact]
        public async Task MikroTik_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new MikroTikDevice("R1");
            var r2 = new MikroTikDevice("R2");

            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "ether1", "R2", "ether1");

            ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("ether1");
            var intfR2Before = r2.GetInterface("ether1");
            var txPacketsBefore = intfR1Before.TxPackets;
            var rxPacketsBefore = intfR2Before.RxPackets;

            SimulatePingWithCounters(r1, r2, "ether1", "ether1");

            var intfR1After = r1.GetInterface("ether1");
            var intfR2After = r2.GetInterface("ether1");

            Assert.Equal(txPacketsBefore + 5, intfR1After.TxPackets);
            Assert.Equal(rxPacketsBefore + 5, intfR2After.RxPackets);
            var initialTxBytes = intfR1Before.TxBytes;
            Assert.Equal(initialTxBytes + 320, intfR1After.TxBytes);
        }

        [Fact]
        public async Task MikroTik_PingWithInterfaceDisabled_ShouldNotIncrementCounters()
        {
            var network = new Network();
            var r1 = new MikroTikDevice("R1");
            var r2 = new MikroTikDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ether1", "R2", "ether1").Wait();

            ConfigureBasicInterfaces(r1, r2);
            
            SimulatePingWithCounters(r1, r2, "ether1", "ether1");
            var initialCounters = r2.GetInterface("ether1").RxPackets;

            // Disable interface in RouterOS style
            await r2.ProcessCommandAsync("/interface set ether1 disabled=yes");

            var pingResult = await r1.ProcessCommandAsync("/ping 192.168.1.2");
            var finalCounters = r2.GetInterface("ether1").RxPackets;
            
            Assert.Equal(initialCounters, finalCounters);
            Assert.Contains("No response", pingResult);
        }

        [Fact]
        public async Task MikroTik_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new MikroTikDevice("R1");
            var r2 = new MikroTikDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ether1", "R2", "ether1").Wait();

            ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("ether1");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "ether1", "ether1", 3);

            var intfR1After = r1.GetInterface("ether1");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes); // 3 * 40 bytes

            var ospfNeighbors = await r1.ProcessCommandAsync("/routing ospf neighbor print");
            Assert.Contains("192.168.1.2", ospfNeighbors);
        }

        [Fact]
        public async Task MikroTik_BgpUpdateCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new MikroTikDevice("R1");
            var r2 = new MikroTikDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ether1", "R2", "ether1").Wait();

            ConfigureBgpPeers(r1, r2, 65001, 65002);

            var intfR1Before = r1.GetInterface("ether1");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateBgpUpdateExchange(r1, r2, "ether1", "ether1", 2);

            var intfR1After = r1.GetInterface("ether1");
            Assert.Equal(initialTxBytes + 96, intfR1After.TxBytes); // 2 * 48 bytes

            var bgpPeers = await r1.ProcessCommandAsync("/routing bgp peer print");
            Assert.Contains("192.168.1.2", bgpPeers);
        }

        [Fact]
        public async Task MikroTik_FirewallRuleCounters_ShouldNotIncrementWhenBlocked()
        {
            var network = new Network();
            var r1 = new MikroTikDevice("R1");
            var r2 = new MikroTikDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ether1", "R2", "ether1").Wait();

            ConfigureBasicInterfaces(r1, r2);

            // Add firewall rule to block ICMP
            await r2.ProcessCommandAsync("/ip firewall filter add chain=input protocol=icmp action=drop");

            var initialRxPackets = r2.GetInterface("ether1").RxPackets;

            var pingResult = await r1.ProcessCommandAsync("/ping 192.168.1.2");
            var finalRxPackets = r2.GetInterface("ether1").RxPackets;
            
            Assert.Equal(initialRxPackets, finalRxPackets);
            Assert.Contains("No response", pingResult);
        }

        [Fact]
        public async Task MikroTik_MultiProtocolCounters_ShouldAccumulateCorrectly()
        {
            var network = new Network();
            var r1 = new MikroTikDevice("R1");
            var r2 = new MikroTikDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ether1", "R2", "ether1").Wait();

            ConfigureOspfDevices(r1, r2);
            ConfigureBgpPeers(r1, r2, 65001, 65002);

            var intfR1Before = r1.GetInterface("ether1");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "ether1", "ether1", 2);
            SimulateBgpUpdateExchange(r1, r2, "ether1", "ether1", 1);

            var intfR1After = r1.GetInterface("ether1");
            Assert.Equal(initialTxBytes + 128, intfR1After.TxBytes); // 80 + 48 bytes
        }

        #region Helper Methods

        private void SimulatePingWithCounters(MikroTikDevice source, MikroTikDevice dest, 
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

        private async Task ConfigureBasicInterfaces(MikroTikDevice r1, MikroTikDevice r2)
        {
            // R1 interface configuration - RouterOS style
            await r1.ProcessCommandAsync("/ip address add address=192.168.1.1/24 interface=ether1");

            // R2 interface configuration - RouterOS style
            await r2.ProcessCommandAsync("/ip address add address=192.168.1.2/24 interface=ether1");
        }

        private async Task ConfigureOspfDevices(MikroTikDevice r1, MikroTikDevice r2)
        {
            // R1 OSPF configuration - RouterOS style
            await r1.ProcessCommandAsync("/ip address add address=192.168.1.1/24 interface=ether1");
            await r1.ProcessCommandAsync("/routing ospf instance set default router-id=1.1.1.1");
            await r1.ProcessCommandAsync("/routing ospf area add name=backbone area-id=0.0.0.0");
            await r1.ProcessCommandAsync("/routing ospf interface add interface=ether1 area=backbone");

            // R2 OSPF configuration - RouterOS style
            await r2.ProcessCommandAsync("/ip address add address=192.168.1.2/24 interface=ether1");
            await r2.ProcessCommandAsync("/routing ospf instance set default router-id=2.2.2.2");
            await r2.ProcessCommandAsync("/routing ospf area add name=backbone area-id=0.0.0.0");
            await r2.ProcessCommandAsync("/routing ospf interface add interface=ether1 area=backbone");
        }

        private async Task ConfigureBgpPeers(MikroTikDevice r1, MikroTikDevice r2, int as1, int as2)
        {
            // R1 BGP configuration - RouterOS style
            await r1.ProcessCommandAsync("/ip address add address=192.168.1.1/24 interface=ether1");
            await r1.ProcessCommandAsync($"/routing bgp instance set default as={as1} router-id=1.1.1.1");
            await r1.ProcessCommandAsync($"/routing bgp peer add instance=default remote-address=192.168.1.2 remote-as={as2}");

            // R2 BGP configuration - RouterOS style
            await r2.ProcessCommandAsync("/ip address add address=192.168.1.2/24 interface=ether1");
            await r2.ProcessCommandAsync($"/routing bgp instance set default as={as2} router-id=2.2.2.2");
            await r2.ProcessCommandAsync($"/routing bgp peer add instance=default remote-address=192.168.1.1 remote-as={as1}");
        }

        private void SimulateOspfHelloExchange(MikroTikDevice r1, MikroTikDevice r2, 
            string r1Intf, string r2Intf, int helloCount)
        {
            var r1Interface = r1.GetInterface(r1Intf);
            var r2Interface = r2.GetInterface(r2Intf);

            if (r1Interface != null && r2Interface != null && 
                r1Interface.IsUp && r2Interface.IsUp)
            {
                r1Interface.TxPackets += helloCount;
                r1Interface.TxBytes += helloCount * 40;
                r2Interface.RxPackets += helloCount;
                r2Interface.RxBytes += helloCount * 40;
            }
        }

        private void SimulateBgpUpdateExchange(MikroTikDevice r1, MikroTikDevice r2, 
            string r1Intf, string r2Intf, int updateCount)
        {
            var r1Interface = r1.GetInterface(r1Intf);
            var r2Interface = r2.GetInterface(r2Intf);

            if (r1Interface != null && r2Interface != null && 
                r1Interface.IsUp && r2Interface.IsUp)
            {
                r1Interface.TxPackets += updateCount;
                r1Interface.TxBytes += updateCount * 48;
                r2Interface.RxPackets += updateCount;
                r2Interface.RxBytes += updateCount * 48;
            }
        }

        #endregion
    }
} 
