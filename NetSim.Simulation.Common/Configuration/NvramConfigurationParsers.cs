using NetSim.Simulation.Common;

namespace NetSim.Simulation.Core
{
    /// <summary>
    /// Contract for parsing a vendor-specific startup configuration (NVRAM) and applying it to a device instance.
    /// </summary>
    public interface INvramConfigurationParser
    {
        void Apply(string nvramContent, NetworkDevice device);
    }

    /// <summary>
    /// Generic helper – feeds each configuration command line directly into the device CLI.
    /// Lines beginning with '#' or '!' are treated as comments and ignored.
    /// </summary>
    public abstract class LineFeedParserBase : INvramConfigurationParser
    {
        protected abstract void PreConfig(NetworkDevice device);
        protected virtual void PostConfig(NetworkDevice device) { }

        public void Apply(string nvramContent, NetworkDevice device)
        {
            PreConfig(device);
            var lines = nvramContent.Replace("\r", "").Split('\n');
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("!") || line.StartsWith("#")) continue;
                device.ProcessCommand(line);
            }
            PostConfig(device);
        }
    }

    public class CiscoNvramConfigurationParser : LineFeedParserBase
    {
        protected override void PreConfig(NetworkDevice device)
        {
            device.ProcessCommand("enable");
            device.ProcessCommand("configure terminal");
        }
        protected override void PostConfig(NetworkDevice device)
        {
            device.ProcessCommand("end");
        }
    }

    public class JuniperNvramConfigurationParser : LineFeedParserBase
    {
        protected override void PreConfig(NetworkDevice device)
        {
            device.ProcessCommand("configure");
        }
        protected override void PostConfig(NetworkDevice device)
        {
            device.ProcessCommand("commit");
            device.ProcessCommand("exit");
        }
    }

    public class AristaNvramConfigurationParser : CiscoNvramConfigurationParser { }
    public class ArubaNvramConfigurationParser : AristaNvramConfigurationParser { }
    public class ExtremeNvramConfigurationParser : AristaNvramConfigurationParser { }
    public class DellNvramConfigurationParser : CiscoNvramConfigurationParser { }

    public class NokiaNvramConfigurationParser : LineFeedParserBase
    {
        protected override void PreConfig(NetworkDevice device)
        {
            device.ProcessCommand("configure");
        }
        protected override void PostConfig(NetworkDevice device)
        {
            device.ProcessCommand("commit");
            device.ProcessCommand("exit");
        }
    }

    public class HuaweiNvramConfigurationParser : LineFeedParserBase
    {
        protected override void PreConfig(NetworkDevice device)
        {
            device.ProcessCommand("system-view");
        }
        protected override void PostConfig(NetworkDevice device)
        {
            device.ProcessCommand("quit");
        }
    }

    public class FortinetNvramConfigurationParser : LineFeedParserBase
    {
        protected override void PreConfig(NetworkDevice device) { }
    }

    public class MikroTikNvramConfigurationParser : LineFeedParserBase
    {
        protected override void PreConfig(NetworkDevice device) { }
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
