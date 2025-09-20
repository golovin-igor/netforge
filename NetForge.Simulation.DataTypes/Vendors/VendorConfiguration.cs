namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Vendor-specific configuration
    /// </summary>
    public class VendorConfiguration
    {
        public string DefaultPrompt { get; set; } = ">";
        public string EnabledPrompt { get; set; } = "#";
        public string ConfigPrompt { get; set; } = "(config)#";
        public IDictionary<string, string> PromptModes { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
    }
}