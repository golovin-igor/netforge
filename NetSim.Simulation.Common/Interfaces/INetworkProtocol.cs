using NetSim.Simulation.Common;
using NetSim.Simulation.Events;

namespace NetSim.Simulation.Interfaces
{
    public enum ProtocolType
    {
        OSPF,
        BGP,
        RIP,
        EIGRP,
        STP,
        CDP,
        ISIS,
        LLDP,
        IGRP,
        VRRP,
        HSRP,
        ARP,
        TELNET
        // Add other protocol types as needed
    }

    public interface INetworkProtocol
    {
        ProtocolType Type { get; }
        void Initialize(NetworkDevice device);
        Task UpdateState(NetworkDevice device);
        void SubscribeToEvents(NetworkEventBus eventBus, NetworkDevice self);
        // Potentially other common methods, e.g., for configuration
        // string GetConfiguration();
        // void ApplyConfiguration(string configSnippet);
    }} 
