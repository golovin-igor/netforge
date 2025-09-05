using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Interfaces.CLI;

public interface ICliSubhandler
{
    CliResult Handle(ICliContext context);
}
