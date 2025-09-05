namespace NetForge.Interfaces.Vendors
{
    /// <summary>
    /// Security and access control capabilities
    /// </summary>
    public interface ISecurityCapabilities
    {
        /// <summary>
        /// Apply access group to an interface
        /// </summary>
        bool ApplyAccessGroup(string interfaceName, int aclNumber, string direction);

        /// <summary>
        /// Remove access group from an interface
        /// </summary>
        bool RemoveAccessGroup(string interfaceName);

        /// <summary>
        /// Add an ACL entry to the specified access list
        /// </summary>
        bool AddAclEntry(int aclNumber, object aclEntry);

        /// <summary>
        /// Set the current ACL number for configuration
        /// </summary>
        bool SetCurrentAclNumber(int aclNumber);

        /// <summary>
        /// Get the current ACL number
        /// </summary>
        int GetCurrentAclNumber();
    }
}