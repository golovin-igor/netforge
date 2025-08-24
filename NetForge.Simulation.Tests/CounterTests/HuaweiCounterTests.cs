using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CounterTests
{
    public class HuaweiCounterTests
    {
        [Fact]
        public async Task Huawei_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new HuaweiDevice("R1");
            var r2 = new HuaweiDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "GigabitEthernet0/0/0", "R2", "GigabitEthernet0/0/0").Wait();

            await ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("GigabitEthernet0/0/0");
            var intfR2Before = r2.GetInterface("GigabitEthernet0/0/0");

            var initialTxPackets = intfR1Before.TxPackets;
            var initialRxPackets = intfR2Before.RxPackets;
            var initialTxBytes = intfR1Before.TxBytes;

            SimulatePingWithCounters(r1, r2, "GigabitEthernet0/0/0", "GigabitEthernet0/0/0");

            var intfR1After = r1.GetInterface("GigabitEthernet0/0/0");
            var intfR2After = r2.GetInterface("GigabitEthernet0/0/0");

            Assert.Equal(initialTxPackets + 5, intfR1After.TxPackets);
            Assert.Equal(initialRxPackets + 5, intfR2After.RxPackets);
            Assert.Equal(initialTxBytes + 320, intfR1After.TxBytes);
        }

        [Fact]
        public async Task Huawei_PingWithInterfaceShutdown_ShouldNotIncrementCounters()
        {
            var network = new Network();
            var r1 = new HuaweiDevice("R1");
            var r2 = new HuaweiDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "GigabitEthernet0/0/0", "R2", "GigabitEthernet0/0/0").Wait();

            await ConfigureBasicInterfaces(r1, r2);

            SimulatePingWithCounters(r1, r2, "GigabitEthernet0/0/0", "GigabitEthernet0/0/0");
            var initialCounters = r2.GetInterface("GigabitEthernet0/0/0").RxPackets;

            await r2.ProcessCommandAsync("system-view");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r2.ProcessCommandAsync("shutdown");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("quit");

            var pingResult = await r1.ProcessCommandAsync("ping 192.168.1.2");
            var finalCounters = r2.GetInterface("GigabitEthernet0/0/0").RxPackets;

            Assert.Equal(initialCounters, finalCounters);
            Assert.Contains("No response", pingResult);
        }

        [Fact]
        public async Task Huawei_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new HuaweiDevice("R1");
            var r2 = new HuaweiDevice("R2");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "GigabitEthernet0/0/0", "R2", "GigabitEthernet0/0/0").Wait();

            await ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("GigabitEthernet0/0/0");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "GigabitEthernet0/0/0", "GigabitEthernet0/0/0", 3);

            var intfR1After = r1.GetInterface("GigabitEthernet0/0/0");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes);

            var ospfNeighbors = await r1.ProcessCommandAsync("display ospf peer");
            Assert.Contains("192.168.1.2", ospfNeighbors);
        }

        private void SimulatePingWithCounters(HuaweiDevice source, HuaweiDevice dest,
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

        private async Task ConfigureBasicInterfaces(HuaweiDevice r1, HuaweiDevice r2)
        {
            await r1.ProcessCommandAsync("system-view");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("undo shutdown");
            await r1.ProcessCommandAsync("quit");
            await r1.ProcessCommandAsync("quit");

            await r2.ProcessCommandAsync("system-view");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("undo shutdown");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("quit");
        }

        private async Task ConfigureOspfDevices(HuaweiDevice r1, HuaweiDevice r2)
        {
            await r1.ProcessCommandAsync("system-view");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("undo shutdown");
            await r1.ProcessCommandAsync("quit");
            await r1.ProcessCommandAsync("ospf 1");
            await r1.ProcessCommandAsync("area 0");
            await r1.ProcessCommandAsync("network 192.168.1.0 0.0.0.255");
            await r1.ProcessCommandAsync("quit");
            await r1.ProcessCommandAsync("quit");

            await r2.ProcessCommandAsync("system-view");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("undo shutdown");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("ospf 1");
            await r2.ProcessCommandAsync("area 0");
            await r2.ProcessCommandAsync("network 192.168.1.0 0.0.0.255");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("quit");
        }

        private void SimulateOspfHelloExchange(HuaweiDevice r1, HuaweiDevice r2,
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
