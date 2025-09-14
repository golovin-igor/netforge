using NetForge.Interfaces.CLI;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Formatters;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.CLI.Handlers
{
    /// <summary>
    /// Unified CLI handler that combines vendor-agnostic commands with vendor-specific formatting
    /// </summary>
    public class UnifiedCliHandler : BaseCliHandler
    {
        private readonly NetworkCommand _command;
        private readonly IVendorCommandFormatter _formatter;

        public UnifiedCliHandler(
            NetworkCommand command,
            IVendorCommandFormatter formatter)
            : base(command.CommandName, command.Description)
        {
            _command = command;
            _formatter = formatter;
        }

        protected override async Task<CliResult> ExecuteCommandAsync(ICliContext context)
        {
            try
            {
                // Extract command arguments (skip the command name itself)
                var args = context.CommandParts.Length > 1
                    ? context.CommandParts[1..]
                    : Array.Empty<string>();

                // Execute business logic
                var commandData = await _command.ExecuteAsync(context.Device, args);

                // Format output for vendor
                var formattedOutput = _formatter.Format(commandData);

                return commandData.Success
                    ? Success(formattedOutput)
                    : Error(CliErrorType.ExecutionError, formattedOutput);
            }
            catch (CommandExecutionException ex)
            {
                return Error(CliErrorType.ExecutionError, $"% {ex.Message}\n");
            }
            catch (Exception ex)
            {
                return Error(CliErrorType.ExecutionError, $"% Internal error: {ex.Message}\n");
            }
        }

        public override List<string> GetCompletions(ICliContext context)
        {
            try
            {
                // Get the partial argument (last part of the command)
                var command = string.Join(" ", context.CommandParts);
                var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var partial = parts.Length > 1 ? parts.Last() : string.Empty;

                return _command.GetCompletions(partial).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public override string GetHelp()
        {
            return _command.GetHelpText();
        }
    }

    /// <summary>
    /// Factory for creating unified CLI handlers
    /// </summary>
    public static class UnifiedCliHandlerFactory
    {
        /// <summary>
        /// Create a unified ping handler for a specific vendor
        /// </summary>
        public static UnifiedCliHandler CreatePingHandler(IPingService pingService, IVendorCommandFormatter formatter)
        {
            var pingCommand = new PingCommand(pingService);
            return new UnifiedCliHandler(pingCommand, formatter);
        }

        /// <summary>
        /// Create a unified ping handler with default formatter
        /// </summary>
        public static UnifiedCliHandler CreatePingHandler(IPingService pingService)
        {
            var pingCommand = new PingCommand(pingService);
            var defaultFormatter = new DefaultCommandFormatter();
            return new UnifiedCliHandler(pingCommand, defaultFormatter);
        }
    }

    /// <summary>
    /// Default command formatter for vendors without specific formatting
    /// </summary>
    public class DefaultCommandFormatter : BaseCommandFormatter
    {
        public override string VendorName => "Default";

        protected override string FormatPingResult(PingCommandData pingData)
        {
            var result = pingData.PingResult;
            var output = new System.Text.StringBuilder();

            if (!result.Success)
            {
                output.AppendLine($"% {result.ErrorMessage}");
                return output.ToString();
            }

            // Generic ping output format
            output.AppendLine($"PING {result.Destination} ({result.ResolvedAddress ?? result.Destination})");

            foreach (var reply in result.Replies)
            {
                if (reply.Success)
                {
                    output.AppendLine($"{reply.BytesReceived} bytes from {reply.FromAddress}: " +
                                     $"seq={reply.SequenceNumber} ttl={reply.Ttl} time={reply.RoundTripTime}ms");
                }
                else
                {
                    output.AppendLine($"Request timeout for seq {reply.SequenceNumber}");
                }
            }

            output.AppendLine();
            output.AppendLine($"--- {result.Destination} ping statistics ---");
            output.AppendLine($"{result.PacketsSent} packets transmitted, " +
                             $"{result.PacketsReceived} packets received, " +
                             $"{result.PacketLossPercentage:F1}% packet loss");

            if (result.PacketsReceived > 0)
            {
                output.AppendLine($"round-trip min/avg/max/stddev = " +
                                 $"{result.MinRoundTripTime}/{result.AvgRoundTripTime}/" +
                                 $"{result.MaxRoundTripTime}/{result.StandardDeviation:F3} ms");
            }

            return output.ToString();
        }
    }
}