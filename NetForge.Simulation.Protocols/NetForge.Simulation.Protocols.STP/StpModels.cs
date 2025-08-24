using System;
using System.Collections.Generic;
using System.Linq;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Base;
// Use the existing StpConfig from Common project

namespace NetForge.Simulation.Protocols.STP
{
    // STP Protocol State
    public class StpState : BaseProtocolState
    {
        public string BridgeId { get; set; } = "";
        public string RootBridgeId { get; set; } = "";
        public int RootPathCost { get; set; } = 0;
        public string RootPort { get; set; } = "";
        public Dictionary<string, StpPortState> PortStates { get; set; } = new();
        public Dictionary<string, StpPortInfo> PortInfo { get; set; } = new();
        public Dictionary<string, DateTime> PortTimers { get; set; } = new();
        public bool TopologyChanged { get; set; } = true;
        public DateTime LastTopologyChange { get; set; } = DateTime.MinValue;
        public int TopologyChangeCount { get; set; } = 0;
        public bool IsRootBridge { get; set; } = false;
        public DateTime LastConfigurationBpduReceived { get; set; } = DateTime.MinValue;
        public DateTime LastBpduSent { get; set; } = DateTime.MinValue;

        public override Dictionary<string, object> GetStateData()
        {
            var baseData = base.GetStateData();
            baseData["BridgeId"] = BridgeId;
            baseData["RootBridgeId"] = RootBridgeId;
            baseData["RootPathCost"] = RootPathCost;
            baseData["RootPort"] = RootPort;
            baseData["PortStates"] = PortStates;
            baseData["IsRootBridge"] = IsRootBridge;
            baseData["TopologyChanged"] = TopologyChanged;
            baseData["TopologyChangeCount"] = TopologyChangeCount;
            return baseData;
        }

        public StpPortInfo GetOrCreatePortInfo(string portName)
        {
            if (!PortInfo.ContainsKey(portName))
            {
                PortInfo[portName] = new StpPortInfo
                {
                    PortName = portName,
                    State = StpPortState.Blocking,
                    Role = StpPortRole.Designated
                };
                PortStates[portName] = StpPortState.Blocking;
                MarkStateChanged();
            }
            return PortInfo[portName];
        }

        public void UpdatePortState(string portName, StpPortState newState)
        {
            if (PortStates.ContainsKey(portName) && PortStates[portName] != newState)
            {
                PortStates[portName] = newState;
                if (PortInfo.ContainsKey(portName))
                {
                    PortInfo[portName].State = newState;
                }
                MarkStateChanged();
                TopologyChanged = true;
            }
        }
    }

    // STP Port Information
    public class StpPortInfo
    {
        public string PortName { get; set; } = "";
        public StpPortState State { get; set; } = StpPortState.Blocking;
        public StpPortRole Role { get; set; } = StpPortRole.Designated;
        public int Priority { get; set; } = 128;
        public int PathCost { get; set; } = 19; // Default for 100 Mbps
        public string DesignatedBridge { get; set; } = "";
        public string DesignatedPort { get; set; } = "";
        public int DesignatedCost { get; set; } = 0;
        public DateTime LastBpduReceived { get; set; } = DateTime.MinValue;
        public DateTime LastBpduSent { get; set; } = DateTime.MinValue;
        public StpBpdu LastReceivedBpdu { get; set; } = new();
        public bool EdgePort { get; set; } = false;
        public bool PortFast { get; set; } = false;
        public int MessageAge { get; set; } = 0;
        public StpTimers Timers { get; set; } = new();
        public StpStatistics? Statistics { get; set; }
    }

    // STP Port States (IEEE 802.1D)
    public enum StpPortState
    {
        Disabled = 0,
        Blocking = 1,
        Listening = 2,
        Learning = 3,
        Forwarding = 4
    }

    // STP Port Roles
    public enum StpPortRole
    {
        Root = 0,
        Designated = 1,
        Alternate = 2,
        Backup = 3,
        Disabled = 4
    }

    // STP BPDU (Bridge Protocol Data Unit)
    public class StpBpdu
    {
        public StpBpduType Type { get; set; } = StpBpduType.Configuration;
        public StpBpduFlags Flags { get; set; } = StpBpduFlags.None;
        public string RootBridgeId { get; set; } = "";
        public int RootPathCost { get; set; } = 0;
        public string BridgeId { get; set; } = "";
        public string PortId { get; set; } = "";
        public int MessageAge { get; set; } = 0;
        public int MaxAge { get; set; } = 20;
        public int HelloTime { get; set; } = 2;
        public int ForwardDelay { get; set; } = 15;
        public DateTime ReceivedTime { get; set; } = DateTime.Now;
        public string SourceMac { get; set; } = "";
        public string SourceInterface { get; set; } = "";
        public int Version { get; set; } = 0; // STP = 0, RSTP = 2
    }

    public enum StpBpduType
    {
        Configuration = 0,
        TopologyChange = 1,
        TopologyChangeNotification = 0x80
    }

    [Flags]
    public enum StpBpduFlags
    {
        None = 0,
        TopologyChange = 0x01,
        TopologyChangeAcknowledgment = 0x80
    }

    // STP Timers
    public class StpTimers
    {
        public DateTime HelloTimer { get; set; } = DateTime.MinValue;
        public DateTime MessageAgeTimer { get; set; } = DateTime.MinValue;
        public DateTime ForwardDelayTimer { get; set; } = DateTime.MinValue;
        public bool HelloTimerRunning { get; set; } = false;
        public bool MessageAgeTimerRunning { get; set; } = false;
        public bool ForwardDelayTimerRunning { get; set; } = false;

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

        public void StartMessageAgeTimer(int maxAge)
        {
            MessageAgeTimer = DateTime.Now.AddSeconds(maxAge);
            MessageAgeTimerRunning = true;
        }

        public void StopMessageAgeTimer()
        {
            MessageAgeTimerRunning = false;
        }

        public bool IsMessageAgeTimerExpired()
        {
            return MessageAgeTimerRunning && DateTime.Now >= MessageAgeTimer;
        }

        public void StartForwardDelayTimer(int delay)
        {
            ForwardDelayTimer = DateTime.Now.AddSeconds(delay);
            ForwardDelayTimerRunning = true;
        }

        public void StopForwardDelayTimer()
        {
            ForwardDelayTimerRunning = false;
        }

        public bool IsForwardDelayTimerExpired()
        {
            return ForwardDelayTimerRunning && DateTime.Now >= ForwardDelayTimer;
        }
    }

    // STP Bridge Priority
    public class StpBridgePriority
    {
        public int Priority { get; set; } = 32768; // Default bridge priority
        public string MacAddress { get; set; } = "";

        public string GetBridgeId()
        {
            return $"{Priority:X4}:{MacAddress}";
        }

        public static int CompareBridgeIds(string bridgeId1, string bridgeId2)
        {
            // Parse and compare bridge IDs (priority + MAC address)
            var parts1 = bridgeId1.Split(':');
            var parts2 = bridgeId2.Split(':');

            if (parts1.Length < 2 || parts2.Length < 2)
                return string.Compare(bridgeId1, bridgeId2, StringComparison.Ordinal);

            // Compare priority first
            if (int.TryParse(parts1[0], System.Globalization.NumberStyles.HexNumber, null, out int priority1) &&
                int.TryParse(parts2[0], System.Globalization.NumberStyles.HexNumber, null, out int priority2))
            {
                int priorityComparison = priority1.CompareTo(priority2);
                if (priorityComparison != 0)
                    return priorityComparison;
            }

            // If priorities are equal, compare MAC addresses
            var mac1 = string.Join(":", parts1.Skip(1));
            var mac2 = string.Join(":", parts2.Skip(1));
            return string.Compare(mac1, mac2, StringComparison.Ordinal);
        }
    }

    // STP Port Cost calculation
    public class StpPortCost
    {
        // Standard STP port costs based on link speed
        public static int GetDefaultCost(string interfaceName, int bandwidth = 0)
        {
            if (bandwidth > 0)
            {
                // Calculate cost based on bandwidth (reference bandwidth = 20 Gbps)
                return Math.Max(1, (int)(20000000 / bandwidth));
            }

            // Default costs based on interface type
            var lowerName = interfaceName.ToLowerInvariant();

            if (lowerName.Contains("gigabit") || lowerName.Contains("ge"))
                return 4; // 1 Gbps
            else if (lowerName.Contains("fastethernet") || lowerName.Contains("fe"))
                return 19; // 100 Mbps
            else if (lowerName.Contains("ethernet") || lowerName.Contains("eth"))
                return 100; // 10 Mbps
            else if (lowerName.Contains("serial"))
                return 647; // T1 (1.544 Mbps)
            else
                return 19; // Default to Fast Ethernet cost
        }
    }

    // STP Statistics
    public class StpStatistics
    {
        public string InterfaceName { get; set; } = "";
        public int BpdusReceived { get; set; } = 0;
        public int BpdusSent { get; set; } = 0;
        public int ConfigBpdusReceived { get; set; } = 0;
        public int ConfigBpdusSent { get; set; } = 0;
        public int TopologyChangeNotificationsReceived { get; set; } = 0;
        public int TopologyChangeNotificationsSent { get; set; } = 0;
        public int InvalidBpdusReceived { get; set; } = 0;
        public int StateTransitions { get; set; } = 0;
        public DateTime LastStateTransition { get; set; } = DateTime.MinValue;
        public DateTime LastBpduReceived { get; set; } = DateTime.MinValue;
        public DateTime LastBpduSent { get; set; } = DateTime.MinValue;
    }

    // STP State Machine
    public class StpStateMachine
    {
        public StpPortInfo PortInfo { get; set; }
        public StpStatistics Statistics { get; set; } = new();

        public StpStateMachine(StpPortInfo portInfo)
        {
            PortInfo = portInfo;
            Statistics.InterfaceName = portInfo.PortName;
        }

        public void ProcessStateTransition(StpPortState newState, string reason = "")
        {
            if (PortInfo.State != newState)
            {
                var oldState = PortInfo.State;
                PortInfo.State = newState;
                Statistics.StateTransitions++;
                Statistics.LastStateTransition = DateTime.Now;

                // Handle state-specific logic
                switch (newState)
                {
                    case StpPortState.Blocking:
                        PortInfo.Timers.StopForwardDelayTimer();
                        break;
                    case StpPortState.Listening:
                        PortInfo.Timers.StartForwardDelayTimer(15); // Default forward delay
                        break;
                    case StpPortState.Learning:
                        PortInfo.Timers.StartForwardDelayTimer(15);
                        break;
                    case StpPortState.Forwarding:
                        PortInfo.Timers.StopForwardDelayTimer();
                        break;
                }
            }
        }

        public StpPortState GetNextState(StpEvent eventType)
        {
            return PortInfo.State switch
            {
                StpPortState.Blocking => eventType switch
                {
                    StpEvent.SelectedAsRoot => StpPortState.Listening,
                    StpEvent.SelectedAsDesignated => StpPortState.Listening,
                    _ => StpPortState.Blocking
                },
                StpPortState.Listening => eventType switch
                {
                    StpEvent.ForwardDelayExpired => StpPortState.Learning,
                    StpEvent.NotSelectedAsRoot => StpPortState.Blocking,
                    StpEvent.NotSelectedAsDesignated => StpPortState.Blocking,
                    _ => StpPortState.Listening
                },
                StpPortState.Learning => eventType switch
                {
                    StpEvent.ForwardDelayExpired => StpPortState.Forwarding,
                    StpEvent.NotSelectedAsRoot => StpPortState.Blocking,
                    StpEvent.NotSelectedAsDesignated => StpPortState.Blocking,
                    _ => StpPortState.Learning
                },
                StpPortState.Forwarding => eventType switch
                {
                    StpEvent.NotSelectedAsRoot => StpPortState.Blocking,
                    StpEvent.NotSelectedAsDesignated => StpPortState.Blocking,
                    _ => StpPortState.Forwarding
                },
                _ => PortInfo.State
            };
        }
    }

    // STP Events
    public enum StpEvent
    {
        PortEnabled,
        PortDisabled,
        SelectedAsRoot,
        NotSelectedAsRoot,
        SelectedAsDesignated,
        NotSelectedAsDesignated,
        ForwardDelayExpired,
        MessageAgeExpired,
        BpduReceived,
        SuperiorBpduReceived,
        InferiorBpduReceived,
        TopologyChange
    }

    // STP Configuration BPDU comparison result
    public enum BpduComparison
    {
        Superior,
        Inferior,
        Same,
        Different
    }

    // STP Helper class for BPDU comparison
    public static class StpBpduComparator
    {
        public static BpduComparison Compare(StpBpdu received, StpBpdu current)
        {
            // Compare root bridge ID first
            int rootComparison = StpBridgePriority.CompareBridgeIds(received.RootBridgeId, current.RootBridgeId);
            if (rootComparison < 0) return BpduComparison.Superior;
            if (rootComparison > 0) return BpduComparison.Inferior;

            // Root bridge IDs are same, compare root path costs
            if (received.RootPathCost < current.RootPathCost) return BpduComparison.Superior;
            if (received.RootPathCost > current.RootPathCost) return BpduComparison.Inferior;

            // Root path costs are same, compare bridge IDs
            int bridgeComparison = StpBridgePriority.CompareBridgeIds(received.BridgeId, current.BridgeId);
            if (bridgeComparison < 0) return BpduComparison.Superior;
            if (bridgeComparison > 0) return BpduComparison.Inferior;

            // Bridge IDs are same, compare port IDs
            int portComparison = string.Compare(received.PortId, current.PortId, StringComparison.Ordinal);
            if (portComparison < 0) return BpduComparison.Superior;
            if (portComparison > 0) return BpduComparison.Inferior;

            return BpduComparison.Same;
        }

        public static bool IsSuperiorBpdu(StpBpdu received, StpBpdu current)
        {
            return Compare(received, current) == BpduComparison.Superior;
        }
    }
}
