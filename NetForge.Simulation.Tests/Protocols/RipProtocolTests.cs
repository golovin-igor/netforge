using NetForge.Simulation.Common;
using NetForge.Simulation.Devices;
using NetForge.Simulation.Events;
using NetForge.Simulation.Protocols.Implementations;
using NetForge.Simulation.Protocols.Routing;
using NetForge.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetForge.Simulation.Tests.Protocols
{
    /// <summary>
    /// LEGACY TESTS: RIP protocol tests for old implementation in NetForge.Simulation.Common.
    /// These tests are for the legacy RIP implementation that will be migrated to the new protocol architecture.
    /// Once RIP is migrated to NetForge.Simulation.Protocols.RIP, create new tests in NetForge.Simulation.Protocols.Tests.
    /// </summary>
    [Trait("Category", "Legacy")]
    public class RipProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly RipProtocol _ripProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public RipProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceRIP");
            _network.AddDeviceAsync(_testDevice).Wait();

            _ripProtocol = _testDevice.GetProtocolsForTesting().OfType<RipProtocol>().FirstOrDefault();
            Assert.NotNull(_ripProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var rip = new RipConfig(2) { IsEnabled = true };
            _testDevice.SetRipConfiguration(rip);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerRipUpdateProcessing()
        {
            string expected1 = $"RIPProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating RIP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForRipOnRelevantDevice_ShouldTriggerRipUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"RIPProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating RIP configuration and state";

            var newCfg = new RipConfig(2) { IsEnabled = true };

            _testDevice.SetRipConfiguration(newCfg);
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenRipDisabled_ShouldClearRipRoutesAndNotProcessFurther()
        {
            var cfg = _testDevice.GetRipConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetRipConfiguration(cfg);
            _deviceLogs.Clear();

            _testDevice.AddRoute(new Route("10.0.0.0", "255.0.0.0", "", "", "RIP"));
            Assert.Contains(_testDevice.GetRoutingTable(), r => r.Protocol == "RIP");

            string expected = $"RIPProtocol on {_testDevice.Name}: RIP configuration missing or not enabled. Clearing RIP routes.";
            string unexpected = "Updating RIP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_testDevice.GetRoutingTable(), r => r.Protocol == "RIP");
            Assert.DoesNotContain(_deviceLogs, l => l.Contains(unexpected));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
