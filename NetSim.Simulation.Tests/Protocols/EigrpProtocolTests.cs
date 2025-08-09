using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;

namespace NetSim.Simulation.Tests.Protocols
{
    public class EigrpProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly EigrpProtocol _eigrpProtocol;
        private readonly List<string> _deviceLogs = new();
        private readonly Action<string> _logHandlerDelegate;

        public EigrpProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceEIGRP");
            _network.AddDeviceAsync(_testDevice).Wait();

            _eigrpProtocol = _testDevice.GetProtocolsForTesting().OfType<EigrpProtocol>().FirstOrDefault();
            Assert.NotNull(_eigrpProtocol);

            _logHandlerDelegate = log => _deviceLogs.Add(log);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            var eigrp = new EigrpConfig(1) { IsEnabled = true };
            _testDevice.SetEigrpConfiguration(eigrp);
            _deviceLogs.Clear();
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerEigrpUpdateProcessing()
        {
            string expected1 = $"EIGRPProtocol on {_testDevice.Name}: Received InterfaceStateChange";
            string expected2 = "Re-evaluating EIGRP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForEigrpOnRelevantDevice_ShouldTriggerEigrpUpdateProcessing()
        {
            _deviceLogs.Clear();
            string expected1 = $"EIGRPProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expected2 = "Re-evaluating EIGRP configuration and state";

            var newCfg = new EigrpConfig(2) { IsEnabled = true };

            _testDevice.SetEigrpConfiguration(newCfg);
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected1) && l.Contains(expected2));
        }

        [Fact]
        public async Task UpdateState_WhenEigrpDisabled_ShouldClearEigrpRoutesAndNotProcessFurther()
        {
            var cfg = _testDevice.GetEigrpConfiguration();
            Assert.NotNull(cfg);
            cfg.IsEnabled = false;
            _testDevice.SetEigrpConfiguration(cfg);
            _deviceLogs.Clear();

            _testDevice.AddRoute(new Route("10.0.0.0", "255.0.0.0", "", "", "EIGRP"));
            Assert.Contains(_testDevice.GetRoutingTable(), r => r.Protocol == "EIGRP");

            string expected = $"EIGRPProtocol on {_testDevice.Name}: EIGRP configuration missing or not enabled. Clearing EIGRP routes.";
            string unexpected = "Updating EIGRP state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(50);

            Assert.Contains(_deviceLogs, l => l.Contains(expected));
            Assert.DoesNotContain(_testDevice.GetRoutingTable(), r => r.Protocol == "EIGRP");
            Assert.DoesNotContain(_deviceLogs, l => l.Contains(unexpected));
        }

        public void Dispose()
        {
            _testDevice.LogEntryAdded -= _logHandlerDelegate;
        }
    }
}
