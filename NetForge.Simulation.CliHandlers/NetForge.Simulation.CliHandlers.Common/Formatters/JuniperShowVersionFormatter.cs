using System.Text;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Services;

namespace NetForge.Simulation.Common.CLI.Formatters
{
    /// <summary>
    /// Juniper JunOS-style show version formatter
    /// </summary>
    public class JuniperShowVersionFormatter : BaseCommandFormatter
    {
        public override string VendorName => "Juniper";

        protected override string FormatPingResult(PingCommandData pingData)
        {
            // Delegate to JuniperPingFormatter for ping results
            var pingFormatter = new JuniperPingFormatter();
            return pingFormatter.Format(pingData);
        }

        public override string Format(CommandData data)
        {
            return data switch
            {
                ShowVersionCommandData versionData => FormatJuniperShowVersion(versionData),
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

        private string FormatJuniperShowVersion(ShowVersionCommandData versionData)
        {
            var version = versionData.VersionData;
            var output = new StringBuilder();

            if (!versionData.Success)
            {
                output.AppendLine($"{versionData.ErrorMessage}");
                return output.ToString();
            }

            // JunOS show version format
            output.AppendLine($"Hostname: {version.DeviceName}");
            output.AppendLine($"Model: {version.Model}");
            output.AppendLine($"Junos: {version.SoftwareVersion}");
            output.AppendLine($"JUNOS Software Release [{version.SoftwareVersion}] (Export edition)");
            output.AppendLine();

            // Chassis information
            output.AppendLine("{master:0}");
            output.AppendLine($"fpc0:");
            output.AppendLine($"  CPU utilization          :    2 percent (5 sec avg)");
            output.AppendLine($"  CPU utilization          :    1 percent (1 min avg)");
            output.AppendLine($"  CPU utilization          :    1 percent (5 min avg)");
            output.AppendLine($"  Memory utilization       :   {version.MemoryInfo.UsedPercentage:F0} percent");
            output.AppendLine($"  Total CPU DRAM installed :    {FormatMB(version.MemoryInfo.TotalMemory)} MB");
            output.AppendLine($"  Memory utilization       :   {version.MemoryInfo.UsedPercentage:F0} percent");
            output.AppendLine($"  Total memory             :    {FormatMB(version.MemoryInfo.TotalMemory)} MB Max");
            output.AppendLine($"  Reserved memory          :    {FormatMB(version.MemoryInfo.ProcessorMemory)} MB");
            output.AppendLine($"  Available memory         :    {FormatMB(version.MemoryInfo.AvailableMemory)} MB");
            output.AppendLine();

            // Chassis hardware details
            output.AppendLine("Chassis                                 Serial No.    Part No.");
            output.AppendLine("Midplane                                REV 06        {version.SerialNumber[..8]}");
            output.AppendLine($"FPC 0            {version.Model,-24} REV 09        {version.SerialNumber}         {version.HardwareInfo.BoardInfo.GetValueOrDefault("Part Number", "750-XXXXX")}");
            output.AppendLine($"  CPU                                   REV 07        BUILTIN           BUILTIN");
            output.AppendLine($"  MIC 0          {version.HardwareInfo.InterfaceCount/2}-port 10/100/1000 Base-T             REV 04        {GenerateMicSerial(version.SerialNumber)}         750-XXXXX");
            output.AppendLine($"  MIC 1          {version.HardwareInfo.InterfaceCount/2}-port 10/100/1000 Base-T             REV 04        {GenerateMicSerial(version.SerialNumber)}         750-XXXXX");
            output.AppendLine($"  PIC 0          {version.HardwareInfo.InterfaceCount/4}-port 10/100/1000 Base-T             REV 05        BUILTIN           BUILTIN");
            output.AppendLine($"  PIC 1          {version.HardwareInfo.InterfaceCount/4}-port 10/100/1000 Base-T             REV 05        BUILTIN           BUILTIN");
            output.AppendLine($"  PIC 2          {version.HardwareInfo.InterfaceCount/4}-port 10/100/1000 Base-T             REV 05        BUILTIN           BUILTIN");
            output.AppendLine($"  PIC 3          {version.HardwareInfo.InterfaceCount/4}-port 10/100/1000 Base-T             REV 05        BUILTIN           BUILTIN");
            output.AppendLine($"Routing Engine 0                        REV 07        {version.SerialNumber}         {version.HardwareInfo.BoardInfo.GetValueOrDefault("Part Number", "750-XXXXX")}");
            output.AppendLine($"CB 0             {version.Model} Control Board      REV 09        BUILTIN           BUILTIN");
            output.AppendLine();

            // Software information
            output.AppendLine("JUNOS Software suite [{version.SoftwareVersion}]");
            output.AppendLine("  JUNOS Base OS boot [{version.SoftwareVersion}]");
            output.AppendLine("  JUNOS Base OS Software suite [{version.SoftwareVersion}]");
            output.AppendLine("  JUNOS Kernel Software suite [{version.SoftwareVersion}]");
            output.AppendLine("  JUNOS Crypto Software suite [{version.SoftwareVersion}]");
            output.AppendLine("  JUNOS Packet Forwarding Engine Support (EX Series) [{version.SoftwareVersion}]");
            output.AppendLine("  JUNOS Routing Software suite [{version.SoftwareVersion}]");
            output.AppendLine("  JUNOS Enterprise Software suite [{version.SoftwareVersion}]");
            output.AppendLine("  JUNOS Web Management Platform Package [{version.SoftwareVersion}]");
            output.AppendLine();

            // System uptime
            var totalMinutes = (int)version.Uptime.TotalMinutes;
            var days = totalMinutes / (24 * 60);
            var hours = (totalMinutes % (24 * 60)) / 60;
            var minutes = totalMinutes % 60;

            if (days > 0)
                output.AppendLine($"{days} day{(days != 1 ? "s" : "")}, {hours:D2}:{minutes:D2}");
            else
                output.AppendLine($"{hours:D2}:{minutes:D2}");

            return output.ToString();
        }

        private string FormatMB(long bytes)
        {
            return $"{bytes / 1024 / 1024}";
        }

        private string GenerateMicSerial(string baseSerial)
        {
            // Generate a related but different serial for MIC cards
            var hash = baseSerial.GetHashCode();
            return $"AD{Math.Abs(hash % 999999):D6}";
        }
    }
}