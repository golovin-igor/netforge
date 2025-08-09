using NetSim.Simulation.Common;
using NetSim.Simulation.Interfaces;

namespace NetSim.Simulation.Tests.TestUtilities
{
    public static class NetworkDeviceTestExtensions
    {
        public static IEnumerable<INetworkProtocol> GetProtocolsForTesting(this NetworkDevice device)
        {
            var fieldInfo = typeof(NetworkDevice).GetField("_protocols",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                return (IEnumerable<INetworkProtocol>)fieldInfo.GetValue(device);
            }
            return new List<INetworkProtocol>();
        }
    }
}
