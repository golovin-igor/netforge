using System.Text;
using System.Threading.Tasks;
using NetForge.Simulation.Common;
using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Alcatel.Basic
{
    /// <summary>
    /// Alcatel history command handler
    /// </summary>
    public class HistoryCommandHandler : VendorAgnosticCliHandler
    {
        public HistoryCommandHandler() : base("history", "Display command history")
        {
            AddAlias("hist");
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (!IsVendor(context, "Alcatel"))
            {
                return RequireVendor(context, "Alcatel");
            }
            
            var output = new StringBuilder();
            output.AppendLine("Command History:");
            output.AppendLine("  1  enable");
            output.AppendLine("  2  show version");
            output.AppendLine("  3  show system");
            output.AppendLine("  4  history");
            
            return Success(output.ToString());
        }
    }
}