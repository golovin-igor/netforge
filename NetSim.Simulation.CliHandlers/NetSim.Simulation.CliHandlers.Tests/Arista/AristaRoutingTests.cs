using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Arista
{
    public class AristaRoutingTests
    {
        [Fact]
        public async Task Arista_ConfigureOspf_ShouldFormNeighborship()
        {
            var network = new Network();
            var r1 = new AristaDevice("R1");
            var r2 = new AristaDevice("R2");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "Ethernet1", "R2", "Ethernet1");

            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure");
            r1.ProcessCommand("interface Ethernet1");
            r1.ProcessCommand("no switchport");
            r1.ProcessCommand("ip address 10.0.0.1/30");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router ospf 1");
            r1.ProcessCommand("router-id 1.1.1.1");
            r1.ProcessCommand("network 10.0.0.0 0.0.0.3 area 0");
            r1.ProcessCommand("maximum-paths 4");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure");
            r2.ProcessCommand("interface Ethernet1");
            r2.ProcessCommand("no switchport");
            r2.ProcessCommand("ip address 10.0.0.2/30");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router ospf 1");
            r2.ProcessCommand("router-id 2.2.2.2");
            r2.ProcessCommand("network 10.0.0.0 0.0.0.3 area 0");
            r2.ProcessCommand("exit");

            var ospfOutput = r1.ProcessCommand("show ospf neighbor");
            Assert.Contains("2.2.2.2", ospfOutput);
            Assert.Contains("FULL", ospfOutput);
            Assert.Contains("10.0.0.2", ospfOutput);
        }

        [Fact]
        public async Task Arista_ConfigureBgp_ShouldEstablishPeering()
        {
            var network = new Network();
            var r1 = new AristaDevice("R1");
            var r2 = new AristaDevice("R2");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "Ethernet1", "R2", "Ethernet1");

            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure");
            r1.ProcessCommand("interface Ethernet1");
            r1.ProcessCommand("no switchport");
            r1.ProcessCommand("ip address 172.16.0.1/30");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router bgp 65001");
            r1.ProcessCommand("router-id 1.1.1.1");
            r1.ProcessCommand("neighbor 172.16.0.2 remote-as 65002");
            r1.ProcessCommand("neighbor 172.16.0.2 description Peer-R2");
            r1.ProcessCommand("network 192.168.1.0/24");
            r1.ProcessCommand("maximum-paths 4");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure");
            r2.ProcessCommand("interface Ethernet1");
            r2.ProcessCommand("no switchport");
            r2.ProcessCommand("ip address 172.16.0.2/30");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router bgp 65002");
            r2.ProcessCommand("router-id 2.2.2.2");
            r2.ProcessCommand("neighbor 172.16.0.1 remote-as 65001");
            r2.ProcessCommand("exit");

            var bgpOutput = r1.ProcessCommand("show bgp summary");
            Assert.Contains("172.16.0.2", bgpOutput);
            Assert.Contains("65002", bgpOutput);
            Assert.Contains("Estab", bgpOutput);
        }

        [Fact]
        public async Task Arista_ConfigureRip_ShouldExchangeRoutes()
        {
            var network = new Network();
            var r1 = new AristaDevice("R1");
            var r2 = new AristaDevice("R2");
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "Ethernet1", "R2", "Ethernet1");

            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure");
            r1.ProcessCommand("interface Ethernet1");
            r1.ProcessCommand("no switchport");
            r1.ProcessCommand("ip address 10.1.1.1/24");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("interface Ethernet2");
            r1.ProcessCommand("no switchport");
            r1.ProcessCommand("ip address 192.168.1.1/24");
            r1.ProcessCommand("no shutdown");
            r1.ProcessCommand("exit");
            r1.ProcessCommand("router rip");
            r1.ProcessCommand("version 2");
            r1.ProcessCommand("network 10.1.1.0");
            r1.ProcessCommand("network 192.168.1.0");
            r1.ProcessCommand("no auto-summary");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure");
            r2.ProcessCommand("interface Ethernet1");
            r2.ProcessCommand("no switchport");
            r2.ProcessCommand("ip address 10.1.1.2/24");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("interface Ethernet2");
            r2.ProcessCommand("no switchport");
            r2.ProcessCommand("ip address 192.168.2.1/24");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("router rip");
            r2.ProcessCommand("version 2");
            r2.ProcessCommand("network 10.1.1.0");
            r2.ProcessCommand("network 192.168.2.0");
            r2.ProcessCommand("exit");

            var routeOutput = r2.ProcessCommand("show ip route");
            Assert.Contains("192.168.1.0", routeOutput);
            Assert.Contains("R", routeOutput);
            Assert.Contains("10.1.1.1", routeOutput);
        }

        [Fact]
        public void Arista_ConfigureStaticRoute_ShouldAddToRoutingTable()
        {
            var r1 = new AristaDevice("R1");
            r1.ProcessCommand("enable");
            r1.ProcessCommand("configure");
            r1.ProcessCommand("ip route 10.10.10.0/24 192.168.1.254");
            r1.ProcessCommand("exit");

            var routeOutput = r1.ProcessCommand("show ip route");
            Assert.Contains("10.10.10.0/24", routeOutput);
            Assert.Contains("S", routeOutput);
            Assert.Contains("192.168.1.254", routeOutput);
        }
    }
}

