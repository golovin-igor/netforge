using NetForge.Simulation.Common;
using System.Text.RegularExpressions;

namespace NetForge.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Utility class for handling Juniper interface name aliases and expansions
    /// </summary>
    public static class JuniperInterfaceAliasHandler
    {

        /// <summary>
        /// Expands interface aliases to full interface names
        /// </summary>
        public static string ExpandInterfaceAlias(string interfaceName)
        {
            return JuniperInterfaceAliasExpander.ExpandInterfaceAlias(interfaceName);
        }

        /// <summary>
        /// Compresses interface names to short aliases
        /// </summary>
        public static string CompressInterfaceName(string interfaceName)
        {
            return JuniperInterfaceAliasCompressor.CompressInterfaceName(interfaceName);
        }

        /// <summary>
        /// Checks if the interface name is valid
        /// </summary>
        public static bool IsValidInterfaceName(string interfaceName)
        {
            return JuniperInterfaceNameValidator.IsValidInterfaceName(interfaceName);
        }

        /// <summary>
        /// Checks if two interface names are equivalent (considering aliases)
        /// </summary>
        public static bool AreEquivalentInterfaceNames(string name1, string name2)
        {
            return JuniperInterfaceNameComparer.AreEquivalentInterfaceNames(name1, name2);
        }

        /// <summary>
        /// Gets all possible aliases for an interface name
        /// </summary>
        public static List<string> GetInterfaceAliases(string interfaceName)
        {
            return JuniperInterfaceAliasHelper.GetInterfaceAliases(interfaceName);
        }

        public static string GetCanonicalInterfaceName(string interfaceName)
        {
            return JuniperCanonicalInterfaceNameHelper.GetCanonicalInterfaceName(interfaceName);
        }

        public static string GetInterfaceNumber(string interfaceName)
        {
            return JuniperInterfaceNumberHelper.GetInterfaceNumber(interfaceName);
        }

        public static string GetInterfaceType(string interfaceName)
        {
            return JuniperInterfaceTypeHelper.GetInterfaceType(interfaceName);
        }
    }
}
