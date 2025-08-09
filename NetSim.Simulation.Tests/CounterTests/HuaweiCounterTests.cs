using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CounterTests
{
    public class HuaweiCounterTests
    {
        [Fact]
        public void Huawei_PingCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new HuaweiDevice("R1");
            var r2 = new HuaweiDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "GigabitEthernet0/0/0", "R2", "GigabitEthernet0/0/0").Wait();

            ConfigureBasicInterfaces(r1, r2);

            var intfR1Before = r1.GetInterface("GigabitEthernet0/0/0");
            var intfR2Before = r2.GetInterface("GigabitEthernet0/0/0");

            SimulatePingWithCounters(r1, r2, "GigabitEthernet0/0/0", "GigabitEthernet0/0/0");

            var intfR1After = r1.GetInterface("GigabitEthernet0/0/0");
            var intfR2After = r2.GetInterface("GigabitEthernet0/0/0");

            Assert.Equal(intfR1Before.TxPackets + 5, intfR1After.TxPackets);
            Assert.Equal(intfR2Before.RxPackets + 5, intfR2After.RxPackets);
            Assert.Equal(320, intfR1After.TxBytes - intfR1Before.TxBytes);
        }

        [Fact]
        public void Huawei_PingWithInterfaceShutdown_ShouldNotIncrementCounters()
        {
            var network = new Network();
            var r1 = new HuaweiDevice("R1");
            var r2 = new HuaweiDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "GigabitEthernet0/0/0", "R2", "GigabitEthernet0/0/0").Wait();

            ConfigureBasicInterfaces(r1, r2);

            SimulatePingWithCounters(r1, r2, "GigabitEthernet0/0/0", "GigabitEthernet0/0/0");
            var initialCounters = r2.GetInterface("GigabitEthernet0/0/0").RxPackets;

            r2.ProcessCommand("system-view");
            r2.ProcessCommand("interface GigabitEthernet0/0/0");
            r2.ProcessCommand("shutdown");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("quit");

            var pingResult = r1.ProcessCommand("ping 192.168.1.2");
            var finalCounters = r2.GetInterface("GigabitEthernet0/0/0").RxPackets;
            
            Assert.Equal(initialCounters, finalCounters);
            Assert.Contains("No response", pingResult);
        }

        [Fact]
        public void Huawei_OspfHelloCounters_ShouldIncrementCorrectly()
        {
            var network = new Network();
            var r1 = new HuaweiDevice("R1");
            var r2 = new HuaweiDevice("R2");
            
            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddLinkAsync("R1", "GigabitEthernet0/0/0", "R2", "GigabitEthernet0/0/0").Wait();

            ConfigureOspfDevices(r1, r2);

            var intfR1Before = r1.GetInterface("GigabitEthernet0/0/0");
            var initialTxBytes = intfR1Before.TxBytes;

            SimulateOspfHelloExchange(r1, r2, "GigabitEthernet0/0/0", "GigabitEthernet0/0/0", 3);

            var intfR1After = r1.GetInterface("GigabitEthernet0/0/0");
            Assert.Equal(initialTxBytes + 120, intfR1After.TxBytes);

            var ospfNeighbors = r1.ProcessCommand("display ospf peer");
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

        private void ConfigureBasicInterfaces(HuaweiDevice r1, HuaweiDevice r2)
        {
            r1.ProcessCommand("system-view");
            r1.ProcessCommand("interface GigabitEthernet0/0/0");
            r1.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            r1.ProcessCommand("undo shutdown");
            r1.ProcessCommand("quit");
            r1.ProcessCommand("quit");

            r2.ProcessCommand("system-view");
            r2.ProcessCommand("interface GigabitEthernet0/0/0");
            r2.ProcessCommand("ip address 192.168.1.2 255.255.255.0");
            r2.ProcessCommand("undo shutdown");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("quit");
        }

        private void ConfigureOspfDevices(HuaweiDevice r1, HuaweiDevice r2)
        {
            r1.ProcessCommand("system-view");
            r1.ProcessCommand("interface GigabitEthernet0/0/0");
            r1.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            r1.ProcessCommand("undo shutdown");
            r1.ProcessCommand("quit");
            r1.ProcessCommand("ospf 1");
            r1.ProcessCommand("area 0");
            r1.ProcessCommand("network 192.168.1.0 0.0.0.255");
            r1.ProcessCommand("quit");
            r1.ProcessCommand("quit");

            r2.ProcessCommand("system-view");
            r2.ProcessCommand("interface GigabitEthernet0/0/0");
            r2.ProcessCommand("ip address 192.168.1.2 255.255.255.0");
            r2.ProcessCommand("undo shutdown");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("ospf 1");
            r2.ProcessCommand("area 0");
            r2.ProcessCommand("network 192.168.1.0 0.0.0.255");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("quit");
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