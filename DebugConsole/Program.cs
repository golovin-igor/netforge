using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetSim.Simulation.Common;
using NetSim.Simulation.Devices;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== NetSim Fortinet Debug Tool ===");
        
        try
        {
            // Create FortinetDevice like in the failing test
            var r1 = new FortinetDevice("R1");
            Console.WriteLine($"Device: {r1.Name} ({r1.Vendor})");
            
            // Capture device logs
            var deviceLogs = new List<string>();
            r1.LogEntryAdded += log => deviceLogs.Add(log);
            
            // Commands from the failing test
            string[] commands = {
                "config system interface",
                "edit port1", 
                "set ip 192.168.1.1 255.255.255.0",
                "set allowaccess ping",
                "next",
                "end"
            };
            
            Console.WriteLine("\n--- Command Execution Results ---");
            
            // Execute each command and capture output
            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                var output = await r1.ProcessCommandAsync(command);
                
                Console.WriteLine($"{i+1}. '{command}'");
                Console.WriteLine($"   Output: \"{output?.Trim()}\"");
                
                if (string.IsNullOrEmpty(output))
                {
                    Console.WriteLine("   WARNING: Empty output - this may cause test failures");
                }
            }
            
            // Check interface configuration after commands
            Console.WriteLine("\n--- Interface Configuration Check ---");
            Console.WriteLine($"Current Interface: '{r1.GetCurrentInterface()}'");
            var port1Interface = r1.GetInterface("port1");
            if (port1Interface != null)
            {
                Console.WriteLine($"Port1 IP: {port1Interface.IpAddress ?? "NOT SET"}");
                Console.WriteLine($"Port1 Mask: {port1Interface.SubnetMask ?? "NOT SET"}");
                Console.WriteLine($"Port1 Status: {(port1Interface.IsUp ? "UP" : "DOWN")}");
            }
            else
            {
                Console.WriteLine("Port1 interface not found!");
            }
            
            // Test critical show commands that tests depend on
            Console.WriteLine("\n--- Critical Show Commands ---");
            
            try
            {
                var ospfOutput = await r1.ProcessCommandAsync("get router info ospf neighbor");
                Console.WriteLine($"OSPF neighbors: \"{ospfOutput?.Trim()}\"");
                
                if (ospfOutput?.Contains("192.168.1.2") == true)
                {
                    Console.WriteLine("✓ OSPF output contains expected IP (192.168.1.2)");
                }
                else
                {
                    Console.WriteLine("✗ OSPF output missing expected IP (192.168.1.2)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OSPF command error: {ex.Message}");
            }
            
            // Show device logs
            Console.WriteLine("\n--- Device Debug Logs ---");
            foreach (var log in deviceLogs)
            {
                Console.WriteLine($"LOG: {log}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}