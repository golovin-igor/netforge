using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.AliasTests
{
    /// <summary>
    /// Tests alias functionality across mixed vendor topologies.
    /// </summary>
    public class MultiVendorAliasTests
    {
        [Fact]
        public async Task MultiVendor_AliasPingAndOspfCounters_ShouldMatchFullCommands()
        {
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new JuniperDevice("R2");
            var r3 = new HuaweiDevice("R3");

            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddDeviceAsync(r3);

            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "ge-0/0/0");
            await network.AddLinkAsync("R2", "ge-0/0/1", "R3", "GigabitEthernet0/0/0");

            // Configure using aliases
            await r1.ProcessCommandAsync("conf t");
            await r1.ProcessCommandAsync("int Gi0/0");
            await r1.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await r1.ProcessCommandAsync("router ospf 1");
            await r1.ProcessCommandAsync("net 192.168.1.0 0.0.0.255 ar 0");

            await r2.ProcessCommandAsync("conf");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/0 unit 0 family inet address 192.168.1.2/24");
            await r2.ProcessCommandAsync("set interfaces ge-0/0/1 unit 0 family inet address 192.168.2.1/24");
            await r2.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/0");
            await r2.ProcessCommandAsync("set protocols ospf area 0.0.0.0 interface ge-0/0/1");
            await r2.ProcessCommandAsync("commit");

            await r3.ProcessCommandAsync("sys");
            await r3.ProcessCommandAsync("int GE0/0/0");
            await r3.ProcessCommandAsync("ip addr 192.168.2.2 255.255.255.0");
            await r3.ProcessCommandAsync("ospf 1");
            await r3.ProcessCommandAsync("area 0");
            await r3.ProcessCommandAsync("net 192.168.2.0 0.0.0.255");
            await r3.ProcessCommandAsync("quit");
            await r3.ProcessCommandAsync("quit");

            // Verify with aliases
            var ospfAlias = await r1.ProcessCommandAsync("sh ip ospf nei");
            var ospfFull = await r1.ProcessCommandAsync("show ip ospf neighbor");
            var pingOutput = await r1.ProcessCommandAsync("ping 192.168.2.2");
            var intfAlias = await r1.ProcessCommandAsync("sh int Gi0/0");
            var intfFull = await r1.ProcessCommandAsync("show interface GigabitEthernet0/0");
            var intfR2 = await r2.ProcessCommandAsync("show interfaces ge-0/0/0");
            var intfR3 = await r3.ProcessCommandAsync("disp int GE0/0/0");

            // Shutdown with alias
            await r3.ProcessCommandAsync("sys");
            await r3.ProcessCommandAsync("int GE0/0/0");
            await r3.ProcessCommandAsync("shut");
            var pingFail = await r1.ProcessCommandAsync("ping 192.168.2.2");
            var intfR3Shut = await r3.ProcessCommandAsync("disp int GE0/0/0");

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
