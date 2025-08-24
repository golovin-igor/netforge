namespace NetForge.Simulation.Common.Events
{
    public enum LinkChangeType
    {
        Added,
        Removed
    }

    public class LinkChangedEventArgs : NetworkEventArgs
    {
        public string Device1Name { get; }
        public string Interface1Name { get; }
        public string Device2Name { get; }
        public string Interface2Name { get; }
        public LinkChangeType ChangeType { get; }

        public LinkChangedEventArgs(string device1Name, string interface1Name, string device2Name, string interface2Name, LinkChangeType changeType)
        {
            Device1Name = device1Name;
            Interface1Name = interface1Name;
            Device2Name = device2Name;
            Interface2Name = interface2Name;
            ChangeType = changeType;
        }
    }
}
