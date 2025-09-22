namespace NetForge.SimulationModel.Types;

public enum InterfaceType
{
    Ethernet,
    FastEthernet,
    GigabitEthernet,
    TenGigabitEthernet,
    Serial,
    Loopback,
    Vlan,
    Tunnel,
    Bridge,
    PortChannel
}

public enum InterfaceStatus
{
    NotPresent,
    Down,
    Up,
    Testing,
    Unknown,
    Dormant,
    LowerLayerDown
}

public enum DuplexMode
{
    Half,
    Full,
    Auto
}