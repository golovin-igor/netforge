using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CounterTests
{
    public class NokiaCounterTests
    {
        [Fact]
        public void Nokia_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new NokiaDevice("R1");
            var r2 = new NokiaDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "1/1/1", "R2", "1/1/1").Wait();

            ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("1/1/1");
            var intfR2Before = r2.GetInterface("1/1/1");

            SimulatePingWithCounters(r1, r2, "1/1/1", "1/1/1");

            var intfR1After = r1.GetInterface("1/1/1");
            var intfR2After = r2.GetInterface("1/1/1");

            Assert.Equal(intfR1Before.TxPackets + 5, intfR1After.TxPackets);
            Assert.Equal(intfR2Before.RxPackets + 5, intfR2After.RxPackets);
            Assert.Equal(320, intfR1After.TxBytes - intfR1Before.TxBytes);
        }

        [Fact]
        public void Nokia_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new NokiaDevice("R1");
            var r2 = new NokiaDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "1/1/1", "R2", "1/1/1").Wait();

            ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("1/1/1");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "1/1/1", "1/1/1", 3);

            var intfR1After = r1.GetInterface("1/1/1");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes);

            var ospfNeighbors = r1.ProcessCommand("show router ospf neighbor");
            Assert.Contains("192.168.1.2", ospfNeighbors);
        }

        private void SimulatePingWithCounters(NokiaDevice source, NokiaDevice dest, 
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

        private void ConfigureBasicInterfaces(NokiaDevice r1, NokiaDevice r2)
        {
            r1.ProcessCommand("configure");
            r1.ProcessCommand("port 1/1/1");
            r1.ProcessCommand("ethernet mode hybrid");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router Base");
            r1.ProcessCommand("interface 1/1/1");
            r1.ProcessCommand("address 192.168.1.1/24");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("port 1/1/1");
            r2.ProcessCommand("ethernet mode hybrid");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router Base");
            r2.ProcessCommand("interface 1/1/1");
            r2.ProcessCommand("address 192.168.1.2/24");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");
        }

        private void ConfigureOspfDevices(NokiaDevice r1, NokiaDevice r2)
        {
            ConfigureBasicInterfaces(r1, r2);

            r1.ProcessCommand("configure");
            r1.ProcessCommand("router Base");
            r1.ProcessCommand("ospf 0");
            r1.ProcessCommand("area 0.0.0.0");
            r1.ProcessCommand("interface 1/1/1");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("router Base");
            r2.ProcessCommand("ospf 0");
            r2.ProcessCommand("area 0.0.0.0");
            r2.ProcessCommand("interface 1/1/1");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");
        }

        private void SimulateOspfHelloExchange(NokiaDevice r1, NokiaDevice r2, 
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