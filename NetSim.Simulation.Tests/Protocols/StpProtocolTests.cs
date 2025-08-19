using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Configuration;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    /// <summary>
    /// LEGACY TESTS: STP protocol tests for old implementation in NetSim.Simulation.Common.
    /// These tests are for the legacy STP implementation that will be migrated to the new protocol architecture.
    /// Once STP is migrated to NetSim.Simulation.Protocols.STP, create new tests in NetSim.Simulation.Protocols.Tests.
    /// </summary>
    [Trait("Category", "Legacy")]
    public class StpProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly StpProtocol _stpProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public StpProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceSTP");
            _network.AddDeviceAsync(_testDevice).Wait();

            _stpProtocol = _testDevice.GetProtocolsForTesting().OfType<StpProtocol>().FirstOrDefault();
            Assert.NotNull(_stpProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var stp = new StpConfig { IsEnabled = true };
            _testDevice.SetStpConfiguration(stp);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerStpUpdateProcessing()
        {
            string expected1 = $"StpProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating STP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            _deviceLogs.Clear();
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForStpOnRelevantDevice_ShouldTriggerStpUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"StpProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating STP configuration and state";

            var newCfg = new StpConfig { IsEnabled = true };
            _testDevice.SetStpConfiguration(newCfg);
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenStpDisabled_ShouldLogAndSkipProcessing()
        {
            var cfg = _testDevice.GetStpConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetStpConfiguration(cfg);
            await Task.Delay(100);
            _deviceLogs.Clear();

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            string expected = $"StpProtocol on {_testDevice.Name}: STP configuration missing or not enabled.";
            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_deviceLogs, l => l.Contains("Updating STP state"));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
