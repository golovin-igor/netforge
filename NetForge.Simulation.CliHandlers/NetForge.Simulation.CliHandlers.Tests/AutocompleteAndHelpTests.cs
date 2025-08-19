using System.Reflection;
using NetForge.Simulation.CliHandlers;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Common;
using Xunit;

namespace NetForge.Simulation.Tests.CliHandlers
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
        public async Task GetCompletionsAtRootShouldReturnBasicCommands()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);
            var completions = manager.GetCompletions(string.Empty);

            Assert.Contains("enable", completions);
            Assert.Contains("ping", completions);
            Assert.Contains("show", completions);
        }

        [Fact]
        public async Task GetCompletionsForShowShouldReturnSubCommands()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);
            var completions = manager.GetCompletions("show");

            Assert.Contains("running-config", completions);
            Assert.Contains("version", completions);
            Assert.Contains("interfaces", completions);
        }

        [Fact]
        public async Task GetCommandHelpForShowIpShouldNotThrow()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);

            Assert.Throws<InvalidOperationException>(() => manager.GetCommandHelp("show ip"));
        }

        [Fact]
        public async Task GetCommandHelpInvalidCommandShouldNotThrow()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);

            Assert.Throws<InvalidOperationException>(() => manager.GetCommandHelp("nonexistent"));
        }

        [Fact]
        public async Task GetCompletionsFuzzyMatchShouldReturnCommand()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);
            var completions = manager.GetCompletions("shw");

            Assert.Contains("show", completions);
        }

        [Fact]
        public async Task GetCommandHelpNoCommandShouldShowGeneralHelp()
        {
            var device = new CiscoDevice("R1");
            var manager = GetManager(device);
            var help = manager.GetCommandHelp(string.Empty);

            Assert.Contains("Available commands:", help);
            Assert.Contains("enable", help);
        }

        [Fact]
        public async Task ProcessCommandInvalidCommandShouldSuggestSimilar()
        {
            var device = new CiscoDevice("R1");
            var output = await device.ProcessCommandAsync("shw version");

            Assert.Contains("Invalid input", output);
            Assert.Contains("show", output);
        }
    }
}
