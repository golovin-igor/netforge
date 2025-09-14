using System.Text;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Services;

namespace NetForge.Simulation.Common.CLI.Formatters
{
    /// <summary>
    /// Juniper JunOS-style ping command formatter
    /// </summary>
    public class JuniperPingFormatter : BaseCommandFormatter
    {
        public override string VendorName => "Juniper";

        protected override string FormatPingResult(PingCommandData pingData)
        {
            var result = pingData.PingResult;
            var output = new StringBuilder();

            if (!result.Success)
            {
                output.AppendLine($"ping: {result.ErrorMessage}");
                return output.ToString();
            }

            // JunOS ping format header
            output.AppendLine($"PING {result.Destination} ({result.ResolvedAddress ?? result.Destination}): {result.PacketSize} data bytes");

            // Individual ping replies in JunOS format
            foreach (var reply in result.Replies)
            {
                if (reply.Success)
                {
                    output.AppendLine($"{reply.BytesReceived} bytes from {reply.FromAddress}: icmp_seq={reply.SequenceNumber} ttl={reply.Ttl} time={reply.RoundTripTime}.{GetSubMilliseconds(reply.RoundTripTime)} ms");
                }
                else
                {
                    var errorMsg = reply.ErrorType?.ToLower() switch
                    {
                        "timeout" => "timeout",
                        "unreachable" => "Destination Host Unreachable",
                        _ => "Request timeout"
                    };
                    output.AppendLine($"Request timeout for icmp_seq {reply.SequenceNumber}");
                }
            }

            // JunOS summary statistics
            output.AppendLine();
            output.AppendLine($"--- {result.Destination} ping statistics ---");
            output.AppendLine($"{result.PacketsSent} packets transmitted, {result.PacketsReceived} packets received, {result.PacketLossPercentage:F1}% packet loss");

            if (result.PacketsReceived > 0)
            {
                output.AppendLine($"round-trip min/avg/max/stddev = {result.MinRoundTripTime}.{GetSubMilliseconds(result.MinRoundTripTime)}/{FormatJunosRtt(result.AvgRoundTripTime)}/{result.MaxRoundTripTime}.{GetSubMilliseconds(result.MaxRoundTripTime)}/{result.StandardDeviation:F3} ms");
            }

            return output.ToString();
        }

        /// <summary>
        /// Generate realistic sub-millisecond values for JunOS format
        /// </summary>
        private int GetSubMilliseconds(int rtt)
        {
            // Generate consistent sub-millisecond part based on RTT
            return (rtt * 123) % 1000;
        }

        /// <summary>
        /// Format RTT in JunOS style with sub-millisecond precision
        /// </summary>
        private string FormatJunosRtt(int avgRtt)
        {
            var subMs = GetSubMilliseconds(avgRtt);
            return $"{avgRtt}.{subMs:D3}";
        }

        /// <summary>
        /// JunOS-specific error message formatting
        /// </summary>
        protected override string FormatGenericResult(CommandData data)
        {
            if (!data.Success)
            {
                return $"ping: {data.ErrorMessage}\n";
            }

            return string.Empty; // JunOS typically doesn't show success messages for commands
        }
    }
}