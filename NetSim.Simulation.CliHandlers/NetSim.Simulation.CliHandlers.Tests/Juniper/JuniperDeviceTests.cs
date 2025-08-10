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
        public async Task JuniperConfigureInterfaceAndPingShouldSucceed()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.1/24");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync("commit");
            await r2.ProcessCommandAsync("exit");

            var pingOutput = await r1.ProcessCommandAsync("ping 192.168.1.2");
            Assert.Contains("5 packets transmitted, 5 packets received", pingOutput);
        }

        [Fact]
        public async Task JuniperConfigureOspfShouldFormAdjacency()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/30");
            await r1.ProcessCommandAsync("set routing-options router-id 1.1.1.1");
            await r1.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/0.0");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            await r2.ProcessCommandAsync("set routing-options router-id 2.2.2.2");
            await r2.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/0.0");
            await r2.ProcessCommandAsync("commit");
            await r2.ProcessCommandAsync("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            var ospfOutput = await r1.ProcessCommandAsync("show ospf neighbor");
            Assert.Contains("10.0.0.2", ospfOutput);
            Assert.Contains("Full", ospfOutput);
        }

        [Fact]
        public async Task JuniperConfigureBgpShouldEstablishPeering()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/30");
            await r1.ProcessCommandAsync("set routing-options autonomous-system 65001");
            await r1.ProcessCommandAsync("set protocols bgp group EBGP type external");
            await r1.ProcessCommandAsync("set protocols bgp group EBGP peer-as 65002");
            await r1.ProcessCommandAsync("set protocols bgp group EBGP neighbor 10.0.0.2");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            await r2.ProcessCommandAsync("set routing-options autonomous-system 65002");
            await r2.ProcessCommandAsync("set protocols bgp group EBGP type external");
            await r2.ProcessCommandAsync("set protocols bgp group EBGP peer-as 65001");
            await r2.ProcessCommandAsync("set protocols bgp group EBGP neighbor 10.0.0.1");
            await r2.ProcessCommandAsync("commit");
            await r2.ProcessCommandAsync("exit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var bgpOutput = await r1.ProcessCommandAsync("show bgp summary");
            Assert.Contains("10.0.0.2", bgpOutput);
            Assert.Contains("Establ", bgpOutput);
        }

        [Fact]
        public async Task JuniperConfigureRipShouldExchangeRoutes()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/24");
            await r1.ProcessCommandAsync("set interfaces lo0 unit 0 family inet address 1.1.1.1/32");
            await r1.ProcessCommandAsync("set protocols rip group RIP-GROUP export direct");
            await r1.ProcessCommandAsync("set protocols rip group RIP-GROUP neighbor ge-0/0/0.0");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/24");
            await r2.ProcessCommandAsync("set interfaces lo0 unit 0 family inet address 2.2.2.2/32");
            await r2.ProcessCommandAsync("set protocols rip group RIP-GROUP export direct");
            await r2.ProcessCommandAsync("set protocols rip group RIP-GROUP neighbor ge-0/0/0.0");
            await r2.ProcessCommandAsync("commit");
            await r2.ProcessCommandAsync("exit");

            network.UpdateProtocols();
            await Task.Delay(100);

            var routeOutput = await r1.ProcessCommandAsync("show route protocol rip");
            Assert.Contains("2.2.2.2/32", routeOutput);
            Assert.Contains("10.0.0.2", routeOutput);
        }

        [Fact]
        public async Task JuniperInterfaceDisableShouldAffectConnectivity()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/30");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            await r2.ProcessCommandAsync("commit");
            await r2.ProcessCommandAsync("exit");

            Assert.Contains("5 packets transmitted, 5 packets received", await r1.ProcessCommandAsync("ping 10.0.0.2"));

            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 disable");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            var ifaceOutput = await r1.ProcessCommandAsync("show interfaces ge-0/0/0 extensive");
            Assert.Contains("Admin  down", ifaceOutput);
            Assert.Contains("0 packets transmitted, 0 packets received", await r1.ProcessCommandAsync("ping 10.0.0.2"));
        }

        [Fact]
        public async Task JuniperFirewallFilterShouldBlockTraffic()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/30");
            await r1.ProcessCommandAsync("set firewall family inet filter BLOCK_PING term 1 from source-address 10.0.0.2/32");
            await r1.ProcessCommandAsync("set firewall family inet filter BLOCK_PING term 1 from protocol icmp");
            await r1.ProcessCommandAsync("set firewall family inet filter BLOCK_PING term 1 then discard");
            await r1.ProcessCommandAsync("set firewall family inet filter BLOCK_PING term default then accept");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet filter input BLOCK_PING");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.2/30");
            await r2.ProcessCommandAsync("commit");
            await r2.ProcessCommandAsync("exit");

            Assert.Contains("0 packets transmitted, 0 packets received", await r2.ProcessCommandAsync("ping 10.0.0.1"));
        }

        [Fact]
        public async Task JuniperConfigureLldpShouldShowNeighbors()
        {
            var (network, r1, r2) = await SetupNetworkWithTwoDevicesAsync();
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set protocols lldp interface ge-0/0/0");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            await r2.ProcessCommandAsync("configure");
            await r2.ProcessCommandAsync("set protocols lldp interface ge-0/0/0");
            await r2.ProcessCommandAsync("commit");
            await r2.ProcessCommandAsync("exit");
            
            network.UpdateProtocols();
            await Task.Delay(50);

            var lldpOutput = await r1.ProcessCommandAsync("show lldp neighbors");
            Assert.Contains("ge-0/0/0.0", lldpOutput);
            Assert.Contains("R2", lldpOutput);
        }

        [Fact]
        public async Task JuniperCommitAndRollbackShouldWork()
        {
            var r1 = new JuniperDevice("R1");
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set system host-name ROUTER_NEW_NAME");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/1 unit 0 family inet address 1.1.1.1/24");
            var beforeCommit = await r1.ProcessCommandAsync("show configuration");
            Assert.Contains("ROUTER_NEW_NAME", beforeCommit);
            Assert.Contains("1.1.1.1/24", beforeCommit);

            await r1.ProcessCommandAsync("commit");
            var afterCommit = await r1.ProcessCommandAsync("show configuration");
            Assert.Contains("ROUTER_NEW_NAME", afterCommit);
            Assert.Contains("1.1.1.1/24", afterCommit);

            await r1.ProcessCommandAsync("rollback 0");
            var afterRollback = await r1.ProcessCommandAsync("show configuration");
            Assert.DoesNotContain("ROUTER_NEW_NAME", afterRollback);
            Assert.DoesNotContain("1.1.1.1/24", afterRollback);
        }

        [Fact]
        public async Task JuniperConfigureStaticRouteShouldAddToRoutingTable()
        {
            var network = new Network();
            var r1 = new JuniperDevice("R1");
            await network.AddDeviceAsync(r1);

            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set routing-options static route 10.10.10.0/24 next-hop 192.168.1.254");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            var routeOutput = await r1.ProcessCommandAsync("show route");
            Assert.Contains("10.10.10.0/24", routeOutput);
            Assert.Contains("192.168.1.254", routeOutput);
        }

        [Fact]
        public async Task JuniperShowCommandsShouldReturnOutput()
        {
            var r1 = new JuniperDevice("R1");
            await r1.ProcessCommandAsync("configure");
            await r1.ProcessCommandAsync("set system host-name TestRouter");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/24");
            await r1.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 description \"WAN Link\"");
            await r1.ProcessCommandAsync("commit");
            await r1.ProcessCommandAsync("exit");

            var config = await r1.ProcessCommandAsync("show configuration");
            Assert.Contains("host-name TestRouter;", config);
            Assert.Contains("address 10.0.0.1/24;", config);
            Assert.Contains("description \"WAN Link\";", config);

            var iface = await r1.ProcessCommandAsync("show interfaces ge-0/0/0 terse");
            Assert.Contains("ge-0/0/0.0", iface);
            Assert.Contains("10.0.0.1/24", iface);

            var route = await r1.ProcessCommandAsync("show route");
            Assert.Contains("10.0.0.0/24", route);
            Assert.Contains("Direct", route);
        }
    }
} 
