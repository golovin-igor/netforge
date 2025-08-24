namespace NetForge.Simulation.Common.Protocols
{
    /// <summary>
    /// Represents the runtime state of an STP protocol instance
    /// </summary>
    public class StpState
    {
        /// <summary>
        /// Dictionary of STP port states indexed by port name
        /// </summary>
        public Dictionary<string, StpPortState> PortStates { get; set; } = new();

        /// <summary>
        /// Track if topology has changed
        /// </summary>
        public bool TopologyChanged { get; set; } = true;

        /// <summary>
        /// Last time topology change was detected
        /// </summary>
        public DateTime LastTopologyChange { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Current root bridge ID
        /// </summary>
        public string RootBridgeId { get; set; } = string.Empty;

        /// <summary>
        /// Current root path cost
        /// </summary>
        public int RootPathCost { get; set; } = int.MaxValue;

        /// <summary>
        /// Current bridge priority
        /// </summary>
        public int BridgePriority { get; set; } = 32768;

        /// <summary>
        /// Current bridge ID
        /// </summary>
        public string BridgeId { get; set; } = string.Empty;

        /// <summary>
        /// Mark topology as changed
        /// </summary>
        public void MarkTopologyChanged()
        {
            TopologyChanged = true;
            LastTopologyChange = DateTime.Now;
        }

        /// <summary>
        /// Get or create STP port state
        /// </summary>
        public StpPortState GetOrCreatePortState(string portName)
        {
            if (!PortStates.ContainsKey(portName))
            {
                PortStates[portName] = new StpPortState(portName);
            }
            return PortStates[portName];
        }

        /// <summary>
        /// Remove STP port state
        /// </summary>
        public void RemovePortState(string portName)
        {
            if (PortStates.Remove(portName))
            {
                MarkTopologyChanged();
            }
        }

        /// <summary>
        /// Check for ports that need BPDU transmission
        /// </summary>
        public List<string> GetPortsNeedingBpdu()
        {
            var portsNeedingBpdu = new List<string>();
            var now = DateTime.Now;

            foreach (var kvp in PortStates)
            {
                var portState = kvp.Value;
                if (portState.IsActive &&
                    (now - portState.LastBpduSent).TotalSeconds >= portState.HelloTime)
                {
                    portsNeedingBpdu.Add(kvp.Key);
                }
            }

            return portsNeedingBpdu;
        }

        /// <summary>
        /// Check for ports that have aged out
        /// </summary>
        public List<string> GetAgedOutPorts()
        {
            var agedOutPorts = new List<string>();
            var now = DateTime.Now;

            foreach (var kvp in PortStates)
            {
                var portState = kvp.Value;
                if (portState.IsActive &&
                    (now - portState.LastBpduReceived).TotalSeconds > portState.MaxAge)
                {
                    agedOutPorts.Add(kvp.Key);
                }
            }

            return agedOutPorts;
        }
    }

    /// <summary>
    /// Represents the state of an STP port
    /// </summary>
    public class StpPortState
    {
        public string PortName { get; set; }
        public StpPortStateType State { get; set; } = StpPortStateType.Blocking;
        public DateTime StateChangeTime { get; set; } = DateTime.Now;
        public StpPortRole Role { get; set; } = StpPortRole.Designated;
        public int Priority { get; set; } = 128;
        public int PathCost { get; set; } = 19;
        public string DesignatedBridge { get; set; } = string.Empty;
        public string DesignatedPort { get; set; } = string.Empty;
        public int DesignatedCost { get; set; } = 0;
        public DateTime LastBpduSent { get; set; } = DateTime.MinValue;
        public DateTime LastBpduReceived { get; set; } = DateTime.MinValue;
        public bool IsActive { get; set; } = true;
        public int HelloTime { get; set; } = 2;
        public int MaxAge { get; set; } = 20;
        public int ForwardDelay { get; set; } = 15;

        public StpPortState(string portName)
        {
            PortName = portName;
        }

        public void ChangeState(StpPortStateType newState)
        {
            if (State != newState)
            {
                State = newState;
                StateChangeTime = DateTime.Now;
            }
        }

        public void ChangeRole(StpPortRole newRole)
        {
            if (Role != newRole)
            {
                Role = newRole;
            }
        }

        public TimeSpan GetTimeInCurrentState()
        {
            return DateTime.Now - StateChangeTime;
        }

        public bool ShouldSendBpdu()
        {
            return (DateTime.Now - LastBpduSent).TotalSeconds >= HelloTime;
        }

        public bool HasAgedOut()
        {
            return (DateTime.Now - LastBpduReceived).TotalSeconds > MaxAge;
        }

        public bool CanTransitionToForwarding()
        {
            return State == StpPortStateType.Learning &&
                   GetTimeInCurrentState().TotalSeconds >= ForwardDelay;
        }

        public bool CanTransitionToLearning()
        {
            return State == StpPortStateType.Listening &&
                   GetTimeInCurrentState().TotalSeconds >= ForwardDelay;
        }
    }

    /// <summary>
    /// STP port states
    /// </summary>
    public enum StpPortStateType
    {
        Blocking,
        Listening,
        Learning,
        Forwarding,
        Disabled
    }

    /// <summary>
    /// STP port roles
    /// </summary>
    public enum StpPortRole
    {
        Root,
        Designated,
        Alternate,
        Backup,
        Disabled
    }
}
