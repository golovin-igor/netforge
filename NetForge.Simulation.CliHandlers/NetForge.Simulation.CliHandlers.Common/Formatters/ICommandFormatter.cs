using NetForge.Simulation.Common.CLI.Commands;

namespace NetForge.Simulation.Common.CLI.Formatters
{
    /// <summary>
    /// Interface for vendor-specific command output formatting
    /// </summary>
    public interface IVendorCommandFormatter
    {
        /// <summary>
        /// The vendor this formatter supports
        /// </summary>
        string VendorName { get; }

        /// <summary>
        /// Format command result data into vendor-specific output
        /// </summary>
        /// <param name="data">The command result data</param>
        /// <returns>Formatted output string</returns>
        string Format(CommandData data);

        /// <summary>
        /// Check if this formatter can handle the given command data type
        /// </summary>
        /// <param name="dataType">The command data type</param>
        /// <returns>True if this formatter can handle the data type</returns>
        bool CanFormat(Type dataType);
    }

    /// <summary>
    /// Base implementation of command formatter
    /// </summary>
    public abstract class BaseCommandFormatter : IVendorCommandFormatter
    {
        public abstract string VendorName { get; }

        public virtual string Format(CommandData data)
        {
            return data switch
            {
                PingCommandData pingData => FormatPingResult(pingData),
                _ => FormatGenericResult(data)
            };
        }

        public virtual bool CanFormat(Type dataType)
        {
            return dataType == typeof(PingCommandData) ||
                   dataType.IsSubclassOf(typeof(CommandData));
        }

        protected abstract string FormatPingResult(PingCommandData pingData);

        protected virtual string FormatGenericResult(CommandData data)
        {
            if (!data.Success)
                return $"% Error: {data.ErrorMessage}\n";

            return "% Command executed successfully\n";
        }
    }
}