using System;
using System.Collections.Generic;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;
// Use the existing HsrpConfig from Common project

namespace NetForge.Simulation.Protocols.HSRP
{
    // HSRP Protocol State
    public class HsrpState : BaseProtocolState
    {
        public string RouterId { get; set; } = "";
        public Dictionary<int, HsrpGroupState> Groups { get; set; } = new();
        public Dictionary<string, HsrpNeighbor> Neighbors { get; set; } = new();
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

        public HsrpNeighbor GetOrCreateNeighbor(string neighborId, Func<HsrpNeighbor> factory)
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

        public HsrpGroupState GetOrCreateGroupState(int groupId)
        {
            if (!Groups.ContainsKey(groupId))
            {
                Groups[groupId] = new HsrpGroupState
                {
                    GroupId = groupId,
                    State = HsrpProtocolState.Initial
                };
                PolicyChanged = true;
            }
            return Groups[groupId];
        }
    }

    // HSRP Group State
    public class HsrpGroupState
    {
        public int GroupId { get; set; }
        public HsrpProtocolState State { get; set; } = HsrpProtocolState.Initial;
        public string VirtualIpAddress { get; set; } = "";
        public string VirtualMacAddress { get; set; } = "";
        public string InterfaceName { get; set; } = "";
        public int Priority { get; set; } = 100;
        public bool Preempt { get; set; } = false;
        public int PreemptDelay { get; set; } = 0;
        public int HelloInterval { get; set; } = 3;
        public int HoldTime { get; set; } = 10;
        public string ActiveRouter { get; set; } = "";
        public string StandbyRouter { get; set; } = "";
        public DateTime LastHelloSent { get; set; } = DateTime.MinValue;
        public DateTime LastHelloReceived { get; set; } = DateTime.MinValue;
        public int Version { get; set; } = 1;
        public string AuthType { get; set; } = "none";
        public string AuthKey { get; set; } = "";
        public HsrpTimers Timers { get; set; } = new();
        public HsrpStatistics Statistics { get; set; } = new();
        public bool IsTracking { get; set; } = false;
        public int TrackPriority { get; set; } = 0;
    }

    // HSRP States according to RFC 2281
    public enum HsrpProtocolState
    {
        Initial = 0,    // Initial state
        Learn = 1,      // Learning state
        Listen = 2,     // Listening state
        Speak = 3,      // Speaking state
        Standby = 4,    // Standby state
        Active = 5      // Active state
    }

    // HSRP Neighbor
    public class HsrpNeighbor
    {
        public string RouterId { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string InterfaceName { get; set; } = "";
        public int Priority { get; set; } = 100;
        public DateTime LastSeen { get; set; } = DateTime.Now;
        public HsrpProtocolState State { get; set; } = HsrpProtocolState.Initial;
        public Dictionary<int, HsrpGroupInfo> Groups { get; set; } = new();
        public int Version { get; set; } = 1;
        public bool IsVirtualIpOwner { get; set; } = false;
    }

    public class HsrpGroupInfo
    {
        public int GroupId { get; set; }
        public string VirtualIpAddress { get; set; } = "";
        public int Priority { get; set; } = 100;
        public HsrpProtocolState State { get; set; } = HsrpProtocolState.Initial;
        public DateTime LastHello { get; set; } = DateTime.Now;
        public string VirtualMacAddress { get; set; } = "";
    }

    // HSRP Hello Packet
    public class HsrpHelloPacket
    {
        public int Version { get; set; } = 1;
        public HsrpOpCode OpCode { get; set; } = HsrpOpCode.Hello;
        public HsrpProtocolState State { get; set; } = HsrpProtocolState.Initial;
        public int HelloTime { get; set; } = 3;
        public int HoldTime { get; set; } = 10;
        public int Priority { get; set; } = 100;
        public int GroupId { get; set; } = 0;
        public string AuthType { get; set; } = "none";
        public byte[] AuthData { get; set; } = new byte[8];
        public string VirtualIpAddress { get; set; } = "";
        public DateTime SentTime { get; set; } = DateTime.Now;
        public string SourceRouter { get; set; } = "";
        public string SourceInterface { get; set; } = "";
    }

    public enum HsrpOpCode
    {
        Hello = 0,
        Coup = 1,        // Coup message (immediate takeover)
        Resign = 2       // Resign message (giving up active role)
    }

    // HSRP Timers
    public class HsrpTimers
    {
        public DateTime HelloTimer { get; set; } = DateTime.MinValue;
        public DateTime ActiveTimer { get; set; } = DateTime.MinValue;
        public DateTime StandbyTimer { get; set; } = DateTime.MinValue;
        public bool HelloTimerRunning { get; set; } = false;
        public bool ActiveTimerRunning { get; set; } = false;
        public bool StandbyTimerRunning { get; set; } = false;

        public void StartHelloTimer(int interval)
        {
            HelloTimer = DateTime.Now.AddSeconds(interval);
            HelloTimerRunning = true;
        }

        public void StopHelloTimer()
        {
            HelloTimerRunning = false;
        }

        public bool IsHelloTimerExpired()
        {
            return HelloTimerRunning && DateTime.Now >= HelloTimer;
        }

        public void StartActiveTimer(int holdTime)
        {
            ActiveTimer = DateTime.Now.AddSeconds(holdTime);
            ActiveTimerRunning = true;
        }

        public void StopActiveTimer()
        {
            ActiveTimerRunning = false;
        }

        public bool IsActiveTimerExpired()
        {
            return ActiveTimerRunning && DateTime.Now >= ActiveTimer;
        }

        public void StartStandbyTimer(int holdTime)
        {
            StandbyTimer = DateTime.Now.AddSeconds(holdTime);
            StandbyTimerRunning = true;
        }

        public void StopStandbyTimer()
        {
            StandbyTimerRunning = false;
        }

        public bool IsStandbyTimerExpired()
        {
            return StandbyTimerRunning && DateTime.Now >= StandbyTimer;
        }
    }

    // HSRP Statistics
    public class HsrpStatistics
    {
        public string InterfaceName { get; set; } = "";
        public int GroupId { get; set; }
        public int HellosSent { get; set; } = 0;
        public int HellosReceived { get; set; } = 0;
        public int CoupsSent { get; set; } = 0;
        public int CoupsReceived { get; set; } = 0;
        public int ResignsSent { get; set; } = 0;
        public int ResignsReceived { get; set; } = 0;
        public int StateTransitions { get; set; } = 0;
        public int BecomeActive { get; set; } = 0;
        public int BecomeStandby { get; set; } = 0;
        public DateTime LastStateChange { get; set; } = DateTime.MinValue;
        public TimeSpan ActiveTime { get; set; } = TimeSpan.Zero;
        public TimeSpan StandbyTime { get; set; } = TimeSpan.Zero;
        public int AuthFailures { get; set; } = 0;
        public int InvalidPackets { get; set; } = 0;
    }

    // HSRP Events
    public enum HsrpEvent
    {
        Startup,
        Shutdown,
        InterfaceUp,
        InterfaceDown,
        ActiveTimerExpired,
        StandbyTimerExpired,
        HelloTimerExpired,
        HigherPriorityHelloReceived,
        LowerPriorityHelloReceived,
        EqualPriorityHelloReceived,
        CoupReceived,
        ResignReceived,
        HelloFromActiveReceived,
        HelloFromStandbyReceived,
        PreemptDelayExpired,
        TrackingInterfaceDown,
        TrackingInterfaceUp
    }

    // HSRP State Machine
    public class HsrpStateMachine(HsrpGroupState groupState)
    {
        public HsrpGroupState GroupState { get; set; } = groupState;

        public void ProcessEvent(HsrpEvent eventType, HsrpHelloPacket? packet = null)
        {
            var previousState = GroupState.State;

            switch (GroupState.State)
            {
                case HsrpProtocolState.Initial:
                    HandleInitialState(eventType, packet);
                    break;
                case HsrpProtocolState.Learn:
                    HandleLearnState(eventType, packet);
                    break;
                case HsrpProtocolState.Listen:
                    HandleListenState(eventType, packet);
                    break;
                case HsrpProtocolState.Speak:
                    HandleSpeakState(eventType, packet);
                    break;
                case HsrpProtocolState.Standby:
                    HandleStandbyState(eventType, packet);
                    break;
                case HsrpProtocolState.Active:
                    HandleActiveState(eventType, packet);
                    break;
            }

            if (previousState != GroupState.State)
            {
                GroupState.Statistics.StateTransitions++;
                GroupState.Statistics.LastStateChange = DateTime.Now;

                if (GroupState.State == HsrpProtocolState.Active)
                {
                    GroupState.Statistics.BecomeActive++;
                }
                else if (GroupState.State == HsrpProtocolState.Standby)
                {
                    GroupState.Statistics.BecomeStandby++;
                }
            }
        }

        private void HandleInitialState(HsrpEvent eventType, HsrpHelloPacket packet)
        {
            switch (eventType)
            {
                case HsrpEvent.Startup:
                case HsrpEvent.InterfaceUp:
                    TransitionTo(HsrpProtocolState.Learn);
                    break;
            }
        }

        private void HandleLearnState(HsrpEvent eventType, HsrpHelloPacket packet)
        {
            switch (eventType)
            {
                case HsrpEvent.HelloTimerExpired:
                    TransitionTo(HsrpProtocolState.Listen);
                    break;
                case HsrpEvent.HelloFromActiveReceived:
                case HsrpEvent.HelloFromStandbyReceived:
                    // Learn virtual IP address from hello
                    if (packet != null && !string.IsNullOrEmpty(packet.VirtualIpAddress))
                    {
                        GroupState.VirtualIpAddress = packet.VirtualIpAddress;
                        TransitionTo(HsrpProtocolState.Listen);
                    }
                    break;
                case HsrpEvent.InterfaceDown:
                case HsrpEvent.Shutdown:
                    TransitionTo(HsrpProtocolState.Initial);
                    break;
            }
        }

        private void HandleListenState(HsrpEvent eventType, HsrpHelloPacket packet)
        {
            switch (eventType)
            {
                case HsrpEvent.ActiveTimerExpired:
                case HsrpEvent.StandbyTimerExpired:
                    TransitionTo(HsrpProtocolState.Speak);
                    break;
                case HsrpEvent.HelloFromActiveReceived:
                    GroupState.Timers.StartActiveTimer(GroupState.HoldTime);
                    break;
                case HsrpEvent.HelloFromStandbyReceived:
                    GroupState.Timers.StartStandbyTimer(GroupState.HoldTime);
                    break;
                case HsrpEvent.InterfaceDown:
                case HsrpEvent.Shutdown:
                    TransitionTo(HsrpProtocolState.Initial);
                    break;
            }
        }

        private void HandleSpeakState(HsrpEvent eventType, HsrpHelloPacket packet)
        {
            switch (eventType)
            {
                case HsrpEvent.StandbyTimerExpired:
                    TransitionTo(HsrpProtocolState.Standby);
                    break;
                case HsrpEvent.HelloFromActiveReceived:
                    GroupState.Timers.StartActiveTimer(GroupState.HoldTime);
                    break;
                case HsrpEvent.HelloFromStandbyReceived:
                    if (packet != null && packet.Priority < GroupState.Priority)
                    {
                        GroupState.Timers.StartStandbyTimer(GroupState.HoldTime);
                    }
                    else
                    {
                        TransitionTo(HsrpProtocolState.Listen);
                    }
                    break;
                case HsrpEvent.HigherPriorityHelloReceived:
                    TransitionTo(HsrpProtocolState.Listen);
                    break;
                case HsrpEvent.InterfaceDown:
                case HsrpEvent.Shutdown:
                    TransitionTo(HsrpProtocolState.Initial);
                    break;
            }
        }

        private void HandleStandbyState(HsrpEvent eventType, HsrpHelloPacket packet)
        {
            switch (eventType)
            {
                case HsrpEvent.ActiveTimerExpired:
                    TransitionTo(HsrpProtocolState.Active);
                    break;
                case HsrpEvent.HelloFromActiveReceived:
                    GroupState.Timers.StartActiveTimer(GroupState.HoldTime);
                    break;
                case HsrpEvent.HigherPriorityHelloReceived:
                    if (packet?.State == HsrpProtocolState.Speak)
                    {
                        TransitionTo(HsrpProtocolState.Listen);
                    }
                    break;
                case HsrpEvent.CoupReceived:
                    TransitionTo(HsrpProtocolState.Listen);
                    break;
                case HsrpEvent.InterfaceDown:
                case HsrpEvent.Shutdown:
                    TransitionTo(HsrpProtocolState.Initial);
                    break;
            }
        }

        private void HandleActiveState(HsrpEvent eventType, HsrpHelloPacket packet)
        {
            switch (eventType)
            {
                case HsrpEvent.HigherPriorityHelloReceived:
                    if (packet?.State == HsrpProtocolState.Speak && GroupState.Preempt)
                    {
                        SendResignMessage();
                        TransitionTo(HsrpProtocolState.Speak);
                    }
                    break;
                case HsrpEvent.CoupReceived:
                    TransitionTo(HsrpProtocolState.Speak);
                    break;
                case HsrpEvent.InterfaceDown:
                case HsrpEvent.Shutdown:
                case HsrpEvent.TrackingInterfaceDown:
                    SendResignMessage();
                    TransitionTo(HsrpProtocolState.Initial);
                    break;
            }
        }

        private void TransitionTo(HsrpProtocolState newState)
        {
            GroupState.State = newState;

            switch (newState)
            {
                case HsrpProtocolState.Learn:
                    GroupState.Timers.StartHelloTimer(GroupState.HelloInterval);
                    break;
                case HsrpProtocolState.Listen:
                    GroupState.Timers.StopHelloTimer();
                    break;
                case HsrpProtocolState.Speak:
                    GroupState.Timers.StartHelloTimer(GroupState.HelloInterval);
                    break;
                case HsrpProtocolState.Standby:
                    GroupState.StandbyRouter = GroupState.ActiveRouter; // We are now standby
                    break;
                case HsrpProtocolState.Active:
                    GroupState.ActiveRouter = GroupState.VirtualIpAddress; // We are now active
                    SendGratuitousArp();
                    break;
            }
        }

        private void SendResignMessage()
        {
            GroupState.Statistics.ResignsSent++;
            // Implementation would send actual HSRP resign packet
        }

        private void SendGratuitousArp()
        {
            // Implementation would send gratuitous ARP for virtual IP
        }
    }

    // HSRP Interface Configuration
    public class HsrpInterfaceConfig
    {
        public string InterfaceName { get; set; } = "";
        public Dictionary<int, HsrpGroupState> Groups { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
        public int Version { get; set; } = 1;
        public bool UseVia { get; set; } = false;
    }

    // HSRP Authentication
    public class HsrpAuthentication
    {
        public string Type { get; set; } = "none"; // none, text, md5
        public string Key { get; set; } = "";
        public int KeyId { get; set; } = 0;

        public bool ValidatePacket(HsrpHelloPacket packet)
        {
            switch (Type.ToLowerInvariant())
            {
                case "none":
                    return true;
                case "text":
                    return ValidateTextAuth(packet);
                case "md5":
                    return ValidateMd5Auth(packet);
                default:
                    return false;
            }
        }

        private bool ValidateTextAuth(HsrpHelloPacket packet)
        {
            // Simple text comparison
            return packet.AuthType == "text" &&
                   System.Text.Encoding.ASCII.GetString(packet.AuthData).TrimEnd('\0') == Key;
        }

        private bool ValidateMd5Auth(HsrpHelloPacket packet)
        {
            // MD5 authentication validation
            // In a real implementation, this would calculate MD5 hash
            return packet.AuthType == "md5";
        }
    }

    // HSRP Virtual MAC Address Generator
    public static class HsrpMacAddressGenerator
    {
        public static string GenerateVirtualMac(int groupId, int version = 1)
        {
            if (version == 1)
            {
                // HSRPv1: 00:00:0c:07:ac:xx (where xx is group ID)
                return $"00:00:0c:07:ac:{groupId:x2}";
            }
            else
            {
                // HSRPv2: 00:00:0c:9f:fx:xx (where xxxx is group ID)
                return $"00:00:0c:9f:f{(groupId >> 8):x}:{(groupId & 0xff):x2}";
            }
        }
    }
}
