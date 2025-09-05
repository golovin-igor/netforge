using NetForge.Interfaces.Devices;
using NetForge.Simulation.Topology.Devices;

namespace NetForge.Simulation.Tests.TestUtilities
{
    public static class NetworkDeviceTestExtensions
    {
        public static IEnumerable<IDeviceProtocol> GetProtocolsForTesting(this NetworkDevice device)
        {
            var fieldInfo = typeof(NetworkDevice).GetField("_protocols",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                return (IEnumerable<IDeviceProtocol>)fieldInfo.GetValue(device);
            }
            return new List<IDeviceProtocol>();
        }
    }
}
