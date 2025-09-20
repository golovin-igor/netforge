using NetForge.Interfaces.Devices;
using NetForge.Simulation.Common.Protocols;
using System.Collections.Concurrent;

namespace NetForge.Simulation.Protocols.ISIS
{
    /// <summary>
    /// Enhanced Link-State Database for IS-IS protocol
    /// Manages LSP storage, aging, flooding, and synchronization
    /// </summary>
    public class LinkStateDatabase
    {
        private readonly ConcurrentDictionary<string, IsisLsp> _database = new();
        private readonly ConcurrentDictionary<string, DateTime> _lspTimers = new();
        private readonly ConcurrentDictionary<string, uint> _sequenceNumbers = new();
        private readonly object _floodingLock = new object();
        private readonly INetworkDevice _device;
        private readonly IsisState _state;

        public event EventHandler<LspEventArgs>? LspAdded;
        public event EventHandler<LspEventArgs>? LspUpdated;
        public event EventHandler<LspEventArgs>? LspRemoved;
        public event EventHandler<LspEventArgs>? LspExpired;

        public LinkStateDatabase(INetworkDevice device, IsisState state)
        {
            _device = device;
            _state = state;

            // Start periodic aging timer
            StartAgingTimer();
        }

        /// <summary>
        /// Add or update an LSP in the database
        /// </summary>
        public LspInstallResult AddOrUpdateLsp(IsisLsp lsp, bool fromNetwork = false)
        {
            var result = new LspInstallResult { LspId = lsp.LspId };

            try
            {
                var existingLsp = _database.TryGetValue(lsp.LspId, out var foundLsp) ? foundLsp : null;

                // Validate LSP
                var validationResult = ValidateLsp(lsp, existingLsp);
                if (!validationResult.IsValid)
                {
                    result.Result = LspInstallResultType.Rejected;
                    result.Reason = validationResult.Reason;
                    return result;
                }

                // Check if this is a newer LSP
                if (existingLsp != null)
                {
                    var comparison = CompareLsps(lsp, existingLsp);
                    if (comparison == LspComparison.Older)
                    {
                        result.Result = LspInstallResultType.Ignored;
                        result.Reason = "LSP is older than existing";
                        return result;
                    }
                    else if (comparison == LspComparison.Same)
                    {
                        // Refresh timer for duplicate LSP
                        _lspTimers[lsp.LspId] = DateTime.Now;
                        result.Result = LspInstallResultType.Refreshed;
                        return result;
                    }
                }

                // Install the LSP
                _database[lsp.LspId] = lsp.Clone();
                _lspTimers[lsp.LspId] = DateTime.Now;
                _sequenceNumbers[lsp.OriginatingSystem] = Math.Max(
                    _sequenceNumbers.TryGetValue(lsp.OriginatingSystem, out var currentSeq) ? currentSeq : 0u,
                    lsp.SequenceNumber);

                // Fire appropriate event
                if (existingLsp == null)
                {
                    result.Result = LspInstallResultType.Added;
                    LspAdded?.Invoke(this, new LspEventArgs { Lsp = lsp });
                    _device.AddLogEntry($"IS-IS LSDB: Added new LSP {lsp.LspId} seq {lsp.SequenceNumber}");
                }
                else
                {
                    result.Result = LspInstallResultType.Updated;
                    LspUpdated?.Invoke(this, new LspEventArgs { Lsp = lsp, PreviousLsp = existingLsp });
                    _device.AddLogEntry($"IS-IS LSDB: Updated LSP {lsp.LspId} seq {lsp.SequenceNumber} (was {existingLsp.SequenceNumber})");
                }

                // Trigger SPF recalculation if this affects routing
                if (ShouldTriggerSpf(lsp, existingLsp))
                {
                    _state.MarkStateChanged();
                    result.TriggeredSpf = true;
                }

                // Schedule flooding if this came from the network
                if (fromNetwork)
                {
                    ScheduleFlooding(lsp);
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Result = LspInstallResultType.Error;
                result.Reason = ex.Message;
                _device.AddLogEntry($"IS-IS LSDB: Error installing LSP {lsp.LspId}: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Remove an LSP from the database
        /// </summary>
        public bool RemoveLsp(string lspId, string reason = "Manual removal")
        {
            if (_database.TryRemove(lspId, out var removedLsp))
            {
                _lspTimers.TryRemove(lspId, out _);

                LspRemoved?.Invoke(this, new LspEventArgs { Lsp = removedLsp });
                _device.AddLogEntry($"IS-IS LSDB: Removed LSP {lspId}: {reason}");

                // Trigger SPF recalculation
                _state.MarkStateChanged();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get an LSP from the database
        /// </summary>
        public IsisLsp? GetLsp(string lspId)
        {
            return _database.GetValueOrDefault(lspId)?.Clone();
        }

        /// <summary>
        /// Get all LSPs from the database
        /// </summary>
        public IEnumerable<IsisLsp> GetAllLsps()
        {
            return _database.Values.Select(lsp => lsp.Clone()).ToList();
        }

        /// <summary>
        /// Get LSPs for a specific level
        /// </summary>
        public IEnumerable<IsisLsp> GetLspsByLevel(IsisLevel level)
        {
            return _database.Values
                .Where(lsp => lsp.Level == level || lsp.Level == IsisLevel.Level1Level2)
                .Select(lsp => lsp.Clone())
                .ToList();
        }

        /// <summary>
        /// Get LSPs originating from a specific system
        /// </summary>
        public IEnumerable<IsisLsp> GetLspsByOriginatingSystem(string systemId)
        {
            return _database.Values
                .Where(lsp => lsp.OriginatingSystem == systemId)
                .Select(lsp => lsp.Clone())
                .ToList();
        }

        /// <summary>
        /// Get next sequence number for a system
        /// </summary>
        public uint GetNextSequenceNumber(string systemId)
        {
            var current = _sequenceNumbers.TryGetValue(systemId, out var sequenceNumber) ? sequenceNumber : 0u;
            return current + 1;
        }

        /// <summary>
        /// Get database statistics
        /// </summary>
        public LsdbStatistics GetStatistics()
        {
            var stats = new LsdbStatistics
            {
                TotalLsps = _database.Count,
                Level1Lsps = _database.Values.Count(l => l.Level == IsisLevel.Level1),
                Level2Lsps = _database.Values.Count(l => l.Level == IsisLevel.Level2),
                Level1Level2Lsps = _database.Values.Count(l => l.Level == IsisLevel.Level1Level2),
                OverloadedLsps = _database.Values.Count(l => l.IsOverloaded),
                ExpiredLsps = _database.Values.Count(l => l.IsExpired),
                OldestLsp = _lspTimers.Values.Any() ? _lspTimers.Values.Min() : DateTime.MaxValue,
                NewestLsp = _lspTimers.Values.Any() ? _lspTimers.Values.Max() : DateTime.MinValue,
                SystemCount = _database.Values.Select(l => l.OriginatingSystem).Distinct().Count()
            };

            return stats;
        }

        /// <summary>
        /// Generate LSP for local system
        /// </summary>
        public IsisLsp GenerateMyLsp(IsisConfig config)
        {
            var lspId = $"{config.SystemId}.00-00";
            var sequenceNumber = GetNextSequenceNumber(config.SystemId);

            var lsp = new IsisLsp
            {
                LspId = lspId,
                SequenceNumber = sequenceNumber,
                RemainingLifetime = (ushort)config.LspMaxLifetime,
                Level = config.Level,
                OriginatingSystem = config.SystemId,
                IsOverloaded = config.IsOverloaded,
                LastUpdate = DateTime.Now,
                Tlvs = new List<IsisTlv>()
            };

            // Build TLVs
            BuildAreaAddressesTlv(lsp, config);
            BuildIsNeighborsTlv(lsp, config);
            BuildIpReachabilityTlv(lsp, config);
            BuildHostnameTlv(lsp, config);

            // Calculate checksum
            lsp.Checksum = CalculateChecksum(lsp);

            return lsp;
        }

        /// <summary>
        /// Validate LSP before installation
        /// </summary>
        private LspValidationResult ValidateLsp(IsisLsp lsp, IsisLsp? existingLsp)
        {
            var result = new LspValidationResult { IsValid = true };

            // Basic validation
            if (string.IsNullOrEmpty(lsp.LspId))
            {
                result.IsValid = false;
                result.Reason = "LSP ID is empty";
                return result;
            }

            if (string.IsNullOrEmpty(lsp.OriginatingSystem))
            {
                result.IsValid = false;
                result.Reason = "Originating system is empty";
                return result;
            }

            if (lsp.RemainingLifetime == 0)
            {
                result.IsValid = false;
                result.Reason = "LSP has zero remaining lifetime";
                return result;
            }

            // Check if this is our own LSP (should not receive our own LSPs from network)
            if (lsp.OriginatingSystem == _state.SystemId && existingLsp == null)
            {
                result.IsValid = false;
                result.Reason = "Received our own LSP from network";
                return result;
            }

            // Validate checksum (simplified for simulation)
            var expectedChecksum = CalculateChecksum(lsp);
            if (lsp.Checksum != 0 && lsp.Checksum != expectedChecksum)
            {
                result.IsValid = false;
                result.Reason = "LSP checksum validation failed";
                return result;
            }

            return result;
        }

        /// <summary>
        /// Compare two LSPs to determine which is newer
        /// </summary>
        private LspComparison CompareLsps(IsisLsp newLsp, IsisLsp existingLsp)
        {
            // Compare sequence numbers
            if (newLsp.SequenceNumber > existingLsp.SequenceNumber)
                return LspComparison.Newer;
            else if (newLsp.SequenceNumber < existingLsp.SequenceNumber)
                return LspComparison.Older;

            // Same sequence number - compare remaining lifetime
            if (newLsp.RemainingLifetime > existingLsp.RemainingLifetime)
                return LspComparison.Newer;
            else if (newLsp.RemainingLifetime < existingLsp.RemainingLifetime)
                return LspComparison.Older;

            // Compare checksum as tiebreaker
            if (newLsp.Checksum > existingLsp.Checksum)
                return LspComparison.Newer;
            else if (newLsp.Checksum < existingLsp.Checksum)
                return LspComparison.Older;

            return LspComparison.Same;
        }

        /// <summary>
        /// Determine if LSP change should trigger SPF recalculation
        /// </summary>
        private bool ShouldTriggerSpf(IsisLsp newLsp, IsisLsp? oldLsp)
        {
            // Always trigger SPF for new LSPs
            if (oldLsp == null)
                return true;

            // Check if topology-affecting TLVs have changed
            var topologyTlvTypes = new byte[] { 2, 22, 128, 130, 135 }; // IS neighbors, Extended IS, IP reachability

            foreach (var tlvType in topologyTlvTypes)
            {
                var newTlvs = newLsp.Tlvs.Where(t => t.Type == tlvType).ToList();
                var oldTlvs = oldLsp.Tlvs.Where(t => t.Type == tlvType).ToList();

                if (newTlvs.Count != oldTlvs.Count)
                    return true;

                for (int i = 0; i < newTlvs.Count; i++)
                {
                    if (!newTlvs[i].Value.SequenceEqual(oldTlvs[i].Value))
                        return true;
                }
            }

            // Check overload bit change
            if (newLsp.IsOverloaded != oldLsp.IsOverloaded)
                return true;

            return false;
        }

        /// <summary>
        /// Schedule flooding of LSP to neighbors
        /// </summary>
        private void ScheduleFlooding(IsisLsp lsp)
        {
            // In a real implementation, this would schedule flooding
            // For simulation, just log the action
            _device.AddLogEntry($"IS-IS LSDB: Scheduling flood of LSP {lsp.LspId} to neighbors");
        }

        /// <summary>
        /// Build Area Addresses TLV
        /// </summary>
        private void BuildAreaAddressesTlv(IsisLsp lsp, IsisConfig config)
        {
            var tlv = new IsisTlv
            {
                Type = 1,
                Description = "Area Addresses",
                Value = System.Text.Encoding.ASCII.GetBytes(config.AreaId)
            };
            tlv.Length = (byte)tlv.Value.Length;
            lsp.Tlvs.Add(tlv);
        }

        /// <summary>
        /// Build IS Neighbors TLV
        /// </summary>
        private void BuildIsNeighborsTlv(IsisLsp lsp, IsisConfig config)
        {
            var neighbors = _state.Neighbors.Values.Where(n => n.IsActive).ToList();
            if (neighbors.Any())
            {
                var neighborData = string.Join(" ", neighbors.Select(n => n.SystemId));
                var tlv = new IsisTlv
                {
                    Type = 2,
                    Description = "IS Neighbors",
                    Value = System.Text.Encoding.ASCII.GetBytes(neighborData)
                };
                tlv.Length = (byte)tlv.Value.Length;
                lsp.Tlvs.Add(tlv);
            }
        }

        /// <summary>
        /// Build IP Reachability TLV
        /// </summary>
        private void BuildIpReachabilityTlv(IsisLsp lsp, IsisConfig config)
        {
            var reachableNetworks = new List<string>();

            foreach (var interfaceName in _device.GetAllInterfaces().Keys)
            {
                var interfaceConfig = _device.GetInterface(interfaceName);
                if (interfaceConfig?.IsShutdown != false || !interfaceConfig.IsUp || string.IsNullOrEmpty(interfaceConfig.IpAddress))
                    continue;

                if (config.Interfaces.GetValueOrDefault(interfaceName, false))
                {
                    var network = GetNetworkAddress(interfaceConfig.IpAddress, interfaceConfig.SubnetMask ?? "255.255.255.0");
                    var prefixLength = ConvertMaskToPrefixLength(interfaceConfig.SubnetMask ?? "255.255.255.0");
                    reachableNetworks.Add($"{network}/{prefixLength}");
                }
            }

            if (reachableNetworks.Any())
            {
                var reachabilityData = string.Join(" ", reachableNetworks);
                var tlv = new IsisTlv
                {
                    Type = 128,
                    Description = "IP Internal Reachability",
                    Value = System.Text.Encoding.ASCII.GetBytes(reachabilityData)
                };
                tlv.Length = (byte)tlv.Value.Length;
                lsp.Tlvs.Add(tlv);
            }
        }

        /// <summary>
        /// Build Hostname TLV
        /// </summary>
        private void BuildHostnameTlv(IsisLsp lsp, IsisConfig config)
        {
            var hostname = _device.Name;
            if (!string.IsNullOrEmpty(hostname))
            {
                var tlv = new IsisTlv
                {
                    Type = 137,
                    Description = "Hostname",
                    Value = System.Text.Encoding.UTF8.GetBytes(hostname)
                };
                tlv.Length = (byte)tlv.Value.Length;
                lsp.Tlvs.Add(tlv);
            }
        }

        /// <summary>
        /// Calculate LSP checksum (simplified)
        /// </summary>
        private ushort CalculateChecksum(IsisLsp lsp)
        {
            // Simplified checksum calculation for simulation
            int checksum = 0;
            checksum += lsp.LspId.GetHashCode() & 0xFFFF;
            checksum += (int)lsp.SequenceNumber & 0xFFFF;
            checksum += lsp.RemainingLifetime;

            foreach (var tlv in lsp.Tlvs)
            {
                checksum += tlv.Type;
                checksum += tlv.Value.Sum(b => b);
            }

            return (ushort)(checksum & 0xFFFF);
        }

        /// <summary>
        /// Start periodic aging timer for LSPs
        /// </summary>
        private void StartAgingTimer()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30)); // Check every 30 seconds
                        await AgeLsps();
                    }
                    catch (Exception ex)
                    {
                        _device.AddLogEntry($"IS-IS LSDB: Error in aging timer: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Age LSPs and remove expired ones
        /// </summary>
        private async Task AgeLsps()
        {
            var now = DateTime.Now;
            var expiredLsps = new List<string>();

            foreach (var kvp in _database.ToList())
            {
                var lsp = kvp.Value;
                var installTime = _lspTimers.GetValueOrDefault(kvp.Key, now);

                // Age the LSP
                var ageSeconds = (int)(now - installTime).TotalSeconds;
                var newLifetime = Math.Max(0, lsp.RemainingLifetime - ageSeconds);

                if (newLifetime == 0)
                {
                    expiredLsps.Add(kvp.Key);
                }
                else if (newLifetime != lsp.RemainingLifetime)
                {
                    // Update remaining lifetime
                    lsp.RemainingLifetime = (ushort)newLifetime;
                }
            }

            // Remove expired LSPs
            foreach (var lspId in expiredLsps)
            {
                if (_database.TryRemove(lspId, out var expiredLsp))
                {
                    _lspTimers.TryRemove(lspId, out _);
                    LspExpired?.Invoke(this, new LspEventArgs { Lsp = expiredLsp });
                    _device.AddLogEntry($"IS-IS LSDB: LSP {lspId} expired and removed");
                    _state.MarkStateChanged(); // Trigger SPF
                }
            }
        }

        /// <summary>
        /// Utility methods
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
                return 24;
            }
        }
    }

    /// <summary>
    /// Supporting classes and enums
    /// </summary>
    public class LspEventArgs : EventArgs
    {
        public IsisLsp Lsp { get; set; } = new();
        public IsisLsp? PreviousLsp { get; set; }
    }

    public class LspInstallResult
    {
        public string LspId { get; set; } = "";
        public LspInstallResultType Result { get; set; }
        public string Reason { get; set; } = "";
        public bool TriggeredSpf { get; set; }
    }

    public enum LspInstallResultType
    {
        Added,
        Updated,
        Refreshed,
        Ignored,
        Rejected,
        Error
    }

    public class LspValidationResult
    {
        public bool IsValid { get; set; }
        public string Reason { get; set; } = "";
    }

    public enum LspComparison
    {
        Newer,
        Older,
        Same
    }

    public class LsdbStatistics
    {
        public int TotalLsps { get; set; }
        public int Level1Lsps { get; set; }
        public int Level2Lsps { get; set; }
        public int Level1Level2Lsps { get; set; }
        public int OverloadedLsps { get; set; }
        public int ExpiredLsps { get; set; }
        public DateTime OldestLsp { get; set; }
        public DateTime NewestLsp { get; set; }
        public int SystemCount { get; set; }
    }
}

namespace NetForge.Simulation.Protocols.ISIS
{
    /// <summary>
    /// Extension methods for LSP
    /// </summary>
    public static class LspExtensions
    {
        public static IsisLsp Clone(this IsisLsp lsp)
        {
            return new IsisLsp
            {
                LspId = lsp.LspId,
                SequenceNumber = lsp.SequenceNumber,
                RemainingLifetime = lsp.RemainingLifetime,
                Checksum = lsp.Checksum,
                Level = lsp.Level,
                Tlvs = lsp.Tlvs.Select(t => new IsisTlv
                {
                    Type = t.Type,
                    Length = t.Length,
                    Value = (byte[])t.Value.Clone(),
                    Description = t.Description
                }).ToList(),
                LastUpdate = lsp.LastUpdate,
                OriginatingSystem = lsp.OriginatingSystem,
                IsOverloaded = lsp.IsOverloaded
            };
        }
    }
}