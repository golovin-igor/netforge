using System.Text;
using NetForge.Simulation.Common.CLI.Commands;
using NetForge.Simulation.Common.CLI.Services;

namespace NetForge.Simulation.Common.CLI.Formatters
{
    /// <summary>
    /// F5 BIG-IP-style ping command formatter
    /// </summary>
    public class F5PingFormatter : BaseCommandFormatter
    {
        public override string VendorName => "F5";

        protected override string FormatPingResult(PingCommandData pingData)
        {
            var result = pingData.PingResult;
            var output = new StringBuilder();

            if (!result.Success)
            {
                output.AppendLine($"ping: {result.ErrorMessage}");
                return output.ToString();
            }

            // F5 BIG-IP ping format header
            output.AppendLine($"PING {result.Destination}: {result.PacketSize} data bytes");

            // Individual ping replies in F5 format
            foreach (var reply in result.Replies)
            {
                if (reply.Success)
                {
                    output.AppendLine($"{reply.BytesReceived} bytes from {reply.FromAddress}: icmp_seq={reply.SequenceNumber}. time={reply.RoundTripTime}. ms");
                }
                else
                {
                    output.AppendLine($"Request timeout for icmp_seq {reply.SequenceNumber}");
                }
            }

            // F5 summary statistics
            output.AppendLine();
            output.AppendLine($"----{result.Destination} PING Statistics----");
            output.AppendLine($"{result.PacketsSent} packets transmitted, {result.PacketsReceived} packets received, {result.PacketLossPercentage:F0}% packet loss");

            if (result.PacketsReceived > 0)
            {
                output.AppendLine($"round-trip (ms)  min/avg/max/stddev = {result.MinRoundTripTime:F3}/{result.AvgRoundTripTime:F3}/{result.MaxRoundTripTime:F3}/{result.StandardDeviation:F3}");
            }

            return output.ToString();
        }

        /// <summary>
        /// F5-specific error message formatting
        /// </summary>
        protected override string FormatGenericResult(CommandData data)
        {
            if (!data.Success)
            {
                return $"ping: {data.ErrorMessage}\n";
            }

            return string.Empty; // F5 typically doesn't show success messages for commands
        }
    }
}