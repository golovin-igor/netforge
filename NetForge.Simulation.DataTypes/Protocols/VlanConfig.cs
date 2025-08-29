namespace NetForge.Simulation.Common.Configuration
{
    /// <summary>
    /// VLAN configuration
    /// </summary>
    public class VlanConfig(int id, string name)
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public bool IsActive { get; set; } = true;
        public bool Active { get; set; } = true;
        public List<string> TaggedPorts { get; set; } = new();
        public List<string> UntaggedPorts { get; set; } = new();
        public List<string> Interfaces { get; set; } = new();

        public VlanConfig(int id) : this(id, $"VLAN{id}")
        {
        }
    }
}
