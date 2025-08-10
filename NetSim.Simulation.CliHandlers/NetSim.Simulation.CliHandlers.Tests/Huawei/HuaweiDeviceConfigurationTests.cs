using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.Huawei
{
    public class HuaweiDeviceConfigurationTests
    {
        [Fact]
        public async Task HuaweiSystemViewShouldEnterConfigMode()
        {
            var device = new HuaweiDevice("SW1");
            var output = await device.ProcessCommandAsync("system-view");
            Assert.Contains("[SW1]", output);

            output = await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            Assert.Contains("[SW1-GigabitEthernet0/0/1]", output);

            output = await device.ProcessCommandAsync("quit");
            Assert.Contains("[SW1]", output);

            output = await device.ProcessCommandAsync("vlan 10");
            Assert.Contains("[SW1-vlan10]", output);
        }

        [Fact]
        public async Task HuaweiInterfaceConfigurationShouldApplySettings()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("description Server Connection");
            await device.ProcessCommandAsync("ip address 192.168.1.1 255.255.255.0");
            await device.ProcessCommandAsync("speed 1000");
            await device.ProcessCommandAsync("duplex full");
            await device.ProcessCommandAsync("undo shutdown");
            await device.ProcessCommandAsync("quit");
            await device.ProcessCommandAsync("interface Vlanif10");
            await device.ProcessCommandAsync("ip address 10.10.10.1 255.255.255.0");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display current-configuration");
            Assert.Contains("interface GigabitEthernet0/0/1", output);
            Assert.Contains("description Server Connection", output);
            Assert.Contains("ip address 192.168.1.1 255.255.255.0", output);
            Assert.Contains("interface Vlanif10", output);
        }

        [Fact]
        public async Task HuaweiShutdownUndoShutdownShouldToggleInterface()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("ip address 10.0.0.1 255.255.255.0");
            await device.ProcessCommandAsync("undo shutdown");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display interface brief");
            Assert.Contains("GigabitEthernet0/0/1", output);
            Assert.Contains("up", output);

            await device.ProcessCommandAsync("interface GigabitEthernet0/0/1");
            await device.ProcessCommandAsync("shutdown");
            await device.ProcessCommandAsync("quit");

            output = await device.ProcessCommandAsync("display interface brief");
            Assert.Contains("down", output);
        }

        [Fact]
        public async Task HuaweiSaveShouldPersistConfiguration()
        {
            var device = new HuaweiDevice("SW1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("sysname Huawei-Test");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("save");
            Assert.Contains("Are you sure to continue?", output);

            output = await device.ProcessCommandAsync("y");
            Assert.Contains("successfully", output);
        }

        [Fact]
        public async Task HuaweiAclConfigurationShouldCreateAccessList()
        {
            var device = new HuaweiDevice("R1");
            await device.ProcessCommandAsync("system-view");
            await device.ProcessCommandAsync("acl 2001");
            await device.ProcessCommandAsync("rule 5 deny source 192.168.1.0 0.0.0.255");
            await device.ProcessCommandAsync("rule 10 permit");
            await device.ProcessCommandAsync("quit");

            var output = await device.ProcessCommandAsync("display acl 2001");
            Assert.Contains("Basic ACL 2001", output);
            Assert.Contains("rule 5 deny", output);
            Assert.Contains("192.168.1.0", output);
            Assert.Contains("rule 10 permit", output);
        }

        [Fact]
        public async Task HuaweiRebootShouldPromptConfirmation()
        {
            var device = new HuaweiDevice("SW1");
            var output = await device.ProcessCommandAsync("reboot");

            Assert.Contains("Continue?", output);
            Assert.Contains("[Y/N]", output);
        }
    }
}

