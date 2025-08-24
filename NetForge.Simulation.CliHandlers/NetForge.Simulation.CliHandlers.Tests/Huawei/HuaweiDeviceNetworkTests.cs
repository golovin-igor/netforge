using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Huawei
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
        public async Task HuaweiPingShouldShowSuccessAndFailure()
        {
            var device = new HuaweiDevice("R1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("undo shutdown");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("ping 10.0.0.2");
            Assert.Contains("56 data bytes", output);
            Assert.Contains("10.0.0.2", output);
            Assert.Contains("Reply from", output);
            Assert.Contains("100.00% packet loss", output, StringComparison.OrdinalIgnoreCase);

            output = await device.ProcessCommandAsync("ping 192.168.99.99");
            Assert.Contains("0.00% packet loss", output, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task HuaweiConfigureInterfaceAndPingShouldSucceed()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("system-view");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 24");
            await r1.ProcessCommandAsync("quit");
            await r1.ProcessCommandAsync("quit");

            await r2.ProcessCommandAsync("system-view");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r2.ProcessCommandAsync("ip address 192.168.1.2 24");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("quit");

            var pingOutput = await r1.ProcessCommandAsync("ping 192.168.1.2");
            Assert.Contains("5 packet(s) transmitted, 5 packet(s) received", pingOutput);
        }

        [Fact]
        public async Task HuaweiConfigureOspfShouldFormAdjacency()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("system-view");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r1.ProcessCommandAsync("ip address 10.0.0.1 30");
            await r1.ProcessCommandAsync("quit");
            await r1.ProcessCommandAsync("ospf 1 router-id 1.1.1.1");
            await r1.ProcessCommandAsync("area 0");
            await r1.ProcessCommandAsync("network 10.0.0.0 0.0.0.3");
            await r1.ProcessCommandAsync("quit");
            await r1.ProcessCommandAsync("quit");

            await r2.ProcessCommandAsync("system-view");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r2.ProcessCommandAsync("ip address 10.0.0.2 30");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("ospf 1 router-id 2.2.2.2");
            await r2.ProcessCommandAsync("area 0");
            await r2.ProcessCommandAsync("network 10.0.0.0 0.0.0.3");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("quit");

            network.UpdateProtocols();
            await Task.Delay(50);

            var ospfOutput = await r1.ProcessCommandAsync("display ospf peer");
            Assert.Contains("2.2.2.2", ospfOutput);
            Assert.Contains("Full", ospfOutput);
        }

        [Fact]
        public async Task HuaweiConfigureBgpShouldEstablishPeering()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("system-view");
            await r1.ProcessCommandAsync("bgp 65001");
            await r1.ProcessCommandAsync("router-id 1.1.1.1");
            await r1.ProcessCommandAsync("peer 10.0.0.2 as-number 65002");
            await r1.ProcessCommandAsync("quit");
            await r1.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r1.ProcessCommandAsync("ip address 10.0.0.1 30");
            await r1.ProcessCommandAsync("quit");
            await r1.ProcessCommandAsync("quit");

            await r2.ProcessCommandAsync("system-view");
            await r2.ProcessCommandAsync("bgp 65002");
            await r2.ProcessCommandAsync("router-id 2.2.2.2");
            await r2.ProcessCommandAsync("peer 10.0.0.1 as-number 65001");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("interface GigabitEthernet0/0/0");
            await r2.ProcessCommandAsync("ip address 10.0.0.2 30");
            await r2.ProcessCommandAsync("quit");
            await r2.ProcessCommandAsync("quit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var bgpOutput = await r1.ProcessCommandAsync("display bgp peer");
            Assert.Contains("10.0.0.2", bgpOutput);
            Assert.Contains("Established", bgpOutput);
        }
    }
}

