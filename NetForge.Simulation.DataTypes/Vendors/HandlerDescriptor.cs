namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Describes a CLI handler for a vendor
    /// </summary>
    public class HandlerDescriptor
    {
        public string HandlerName { get; set; } = "";
        public string CommandPattern { get; set; } = "";
        public string ImplementationClass { get; set; } = "";
        public string AssemblyName { get; set; } = "";
        public HandlerType Type { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 0;
        public IList<string> RequiredModes { get; set; } = new List<string>();
    }
}