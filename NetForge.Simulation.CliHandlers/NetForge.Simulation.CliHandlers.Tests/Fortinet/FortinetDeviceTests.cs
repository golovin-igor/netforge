using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Fortinet
{
    public class FortinetDeviceTests
    {
        private async Task<(Network, FortinetDevice, FortinetDevice)> SetupNetworkWithTwoDevicesAsync(string fw1Name = "FW1", string fw2Name = "FW2", string fw1Interface = "port1", string fw2Interface = "port1")
        {
            var network = new Network();
            var fw1 = new FortinetDevice(fw1Name);
            var fw2 = new FortinetDevice(fw2Name);
            await network.AddDeviceAsync(fw1);
            await network.AddDeviceAsync(fw2);
            await network.AddLinkAsync(fw1Name, fw1Interface, fw2Name, fw2Interface);
            return (network, fw1, fw2);
        }

        [Fact]
        public async Task FortinetConfigureInterfacesAndPingShouldSucceed()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            await fw1.ProcessCommandAsync("config system interface");
            await fw1.ProcessCommandAsync("edit port1");
            await fw1.ProcessCommandAsync("set ip 192.168.1.1 255.255.255.0");
            await fw1.ProcessCommandAsync("set allowaccess ping");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            await fw2.ProcessCommandAsync("config system interface");
            await fw2.ProcessCommandAsync("edit port1");
            await fw2.ProcessCommandAsync("set ip 192.168.1.2 255.255.255.0");
            await fw2.ProcessCommandAsync("set allowaccess ping");
            await fw2.ProcessCommandAsync("next");
            await fw2.ProcessCommandAsync("end");

            var pingOutput = await fw1.ProcessCommandAsync("execute ping 192.168.1.2");
            Assert.Contains("5 packets transmitted, 5 packets received", pingOutput);
        }

        [Fact]
        public async Task FortinetStaticRoutingShouldWork()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            await fw1.ProcessCommandAsync("config system interface");
            await fw1.ProcessCommandAsync("edit port1");
            await fw1.ProcessCommandAsync("set ip 10.0.1.1 255.255.255.0");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("edit port2");
            await fw1.ProcessCommandAsync("set ip 192.168.1.1 255.255.255.0");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            await fw2.ProcessCommandAsync("config system interface");
            await fw2.ProcessCommandAsync("edit port1");
            await fw2.ProcessCommandAsync("set ip 10.0.1.2 255.255.255.0");
            await fw2.ProcessCommandAsync("next");
            await fw2.ProcessCommandAsync("edit port2");
            await fw2.ProcessCommandAsync("set ip 192.168.2.1 255.255.255.0");
            await fw2.ProcessCommandAsync("next");
            await fw2.ProcessCommandAsync("end");

            await fw1.ProcessCommandAsync("config router static");
            await fw1.ProcessCommandAsync("edit 1");
            await fw1.ProcessCommandAsync("set dst 192.168.2.0 255.255.255.0");
            await fw1.ProcessCommandAsync("set gateway 10.0.1.2");
            await fw1.ProcessCommandAsync("set device port1");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            network.UpdateProtocols();
            await Task.Delay(50);

            var routeTableFw1 = await fw1.ProcessCommandAsync("get router info routing-table all");
            Assert.Contains("S*      192.168.2.0/24 [10/0] via 10.0.1.2, port1", routeTableFw1);
        }

        [Fact]
        public async Task FortinetFirewallPolicyShouldBlockTraffic()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            await fw1.ProcessCommandAsync("config system interface");
            await fw1.ProcessCommandAsync("edit port1");
            await fw1.ProcessCommandAsync("set ip 10.0.0.1 255.255.255.0");
            await fw1.ProcessCommandAsync("set allowaccess ping http https ssh");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            await fw2.ProcessCommandAsync("config system interface");
            await fw2.ProcessCommandAsync("edit port1");
            await fw2.ProcessCommandAsync("set ip 10.0.0.2 255.255.255.0");
            await fw2.ProcessCommandAsync("set allowaccess ping http https ssh");
            await fw2.ProcessCommandAsync("next");
            await fw2.ProcessCommandAsync("end");

            await fw1.ProcessCommandAsync("config firewall policy");
            await fw1.ProcessCommandAsync("edit 1");
            await fw1.ProcessCommandAsync("set srcintf port1");
            await fw1.ProcessCommandAsync("set dstintf port1");
            await fw1.ProcessCommandAsync("set srcaddr 10.0.0.2/32");
            await fw1.ProcessCommandAsync("set dstaddr 10.0.0.1/32");
            await fw1.ProcessCommandAsync("set action deny");
            await fw1.ProcessCommandAsync("set service ICMP");
            await fw1.ProcessCommandAsync("set schedule always");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            var pingOutput = await fw2.ProcessCommandAsync("execute ping 10.0.0.1");
            Assert.DoesNotContain("0% packet loss", pingOutput); // Expecting loss
            Assert.Contains("100% packet loss", pingOutput); 
        }

        [Fact]
        public async Task FortinetOspfRoutingShouldFormAdjacency()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            await fw1.ProcessCommandAsync("config system interface");
            await fw1.ProcessCommandAsync("edit port1");
            await fw1.ProcessCommandAsync("set ip 10.0.0.1 255.255.255.252");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            await fw2.ProcessCommandAsync("config system interface");
            await fw2.ProcessCommandAsync("edit port1");
            await fw2.ProcessCommandAsync("set ip 10.0.0.2 255.255.255.252");
            await fw2.ProcessCommandAsync("next");
            await fw2.ProcessCommandAsync("end");

            await fw1.ProcessCommandAsync("config router ospf");
            await fw1.ProcessCommandAsync("set router-id 1.1.1.1");
            await fw1.ProcessCommandAsync("config network");
            await fw1.ProcessCommandAsync("edit 1");
            await fw1.ProcessCommandAsync("set prefix 10.0.0.0 255.255.255.252");
            await fw1.ProcessCommandAsync("set area 0.0.0.0");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");
            await fw1.ProcessCommandAsync("end");

            await fw2.ProcessCommandAsync("config router ospf");
            await fw2.ProcessCommandAsync("set router-id 2.2.2.2");
            await fw2.ProcessCommandAsync("config network");
            await fw2.ProcessCommandAsync("edit 1");
            await fw2.ProcessCommandAsync("set prefix 10.0.0.0 255.255.255.252");
            await fw2.ProcessCommandAsync("set area 0.0.0.0");
            await fw2.ProcessCommandAsync("next");
            await fw2.ProcessCommandAsync("end");
            await fw2.ProcessCommandAsync("end");

            network.UpdateProtocols();
            await Task.Delay(100);

            var ospfNeighbor = await fw1.ProcessCommandAsync("get router info ospf neighbor");
            Assert.Contains("10.0.0.2", ospfNeighbor);
            Assert.Contains("Full", ospfNeighbor);
        }

        [Fact]
        public async Task FortinetBgpRoutingShouldEstablishPeering()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            await fw1.ProcessCommandAsync("config system interface");
            await fw1.ProcessCommandAsync("edit port1");
            await fw1.ProcessCommandAsync("set ip 10.0.0.1 255.255.255.252");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            await fw2.ProcessCommandAsync("config system interface");
            await fw2.ProcessCommandAsync("edit port1");
            await fw2.ProcessCommandAsync("set ip 10.0.0.2 255.255.255.252");
            await fw2.ProcessCommandAsync("next");
            await fw2.ProcessCommandAsync("end");

            await fw1.ProcessCommandAsync("config router bgp");
            await fw1.ProcessCommandAsync("set as 65001");
            await fw1.ProcessCommandAsync("set router-id 1.1.1.1");
            await fw1.ProcessCommandAsync("config neighbor");
            await fw1.ProcessCommandAsync("edit 10.0.0.2");
            await fw1.ProcessCommandAsync("set remote-as 65002");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");
            await fw1.ProcessCommandAsync("end");

            await fw2.ProcessCommandAsync("config router bgp");
            await fw2.ProcessCommandAsync("set as 65002");
            await fw2.ProcessCommandAsync("set router-id 2.2.2.2");
            await fw2.ProcessCommandAsync("config neighbor");
            await fw2.ProcessCommandAsync("edit 10.0.0.1");
            await fw2.ProcessCommandAsync("set remote-as 65001");
            await fw2.ProcessCommandAsync("next");
            await fw2.ProcessCommandAsync("end");
            await fw2.ProcessCommandAsync("end");

            network.UpdateProtocols();
            await Task.Delay(200);

            var bgpSummary = await fw1.ProcessCommandAsync("get router info bgp summary");
            Assert.Contains("10.0.0.2", bgpSummary);
            Assert.Contains("Established", bgpSummary);
        }

        [Fact]
        public async Task ShowFullConfigurationShouldDisplayConfiguredSettings() 
        {
            var (network, fw1, _) = await SetupNetworkWithTwoDevicesAsync();
            await fw1.ProcessCommandAsync("config system interface");
            await fw1.ProcessCommandAsync("edit port1");
            await fw1.ProcessCommandAsync("set ip 192.168.1.1 255.255.255.0");
            await fw1.ProcessCommandAsync("set alias TestAlias");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            await fw1.ProcessCommandAsync("config router static");
            await fw1.ProcessCommandAsync("edit 1");
            await fw1.ProcessCommandAsync("set gateway 192.168.1.254");
            await fw1.ProcessCommandAsync("next");
            await fw1.ProcessCommandAsync("end");

            var output = await fw1.ProcessCommandAsync("show full-configuration");
            Assert.Contains("set ip 192.168.1.1 255.255.255.0", output);
            Assert.Contains("set alias TestAlias", output);
            Assert.Contains("set gateway 192.168.1.254", output);
        }

        [Fact]
        public async Task InvalidCommandShouldReturnError() // Does not use network setup
        {
            var fw1 = new FortinetDevice("FW1");
            var output = await fw1.ProcessCommandAsync("config system blah");
            Assert.Contains("Command parse error", output); 
        }
    }
} 
