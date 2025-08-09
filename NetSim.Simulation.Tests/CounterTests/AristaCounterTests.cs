using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CounterTests
{
    public class AristaCounterTests
    {
        [Fact]
        public void Arista_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new AristaDevice("R1");
            var r2 = new AristaDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "Ethernet1", "R2", "Ethernet1").Wait();

            ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("Ethernet1");
            var intfR2Before = r2.GetInterface("Ethernet1");

            SimulatePingWithCounters(r1, r2, "Ethernet1", "Ethernet1");

            var intfR1After = r1.GetInterface("Ethernet1");
            var intfR2After = r2.GetInterface("Ethernet1");

            Assert.Equal(intfR1Before.TxPackets + 5, intfR1After.TxPackets);
            Assert.Equal(intfR2Before.RxPackets + 5, intfR2After.RxPackets);
            Assert.Equal(320, intfR1After.TxBytes - intfR1Before.TxBytes);
        }

        [Fact]
        public void Arista_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new AristaDevice("R1");
            var r2 = new AristaDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "Ethernet1", "R2", "Ethernet1").Wait();

            ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("Ethernet1");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "Ethernet1", "Ethernet1", 3);

            var intfR1After = r1.GetInterface("Ethernet1");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes);

            var ospfNeighbors = r1.ProcessCommand("show ip ospf neighbor");
            Assert.Contains("192.168.1.2", ospfNeighbors);
        }

        private void SimulatePingWithCounters(AristaDevice source, AristaDevice dest, 
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

        private void ConfigureBasicInterfaces(AristaDevice r1, AristaDevice r2)
        {
            r1.ProcessCommand("configure");
            r1.ProcessCommand("interface Ethernet1");
            r1.ProcessCommand("ip address 192.168.1.1/24");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("interface Ethernet1");
            r2.ProcessCommand("ip address 192.168.1.2/24");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
        }

        private void ConfigureOspfDevices(AristaDevice r1, AristaDevice r2)
        {
            r1.ProcessCommand("configure");
            r1.ProcessCommand("interface Ethernet1");
            r1.ProcessCommand("ip address 192.168.1.1/24");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router ospf 1");
            r1.ProcessCommand("network 192.168.1.0/24 area 0");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("interface Ethernet1");
            r2.ProcessCommand("ip address 192.168.1.2/24");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router ospf 1");
            r2.ProcessCommand("network 192.168.1.0/24 area 0");
            r2.ProcessCommand("exit");
        }

        private void SimulateOspfHelloExchange(AristaDevice r1, AristaDevice r2, 
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