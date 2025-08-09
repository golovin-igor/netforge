using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers
{
    public class CommandHandlerTests
    {
        [Fact]
        public void ShowVersionHandler_ShouldReturnVersionInfo()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show version");
            
            // Assert
            Assert.Contains("Cisco IOS Software", output);
            Assert.Contains("TestRouter", output);
            Assert.Contains("TestRouter>", output); // Should have prompt at end
        }
        
        [Fact]
        public void ShowRunningConfigHandler_ShouldReturnConfig()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Enter privileged mode
            device.ProcessCommand("enable");
            Assert.Equal("privileged", (device as CiscoDevice).GetMode());
            
            // Configure terminal and change hostname
            device.ProcessCommand("configure terminal");
            device.ProcessCommand("hostname TestCisco");
            device.ProcessCommand("exit");
            
            // Verify we're in privileged mode before running show command
            var currentPrompt = device.GetPrompt();
            Assert.Equal("TestCisco#", currentPrompt); // Should be in privileged mode
            
            // Act
            var output = device.ProcessCommand("show running-config");
            
            // Assert
            Assert.Contains("hostname TestCisco", output);
            Assert.Contains("TestCisco#", output); // Should have prompt at end
        }
        
        [Fact]
        public void PingHandler_ShouldSimulatePing()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("ping 8.8.8.8");
            
            // Assert
            Assert.Contains("Network not initialized", output);
            Assert.Equal("TestRouter>", device.GetPrompt());
        }
        
        [Fact]
        public void EnableHandler_ShouldChangeModeCorrectly()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act & Assert - Start in user mode
            Assert.Equal("TestRouter>", device.GetPrompt());
            Assert.Equal("user", (device as CiscoDevice).GetMode());
            
            var output = device.ProcessCommand("enable");
            
            // Check that mode changed
            Assert.Equal("privileged", (device as CiscoDevice).GetMode());
            Assert.Equal("TestRouter#", device.GetPrompt());
            Assert.Equal("TestRouter#", output); // Output should be just the new prompt
        }
        
        [Fact]
        public void InvalidCommand_ShouldFallbackToLegacy()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("invalid command");
            
            // Assert
            Assert.Contains("Invalid input detected", output);
        }
        
        [Fact]
        public void HostnameHandler_ShouldChangeHostname()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("hostname NewName");
            
            // Assert
            Assert.Equal("NewName(config)#", output);
            
            // Verify it persists in show run
            device.ProcessCommand("exit");
            var showRun = device.ProcessCommand("show running-config");
            Assert.Contains("hostname NewName", showRun);
        }
        
        [Fact]
        public void VlanHandler_ShouldCreateVlan()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("vlan 10");
            
            // Assert
            Assert.Equal("TestRouter(config-vlan)#", output);
            
            // Configure VLAN name
            device.ProcessCommand("name TestVLAN");
            device.ProcessCommand("exit");
            device.ProcessCommand("exit");
            
            // Verify VLAN exists
            var showVlan = device.ProcessCommand("show vlan brief");
            Assert.Contains("10", showVlan);
            Assert.Contains("TestVLAN", showVlan);
        }
        
        [Fact]
        public void InterfaceHandler_ShouldConfigureInterface()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("interface GigabitEthernet0/0");
            
            // Assert
            Assert.Equal("TestRouter(config-if)#", output);
            
            // Configure IP address
            device.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            device.ProcessCommand("no shutdown");
            device.ProcessCommand("description Test Interface");
            device.ProcessCommand("exit");
            device.ProcessCommand("exit");
            
            // Verify interface configuration
            var showInt = device.ProcessCommand("show interfaces GigabitEthernet0/0");
            Assert.Contains("192.168.1.1", showInt);
            Assert.Contains("Test Interface", showInt);
        }
        
        [Fact]
        public void RouterOspfHandler_ShouldEnterOspfMode()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("router ospf 1");
            
            // Assert
            Assert.Equal("TestRouter(config-router)#", output);
            
            // Verify OSPF configuration appears in running config
            device.ProcessCommand("exit");
            device.ProcessCommand("exit");
            var showRun = device.ProcessCommand("show running-config");
            Assert.Contains("router ospf 1", showRun);
        }
        
        [Fact]
        public void IpRouteHandler_ShouldAddStaticRoute()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("ip route 10.0.0.0 255.255.255.0 192.168.1.254");
            
            // Assert
            Assert.Equal("TestRouter(config)#", output);
            
            // Verify route in running config
            device.ProcessCommand("exit");
            var showRun = device.ProcessCommand("show running-config");
            Assert.Contains("ip route 10.0.0.0 255.255.255.0 192.168.1.254", showRun);
            
            // Verify route in routing table
            var showRoute = device.ProcessCommand("show ip route");
            Assert.Contains("S    10.0.0.0/24", showRoute);
            Assert.Contains("192.168.1.254", showRoute);
        }
        
        [Fact]
        public void HelpCommand_AtRootLevel_ShouldShowAvailableCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("?");
            
            // Assert
            Assert.Contains("Available commands:", output);
            Assert.Contains("enable", output);
            Assert.Contains("show", output);
            Assert.Contains("ping", output);
        }
        
        [Fact]
        public void HelpCommand_WithPartialCommand_ShouldShowSubcommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show ?");
            
            // Assert
            Assert.Contains("running-config", output);
            Assert.Contains("version", output);
            Assert.Contains("interfaces", output);
            Assert.Contains("vlan", output);
            Assert.Contains("ip", output);
        }
        
        [Fact]
        public void HelpCommand_WithNestedCommand_ShouldShowNestedOptions()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("show ip ?");
            
            // Assert
            Assert.Contains("route", output);
            Assert.Contains("interface", output);
        }
        
        [Fact]
        public void HelpCommand_InConfigMode_ShouldShowConfigCommands()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
            
            // Act
            var output = device.ProcessCommand("?");
            
            // Assert
            Assert.Contains("hostname", output);
            Assert.Contains("interface", output);
            Assert.Contains("vlan", output);
            Assert.Contains("router", output);
            Assert.Contains("ip", output);
        }
        
        [Fact]
        public void HelpCommand_WithCompleteCommand_ShouldShowCR()
        {
            // Arrange
            var device = new CiscoDevice("TestRouter");
            
            // Act
            var output = device.ProcessCommand("enable ?");
            
            // Assert
            Assert.Contains("<cr>", output);
        }
    }
} 
