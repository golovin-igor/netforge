using System.Text;
using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Alcatel.Show;

public class ShowVersionSubhandler(ShowCommandHandler parent) : ICliSubhandler
{
    public CliResult Handle(ICliContext context)
    {
        var device = context.Device;
        var output = new StringBuilder();

        output.AppendLine("Alcatel-Lucent Operating System Software");
        output.AppendLine($"System Name: {device?.Name}");
        output.AppendLine($"Vendor: {device?.Vendor}");
        output.AppendLine($"Model: OmniSwitch 6850");
        output.AppendLine($"Software Version: AOS Release 8.9.221.R01");
        output.AppendLine($"Build Date: {DateTime.Now:MMM dd yyyy HH:mm:ss}");
        output.AppendLine($"System uptime: {parent.GetSystemUptime()}");
        output.AppendLine($"CPU utilization: 8%");
        output.AppendLine($"Memory utilization: 22%");
        output.AppendLine();

        return CliResult.Ok(output.ToString());
    }
}
