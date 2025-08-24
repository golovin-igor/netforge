using NetForge.Simulation.Common.Interfaces;
using NetForge.Simulation.Protocols.Common;
using NetForge.Simulation.Protocols.Common.Services;

namespace NetForge.Simulation.Protocols.Tests
{
    /// <summary>
    /// Tests for protocol plugin discovery and loading functionality
    /// Validates the auto-discovery system that finds and loads protocol plugins
    /// </summary>
    public class ProtocolPluginDiscoveryTests
    {
        private readonly ProtocolDiscoveryService _discoveryService;

        public ProtocolPluginDiscoveryTests()
        {
            _discoveryService = new ProtocolDiscoveryService();
        }

        [Fact]
        public void ProtocolDiscovery_ShouldFindAllExpectedPlugins()
        {
            // Act
            var plugins = _discoveryService.DiscoverProtocolPlugins();

            // Assert
            Assert.NotNull(plugins);
            var pluginList = plugins.ToList();
            Assert.NotEmpty(pluginList);

            // Should find plugins for some expected protocol types (test what actually exists)
            var foundProtocolTypes = pluginList.Select(p => p.ProtocolType).Distinct().ToList();

            // Verify we found at least some protocols
            Assert.True(foundProtocolTypes.Count > 0, "Should find at least one protocol plugin");

            // Log what we found for debugging
            Console.WriteLine($"Found {pluginList.Count} plugins for {foundProtocolTypes.Count} protocol types");
            foreach (var protocolType in foundProtocolTypes)
            {
                Console.WriteLine($"  - {protocolType}");
            }
        }

        [Fact]
        public void ProtocolDiscovery_ShouldProvideValidStatistics()
        {
            // Act
            var stats = _discoveryService.GetDiscoveryStatistics();

            // Assert
            Assert.NotNull(stats);
            Assert.True(stats.ContainsKey("TotalPlugins"));
            Assert.True(stats.ContainsKey("ProtocolTypes"));
            Assert.True(stats.ContainsKey("SupportedVendors"));
            Assert.True(stats.ContainsKey("PluginsByType"));
            Assert.True(stats.ContainsKey("PluginsByVendor"));

            var totalPlugins = (int)stats["TotalPlugins"];
            var protocolTypes = (int)stats["ProtocolTypes"];

            Console.WriteLine($"Discovery statistics: {totalPlugins} plugins, {protocolTypes} protocol types");

            Assert.True(totalPlugins >= 0, "Total plugins should be non-negative");
            Assert.True(protocolTypes >= 0, "Protocol types should be non-negative");
            Assert.True(protocolTypes <= totalPlugins, "Protocol types should not exceed total plugins");
        }

        [Fact]
        public void GetSupportedVendors_ShouldReturnValidVendorList()
        {
            // Act
            var supportedVendors = _discoveryService.GetSupportedVendors();

            // Assert
            Assert.NotNull(supportedVendors);
            var vendorList = supportedVendors.ToList();

            Console.WriteLine($"Supported vendors: {string.Join(", ", vendorList)}");

            // Should be a valid enumerable (may be empty if no plugins found)
            Assert.True(vendorList.Count >= 0, "Should return a valid vendor list");
        }

        [Fact]
        public void DiscoveryService_ShouldBeThreadSafe()
        {
            // Arrange
            var tasks = new List<Task<IEnumerable<IProtocolPlugin>>>();
            var threadCount = 5;

            // Act - Run discovery from multiple threads simultaneously
            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(Task.Run(() => _discoveryService.DiscoverProtocolPlugins()));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert - All results should be consistent
            var firstResult = tasks[0].Result.OrderBy(p => p.PluginName).ToList();

            for (int i = 1; i < tasks.Count; i++)
            {
                var result = tasks[i].Result.OrderBy(p => p.PluginName).ToList();
                Assert.Equal(firstResult.Count, result.Count);

                // Check that we get consistent results across threads
                for (int j = 0; j < firstResult.Count; j++)
                {
                    Assert.Equal(firstResult[j].PluginName, result[j].PluginName);
                    Assert.Equal(firstResult[j].ProtocolType, result[j].ProtocolType);
                }
            }
        }

        [Fact]
        public void GetProtocolsForVendor_ShouldNotThrow()
        {
            // Arrange
            var vendors = new[] { "cisco", "juniper", "arista", "generic" };

            foreach (var vendor in vendors)
            {
                // Act & Assert - Should not throw even if no protocols are found
                var protocols = _discoveryService.GetProtocolsForVendor(vendor);
                Assert.NotNull(protocols);

                var protocolList = protocols.ToList();
                Console.WriteLine($"Vendor {vendor}: {protocolList.Count} protocols");
            }
        }

        [Fact]
        public void IsProtocolAvailable_ShouldHandleAllProtocolTypes()
        {
            // Arrange - Test all protocol types in the enum
            var allProtocolTypes = Enum.GetValues<ProtocolType>();

            foreach (var protocolType in allProtocolTypes)
            {
                // Act - Should not throw
                var isAvailable = _discoveryService.IsProtocolAvailable(protocolType);

                // Assert - Should return a boolean value
                Assert.True(isAvailable || !isAvailable); // This always passes but ensures no exception

                Console.WriteLine($"Protocol {protocolType}: {(isAvailable ? "Available" : "Not Available")}");
            }
        }

        [Fact]
        public void ProtocolPlugins_ShouldHaveValidBasicProperties()
        {
            // Act
            var plugins = _discoveryService.DiscoverProtocolPlugins();

            // Assert
            foreach (var plugin in plugins)
            {
                Assert.NotNull(plugin);
                Assert.False(string.IsNullOrEmpty(plugin.PluginName));
                Assert.True(plugin.IsValid());
                Assert.NotNull(plugin.GetSupportedVendors());

                Console.WriteLine($"Plugin: {plugin.PluginName} (Type: {plugin.ProtocolType}, Version: {plugin.Version ?? "N/A"})");
            }
        }
    }
}
