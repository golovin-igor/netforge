namespace NetForge.Simulation.Common.Security
{
    /// <summary>
    /// Represents an Access Control List (ACL)
    /// </summary>
    public class AccessList
    {
        public int Number { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<AclEntry> Entries { get; set; } = new List<AclEntry>();

        public AccessList(int number)
        {
            Number = number;
        }

        public AccessList(string name)
        {
            Name = name;
        }
    }
}
