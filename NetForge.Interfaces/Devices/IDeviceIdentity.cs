namespace NetForge.Interfaces.Devices;

/// <summary>
/// Defines the core identity properties of a network device.
/// This interface represents the fundamental identity aspects that all devices must have.
/// </summary>
public interface IDeviceIdentity
{
    /// <summary>
    /// Gets the unique name of the device within the network.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the vendor name of the device (e.g., "Cisco", "Juniper", "Arista").
    /// </summary>
    string Vendor { get; }

    /// <summary>
    /// Gets or sets the unique identifier for this device in the network.
    /// This ID is typically imported from the generated network configuration.
    /// </summary>
    string DeviceId { get; set; }

    /// <summary>
    /// Gets the device name for protocol service compatibility.
    /// </summary>
    string DeviceName { get; }

    /// <summary>
    /// Gets the device type for protocol service compatibility (e.g., "Router", "Switch", "Firewall").
    /// </summary>
    string DeviceType { get; }

    /// <summary>
    /// Gets the hostname of the device.
    /// </summary>
    string GetHostname();

    /// <summary>
    /// Sets the hostname of the device.
    /// </summary>
    /// <param name="hostname">The new hostname to set.</param>
    void SetHostname(string hostname);
}