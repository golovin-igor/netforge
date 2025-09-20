namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Describes a device model supported by a vendor
    /// </summary>
    public class DeviceModelDescriptor
    {
        public string ModelName { get; set; } = "";
        public string ModelFamily { get; set; } = "";
        public string Description { get; set; } = "";
        public DeviceType DeviceType { get; set; }
        public IList<string> Features { get; set; } = new List<string>();
        public IDictionary<string, object> Capabilities { get; set; } = new Dictionary<string, object>();
    }
}