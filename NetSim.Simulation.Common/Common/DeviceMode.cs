namespace NetSim.Simulation.Core
{
    /// <summary>
    /// Strongly typed device modes for compile-time safety and reduced typos
    /// </summary>
    public enum DeviceMode
    {
        /// <summary>
        /// Base user mode (limited privileges)
        /// </summary>
        User,
        
        /// <summary>
        /// Privileged/enable mode (elevated privileges)
        /// </summary>
        Privileged,
        
        /// <summary>
        /// Global configuration mode
        /// </summary>
        Config,
        
        /// <summary>
        /// Interface configuration mode
        /// </summary>
        Interface,
        
        /// <summary>
        /// VLAN configuration mode
        /// </summary>
        Vlan,
        
        /// <summary>
        /// Router configuration mode
        /// </summary>
        Router,
        
        /// <summary>
        /// BGP router sub-mode
        /// </summary>
        RouterBgp,
        
        /// <summary>
        /// OSPF router sub-mode
        /// </summary>
        RouterOspf,
        
        /// <summary>
        /// RIP router sub-mode
        /// </summary>
        RouterRip,
        
        /// <summary>
        /// Access list configuration mode
        /// </summary>
        Acl,
        
        // Juniper-specific modes
        /// <summary>
        /// Juniper operational mode
        /// </summary>
        Operational,
        
        /// <summary>
        /// Juniper configuration mode
        /// </summary>
        Configuration,
        
        // Nokia-specific modes
        /// <summary>
        /// Nokia admin mode
        /// </summary>
        Admin,
        
        // Fortinet-specific modes
        /// <summary>
        /// Fortinet global mode
        /// </summary>
        Global,
        
        /// <summary>
        /// Fortinet global configuration mode
        /// </summary>
        GlobalConfig,
        
        /// <summary>
        /// Fortinet system interface mode
        /// </summary>
        SystemInterface,
        
        /// <summary>
        /// Fortinet router OSPF mode
        /// </summary>
        RouterOspfFortinet,
        
        /// <summary>
        /// Fortinet router BGP mode
        /// </summary>
        RouterBgpFortinet,
        
        /// <summary>
        /// Fortinet BGP neighbor mode
        /// </summary>
        BgpNeighbor,
        
        /// <summary>
        /// Fortinet BGP neighbor edit mode
        /// </summary>
        BgpNeighborEdit,
        
        /// <summary>
        /// Fortinet BGP network mode
        /// </summary>
        BgpNetwork,
        
        /// <summary>
        /// Fortinet BGP network edit mode
        /// </summary>
        BgpNetworkEdit,
        
        /// <summary>
        /// Fortinet router RIP mode
        /// </summary>
        RouterRipFortinet,
        
        /// <summary>
        /// Fortinet router static mode
        /// </summary>
        RouterStatic,
        
        /// <summary>
        /// Fortinet static route edit mode
        /// </summary>
        StaticRouteEdit,
        
        /// <summary>
        /// Fortinet firewall mode
        /// </summary>
        Firewall
    }
    
    /// <summary>
    /// Helper methods for DeviceMode enum
    /// </summary>
    public static class DeviceModeExtensions
    {
        /// <summary>
        /// Convert DeviceMode enum to string for display/compatibility
        /// </summary>
        public static string ToModeString(this DeviceMode mode)
        {
            return mode switch
            {
                DeviceMode.User => "user",
                DeviceMode.Privileged => "privileged",
                DeviceMode.Config => "config",
                DeviceMode.Interface => "interface",
                DeviceMode.Vlan => "vlan",
                DeviceMode.Router => "router",
                DeviceMode.RouterBgp => "bgp",
                DeviceMode.RouterOspf => "ospf",
                DeviceMode.RouterRip => "rip",
                DeviceMode.Acl => "acl",
                DeviceMode.Operational => "operational",
                DeviceMode.Configuration => "configuration",
                DeviceMode.Admin => "admin",
                DeviceMode.Global => "global",
                DeviceMode.GlobalConfig => "global_config",
                DeviceMode.SystemInterface => "system_if",
                DeviceMode.RouterOspfFortinet => "router_ospf",
                DeviceMode.RouterBgpFortinet => "router_bgp",
                DeviceMode.BgpNeighbor => "bgp_neighbor",
                DeviceMode.BgpNeighborEdit => "bgp_neighbor_edit",
                DeviceMode.BgpNetwork => "bgp_network",
                DeviceMode.BgpNetworkEdit => "bgp_network_edit",
                DeviceMode.RouterRipFortinet => "router_rip",
                DeviceMode.RouterStatic => "router_static",
                DeviceMode.StaticRouteEdit => "static_route_edit",
                DeviceMode.Firewall => "firewall",
                _ => "unknown"
            };
        }
        
        /// <summary>
        /// Convert string to DeviceMode enum
        /// </summary>
        public static DeviceMode FromModeString(string modeString)
        {
            return modeString switch
            {
                "user" => DeviceMode.User,
                "privileged" => DeviceMode.Privileged,
                "config" => DeviceMode.Config,
                "interface" => DeviceMode.Interface,
                "vlan" => DeviceMode.Vlan,
                "router" => DeviceMode.Router,
                "bgp" => DeviceMode.RouterBgp,
                "ospf" => DeviceMode.RouterOspf,
                "rip" => DeviceMode.RouterRip,
                "acl" => DeviceMode.Acl,
                "operational" => DeviceMode.Operational,
                "configuration" => DeviceMode.Configuration,
                "admin" => DeviceMode.Admin,
                "global" => DeviceMode.Global,
                "global_config" => DeviceMode.GlobalConfig,
                "system_if" => DeviceMode.SystemInterface,
                "router_ospf" => DeviceMode.RouterOspfFortinet,
                "router_bgp" => DeviceMode.RouterBgpFortinet,
                "bgp_neighbor" => DeviceMode.BgpNeighbor,
                "bgp_neighbor_edit" => DeviceMode.BgpNeighborEdit,
                "bgp_network" => DeviceMode.BgpNetwork,
                "bgp_network_edit" => DeviceMode.BgpNetworkEdit,
                "router_rip" => DeviceMode.RouterRipFortinet,
                "router_static" => DeviceMode.RouterStatic,
                "static_route_edit" => DeviceMode.StaticRouteEdit,
                "firewall" => DeviceMode.Firewall,
                _ => DeviceMode.User // Default fallback
            };
        }
        
        /// <summary>
        /// Check if mode is a router sub-mode
        /// </summary>
        public static bool IsRouterMode(this DeviceMode mode)
        {
            return mode == DeviceMode.Router || 
                   mode == DeviceMode.RouterBgp || 
                   mode == DeviceMode.RouterOspf || 
                   mode == DeviceMode.RouterRip ||
                   mode == DeviceMode.RouterOspfFortinet ||
                   mode == DeviceMode.RouterBgpFortinet ||
                   mode == DeviceMode.RouterRipFortinet ||
                   mode == DeviceMode.RouterStatic;
        }
        
        /// <summary>
        /// Check if mode is a configuration mode (allows config commands)
        /// </summary>
        public static bool IsConfigurationMode(this DeviceMode mode)
        {
            return mode == DeviceMode.Config ||
                   mode == DeviceMode.Interface ||
                   mode == DeviceMode.Vlan ||
                   mode == DeviceMode.Router ||
                   mode == DeviceMode.RouterBgp ||
                   mode == DeviceMode.RouterOspf ||
                   mode == DeviceMode.RouterRip ||
                   mode == DeviceMode.Acl ||
                   mode == DeviceMode.Configuration ||
                   mode == DeviceMode.GlobalConfig ||
                   mode == DeviceMode.SystemInterface ||
                   mode == DeviceMode.RouterOspfFortinet ||
                   mode == DeviceMode.RouterBgpFortinet ||
                   mode == DeviceMode.BgpNeighbor ||
                   mode == DeviceMode.BgpNeighborEdit ||
                   mode == DeviceMode.BgpNetwork ||
                   mode == DeviceMode.BgpNetworkEdit ||
                   mode == DeviceMode.RouterRipFortinet ||
                   mode == DeviceMode.RouterStatic ||
                   mode == DeviceMode.StaticRouteEdit ||
                   mode == DeviceMode.Firewall;
        }
    }
} 
