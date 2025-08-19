using System;
using System.Collections.Generic;
using System.Linq;

namespace NetForge.Simulation.CliHandlers.Juniper
{
    /// <summary>
    /// Provides helpers for Juniper interface alias generation.
    /// </summary>
    public static class JuniperInterfaceAliasHelper
    {
        public static List<string> GetInterfaceAliases(string interfaceName)
        {
            var aliases = new List<string>();
            if (string.IsNullOrEmpty(interfaceName))
            {
                aliases.Add("");
                return aliases;
            }

            var canonical = JuniperInterfaceAliasHandler.GetCanonicalInterfaceName(interfaceName);
            var expanded = JuniperInterfaceAliasExpander.ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();

            // Always include the original and canonical forms
            aliases.Add(interfaceName);
            aliases.Add(canonical);

            // Generate all possible aliases based on interface type
            string number = JuniperInterfaceAliasHandler.GetInterfaceNumber(expanded);
            string cleanNumber = number.TrimStart('/', '-');
            // For high-speed interfaces, ensure cleanNumber is always x/y/z
            if ((normalized.StartsWith("25ge-") || normalized.StartsWith("twentyfivegigabitethernet-") ||
                 normalized.StartsWith("100ge-") || normalized.StartsWith("hundredgigabitethernet-") ||
                 normalized.StartsWith("40ge-") || normalized.StartsWith("fortygigabitethernet-")) && cleanNumber.Count(c => c == '/') == 1)
            {
                // If only x/y, try to extract x/y/z from the original string
                var m = System.Text.RegularExpressions.Regex.Match(expanded, @"([0-9]+/[0-9]+/[0-9]+)");
                if (m.Success) cleanNumber = m.Groups[1].Value;
            }
            // 25GE
            if (normalized.StartsWith("25ge-") || normalized.StartsWith("twentyfivegigabitethernet-"))
            {
                aliases.Add($"25ge-{cleanNumber}");
                aliases.Add($"25g{cleanNumber}");
                aliases.Add($"TwentyFiveGigabitEthernet-{cleanNumber}");
                aliases.Add($"twentyfivegigabit-{cleanNumber}");
            }
            // 100GE
            if (normalized.StartsWith("100ge-") || normalized.StartsWith("hundredgigabitethernet-"))
            {
                aliases.Add($"100ge-{cleanNumber}");
                aliases.Add($"100g{cleanNumber}");
                aliases.Add($"HundredGigabitEthernet-{cleanNumber}");
                aliases.Add($"hundredgigabit-{cleanNumber}");
            }
            // 40GE
            if (normalized.StartsWith("40ge-") || normalized.StartsWith("fortygigabitethernet-"))
            {
                aliases.Add($"40ge-{cleanNumber}");
                aliases.Add($"40g{cleanNumber}");
                aliases.Add($"FortyGigabitEthernet-{cleanNumber}");
                aliases.Add($"fortygigabit-{cleanNumber}");
            }
            // GE
            if (normalized.StartsWith("ge-") || normalized.StartsWith("gigabitethernet-"))
            {
                aliases.Add($"ge-{cleanNumber}");
                aliases.Add($"gig{cleanNumber}");
                aliases.Add($"gig-{cleanNumber}");
                aliases.Add($"GigabitEthernet-{cleanNumber}");
                aliases.Add($"gigabit-{cleanNumber}");
            }
            // XE
            if (normalized.StartsWith("xe-") || normalized.StartsWith("tengigabitethernet-"))
            {
                aliases.Add($"xe-{cleanNumber}");
                aliases.Add($"xge-{cleanNumber}");
                aliases.Add($"ten{cleanNumber}");
                aliases.Add($"tengig-{cleanNumber}");
                aliases.Add($"TenGigabitEthernet-{cleanNumber}");
                aliases.Add($"tengigabit-{cleanNumber}");
            }
            // ET
            if (normalized.StartsWith("et-") || normalized.StartsWith("ethernetinterface-"))
            {
                aliases.Add($"et-{cleanNumber}");
                aliases.Add($"ether-{cleanNumber}");
                aliases.Add($"EthernetInterface-{cleanNumber}");
            }
            // Management
            if (normalized.StartsWith("em") || normalized.StartsWith("fxp") || normalized.StartsWith("me") || normalized.StartsWith("mgmt") || normalized.StartsWith("management"))
            {
                aliases.Add($"em{number}");
                aliases.Add($"fxp{number}");
                aliases.Add($"me{number}");
                aliases.Add($"mgmt{number}");
                aliases.Add($"Management{number}");
                aliases.Add($"management{number}");
            }
            // Loopback
            if (normalized.StartsWith("lo"))
            {
                aliases.Add($"lo{number}");
                aliases.Add($"loop{number}");
                aliases.Add($"Loopback{number}");
            }
            // AggregatedEthernet
            if (normalized.StartsWith("ae") || normalized.StartsWith("agg"))
            {
                aliases.Add($"ae{number}");
                aliases.Add($"agg{number}");
                aliases.Add($"aggregated{number}");
                aliases.Add($"AggregatedEthernet{number}");
            }
            // RedundantEthernet
            if (normalized.StartsWith("reth") || normalized.StartsWith("redundant"))
            {
                aliases.Add($"reth{number}");
                aliases.Add($"RedundantEthernet{number}");
                aliases.Add($"redundant{number}");
            }
            // STunnel
            if (normalized.StartsWith("stunnel") || normalized.StartsWith("st"))
            {
                aliases.Add($"st{number}");
                aliases.Add($"tunnel{number}");
                aliases.Add($"STunnel{number}");
            }
            // GRE
            if (normalized.StartsWith("gr-") || normalized.StartsWith("gre-"))
            {
                aliases.Add($"GRE-{number}");
            }
            // IRB
            if (normalized.StartsWith("irb"))
            {
                aliases.Add($"irb{number}");
                aliases.Add($"IRB{number}");
            }
            // VLAN
            if (normalized.StartsWith("vlan"))
            {
                aliases.Add($"vlan{number}");
                aliases.Add($"VLAN{number}");
                aliases.Add($"vl{number}");
            }
            // Ether short alias for et-
            if (normalized.StartsWith("et-"))
            {
                aliases.Add($"ether-{number}");
            }
            // Add tunnel0 for stunnel
            if (normalized.StartsWith("stunnel") || normalized.StartsWith("st"))
            {
                aliases.Add($"tunnel{number}");
            }
            // Add loop0 for lo
            if (normalized.StartsWith("lo"))
            {
                aliases.Add($"loop{number}");
            }
            // Add agg0 for ae
            if (normalized.StartsWith("ae") || normalized.StartsWith("agg"))
            {
                aliases.Add($"agg{number}");
            }
            // Add mgmt0 for management
            if (normalized.StartsWith("em") || normalized.StartsWith("fxp") || normalized.StartsWith("me") || normalized.StartsWith("mgmt"))
            {
                aliases.Add($"mgmt{number}");
            }
            return aliases.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
