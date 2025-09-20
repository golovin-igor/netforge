using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Describes a protocol supported by a vendor
    /// </summary>
    public class ProtocolDescriptor
    {
        public NetworkProtocolType ProtocolType { get; set; }
        public string ImplementationClass { get; set; } = "";
        public string AssemblyName { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 0;
        public IDictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
        public IList<string> RequiredFeatures { get; set; } = new List<string>();
    }
}