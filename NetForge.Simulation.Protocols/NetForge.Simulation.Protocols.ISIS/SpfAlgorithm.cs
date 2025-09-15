using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Protocols;
using System.Collections.Generic;

namespace NetForge.Simulation.Protocols.ISIS
{
    /// <summary>
    /// Implementation of Dijkstra's Shortest Path First (SPF) algorithm for IS-IS
    /// </summary>
    public class SpfAlgorithm
    {
        private readonly INetworkDevice _device;
        private readonly IsisState _state;

        public SpfAlgorithm(INetworkDevice device, IsisState state)
        {
            _device = device;
            _state = state;
        }

        /// <summary>
        /// Run the complete SPF calculation
        /// </summary>
        public async Task<SpfResult> RunSpf()
        {
            var result = new SpfResult
            {
                StartTime = DateTime.Now,
                RootSystem = _state.SystemId
            };

            try
            {
                // Build the network topology from LSP database
                var topology = BuildTopology();

                // Run Dijkstra's algorithm
                var spfTree = await RunDijkstra(topology, _state.SystemId);

                // Convert SPF tree to routes
                var routes = GenerateRoutes(spfTree, topology);

                result.Routes = routes;
                result.IsSuccessful = true;
                result.NodesProcessed = spfTree.Count;

                _device.AddLogEntry($"IS-IS SPF: Processed {result.NodesProcessed} nodes, calculated {routes.Count} routes");
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = ex.Message;
                _device.AddLogEntry($"IS-IS SPF: Error during calculation - {ex.Message}");
            }
            finally
            {
                result.EndTime = DateTime.Now;
                result.CalculationTime = result.EndTime - result.StartTime;
            }

            return result;
        }

        /// <summary>
        /// Build network topology from LSP database
        /// </summary>
        private NetworkTopology BuildTopology()
        {
            var topology = new NetworkTopology();

            // Add all systems as nodes
            foreach (var lsp in _state.LspDatabase.Values.Where(l => !l.IsExpired))
            {
                var node = new TopologyNode
                {
                    SystemId = lsp.OriginatingSystem,
                    Level = lsp.Level,
                    IsOverloaded = lsp.IsOverloaded,
                    Neighbors = new Dictionary<string, int>(),
                    IpPrefixes = new List<IpPrefix>()
                };

                // Parse LSP TLVs to extract topology information
                ParseLspTlvs(lsp, node);

                topology.Nodes[lsp.OriginatingSystem] = node;
            }

            // Add ourselves if not in database
            if (!topology.Nodes.ContainsKey(_state.SystemId))
            {
                var selfNode = CreateSelfNode();
                topology.Nodes[_state.SystemId] = selfNode;
            }

            return topology;
        }

        /// <summary>
        /// Parse LSP TLVs to extract topology information
        /// </summary>
        private void ParseLspTlvs(IsisLsp lsp, TopologyNode node)
        {
            foreach (var tlv in lsp.Tlvs)
            {
                switch (tlv.Type)
                {
                    case 2: // IS Neighbors TLV
                        ParseIsNeighborsTlv(tlv, node);
                        break;

                    case 22: // Extended IS Reachability TLV
                        ParseExtendedIsReachabilityTlv(tlv, node);
                        break;

                    case 128: // IP Internal Reachability TLV
                        ParseIpInternalReachabilityTlv(tlv, node);
                        break;

                    case 130: // IP External Reachability TLV
                        ParseIpExternalReachabilityTlv(tlv, node);
                        break;

                    case 135: // Extended IP Reachability TLV
                        ParseExtendedIpReachabilityTlv(tlv, node);
                        break;
                }
            }
        }

        /// <summary>
        /// Parse IS Neighbors TLV (Type 2)
        /// </summary>
        private void ParseIsNeighborsTlv(IsisTlv tlv, TopologyNode node)
        {
            // Simplified parsing - in real implementation would parse binary format
            var neighborsData = System.Text.Encoding.ASCII.GetString(tlv.Value);
            if (!string.IsNullOrEmpty(neighborsData))
            {
                // For simulation, assume neighbors are space-separated system IDs
                var neighbors = neighborsData.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var neighbor in neighbors)
                {
                    if (!string.IsNullOrEmpty(neighbor.Trim()))
                    {
                        node.Neighbors[neighbor.Trim()] = 10; // Default metric
                    }
                }
            }
        }

        /// <summary>
        /// Parse Extended IS Reachability TLV (Type 22)
        /// </summary>
        private void ParseExtendedIsReachabilityTlv(IsisTlv tlv, TopologyNode node)
        {
            // In real implementation, would parse:
            // - System ID (7 bytes)
            // - Metric (3 bytes)
            // - Sub-TLVs

            // For simulation, use simplified format
            var data = System.Text.Encoding.ASCII.GetString(tlv.Value);
            var parts = data.Split('|');

            for (int i = 0; i < parts.Length - 1; i += 2)
            {
                var systemId = parts[i];
                if (int.TryParse(parts[i + 1], out var metric))
                {
                    node.Neighbors[systemId] = metric;
                }
            }
        }

        /// <summary>
        /// Parse IP Internal Reachability TLV (Type 128)
        /// </summary>
        private void ParseIpInternalReachabilityTlv(IsisTlv tlv, TopologyNode node)
        {
            var reachabilityData = System.Text.Encoding.ASCII.GetString(tlv.Value);
            var prefixes = reachabilityData.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var prefix in prefixes)
            {
                if (ParseIpPrefix(prefix, out var ipPrefix))
                {
                    ipPrefix.RouteType = IsisRouteType.Internal;
                    ipPrefix.Level = node.Level;
                    node.IpPrefixes.Add(ipPrefix);
                }
            }
        }

        /// <summary>
        /// Parse IP External Reachability TLV (Type 130)
        /// </summary>
        private void ParseIpExternalReachabilityTlv(IsisTlv tlv, TopologyNode node)
        {
            var reachabilityData = System.Text.Encoding.ASCII.GetString(tlv.Value);
            var prefixes = reachabilityData.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var prefix in prefixes)
            {
                if (ParseIpPrefix(prefix, out var ipPrefix))
                {
                    ipPrefix.RouteType = IsisRouteType.External;
                    ipPrefix.Level = node.Level;
                    node.IpPrefixes.Add(ipPrefix);
                }
            }
        }

        /// <summary>
        /// Parse Extended IP Reachability TLV (Type 135)
        /// </summary>
        private void ParseExtendedIpReachabilityTlv(IsisTlv tlv, TopologyNode node)
        {
            // Similar to internal reachability but with extended format
            ParseIpInternalReachabilityTlv(tlv, node);
        }

        /// <summary>
        /// Parse IP prefix from string format
        /// </summary>
        private bool ParseIpPrefix(string prefixStr, out IpPrefix ipPrefix)
        {
            ipPrefix = new IpPrefix();

            try
            {
                if (prefixStr.Contains('/'))
                {
                    var parts = prefixStr.Split('/');
                    ipPrefix.Network = parts[0];
                    ipPrefix.PrefixLength = int.Parse(parts[1]);
                    ipPrefix.Mask = ConvertPrefixLengthToMask(ipPrefix.PrefixLength);
                }
                else if (prefixStr.Contains(' '))
                {
                    var parts = prefixStr.Split(' ');
                    ipPrefix.Network = parts[0];
                    ipPrefix.Mask = parts.Length > 1 ? parts[1] : "255.255.255.0";
                    ipPrefix.PrefixLength = ConvertMaskToPrefixLength(ipPrefix.Mask);
                }
                else
                {
                    ipPrefix.Network = prefixStr;
                    ipPrefix.Mask = "255.255.255.0";
                    ipPrefix.PrefixLength = 24;
                }

                ipPrefix.Metric = 10; // Default metric
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create topology node for ourselves
        /// </summary>
        private TopologyNode CreateSelfNode()
        {
            var selfNode = new TopologyNode
            {
                SystemId = _state.SystemId,
                Level = _state.Level,
                IsOverloaded = false,
                Neighbors = new Dictionary<string, int>(),
                IpPrefixes = new List<IpPrefix>()
            };

            // Add our neighbors
            foreach (var neighbor in _state.Neighbors.Values.Where(n => n.IsActive))
            {
                selfNode.Neighbors[neighbor.SystemId] = 10; // Default metric
            }

            // Add our directly connected networks
            foreach (var interfaceName in _device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = _device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp || string.IsNullOrEmpty(interfaceConfig.IpAddress))
                    continue;

                var network = GetNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask ?? "255.255.255.0");
                var prefixLength = ConvertMaskToPrefixLength(interfaceConfig.SubnetMask ?? "255.255.255.0");

                selfNode.IpPrefixes.Add(new IpPrefix
                {
                    Network = network,
                    Mask = interfaceConfig.SubnetMask ?? "255.255.255.0",
                    PrefixLength = prefixLength,
                    Metric = 0, // Directly connected
                    RouteType = IsisRouteType.Internal,
                    Level = _state.Level
                });
            }

            return selfNode;
        }

        /// <summary>
        /// Run Dijkstra's algorithm to build SPF tree
        /// </summary>
        private async Task<Dictionary<string, SpfNode>> RunDijkstra(NetworkTopology topology, string rootSystemId)
        {
            var spfTree = new Dictionary<string, SpfNode>();
            var tentativeNodes = new PriorityQueue<SpfNode, int>();
            var processedNodes = new HashSet<string>();

            // Initialize root node
            var rootNode = new SpfNode
            {
                SystemId = rootSystemId,
                Cost = 0,
                Parent = null,
                NextHop = null,
                Interface = null
            };

            spfTree[rootSystemId] = rootNode;
            tentativeNodes.Enqueue(rootNode, 0);

            _device.AddLogEntry($"IS-IS SPF: Starting Dijkstra from root {rootSystemId}");

            while (tentativeNodes.Count > 0)
            {
                var currentNode = tentativeNodes.Dequeue();

                if (processedNodes.Contains(currentNode.SystemId))
                    continue;

                processedNodes.Add(currentNode.SystemId);

                // Get topology node for current system
                if (!topology.Nodes.TryGetValue(currentNode.SystemId, out var topologyNode))
                    continue;

                // Skip overloaded nodes (except for directly connected)
                if (topologyNode.IsOverloaded && currentNode.Cost > 0)
                {
                    _device.AddLogEntry($"IS-IS SPF: Skipping overloaded node {currentNode.SystemId}");
                    continue;
                }

                // Process neighbors
                foreach (var neighborEntry in topologyNode.Neighbors)
                {
                    var neighborSystemId = neighborEntry.Key;
                    var linkCost = neighborEntry.Value;

                    if (processedNodes.Contains(neighborSystemId))
                        continue;

                    var newCost = currentNode.Cost + linkCost;

                    // Check if we have a better path to this neighbor
                    if (!spfTree.TryGetValue(neighborSystemId, out var existingNode) || newCost < existingNode.Cost)
                    {
                        var neighborNode = new SpfNode
                        {
                            SystemId = neighborSystemId,
                            Cost = newCost,
                            Parent = currentNode.SystemId,
                            NextHop = currentNode.Cost == 0 ? neighborSystemId : currentNode.NextHop ?? neighborSystemId,
                            Interface = currentNode.Cost == 0 ? FindInterfaceToNeighbor(neighborSystemId) : currentNode.Interface
                        };

                        spfTree[neighborSystemId] = neighborNode;
                        tentativeNodes.Enqueue(neighborNode, newCost);

                        _device.AddLogEntry($"IS-IS SPF: Added {neighborSystemId} with cost {newCost} via {neighborNode.NextHop}");
                    }
                }

                // Small delay to simulate processing time
                if (tentativeNodes.Count > 0)
                    await Task.Delay(1);
            }

            _device.AddLogEntry($"IS-IS SPF: Completed Dijkstra, processed {processedNodes.Count} nodes");
            return spfTree;
        }

        /// <summary>
        /// Generate routes from SPF tree
        /// </summary>
        private List<IsisRoute> GenerateRoutes(Dictionary<string, SpfNode> spfTree, NetworkTopology topology)
        {
            var routes = new List<IsisRoute>();

            foreach (var spfNode in spfTree.Values)
            {
                if (spfNode.SystemId == _state.SystemId)
                    continue; // Skip ourselves

                if (!topology.Nodes.TryGetValue(spfNode.SystemId, out var topologyNode))
                    continue;

                // Generate routes for all IP prefixes advertised by this node
                foreach (var ipPrefix in topologyNode.IpPrefixes)
                {
                    var route = new IsisRoute
                    {
                        Destination = ipPrefix.Network,
                        Mask = ipPrefix.Mask,
                        NextHop = FindNextHopIpAddress(spfNode.NextHop),
                        Interface = spfNode.Interface ?? "",
                        Metric = spfNode.Cost + ipPrefix.Metric,
                        Level = ipPrefix.Level,
                        RouteType = ipPrefix.RouteType,
                        OriginatingLsp = $"{spfNode.SystemId}.00-00",
                        LastUpdate = DateTime.Now,
                        Path = BuildPath(spfNode, spfTree)
                    };

                    // Only add if we have a valid next hop
                    if (!string.IsNullOrEmpty(route.NextHop) && !string.IsNullOrEmpty(route.Interface))
                    {
                        routes.Add(route);
                    }
                }
            }

            return routes;
        }

        /// <summary>
        /// Find the interface connected to a specific neighbor
        /// </summary>
        private string? FindInterfaceToNeighbor(string neighborSystemId)
        {
            foreach (var neighbor in _state.Neighbors.Values)
            {
                if (neighbor.SystemId == neighborSystemId && neighbor.IsActive)
                {
                    return neighbor.InterfaceName;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the IP address of a next hop system
        /// </summary>
        private string FindNextHopIpAddress(string? nextHopSystemId)
        {
            if (string.IsNullOrEmpty(nextHopSystemId))
                return "";

            // Find the neighbor with this system ID
            var neighbor = _state.Neighbors.Values.FirstOrDefault(n => n.SystemId == nextHopSystemId && n.IsActive);
            if (neighbor != null)
            {
                // Find the connected device and get its IP address
                var connectedDevice = _device.GetConnectedDevice(neighbor.InterfaceName);
                if (connectedDevice.HasValue)
                {
                    var neighborInterface = connectedDevice.Value.interfaceName;
                    var neighborConfig = connectedDevice.Value.device.GetInterface(neighborInterface);
                    return neighborConfig?.IpAddress ?? "";
                }
            }

            return "";
        }

        /// <summary>
        /// Build the path from root to destination
        /// </summary>
        private List<string> BuildPath(SpfNode destinationNode, Dictionary<string, SpfNode> spfTree)
        {
            var path = new List<string>();
            var currentNode = destinationNode;

            while (currentNode != null)
            {
                path.Insert(0, currentNode.SystemId);

                if (string.IsNullOrEmpty(currentNode.Parent))
                    break;

                spfTree.TryGetValue(currentNode.Parent, out currentNode);
            }

            return path;
        }

        /// <summary>
        /// Utility methods for IP address manipulation
        /// </summary>
        private string GetNetworkAddress(string ipAddress, string subnetMask)
        {
            try
            {
                var ip = System.Net.IPAddress.Parse(ipAddress);
                var mask = System.Net.IPAddress.Parse(subnetMask);

                var ipBytes = ip.GetAddressBytes();
                var maskBytes = mask.GetAddressBytes();
                var networkBytes = new byte[ipBytes.Length];

                for (int i = 0; i < ipBytes.Length; i++)
                {
                    networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                }

                return new System.Net.IPAddress(networkBytes).ToString();
            }
            catch
            {
                return ipAddress;
            }
        }

        private string ConvertPrefixLengthToMask(int prefixLength)
        {
            if (prefixLength < 0 || prefixLength > 32) return "255.255.255.0";

            uint mask = 0xFFFFFFFF << (32 - prefixLength);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        private int ConvertMaskToPrefixLength(string mask)
        {
            try
            {
                var maskBytes = System.Net.IPAddress.Parse(mask).GetAddressBytes();
                int prefixLength = 0;

                foreach (var b in maskBytes)
                {
                    prefixLength += System.Numerics.BitOperations.PopCount((uint)b);
                }

                return prefixLength;
            }
            catch
            {
                return 24; // Default /24
            }
        }
    }

    /// <summary>
    /// Supporting classes for SPF algorithm
    /// </summary>
    public class NetworkTopology
    {
        public Dictionary<string, TopologyNode> Nodes { get; set; } = new();
    }

    public class TopologyNode
    {
        public string SystemId { get; set; } = "";
        public IsisLevel Level { get; set; }
        public bool IsOverloaded { get; set; }
        public Dictionary<string, int> Neighbors { get; set; } = new(); // SystemId -> Metric
        public List<IpPrefix> IpPrefixes { get; set; } = new();
    }

    public class IpPrefix
    {
        public string Network { get; set; } = "";
        public string Mask { get; set; } = "";
        public int PrefixLength { get; set; }
        public int Metric { get; set; }
        public IsisRouteType RouteType { get; set; }
        public IsisLevel Level { get; set; }
    }

    public class SpfNode
    {
        public string SystemId { get; set; } = "";
        public int Cost { get; set; }
        public string? Parent { get; set; }
        public string? NextHop { get; set; }
        public string? Interface { get; set; }
    }

    public class SpfResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan CalculationTime { get; set; }
        public string RootSystem { get; set; } = "";
        public List<IsisRoute> Routes { get; set; } = new();
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; } = "";
        public int NodesProcessed { get; set; }
    }
}