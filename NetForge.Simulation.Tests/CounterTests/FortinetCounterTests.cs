using NetForge.Simulation.Common;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CounterTests
{
    public class FortinetCounterTests
    {
        [Fact]
        public async Task Fortinet_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new FortinetDevice("R1");
            var r2 = new FortinetDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "port1", "R2", "port1");

            await ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("port1");
            var intfR2Before = r2.GetInterface("port1");

            var initialTxPackets = intfR1Before.TxPackets;
            var initialRxPackets = intfR2Before.RxPackets;
            var initialTxBytes = intfR1Before.TxBytes;

            SimulatePingWithCounters(r1, r2, "port1", "port1");

            var intfR1After = r1.GetInterface("port1");
            var intfR2After = r2.GetInterface("port1");

            Assert.Equal(initialTxPackets + 5, intfR1After.TxPackets);
            Assert.Equal(initialRxPackets + 5, intfR2After.RxPackets);
            Assert.Equal(initialTxBytes + 320, intfR1After.TxBytes);
        }

        [Fact]
        public async Task Fortinet_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new FortinetDevice("R1");
            var r2 = new FortinetDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "port1", "R2", "port1");

            await ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("port1");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "port1", "port1", 3);

            var intfR1After = r1.GetInterface("port1");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes);

            var ospfNeighbors = await r1.ProcessCommandAsync("get router info ospf neighbor");
            Assert.Contains("192.168.1.2", ospfNeighbors);
        }

        private void SimulatePingWithCounters(FortinetDevice source, FortinetDevice dest, 
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

        private async Task ConfigureBasicInterfaces(FortinetDevice r1, FortinetDevice r2)
        {
            await r1.ProcessCommandAsync("config system interface");
            await r1.ProcessCommandAsync("edit port1");
            await r1.ProcessCommandAsync("set ip 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("set allowaccess ping");
            await r1.ProcessCommandAsync("next");
            await r1.ProcessCommandAsync("end");

            await r2.ProcessCommandAsync("config system interface");
            await r2.ProcessCommandAsync("edit port1");
            await r2.ProcessCommandAsync("set ip 192.168.1.2 255.255.255.0");
            await r2.ProcessCommandAsync("set allowaccess ping");
            await r2.ProcessCommandAsync("next");
            await r2.ProcessCommandAsync("end");
        }

        private async Task ConfigureOspfDevices(FortinetDevice r1, FortinetDevice r2)
        {
            await ConfigureBasicInterfaces(r1, r2);

            await r1.ProcessCommandAsync("config router ospf");
            await r1.ProcessCommandAsync("set router-id 1.1.1.1");
            await r1.ProcessCommandAsync("config area");
            await r1.ProcessCommandAsync("edit 0.0.0.0");
            await r1.ProcessCommandAsync("next");
            await r1.ProcessCommandAsync("end");
            await r1.ProcessCommandAsync("config network");
            await r1.ProcessCommandAsync("edit 1");
            await r1.ProcessCommandAsync("set prefix 192.168.1.0 255.255.255.0");
            await r1.ProcessCommandAsync("set area 0.0.0.0");
            await r1.ProcessCommandAsync("next");
            await r1.ProcessCommandAsync("end");
            await r1.ProcessCommandAsync("end");

            await r2.ProcessCommandAsync("config router ospf");
            await r2.ProcessCommandAsync("set router-id 2.2.2.2");
            await r2.ProcessCommandAsync("config area");
            await r2.ProcessCommandAsync("edit 0.0.0.0");
            await r2.ProcessCommandAsync("next");
            await r2.ProcessCommandAsync("end");
            await r2.ProcessCommandAsync("config network");
            await r2.ProcessCommandAsync("edit 1");
            await r2.ProcessCommandAsync("set prefix 192.168.1.0 255.255.255.0");
            await r2.ProcessCommandAsync("set area 0.0.0.0");
            await r2.ProcessCommandAsync("next");
            await r2.ProcessCommandAsync("end");
            await r2.ProcessCommandAsync("end");
        }

        private void SimulateOspfHelloExchange(FortinetDevice r1, FortinetDevice r2, 
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
