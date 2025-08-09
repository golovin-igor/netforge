using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Huawei
{
    public class HuaweiDeviceConfigurationTests
    {
        [Fact]
        public void Huawei_SystemView_ShouldEnterConfigMode()
        {
            var device = new HuaweiDevice("SW1");
            var output = device.ProcessCommand("system-view");
            Assert.Contains("[SW1]", output);

            output = device.ProcessCommand("interface GigabitEthernet0/0/1");
            Assert.Contains("[SW1-GigabitEthernet0/0/1]", output);

            output = device.ProcessCommand("quit");
            Assert.Contains("[SW1]", output);

            output = device.ProcessCommand("vlan 10");
            Assert.Contains("[SW1-vlan10]", output);
        }

        [Fact]
        public void Huawei_InterfaceConfiguration_ShouldApplySettings()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("description Server Connection");
            device.ProcessCommand("ip address 192.168.1.1 255.255.255.0");
            device.ProcessCommand("speed 1000");
            device.ProcessCommand("duplex full");
            device.ProcessCommand("undo shutdown");
            device.ProcessCommand("quit");
            device.ProcessCommand("interface Vlanif10");
            device.ProcessCommand("ip address 10.10.10.1 255.255.255.0");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display current-configuration");
            Assert.Contains("interface GigabitEthernet0/0/1", output);
            Assert.Contains("description Server Connection", output);
            Assert.Contains("ip address 192.168.1.1 255.255.255.0", output);
            Assert.Contains("interface Vlanif10", output);
        }

        [Fact]
        public void Huawei_ShutdownUndoShutdown_ShouldToggleInterface()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("ip address 10.0.0.1 255.255.255.0");
            device.ProcessCommand("undo shutdown");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display interface brief");
            Assert.Contains("GigabitEthernet0/0/1", output);
            Assert.Contains("up", output);

            device.ProcessCommand("interface GigabitEthernet0/0/1");
            device.ProcessCommand("shutdown");
            device.ProcessCommand("quit");

            output = device.ProcessCommand("display interface brief");
            Assert.Contains("down", output);
        }

        [Fact]
        public void Huawei_Save_ShouldPersistConfiguration()
        {
            var device = new HuaweiDevice("SW1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("sysname Huawei-Test");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("save");
            Assert.Contains("Are you sure to continue?", output);

            output = device.ProcessCommand("y");
            Assert.Contains("successfully", output);
        }

        [Fact]
        public void Huawei_AclConfiguration_ShouldCreateAccessList()
        {
            var device = new HuaweiDevice("R1");
            device.ProcessCommand("system-view");
            device.ProcessCommand("acl 2001");
            device.ProcessCommand("rule 5 deny source 192.168.1.0 0.0.0.255");
            device.ProcessCommand("rule 10 permit");
            device.ProcessCommand("quit");

            var output = device.ProcessCommand("display acl 2001");
            Assert.Contains("Basic ACL 2001", output);
            Assert.Contains("rule 5 deny", output);
            Assert.Contains("192.168.1.0", output);
            Assert.Contains("rule 10 permit", output);
        }

        [Fact]
        public void Huawei_Reboot_ShouldPromptConfirmation()
        {
            var device = new HuaweiDevice("SW1");
            var output = device.ProcessCommand("reboot");

            Assert.Contains("Continue?", output);
            Assert.Contains("[Y/N]", output);
        }
    }
}

