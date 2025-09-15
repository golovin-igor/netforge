using NetForge.Simulation.DataTypes.ValueObjects;
using NetForge.Interfaces.Devices;
using System.Collections.Concurrent;
using NetForge.Simulation.Common.Common;
using NetForge.Simulation.Common.Protocols;

namespace NetForge.Simulation.Protocols.EIGRP
{
    /// <summary>
    /// DUAL (Diffusing Update Algorithm) implementation for EIGRP
    /// This is the core algorithm that ensures loop-free routing
    /// </summary>
    public class DualAlgorithm
    {
        private readonly INetworkDevice _device;
        private readonly EigrpState _state;
        private readonly object _computationLock = new object();
        private readonly Dictionary<string, DualComputation> _activeComputations = new();

        public event EventHandler<RouteComputationEventArgs>? RouteComputationCompleted;

        public DualAlgorithm(INetworkDevice device, EigrpState state)
        {
            _device = device;
            _state = state;
        }

        /// <summary>
        /// Process an EIGRP update and run DUAL if necessary
        /// </summary>
        public async Task<bool> ProcessUpdate(EigrpUpdate update)
        {
            var destinationKey = $"{update.Network}_{update.Mask}";

            lock (_computationLock)
            {
                // Update topology table
                UpdateTopologyEntry(update);

                // Check if we need to run DUAL computation
                var currentEntry = GetBestTopologyEntry(destinationKey);

                if (currentEntry == null)
                    return false;

                // Check feasibility condition
                if (!IsFeasible(update, currentEntry))
                {
                    // Start active computation (query neighbors)
                    return StartActiveComputation(destinationKey);
                }
                else
                {
                    // Passive computation (local change only)
                    return RunPassiveComputation(destinationKey);
                }
            }
        }

        /// <summary>
        /// Check the Feasibility Condition: RD < FD
        /// This ensures loop-free paths
        /// </summary>
        private bool IsFeasible(EigrpUpdate update, EigrpTopologyEntry currentBest)
        {
            // Feasibility Condition: Reported Distance < Feasible Distance
            return update.ReportedDistance < currentBest.FeasibleDistance;
        }

        /// <summary>
        /// Run passive computation (local feasible successor available)
        /// </summary>
        private bool RunPassiveComputation(string destinationKey)
        {
            var entries = GetTopologyEntries(destinationKey);
            if (!entries.Any()) return false;

            // Find the best route (lowest feasible distance)
            var successor = entries
                .Where(e => e.RouteState == EigrpRouteState.Passive)
                .OrderBy(e => e.FeasibleDistance)
                .FirstOrDefault();

            if (successor != null)
            {
                // Mark as successor
                foreach (var entry in entries)
                {
                    entry.IsSuccessor = (entry == successor);
                    entry.IsFeasibleSuccessor = false;
                }

                // Find feasible successors (backup paths)
                var feasibleSuccessors = entries
                    .Where(e => e != successor &&
                               e.ReportedDistance < successor.FeasibleDistance &&
                               e.RouteState == EigrpRouteState.Passive)
                    .OrderBy(e => e.FeasibleDistance)
                    .ToList();

                foreach (var fs in feasibleSuccessors)
                {
                    fs.IsFeasibleSuccessor = true;
                }

                // Install route
                InstallRoute(successor);

                _device.AddLogEntry($"EIGRP DUAL: Passive computation completed for {destinationKey}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Start active computation (query neighbors for path information)
        /// </summary>
        private bool StartActiveComputation(string destinationKey)
        {
            if (_activeComputations.ContainsKey(destinationKey))
            {
                _device.AddLogEntry($"EIGRP DUAL: Computation already active for {destinationKey}");
                return false;
            }

            var computation = new DualComputation
            {
                DestinationKey = destinationKey,
                StartTime = DateTime.Now,
                State = DualComputationState.Active,
                RepliesOutstanding = new HashSet<string>()
            };

            // Mark all topology entries for this destination as Active
            var entries = GetTopologyEntries(destinationKey);
            foreach (var entry in entries)
            {
                if (entry.RouteState == EigrpRouteState.Passive)
                {
                    entry.RouteState = EigrpRouteState.Active1;
                    entry.QueryCount++;
                }
            }

            // Send queries to all neighbors
            foreach (var neighbor in _state.Neighbors.Values.Where(n => n.State == EigrpNeighborState.Up))
            {
                SendQuery(neighbor, destinationKey);
                computation.RepliesOutstanding.Add(neighbor.RouterId);
            }

            _activeComputations[destinationKey] = computation;

            // Start SIA timer
            StartSiaTimer(destinationKey);

            _device.AddLogEntry($"EIGRP DUAL: Started active computation for {destinationKey}, waiting for {computation.RepliesOutstanding.Count} replies");
            return true;
        }

        /// <summary>
        /// Process a reply from a neighbor
        /// </summary>
        public async Task<bool> ProcessReply(EigrpReply reply)
        {
            var destinationKey = $"{reply.Network}_{reply.Mask}";
            bool needsCompletion = false;

            lock (_computationLock)
            {
                if (!_activeComputations.TryGetValue(destinationKey, out var computation))
                {
                    _device.AddLogEntry($"EIGRP DUAL: Received unexpected reply for {destinationKey} from {reply.SourceRouter}");
                    return false;
                }

                // Remove from replies outstanding
                computation.RepliesOutstanding.Remove(reply.SourceRouter);

                // Update topology table with reply information
                UpdateTopologyFromReply(reply);

                // Check if all replies received
                if (computation.RepliesOutstanding.Count == 0)
                {
                    needsCompletion = true;
                }
                else
                {
                    _device.AddLogEntry($"EIGRP DUAL: Received reply from {reply.SourceRouter}, {computation.RepliesOutstanding.Count} replies still outstanding");
                }
            }

            // Complete computation outside of lock
            if (needsCompletion)
            {
                return await CompleteActiveComputation(destinationKey);
            }

            return false;
        }

        /// <summary>
        /// Complete active computation when all replies received
        /// </summary>
        private async Task<bool> CompleteActiveComputation(string destinationKey)
        {
            if (!_activeComputations.TryGetValue(destinationKey, out var computation))
                return false;

            var entries = GetTopologyEntries(destinationKey);

            // Find new successor
            var successor = entries
                .Where(e => e.ReportedDistance < long.MaxValue)
                .OrderBy(e => e.FeasibleDistance)
                .FirstOrDefault();

            if (successor != null)
            {
                // Mark routes as passive and update successor status
                foreach (var entry in entries)
                {
                    entry.RouteState = EigrpRouteState.Passive;
                    entry.IsSuccessor = (entry == successor);
                    entry.IsFeasibleSuccessor = false;
                }

                // Find feasible successors
                var feasibleSuccessors = entries
                    .Where(e => e != successor &&
                               e.ReportedDistance < successor.FeasibleDistance)
                    .OrderBy(e => e.FeasibleDistance)
                    .ToList();

                foreach (var fs in feasibleSuccessors)
                {
                    fs.IsFeasibleSuccessor = true;
                }

                // Install new route
                InstallRoute(successor);

                _device.AddLogEntry($"EIGRP DUAL: Active computation completed for {destinationKey}, new successor via {successor.NextHop}");
            }
            else
            {
                // No successor found, remove route
                RemoveRoute(destinationKey);
                _device.AddLogEntry($"EIGRP DUAL: No successor found for {destinationKey}, route removed");
            }

            // Clean up computation
            _activeComputations.Remove(destinationKey);

            // Notify completion
            RouteComputationCompleted?.Invoke(this, new RouteComputationEventArgs
            {
                DestinationKey = destinationKey,
                SuccessorFound = successor != null,
                ComputationTime = DateTime.Now - computation.StartTime
            });

            return true;
        }

        /// <summary>
        /// Handle Stuck-in-Active (SIA) condition
        /// </summary>
        private async Task HandleSiaCondition(string destinationKey)
        {
            if (!_activeComputations.TryGetValue(destinationKey, out var computation))
                return;

            _device.AddLogEntry($"EIGRP DUAL: SIA condition detected for {destinationKey}");

            // Send SIA queries to neighbors that haven't replied
            foreach (var neighborId in computation.RepliesOutstanding.ToList())
            {
                var neighbor = _state.Neighbors.Values.FirstOrDefault(n => n.RouterId == neighborId);
                if (neighbor != null)
                {
                    await SendSiaQuery(neighbor, destinationKey);
                }
            }

            computation.SiaCount++;
            computation.State = DualComputationState.SiaQuery;

            // If too many SIA conditions, reset neighbor relationship
            if (computation.SiaCount > 3)
            {
                _device.AddLogEntry($"EIGRP DUAL: Too many SIA conditions for {destinationKey}, resetting neighbor relationships");

                foreach (var neighborId in computation.RepliesOutstanding.ToList())
                {
                    ResetNeighbor(neighborId);
                }

                // Force completion with available information
                await CompleteActiveComputation(destinationKey);
            }
        }

        private void StartSiaTimer(string destinationKey)
        {
            Task.Run(async () =>
            {
                await Task.Delay(180000); // 3 minutes SIA timer

                if (_activeComputations.ContainsKey(destinationKey))
                {
                    await HandleSiaCondition(destinationKey);
                }
            });
        }

        private void UpdateTopologyEntry(EigrpUpdate update)
        {
            var entryKey = $"{update.Network}_{update.Mask}_via_{update.SourceRouter}";

            _state.TopologyTable[entryKey] = new EigrpTopologyEntry
            {
                Network = update.Network,
                Mask = update.Mask,
                ViaNeighbor = update.SourceRouter,
                NextHop = update.NextHopRouter,
                Interface = update.IncomingInterface,
                FeasibleDistance = update.CompositeMetric,
                ReportedDistance = update.ReportedDistance,
                Metric = update.Metrics,
                RouteState = EigrpRouteState.Passive,
                LastUpdate = DateTime.Now
            };

            _state.TopologyChanged = true;
        }

        private void UpdateTopologyFromReply(EigrpReply reply)
        {
            var entryKey = $"{reply.Network}_{reply.Mask}_via_{reply.SourceRouter}";

            if (reply.IsUnreachable)
            {
                // Remove unreachable route
                _state.TopologyTable.Remove(entryKey);
            }
            else
            {
                // Update with new metrics
                if (_state.TopologyTable.TryGetValue(entryKey, out var entry))
                {
                    entry.FeasibleDistance = reply.CompositeMetric;
                    entry.ReportedDistance = reply.ReportedDistance;
                    entry.Metric = reply.Metrics;
                    entry.LastUpdate = DateTime.Now;
                }
            }
        }

        private EigrpTopologyEntry? GetBestTopologyEntry(string destinationKey)
        {
            return GetTopologyEntries(destinationKey)
                .Where(e => e.RouteState == EigrpRouteState.Passive)
                .OrderBy(e => e.FeasibleDistance)
                .FirstOrDefault();
        }

        private List<EigrpTopologyEntry> GetTopologyEntries(string destinationKey)
        {
            var networkMask = destinationKey.Split('_');
            if (networkMask.Length != 2) return new List<EigrpTopologyEntry>();

            return _state.TopologyTable.Values
                .Where(e => $"{e.Network}_{e.Mask}" == destinationKey)
                .ToList();
        }

        private void InstallRoute(EigrpTopologyEntry successor)
        {
            var route = new EigrpRoute
            {
                Network = successor.Network,
                Mask = successor.Mask,
                NextHop = successor.NextHop,
                Interface = successor.Interface,
                Metric = successor.FeasibleDistance,
                CompositeMetric = successor.Metric,
                AdminDistance = successor.ViaNeighbor == "Connected" ? 0 : 90,
                RouteSource = "EIGRP",
                IsInternal = true,
                InstallTime = DateTime.Now
            };

            var routeKey = $"{route.Network}_{route.Mask}";
            _state.RoutingTable[routeKey] = route;

            // Install in device routing table
            var deviceRoute = new Route(
                route.Network,
                route.Mask,
                route.NextHop,
                route.Interface,
                "EIGRP")
            {
                Metric = (int)Math.Min(route.Metric, int.MaxValue),
                AdminDistance = route.AdminDistance
            };

            _device.ClearRoutesByProtocol("EIGRP");
            _device.AddRoute(deviceRoute);
        }

        private void RemoveRoute(string destinationKey)
        {
            var networkMask = destinationKey.Split('_');
            if (networkMask.Length != 2) return;

            var network = networkMask[0];
            var mask = networkMask[1];

            _device.ClearRoutesByProtocol("EIGRP");

            var routeKey = destinationKey;
            _state.RoutingTable.Remove(routeKey);
        }

        private async Task SendQuery(EigrpNeighbor neighbor, string destinationKey)
        {
            var networkMask = destinationKey.Split('_');
            if (networkMask.Length != 2) return;

            var query = new EigrpQuery
            {
                Network = networkMask[0],
                Mask = networkMask[1],
                QueryingNeighbor = _state.RouterId,
                QueryTime = DateTime.Now,
                RepliesOutstanding = new HashSet<string> { neighbor.RouterId }
            };

            _state.ActiveQueries[destinationKey] = query;

            _device.AddLogEntry($"EIGRP DUAL: Sent query for {destinationKey} to {neighbor.RouterId}");
            await Task.Delay(1); // Simulate network transmission
        }

        private async Task SendSiaQuery(EigrpNeighbor neighbor, string destinationKey)
        {
            _device.AddLogEntry($"EIGRP DUAL: Sent SIA query for {destinationKey} to {neighbor.RouterId}");
            await Task.Delay(1); // Simulate network transmission
        }

        private void ResetNeighbor(string neighborId)
        {
            if (_state.Neighbors.TryGetValue(neighborId, out var neighbor))
            {
                neighbor.State = EigrpNeighborState.Down;
                _device.AddLogEntry($"EIGRP DUAL: Reset neighbor {neighborId} due to SIA condition");
            }
        }
    }

    // Supporting classes for DUAL algorithm
    public class DualComputation
    {
        public string DestinationKey { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DualComputationState State { get; set; }
        public HashSet<string> RepliesOutstanding { get; set; } = new();
        public int SiaCount { get; set; } = 0;
    }

    public enum DualComputationState
    {
        Active,
        SiaQuery,
        Completed
    }

    public class EigrpUpdate
    {
        public string Network { get; set; } = "";
        public string Mask { get; set; } = "";
        public string SourceRouter { get; set; } = "";
        public string NextHopRouter { get; set; } = "";
        public string IncomingInterface { get; set; } = "";
        public long ReportedDistance { get; set; }
        public long CompositeMetric { get; set; }
        public EigrpMetric Metrics { get; set; } = new();
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }

    public class EigrpReply
    {
        public string Network { get; set; } = "";
        public string Mask { get; set; } = "";
        public string SourceRouter { get; set; } = "";
        public long ReportedDistance { get; set; }
        public long CompositeMetric { get; set; }
        public EigrpMetric Metrics { get; set; } = new();
        public bool IsUnreachable { get; set; } = false;
        public DateTime ReplyTime { get; set; } = DateTime.Now;
    }

    public class RouteComputationEventArgs : EventArgs
    {
        public string DestinationKey { get; set; } = "";
        public bool SuccessorFound { get; set; }
        public TimeSpan ComputationTime { get; set; }
    }
}