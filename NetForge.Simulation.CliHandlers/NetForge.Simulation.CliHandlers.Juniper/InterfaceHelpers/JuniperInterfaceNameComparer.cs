using System;
using System.Collections.Generic;
using System.Linq;

namespace NetForge.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Compares Juniper interface names for equivalence.
    /// </summary>
    public static class JuniperInterfaceNameComparer
    {
        public static bool AreEquivalentInterfaceNames(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) && string.IsNullOrEmpty(name2))
                return false;
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return false;

            var aliases1 = JuniperInterfaceAliasHandler.GetInterfaceAliases(name1);
            var aliases2 = JuniperInterfaceAliasHandler.GetInterfaceAliases(name2);
            return aliases1.Any(a1 => aliases2.Any(a2 => string.Equals(a1, a2, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
