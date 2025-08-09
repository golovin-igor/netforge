namespace NetSim.Simulation.Protocols.Routing
{
    /// <summary>
    /// Represents the runtime state of an HSRP protocol instance
    /// </summary>
    public class HsrpState
    {
        /// <summary>
        /// Dictionary of HSRP group states indexed by group ID
        /// </summary>
        public Dictionary<int, HsrpGroupState> GroupStates { get; set; } = new();
        
        /// <summary>
        /// Track if any group state has changed
        /// </summary>
        public bool StateChanged { get; set; } = true;
        
        /// <summary>
        /// Last time state was updated
        /// </summary>
        public DateTime LastStateUpdate { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Mark state as changed
        /// </summary>
        public void MarkStateChanged()
        {
            StateChanged = true;
        }
        
        /// <summary>
        /// Get or create HSRP group state
        /// </summary>
        public HsrpGroupState GetOrCreateGroupState(int groupId, string interfaceName)
        {
            if (!GroupStates.ContainsKey(groupId))
            {
                GroupStates[groupId] = new HsrpGroupState(groupId, interfaceName);
            }
            return GroupStates[groupId];
        }
        
        /// <summary>
        /// Remove HSRP group state
        /// </summary>
        public void RemoveGroupState(int groupId)
        {
            if (GroupStates.Remove(groupId))
            {
                MarkStateChanged();
            }
        }
        
        /// <summary>
        /// Check for groups that need hello messages
        /// </summary>
        public List<int> GetGroupsNeedingHello()
        {
            var groupsNeedingHello = new List<int>();
            var now = DateTime.Now;
            
            foreach (var kvp in GroupStates)
            {
                var groupState = kvp.Value;
                if (groupState.IsActive && 
                    (now - groupState.LastHelloSent).TotalSeconds >= groupState.HelloInterval)
                {
                    groupsNeedingHello.Add(kvp.Key);
                }
            }
            
            return groupsNeedingHello;
        }
        
        /// <summary>
        /// Check for groups that have timed out
        /// </summary>
        public List<int> GetTimedOutGroups()
        {
            var timedOutGroups = new List<int>();
            var now = DateTime.Now;
            
            foreach (var kvp in GroupStates)
            {
                var groupState = kvp.Value;
                if (groupState.IsActive && 
                    (now - groupState.LastHelloReceived).TotalSeconds > groupState.HoldTime)
                {
                    timedOutGroups.Add(kvp.Key);
                }
            }
            
            return timedOutGroups;
        }
    }
    
    /// <summary>
    /// Represents the state of an HSRP group
    /// </summary>
    public class HsrpGroupState
    {
        public int GroupId { get; set; }
        public string InterfaceName { get; set; }
        public HsrpGroupStateType State { get; set; } = HsrpGroupStateType.Initial;
        public DateTime StateChangeTime { get; set; } = DateTime.Now;
        public string ActiveRouter { get; set; } = string.Empty;
        public string StandbyRouter { get; set; } = string.Empty;
        public string VirtualIp { get; set; } = string.Empty;
        public int Priority { get; set; } = 100;
        public bool Preempt { get; set; } = false;
        public int HelloInterval { get; set; } = 3;
        public int HoldTime { get; set; } = 10;
        public DateTime LastHelloSent { get; set; } = DateTime.MinValue;
        public DateTime LastHelloReceived { get; set; } = DateTime.MinValue;
        public bool IsActive { get; set; } = true;
        public bool IsVirtualIpOwner { get; set; } = false;
        
        public HsrpGroupState(int groupId, string interfaceName)
        {
            GroupId = groupId;
            InterfaceName = interfaceName;
        }
        
        public void ChangeState(HsrpGroupStateType newState)
        {
            if (State != newState)
            {
                State = newState;
                StateChangeTime = DateTime.Now;
            }
        }
        
        public TimeSpan GetTimeInCurrentState()
        {
            return DateTime.Now - StateChangeTime;
        }
        
        public bool ShouldSendHello()
        {
            return (DateTime.Now - LastHelloSent).TotalSeconds >= HelloInterval;
        }
        
        public bool HasTimedOut()
        {
            return (DateTime.Now - LastHelloReceived).TotalSeconds > HoldTime;
        }
    }
    
    /// <summary>
    /// HSRP group states
    /// </summary>
    public enum HsrpGroupStateType
    {
        Initial,
        Learn,
        Listen,
        Speak,
        Standby,
        Active
    }
} 
