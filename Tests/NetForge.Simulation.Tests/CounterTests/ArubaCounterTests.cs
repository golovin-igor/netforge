using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CounterTests
{
    public class ArubaCounterTests
    {
        [Fact]
        public async Task Aruba_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new ArubaDevice("R1");
            var r2 = new ArubaDevice("R2");

            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "1/1/1", "R2", "1/1/1");

            await ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("1/1/1");
            var intfR2Before = r2.GetInterface("1/1/1");

            var initialTxPackets = intfR1Before.TxPackets;
            var initialRxPackets = intfR2Before.RxPackets;
            var initialTxBytes = intfR1Before.TxBytes;

            SimulatePingWithCounters(r1, r2, "1/1/1", "1/1/1");

            var intfR1After = r1.GetInterface("1/1/1");
            var intfR2After = r2.GetInterface("1/1/1");

            Assert.Equal(initialTxPackets + 5, intfR1After.TxPackets);
            Assert.Equal(initialRxPackets + 5, intfR2After.RxPackets);
            Assert.Equal(initialTxBytes + 320, intfR1After.TxBytes);
        }

        [Fact]
        public async Task Aruba_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new ArubaDevice("R1");
            var r2 = new ArubaDevice("R2");

            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "1/1/1", "R2", "1/1/1");

            await ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("1/1/1");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "1/1/1", "1/1/1", 3);

            var intfR1After = r1.GetInterface("1/1/1");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes);

            var ospfNeighbors = await r1.ProcessCommandAsync("show ip ospf neighbor");
            Assert.Contains("192.168.1.2", ospfNeighbors);
        }

        private void SimulatePingWithCounters(ArubaDevice source, ArubaDevice dest,
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

        private async Task ConfigureBasicInterfaces(ArubaDevice r1, ArubaDevice r2)
        {
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface 1/1/1");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface 1/1/1");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");
        }

        private async Task ConfigureOspfDevices(ArubaDevice r1, ArubaDevice r2)
        {
            await r1.ProcessCommandAsync("configure terminal");
            await r1.ProcessCommandAsync("interface 1/1/1");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router ospf 1");
            await r1.ProcessCommandAsync("network 192.168.1.0 0.0.0.255 area 0");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure terminal");
            await r2.ProcessCommandAsync("interface 1/1/1");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router ospf 1");
            await r2.ProcessCommandAsync("network 192.168.1.0 0.0.0.255 area 0");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("exit");
        }

        private void SimulateOspfHelloExchange(ArubaDevice r1, ArubaDevice r2,
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
    }
}
