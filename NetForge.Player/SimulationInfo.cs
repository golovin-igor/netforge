// TODO: Enhance SimulationInfo for comprehensive capability reporting
// - Add version information display
// - Include build timestamp and commit hash
// - Show loaded plugin versions
// - Display configuration status
// - Add performance metrics summary
// - Include system requirements check
// - Show available network interfaces for external connectivity
// - Display licensing and usage information

using System.Reflection;
using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Vendors;

namespace NetForge.Player;

public static class SimulationInfo
{
    public static void Print()
    {
        // TODO: Expand capability reporting with additional information
        // - System information (OS, .NET version, available memory)
        // - Network interface discovery for external connectivity
        // - Plugin health checks and validation
        // - Configuration file location and status
        // - Performance benchmarks summary
        // - Security features status
        // - Available terminal server ports and protocols

        Console.WriteLine("Capabilities:");
        Console.ForegroundColor = ConsoleColor.Green;

        var vendors = ProgressIndicator.WithProgress("Scanning vendors", GetVendors);
        var protocols = ProgressIndicator.WithProgress("Scanning protocols", GetSimulatedProtocols);

        // TODO: Add additional capability scanning
        // - Scan available terminal servers
        // - Check external connectivity prerequisites
        // - Validate configuration files
        // - Test network bridge capabilities
        // - Check administrative privileges for virtual interfaces

        Console.WriteLine($"Vendors:\n{vendors}\nProtocols:\n{protocols}\n");
        Console.ResetColor();

    }

    private static string GetSimulatedProtocols()
    {
        //scan the current folder and load all assemblies that match the pattern NetForge.Simulation.Core.Protocols.*
        //then scan each assembly for types that implement IDeviceProtocol

        var protocolAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "NetForge.Simulation.Core.Protocols.*.dll");
        var protocolList = new List<string>();

        foreach (var assemblyPath in protocolAssemblies)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(IDeviceProtocol).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
                    {
                        //create an instance of the protocol to get its name
                        var protocolInstance = (IDeviceProtocol)Activator.CreateInstance(type)!;
                        protocolList.Add(protocolInstance.Type.ToString());
                    }
                }
            }
            catch
            {
                // Ignore assemblies that cannot be loaded or scanned
            }
        }


        return string.Join(", ", protocolList);
    }

    private static string GetVendors()
    {
        //scan the current folder and load all assemblies that match the pattern NetForge.Simulation.CliHandlers.*.dll
        //find types that implement IVendorContext and get their VendorName

        var vendorAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "NetForge.Simulation.CliHandlers.*.dll");
        var vendorList = new List<string>();

        foreach (var assemblyPath in vendorAssemblies)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(IVendorContext).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
                    {
                        //create an instance of the vendor context to get its name
                        var vendorInstance = (IVendorContext)Activator.CreateInstance(type, new DummyNetworkDevice())!;
                        vendorList.Add(vendorInstance.VendorName);
                    }
                }
            }
            catch
            {
                // Ignore assemblies that cannot be loaded or scanned
            }
        }

        return string.Join(", ", vendorList);
    }



}
