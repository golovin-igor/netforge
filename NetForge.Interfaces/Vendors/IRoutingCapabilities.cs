namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Routing protocol configuration capabilities
    /// </summary>
    public interface IRoutingCapabilities
    {
        /// <summary>
        /// Initialize OSPF routing protocol
        /// </summary>
        bool InitializeOspf(int processId);

        /// <summary>
        /// Initialize BGP routing protocol
        /// </summary>
        bool InitializeBgp(int asNumber);

        /// <summary>
        /// Initialize RIP routing protocol
        /// </summary>
        bool InitializeRip();

        /// <summary>
        /// Initialize EIGRP routing protocol
        /// </summary>
        bool InitializeEigrp(int asNumber);

        /// <summary>
        /// Set current router protocol context
        /// </summary>
        bool SetCurrentRouterProtocol(string protocol);
    }
}