using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;
using NetSim.Simulation.Events;
using NetSim.Simulation.Protocols.Implementations;
using NetSim.Simulation.Protocols.Routing;
using NetSim.Simulation.Tests.TestUtilities;
using Xunit;
// For OspfConfig

// For List<string> in log checking

namespace NetSim.Simulation.Tests.Protocols
{
    public class OspfProtocolTests : IDisposable
    {
        private readonly Network _network;
        private readonly NetworkEventBus _eventBus;
        private readonly CiscoDevice _testDevice;
        private readonly OspfProtocol _ospfProtocol; 
        private List<string> _deviceLogs; // To capture logs for assertion
        private Action<string> _logHandlerDelegate; // To store the delegate for unsubscribing

        public OspfProtocolTests()
        {
            _network = new Network();
            _eventBus = _network.EventBus;
            _testDevice = new CiscoDevice("TestDeviceOSPF"); 
            _network.AddDeviceAsync(_testDevice).Wait(); 

            _ospfProtocol = _testDevice.GetProtocolsForTesting().OfType<OspfProtocol>().FirstOrDefault();
            Assert.NotNull(_ospfProtocol); // Ensure OSPF protocol is found

            // Setup log capture for the device
            _deviceLogs = new List<string>();
            _logHandlerDelegate = (logEntry) => _deviceLogs.Add(logEntry);
            _testDevice.LogEntryAdded += _logHandlerDelegate;

            // Default OSPF config for tests that need it active
            var ospfConfig = new OspfConfig(1);
            ospfConfig.IsEnabled = true;
            _testDevice.SetOspfConfiguration(ospfConfig); 
            _deviceLogs.Clear(); // Clear logs after initial setup config events
        }

        [Fact]
        public async Task InterfaceStateChange_OnRelevantDevice_ShouldTriggerOspfUpdateProcessing()
        {
            // Arrange
            string expectedLogSubstring = $"OSPFProtocol on {_testDevice.Name}: Received InterfaceStateChange for GigabitEthernet0/0";
            string expectedLogSubstring2 = "Re-evaluating OSPF state";

            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            Assert.NotNull(iface);

            // Clear logs before triggering the event
            _deviceLogs.Clear();

            // Act
            iface.IsUp = !iface.IsUp; // Toggle to trigger event
            await Task.Delay(200); // Allow more time for event handling

            // Assert
            Assert.Contains(_deviceLogs, log => log.Contains(expectedLogSubstring) && log.Contains(expectedLogSubstring2));
        }

        [Fact]
        public async Task ProtocolConfigChange_ForOspfOnRelevantDevice_ShouldTriggerOspfUpdateProcessing()
        {
            // Arrange
            // Clear initial config logs from constructor
            _deviceLogs.Clear();
            string expectedLogSubstring = $"OSPFProtocol on {_testDevice.Name}: Received ProtocolConfigChange";
            string expectedLogSubstring2 = "Re-evaluating OSPF configuration and state";

            var newOspfConfig = new OspfConfig(2); 
            newOspfConfig.IsEnabled = true;

            // Act
            _testDevice.SetOspfConfiguration(newOspfConfig); 
            await Task.Delay(200); // Allow more time for event handling

            // Assert
            Assert.Contains(_deviceLogs, log => log.Contains(expectedLogSubstring) && log.Contains(expectedLogSubstring2));
        }
        
        [Fact]
        public async Task UpdateState_WhenOspfDisabled_ShouldClearOspfRoutesAndNotProcessFurther()
        {
            // Arrange
            var ospfConfig = _testDevice.GetOspfConfiguration();
            Assert.NotNull(ospfConfig); // Should be configured by constructor
            ospfConfig.IsEnabled = false;
            _testDevice.SetOspfConfiguration(ospfConfig); // Publish the change
            
            // Wait for OSPF disable to be processed
            await Task.Delay(100);
            _deviceLogs.Clear(); // Clear logs from SetOspfConfiguration event handling

            _testDevice.AddRoute(new Route("10.0.0.0", "255.0.0.0", "", "", "OSPF")); // Add a dummy OSPF route
            Assert.Contains(_testDevice.GetRoutingTable(), r => r.Protocol == "OSPF");

            string expectedLogForDisabled = $"OSPFProtocol ({ospfConfig.ProcessId}): Not enabled, clearing OSPF routes and skipping update.";
            string unexpectedLogForProcessing = $"Simulating neighbor discovery on interfaces";

            // Act: Directly call UpdateState or trigger via an event that OSPF protocol handles
            // For this test, let's trigger it via an interface change event, as it would normally occur
            var iface = _testDevice.GetInterface("GigabitEthernet0/0");
            iface.IsUp = !iface.IsUp;
            await Task.Delay(200); // Allow more time for event handling

            // Assert
            Assert.Contains(_deviceLogs, log => log.Contains(expectedLogForDisabled));
            Assert.DoesNotContain(_testDevice.GetRoutingTable(), r => r.Protocol == "OSPF");
            Assert.DoesNotContain(_deviceLogs, log => log.Contains(unexpectedLogForProcessing));
        }

        public void Dispose()
        {
            // Unsubscribe from log event to prevent issues between tests
            if (_testDevice != null && _logHandlerDelegate != null)
            {
                _testDevice.LogEntryAdded -= _logHandlerDelegate;
            }
        }
    }

} 