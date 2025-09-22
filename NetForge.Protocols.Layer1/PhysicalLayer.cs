using NetForge.SimulationModel.Core;
using NetForge.SimulationModel.Devices;
using NetForge.SimulationModel.Events;
using NetForge.SimulationModel.Types;

namespace NetForge.Protocols.Layer1;

public class PhysicalLayer : IPhysicalLayer
{
    private readonly INetworkDevice _device;
    private readonly IEventBus _eventBus;
    private readonly Dictionary<string, PhysicalInterface> _interfaces;

    public PhysicalLayer(INetworkDevice device, IEventBus eventBus)
    {
        _device = device;
        _eventBus = eventBus;
        _interfaces = new Dictionary<string, PhysicalInterface>();
    }

    public void TransmitBits(byte[] data, string interfaceId)
    {
        if (!_interfaces.TryGetValue(interfaceId, out var physicalInterface))
        {
            throw new InvalidOperationException($"Interface {interfaceId} not found");
        }

        if (physicalInterface.Status != InterfaceState.Up)
        {
            return; // Cannot transmit on a down interface
        }

        // Simulate physical transmission
        var frame = new EthernetFrame
        {
            Data = data,
            InterfaceId = interfaceId,
            Timestamp = DateTime.UtcNow
        };

        _eventBus.Publish(new FrameTransmittedEvent
        {
            DeviceId = _device.Id,
            InterfaceId = interfaceId,
            Frame = frame,
            Timestamp = DateTime.UtcNow
        });
    }

    public void ReceiveBits(byte[] data, string interfaceId)
    {
        if (!_interfaces.TryGetValue(interfaceId, out var physicalInterface))
        {
            return; // Silently drop if interface doesn't exist
        }

        if (physicalInterface.Status != InterfaceState.Up)
        {
            return; // Cannot receive on a down interface
        }

        physicalInterface.UpdateStatistics(data.Length);

        // Pass up to Layer 2
        _eventBus.Publish(new FrameReceivedEvent
        {
            DeviceId = _device.Id,
            InterfaceId = interfaceId,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
    }

    public void SetInterfaceState(string interfaceId, InterfaceState status)
    {
        if (_interfaces.TryGetValue(interfaceId, out var physicalInterface))
        {
            var previousStatus = physicalInterface.Status;
            physicalInterface.Status = status;

            if (previousStatus != status)
            {
                _eventBus.Publish(new InterfaceStateChangedEvent
                {
                    DeviceId = _device.Id,
                    InterfaceId = interfaceId,
                    PreviousStatus = previousStatus,
                    NewStatus = status,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    public InterfaceState GetInterfaceState(string interfaceId)
    {
        return _interfaces.TryGetValue(interfaceId, out var physicalInterface)
            ? physicalInterface.Status
            : InterfaceState.NotPresent;
    }

    public void AddInterface(string interfaceId, InterfaceType type)
    {
        if (!_interfaces.ContainsKey(interfaceId))
        {
            _interfaces[interfaceId] = new PhysicalInterface
            {
                Id = interfaceId,
                Type = type,
                Status = InterfaceState.Down,
                Speed = GetDefaultSpeed(type),
                Duplex = DuplexMode.Full
            };
        }
    }

    public void RemoveInterface(string interfaceId)
    {
        _interfaces.Remove(interfaceId);
    }

    private long GetDefaultSpeed(InterfaceType type)
    {
        return type switch
        {
            InterfaceType.FastEthernet => 100_000_000,     // 100 Mbps
            InterfaceType.GigabitEthernet => 1_000_000_000, // 1 Gbps
            InterfaceType.TenGigabitEthernet => 10_000_000_000, // 10 Gbps
            _ => 10_000_000 // 10 Mbps default
        };
    }
}
