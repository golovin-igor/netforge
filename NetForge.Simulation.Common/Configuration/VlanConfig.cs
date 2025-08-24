namespace NetForge.Simulation.Common.Configuration
{
    /// <summary>
    /// VLAN configuration
    /// </summary>
    public class VlanConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool Active { get; set; }
        public List<string> TaggedPorts { get; set; }
        public List<string> UntaggedPorts { get; set; }
        public List<string> Interfaces { get; set; }

        public VlanConfig(int id, string name)
        {
            Id = id;
            Name = name;
            IsActive = true;
            Active = true;
            TaggedPorts = new List<string>();
            UntaggedPorts = new List<string>();
            Interfaces = new List<string>();
        }

        public VlanConfig(int id) : this(id, $"VLAN{id}")
        {
        }
    }
}
