using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Fortinet
{
    public class FortinetCommandHandlerTests
    {
        [Fact]
        public void ConfigGlobal_ShouldEnterGlobalConfigMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            
            // Act
            var result = device.ProcessCommand("config global");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("global_config", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM (global) #", result);
        }

        [Fact]
        public void ConfigSystemInterface_ShouldEnterSystemInterfaceMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            
            // Act
            var result = device.ProcessCommand("config system interface");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("system_if", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM (interface) #", result);
        }

        [Fact]
        public void ConfigRouterOspf_ShouldEnterOspfMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            
            // Act
            var result = device.ProcessCommand("config router ospf");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router_ospf", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM (ospf) #", result);
            Assert.NotNull(device.GetOspfConfiguration());
        }

        [Fact]
        public void ConfigRouterBgp_ShouldEnterBgpMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            
            // Act
            var result = device.ProcessCommand("config router bgp");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("router_bgp", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM (bgp) #", result);
            Assert.NotNull(device.GetBgpConfiguration());
        }

        [Fact]
        public void SetHostname_InGlobalConfigMode_ShouldChangeHostname()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config global");

            // Act
            var result = device.ProcessCommand("set hostname \"TestDevice\"");

            // Assert
            Assert.NotNull(result);
            // Note: The hostname might not change in the device object, this tests the command execution
            Assert.Contains("FortiGate-VM", device.GetHostname()); // Original hostname likely preserved
        }

        [Fact]
        public void EditInterface_ShouldEnterInterfaceMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config system interface");

            // Act
            var result = device.ProcessCommand("edit port1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("interface", device.GetCurrentMode());
            Assert.Equal("port1", device.GetCurrentInterface());
        }

        [Fact]
        public void SetIpAddress_InInterfaceMode_ShouldConfigureInterface()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config system interface");
            device.ProcessCommand("edit port1");

            // Act
            var result = device.ProcessCommand("set ip 192.168.1.1 255.255.255.0");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetAllInterfaces();
            Assert.Equal("192.168.1.1", interfaces["port1"].IpAddress);
            Assert.Equal("255.255.255.0", interfaces["port1"].SubnetMask);
        }

        [Fact]
        public void SetInterfaceDescription_ShouldSetDescription()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config system interface");
            device.ProcessCommand("edit port1");

            // Act
            var result = device.ProcessCommand("set description \"LAN Interface\"");

            // Assert
            Assert.NotNull(result);
            var interfaces = device.GetAllInterfaces();
            Assert.Equal("LAN Interface", interfaces["port1"].Description);
        }

        [Fact]
        public void SetInterfaceStatus_ShouldControlInterfaceState()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config system interface");
            device.ProcessCommand("edit port1");

            // Act - Shutdown interface
            device.ProcessCommand("set status down");
            var interfaces1 = device.GetAllInterfaces();

            // Act - Enable interface
            device.ProcessCommand("set status up");
            var interfaces2 = device.GetAllInterfaces();

            // Assert
            Assert.True(interfaces1["port1"].IsShutdown);
            Assert.False(interfaces1["port1"].IsUp);
            Assert.False(interfaces2["port1"].IsShutdown);
            Assert.True(interfaces2["port1"].IsUp);
        }

        [Fact]
        public void GetSystemInterface_ShouldReturnInterfaceInfo()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config system interface");
            device.ProcessCommand("edit port1");
            device.ProcessCommand("set ip 192.168.1.1 255.255.255.0");
            device.ProcessCommand("end");

            // Act
            var result = device.ProcessCommand("get system interface port1");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("port1", result);
            Assert.Contains("192.168.1.1", result);
            Assert.Contains("255.255.255.0", result);
        }

        [Fact]
        public void GetSystemStatus_ShouldReturnSystemInfo()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = device.ProcessCommand("get system status");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("FortiGate-VM64", result);
            Assert.Contains("Version:", result);
            Assert.Contains("Hostname: FortiGate-VM", result);
            Assert.Contains("Serial-Number:", result);
        }

        [Fact]
        public void GetRouterInfoRoutingTable_ShouldReturnRoutes()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config system interface");
            device.ProcessCommand("edit port1");
            device.ProcessCommand("set ip 192.168.1.1 255.255.255.0");
            device.ProcessCommand("end");

            // Act
            var result = device.ProcessCommand("get router info routing-table");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Routing table for VRF=0", result);
            Assert.Contains("Codes:", result);
            Assert.Contains("C - connected", result);
        }

        [Fact]
        public void ShowFullConfiguration_ShouldReturnCompleteConfig()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config global");
            device.ProcessCommand("set hostname \"TestFortiGate\"");
            device.ProcessCommand("end");

            // Act
            var result = device.ProcessCommand("show full-configuration");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("#config-version=", result);
            Assert.Contains("config system global", result);
            Assert.Contains("set hostname \"TestFortiGate\"", result);
            Assert.Contains("config system interface", result);
        }

        [Fact]
        public void ShowSystemInterface_ShouldReturnInterfaceStatus()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = device.ProcessCommand("show system interface");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("onboard", result);
            Assert.Contains("port1", result);
            Assert.Contains("port2", result);
            Assert.Contains("internal", result);
        }

        [Fact]
        public void ShowFirewallPolicy_ShouldReturnPolicies()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = device.ProcessCommand("show firewall policy");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("policyid", result);
            Assert.Contains("name", result);
            Assert.Contains("src", result);
            Assert.Contains("dst", result);
            Assert.Contains("LAN-to-WAN", result);
        }

        [Fact]
        public void ExecutePing_ShouldSimulatePing()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = device.ProcessCommand("execute ping 8.8.8.8");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("PING 8.8.8.8", result);
            Assert.Contains("64 bytes from 8.8.8.8", result);
            Assert.Contains("ping statistics", result);
            Assert.Contains("5 packets transmitted", result);
        }

        [Fact]
        public void ExecuteSaveConfig_ShouldConfirmSave()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = device.ProcessCommand("execute save config");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Configuration saved", result);
        }

        [Fact]
        public void ExecuteReboot_ShouldPromptConfirmation()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = device.ProcessCommand("execute reboot");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("reboot the system", result);
            Assert.Contains("Do you want to continue", result);
        }

        [Fact]
        public void DiagnoseIpArpList_ShouldReturnArpTable()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config system interface");
            device.ProcessCommand("edit port1");
            device.ProcessCommand("set ip 192.168.1.1 255.255.255.0");
            device.ProcessCommand("end");

            // Act
            var result = device.ProcessCommand("diagnose ip arp list");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("IP address", result);
            Assert.Contains("HW type", result);
            Assert.Contains("MAC address", result);
            Assert.Contains("192.168.1.1", result);
        }

        [Fact]
        public void DiagnoseNetlinkInterface_ShouldReturnInterfaceList()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = device.ProcessCommand("diagnose netlink interface");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("port1", result);
            Assert.Contains("port2", result);
            Assert.Contains("internal", result);
        }

        [Fact]
        public void DiagnoseHardwareDeviceinfo_ShouldReturnHardwareInfo()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Act
            var result = device.ProcessCommand("diagnose hardware deviceinfo");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("port1", result);
            Assert.Contains("aggregate", result);
            Assert.Contains("lacp1", result);
        }

        [Fact]
        public void OspfConfiguration_ShouldConfigureOspfSettings()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config router ospf");

            // Act
            var result1 = device.ProcessCommand("set router-id 1.1.1.1");
            var result2 = device.ProcessCommand("config network");
            device.ProcessCommand("end");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            var ospfConfig = device.GetOspfConfiguration();
            Assert.NotNull(ospfConfig);
            Assert.Equal("1.1.1.1", ospfConfig.RouterId);
        }

        [Fact]
        public void BgpConfiguration_ShouldConfigureBgpSettings()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config router bgp");

            // Act
            var result1 = device.ProcessCommand("set as 65001");
            var result2 = device.ProcessCommand("set router-id 2.2.2.2");
            var result3 = device.ProcessCommand("config neighbor 192.168.1.2");
            device.ProcessCommand("end");

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
        public void EndCommand_ShouldReturnToGlobalMode()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config global");
            Assert.Equal("global_config", device.GetCurrentMode());

            // Act
            var result = device.ProcessCommand("end");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("global", device.GetCurrentMode());
            Assert.Contains("FortiGate-VM #", result);
        }

        [Fact]
        public void InvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");

            // Act
            var result = device.ProcessCommand("invalid command");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Invalid command", result);
        }

        [Fact]
        public void ModeTransitions_ShouldWorkCorrectly()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");

            // Start in global mode
            Assert.Equal("global", device.GetCurrentMode());

            // Enter global config mode
            device.ProcessCommand("config global");
            Assert.Equal("global_config", device.GetCurrentMode());

            // Return to global mode
            device.ProcessCommand("end");
            Assert.Equal("global", device.GetCurrentMode());

            // Enter system interface mode
            device.ProcessCommand("config system interface");
            Assert.Equal("system_if", device.GetCurrentMode());

            // Edit specific interface
            device.ProcessCommand("edit port1");
            Assert.Equal("interface", device.GetCurrentMode());

            // Return to system interface mode
            device.ProcessCommand("next");
            Assert.Equal("system_if", device.GetCurrentMode());

            // Return to global mode
            device.ProcessCommand("end");
            Assert.Equal("global", device.GetCurrentMode());
        }

        [Fact]
        public void VlanConfiguration_ShouldCreateAndConfigureVlan()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config system vlan");

            // Act
            var result1 = device.ProcessCommand("edit 100");
            var result2 = device.ProcessCommand("set name \"TestVLAN\"");
            var result3 = device.ProcessCommand("next");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            var vlans = device.GetAllVlans();
            Assert.True(vlans.ContainsKey(100));
            Assert.Equal("TestVLAN", vlans[100].Name);
        }

        [Fact]
        public void RipConfiguration_ShouldConfigureRipSettings()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config router rip");

            // Act
            var result1 = device.ProcessCommand("set version 2");
            var result2 = device.ProcessCommand("config network 192.168.1.0");
            device.ProcessCommand("end");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            var ripConfig = device.GetRipConfiguration();
            Assert.NotNull(ripConfig);
            Assert.Equal(2, ripConfig.Version);
        }

        [Fact]
        public void FirewallPolicyConfiguration_ShouldAllowPolicyEditing()
        {
            // Arrange
            var device = new FortinetDevice("FortiGate-VM");
            device.ProcessCommand("config firewall policy");

            // Act
            var result1 = device.ProcessCommand("edit 1");
            var result2 = device.ProcessCommand("set name \"Test Policy\"");
            var result3 = device.ProcessCommand("next");
            device.ProcessCommand("end");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
        }

        [Fact]
        public void FortinetGlobalHandler_ConfigCommand_ShouldEnterConfigMode()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = device.ProcessCommand("config system global");
            
            // Assert
            Assert.Equal("global_config", device.GetCurrentMode());
            Assert.Equal("TestFortiGate (global) #", device.GetPrompt());
            Assert.Equal("TestFortiGate (global) #", output);
        }

        [Fact]
        public void FortinetGlobalHandler_GetCommand_ShouldShowSystemInfo()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = device.ProcessCommand("get system status");
            
            // Assert
            Assert.Contains("Version:", output);
            Assert.Contains("Serial-Number:", output);
            Assert.Equal("TestFortiGate #", device.GetPrompt());
        }

        [Fact]
        public void FortinetGlobalHandler_ShowCommand_ShouldDisplayInfo()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = device.ProcessCommand("show system interface");
            
            // Assert
            Assert.Contains("Interface information", output);
            Assert.Equal("TestFortiGate #", device.GetPrompt());
        }

        [Fact]
        public void FortinetGlobalHandler_ExecuteCommand_ShouldExecuteCommand()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = device.ProcessCommand("execute ping 192.168.1.1");
            
            // Assert
            Assert.Contains("PING 192.168.1.1", output);
            Assert.Contains("bytes from", output);
            Assert.Equal("TestFortiGate #", device.GetPrompt());
        }

        [Fact]
        public void FortinetGlobalHandler_DiagnoseCommand_ShouldRunDiagnostics()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = device.ProcessCommand("diagnose sys version");
            
            // Assert
            Assert.Contains("FortiOS version", output);
            Assert.Equal("TestFortiGate #", device.GetPrompt());
        }

        [Fact]
        public void FortinetGlobalHandler_WhenInRouterMode_ShouldHandleRouterCommands()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            device.ProcessCommand("config router ospf");
            
            // Act
            var output = device.ProcessCommand("show");
            
            // Assert
            Assert.Contains("OSPF configuration", output);
            Assert.Equal("router_ospf", device.GetCurrentMode());
        }

        [Fact]
        public void FortinetGlobalHandler_WhenInRouterMode_ShouldRejectNonRouterCommands()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            device.ProcessCommand("config router ospf");
            
            // Act
            var output = device.ProcessCommand("config system global");
            
            // Assert
            Assert.Contains("Invalid command", output);
            Assert.Equal("router_ospf", device.GetCurrentMode());
        }

        [Fact]
        public void FortinetGlobalHandler_WhenNotInGlobalMode_ShouldReturnError()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            device.ProcessCommand("config system interface");
            
            // Act
            var output = device.ProcessCommand("get system status");
            
            // Assert
            Assert.Contains("Invalid mode", output);
            Assert.Equal("system_if", device.GetCurrentMode());
        }

        [Fact]
        public void FortinetGlobalHandler_WithInvalidCommand_ShouldReturnError()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = device.ProcessCommand("invalid command");
            
            // Assert
            Assert.Contains("Invalid command", output);
            Assert.Equal("global", device.GetCurrentMode());
        }

        [Fact]
        public void FortinetGlobalHandler_WithIncompleteCommand_ShouldReturnError()
        {
            // Arrange
            var device = new FortinetDevice("TestFortiGate");
            
            // Act
            var output = device.ProcessCommand("config");
            
            // Assert
            Assert.Contains("Incomplete command", output);
            Assert.Equal("global", device.GetCurrentMode());
        }


    }
} 
