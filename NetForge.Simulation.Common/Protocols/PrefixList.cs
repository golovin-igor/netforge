namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents a prefix list configuration
    /// </summary>
    public class PrefixList
    {
        public string Name { get; set; }
        public List<string> Entries { get; set; } = new List<string>();
        public Dictionary<int, PrefixListEntry> SequencedEntries { get; set; } = new Dictionary<int, PrefixListEntry>();
        public List<string> Prefixes { get; set; } = new List<string>();

        public PrefixList(string name)
        {
            Name = name;
        }
    }

    public class PrefixListEntry
    {
        public int SequenceNumber { get; set; }
        public string Action { get; set; } = "permit";
        public string Prefix { get; set; }
        public int? GreaterEqual { get; set; }
        public int? LessEqual { get; set; }

        public PrefixListEntry(int sequenceNumber, string action, string prefix)
        {
            SequenceNumber = sequenceNumber;
            Action = action;
            Prefix = prefix;
        }
    }
}
