using NetForge.Interfaces.Cli;

namespace NetForge.Simulation.Common.CLI.Extensions
{
    /// <summary>
    /// Extension methods for enhanced CLI completion functionality
    /// </summary>
    public static class CompletionExtensions
    {
        /// <summary>
        /// Gets context-aware completions for a command
        /// </summary>
        public static List<string> GetContextualCompletions(this ICliHandler handler, ICliContext context, string partialCommand)
        {
            var completions = new List<string>();

            // Get basic completions from handler
            var basicCompletions = handler.GetCompletions(context);

            // Filter by partial command if provided
            if (!string.IsNullOrEmpty(partialCommand))
            {
                // Add exact prefix matches first
                var exactMatches = basicCompletions.Where(c =>
                    c.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase)).ToList();
                completions.AddRange(exactMatches);

                // Add fuzzy matches if no exact matches
                if (exactMatches.Count == 0)
                {
                    var fuzzyMatches = basicCompletions.Where(c =>
                        GetFuzzyMatchScore(partialCommand, c) > 0.5).ToList();
                    completions.AddRange(fuzzyMatches);
                }
            }
            else
            {
                completions.AddRange(basicCompletions);
            }

            return completions.Distinct().OrderBy(c => c).ToList();
        }

        /// <summary>
        /// Gets interface names for completion
        /// </summary>
        public static List<string> GetInterfaceCompletions(this ICliContext context, string partialName = "")
        {
            var interfaces = new List<string>();

            // Get interfaces from device
            var device = context.Device;
            if (device != null)
            {
                try
                {
                    var deviceInterfaces = device.GetAllInterfaces();
                    interfaces.AddRange(deviceInterfaces.Select(i => i.Value.Name));
                }
                catch
                {
                    // Fallback to common interface names
                    interfaces.AddRange(GetCommonInterfaceNames());
                }
            }
            else
            {
                interfaces.AddRange(GetCommonInterfaceNames());
            }

            // Filter by partial name if provided
            if (!string.IsNullOrEmpty(partialName))
            {
                interfaces = interfaces.Where(i =>
                    i.StartsWith(partialName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return interfaces.Distinct().OrderBy(i => i).ToList();
        }

        /// <summary>
        /// Gets VLAN IDs for completion
        /// </summary>
        public static List<string> GetVlanCompletions(this ICliContext context, string partialId = "")
        {
            var vlans = new List<string>();

            // Get VLANs from device
            var device = context.Device;
            if (device != null)
            {
                try
                {
                    var deviceVlans = device.GetAllVlans();
                    vlans.AddRange(deviceVlans.Values.Select(v => v.Id.ToString()));
                }
                catch
                {
                    // Fallback to common VLAN IDs
                    vlans.AddRange(new[] { "1", "10", "20", "30", "100", "200", "300" });
                }
            }
            else
            {
                vlans.AddRange(new[] { "1", "10", "20", "30", "100", "200", "300" });
            }

            // Filter by partial ID if provided
            if (!string.IsNullOrEmpty(partialId))
            {
                vlans = vlans.Where(v =>
                    v.StartsWith(partialId, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return vlans.Distinct().OrderBy(v => v).ToList();
        }

        /// <summary>
        /// Gets protocol completions based on context
        /// </summary>
        public static List<string> GetProtocolCompletions(this ICliContext context, string partialProtocol = "")
        {
            var protocols = new List<string> { "ospf", "bgp", "eigrp", "rip", "isis", "static" };

            // Get vendor-specific protocols if available
            if (context.VendorContext != null)
            {
                var vendorProtocols = context.VendorContext.GetCommandCompletions(new[] { "router" });
                protocols.AddRange(vendorProtocols);
            }

            // Filter by partial protocol if provided
            if (!string.IsNullOrEmpty(partialProtocol))
            {
                protocols = protocols.Where(p =>
                    p.StartsWith(partialProtocol, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return protocols.Distinct().OrderBy(p => p).ToList();
        }

        /// <summary>
        /// Gets IP address completions for common configurations
        /// </summary>
        public static List<string> GetIpAddressCompletions(this ICliContext context, string partialIp = "")
        {
            var addresses = new List<string>
            {
                "192.168.1.1",
                "192.168.1.0",
                "10.0.0.1",
                "10.0.0.0",
                "172.16.0.1",
                "172.16.0.0",
                "127.0.0.1",
                "0.0.0.0"
            };

            // Filter by partial IP if provided
            if (!string.IsNullOrEmpty(partialIp))
            {
                addresses = addresses.Where(a =>
                    a.StartsWith(partialIp, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return addresses.Distinct().OrderBy(a => a).ToList();
        }

        /// <summary>
        /// Gets mode-specific completions based on current device mode
        /// </summary>
        public static List<string> GetModeSpecificCompletions(this ICliContext context)
        {
            var currentMode = context.CurrentMode.ToLowerInvariant();

            return currentMode switch
            {
                "user" => new List<string> { "enable", "ping", "show", "exit" },
                "privileged" => new List<string> { "configure", "show", "ping", "write", "reload", "disable", "exit", "copy", "debug" },
                "config" => new List<string> { "interface", "router", "hostname", "ip", "vlan", "access-list", "line", "banner", "service", "no", "exit", "end" },
                "interface" => new List<string> { "ip", "shutdown", "no", "description", "switchport", "spanning-tree", "exit" },
                "router" => new List<string> { "network", "neighbor", "router-id", "version", "auto-summary", "redistribute", "exit" },
                "line" => new List<string> { "password", "login", "transport", "access-class", "exec-timeout", "exit" },
                "vlan" => new List<string> { "name", "state", "exit" },
                _ => new List<string>()
            };
        }

        /// <summary>
        /// Calculate fuzzy match score between two strings
        /// </summary>
        private static double GetFuzzyMatchScore(string input, string target)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(target))
                return 0;

            input = input.ToLowerInvariant();
            target = target.ToLowerInvariant();

            // Exact match
            if (input == target)
                return 1.0;

            // Starts with match
            if (target.StartsWith(input))
                return 0.9;

            // Contains match
            if (target.Contains(input))
                return 0.7;

            // Levenshtein distance based scoring
            var distance = CalculateLevenshteinDistance(input, target);
            var maxLength = Math.Max(input.Length, target.Length);
            var similarity = 1.0 - ((double)distance / maxLength);

            return similarity;
        }

        /// <summary>
        /// Calculate Levenshtein distance between two strings
        /// </summary>
        private static int CalculateLevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        /// <summary>
        /// Get common interface names for fallback
        /// </summary>
        private static List<string> GetCommonInterfaceNames()
        {
            return new List<string>
            {
                "ethernet0/0", "ethernet0/1", "ethernet0/2", "ethernet0/3",
                "e0/0", "e0/1", "e0/2", "e0/3",
                "fastethernet0/0", "fastethernet0/1", "fastethernet0/2", "fastethernet0/3",
                "fa0/0", "fa0/1", "fa0/2", "fa0/3",
                "gigabitethernet0/0", "gigabitethernet0/1", "gigabitethernet0/2", "gigabitethernet0/3",
                "gi0/0", "gi0/1", "gi0/2", "gi0/3",
                "serial0/0", "serial0/1", "serial0/2", "serial0/3",
                "s0/0", "s0/1", "s0/2", "s0/3",
                "loopback0", "loopback1", "loopback2", "loopback3",
                "lo0", "lo1", "lo2", "lo3",
                "vlan1", "vlan10", "vlan20", "vlan30", "vlan100"
            };
        }
    }
}
