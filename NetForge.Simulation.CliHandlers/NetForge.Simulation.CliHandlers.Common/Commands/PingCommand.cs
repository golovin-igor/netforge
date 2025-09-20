using NetForge.Interfaces.CLI;
using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.CLI.Services;

namespace NetForge.Simulation.Common.CLI.Commands
{
    /// <summary>
    /// Vendor-agnostic ping command that contains only business logic
    /// </summary>
    public class PingCommand : NetworkCommand
    {
        private readonly IPingService _pingService;

        public PingCommand(IPingService pingService)
        {
            _pingService = pingService;
        }

        public override string CommandName => "ping";
        public override string Description => "Send ping packets to test connectivity";

        protected override async Task<CommandData> ExecuteBusinessLogicAsync(INetworkDevice device, string[] args)
        {
            if (args.Length < 1)
            {
                throw new CommandExecutionException("Incomplete command. Usage: ping <destination>");
            }

            var destination = args[0];

            // Parse optional parameters
            var options = ParsePingOptions(args);

            // Execute ping using business service
            var pingResult = _pingService.ExecutePingWithOptions(device, options);

            return new PingCommandData(pingResult);
        }

        public override string GetHelpText()
        {
            return "Send ping packets to test connectivity\n" +
                   "Usage: ping <destination> [options]\n" +
                   "  destination  Target IP address or hostname\n" +
                   "Options (vendor-specific):\n" +
                   "  -c <count>   Number of packets to send\n" +
                   "  -s <size>    Packet size in bytes\n" +
                   "  -t <timeout> Timeout in seconds\n" +
                   "  -i <interval> Interval between packets";
        }

        public override IEnumerable<string> GetCompletions(string partial)
        {
            // Return common IP addresses and hostnames for completion
            var completions = new List<string>
            {
                "127.0.0.1",
                "192.168.1.1",
                "192.168.1.2",
                "10.0.0.1",
                "localhost",
                "router1",
                "router2",
                "switch1",
                "server1"
            };

            return completions.Where(c => c.StartsWith(partial, StringComparison.OrdinalIgnoreCase));
        }

        private PingOptions ParsePingOptions(string[] args)
        {
            var options = new PingOptions
            {
                Destination = args[0],
                PingCount = 5,
                PacketSize = 64,
                TimeoutSeconds = 2,
                Interval = 1000
            };

            // Parse additional arguments
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-c" when i + 1 < args.Length:
                        if (int.TryParse(args[i + 1], out int count))
                            options.PingCount = count;
                        i++;
                        break;

                    case "-s" when i + 1 < args.Length:
                        if (int.TryParse(args[i + 1], out int size))
                            options.PacketSize = size;
                        i++;
                        break;

                    case "-t" when i + 1 < args.Length:
                        if (int.TryParse(args[i + 1], out int timeout))
                            options.TimeoutSeconds = timeout;
                        i++;
                        break;

                    case "-i" when i + 1 < args.Length:
                        if (int.TryParse(args[i + 1], out int interval))
                            options.Interval = interval;
                        i++;
                        break;

                    case "-I" when i + 1 < args.Length:
                        options.SourceInterface = args[i + 1];
                        i++;
                        break;

                    case "-S" when i + 1 < args.Length:
                        options.SourceIpAddress = args[i + 1];
                        i++;
                        break;

                    case "-f":
                        options.DontFragment = true;
                        break;

                    case "-R":
                        options.RecordRoute = true;
                        break;

                    case "-T":
                        options.Timestamp = true;
                        break;

                    case "-v":
                        options.Verbose = true;
                        break;

                    case "-q":
                        options.QuietMode = true;
                        break;
                }
            }

            return options;
        }
    }

    /// <summary>
    /// Base class for vendor-agnostic network commands
    /// </summary>
    public abstract class NetworkCommand
    {
        public abstract string CommandName { get; }
        public abstract string Description { get; }

        protected abstract Task<CommandData> ExecuteBusinessLogicAsync(INetworkDevice device, string[] args);
        public abstract string GetHelpText();
        public abstract IEnumerable<string> GetCompletions(string partial);

        public async Task<CommandData> ExecuteAsync(INetworkDevice device, string[] args)
        {
            try
            {
                return await ExecuteBusinessLogicAsync(device, args);
            }
            catch (CommandExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CommandExecutionException($"Command execution failed: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Base class for command result data
    /// </summary>
    public abstract class CommandData
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Ping command result data
    /// </summary>
    public class PingCommandData : CommandData
    {
        public PingResultData PingResult { get; }

        public PingCommandData(PingResultData pingResult)
        {
            PingResult = pingResult;
            Success = pingResult.Success;
            ErrorMessage = pingResult.ErrorMessage;
        }
    }

    /// <summary>
    /// Exception for command execution errors
    /// </summary>
    public class CommandExecutionException : Exception
    {
        public CommandExecutionException(string message) : base(message) { }
        public CommandExecutionException(string message, Exception innerException) : base(message, innerException) { }
    }
}