using NetForge.Simulation.Common;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;

namespace NetForge.Simulation.Tests.TestUtilities
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
