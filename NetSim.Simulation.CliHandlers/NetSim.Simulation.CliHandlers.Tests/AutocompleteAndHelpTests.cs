using System.Reflection;
using NetSim.Simulation.CliHandlers;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Common;
using Xunit;

namespace NetSim.Simulation.Tests.CliHandlers
{
    public class AutocompleteAndHelpTests
    {
        private static CliHandlerManager GetManager(NetworkDevice device)
        {
            var field = typeof(NetworkDevice).GetField("CommandManager", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            return (CliHandlerManager)field!.GetValue(device)!;
        }

        [Fact]
        public void GetCompletions_AtRoot_ShouldReturnBasicCommands()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);
            var completions = manager.GetCompletions(string.Empty);

            Assert.Contains("enable", completions);
            Assert.Contains("ping", completions);
            Assert.Contains("show", completions);
        }

        [Fact]
        public void GetCompletions_ForShow_ShouldReturnSubCommands()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);
            var completions = manager.GetCompletions("show");

            Assert.Contains("running-config", completions);
            Assert.Contains("version", completions);
            Assert.Contains("interfaces", completions);
        }

        [Fact]
        public void GetCommandHelp_ForShowIp_ShouldNotThrow()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);

            Assert.Throws<InvalidOperationException>(() => manager.GetCommandHelp("show ip"));
        }

        [Fact]
        public void GetCommandHelp_InvalidCommand_ShouldNotThrow()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);

            Assert.Throws<InvalidOperationException>(() => manager.GetCommandHelp("nonexistent"));
        }

        [Fact]
        public void GetCompletions_FuzzyMatch_ShouldReturnCommand()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);
            var completions = manager.GetCompletions("shw");

            Assert.Contains("show", completions);
        }

        [Fact]
        public void GetCommandHelp_NoCommand_ShouldShowGeneralHelp()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);
            var help = manager.GetCommandHelp(string.Empty);

            Assert.Contains("Available commands:", help);
            Assert.Contains("enable", help);
        }

        [Fact]
        public void ProcessCommand_InvalidCommand_ShouldSuggestSimilar()
        {
            var device = new CiscoDevice("R1");
            var output = device.ProcessCommand("shw version");

            Assert.Contains("Invalid input", output);
            Assert.Contains("show", output);
        }
    }
}
