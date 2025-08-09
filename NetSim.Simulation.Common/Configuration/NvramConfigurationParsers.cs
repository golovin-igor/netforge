using NetSim.Simulation.Common;

namespace NetSim.Simulation.Core
{
    /// <summary>
    /// Contract for parsing a vendor-specific startup configuration (NVRAM) and applying it to a device instance.
    /// </summary>
    public interface INvramConfigurationParser
    {
    Task Apply(string nvramContent, NetworkDevice device);
    }

    /// <summary>
    /// Generic helper – feeds each configuration command line directly into the device CLI.
    /// Lines beginning with '#' or '!' are treated as comments and ignored.
    /// </summary>
    public abstract class LineFeedParserBase : INvramConfigurationParser
    {
        protected abstract Task PreConfig(NetworkDevice device);
        protected virtual Task PostConfig(NetworkDevice device) { return Task.CompletedTask; }

        public async Task Apply(string nvramContent, NetworkDevice device)
        {
            await PreConfig(device);
            var lines = nvramContent.Replace("\r", "").Split('\n');
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("!") || line.StartsWith("#")) continue;
                await device.ProcessCommandAsync(line);
            }
            await PostConfig(device);
        }
    }

    public class CiscoNvramConfigurationParser : LineFeedParserBase
    {
        protected override async Task PreConfig(NetworkDevice device)
        {
            await device.ProcessCommandAsync("enable");
            await device.ProcessCommandAsync("configure terminal");
        }
        protected override async Task PostConfig(NetworkDevice device)
        {
            await device.ProcessCommandAsync("end");
        }
    }

    public class JuniperNvramConfigurationParser : LineFeedParserBase
    {
        protected override async Task PreConfig(NetworkDevice device)
        {
            await device.ProcessCommandAsync("configure");
        }
        protected override async Task PostConfig(NetworkDevice device)
        {
            await device.ProcessCommandAsync("commit");
            await device.ProcessCommandAsync("exit");
        }
    }

    public class AristaNvramConfigurationParser : CiscoNvramConfigurationParser { }
    public class ArubaNvramConfigurationParser : AristaNvramConfigurationParser { }
    public class ExtremeNvramConfigurationParser : AristaNvramConfigurationParser { }
    public class DellNvramConfigurationParser : CiscoNvramConfigurationParser { }

    public class NokiaNvramConfigurationParser : LineFeedParserBase
    {
        protected override async Task PreConfig(NetworkDevice device)
        {
            await device.ProcessCommandAsync("configure");
        }
        protected override async Task PostConfig(NetworkDevice device)
        {
            await device.ProcessCommandAsync("commit");
            await device.ProcessCommandAsync("exit");
        }
    }

    public class HuaweiNvramConfigurationParser : LineFeedParserBase
    {
        protected override async Task PreConfig(NetworkDevice device)
        {
            await device.ProcessCommandAsync("system-view");
        }
        protected override async Task PostConfig(NetworkDevice device)
        {
            await device.ProcessCommandAsync("quit");
        }
    }

    public class FortinetNvramConfigurationParser : LineFeedParserBase
    {
    protected override Task PreConfig(NetworkDevice device) { return Task.CompletedTask; }
    }

    public class MikroTikNvramConfigurationParser : LineFeedParserBase
    {
    protected override Task PreConfig(NetworkDevice device) { return Task.CompletedTask; }
    }

    /// <summary>
    /// Simple factory to obtain parser by vendor name.
    /// </summary>
    public static class NvramParserFactory
    {
        public static INvramConfigurationParser GetParser(string vendor)
        {
            return vendor switch
            {
                "Cisco" => new CiscoNvramConfigurationParser(),
                "Arista" => new AristaNvramConfigurationParser(),
                "Aruba" => new ArubaNvramConfigurationParser(),
                "Extreme" => new ExtremeNvramConfigurationParser(),
                "Dell" => new DellNvramConfigurationParser(),
                "Juniper" => new JuniperNvramConfigurationParser(),
                "Nokia" => new NokiaNvramConfigurationParser(),
                "Huawei" => new HuaweiNvramConfigurationParser(),
                "Fortinet" => new FortinetNvramConfigurationParser(),
                "MikroTik" => new MikroTikNvramConfigurationParser(),
                _ => throw new NotSupportedException($"Parser for vendor {vendor} is not implemented.")
            };
        }
    }
} 
