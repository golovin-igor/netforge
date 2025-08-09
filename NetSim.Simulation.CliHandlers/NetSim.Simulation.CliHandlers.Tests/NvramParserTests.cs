using NetSim.Simulation.Core;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers
{
    public class NvramParserTests
    {
        [Fact]
        public void CiscoNvramParser_ShouldApplyConfig()
        {
            var device = new CiscoDevice("R1");
            var nvram = @"interface GigabitEthernet0/0
 ip address 192.168.1.1 255.255.255.0
 no shutdown
 exit
 vlan 10
 name SALES";

            var parser = NvramParserFactory.GetParser(device.Vendor);
            parser.Apply(nvram, device);

            var iface = device.GetInterface("GigabitEthernet0/0");
            Assert.Equal("192.168.1.1", iface.IpAddress);
            var vlanOut = device.ProcessCommand("show vlan brief");
            Assert.Contains("10", vlanOut);
            Assert.Contains("SALES", vlanOut);
        }

        [Fact]
        public void JuniperNvramParser_ShouldApplyConfig()
        {
            var device = new JuniperDevice("R1");
            var nvram = @"set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/24
set routing-options router-id 1.1.1.1
set protocols ospf area 0.0.0.0 interface ge-0/0/0";

            var parser = NvramParserFactory.GetParser(device.Vendor);
            parser.Apply(nvram, device);

            var show = device.ProcessCommand("show ospf neighbor");
            // No neighbor yet, but OSPF should be configured
            Assert.Contains("ge-0/0/0", device.ProcessCommand("show ospf interface"));
            var iface = device.GetInterface("ge-0/0/0");
            Assert.Equal("10.0.0.1", iface.IpAddress);
        }

        [Fact]
        public void HuaweiNvramParser_ShouldApplyConfig()
        {
            var device = new HuaweiDevice("SW1");
            var nvram = @"vlan 20";
            var parser = NvramParserFactory.GetParser(device.Vendor);
            parser.Apply(nvram, device);
            var outp = device.ProcessCommand("display vlan");
            Assert.Contains("20", outp);
        }

        [Fact]
        public void AristaNvramParser_ShouldApplyConfig()
        {
            var device = new AristaDevice("SW1");
            var nvram = @"vlan 30
name DATA
interface Ethernet1
no switchport
ip address 172.16.0.1/24";
            var parser = NvramParserFactory.GetParser(device.Vendor);
            parser.Apply(nvram, device);
            var vlanOut = device.ProcessCommand("show vlan");
            Assert.Contains("30", vlanOut);
            Assert.Contains("DATA", vlanOut);
            Assert.Contains("Ethernet1", device.ProcessCommand("show ip interface brief"));
        }

        [Fact]
        public void NokiaNvramParser_ShouldApplyConfig()
        {
            var device = new NokiaDevice("SR1");
            var nvram = @"system
 name ""Core""
 exit
 vlan 100
 name ""Mgmt""
 exit";
            var parser = NvramParserFactory.GetParser(device.Vendor);
            parser.Apply(nvram, device);
            var sys = device.ProcessCommand("show system information");
            Assert.Contains("Core", sys);
            var vlan = device.ProcessCommand("show vlan");
            Assert.Contains("100", vlan);
        }

        [Fact]
        public void FortinetNvramParser_ShouldApplyConfig()
        {
            var device = new FortinetDevice("FW1");
            var nvram = @"config system interface
edit port1
set ip 10.10.10.1 255.255.255.0
next
end";
            var parser = NvramParserFactory.GetParser(device.Vendor);
            parser.Apply(nvram, device);
            var iface = device.GetInterface("port1");
            Assert.Equal("10.10.10.1", iface.IpAddress);
        }

        [Fact]
        public void MikroTikNvramParser_ShouldApplyConfig()
        {
            var device = new MikroTikDevice("MT1");
            var nvram = @"/interface vlan add vlan-id=50 interface=ether1 name=vlan50
/ip address add address=192.168.50.1/24 interface=vlan50";
            var parser = NvramParserFactory.GetParser(device.Vendor);
            parser.Apply(nvram, device);
            var vlan = device.ProcessCommand("/ping 192.168.50.1"); // loopback ping
            Assert.Contains("Success rate", vlan);
        }
    }
} 
