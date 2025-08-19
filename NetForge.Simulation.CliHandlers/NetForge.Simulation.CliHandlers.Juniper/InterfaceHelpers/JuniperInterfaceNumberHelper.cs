using System.Text.RegularExpressions;

namespace NetForge.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Provides helpers for extracting Juniper interface numbers.
    /// </summary>
    public static class JuniperInterfaceNumberHelper
    {
        public static string GetInterfaceNumber(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "";

            var expanded = JuniperInterfaceAliasExpander.ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();

            // For high-speed interfaces: ge-, xe-, et-, 25ge-, 40ge-, 100ge-, gr-
            var m = Regex.Match(expanded, @"(?:ge|xe|et|25ge|40ge|100ge|gr)-([0-9]+/[0-9]+/[0-9]+(?:\.[0-9]+)?)", RegexOptions.IgnoreCase);
            if (m.Success)
                return m.Groups[1].Value;

            // For high-speed interfaces, if only x/y is found, try to extract x/y/z from the original string or canonical form
            if (Regex.IsMatch(expanded, @"(?:ge|xe|et|25ge|40ge|100ge|gr)-[0-9]+/[0-9]+$", RegexOptions.IgnoreCase))
            {
                // Try original string first
                var m2 = Regex.Match(interfaceName, @"([0-9]+/[0-9]+/[0-9]+)");
                if (m2.Success) return m2.Groups[1].Value;
                // Try canonical form
                m2 = Regex.Match(expanded, @"([0-9]+/[0-9]+/[0-9]+)");
                if (m2.Success) return m2.Groups[1].Value;
                // Try to build x/y/z from x/y if possible (fallback)
                var m3 = Regex.Match(expanded, @"(?:ge|xe|et|25ge|40ge|100ge|gr)-([0-9]+/[0-9]+)", RegexOptions.IgnoreCase);
                if (m3.Success) return m3.Groups[1].Value + "/0";
            }

            // For types with dash and numbers (fallback, e.g., st-, gre-)
            var dashIdx = expanded.IndexOf('-');
            if (dashIdx > 0 && dashIdx < expanded.Length - 1)
            {
                var numPart = expanded.Substring(dashIdx + 1);
                numPart = Regex.Replace(numPart, "^[^0-9]+", "");
                if (Regex.IsMatch(numPart, @"^\d+(/\d+){0,2}(\.\d+)?$"))
                    return numPart;
            }

            // For types with no dash but numbers at the end (e.g., ae0, lo0, me0, em0, fxp0, IRB0, VLAN.100, st0, reth0)
            m = Regex.Match(expanded, @"[a-zA-Z]+[.\-]?([0-9][0-9./]*)$");
            if (m.Success)
                return m.Groups[1].Value.TrimStart('.', '/');

            // For VLAN.100 or similar
            m = Regex.Match(expanded, @"vlan[.](\d+)", RegexOptions.IgnoreCase);
            if (m.Success)
                return m.Groups[1].Value;

            // For IRB.100 or similar
            m = Regex.Match(expanded, @"irb[.](\d+)", RegexOptions.IgnoreCase);
            if (m.Success)
                return m.Groups[1].Value;

            return "";
        }
    }
}
