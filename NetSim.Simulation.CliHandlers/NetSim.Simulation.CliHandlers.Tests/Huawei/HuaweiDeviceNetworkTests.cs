using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Huawei
{
    public class HuaweiDeviceNetworkTests
    {
        private async Task<(Network, HuaweiDevice, HuaweiDevice)> SetupNetworkWithTwoDevicesAsync(string r1Name = "R1", string r2Name = "R2", string iface1 = "GigabitEthernet0/0/0", string iface2 = "GigabitEthernet0/0/0")
        {
            var network = new Network();
            var r1 = new HuaweiDevice(r1Name);
            var r2 = new HuaweiDevice(r2Name);
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync(r1Name, iface1, r2Name, iface2);
            return (network, r1, r2);
        }

        [Fact]
        public void Huawei_Ping_ShouldShowSuccessAndFailure()
        {
            var device = new HuaweiDevice("R1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("undo shutdown");
            device.ProcessCommand("quit");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("ping 10.0.0.2");
            Assert.Contains("56 data bytes", output);
            Assert.Contains("10.0.0.2", output);
            Assert.Contains("Reply from", output);
            Assert.Contains("100.00% packet loss", output, StringComparison.OrdinalIgnoreCase);

            output = device.ProcessCommand("ping 192.168.99.99");
            Assert.Contains("0.00% packet loss", output, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Huawei_ConfigureInterfaceAndPing_ShouldSucceed()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("system-view");
            r1.ProcessCommand("interface GigabitEthernet0/0/0");
            r1.ProcessCommand("ip address 192.168.1.1 24");
            r1.ProcessCommand("quit");
            r1.ProcessCommand("quit");

            r2.ProcessCommand("system-view");
            r2.ProcessCommand("interface GigabitEthernet0/0/0");
            r2.ProcessCommand("ip address 192.168.1.2 24");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("quit");

            var pingOutput = r1.ProcessCommand("ping 192.168.1.2");
            Assert.Contains("5 packet(s) transmitted, 5 packet(s) received", pingOutput);
        }

        [Fact]
        public async Task Huawei_ConfigureOspf_ShouldFormAdjacency()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("system-view");
            r1.ProcessCommand("interface GigabitEthernet0/0/0");
            r1.ProcessCommand("ip address 10.0.0.1 30");
            r1.ProcessCommand("quit");
            r1.ProcessCommand("ospf 1 router-id 1.1.1.1");
            r1.ProcessCommand("area 0");
            r1.ProcessCommand("network 10.0.0.0 0.0.0.3");
            r1.ProcessCommand("quit");
            r1.ProcessCommand("quit");

            r2.ProcessCommand("system-view");
            r2.ProcessCommand("interface GigabitEthernet0/0/0");
            r2.ProcessCommand("ip address 10.0.0.2 30");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("ospf 1 router-id 2.2.2.2");
            r2.ProcessCommand("area 0");
            r2.ProcessCommand("network 10.0.0.0 0.0.0.3");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("quit");

            network.UpdateProtocols();
            await Task.Delay(50);

            var ospfOutput = r1.ProcessCommand("display ospf peer");
            Assert.Contains("2.2.2.2", ospfOutput);
            Assert.Contains("Full", ospfOutput);
        }

        [Fact]
        public async Task Huawei_ConfigureBgp_ShouldEstablishPeering()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("system-view");
            r1.ProcessCommand("bgp 65001");
            r1.ProcessCommand("router-id 1.1.1.1");
            r1.ProcessCommand("peer 10.0.0.2 as-number 65002");
            r1.ProcessCommand("quit");
            r1.ProcessCommand("interface GigabitEthernet0/0/0");
            r1.ProcessCommand("ip address 10.0.0.1 30");
            r1.ProcessCommand("quit");
            r1.ProcessCommand("quit");

            r2.ProcessCommand("system-view");
            r2.ProcessCommand("bgp 65002");
            r2.ProcessCommand("router-id 2.2.2.2");
            r2.ProcessCommand("peer 10.0.0.1 as-number 65001");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("interface GigabitEthernet0/0/0");
            r2.ProcessCommand("ip address 10.0.0.2 30");
            r2.ProcessCommand("quit");
            r2.ProcessCommand("quit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var bgpOutput = r1.ProcessCommand("display bgp peer");
            Assert.Contains("10.0.0.2", bgpOutput);
            Assert.Contains("Established", bgpOutput);
        }
    }
}

