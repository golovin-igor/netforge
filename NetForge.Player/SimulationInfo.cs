using System.Reflection;
using NetForge.Simulation.Common;
using NetForge.Simulation.Interfaces;
using NetForge.Simulation.Protocols.Common;

namespace NetForge.Player;

public static class SimulationInfo
{
    public class DummyNetworkDevice() : NetworkDevice("DummyDevice")
    {
        protected override void InitializeDefaultInterfaces()
        {
        }

        public override string GetPrompt()
        {
            return "DummyDevice>";
        }

        protected override void RegisterDeviceSpecificHandlers()
        {
        }
    }

    public static void Print()
    {
        Console.WriteLine("Capabilities:");
        Console.ForegroundColor = ConsoleColor.Green;
        
        var vendors = ProgressIndicator.WithProgress("Scanning vendors", GetVendors);
        var protocols = ProgressIndicator.WithProgress("Scanning protocols", GetSimulatedProtocols);
        
        Console.WriteLine($"Vendors:\n{vendors}\nProtocols:\n{protocols}\n");
        Console.ResetColor();

    }

    private static string GetSimulatedProtocols()
    {
        //scan the current folder and load all assemblies that match the pattern NetForge.Simulation.Protocols.*
        //then scan each assembly for types that implement IDeviceProtocol

        var protocolAssemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "NetForge.Simulation.Protocols.*.dll");
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
