using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests alias functionality across mixed vendor topologies.
    /// </summary>
    public class MultiVendorAliasTests
    {
        [Fact]
        public void MultiVendor_AliasPingAndOspfCounters_ShouldMatchFullCommands()
        {
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new JuniperDevice("R2");
            var r3 = new HuaweiDevice("R3");

            network.AddDeviceAsync(r1).Wait();
            network.AddDeviceAsync(r2).Wait();
            network.AddDeviceAsync(r3).Wait();

            network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "ge-0/0/0").Wait();
            network.AddLinkAsync("R2", "ge-0/0/1", "R3", "GigabitEthernet0/0/0").Wait();

            // Configure using aliases
            r1.ProcessCommand("conf t");
            r1.ProcessCommand("int Gi0/0");
            r1.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            r1.ProcessCommand("router ospf 1");
            r1.ProcessCommand("net 192.168.1.0 0.0.0.255 ar 0");

            r2.ProcessCommand("conf");
            r2.ProcessCommand("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            r2.ProcessCommand("set interfaces ge-0/0/1 unit 0 family inet address 192.168.2.1/24");
            r2.ProcessCommand("set protocols ospf area 0.0.0.0 interface ge-0/0/0");
            r2.ProcessCommand("set protocols ospf area 0.0.0.0 interface ge-0/0/1");
            r2.ProcessCommand("commit");

            r3.ProcessCommand("sys");
            r3.ProcessCommand("int GE0/0/0");
            r3.ProcessCommand("ip addr 192.168.2.2 255.255.255.0");
            r3.ProcessCommand("ospf 1");
            r3.ProcessCommand("area 0");
            r3.ProcessCommand("net 192.168.2.0 0.0.0.255");
            r3.ProcessCommand("quit");
            r3.ProcessCommand("quit");

            // Verify with aliases
            var ospfAlias = r1.ProcessCommand("sh ip ospf nei");
            var ospfFull = r1.ProcessCommand("show ip ospf neighbor");
            var pingOutput = r1.ProcessCommand("ping 192.168.2.2");
            var intfAlias = r1.ProcessCommand("sh int Gi0/0");
            var intfFull = r1.ProcessCommand("show interface GigabitEthernet0/0");
            var intfR2 = r2.ProcessCommand("show interfaces ge-0/0/0");
            var intfR3 = r3.ProcessCommand("disp int GE0/0/0");

            // Shutdown with alias
            r3.ProcessCommand("sys");
            r3.ProcessCommand("int GE0/0/0");
            r3.ProcessCommand("shut");
            var pingFail = r1.ProcessCommand("ping 192.168.2.2");
            var intfR3Shut = r3.ProcessCommand("disp int GE0/0/0");

            Assert.Equal(ospfFull, ospfAlias);
            Assert.Contains("Success", pingOutput);
            Assert.Equal(intfFull, intfAlias);
            Assert.Contains("Input packets", intfR2);
            Assert.Contains("Input packets", intfR3);
            Assert.Contains("No response", pingFail);
            Assert.Contains("administratively down", intfR3Shut);
        }
    }
}
