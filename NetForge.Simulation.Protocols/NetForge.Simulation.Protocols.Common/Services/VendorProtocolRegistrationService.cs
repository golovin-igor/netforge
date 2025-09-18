using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces;
using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Services;

public interface IVendorProtocolRegistrationService
{
    Task RegisterProtocolsAsync(INetworkDevice device);
    Task RegisterProtocolAsync(INetworkDevice device, NetworkProtocolType protocolType);
    IEnumerable<NetworkProtocolType> GetSupportedProtocols(string vendor);
}

public class VendorProtocolRegistrationService : IVendorProtocolRegistrationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVendorRegistry _vendorRegistry;
    private readonly VendorBasedProtocolService _protocolService;

    public VendorProtocolRegistrationService(
        IServiceProvider serviceProvider,
        IVendorRegistry vendorRegistry,
        VendorBasedProtocolService protocolService)
    {
        _serviceProvider = serviceProvider;
        _vendorRegistry = vendorRegistry;
        _protocolService = protocolService;
    }

    public async Task RegisterProtocolsAsync(INetworkDevice device)
    {
        var deviceIdentity = device as IDeviceIdentity
            ?? throw new InvalidOperationException("Device must implement IDeviceIdentity");

        var protocolHost = device as IProtocolHost
            ?? throw new InvalidOperationException("Device must implement IProtocolHost");

        var vendor = deviceIdentity.Vendor;

        var supportedProtocols = GetSupportedProtocols(vendor);

        foreach (var protocolType in supportedProtocols)
        {
            await RegisterProtocolAsync(device, protocolType);
        }
    }

    public async Task RegisterProtocolAsync(INetworkDevice device, NetworkProtocolType protocolType)
    {
        var protocolHost = device as IProtocolHost
            ?? throw new InvalidOperationException("Device must implement IProtocolHost");

        // Check if protocol is already registered by checking if we can get it
        var existingProtocol = protocolHost.GetRegisteredProtocols()
            .FirstOrDefault(p => p.Type == protocolType);

        if (existingProtocol != null)
        {
            return; // Protocol already registered
        }

        var deviceIdentity = device as IDeviceIdentity
            ?? throw new InvalidOperationException("Device must implement IDeviceIdentity");

        var protocol = _protocolService.GetProtocol(protocolType, deviceIdentity.Vendor);
        if (protocol is IDeviceProtocol deviceProtocol)
        {
            protocolHost.RegisterProtocol(deviceProtocol);
        }

        // Make this async even though RegisterProtocol is sync to maintain interface consistency
        await Task.CompletedTask;
    }

    public IEnumerable<NetworkProtocolType> GetSupportedProtocols(string vendor)
    {
        var vendorDescriptor = _vendorRegistry.GetVendor(vendor);
        if (vendorDescriptor == null)
        {
            return Enumerable.Empty<NetworkProtocolType>();
        }

        return vendorDescriptor.SupportedProtocols.Select(p => p.ProtocolType);
    }
}