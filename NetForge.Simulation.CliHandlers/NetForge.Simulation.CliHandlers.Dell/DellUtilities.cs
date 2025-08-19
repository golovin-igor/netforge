using NetForge.Simulation.Common;
using System.Text.RegularExpressions;

namespace NetForge.Simulation.CliHandlers.Dell
{
    /// <summary>
    /// Utility class for handling Dell interface name aliases and expansions
    /// </summary>
    public static class DellInterfaceAliasHandler
    {
        /// <summary>
        /// Expands interface aliases to full interface names
        /// </summary>
        public static string ExpandInterfaceAlias(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            var normalized = interfaceName.ToLower().Trim();
            
            // Handle sub-interfaces first (e.g., "eth1.200")
            var parts = normalized.Split('.');
            var mainInterface = parts[0];
            var subInterface = parts.Length > 1 ? "." + parts[1] : "";
            
            // Extract the base interface name and number
            var match = Regex.Match(mainInterface, @"^([a-z0-9-]+)\s*(.*)$");
            if (!match.Success)
                return interfaceName;
                
            var prefix = match.Groups[1].Value;
            var number = match.Groups[2].Value;
            
            // Map prefixes to canonical names (what tests expect)
            var fullName = prefix switch
            {
                "e" => "ethernet",
                "eth" => "ethernet",
                "ethernet" => "ethernet",
                "gi" => "gigabitethernet",
                "gig" => "gigabitethernet", 
                "gigabit" => "gigabitethernet",
                "gigabitethernet" => "gigabitethernet",
                "te" => "tengigabitethernet",
                "ten" => "tengigabitethernet",
                "tengig" => "tengigabitethernet",
                "tengigabit" => "tengigabitethernet",
                "tengigabitethernet" => "tengigabitethernet",
                "25g" => "twentyfivegigabitethernet",
                "25ge" => "twentyfivegigabitethernet",
                "twentyfive" => "twentyfivegigabitethernet",
                "twentyfivegigabit" => "twentyfivegigabitethernet",
                "twentyfivegigabitethernet" => "twentyfivegigabitethernet",
                "40g" => "fortygigabitethernet",
                "40ge" => "fortygigabitethernet",
                "forty" => "fortygigabitethernet",
                "fortygigabit" => "fortygigabitethernet",
                "fortygigabitethernet" => "fortygigabitethernet",
                "100g" => "hundredgigabitethernet",
                "100ge" => "hundredgigabitethernet",
                "hundred" => "hundredgigabitethernet",
                "hundredgigabit" => "hundredgigabitethernet",
                "hundredgigabitethernet" => "hundredgigabitethernet",
                "ma" => "mgmt",
                "mgmt" => "mgmt",
                "management" => "mgmt",
                "lo" => "loopback",
                "loop" => "loopback",
                "loopback" => "loopback",
                "po" => "port-channel",
                "pc" => "port-channel",
                "port" => "port-channel",
                "portchannel" => "port-channel",
                "port-channel" => "port-channel",
                "vl" => "vlan",
                "vlan" => "vlan",
                "tu" => "tunnel",
                "tunnel" => "tunnel",
                "nu" => "null",
                "null" => "null",
                _ => prefix
            };
            
            // Reconstruct the interface name
            var result = fullName + " " + number.Trim() + subInterface;
            
            // Handle case preservation for original input
            var originalPrefix = interfaceName.Split(' ')[0];
            if (char.IsUpper(originalPrefix[0]))
            {
                result = char.ToUpper(result[0]) + result.Substring(1);
            }
            
            return result;
        }

        /// <summary>
        /// Compresses interface names to shorter aliases
        /// </summary>
        public static string CompressInterfaceName(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return interfaceName;

            var normalized = interfaceName.ToLower().Trim();
            
            // Handle sub-interfaces
            var parts = normalized.Split('.');
            var mainInterface = parts[0];
            var subInterface = parts.Length > 1 ? "." + parts[1] : "";
            
            // Extract prefix and number
            var match = Regex.Match(mainInterface, @"^([a-z0-9-]+)\s+(.*)$");
            if (!match.Success)
                return interfaceName;
                
            var prefix = match.Groups[1].Value;
            var number = match.Groups[2].Value;
            
            // Map to shortest abbreviation
            var shortName = prefix switch
            {
                "ethernet" => "e",
                "gigabitethernet" => "gi",
                "tengigabitethernet" => "te",
                "twentyfivegigabitethernet" => "25g",
                "fortygigabitethernet" => "40g",
                "hundredgigabitethernet" => "100g",
                "management" => "ma",
                "mgmt" => "ma",
                "loopback" => "lo",
                "port-channel" => "po",
                "vlan" => "vl",
                "tunnel" => "tu",
                "null" => "nu",
                _ => prefix
            };
            
            return shortName + " " + number.Trim() + subInterface;
        }

        /// <summary>
        /// Validates if an interface name is valid
        /// </summary>
        public static bool IsValidInterfaceName(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
                return false;

            var normalized = interfaceName.ToLower().Trim();
            
            // Check for valid Dell interface patterns with spaces
            var patterns = new[]
            {
                // Ethernet patterns
                @"^e\s+\d+/\d+/\d+(\.\d+)?$",
                @"^eth\s+\d+/\d+/\d+(\.\d+)?$",
                @"^ethernet\s+\d+/\d+/\d+(\.\d+)?$",
                
                // GigabitEthernet patterns
                @"^gi\s+\d+/\d+/\d+(\.\d+)?$",
                @"^gig\s+\d+/\d+/\d+(\.\d+)?$",
                @"^gigabit\s+\d+/\d+/\d+(\.\d+)?$",
                @"^gigabitethernet\s+\d+/\d+/\d+(\.\d+)?$",
                
                // TenGigabitEthernet patterns
                @"^te\s+\d+/\d+/\d+(\.\d+)?$",
                @"^ten\s+\d+/\d+/\d+(\.\d+)?$",
                @"^tengig\s+\d+/\d+/\d+(\.\d+)?$",
                @"^tengigabit\s+\d+/\d+/\d+(\.\d+)?$",
                @"^tengigabitethernet\s+\d+/\d+/\d+(\.\d+)?$",
                
                // TwentyFiveGigabitEthernet patterns
                @"^25g\s+\d+/\d+/\d+(\.\d+)?$",
                @"^25ge\s+\d+/\d+/\d+(\.\d+)?$",
                @"^twentyfive\s+\d+/\d+/\d+(\.\d+)?$",
                @"^twentyfivegigabit\s+\d+/\d+/\d+(\.\d+)?$",
                @"^twentyfivegigabitethernet\s+\d+/\d+/\d+(\.\d+)?$",
                
                // FortyGigabitEthernet patterns
                @"^40g\s+\d+/\d+/\d+(\.\d+)?$",
                @"^40ge\s+\d+/\d+/\d+(\.\d+)?$",
                @"^forty\s+\d+/\d+/\d+(\.\d+)?$",
                @"^fortygigabit\s+\d+/\d+/\d+(\.\d+)?$",
                @"^fortygigabitethernet\s+\d+/\d+/\d+(\.\d+)?$",
                
                // HundredGigabitEthernet patterns
                @"^100g\s+\d+/\d+/\d+(\.\d+)?$",
                @"^100ge\s+\d+/\d+/\d+(\.\d+)?$",
                @"^hundred\s+\d+/\d+/\d+(\.\d+)?$",
                @"^hundredgigabit\s+\d+/\d+/\d+(\.\d+)?$",
                @"^hundredgigabitethernet\s+\d+/\d+/\d+(\.\d+)?$",
                
                // Management patterns
                @"^ma\s+\d+/\d+/\d+$",
                @"^mgmt\s+\d+/\d+/\d+$",
                @"^management\s+\d+/\d+/\d+$",
                
                // Loopback patterns
                @"^lo\s+\d+$",
                @"^loop\s+\d+$",
                @"^loopback\s+\d+$",
                
                // Port-channel patterns
                @"^po\s+\d+$",
                @"^pc\s+\d+$",
                @"^port\s+\d+$",
                @"^portchannel\s+\d+$",
                @"^port-channel\s+\d+$",
                
                // VLAN patterns
                @"^vl\s+\d+$",
                @"^vlan\s+\d+$",
                
                // Tunnel patterns
                @"^tu\s+\d+$",
                @"^tunnel\s+\d+$",
                
                // Null patterns
                @"^nu\s+\d+$",
                @"^null\s+\d+$"
            };
            
            return patterns.Any(pattern => Regex.IsMatch(normalized, pattern));
        }

        /// <summary>
        /// Checks if two interface names are equivalent (considering aliases)
        /// </summary>
        public static bool AreEquivalentInterfaceNames(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) && string.IsNullOrEmpty(name2))
                return false; // Both empty strings should return false
            
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return false;

            var expanded1 = ExpandInterfaceAlias(name1);
            var expanded2 = ExpandInterfaceAlias(name2);
            
            return string.Equals(expanded1, expanded2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets all possible aliases for an interface name
        /// </summary>
        public static List<string> GetInterfaceAliases(string interfaceName)
        {
            var aliases = new List<string>();
            
            if (string.IsNullOrEmpty(interfaceName))
            {
                aliases.Add(interfaceName ?? "");
                return aliases;
            }

            var normalized = interfaceName.ToLower().Trim();
            
            // Handle sub-interfaces
            var parts = normalized.Split('.');
            var mainInterface = parts[0];
            var subInterface = parts.Length > 1 ? "." + parts[1] : "";
            
            // Extract prefix and number
            var match = Regex.Match(mainInterface, @"^([a-z0-9-]+)\s*(.*)$");
            if (!match.Success)
            {
                aliases.Add(interfaceName);
                return aliases;
            }
            
            var prefix = match.Groups[1].Value;
            var number = match.Groups[2].Value.Trim();
            
            // Generate aliases based on interface type
            switch (prefix)
            {
                case "e":
                case "eth":
                case "ethernet":
                    aliases.Add($"ethernet {number}{subInterface}");
                    aliases.Add($"eth {number}{subInterface}");
                    aliases.Add($"e {number}{subInterface}");
                    break;
                    
                case "gi":
                case "gig":
                case "gigabit":
                case "gigabitethernet":
                    aliases.Add($"gigabitethernet {number}{subInterface}");
                    aliases.Add($"gigabit {number}{subInterface}");
                    aliases.Add($"gig {number}{subInterface}");
                    aliases.Add($"gi {number}{subInterface}");
                    break;
                    
                case "te":
                case "ten":
                case "tengig":
                case "tengigabit":
                case "tengigabitethernet":
                    aliases.Add($"tengigabitethernet {number}{subInterface}");
                    aliases.Add($"tengigabit {number}{subInterface}");
                    aliases.Add($"tengig {number}{subInterface}");
                    aliases.Add($"ten {number}{subInterface}");
                    aliases.Add($"te {number}{subInterface}");
                    break;
                    
                case "25g":
                case "25ge":
                case "twentyfive":
                case "twentyfivegigabit":
                case "twentyfivegigabitethernet":
                    aliases.Add($"twentyfivegigabitethernet {number}{subInterface}");
                    aliases.Add($"twentyfivegigabit {number}{subInterface}");
                    aliases.Add($"twentyfive {number}{subInterface}");
                    aliases.Add($"25ge {number}{subInterface}");
                    aliases.Add($"25g {number}{subInterface}");
                    break;
                    
                case "40g":
                case "40ge":
                case "forty":
                case "fortygigabit":
                case "fortygigabitethernet":
                    aliases.Add($"fortygigabitethernet {number}{subInterface}");
                    aliases.Add($"fortygigabit {number}{subInterface}");
                    aliases.Add($"forty {number}{subInterface}");
                    aliases.Add($"40ge {number}{subInterface}");
                    aliases.Add($"40g {number}{subInterface}");
                    break;
                    
                case "100g":
                case "100ge":
                case "hundred":
                case "hundredgigabit":
                case "hundredgigabitethernet":
                    aliases.Add($"hundredgigabitethernet {number}{subInterface}");
                    aliases.Add($"hundredgigabit {number}{subInterface}");
                    aliases.Add($"hundred {number}{subInterface}");
                    aliases.Add($"100ge {number}{subInterface}");
                    aliases.Add($"100g {number}{subInterface}");
                    break;
                    
                case "ma":
                case "mgmt":
                case "management":
                    aliases.Add($"mgmt {number}{subInterface}");
                    aliases.Add($"management {number}{subInterface}");
                    aliases.Add($"ma {number}{subInterface}");
                    break;
                    
                case "lo":
                case "loop":
                case "loopback":
                    aliases.Add($"loopback {number}{subInterface}");
                    aliases.Add($"loop {number}{subInterface}");
                    aliases.Add($"lo {number}{subInterface}");
                    break;
                    
                case "po":
                case "pc":
                case "port":
                case "portchannel":
                    aliases.Add($"port-channel {number}{subInterface}");
                    aliases.Add($"portchannel {number}{subInterface}");
                    aliases.Add($"port {number}{subInterface}");
                    aliases.Add($"pc {number}{subInterface}");
                    aliases.Add($"po {number}{subInterface}");
                    break;
                    
                case "vl":
                case "vlan":
                    aliases.Add($"vlan {number}{subInterface}");
                    aliases.Add($"vl {number}{subInterface}");
                    break;
                    
                case "tu":
                case "tunnel":
                    aliases.Add($"tunnel {number}{subInterface}");
                    aliases.Add($"tu {number}{subInterface}");
                    break;
                    
                case "nu":
                case "null":
                    aliases.Add($"null {number}{subInterface}");
                    aliases.Add($"nu {number}{subInterface}");
                    break;
                    
                default:
                    aliases.Add(interfaceName);
                    break;
            }
            
            return aliases.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Gets the interface type from the name
        /// </summary>
        public static string GetInterfaceType(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "Unknown";

            var expanded = ExpandInterfaceAlias(interfaceName);
            var normalized = expanded.ToLower();

            if (normalized.StartsWith("ethernet "))
                return "ethernet";
            if (normalized.StartsWith("gigabitethernet "))
                return "gigabitethernet";
            if (normalized.StartsWith("tengigabitethernet "))
                return "tengigabitethernet";
            if (normalized.StartsWith("twentyfivegigabitethernet "))
                return "twentyfivegigabitethernet";
            if (normalized.StartsWith("fortygigabitethernet "))
                return "fortygigabitethernet";
            if (normalized.StartsWith("hundredgigabitethernet "))
                return "hundredgigabitethernet";
            if (normalized.StartsWith("mgmt "))
                return "mgmt";
            if (normalized.StartsWith("loopback "))
                return "loopback";
            if (normalized.StartsWith("port-channel "))
                return "port-channel";
            if (normalized.StartsWith("vlan "))
                return "vlan";
            if (normalized.StartsWith("tunnel "))
                return "tunnel";
            if (normalized.StartsWith("null "))
                return "null";

            return "Unknown";
        }

        /// <summary>
        /// Gets the interface number from the name
        /// </summary>
        public static string GetInterfaceNumber(string interfaceName)
        {
            if (string.IsNullOrEmpty(interfaceName))
                return "";

            var normalized = interfaceName.ToLower().Trim();
            
            // Extract the number part after the interface type
            var match = Regex.Match(normalized, @"^[a-z-]+\s+(.*)$");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            // Fallback - extract anything after the alphabetic prefix
            var fallbackMatch = Regex.Match(normalized, @"^[a-z-]+(.*)$");
            return fallbackMatch.Success ? fallbackMatch.Groups[1].Value.Trim() : "";
        }

        /// <summary>
        /// Gets the canonical interface name (expanded and normalized)
        /// </summary>
        public static string GetCanonicalInterfaceName(string interfaceName)
        {
            return ExpandInterfaceAlias(interfaceName);
        }
    }
} 
