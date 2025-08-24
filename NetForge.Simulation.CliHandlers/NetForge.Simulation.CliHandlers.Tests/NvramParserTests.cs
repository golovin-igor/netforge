using NetForge.Simulation.Common.Configuration;
using NetForge.Simulation.Core;
using NetForge.Simulation.Core.Devices;

namespace NetForge.Simulation.CliHandlers.Tests
{
    public class NvramParserTests
    {
        [Fact]
        public async Task CiscoNvramParserShouldApplyConfig()
        {
            var device = new CiscoDevice("R1");
            var nvram = """
                        interface GigabitEthernet0/0
                         ip address 192.168.1.1 255.255.255.0
                         no shutdown
                         exit
                         vlan 10
                         name SALES
                        """;

            var parser = NvramParserFactory.GetParser(device.Vendor);
            await parser.Apply(nvram, device);

            var iface = device.GetInterface("GigabitEthernet0/0");
            Assert.Equal("192.168.1.1", iface?.IpAddress);
            var vlanOut = await device.ProcessCommandAsync("show vlan brief");
            Assert.Contains("10", vlanOut);
            Assert.Contains("SALES", vlanOut);
        }

        [Fact]
        public async Task JuniperNvramParserShouldApplyConfig()
        {
            var device = new JuniperDevice("R1");
            var nvram = """
                        set interfaces ge-0/0/0 unit 0 family inet address 10.0.0.1/24
                        set routing-options router-id 1.1.1.1
                        set protocols ospf area 0.0.0.0 interface ge-0/0/0
                        """;

            var parser = NvramParserFactory.GetParser(device.Vendor);
            await parser.Apply(nvram, device);

            await device.ProcessCommandAsync("show ospf neighbor");
            // No neighbor yet, but OSPF should be configured
            Assert.Contains("ge-0/0/0", await device.ProcessCommandAsync("show ospf interface"));
            var iface = device.GetInterface("ge-0/0/0");
            Assert.Equal("10.0.0.1", iface?.IpAddress);
        }

        [Fact]
        public async Task HuaweiNvramParserShouldApplyConfig()
        {
            var device = new HuaweiDevice("SW1");
            var nvram = """vlan 20""";
            var parser = NvramParserFactory.GetParser(device.Vendor);
            await parser.Apply(nvram, device);
            var outp = await device.ProcessCommandAsync("display vlan");
            Assert.Contains("20", outp);
        }

        [Fact]
        public async Task AristaNvramParserShouldApplyConfig()
        {
            var device = new AristaDevice("SW1");
            var nvram = """
                        vlan 30
                        name DATA
                        interface Ethernet1
                        no switchport
                        ip address 172.16.0.1/24
                        """;
            var parser = NvramParserFactory.GetParser(device.Vendor);
            await parser.Apply(nvram, device);
            var vlanOut = await device.ProcessCommandAsync("show vlan");
            Assert.Contains("30", vlanOut);
            Assert.Contains("DATA", vlanOut);
            Assert.Contains("Ethernet1", await device.ProcessCommandAsync("show ip interface brief"));
        }

        [Fact]
        public async Task NokiaNvramParserShouldApplyConfig()
        {
            var device = new NokiaDevice("SR1");
            var nvram = """
                        system
                         name "Core"
                         exit
                         vlan 100
                         name "Mgmt"
                         exit
                        """;
            var parser = NvramParserFactory.GetParser(device.Vendor);
            await parser.Apply(nvram, device);
            var sys = await device.ProcessCommandAsync("show system information");
            Assert.Contains("Core", sys);
            var vlan = await device.ProcessCommandAsync("show vlan");
            Assert.Contains("100", vlan);
        }

        [Fact]
        public async Task FortinetNvramParserShouldApplyConfig()
        {
            var device = new FortinetDevice("FW1");
            var nvram = """
                        config system interface
                        edit port1
                        set ip 10.10.10.1 255.255.255.0
                        next
                        end
                        """;
            var parser = NvramParserFactory.GetParser(device.Vendor);
            await parser.Apply(nvram, device);
            var iface = device.GetInterface("port1");
            Assert.Equal("10.10.10.1", iface?.IpAddress);
        }

        [Fact]
        public async Task MikroTikNvramParserShouldApplyConfig()
        {
            var device = new MikroTikDevice("MT1");
            var nvram = """
                        /interface vlan add vlan-id=50 interface=ether1 name=vlan50
                        /ip address add address=192.168.50.1/24 interface=vlan50
                        """;
            var parser = NvramParserFactory.GetParser(device.Vendor);
            await parser.Apply(nvram, device);
            var vlan = await device.ProcessCommandAsync("/ping 192.168.50.1"); // loopback ping
            Assert.Contains("Success rate", vlan);
        }
    }
}
