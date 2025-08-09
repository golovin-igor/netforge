namespace NetSim.Simulation.Interfaces
{
    /// <summary>
    /// Interface for devices that support candidate configuration (e.g., Juniper set/commit)
    /// </summary>
    public interface ICandidateConfiguration
    {
        /// <summary>
        /// Add a configuration line to the candidate configuration
        /// </summary>
        void AddToCandidateConfig(string configLine);
        
        /// <summary>
        /// Get the current candidate configuration
        /// </summary>
        string GetCandidateConfig();
        
        /// <summary>
        /// Clear the candidate configuration
        /// </summary>
        void ClearCandidateConfig();
        
        /// <summary>
        /// Commit the candidate configuration to running configuration
        /// </summary>
        void CommitCandidateConfig();
        
        /// <summary>
        /// Delete a configuration path from candidate configuration
        /// </summary>
        void DeleteFromCandidateConfig(string configPath);
        
        /// <summary>
        /// Check if there are uncommitted changes in candidate configuration
        /// </summary>
        bool HasUncommittedChanges();
        
        /// <summary>
        /// Show differences between candidate and running configuration
        /// </summary>
        string ShowConfigurationDifferences();
        
        /// <summary>
        /// Rollback candidate configuration to running configuration
        /// </summary>
        void RollbackCandidateConfig();
        
        /// <summary>
        /// Load configuration from external source into candidate
        /// </summary>
        void LoadCandidateConfig(string configuration);
    }
} 
