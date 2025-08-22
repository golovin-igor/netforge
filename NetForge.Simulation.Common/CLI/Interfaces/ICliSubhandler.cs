using NetForge.Simulation.CliHandlers;

namespace NetForge.Simulation.Common.CLI.Interfaces;

public interface ICliSubhandler
{
    CliResult Handle(CliContext context);
}
