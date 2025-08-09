using System.Threading.Tasks;
using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Cisco
{
    public class CiscoDeviceDebugTests
    {
        [Fact]
        public async Task Debug_IpAddressAssignment()
        {
            // Create a simple network with two devices
            var network = new Network();
            var r1 = new CiscoDevice("R1");
            var r2 = new CiscoDevice("R2");
            
            await network.AddDeviceAsync(r1);
            await network.AddDeviceAsync(r2);
            await network.AddLinkAsync("R1", "GigabitEthernet0/0", "R2", "GigabitEthernet0/0");

            // Configure R1 with detailed debugging
            System.Console.WriteLine("=== Starting R1 Configuration ===");
            
            var result1 = r1.ProcessCommand("enable");
            System.Console.WriteLine($"enable result: '{result1}'");
            
            var result2 = r1.ProcessCommand("configure terminal");
            System.Console.WriteLine($"configure terminal result: '{result2}'");
            
            var result3 = r1.ProcessCommand("interface GigabitEthernet0/0");
            System.Console.WriteLine($"interface GigabitEthernet0/0 result: '{result3}'");
            
            // Check mode after interface command
            System.Console.WriteLine($"Current mode after interface command: {r1.GetModeEnum()}");
            System.Console.WriteLine($"Current interface: {r1.GetCurrentInterface()}");
            
            var result4 = r1.ProcessCommand("ip address 10.0.0.1 255.255.255.252");
            System.Console.WriteLine($"ip address command result: '{result4}'");
            
            // Check interface after IP command
            var r1Interface = r1.GetInterface("GigabitEthernet0/0");
            System.Console.WriteLine($"Interface after IP command - IP: {r1Interface?.IpAddress}, Mask: {r1Interface?.SubnetMask}");
            
            var result5 = r1.ProcessCommand("no shutdown");
            System.Console.WriteLine($"no shutdown result: '{result5}'");
            
            var result6 = r1.ProcessCommand("exit");
            System.Console.WriteLine($"exit from interface result: '{result6}'");
            
            var result7 = r1.ProcessCommand("exit");
            System.Console.WriteLine($"exit from config result: '{result7}'");

            // Configure R2
            r2.ProcessCommand("enable");
            r2.ProcessCommand("configure terminal");
            r2.ProcessCommand("interface GigabitEthernet0/0");
            r2.ProcessCommand("ip address 10.0.0.2 255.255.255.252");
            r2.ProcessCommand("no shutdown");
            r2.ProcessCommand("exit");
            r2.ProcessCommand("exit");

            // Debug: Check if IP addresses are actually set
            var r2Interface = r2.GetInterface("GigabitEthernet0/0");
            
            Assert.NotNull(r1Interface);
            Assert.NotNull(r2Interface);
            
            // Debug output
            System.Console.WriteLine($"R1 Interface IP: {r1Interface.IpAddress}");
            System.Console.WriteLine($"R2 Interface IP: {r2Interface.IpAddress}");
            
            // The issue is here - IP addresses are not being set
            if (r1Interface.IpAddress == null)
            {
                System.Console.WriteLine("ERROR: R1 IP address is null!");
                // Let's try again with more debugging
                r1.ProcessCommand("configure terminal");
                r1.ProcessCommand("interface GigabitEthernet0/0");
                var ipResult = r1.ProcessCommand("ip address 10.0.0.1 255.255.255.252");
                System.Console.WriteLine($"Second attempt IP command result: '{ipResult}'");
                System.Console.WriteLine($"Interface after second attempt: {r1Interface.IpAddress}");
                r1.ProcessCommand("exit");
                r1.ProcessCommand("exit");
            }
            
            Assert.Equal("10.0.0.1", r1Interface.IpAddress);
            Assert.Equal("10.0.0.2", r2Interface.IpAddress);
        }
        
        [Fact]
        public void Debug_CommandHandlerRegistration()
        {
            var device = new CiscoDevice("TEST");
            
            // Test if basic commands work
            var enableResult = device.ProcessCommand("enable");
            System.Console.WriteLine($"Enable result: '{enableResult}'");
            
            var configResult = device.ProcessCommand("configure terminal");
            System.Console.WriteLine($"Config result: '{configResult}'");
            
            var interfaceResult = device.ProcessCommand("interface GigabitEthernet0/0");
            System.Console.WriteLine($"Interface result: '{interfaceResult}'");
            
            // Check if we're in the right mode
            System.Console.WriteLine($"Current mode: {device.GetModeEnum()}");
            System.Console.WriteLine($"Current interface: {device.GetCurrentInterface()}");
            
            // Test IP command
            var ipResult = device.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            System.Console.WriteLine($"IP command result: '{ipResult}'");
            
            // Check if IP was set
            var iface = device.GetInterface("GigabitEthernet0/0");
            System.Console.WriteLine($"Interface IP after command: {iface?.IpAddress}");
            
            Assert.NotNull(iface);
            Assert.Equal("192.168.1.1", iface.IpAddress);
        }
    }
} 
