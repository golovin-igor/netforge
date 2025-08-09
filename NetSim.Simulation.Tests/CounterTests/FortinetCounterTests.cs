using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CounterTests
{
    public class FortinetCounterTests
    {
        [Fact]
        public void Fortinet_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new FortinetDevice("R1");
            var r2 = new FortinetDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "port1", "R2", "port1").Wait();

            ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("port1");
            var intfR2Before = r2.GetInterface("port1");

            SimulatePingWithCounters(r1, r2, "port1", "port1");

            var intfR1After = r1.GetInterface("port1");
            var intfR2After = r2.GetInterface("port1");

            Assert.Equal(intfR1Before.TxPackets + 5, intfR1After.TxPackets);
            Assert.Equal(intfR2Before.RxPackets + 5, intfR2After.RxPackets);
            Assert.Equal(320, intfR1After.TxBytes - intfR1Before.TxBytes);
        }

        [Fact]
        public void Fortinet_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new FortinetDevice("R1");
            var r2 = new FortinetDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "port1", "R2", "port1").Wait();

            ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("port1");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "port1", "port1", 3);

            var intfR1After = r1.GetInterface("port1");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes);

            var ospfNeighbors = r1.ProcessCommand("get router info ospf neighbor");
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

        private void ConfigureBasicInterfaces(FortinetDevice r1, FortinetDevice r2)
        {
            r1.ProcessCommand("config system interface");
            r1.ProcessCommand("edit port1");
            r1.ProcessCommand("set ip 192.168.1.1 255.255.255.0");
            r1.ProcessCommand("set allowaccess ping");
            r1.ProcessCommand("next");
            r1.ProcessCommand("end");

            r2.ProcessCommand("config system interface");
            r2.ProcessCommand("edit port1");
            r2.ProcessCommand("set ip 192.168.1.2 255.255.255.0");
            r2.ProcessCommand("set allowaccess ping");
            r2.ProcessCommand("next");
            r2.ProcessCommand("end");
        }

        private void ConfigureOspfDevices(FortinetDevice r1, FortinetDevice r2)
        {
            ConfigureBasicInterfaces(r1, r2);

            r1.ProcessCommand("config router ospf");
            r1.ProcessCommand("set router-id 1.1.1.1");
            r1.ProcessCommand("config area");
            r1.ProcessCommand("edit 0.0.0.0");
            r1.ProcessCommand("next");
            r1.ProcessCommand("end");
            r1.ProcessCommand("config network");
            r1.ProcessCommand("edit 1");
            r1.ProcessCommand("set prefix 192.168.1.0 255.255.255.0");
            r1.ProcessCommand("set area 0.0.0.0");
            r1.ProcessCommand("next");
            r1.ProcessCommand("end");
            r1.ProcessCommand("end");

            r2.ProcessCommand("config router ospf");
            r2.ProcessCommand("set router-id 2.2.2.2");
            r2.ProcessCommand("config area");
            r2.ProcessCommand("edit 0.0.0.0");
            r2.ProcessCommand("next");
            r2.ProcessCommand("end");
            r2.ProcessCommand("config network");
            r2.ProcessCommand("edit 1");
            r2.ProcessCommand("set prefix 192.168.1.0 255.255.255.0");
            r2.ProcessCommand("set area 0.0.0.0");
            r2.ProcessCommand("next");
            r2.ProcessCommand("end");
            r2.ProcessCommand("end");
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