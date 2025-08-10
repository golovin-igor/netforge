using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers
{
    public class CommandHandlerTests
    {
        [Fact]
        public async Task ShowVersionHandlerShouldReturnVersionInfo()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show version");
            
            // Assert
            Assert.Contains("Cisco IOS Software", output);
            Assert.Contains("TestRouter", output);
            Assert.Contains("TestRouter>", output); // Should have prompt at end
        }
        
        [Fact]
        public async Task ShowRunningConfigHandlerShouldReturnConfig()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Enter privileged mode
            await device.ProcessCommandAsync("enable");
            Assert.Equal("privileged", (device as CiscoDevice).GetMode());
            
            // Configure terminal and change hostname
            await device.ProcessCommandAsync("configure terminal");
            await device.ProcessCommandAsync("hostname TestCisco");
            await device.ProcessCommandAsync("exit");
            
            // Verify we're in privileged mode before running show command
            var currentPrompt = device.GetPrompt();
            Assert.Equal("TestCisco#", currentPrompt); // Should be in privileged mode
            
            // Act
            var output = await device.ProcessCommandAsync("show running-config");
            
            // Assert
            Assert.Contains("hostname TestCisco", output);
            Assert.Contains("TestCisco#", output); // Should have prompt at end
        }
        
        [Fact]
        public async Task PingHandlerShouldSimulatePing()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("ping 8.8.8.8");
            
            // Assert
            Assert.Contains("Network not initialized", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }
        
        [Fact]
        public async Task EnableHandlerShouldChangeModeCorrectly()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act & Assert - Start in user mode
            Assert.Equal("TestRouter>", device.GetPrompt());
            Assert.Equal("user", (device as CiscoDevice).GetMode());
            
            var output = await device.ProcessCommandAsync("enable");
            
            // Check that mode changed
            Assert.Equal("privileged", (device as CiscoDevice).GetMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output); // Output should be just the new prompt
        }
        
        [Fact]
        public async Task InvalidCommandShouldFallbackToLegacy()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("invalid command");
            
            // Assert
            Assert.Contains("Invalid input detected", output);
        }
        
        [Fact]
        public async Task HostnameHandlerShouldChangeHostname()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("hostname NewName");
            
            // Assert
            Assert.Equal("NewName(config)#", output);
            
            // Verify it persists in show run
            await device.ProcessCommandAsync("exit");
            var showRun = await device.ProcessCommandAsync("show running-config");
            Assert.Contains("hostname NewName", showRun);
        }
        
        [Fact]
        public async Task VlanHandlerShouldCreateVlan()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("vlan 10");
            
            // Assert
            Assert.Equal("TestRouter(config-vlan)#", output);
            
            // Configure VLAN name
            await device.ProcessCommandAsync("name TestVLAN");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("exit");
            
            // Verify VLAN exists
            var showVlan = await device.ProcessCommandAsync("show vlan brief");
            Assert.Contains("10", showVlan);
            Assert.Contains("TestVLAN", showVlan);
        }
        
        [Fact]
        public async Task InterfaceHandlerShouldConfigureInterface()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("interface GigabitEthernet0/0");
            
            // Assert
            Assert.Equal("TestRouter(config-if)#", output);
            
            // Configure IP address
            await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await device.ProcessCommandAsync("no shutdown");
            await device.ProcessCommandAsync("description Test Interface");
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("exit");
            
            // Verify interface configuration
            var showInt = await device.ProcessCommandAsync("show interfaces GigabitEthernet0/0");
            Assert.Contains("192.168.1.1", showInt);
            Assert.Contains("Test Interface", showInt);
        }
        
        [Fact]
        public async Task RouterOspfHandlerShouldEnterOspfMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("router ospf 1");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", output);
            
            // Verify OSPF configuration appears in running config
            await device.ProcessCommandAsync("exit");
            await device.ProcessCommandAsync("exit");
            var showRun = await device.ProcessCommandAsync("show running-config");
            Assert.Contains("router ospf 1", showRun);
        }
        
        [Fact]
        public async Task IpRouteHandlerShouldAddStaticRoute()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("ip route 10.0.0.0 255.255.255.0 192.168.1.254");
            
            // Assert
            Assert.Equal("TestRouter(config)#", output);
            
            // Verify route in running config
            await device.ProcessCommandAsync("exit");
            var showRun = await device.ProcessCommandAsync("show running-config");
            Assert.Contains("ip route 10.0.0.0 255.255.255.0 192.168.1.254", showRun);
            
            // Verify route in routing table
            var showRoute = await device.ProcessCommandAsync("show ip route");
            Assert.Contains("S    10.0.0.0/24", showRoute);
            Assert.Contains("192.168.1.254", showRoute);
        }
        
        [Fact]
        public async Task HelpCommandAtRootLevelShouldShowAvailableCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("?");
            
            // Assert
            Assert.Contains("Available commands:", output);
            Assert.Contains("enable", output);
            Assert.Contains("show", output);
            Assert.Contains("ping", output);
        }
        
        [Fact]
        public async Task HelpCommandWithPartialCommandShouldShowSubcommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show ?");
            
            // Assert
            Assert.Contains("running-config", output);
            Assert.Contains("version", output);
            Assert.Contains("interfaces", output);
            Assert.Contains("vlan", output);
            Assert.Contains("ip", output);
        }
        
        [Fact]
        public async Task HelpCommandWithNestedCommandShouldShowNestedOptions()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("show ip ?");
            
            // Assert
            Assert.Contains("route", output);
            Assert.Contains("interface", output);
        }
        
        [Fact]
        public async Task HelpCommandInConfigModeShouldShowConfigCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
            
            // Act
            var output = await device.ProcessCommandAsync("?");
            
            // Assert
            Assert.Contains("hostname", output);
            Assert.Contains("interface", output);
            Assert.Contains("vlan", output);
            Assert.Contains("router", output);
            Assert.Contains("ip", output);
        }
        
        [Fact]
        public async Task HelpCommandWithCompleteCommandShouldShowCR()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = await device.ProcessCommandAsync("enable ?");
            
            // Assert
            Assert.Contains("<cr>", output);
        }
    }
} 
