using NetForge.Simulation.Common.Events;
using NetForge.Simulation.DataTypes.Events;

namespace NetForge.Simulation.Common.Events
{
    public enum LinkChangeType
    {
        Added,
        Removed
    }

    public class LinkChangedEventArgs(string device1Name, string interface1Name, string device2Name, string interface2Name, LinkChangeType changeType)
        : NetworkEventArgs
    {
        public string Device1Name { get; } = device1Name;
        public string Interface1Name { get; } = interface1Name;
        public string Device2Name { get; } = device2Name;
        public string Interface2Name { get; } = interface2Name;
        public LinkChangeType ChangeType { get; } = changeType;
    }
}
