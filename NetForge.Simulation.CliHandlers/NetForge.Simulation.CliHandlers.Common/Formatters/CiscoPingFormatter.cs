using System.Text;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Services;

namespace NetForge.Simulation.Common.CLI.Formatters
{
    /// <summary>
    /// Cisco IOS-style ping command formatter
    /// </summary>
    public class CiscoPingFormatter : BaseCommandFormatter
    {
        public override string VendorName => "Cisco";

        protected override string FormatPingResult(PingCommandData pingData)
        {
            var result = pingData.PingResult;
            var output = new StringBuilder();

            if (!result.Success)
            {
                output.AppendLine($"% {result.ErrorMessage}");
                return output.ToString();
            }

            // Cisco IOS ping format
            output.AppendLine("Type escape sequence to abort.");
            output.AppendLine($"Sending {result.PacketsSent}, {result.PacketSize}-byte ICMP Echos to {result.Destination}, timeout is {result.TimeoutSeconds} seconds:");

            // Create Cisco-style ping response pattern
            var responsePattern = CreateCiscoResponsePattern(result);
            output.AppendLine(responsePattern);

            // Summary statistics in Cisco format
            if (result.PacketsReceived > 0)
            {
                var successRate = (result.PacketsReceived * 100) / result.PacketsSent;
                output.AppendLine($"Success rate is {successRate} percent ({result.PacketsReceived}/{result.PacketsSent}), round-trip min/avg/max = {result.MinRoundTripTime}/{result.AvgRoundTripTime}/{result.MaxRoundTripTime} ms");
            }
            else
            {
                output.AppendLine($"Success rate is 0 percent (0/{result.PacketsSent})");
            }

            return output.ToString();
        }

        /// <summary>
        /// Create Cisco-style response pattern (!, ., U, Q, M, etc.)
        /// </summary>
        private string CreateCiscoResponsePattern(PingResultData result)
        {
            var pattern = new StringBuilder();

            foreach (var reply in result.Replies)
            {
                if (reply.Success)
                {
                    pattern.Append('!'); // Success
                }
                else
                {
                    switch (reply.ErrorType?.ToLower())
                    {
                        case "timeout":
                            pattern.Append('.'); // Timeout
                            break;
                        case "unreachable":
                            pattern.Append('U'); // Destination unreachable
                            break;
                        case "quench":
                            pattern.Append('Q'); // Source quench
                            break;
                        case "mask":
                            pattern.Append('M'); // Could not fragment
                            break;
                        case "redirect":
                            pattern.Append('r'); // Redirect
                            break;
                        default:
                            pattern.Append('?'); // Unknown
                            break;
                    }
                }
            }

            return pattern.ToString();
        }

        /// <summary>
        /// Cisco-specific error message formatting
        /// </summary>
        protected override string FormatGenericResult(CommandData data)
        {
            if (!data.Success)
            {
                return $"% {data.ErrorMessage}\n";
            }

            return "% Command executed successfully\n";
        }
    }
}