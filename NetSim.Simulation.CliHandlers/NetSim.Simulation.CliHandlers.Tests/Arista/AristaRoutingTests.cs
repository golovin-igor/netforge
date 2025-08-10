using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Arista
{
    public class AristaRoutingTests
    {
        [Fact]
        public async Task AristaConfigureOspfShouldFormNeighborship()
        {
            var network = new Network();
            var r1 = new AristaDevice("R1");
            var r2 = new AristaDevice("R2");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "Ethernet1", "R2", "Ethernet1");

            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("interface Ethernet1");
            await r1.ProcessCommandAsync("no switchport");
            await r1.ProcessCommandAsync("ip address 10.0.0.1/30");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router ospf 1");
            await r1.ProcessCommandAsync("router-id 1.1.1.1");
            await r1.ProcessCommandAsync("network 10.0.0.0 0.0.0.3 area 0");
            await r1.ProcessCommandAsync("maximum-paths 4");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("interface Ethernet1");
            await r2.ProcessCommandAsync("no switchport");
            await r2.ProcessCommandAsync("ip address 10.0.0.2/30");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router ospf 1");
            await r2.ProcessCommandAsync("router-id 2.2.2.2");
            await r2.ProcessCommandAsync("network 10.0.0.0 0.0.0.3 area 0");
            await r2.ProcessCommandAsync("exit");

            var ospfOutput = await r1.ProcessCommandAsync("show ospf neighbor");
            Assert.Contains("2.2.2.2", ospfOutput);
            Assert.Contains("FULL", ospfOutput);
            Assert.Contains("10.0.0.2", ospfOutput);
        }

        [Fact]
        public async Task AristaConfigureBgpShouldEstablishPeering()
        {
            var network = new Network();
            var r1 = new AristaDevice("R1");
            var r2 = new AristaDevice("R2");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "Ethernet1", "R2", "Ethernet1");

            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("interface Ethernet1");
            await r1.ProcessCommandAsync("no switchport");
            await r1.ProcessCommandAsync("ip address 172.16.0.1/30");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router bgp 65001");
            await r1.ProcessCommandAsync("router-id 1.1.1.1");
            await r1.ProcessCommandAsync("neighbor 172.16.0.2 remote-as 65002");
            await r1.ProcessCommandAsync("neighbor 172.16.0.2 description Peer-R2");
            await r1.ProcessCommandAsync("network 192.168.1.0/24");
            await r1.ProcessCommandAsync("maximum-paths 4");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("interface Ethernet1");
            await r2.ProcessCommandAsync("no switchport");
            await r2.ProcessCommandAsync("ip address 172.16.0.2/30");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router bgp 65002");
            await r2.ProcessCommandAsync("router-id 2.2.2.2");
            await r2.ProcessCommandAsync("neighbor 172.16.0.1 remote-as 65001");
            await r2.ProcessCommandAsync("exit");

            var bgpOutput = await r1.ProcessCommandAsync("show bgp summary");
            Assert.Contains("172.16.0.2", bgpOutput);
            Assert.Contains("65002", bgpOutput);
            Assert.Contains("Estab", bgpOutput);
        }

        [Fact]
        public async Task AristaConfigureRipShouldExchangeRoutes()
        {
            var network = new Network();
            var r1 = new AristaDevice("R1");
            var r2 = new AristaDevice("R2");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "Ethernet1", "R2", "Ethernet1");

            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("interface Ethernet1");
            await r1.ProcessCommandAsync("no switchport");
            await r1.ProcessCommandAsync("ip address 10.1.1.1/24");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("interface Ethernet2");
            await r1.ProcessCommandAsync("no switchport");
            await r1.ProcessCommandAsync("ip address 192.168.1.1/24");
            await r1.ProcessCommandAsync("no shutdown");
            await r1.ProcessCommandAsync("exit");
            await r1.ProcessCommandAsync("router rip");
            await r1.ProcessCommandAsync("version 2");
            await r1.ProcessCommandAsync("network 10.1.1.0");
            await r1.ProcessCommandAsync("network 192.168.1.0");
            await r1.ProcessCommandAsync("no auto-summary");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("enable");
            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("interface Ethernet1");
            await r2.ProcessCommandAsync("no switchport");
            await r2.ProcessCommandAsync("ip address 10.1.1.2/24");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("interface Ethernet2");
            await r2.ProcessCommandAsync("no switchport");
            await r2.ProcessCommandAsync("ip address 192.168.2.1/24");
            await r2.ProcessCommandAsync("no shutdown");
            await r2.ProcessCommandAsync("exit");
            await r2.ProcessCommandAsync("router rip");
            await r2.ProcessCommandAsync("version 2");
            await r2.ProcessCommandAsync("network 10.1.1.0");
            await r2.ProcessCommandAsync("network 192.168.2.0");
            await r2.ProcessCommandAsync("exit");

            var routeOutput = await r2.ProcessCommandAsync("show ip route");
            Assert.Contains("192.168.1.0", routeOutput);
            Assert.Contains("R", routeOutput);
            Assert.Contains("10.1.1.1", routeOutput);
        }

        [Fact]
        public async Task AristaConfigureStaticRouteShouldAddToRoutingTable()
        {
            var r1 = new AristaDevice("R1");
            await r1.ProcessCommandAsync("enable");
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("ip route 10.10.10.0/24 192.168.1.254");
            await r1.ProcessCommandAsync("exit");

            var routeOutput = await r1.ProcessCommandAsync("show ip route");
            Assert.Contains("10.10.10.0/24", routeOutput);
            Assert.Contains("S", routeOutput);
            Assert.Contains("192.168.1.254", routeOutput);
        }
    }
}

