namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents the runtime state of a VRRP protocol instance
    /// </summary>
    public class VrrpState
    {
        /// <summary>
        /// Dictionary of VRRP group states indexed by VRID
        /// </summary>
        public Dictionary<int, VrrpGroupState> GroupStates { get; set; } = new();

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
        /// Get or create VRRP group state
        /// </summary>
        public VrrpGroupState GetOrCreateGroupState(int vrid, string interfaceName)
        {
            if (!GroupStates.ContainsKey(vrid))
            {
                GroupStates[vrid] = new VrrpGroupState(vrid, interfaceName);
            }
            return GroupStates[vrid];
        }

        /// <summary>
        /// Remove VRRP group state
        /// </summary>
        public void RemoveGroupState(int vrid)
        {
            if (GroupStates.Remove(vrid))
            {
                MarkStateChanged();
            }
        }

        /// <summary>
        /// Check for groups that need advertisement messages
        /// </summary>
        public List<int> GetGroupsNeedingAdvertisement()
        {
            var groupsNeedingAdvertisement = new List<int>();
            var now = DateTime.Now;

            foreach (var kvp in GroupStates)
            {
                var groupState = kvp.Value;
                if (groupState.State == VrrpGroupStateType.Master &&
                    (now - groupState.LastAdvertisementSent).TotalSeconds >= groupState.AdvertisementInterval)
                {
                    groupsNeedingAdvertisement.Add(kvp.Key);
                }
            }

            return groupsNeedingAdvertisement;
        }

        /// <summary>
        /// Check for groups that have master timeout
        /// </summary>
        public List<int> GetGroupsWithMasterTimeout()
        {
            var groupsWithTimeout = new List<int>();
            var now = DateTime.Now;

            foreach (var kvp in GroupStates)
            {
                var groupState = kvp.Value;
                if (groupState.State == VrrpGroupStateType.Backup &&
                    (now - groupState.LastAdvertisementReceived).TotalSeconds > groupState.MasterDownInterval)
                {
                    groupsWithTimeout.Add(kvp.Key);
                }
            }

            return groupsWithTimeout;
        }
    }

    /// <summary>
    /// Represents the state of a VRRP group
    /// </summary>
    public class VrrpGroupState
    {
        public int Vrid { get; set; }
        public string InterfaceName { get; set; }
        public VrrpGroupStateType State { get; set; } = VrrpGroupStateType.Initialize;
        public DateTime StateChangeTime { get; set; } = DateTime.Now;
        public string MasterRouter { get; set; } = string.Empty;
        public List<string> VirtualIps { get; set; } = new();
        public int Priority { get; set; } = 100;
        public bool Preempt { get; set; } = true;
        public int AdvertisementInterval { get; set; } = 1;
        public int MasterDownInterval { get; set; } = 3;
        public DateTime LastAdvertisementSent { get; set; } = DateTime.MinValue;
        public DateTime LastAdvertisementReceived { get; set; } = DateTime.MinValue;
        public bool IsActive { get; set; } = true;
        public bool IsVirtualIpOwner { get; set; } = false;

        public VrrpGroupState(int vrid, string interfaceName)
        {
            Vrid = vrid;
            InterfaceName = interfaceName;
        }

        public void ChangeState(VrrpGroupStateType newState)
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

        public bool ShouldSendAdvertisement()
        {
            return State == VrrpGroupStateType.Master &&
                   (DateTime.Now - LastAdvertisementSent).TotalSeconds >= AdvertisementInterval;
        }

        public bool HasMasterTimedOut()
        {
            return State == VrrpGroupStateType.Backup &&
                   (DateTime.Now - LastAdvertisementReceived).TotalSeconds > MasterDownInterval;
        }
    }

    /// <summary>
    /// VRRP group states
    /// </summary>
    public enum VrrpGroupStateType
    {
        Initialize,
        Backup,
        Master
    }
}
