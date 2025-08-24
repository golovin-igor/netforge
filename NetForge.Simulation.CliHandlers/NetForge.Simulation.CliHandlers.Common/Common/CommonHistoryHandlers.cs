using NetForge.Simulation.Common.CLI.Base;

namespace NetForge.Simulation.CliHandlers.Common.Common
{
    /// <summary>
    /// Vendor-agnostic history recall command handler (!!)
    /// </summary>
    public class HistoryRecallCommandHandler : VendorAgnosticCliHandler
    {
        public HistoryRecallCommandHandler() : base("!!", "Recall last command")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            try
            {
                // Get command history from device
                var history = GetCommandHistory(context);
                
                if (history.Count == 0)
                {
                    return Error(CliErrorType.ExecutionError, 
                        "% No history available");
                }
                
                // Get the last command
                var lastCommand = history.LastOrDefault();
                
                if (string.IsNullOrEmpty(lastCommand))
                {
                    return Error(CliErrorType.ExecutionError, 
                        "% No command to recall");
                }
                
                // Process the recalled command
                var result = await context.Device.ProcessCommandAsync(lastCommand);
                
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError, 
                    $"% Error recalling command: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Vendor-agnostic numbered history recall command handler (!)
    /// </summary>
    public class HistoryNumberRecallCommandHandler : VendorAgnosticCliHandler
    {
        public HistoryNumberRecallCommandHandler() : base("!", "Recall numbered command from history")
        {
        }
        
        protected override async Task<CliResult> ExecuteCommandAsync(CliContext context)
        {
            if (context.CommandParts.Length < 2)
            {
                return Error(CliErrorType.IncompleteCommand, 
                    "% Incomplete command - need history number");
            }
            
            if (!int.TryParse(context.CommandParts[1], out int historyNumber))
            {
                return Error(CliErrorType.InvalidParameter, 
                    "% Invalid history number");
            }
            
            try
            {
                // Get command history from device
                var history = GetCommandHistory(context);
                
                if (historyNumber < 1 || historyNumber > history.Count)
                {
                    return Error(CliErrorType.InvalidParameter, 
                        "% History number out of range");
                }
                
                // Get the specified command by index (1-based to 0-based)
                var command = history[historyNumber - 1];
                
                if (string.IsNullOrEmpty(command))
                {
                    return Error(CliErrorType.InvalidParameter, 
                        "% Command not found in history");
                }
                
                // Process the recalled command
                var result = await context.Device.ProcessCommandAsync(command);
                
                return Success(result);
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError, 
                    $"% Error recalling command: {ex.Message}");
            }
        }
    }
} 
