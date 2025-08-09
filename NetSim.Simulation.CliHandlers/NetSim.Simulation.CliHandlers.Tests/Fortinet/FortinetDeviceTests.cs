using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Fortinet
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
        public async Task Fortinet_ConfigureInterfacesAndPing_ShouldSucceed()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            fw1.ProcessCommand("config system interface");
            fw1.ProcessCommand("edit port1");
            fw1.ProcessCommand("set ip 192.168.1.1 255.255.255.0");
            fw1.ProcessCommand("set allowaccess ping");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            fw2.ProcessCommand("config system interface");
            fw2.ProcessCommand("edit port1");
            fw2.ProcessCommand("set ip 192.168.1.2 255.255.255.0");
            fw2.ProcessCommand("set allowaccess ping");
            fw2.ProcessCommand("next");
            fw2.ProcessCommand("end");

            var pingOutput = fw1.ProcessCommand("execute ping 192.168.1.2");
            Assert.Contains("5 packets transmitted, 5 packets received", pingOutput);
        }

        [Fact]
        public async Task Fortinet_StaticRouting_ShouldWork()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            fw1.ProcessCommand("config system interface");
            fw1.ProcessCommand("edit port1");
            fw1.ProcessCommand("set ip 10.0.1.1 255.255.255.0");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("edit port2");
            fw1.ProcessCommand("set ip 192.168.1.1 255.255.255.0");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            fw2.ProcessCommand("config system interface");
            fw2.ProcessCommand("edit port1");
            fw2.ProcessCommand("set ip 10.0.1.2 255.255.255.0");
            fw2.ProcessCommand("next");
            fw2.ProcessCommand("edit port2");
            fw2.ProcessCommand("set ip 192.168.2.1 255.255.255.0");
            fw2.ProcessCommand("next");
            fw2.ProcessCommand("end");

            fw1.ProcessCommand("config router static");
            fw1.ProcessCommand("edit 1");
            fw1.ProcessCommand("set dst 192.168.2.0 255.255.255.0");
            fw1.ProcessCommand("set gateway 10.0.1.2");
            fw1.ProcessCommand("set device port1");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            network.UpdateProtocols();
            await Task.Delay(50);

            var routeTableFw1 = fw1.ProcessCommand("get router info routing-table all");
            Assert.Contains("S*      192.168.2.0/24 [10/0] via 10.0.1.2, port1", routeTableFw1);
        }

        [Fact]
        public async Task Fortinet_FirewallPolicy_ShouldBlockTraffic()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            fw1.ProcessCommand("config system interface");
            fw1.ProcessCommand("edit port1");
            fw1.ProcessCommand("set ip 10.0.0.1 255.255.255.0");
            fw1.ProcessCommand("set allowaccess ping http https ssh");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            fw2.ProcessCommand("config system interface");
            fw2.ProcessCommand("edit port1");
            fw2.ProcessCommand("set ip 10.0.0.2 255.255.255.0");
            fw2.ProcessCommand("set allowaccess ping http https ssh");
            fw2.ProcessCommand("next");
            fw2.ProcessCommand("end");

            fw1.ProcessCommand("config firewall policy");
            fw1.ProcessCommand("edit 1");
            fw1.ProcessCommand("set srcintf port1");
            fw1.ProcessCommand("set dstintf port1");
            fw1.ProcessCommand("set srcaddr 10.0.0.2/32");
            fw1.ProcessCommand("set dstaddr 10.0.0.1/32");
            fw1.ProcessCommand("set action deny");
            fw1.ProcessCommand("set service ICMP");
            fw1.ProcessCommand("set schedule always");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            var pingOutput = fw2.ProcessCommand("execute ping 10.0.0.1");
            Assert.DoesNotContain("0% packet loss", pingOutput); // Expecting loss
            Assert.Contains("100% packet loss", pingOutput); 
        }

        [Fact]
        public async Task Fortinet_OspfRouting_ShouldFormAdjacency()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            fw1.ProcessCommand("config system interface");
            fw1.ProcessCommand("edit port1");
            fw1.ProcessCommand("set ip 10.0.0.1 255.255.255.252");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            fw2.ProcessCommand("config system interface");
            fw2.ProcessCommand("edit port1");
            fw2.ProcessCommand("set ip 10.0.0.2 255.255.255.252");
            fw2.ProcessCommand("next");
            fw2.ProcessCommand("end");

            fw1.ProcessCommand("config router ospf");
            fw1.ProcessCommand("set router-id 1.1.1.1");
            fw1.ProcessCommand("config network");
            fw1.ProcessCommand("edit 1");
            fw1.ProcessCommand("set prefix 10.0.0.0 255.255.255.252");
            fw1.ProcessCommand("set area 0.0.0.0");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");
            fw1.ProcessCommand("end");

            fw2.ProcessCommand("config router ospf");
            fw2.ProcessCommand("set router-id 2.2.2.2");
            fw2.ProcessCommand("config network");
            fw2.ProcessCommand("edit 1");
            fw2.ProcessCommand("set prefix 10.0.0.0 255.255.255.252");
            fw2.ProcessCommand("set area 0.0.0.0");
            fw2.ProcessCommand("next");
            fw2.ProcessCommand("end");
            fw2.ProcessCommand("end");

            network.UpdateProtocols();
            await Task.Delay(100);

            var ospfNeighbor = fw1.ProcessCommand("get router info ospf neighbor");
            Assert.Contains("10.0.0.2", ospfNeighbor);
            Assert.Contains("Full", ospfNeighbor);
        }

        [Fact]
        public async Task Fortinet_BgpRouting_ShouldEstablishPeering()
        {
            var (network, fw1, fw2) = await SetupNetworkWithTwoDevicesAsync();
            fw1.ProcessCommand("config system interface");
            fw1.ProcessCommand("edit port1");
            fw1.ProcessCommand("set ip 10.0.0.1 255.255.255.252");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            fw2.ProcessCommand("config system interface");
            fw2.ProcessCommand("edit port1");
            fw2.ProcessCommand("set ip 10.0.0.2 255.255.255.252");
            fw2.ProcessCommand("next");
            fw2.ProcessCommand("end");

            fw1.ProcessCommand("config router bgp");
            fw1.ProcessCommand("set as 65001");
            fw1.ProcessCommand("set router-id 1.1.1.1");
            fw1.ProcessCommand("config neighbor");
            fw1.ProcessCommand("edit 10.0.0.2");
            fw1.ProcessCommand("set remote-as 65002");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");
            fw1.ProcessCommand("end");

            fw2.ProcessCommand("config router bgp");
            fw2.ProcessCommand("set as 65002");
            fw2.ProcessCommand("set router-id 2.2.2.2");
            fw2.ProcessCommand("config neighbor");
            fw2.ProcessCommand("edit 10.0.0.1");
            fw2.ProcessCommand("set remote-as 65001");
            fw2.ProcessCommand("next");
            fw2.ProcessCommand("end");
            fw2.ProcessCommand("end");

            network.UpdateProtocols();
            await Task.Delay(200);

            var bgpSummary = fw1.ProcessCommand("get router info bgp summary");
            Assert.Contains("10.0.0.2", bgpSummary);
            Assert.Contains("Established", bgpSummary);
        }

        [Fact]
        public async Task ShowFullConfiguration_ShouldDisplayConfiguredSettings() 
        {
            var (network, fw1, _) = await SetupNetworkWithTwoDevicesAsync();
            fw1.ProcessCommand("config system interface");
            fw1.ProcessCommand("edit port1");
            fw1.ProcessCommand("set ip 192.168.1.1 255.255.255.0");
            fw1.ProcessCommand("set alias TestAlias");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            fw1.ProcessCommand("config router static");
            fw1.ProcessCommand("edit 1");
            fw1.ProcessCommand("set gateway 192.168.1.254");
            fw1.ProcessCommand("next");
            fw1.ProcessCommand("end");

            var output = fw1.ProcessCommand("show full-configuration");
            Assert.Contains("set ip 192.168.1.1 255.255.255.0", output);
            Assert.Contains("set alias TestAlias", output);
            Assert.Contains("set gateway 192.168.1.254", output);
        }

        [Fact]
        public void InvalidCommand_ShouldReturnError() // Does not use network setup
        {
            var fw1 = new FortinetDevice("FW1");
            var output = fw1.ProcessCommand("config system blah");
            Assert.Contains("Command parse error", output); 
        }
    }
} 
