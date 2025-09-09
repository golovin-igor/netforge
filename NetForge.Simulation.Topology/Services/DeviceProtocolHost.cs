using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common.Services;

namespace NetForge.Simulation.Topology.Services;

/// <summary>
/// Manages protocol registration, lifecycle, and coordination for network devices.
/// This service extracts protocol management responsibilities from NetworkDevice.
/// </summary>
public class DeviceProtocolHost : IProtocolHost
{
    private readonly List<IDeviceProtocol> _protocols = [];
    private readonly INetworkDevice _device;
    private IProtocolService? _protocolService;

    public DeviceProtocolHost(INetworkDevice device)
    {
        _device = device ?? throw new ArgumentNullException(nameof(device));
    }

    /// <summary>
    /// Gets the enhanced protocol service for this device.
    /// </summary>
    public IProtocolService GetProtocolService()
    {
        if (_protocolService == null)
        {
            _protocolService = new NetworkDeviceProtocolService(_device);
        }
        return _protocolService;
    }

    /// <summary>
    /// Registers a network protocol implementation with the device.
    /// </summary>
    public void RegisterProtocol(IDeviceProtocol protocol)
    {
        if (protocol == null)
            throw new ArgumentNullException(nameof(protocol));

        if (!_protocols.Any(p => p.GetType() == protocol.GetType()))
        {
            _protocols.Add(protocol);
            
            // Initialize the protocol if the device is already in a network
            if (_device.ParentNetwork != null)
            {
                protocol.Initialize(_device);
            }
        }
    }

    /// <summary>
    /// Unregisters a network protocol from the device.
    /// </summary>
    public bool UnregisterProtocol(IDeviceProtocol protocol)
    {
        if (protocol == null)
            return false;

        return _protocols.Remove(protocol);
    }

    /// <summary>
    /// Unregisters a protocol by type.
    /// </summary>
    public bool UnregisterProtocol<T>() where T : IDeviceProtocol
    {
        var protocol = _protocols.FirstOrDefault(p => p is T);
        if (protocol != null)
        {
            return UnregisterProtocol(protocol);
        }
        return false;
    }

    /// <summary>
    /// Updates the state of all registered network protocols.
    /// </summary>
    public async Task UpdateAllProtocolStates()
    {
        var tasks = _protocols.Select(p => UpdateProtocolStateAsync(p)).ToList();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Updates a single protocol state with error handling.
    /// </summary>
    private async Task UpdateProtocolStateAsync(IDeviceProtocol protocol)
    {
        try
        {
            await protocol.UpdateState(_device);
        }
        catch (Exception ex)
        {
            // Log the error but don't let one protocol failure affect others
            Console.WriteLine($"Error updating protocol {protocol.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Subscribe all registered protocols to events.
    /// This should be called when a device is added to a network.
    /// </summary>
    public void SubscribeProtocolsToEvents()
    {
        foreach (var protocol in _protocols)
        {
            try
            {
                protocol.Initialize(_device);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subscribing protocol {protocol.GetType().Name} to events: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets all registered network protocols.
    /// </summary>
    public IReadOnlyList<IDeviceProtocol> GetRegisteredProtocols()
    {
        return _protocols.AsReadOnly();
    }

    /// <summary>
    /// Gets a specific protocol by type.
    /// </summary>
    public T? GetProtocol<T>() where T : IDeviceProtocol
    {
        return _protocols.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Checks if a specific protocol type is registered.
    /// </summary>
    public bool IsProtocolRegistered<T>() where T : IDeviceProtocol
    {
        return _protocols.Any(p => p is T);
    }

    /// <summary>
    /// Initializes all registered protocols.
    /// </summary>
    public void InitializeAllProtocols()
    {
        foreach (var protocol in _protocols)
        {
            try
            {
                protocol.Initialize(_device);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing protocol {protocol.GetType().Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Disposes all registered protocols.
    /// </summary>
    public void DisposeAllProtocols()
    {
        // Simply clear the protocols list as IDeviceProtocol doesn't have Dispose method
        _protocols.Clear();
    }

    /// <summary>
    /// Gets the count of registered protocols.
    /// </summary>
    public int ProtocolCount => _protocols.Count;
}