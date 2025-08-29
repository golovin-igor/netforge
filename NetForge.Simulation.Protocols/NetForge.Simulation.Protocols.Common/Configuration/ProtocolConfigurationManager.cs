using NetForge.Simulation.Common.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Protocols.Common.Configuration
{
    /// <summary>
    /// Interface for protocol configuration management with validation and templates
    /// </summary>
    public interface IProtocolConfigurationManager
    {
        // Configuration access
        T GetConfiguration<T>(NetworkProtocolType type) where T : class;
        Task<bool> ApplyConfiguration<T>(NetworkProtocolType type, T configuration) where T : class;
        bool ValidateConfiguration<T>(T configuration) where T : class;
        IEnumerable<string> GetConfigurationErrors<T>(T configuration) where T : class;

        // Configuration templates and defaults
        T GetDefaultConfiguration<T>(NetworkProtocolType type) where T : class;
        IEnumerable<T> GetConfigurationTemplates<T>(NetworkProtocolType type) where T : class;
        Task<bool> SaveConfigurationTemplate<T>(NetworkProtocolType type, string templateName, T configuration) where T : class;
        Task<bool> DeleteConfigurationTemplate(NetworkProtocolType type, string templateName);

        // Configuration comparison and merging
        Dictionary<string, object> CompareConfigurations<T>(T config1, T config2) where T : class;
        T MergeConfigurations<T>(T baseConfig, T overrideConfig) where T : class;

        // Configuration backup and restore
        Task<string> BackupConfiguration(NetworkProtocolType type);
        Task<bool> RestoreConfiguration(NetworkProtocolType type, string backupData);
        IEnumerable<string> GetConfigurationBackups(NetworkProtocolType type);

        // Configuration validation rules
        void RegisterValidationRule<T>(Func<T, bool> rule, string errorMessage) where T : class;
        void RegisterValidationRule<T>(NetworkProtocolType type, Func<T, bool> rule, string errorMessage) where T : class;
    }

    /// <summary>
    /// Base interface for all protocol configurations
    /// Provides common configuration properties and validation
    /// </summary>
    public interface IProtocolConfiguration
    {
        /// <summary>
        /// Whether the protocol is enabled
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Protocol name for identification
        /// </summary>
        string ProtocolName { get; }

        /// <summary>
        /// Configuration version for compatibility checking
        /// </summary>
        string ConfigurationVersion { get; }

        /// <summary>
        /// Validate the configuration
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValid();

        /// <summary>
        /// Get configuration validation errors
        /// </summary>
        /// <returns>List of validation error messages</returns>
        IEnumerable<string> GetValidationErrors();

        /// <summary>
        /// Get configuration as dictionary for serialization
        /// </summary>
        /// <returns>Configuration data dictionary</returns>
        Dictionary<string, object> ToDictionary();

        /// <summary>
        /// Load configuration from dictionary
        /// </summary>
        /// <param name="data">Configuration data</param>
        void FromDictionary(Dictionary<string, object> data);
    }

    /// <summary>
    /// Base class for protocol configurations with common functionality
    /// </summary>
    public abstract class BaseProtocolConfiguration : IProtocolConfiguration
    {
        [Required]
        public bool IsEnabled { get; set; } = true;

        public abstract string ProtocolName { get; }

        public virtual string ConfigurationVersion => "1.0";

        /// <summary>
        /// Additional configuration metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Timestamp when configuration was last modified
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.Now;

        /// <summary>
        /// User who last modified the configuration
        /// </summary>
        public string ModifiedBy { get; set; } = "System";

        /// <summary>
        /// Validate the configuration using data annotations and custom rules
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public virtual bool IsValid()
        {
            var errors = GetValidationErrors();
            return !errors.Any();
        }

        /// <summary>
        /// Get configuration validation errors
        /// </summary>
        /// <returns>List of validation error messages</returns>
        public virtual IEnumerable<string> GetValidationErrors()
        {
            var errors = new List<string>();
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();

            // Data annotation validation
            if (!Validator.TryValidateObject(this, context, results, true))
            {
                errors.AddRange(results.Select(r => r.ErrorMessage));
            }

            // Custom validation
            errors.AddRange(ValidateCustomRules());

            return errors;
        }

        /// <summary>
        /// Implement custom validation rules in derived classes
        /// </summary>
        /// <returns>List of custom validation errors</returns>
        protected virtual IEnumerable<string> ValidateCustomRules()
        {
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Get configuration as dictionary for serialization
        /// </summary>
        /// <returns>Configuration data dictionary</returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["IsEnabled"] = IsEnabled,
                ["ProtocolName"] = ProtocolName,
                ["ConfigurationVersion"] = ConfigurationVersion,
                ["LastModified"] = LastModified,
                ["ModifiedBy"] = ModifiedBy,
                ["Metadata"] = Metadata
            };
        }

        /// <summary>
        /// Load configuration from dictionary
        /// </summary>
        /// <param name="data">Configuration data</param>
        public virtual void FromDictionary(Dictionary<string, object> data)
        {
            if (data.TryGetValue("IsEnabled", out var enabled))
                IsEnabled = Convert.ToBoolean(enabled);

            if (data.TryGetValue("LastModified", out var lastMod))
                LastModified = Convert.ToDateTime(lastMod);

            if (data.TryGetValue("ModifiedBy", out var modBy))
                ModifiedBy = modBy?.ToString() ?? "System";

            if (data.TryGetValue("Metadata", out var meta) && meta is Dictionary<string, object> metadata)
                Metadata = metadata;
        }

        /// <summary>
        /// Create a deep copy of the configuration
        /// </summary>
        /// <returns>Deep copy of configuration</returns>
        public virtual BaseProtocolConfiguration Clone()
        {
            var json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<BaseProtocolConfiguration>(json);
        }

        /// <summary>
        /// Update the modification timestamp and user
        /// </summary>
        /// <param name="modifiedBy">User making the modification</param>
        public virtual void MarkAsModified(string modifiedBy = "System")
        {
            LastModified = DateTime.Now;
            ModifiedBy = modifiedBy;
        }
    }

    /// <summary>
    /// Concrete implementation of protocol configuration manager
    /// </summary>
    public class ProtocolConfigurationManager : IProtocolConfigurationManager
    {
        private readonly Dictionary<NetworkProtocolType, Dictionary<string, object>> _configurations = new();
        private readonly Dictionary<NetworkProtocolType, Dictionary<string, object>> _templates = new();
        private readonly Dictionary<NetworkProtocolType, List<string>> _backups = new();
        private readonly Dictionary<Type, List<(Func<object, bool> rule, string error)>> _globalValidationRules = new();
        private readonly Dictionary<NetworkProtocolType, Dictionary<Type, List<(Func<object, bool> rule, string error)>>> _protocolValidationRules = new();

        /// <summary>
        /// Get configuration for a specific protocol
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <returns>Configuration instance or null if not found</returns>
        public T GetConfiguration<T>(NetworkProtocolType type) where T : class
        {
            if (_configurations.TryGetValue(type, out var configs) &&
                configs.TryGetValue(typeof(T).Name, out var config))
            {
                return config as T;
            }

            return null;
        }

        /// <summary>
        /// Apply configuration to a protocol with validation
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <param name="configuration">Configuration to apply</param>
        /// <returns>True if successfully applied, false otherwise</returns>
        public async Task<bool> ApplyConfiguration<T>(NetworkProtocolType type, T configuration) where T : class
        {
            try
            {
                // Validate configuration
                if (!ValidateConfiguration(configuration))
                {
                    return false;
                }

                // Store configuration
                if (!_configurations.ContainsKey(type))
                {
                    _configurations[type] = new Dictionary<string, object>();
                }

                _configurations[type][typeof(T).Name] = configuration;

                // Update modification timestamp if it's a base protocol configuration
                if (configuration is BaseProtocolConfiguration baseConfig)
                {
                    baseConfig.MarkAsModified();
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log error in real implementation
                Console.WriteLine($"Error applying configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate a configuration using registered rules
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="configuration">Configuration to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool ValidateConfiguration<T>(T configuration) where T : class
        {
            var errors = GetConfigurationErrors(configuration);
            return !errors.Any();
        }

        /// <summary>
        /// Get validation errors for a configuration
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="configuration">Configuration to validate</param>
        /// <returns>List of validation errors</returns>
        public IEnumerable<string> GetConfigurationErrors<T>(T configuration) where T : class
        {
            var errors = new List<string>();

            // Built-in validation for IProtocolConfiguration
            if (configuration is IProtocolConfiguration protocolConfig)
            {
                errors.AddRange(protocolConfig.GetValidationErrors());
            }

            // Global validation rules
            var configType = typeof(T);
            if (_globalValidationRules.TryGetValue(configType, out var globalRules))
            {
                foreach (var (rule, error) in globalRules)
                {
                    if (!rule(configuration))
                    {
                        errors.Add(error);
                    }
                }
            }

            // Protocol-specific validation rules
            foreach (var protocolRules in _protocolValidationRules.Values)
            {
                if (protocolRules.TryGetValue(configType, out var rules))
                {
                    foreach (var (rule, error) in rules)
                    {
                        if (!rule(configuration))
                        {
                            errors.Add(error);
                        }
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Get default configuration for a protocol
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <returns>Default configuration instance</returns>
        public T GetDefaultConfiguration<T>(NetworkProtocolType type) where T : class
        {
            // In a real implementation, this would load from a configuration file or database
            // For now, create a default instance using reflection
            try
            {
                return Activator.CreateInstance<T>();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get configuration templates for a protocol
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <returns>Enumerable of template configurations</returns>
        public IEnumerable<T> GetConfigurationTemplates<T>(NetworkProtocolType type) where T : class
        {
            if (_templates.TryGetValue(type, out var templates))
            {
                return templates.Values.OfType<T>();
            }

            return Enumerable.Empty<T>();
        }

        /// <summary>
        /// Save a configuration template
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <param name="templateName">Template name</param>
        /// <param name="configuration">Configuration to save as template</param>
        /// <returns>True if successfully saved, false otherwise</returns>
        public async Task<bool> SaveConfigurationTemplate<T>(NetworkProtocolType type, string templateName, T configuration) where T : class
        {
            try
            {
                if (!_templates.ContainsKey(type))
                {
                    _templates[type] = new Dictionary<string, object>();
                }

                _templates[type][templateName] = configuration;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete a configuration template
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <param name="templateName">Template name</param>
        /// <returns>True if successfully deleted, false otherwise</returns>
        public async Task<bool> DeleteConfigurationTemplate(NetworkProtocolType type, string templateName)
        {
            if (_templates.TryGetValue(type, out var templates))
            {
                return templates.Remove(templateName);
            }

            return false;
        }

        /// <summary>
        /// Compare two configurations and return differences
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="config1">First configuration</param>
        /// <param name="config2">Second configuration</param>
        /// <returns>Dictionary of differences</returns>
        public Dictionary<string, object> CompareConfigurations<T>(T config1, T config2) where T : class
        {
            var differences = new Dictionary<string, object>();

            // Convert to dictionaries for comparison
            if (config1 is IProtocolConfiguration proto1 && config2 is IProtocolConfiguration proto2)
            {
                var dict1 = proto1.ToDictionary();
                var dict2 = proto2.ToDictionary();

                foreach (var key in dict1.Keys.Union(dict2.Keys))
                {
                    var value1 = dict1.GetValueOrDefault(key);
                    var value2 = dict2.GetValueOrDefault(key);

                    if (!Equals(value1, value2))
                    {
                        differences[key] = new { Old = value1, New = value2 };
                    }
                }
            }

            return differences;
        }

        /// <summary>
        /// Merge configurations with override taking precedence
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="baseConfig">Base configuration</param>
        /// <param name="overrideConfig">Override configuration</param>
        /// <returns>Merged configuration</returns>
        public T MergeConfigurations<T>(T baseConfig, T overrideConfig) where T : class
        {
            // In a real implementation, this would use reflection or serialization
            // to merge object properties intelligently
            try
            {
                var json = JsonSerializer.Serialize(overrideConfig);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return baseConfig;
            }
        }

        /// <summary>
        /// Backup configuration to JSON string
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>Backup data as JSON string</returns>
        public async Task<string> BackupConfiguration(NetworkProtocolType type)
        {
            try
            {
                if (_configurations.TryGetValue(type, out var config))
                {
                    var backup = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

                    // Store backup reference
                    if (!_backups.ContainsKey(type))
                    {
                        _backups[type] = new List<string>();
                    }

                    var backupId = $"{type}_{DateTime.Now:yyyyMMdd_HHmmss}";
                    _backups[type].Add(backupId);

                    return backup;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Restore configuration from backup data
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <param name="backupData">Backup data as JSON string</param>
        /// <returns>True if successfully restored, false otherwise</returns>
        public async Task<bool> RestoreConfiguration(NetworkProtocolType type, string backupData)
        {
            try
            {
                var config = JsonSerializer.Deserialize<Dictionary<string, object>>(backupData);
                _configurations[type] = config;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get list of available configuration backups
        /// </summary>
        /// <param name="type">Protocol type</param>
        /// <returns>List of backup identifiers</returns>
        public IEnumerable<string> GetConfigurationBackups(NetworkProtocolType type)
        {
            return _backups.GetValueOrDefault(type, new List<string>());
        }

        /// <summary>
        /// Register global validation rule for a configuration type
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="rule">Validation rule function</param>
        /// <param name="errorMessage">Error message if rule fails</param>
        public void RegisterValidationRule<T>(Func<T, bool> rule, string errorMessage) where T : class
        {
            var configType = typeof(T);
            if (!_globalValidationRules.ContainsKey(configType))
            {
                _globalValidationRules[configType] = new List<(Func<object, bool>, string)>();
            }

            _globalValidationRules[configType].Add((obj => rule((T)obj), errorMessage));
        }

        /// <summary>
        /// Register protocol-specific validation rule
        /// </summary>
        /// <typeparam name="T">Configuration type</typeparam>
        /// <param name="type">Protocol type</param>
        /// <param name="rule">Validation rule function</param>
        /// <param name="errorMessage">Error message if rule fails</param>
        public void RegisterValidationRule<T>(NetworkProtocolType type, Func<T, bool> rule, string errorMessage) where T : class
        {
            if (!_protocolValidationRules.ContainsKey(type))
            {
                _protocolValidationRules[type] = new Dictionary<Type, List<(Func<object, bool>, string)>>();
            }

            var configType = typeof(T);
            if (!_protocolValidationRules[type].ContainsKey(configType))
            {
                _protocolValidationRules[type][configType] = new List<(Func<object, bool>, string)>();
            }

            _protocolValidationRules[type][configType].Add((obj => rule((T)obj), errorMessage));
        }
    }
}
