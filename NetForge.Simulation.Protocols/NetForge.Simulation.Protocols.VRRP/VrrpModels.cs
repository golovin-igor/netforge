using System;
using System.Collections.Generic;
using NetForge.Simulation.Protocols.Common;
// Use the existing VrrpConfig from Common project  
using NetForge.Simulation.Protocols.Routing;

namespace NetForge.Simulation.Protocols.VRRP
{
    // VRRP Protocol State
    public class VrrpState : BaseProtocolState
    {
        public string RouterId { get; set; } = "";
        public Dictionary<int, VrrpGroup> Groups { get; set; } = new();
        public Dictionary<string, VrrpNeighbor> Neighbors { get; set; } = new();
        public Dictionary<string, DateTime> InterfaceTimers { get; set; } = new();
        public bool PolicyChanged { get; set; } = true;

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["RouterId"] = RouterId;
            baseData["Groups"] = Groups;
            baseData["Neighbors"] = Neighbors;
            baseData["PolicyChanged"] = PolicyChanged;
            return baseData;
        }

        public VrrpNeighbor GetOrCreateNeighbor(string neighborId, Func<VrrpNeighbor> factory)
        {
            if (!Neighbors.ContainsKey(neighborId))
            {
                Neighbors[neighborId] = factory();
                MarkStateChanged();
            }
            return Neighbors[neighborId];
        }

        public override void RemoveNeighbor(string neighborId)
        {
            if (Neighbors.Remove(neighborId))
            {
                MarkStateChanged();
                PolicyChanged = true;
            }
        }

        public VrrpGroup GetOrCreateGroup(int groupId)
        {
            if (!Groups.ContainsKey(groupId))
            {
                Groups[groupId] = new VrrpGroup
                {
                    GroupId = groupId,
                    State = VrrpState.Initialize
                };
                PolicyChanged = true;
            }
            return Groups[groupId];
        }
    }

    // VRRP Group Configuration and State
    public class VrrpGroup
    {
        public int GroupId { get; set; }
        public string VirtualIpAddress { get; set; } = "";
        public string Interface { get; set; } = "";
        public int Priority { get; set; } = 100; // Default priority
        public VrrpState State { get; set; } = VrrpState.Initialize;
        public int AdvertisementInterval { get; set; } = 1; // seconds
        public bool Preempt { get; set; } = true;
        public int PreemptDelay { get; set; } = 0;
        public string MasterIpAddress { get; set; } = "";
        public DateTime LastAdvertisement { get; set; } = DateTime.MinValue;
        public int MasterDownInterval { get; set; } = 3; // seconds
        public string VirtualMacAddress { get; set; } = "";
        public List<string> AuthenticationKeys { get; set; } = new();
        public VrrpVersion Version { get; set; } = VrrpVersion.Version3;
        public bool IsOwner { get; set; } = false; // True if this router owns the virtual IP
    }

    public enum VrrpState
    {
        Initialize = 0,
        Backup = 1,
        Master = 2
    }

    public enum VrrpVersion
    {
        Version2 = 2,
        Version3 = 3
    }

    // VRRP Neighbor
    public class VrrpNeighbor
    {
        public string RouterId { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string InterfaceName { get; set; } = "";
        public int Priority { get; set; } = 100;
        public DateTime LastSeen { get; set; } = DateTime.Now;
        public VrrpState State { get; set; } = VrrpState.Backup;
        public Dictionary<int, VrrpGroupInfo> Groups { get; set; } = new();
    }

    public class VrrpGroupInfo
    {
        public int GroupId { get; set; }
        public string VirtualIpAddress { get; set; } = "";
        public int Priority { get; set; } = 100;
        public VrrpState State { get; set; } = VrrpState.Backup;
        public DateTime LastAdvertisement { get; set; } = DateTime.Now;
    }

    // VRRP Advertisement Packet
    public class VrrpAdvertisement
    {
        public VrrpVersion Version { get; set; } = VrrpVersion.Version3;
        public VrrpPacketType Type { get; set; } = VrrpPacketType.Advertisement;
        public int VirtualRouterId { get; set; }
        public int Priority { get; set; } = 100;
        public int CountIpAddrs { get; set; } = 1;
        public int AuthType { get; set; } = 0; // No authentication
        public int AdvertisementInterval { get; set; } = 1;
        public int Checksum { get; set; } = 0;
        public List<string> IpAddresses { get; set; } = new();
        public byte[] AuthData { get; set; } = new byte[8];
        public DateTime SentTime { get; set; } = DateTime.Now;
        public string SourceRouter { get; set; } = "";
    }

    public enum VrrpPacketType
    {
        Advertisement = 1
    }

    // VRRP Interface Configuration
    public class VrrpInterfaceConfig
    {
        public string InterfaceName { get; set; } = "";
        public Dictionary<int, VrrpGroup> Groups { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
        public VrrpVersion Version { get; set; } = VrrpVersion.Version3;
    }

    // VRRP Statistics
    public class VrrpStatistics
    {
        public string InterfaceName { get; set; } = "";
        public int GroupId { get; set; }
        public int BecomeMaster { get; set; } = 0;
        public int AdvertisementsSent { get; set; } = 0;
        public int AdvertisementsReceived { get; set; } = 0;
        public int PriorityZeroPacketsSent { get; set; } = 0;
        public int PriorityZeroPacketsReceived { get; set; } = 0;
        public int InvalidPacketsReceived { get; set; } = 0;
        public int AddressListErrors { get; set; } = 0;
        public int AuthFailures { get; set; } = 0;
        public DateTime LastStateChange { get; set; } = DateTime.MinValue;
        public TimeSpan MasterUptime { get; set; } = TimeSpan.Zero;
    }

    // VRRP Timer Management
    public class VrrpTimers
    {
        public DateTime LastAdvertisementSent { get; set; } = DateTime.MinValue;
        public DateTime LastAdvertisementReceived { get; set; } = DateTime.MinValue;
        public DateTime MasterDownTimer { get; set; } = DateTime.MinValue;
        public DateTime PreemptDelayTimer { get; set; } = DateTime.MinValue;
        public bool MasterDownTimerRunning { get; set; } = false;
        public bool PreemptDelayTimerRunning { get; set; } = false;

        public bool IsMasterDownExpired()
        {
            return MasterDownTimerRunning && DateTime.Now >= MasterDownTimer;
        }

        public bool IsPreemptDelayExpired()
        {
            return PreemptDelayTimerRunning && DateTime.Now >= PreemptDelayTimer;
        }

        public void StartMasterDownTimer(int interval)
        {
            MasterDownTimer = DateTime.Now.AddSeconds(interval);
            MasterDownTimerRunning = true;
        }

        public void StopMasterDownTimer()
        {
            MasterDownTimerRunning = false;
        }

        public void StartPreemptDelayTimer(int delay)
        {
            if (delay > 0)
            {
                PreemptDelayTimer = DateTime.Now.AddSeconds(delay);
                PreemptDelayTimerRunning = true;
            }
        }

        public void StopPreemptDelayTimer()
        {
            PreemptDelayTimerRunning = false;
        }
    }

    // VRRP Event Types
    public enum VrrpEvent
    {
        Startup,
        Shutdown,
        HigherPriorityReceived,
        LowerPriorityReceived,
        EqualPriorityReceived,
        MasterDownTimer,
        PreemptDelayTimer,
        InterfaceUp,
        InterfaceDown,
        PriorityChange
    }

    // VRRP State Machine Context
    public class VrrpStateMachine
    {
        public VrrpGroup Group { get; set; }
        public VrrpTimers Timers { get; set; } = new();
        public VrrpStatistics Statistics { get; set; } = new();
        
        public VrrpStateMachine(VrrpGroup group)
        {
            Group = group;
            Statistics.GroupId = group.GroupId;
        }

        public void ProcessEvent(VrrpEvent eventType, VrrpAdvertisement advertisement = null)
        {
            var previousState = Group.State;
            
            switch (Group.State)
            {
                case VrrpState.Initialize:
                    HandleInitializeState(eventType, advertisement);
                    break;
                case VrrpState.Backup:
                    HandleBackupState(eventType, advertisement);
                    break;
                case VrrpState.Master:
                    HandleMasterState(eventType, advertisement);
                    break;
            }

            if (previousState != Group.State)
            {
                Statistics.LastStateChange = DateTime.Now;
                if (Group.State == VrrpState.Master)
                {
                    Statistics.BecomeMaster++;
                }
            }
        }

        private void HandleInitializeState(VrrpEvent eventType, VrrpAdvertisement advertisement)
        {
            switch (eventType)
            {
                case VrrpEvent.Startup:
                    if (Group.IsOwner)
                    {
                        TransitionToMaster();
                    }
                    else
                    {
                        TransitionToBackup();
                    }
                    break;
                case VrrpEvent.InterfaceDown:
                case VrrpEvent.Shutdown:
                    // Stay in Initialize
                    break;
            }
        }

        private void HandleBackupState(VrrpEvent eventType, VrrpAdvertisement advertisement)
        {
            switch (eventType)
            {
                case VrrpEvent.MasterDownTimer:
                    TransitionToMaster();
                    break;
                case VrrpEvent.HigherPriorityReceived:
                    // Reset master down timer
                    Timers.StartMasterDownTimer(Group.MasterDownInterval);
                    break;
                case VrrpEvent.EqualPriorityReceived:
                    // Compare IP addresses, higher IP becomes master
                    if (advertisement != null && ShouldBecomeMaster(advertisement))
                    {
                        TransitionToMaster();
                    }
                    break;
                case VrrpEvent.LowerPriorityReceived:
                    if (Group.Preempt)
                    {
                        if (Group.PreemptDelay > 0)
                        {
                            Timers.StartPreemptDelayTimer(Group.PreemptDelay);
                        }
                        else
                        {
                            TransitionToMaster();
                        }
                    }
                    break;
                case VrrpEvent.PreemptDelayTimer:
                    TransitionToMaster();
                    break;
                case VrrpEvent.InterfaceDown:
                case VrrpEvent.Shutdown:
                    TransitionToInitialize();
                    break;
            }
        }

        private void HandleMasterState(VrrpEvent eventType, VrrpAdvertisement advertisement)
        {
            switch (eventType)
            {
                case VrrpEvent.HigherPriorityReceived:
                    TransitionToBackup();
                    break;
                case VrrpEvent.EqualPriorityReceived:
                    // Compare IP addresses
                    if (advertisement != null && !ShouldBecomeMaster(advertisement))
                    {
                        TransitionToBackup();
                    }
                    break;
                case VrrpEvent.InterfaceDown:
                case VrrpEvent.Shutdown:
                    // Send priority 0 advertisement and transition to Initialize
                    SendGratuitousAdvertisement();
                    TransitionToInitialize();
                    break;
            }
        }

        private void TransitionToInitialize()
        {
            Group.State = VrrpState.Initialize;
            Timers.StopMasterDownTimer();
            Timers.StopPreemptDelayTimer();
        }

        private void TransitionToBackup()
        {
            Group.State = VrrpState.Backup;
            Timers.StartMasterDownTimer(Group.MasterDownInterval);
            Timers.StopPreemptDelayTimer();
        }

        private void TransitionToMaster()
        {
            Group.State = VrrpState.Master;
            Timers.StopMasterDownTimer();
            Timers.StopPreemptDelayTimer();
            
            // Start sending advertisements
            Group.LastAdvertisement = DateTime.Now;
        }

        private bool ShouldBecomeMaster(VrrpAdvertisement advertisement)
        {
            // In case of equal priority, router with higher IP address becomes master
            // This is a simplified comparison
            return string.Compare(Group.MasterIpAddress, advertisement.SourceRouter, StringComparison.Ordinal) > 0;
        }

        private void SendGratuitousAdvertisement()
        {
            Statistics.PriorityZeroPacketsSent++;
            // Implementation would send actual VRRP packet with priority 0
        }
    }
}