using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Cisco.Configuration;

public class SwitchportModeDynamicHandler : VendorAgnosticCliHandler
{
    public SwitchportModeDynamicHandler() : base("dynamic", "Set switchport to dynamic mode")
    {
        AddSubHandler("auto", new SwitchportModeDynamicAutoHandler());
        AddSubHandler("desirable", new SwitchportModeDynamicDesirableHandler());
    }

    protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
    {
        return Error(CliErrorType.IncompleteCommand,
            "% Incomplete command. Available options: auto, desirable");
    }
}
