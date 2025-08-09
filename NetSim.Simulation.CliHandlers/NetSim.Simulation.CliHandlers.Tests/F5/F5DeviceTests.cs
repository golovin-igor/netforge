using NetSim.Simulation.Devices;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers.F5
{
    public class F5DeviceTests
    {
        [Fact]
        public void Enable_Configure_Hostname_ShouldChangePrompt()
        {
            var device = new F5Device("F5-1");

            device.ProcessCommand("enable");
            Assert.Equal("privileged", device.GetCurrentMode());
            Assert.Equal("F5-1#", device.GetPrompt());

            device.ProcessCommand("configure");
            Assert.Equal("config", device.GetCurrentMode());
            Assert.Equal("F5-1(config)#", device.GetPrompt());

            device.ProcessCommand("hostname LABF5");
            Assert.Equal("LABF5(config)#", device.GetPrompt());
        }

        [Fact]
        public void DisableCommand_ShouldExitPrivilegedMode()
        {
            var device = new F5Device("F5");
            device.ProcessCommand("enable");
            Assert.Equal("privileged", device.GetCurrentMode());

            device.ProcessCommand("disable");
            Assert.Equal("user", device.GetCurrentMode());
            Assert.Equal("F5>", device.GetPrompt());
        }

        [Fact]
        public void ShowVersion_ShouldDisplaySystemInfo()
        {
            var device = new F5Device("F5");
            var output = device.ProcessCommand("show version");

            Assert.Contains("F5 BIG-IP System", output);
            Assert.Contains("Version", output);
        }

        [Fact]
        public void ShowLtmPool_ShouldDisplayPoolInformation()
        {
            var device = new F5Device("F5");
            var output = device.ProcessCommand("show ltm pool");

            Assert.Contains("LTM Pool Information", output);
            Assert.Contains("web_pool", output);
        }

        [Fact]
        public void PingCommand_ShouldReturnPingStatistics()
        {
            var device = new F5Device("F5");
            var output = device.ProcessCommand("ping 8.8.8.8");

            Assert.Contains("PING 8.8.8.8", output);
            Assert.Contains("5 packets transmitted", output);
        }
    }
}
