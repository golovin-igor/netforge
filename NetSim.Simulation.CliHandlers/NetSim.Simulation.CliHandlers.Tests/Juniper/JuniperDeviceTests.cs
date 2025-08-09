using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Juniper
{
    public class JuniperDeviceTests
    {
        private async Task<(Network, JuniperDevice, JuniperDevice)> SetupNetworkWithTwoDevicesAsync(string r1Name = "R1", string r2Name = "R2", string r1Interface = "ge-0/0/0", string r2Interface = "ge-0/0/0")
        {
            var network = new Network();
            var r1 = new JuniperDevice(r1Name);
            var r2 = new JuniperDevice(r2Name);
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync(r1Name, r1Interface, r2Name, r2Interface);
            return (network, r1, r2);
        }

        [Fact]
        public async Task Juniper_ConfigureInterfaceAndPing_ShouldSucceed()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            r2.ProcessCommand("commit");
            r2.ProcessCommand("exit");

            var pingOutput = r1.ProcessCommand("ping 192.168.1.2");
            Assert.Contains("5 packets transmitted, 5 packets received", pingOutput);
        }

        [Fact]
        public async Task Juniper_ConfigureOspf_ShouldFormAdjacency()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/30");
            r1.ProcessCommand("set routing-options router-id 1.1.1.1");
            r1.ProcessCommand("set protocols ospf area 0.0.0.0 interface ge-0/0/0.0");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            r2.ProcessCommand("set routing-options router-id 2.2.2.2");
            r2.ProcessCommand("set protocols ospf area 0.0.0.0 interface ge-0/0/0.0");
            r2.ProcessCommand("commit");
            r2.ProcessCommand("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            var ospfOutput = r1.ProcessCommand("show ospf neighbor");
            Assert.Contains("10.0.0.2", ospfOutput);
            Assert.Contains("Full", ospfOutput);
        }

        [Fact]
        public async Task Juniper_ConfigureBgp_ShouldEstablishPeering()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/30");
            r1.ProcessCommand("set routing-options autonomous-system 65001");
            r1.ProcessCommand("set protocols bgp group EBGP type external");
            r1.ProcessCommand("set protocols bgp group EBGP peer-as 65002");
            r1.ProcessCommand("set protocols bgp group EBGP neighbor 10.0.0.2");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            r2.ProcessCommand("set routing-options autonomous-system 65002");
            r2.ProcessCommand("set protocols bgp group EBGP type external");
            r2.ProcessCommand("set protocols bgp group EBGP peer-as 65001");
            r2.ProcessCommand("set protocols bgp group EBGP neighbor 10.0.0.1");
            r2.ProcessCommand("commit");
            r2.ProcessCommand("exit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var bgpOutput = r1.ProcessCommand("show bgp summary");
            Assert.Contains("10.0.0.2", bgpOutput);
            Assert.Contains("Establ", bgpOutput);
        }

        [Fact]
        public async Task Juniper_ConfigureRip_ShouldExchangeRoutes()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/24");
            r1.ProcessCommand("set interfaces lo0 unit 0 family inet address 1.1.1.1/32");
            r1.ProcessCommand("set protocols rip group RIP-GROUP export direct");
            r1.ProcessCommand("set protocols rip group RIP-GROUP neighbor ge-0/0/0.0");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/24");
            r2.ProcessCommand("set interfaces lo0 unit 0 family inet address 2.2.2.2/32");
            r2.ProcessCommand("set protocols rip group RIP-GROUP export direct");
            r2.ProcessCommand("set protocols rip group RIP-GROUP neighbor ge-0/0/0.0");
            r2.ProcessCommand("commit");
            r2.ProcessCommand("exit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var routeOutput = r1.ProcessCommand("show route protocol rip");
            Assert.Contains("2.2.2.2/32", routeOutput);
            Assert.Contains("10.0.0.2", routeOutput);
        }

        [Fact]
        public async Task Juniper_InterfaceDisable_ShouldAffectConnectivity()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/30");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            r2.ProcessCommand("commit");
            r2.ProcessCommand("exit");

            Assert.Contains("5 packets transmitted, 5 packets received", r1.ProcessCommand("ping 10.0.0.2"));

            r1.ProcessCommand("configure");
            r1.ProcessCommand("set interfaces ge-0/0/0 disable");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            var ifaceOutput = r1.ProcessCommand("show interfaces ge-0/0/0 extensive");
            Assert.Contains("Admin  down", ifaceOutput);
            Assert.Contains("0 packets transmitted, 0 packets received", r1.ProcessCommand("ping 10.0.0.2"));
        }

        [Fact]
        public async Task Juniper_FirewallFilter_ShouldBlockTraffic()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/30");
            r1.ProcessCommand("set firewall family inet filter BLOCK_PING term 1 from source-address 10.0.0.2/32");
            r1.ProcessCommand("set firewall family inet filter BLOCK_PING term 1 from protocol icmp");
            r1.ProcessCommand("set firewall family inet filter BLOCK_PING term 1 then discard");
            r1.ProcessCommand("set firewall family inet filter BLOCK_PING term default then accept");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet filter input BLOCK_PING");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            r2.ProcessCommand("commit");
            r2.ProcessCommand("exit");

            Assert.Contains("0 packets transmitted, 0 packets received", r2.ProcessCommand("ping 10.0.0.1"));
        }

        [Fact]
        public async Task Juniper_ConfigureLldp_ShouldShowNeighbors()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set protocols lldp interface ge-0/0/0");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            r2.ProcessCommand("configure");
            r2.ProcessCommand("set protocols lldp interface ge-0/0/0");
            r2.ProcessCommand("commit");
            r2.ProcessCommand("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            var lldpOutput = r1.ProcessCommand("show lldp neighbors");
            Assert.Contains("ge-0/0/0.0", lldpOutput);
            Assert.Contains("R2", lldpOutput);
        }

        [Fact]
        public void Juniper_CommitAndRollback_ShouldWork()
        {
            var r1 = new JuniperDevice("R1");
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set system host-name ROUTER_NEW_NAME");
            r1.ProcessCommand("set interfaces ge-0/0/1 unit 0 family inet address 1.1.1.1/24");
            var beforeCommit = r1.ProcessCommand("show configuration");
            Assert.Contains("ROUTER_NEW_NAME", beforeCommit);
            Assert.Contains("1.1.1.1/24", beforeCommit);

            r1.ProcessCommand("commit");
            var afterCommit = r1.ProcessCommand("show configuration");
            Assert.Contains("ROUTER_NEW_NAME", afterCommit);
            Assert.Contains("1.1.1.1/24", afterCommit);

            r1.ProcessCommand("rollback 0");
            var afterRollback = r1.ProcessCommand("show configuration");
            Assert.DoesNotContain("ROUTER_NEW_NAME", afterRollback);
            Assert.DoesNotContain("1.1.1.1/24", afterRollback);
        }

        [Fact]
        public async Task Juniper_ConfigureStaticRoute_ShouldAddToRoutingTable()
        {
            var network = new Network();
            var r1 = new JuniperDevice("R1");
            await network.AddDeviceAsync(r1);

            r1.ProcessCommand("configure");
            r1.ProcessCommand("set routing-options static route 10.10.10.0/24 next-hop 192.168.1.254");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            var routeOutput = r1.ProcessCommand("show route");
            Assert.Contains("10.10.10.0/24", routeOutput);
            Assert.Contains("192.168.1.254", routeOutput);
        }

        [Fact]
        public void Juniper_ShowCommands_ShouldReturnOutput()
        {
            var r1 = new JuniperDevice("R1");
            r1.ProcessCommand("configure");
            r1.ProcessCommand("set system host-name TestRouter");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/24");
            r1.ProcessCommand("set interfaces ge-0/0/0 unit 0 description \"WAN Link\"");
            r1.ProcessCommand("commit");
            r1.ProcessCommand("exit");

            var config = r1.ProcessCommand("show configuration");
            Assert.Contains("host-name TestRouter;", config);
            Assert.Contains("address 10.0.0.1/24;", config);
            Assert.Contains("description \"WAN Link\";", config);

            var iface = r1.ProcessCommand("show interfaces ge-0/0/0 terse");
            Assert.Contains("ge-0/0/0.0", iface);
            Assert.Contains("10.0.0.1/24", iface);

            var route = r1.ProcessCommand("show route");
            Assert.Contains("10.0.0.0/24", route);
            Assert.Contains("Direct", route);
        }
    }
} 
