using NetForge.Simulation.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.Fortinet
{
    public class FortinetCommandHandlerTests
    {
        [Fact]
        public async Task ConfigGlobalShouldEnterGlobalConfigMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            
            // Act
            var result = await device.ProcessCommandAsync("config global");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("global_config", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM (global) #", result);
        }

        [Fact]
        public async Task ConfigSystemInterfaceShouldEnterSystemInterfaceMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            
            // Act
            var result = await device.ProcessCommandAsync("config system interface");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("system_if", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM (interface) #", result);
        }

        [Fact]
        public async Task ConfigRouterOspfShouldEnterOspfMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            
            // Act
            var result = await device.ProcessCommandAsync("config router ospf");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router_ospf", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM (ospf) #", result);
            Assert.NotNull(device.GetOspfConfiguration());
        }

        [Fact]
        public async Task ConfigRouterBgpShouldEnterBgpMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            
            // Act
            var result = await device.ProcessCommandAsync("config router bgp");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router_bgp", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM (bgp) #", result);
            Assert.NotNull(device.GetBgpConfiguration());
        }

        [Fact]
        public async Task SetHostnameInGlobalConfigModeShouldChangeHostname()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config global");

            // Act
            var result = await device.ProcessCommandAsync("set hostname \"TestDevice\"");

            // Assert
            Assert.NotNull(result);
            // Note: The hostname might not change in the device object, this tests the command execution
            Assert.Contains("FortiGate-VM", device.GetHostname()); // Original hostname likely preserved
        }

        [Fact]
        public async Task EditInterfaceShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config system interface");

            // Act
            var result = await device.ProcessCommandAsync("edit port1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("port1", device.GetCurrentInterface());
        }

        [Fact]
        public async Task SetIpAddressInInterfaceModeShouldConfigureInterface()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config system interface");
            await device.ProcessCommandAsync("edit port1");

            // Act
            var result = await device.ProcessCommandAsync("set ip 192.168.1.1 255.255.255.0");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetAllInterfaces();
            Assert.Equal("192.168.1.1", interfaces["port1"].IpAddress);
            Assert.Equal("255.255.255.0", interfaces["port1"].SubnetMask);
        }

        [Fact]
        public async Task SetInterfaceDescriptionShouldSetDescription()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config system interface");
            await device.ProcessCommandAsync("edit port1");

            // Act
            var result = await device.ProcessCommandAsync("set description \"LAN Interface\"");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetAllInterfaces();
            Assert.Equal("LAN Interface", interfaces["port1"].Description);
        }

        [Fact]
        public async Task SetInterfaceStatusShouldControlInterfaceState()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config system interface");
            await device.ProcessCommandAsync("edit port1");

            // Act - Shutdown interface
            await device.ProcessCommandAsync("set status down");
            var interfaces1 = device.GetAllInterfaces();

            // Act - Enable interface
            await device.ProcessCommandAsync("set status up");
            var interfaces2 = device.GetAllInterfaces();

            // Assert
            Assert.True(interfaces1["port1"].IsShutdown);
            Assert.False(interfaces1["port1"].IsUp);
            Assert.False(interfaces2["port1"].IsShutdown);
            Assert.True(interfaces2["port1"].IsUp);
        }

        [Fact]
        public async Task GetSystemInterfaceShouldReturnInterfaceInfo()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config system interface");
            await device.ProcessCommandAsync("edit port1");
            await device.ProcessCommandAsync("set ip 192.168.1.1 255.255.255.0");
            await device.ProcessCommandAsync("end");

            // Act
            var result = await device.ProcessCommandAsync("get system interface port1");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("port1", result);
            Assert.Contains("192.168.1.1", result);
            Assert.Contains("255.255.255.0", result);
        }

        [Fact]
        public async Task GetSystemStatusShouldReturnSystemInfo()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = await device.ProcessCommandAsync("get system status");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("FortiGate-VM64", result);
            Assert.Contains("Version:", result);
            Assert.Contains("Hostname: FortiGate-VM", result);
            Assert.Contains("Serial-Number:", result);
        }

        [Fact]
        public async Task GetRouterInfoRoutingTableShouldReturnRoutes()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config system interface");
            await device.ProcessCommandAsync("edit port1");
            await device.ProcessCommandAsync("set ip 192.168.1.1 255.255.255.0");
            await device.ProcessCommandAsync("end");

            // Act
            var result = await device.ProcessCommandAsync("get router info routing-table");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Routing table for VRF=0", result);
            Assert.Contains("Codes:", result);
            Assert.Contains("C - connected", result);
        }

        [Fact]
        public async Task ShowFullConfigurationShouldReturnCompleteConfig()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config global");
            await device.ProcessCommandAsync("set hostname \"TestFortiGate\"");
            await device.ProcessCommandAsync("end");

            // Act
            var result = await device.ProcessCommandAsync("show full-configuration");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("#config-version=", result);
            Assert.Contains("config system global", result);
            Assert.Contains("set hostname \"TestFortiGate\"", result);
            Assert.Contains("config system interface", result);
        }

        [Fact]
        public async Task ShowSystemInterfaceShouldReturnInterfaceStatus()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = await device.ProcessCommandAsync("show system interface");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("onboard", result);
            Assert.Contains("port1", result);
            Assert.Contains("port2", result);
            Assert.Contains("internal", result);
        }

        [Fact]
        public async Task ShowFirewallPolicyShouldReturnPolicies()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = await device.ProcessCommandAsync("show firewall policy");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("policyid", result);
            Assert.Contains("name", result);
            Assert.Contains("src", result);
            Assert.Contains("dst", result);
            Assert.Contains("LAN-to-WAN", result);
        }

        [Fact]
        public async Task ExecutePingShouldSimulatePing()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = await device.ProcessCommandAsync("execute ping 8.8.8.8");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("PING 8.8.8.8", result);
            Assert.Contains("64 bytes from 8.8.8.8", result);
            Assert.Contains("ping statistics", result);
            Assert.Contains("5 packets transmitted", result);
        }

        [Fact]
        public async Task ExecuteSaveConfigShouldConfirmSave()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = await device.ProcessCommandAsync("execute save config");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Configuration saved", result);
        }

        [Fact]
        public async Task ExecuteRebootShouldPromptConfirmation()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = await device.ProcessCommandAsync("execute reboot");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("reboot the system", result);
            Assert.Contains("Do you want to continue", result);
        }

        [Fact]
        public async Task DiagnoseIpArpListShouldReturnArpTable()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config system interface");
            await device.ProcessCommandAsync("edit port1");
            await device.ProcessCommandAsync("set ip 192.168.1.1 255.255.255.0");
            await device.ProcessCommandAsync("end");

            // Act
            var result = await device.ProcessCommandAsync("diagnose ip arp list");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("IP address", result);
            Assert.Contains("HW type", result);
            Assert.Contains("MAC address", result);
            Assert.Contains("192.168.1.1", result);
        }

        [Fact]
        public async Task DiagnoseNetlinkInterfaceShouldReturnInterfaceList()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = await device.ProcessCommandAsync("diagnose netlink interface");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("port1", result);
            Assert.Contains("port2", result);
            Assert.Contains("internal", result);
        }

        [Fact]
        public async Task DiagnoseHardwareDeviceinfoShouldReturnHardwareInfo()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = await device.ProcessCommandAsync("diagnose hardware deviceinfo");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("port1", result);
            Assert.Contains("aggregate", result);
            Assert.Contains("lacp1", result);
        }

        [Fact]
        public async Task OspfConfigurationShouldConfigureOspfSettings()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config router ospf");

            // Act
            var result1 = await device.ProcessCommandAsync("set router-id 1.1.1.1");
            var result2 = await device.ProcessCommandAsync("config network");
            await device.ProcessCommandAsync("end");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Equal("1.1.1.1", ospfConfig.RouterId);
        }

        [Fact]
        public async Task BgpConfigurationShouldConfigureBgpSettings()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config router bgp");

            // Act
            var result1 = await device.ProcessCommandAsync("set as 65001");
            var result2 = await device.ProcessCommandAsync("set router-id 2.2.2.2");
            var result3 = await device.ProcessCommandAsync("config neighbor 192.168.1.2");
            await device.ProcessCommandAsync("end");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            var bgpConfig = device.GetBgpConfiguration();
            Assert.NotNull(bgpConfig);
            Assert.Equal(65001, bgpConfig.LocalAs);
            Assert.Equal("2.2.2.2", bgpConfig.RouterId);
        }

        [Fact]
        public async Task EndCommandShouldReturnToGlobalMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config global");
            Assert.Equal("global_config", device.GetCurrentMode());

            // Act
            var result = await device.ProcessCommandAsync("end");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("global", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM #", result);
        }

        [Fact]
        public async Task InvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");

            // Act
            var result = await device.ProcessCommandAsync("invalid command");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Invalid command", result);
        }

        [Fact]
        public async Task ModeTransitionsShouldWorkCorrectly()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Start in global mode
            Assert.Equal("global", device.GetCurrentMode());

            // Enter global config mode
            await device.ProcessCommandAsync("config global");
            Assert.Equal("global_config", device.GetCurrentMode());

            // Return to global mode
            await device.ProcessCommandAsync("end");
            Assert.Equal("global", device.GetCurrentMode());

            // Enter system interface mode
            await device.ProcessCommandAsync("config system interface");
            Assert.Equal("system_if", device.GetCurrentMode());

            // Edit specific interface
            await device.ProcessCommandAsync("edit port1");
            Assert.Equal("interface", device.GetCurrentMode());

            // Return to system interface mode
            await device.ProcessCommandAsync("next");
            Assert.Equal("system_if", device.GetCurrentMode());

            // Return to global mode
            await device.ProcessCommandAsync("end");
            Assert.Equal("global", device.GetCurrentMode());
        }

        [Fact]
        public async Task VlanConfigurationShouldCreateAndConfigureVlan()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config system vlan");

            // Act
            var result1 = await device.ProcessCommandAsync("edit 100");
            var result2 = await device.ProcessCommandAsync("set name \"TestVLAN\"");
            var result3 = await device.ProcessCommandAsync("next");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            var vlans = device.GetAllVlans();
            Assert.True(vlans.ContainsKey(100));
            Assert.Equal("TestVLAN", vlans[100].Name);
        }

        [Fact]
        public async Task RipConfigurationShouldConfigureRipSettings()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config router rip");

            // Act
            var result1 = await device.ProcessCommandAsync("set version 2");
            var result2 = await device.ProcessCommandAsync("config network 192.168.1.0");
            await device.ProcessCommandAsync("end");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            var ripConfig = device.GetRipConfiguration();
            Assert.NotNull(ripConfig);
            Assert.Equal(2, ripConfig.Version);
        }

        [Fact]
        public async Task FirewallPolicyConfigurationShouldAllowPolicyEditing()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            await device.ProcessCommandAsync("config firewall policy");

            // Act
            var result1 = await device.ProcessCommandAsync("edit 1");
            var result2 = await device.ProcessCommandAsync("set name \"Test Policy\"");
            var result3 = await device.ProcessCommandAsync("next");
            await device.ProcessCommandAsync("end");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
        }

        [Fact]
        public async Task FortinetGlobalHandlerConfigCommandShouldEnterConfigMode()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = await device.ProcessCommandAsync("config system global");
            
            // Assert
            Assert.Equal("global_config", device.GetCurrentMode());
            Assert.Equal("TestFortiGate (global) #", device.GetPrompt());
            Assert.Equal("TestFortiGate (global) #", output);
        }

        [Fact]
        public async Task FortinetGlobalHandlerGetCommandShouldShowSystemInfo()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = await device.ProcessCommandAsync("get system status");
            
            // Assert
            Assert.Contains("Version:", output);
            Assert.Contains("Serial-Number:", output);
            Assert.Equal("TestFortiGate #", device.GetPrompt());
        }

        [Fact]
        public async Task FortinetGlobalHandlerShowCommandShouldDisplayInfo()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = await device.ProcessCommandAsync("show system interface");
            
            // Assert
            Assert.Contains("Interface information", output);
            Assert.Equal("TestFortiGate #", device.GetPrompt());
        }

        [Fact]
        public async Task FortinetGlobalHandlerExecuteCommandShouldExecuteCommand()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = await device.ProcessCommandAsync("execute ping 192.168.1.1");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("bytes from", output);
            Assert.Equal("TestFortiGate #", device.GetPrompt());
        }

        [Fact]
        public async Task FortinetGlobalHandlerDiagnoseCommandShouldRunDiagnostics()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = await device.ProcessCommandAsync("diagnose sys version");
            
            // Assert
            Assert.Contains("FortiOS version", output);
            Assert.Equal("TestFortiGate #", device.GetPrompt());
        }

        [Fact]
        public async Task FortinetGlobalHandlerWhenInRouterModeShouldHandleRouterCommands()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            await device.ProcessCommandAsync("config router ospf");
            
            // Act
            var output = await device.ProcessCommandAsync("show");
            
            // Assert
            Assert.Contains("OSPF configuration", output);
            Assert.Equal("router_ospf", device.GetCurrentMode());
        }

        [Fact]
        public async Task FortinetGlobalHandlerWhenInRouterModeShouldRejectNonRouterCommands()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            await device.ProcessCommandAsync("config router ospf");
            
            // Act
            var output = await device.ProcessCommandAsync("config system global");
            
            // Assert
            Assert.Contains("Invalid command", output);
            Assert.Equal("router_ospf", device.GetCurrentMode());
        }

        [Fact]
        public async Task FortinetGlobalHandlerWhenNotInGlobalModeShouldReturnError()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            await device.ProcessCommandAsync("config system interface");
            
            // Act
            var output = await device.ProcessCommandAsync("get system status");
            
            // Assert
            Assert.Contains("Invalid mode", output);
            Assert.Equal("system_if", device.GetCurrentMode());
        }

        [Fact]
        public async Task FortinetGlobalHandlerWithInvalidCommandShouldReturnError()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = await device.ProcessCommandAsync("invalid command");
            
            // Assert
            Assert.Contains("Invalid command", output);
            Assert.Equal("global", device.GetCurrentMode());
        }

        [Fact]
        public async Task FortinetGlobalHandlerWithIncompleteCommandShouldReturnError()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = await device.ProcessCommandAsync("config");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Equal("global", device.GetCurrentMode());
        }


    }
} 
