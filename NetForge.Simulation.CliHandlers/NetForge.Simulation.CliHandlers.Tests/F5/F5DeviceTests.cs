using NetForge.Simulation.Core.Devices;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers.F5
{
    public class F5DeviceTests
    {
        [Fact]
        public async Task EnableConfigureHostnameShouldChangePrompt()
        {
            var device = new F5Device("F5-1");

            await device.ProcessCommandAsync("enable");
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("F5-1#", device.GetPrompt());

            await device.ProcessCommandAsync("configure");
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("F5-1(config)#", device.GetPrompt());

            await device.ProcessCommandAsync("hostname LABF5");
            Assert.Equal("LABF5(config)#", device.GetPrompt());
        }

        [Fact]
        public async Task DisableCommandShouldExitPrivilegedMode()
        {
            var device = new F5Device("F5");
            await device.ProcessCommandAsync("enable");
            Assert.Equal("privileged", device.GetCurrentMode());

            await device.ProcessCommandAsync("disable");
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("F5>", device.GetPrompt());
        }

        [Fact]
        public async Task ShowVersionShouldDisplaySystemInfo()
        {
            var device = new F5Device("F5");
            var output = await device.ProcessCommandAsync("show version");

            Assert.Contains("F5 BIG-IP System", output);
            Assert.Contains("Version", output);
        }

        [Fact]
        public async Task ShowLtmPoolShouldDisplayPoolInformation()
        {
            var device = new F5Device("F5");
            var output = await device.ProcessCommandAsync("show ltm pool");

            Assert.Contains("LTM Pool Information", output);
            Assert.Contains("web_pool", output);
        }

        [Fact]
        public async Task PingCommandShouldReturnPingStatistics()
        {
            var device = new F5Device("F5");
            var output = await device.ProcessCommandAsync("ping 8.8.8.8");

            Assert.Contains("PING 8.8.8.8", output);
            Assert.Contains("5 packets transmitted", output);
        }
    }
}
