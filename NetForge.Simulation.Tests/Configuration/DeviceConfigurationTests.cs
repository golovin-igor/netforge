using NetForge.Simulation.Configuration;
using System;
using Xunit;

namespace NetForge.Simulation.Tests.Configuration
{
    public class DeviceConfigurationTests
    {
        [Fact]
        public void Reset_ShouldInitializeWithDefaults()
        {
            var config = new DeviceConfiguration();
            config.AppendItem("hostname", "MyDevice");

            config.Reset();
            var output = config.Build();

            Assert.Contains("name device", output);
            Assert.Contains("config default", output);
            Assert.DoesNotContain("hostname MyDevice", output);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AppendItem_InvalidKey_ShouldThrow(string key)
        {
            var config = new DeviceConfiguration();
            Assert.Throws<ArgumentException>(() => config.AppendItem(key!, "v"));
        }

        [Fact]
        public void AppendLine_ShouldParseLinesAndIgnoreComments()
        {
            var config = new DeviceConfiguration();
            config.Reset();

            string lines = "hostname Router\n# comment\nip route 0.0.0.0 0.0.0.0 1.1.1.1\nfeature x";
            config.AppendLine(lines);

            var result = config.Build();

            Assert.Contains("hostname Router", result);
            Assert.Contains("ip route 0.0.0.0 0.0.0.0 1.1.1.1", result);
            Assert.Contains("feature x", result);
            Assert.DoesNotContain("# comment", result);
        }

        [Fact]
        public void AppendLine_NoPrefixShouldRemoveItem()
        {
            var config = new DeviceConfiguration();
            config.Reset();
            config.AppendLine("hostname Router");
            Assert.Contains("hostname Router", config.Build());

            config.AppendLine("no hostname");
            var output = config.Build();

            Assert.DoesNotContain("hostname Router", output);
        }

        [Fact]
        public void Import_ShouldProcessEachLine()
        {
            var config = new DeviceConfiguration();
            config.Reset();

            string text = "foo 1\r\nbar 2\n# comment\nbaz";
            config.Import(text);
            var result = config.Build();

            Assert.Contains("foo 1", result);
            Assert.Contains("bar 2", result);
            Assert.Contains("baz", result);
            Assert.DoesNotContain("# comment", result);
            Assert.True(config.Length > 0);
        }
    }
}
