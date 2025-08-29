using NetForge.Interfaces.Cli;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.Common.CLI.Interfaces;

public interface ICliSubhandler
{
    CliResult Handle(ICliContext context);
}
