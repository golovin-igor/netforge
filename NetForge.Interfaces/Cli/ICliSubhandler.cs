using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Interfaces.Cli
{

public interface ICliSubhandler
{
    CliResult Handle(ICliContext context);
}
}
