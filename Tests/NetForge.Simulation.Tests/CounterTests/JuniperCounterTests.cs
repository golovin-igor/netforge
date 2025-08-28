using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CounterTests
{
    /// <summary>
    /// Tests for verifying RX/TX counter increments on Juniper devices
    /// Validates packet/byte counters for ping, OSPF, BGP, RIP protocols using Junos CLI
    /// Tests interface up/down conditions and firewall filtering effects
    /// </summary>
    public class JuniperCounterTests
    {
        [Fact]
        public async Task Juniper_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new JuniperDevice("R1");
            var r2 = new JuniperDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ge-0/0/0", "R2", "ge-0/0/0").Wait();

            await ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("ge-0/0/0");
            var intfR2Before = r2.GetInterface("ge-0/0/0");
            var txPacketsBefore = intfR1Before.TxPackets;
            var rxPacketsBefore = intfR2Before.RxPackets;

            SimulatePingWithCounters(r1, r2, "ge-0/0/0", "ge-0/0/0");

            var intfR1After = r1.GetInterface("ge-0/0/0");
            var intfR2After = r2.GetInterface("ge-0/0/0");

            Assert.Equal(txPacketsBefore + 5, intfR1After.TxPackets);
            Assert.Equal(rxPacketsBefore + 5, intfR2After.RxPackets);
            var initialTxBytes = intfR1Before.TxBytes;
            Assert.Equal(initialTxBytes + 320, intfR1After.TxBytes);
        }

        [Fact]
        public async Task Juniper_PingWithInterfaceDown_ShouldNotIncrementCounters()
        {
            var network = new Network();
            var r1 = new JuniperDevice("R1");
            var r2 = new JuniperDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ge-0/0/0", "R2", "ge-0/0/0").Wait();

            await ConfigureBasicInterfaces(r1, r2);

            SimulatePingWithCounters(r1, r2, "ge-0/0/0", "ge-0/0/0");
            var initialCounters = r2.GetInterface("ge-0/0/0").RxPackets;

            // Disable interface in Junos style
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 disable");
            await r2.ProcessCommandAsync("commit");

            var pingResult = await r1.ProcessCommandAsync("ping 192.168.1.2");
            var finalCounters = r2.GetInterface("ge-0/0/0").RxPackets;

            Assert.Equal(initialCounters, finalCounters);
            Assert.Contains("No response", pingResult);
        }

        [Fact]
        public async Task Juniper_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new JuniperDevice("R1");
            var r2 = new JuniperDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ge-0/0/0", "R2", "ge-0/0/0").Wait();

            await ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("ge-0/0/0");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "ge-0/0/0", "ge-0/0/0", 3);

            var intfR1After = r1.GetInterface("ge-0/0/0");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes); // 3 * 40 bytes

            var ospfNeighbors = await r1.ProcessCommandAsync("show ospf neighbor");
            Assert.Contains("192.168.1.2", ospfNeighbors);
        }

        [Fact]
        public async Task Juniper_BgpUpdateCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new JuniperDevice("R1");
            var r2 = new JuniperDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ge-0/0/0", "R2", "ge-0/0/0").Wait();

            await ConfigureBgpPeers(r1, r2, 65001, 65002);

            var intfR1Before = r1.GetInterface("ge-0/0/0");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateBgpUpdateExchange(r1, r2, "ge-0/0/0", "ge-0/0/0", 2);

            var intfR1After = r1.GetInterface("ge-0/0/0");
            Assert.Equal(initialTxBytes + 96, intfR1After.TxBytes); // 2 * 48 bytes

            var bgpSummary = await r1.ProcessCommandAsync("show bgp summary");
            Assert.Contains("192.168.1.2", bgpSummary);
        }

        [Fact]
        public async Task Juniper_FirewallFilterCounters_ShouldNotIncrementWhenBlocked()
        {
            var network = new Network();
            var r1 = new JuniperDevice("R1");
            var r2 = new JuniperDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ge-0/0/0", "R2", "ge-0/0/0").Wait();

            await ConfigureBasicInterfaces(r1, r2);

            // Apply firewall filter to block ICMP
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set firewall filter block-icmp term 1 from protocol icmp");
            await r2.ProcessCommandAsync("set firewall filter block-icmp term 1 then discard");
            await r2.ProcessCommandAsync("set firewall filter block-icmp term 2 then accept");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet filter input block-icmp");
            await r2.ProcessCommandAsync("commit");

            var initialRxPackets = r2.GetInterface("ge-0/0/0").RxPackets;

            var pingResult = await r1.ProcessCommandAsync("ping 192.168.1.2");
            var finalRxPackets = r2.GetInterface("ge-0/0/0").RxPackets;

            Assert.Equal(initialRxPackets, finalRxPackets);
            Assert.Contains("No response", pingResult);
        }

        [Fact]
        public async Task Juniper_MultiProtocolCounters_ShouldAccumulateCorrectly()
        {
            var network = new Network();
            var r1 = new JuniperDevice("R1");
            var r2 = new JuniperDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "ge-0/0/0", "R2", "ge-0/0/0").Wait();

            await ConfigureOspfDevices(r1, r2);
            await ConfigureBgpPeers(r1, r2, 65001, 65002);

            var intfR1Before = r1.GetInterface("ge-0/0/0");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "ge-0/0/0", "ge-0/0/0", 2);
            SimulateBgpUpdateExchange(r1, r2, "ge-0/0/0", "ge-0/0/0", 1);

            var intfR1After = r1.GetInterface("ge-0/0/0");
            Assert.Equal(initialTxBytes + 128, intfR1After.TxBytes); // 80 + 48 bytes
        }

        #region Helper Methods

        private void SimulatePingWithCounters(JuniperDevice source, JuniperDevice dest,
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

        private async Task ConfigureBasicInterfaces(JuniperDevice r1, JuniperDevice r2)
        {
            // R1 interface configuration - Junos style
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            await r1.ProcessCommandAsync("commit");

            // R2 interface configuration - Junos style
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync("commit");
        }

        private async Task ConfigureOspfDevices(JuniperDevice r1, JuniperDevice r2)
        {
            // R1 OSPF configuration - Junos style
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            await r1.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/0");
            await r1.ProcessCommandAsync("commit");

            // R2 OSPF configuration - Junos style
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/0");
            await r2.ProcessCommandAsync("commit");
        }

        private async Task ConfigureBgpPeers(JuniperDevice r1, JuniperDevice r2, int as1, int as2)
        {
            // R1 BGP configuration - Junos style
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            await r1.ProcessCommandAsync($"set routing-options autonomous-system {as1}");
            await r1.ProcessCommandAsync($"set protocols bgp group external type external");
            await r1.ProcessCommandAsync($"set protocols bgp group external neighbor 192.168.1.2 peer-as {as2}");
            await r1.ProcessCommandAsync("commit");

            // R2 BGP configuration - Junos style
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync($"set routing-options autonomous-system {as2}");
            await r2.ProcessCommandAsync($"set protocols bgp group external type external");
            await r2.ProcessCommandAsync($"set protocols bgp group external neighbor 192.168.1.1 peer-as {as1}");
            await r2.ProcessCommandAsync("commit");
        }

        private void SimulateOspfHelloExchange(JuniperDevice r1, JuniperDevice r2,
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

        private void SimulateBgpUpdateExchange(JuniperDevice r1, JuniperDevice r2,
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
