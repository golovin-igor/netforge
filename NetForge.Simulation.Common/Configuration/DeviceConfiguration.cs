using System.Text;
using System.Text.RegularExpressions;

namespace NetForge.Simulation.Configuration;

/// <summary>
/// Represents the NVRAM running configuration for a device.
/// </summary>
public class DeviceConfiguration
{
    private readonly StringBuilder _configurationBuilder = new();
    private readonly Dictionary<string, string> _configItems = new();
    private const string DefaultDeviceName = "device";
    private const string DefaultConfigKey = "config";
    private const string DefaultConfigValue = "default";

    public void Reset()
    {
        _configurationBuilder.Clear();
        _configItems.Clear();

        // Set minimal default configuration
        _configItems["name"] = DefaultDeviceName;
        _configItems[DefaultConfigKey] = DefaultConfigValue;
    }

    public string Build()
    {
        _configurationBuilder.Clear();

        // Add generic header
        _configurationBuilder.Append($"# Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}").AppendLine();
        _configurationBuilder.AppendLine();

        // Add configuration items in a generic format
        foreach (var item in _configItems)
        {
            if (string.IsNullOrEmpty(item.Value))
            {
                _configurationBuilder.Append($"{item.Key}").AppendLine();
            }
            else
            {
                _configurationBuilder.Append($"{item.Key} {item.Value}").AppendLine();
            }
        }

        // Add end marker
        _configurationBuilder.AppendLine();
        return _configurationBuilder.ToString();
    }

    public void AppendItem(string item, string value)
    {
        if (string.IsNullOrWhiteSpace(item))
        {
            throw new ArgumentException("Configuration item cannot be null or empty", nameof(item));
        }

        // Update or add the configuration item
        string key = item.Trim().ToLowerInvariant();
        _configItems[key] = value?.Trim() ?? string.Empty;
    }

    public void AppendLine(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        // Normalize and split the raw input into lines
        string[] lines = raw.Trim().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Skip comments and empty lines
            if (trimmedLine.StartsWith("#") || string.IsNullOrEmpty(trimmedLine))
            {
                continue;
            }

            // Check for "no" prefix to remove settings
            if (trimmedLine.StartsWith("no ", StringComparison.OrdinalIgnoreCase))
            {
                string key = trimmedLine.Substring(3).Trim().ToLowerInvariant();
                // Remove the key if it exists
                if (_configItems.ContainsKey(key))
                {
                    _configItems.Remove(key);
                }

                continue;
            }

            // Parse generic key-value pairs or commands
            // Use regex to split on first whitespace to separate key from value
            var match = Regex.Match(trimmedLine, @"^(\S+)\s*(.*)?$");
            if (match.Success)
            {
                string key = match.Groups[1].Value.ToLowerInvariant();
                string value = match.Groups[2].Value.Trim();
                _configItems[key] = value;
            }
        }
    }

    public void Import(string config)
    {
        foreach (var line in config.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmedLine = line.Trim();
            AppendLine(trimmedLine);
        }
    }

    public override string ToString()
    {
        return Build();
    }
    
    public int Length => _configurationBuilder.Length;
}
