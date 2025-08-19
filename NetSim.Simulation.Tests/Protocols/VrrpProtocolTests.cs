using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    /// <summary>
    /// LEGACY TESTS: VRRP protocol tests for old implementation in NetSim.Simulation.Common.
    /// These tests are for the legacy VRRP implementation that will be migrated to the new protocol architecture.
    /// Once VRRP is migrated to NetSim.Simulation.Protocols.VRRP, create new tests in NetSim.Simulation.Protocols.Tests.
    /// </summary>
    [Trait("Category", "Legacy")]
    public class VrrpProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly VrrpProtocol _vrrpProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public VrrpProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceVRRP");
            _network.AddDeviceAsync(_testDevice).Wait();

            _vrrpProtocol = _testDevice.GetProtocolsForTesting().OfType<VrrpProtocol>().FirstOrDefault();
            Assert.NotNull(_vrrpProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var cfg = new VrrpConfig();
            cfg.AddGroup(1, "10.0.0.1", 110, "GigabitEthernet0/0");
            cfg.IsEnabled = true;
            _testDevice.SetVrrpConfiguration(cfg);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerVrrpUpdateProcessing()
        {
            string expected1 = $"VrrpProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating VRRP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            _deviceLogs.Clear();
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForVrrpOnRelevantDevice_ShouldTriggerVrrpUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"VrrpProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating VRRP configuration and state";

            var newCfg = new VrrpConfig();
            newCfg.AddGroup(2, "10.0.0.2", 120, "GigabitEthernet0/0");
            newCfg.IsEnabled = true;
            _testDevice.SetVrrpConfiguration(newCfg);
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenVrrpDisabled_ShouldLogAndSkipProcessing()
        {
            var cfg = _testDevice.GetVrrpConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetVrrpConfiguration(cfg);
            await Task.Delay(100);
            _deviceLogs.Clear();

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            string expected = $"VrrpProtocol on {_testDevice.Name}: VRRP configuration missing or no VRRP groups configured.";
            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_deviceLogs, l => l.Contains("Updating VRRP state"));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
