using System.Text;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Services;

namespace NetForge.Simulation.Common.CLI.Formatters
{
    /// <summary>
    /// Cisco IOS-style show version formatter
    /// </summary>
    public class CiscoShowVersionFormatter : BaseCommandFormatter
    {
        public override string VendorName => "Cisco";

        protected override string FormatPingResult(PingCommandData pingData)
        {
            // Delegate to CiscoPingFormatter for ping results
            var pingFormatter = new CiscoPingFormatter();
            return pingFormatter.Format(pingData);
        }

        public override string Format(CommandData data)
        {
            return data switch
            {
                ShowVersionCommandData versionData => FormatCiscoShowVersion(versionData),
                PingCommandData pingData => FormatPingResult(pingData),
                _ => FormatGenericResult(data)
            };
        }

        public override bool CanFormat(Type dataType)
        {
            return dataType == typeof(ShowVersionCommandData) ||
                   dataType == typeof(PingCommandData) ||
                   dataType.IsSubclassOf(typeof(CommandData));
        }

        private string FormatCiscoShowVersion(ShowVersionCommandData versionData)
        {
            var version = versionData.VersionData;
            var output = new StringBuilder();

            if (!versionData.Success)
            {
                output.AppendLine($"% {versionData.ErrorMessage}");
                return output.ToString();
            }

            // Cisco IOS show version format
            output.AppendLine($"Cisco IOS Software, {version.Model} Software ({version.Model.Replace(" ", "_").ToUpper()}-LANBASEK9-M), Version {version.SoftwareVersion}, RELEASE SOFTWARE (fc2)");
            output.AppendLine($"Technical Support: http://www.cisco.com/techsupport");
            output.AppendLine($"Copyright (c) 1986-2016 by Cisco Systems, Inc.");
            output.AppendLine($"Compiled {version.BuildDate} by prod_rel_team");
            output.AppendLine();

            // System information
            output.AppendLine($"ROM: Bootstrap program is {version.Model} boot loader");
            output.AppendLine($"BOOTLDR: {version.Model} Boot Loader (C2960-HBOOT-M) Version 12.2(44)SE5, RELEASE SOFTWARE (fc1)");
            output.AppendLine();

            output.AppendLine($"{version.DeviceName} uptime is {FormatCiscoUptime(version.Uptime)}");
            output.AppendLine($"System returned to ROM by power-on");
            output.AppendLine($"System restarted at {version.SystemInfo.LastRestart:HH:mm:ss UTC ddd MMM d yyyy}");
            output.AppendLine($"System image file is \"{version.StorageInfo.BootFlash}{version.BootInfo.BootImage}\"");
            output.AppendLine();

            // Hardware and memory
            output.AppendLine();
            output.AppendLine($"This product contains cryptographic features and is subject to United");
            output.AppendLine($"States and local country laws governing import, export, transfer and");
            output.AppendLine($"use. Delivery of Cisco cryptographic products does not imply");
            output.AppendLine($"third-party authority to import, export, distribute or use encryption.");
            output.AppendLine($"Importers, exporters, distributors and users are responsible for");
            output.AppendLine($"compliance with U.S. and local country laws. By using this product you");
            output.AppendLine($"agree to comply with applicable laws and regulations. If you are unable");
            output.AppendLine($"to comply with U.S. and local laws, return this product immediately.");
            output.AppendLine();

            output.AppendLine($"Technology Package License Information for Module:'{version.DeviceName}'");
            output.AppendLine();
            output.AppendLine($"cisco {version.HardwareInfo.ChassisType} ({version.HardwareInfo.ProcessorType}) processor (revision A0) with {FormatKBytes(version.MemoryInfo.ProcessorMemory)}/{FormatKBytes(version.MemoryInfo.IOMemory)} bytes of memory.");
            output.AppendLine($"Processor board ID {version.SerialNumber}");
            output.AppendLine($"Last reset from power-on");
            output.AppendLine($"{version.HardwareInfo.InterfaceCount} FastEthernet interfaces");
            output.AppendLine($"The password-recovery mechanism is enabled.");
            output.AppendLine();

            output.AppendLine($"64K bytes of flash-simulated non-volatile configuration memory.");
            output.AppendLine($"Base ethernet MAC Address       : {version.HardwareInfo.BaseMacAddress}");
            output.AppendLine($"Motherboard assembly number     : {version.HardwareInfo.BoardInfo.GetValueOrDefault("Part Number", "73-XXXXX-XX")}");
            output.AppendLine($"Power supply part number         : 341-0097-02");
            output.AppendLine($"Motherboard serial number       : {version.HardwareInfo.BoardInfo.GetValueOrDefault("Serial Number", version.SerialNumber)}");
            output.AppendLine($"Power supply serial number       : AZS{version.SerialNumber[3..]}");
            output.AppendLine($"Model revision number            : {version.HardwareInfo.BoardInfo.GetValueOrDefault("Revision", "A0")}");
            output.AppendLine($"Motherboard revision number     : A0");
            output.AppendLine($"Model number                     : {version.Model}");
            output.AppendLine($"System serial number             : {version.SerialNumber}");
            output.AppendLine($"Top Assembly Part Number         : 800-32797-02");
            output.AppendLine($"Top Assembly Revision Number     : A0");
            output.AppendLine($"Version ID                       : V02");
            output.AppendLine($"CLEI Code Number                 : COM3L00BRA");
            output.AppendLine($"Hardware Board Revision Number   : 0x01");
            output.AppendLine();

            // Switch ports
            output.AppendLine("Switch Ports Model              SW Version            SW Image");
            output.AppendLine("------ ----- -----              ----------            --------");
            output.AppendLine($"*    1 {version.HardwareInfo.InterfaceCount,2}    {version.Model,-20} {version.SoftwareVersion,-17} {version.BootInfo.BootImage}");
            output.AppendLine();

            // Configuration register
            output.AppendLine($"Configuration register is {version.StorageInfo.ConfigRegister}");

            return output.ToString();
        }

        private string FormatCiscoUptime(TimeSpan uptime)
        {
            var parts = new List<string>();

            if (uptime.Days > 0)
                parts.Add($"{uptime.Days} day{(uptime.Days != 1 ? "s" : "")}");

            if (uptime.Hours > 0)
                parts.Add($"{uptime.Hours} hour{(uptime.Hours != 1 ? "s" : "")}");

            if (uptime.Minutes > 0)
                parts.Add($"{uptime.Minutes} minute{(uptime.Minutes != 1 ? "s" : "")}");

            return string.Join(", ", parts);
        }

        private string FormatKBytes(long bytes)
        {
            return $"{bytes / 1024}K";
        }
    }
}