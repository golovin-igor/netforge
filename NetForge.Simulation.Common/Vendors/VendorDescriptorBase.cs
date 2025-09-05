using NetForge.Interfaces.Vendors;
using NetForge.Simulation.DataTypes;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Base implementation of vendor descriptor
    /// </summary>
    public abstract class VendorDescriptorBase : IVendorDescriptor
    {
        public abstract string VendorName { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public virtual int Priority => 0;

        private readonly List<DeviceModelDescriptor> _supportedModels = new();
        private readonly List<ProtocolDescriptor> _supportedProtocols = new();
        private readonly List<HandlerDescriptor> _cliHandlers = new();
        private VendorConfiguration _configuration = new();

        public IEnumerable<DeviceModelDescriptor> SupportedModels => _supportedModels;
        public IEnumerable<ProtocolDescriptor> SupportedProtocols => _supportedProtocols;
        public IEnumerable<HandlerDescriptor> CliHandlers => _cliHandlers;
        public VendorConfiguration Configuration => _configuration;

        protected VendorDescriptorBase()
        {
            InitializeVendor();
        }

        /// <summary>
        /// Initialize vendor-specific configuration
        /// </summary>
        protected abstract void InitializeVendor();

        /// <summary>
        /// Add a supported device model
        /// </summary>
        protected void AddModel(DeviceModelDescriptor model)
        {
            _supportedModels.Add(model);
        }

        /// <summary>
        /// Add a supported protocol
        /// </summary>
        protected void AddProtocol(NetworkProtocolType protocolType, string implementationClass, string assemblyName = "")
        {
            _supportedProtocols.Add(new ProtocolDescriptor
            {
                ProtocolType = protocolType,
                ImplementationClass = implementationClass,
                AssemblyName = string.IsNullOrEmpty(assemblyName) ? GetDefaultAssemblyName(protocolType) : assemblyName,
                IsEnabled = true
            });
        }

        /// <summary>
        /// Add a CLI handler
        /// </summary>
        protected void AddHandler(string handlerName, string commandPattern, string implementationClass, 
            HandlerType type, string assemblyName = "")
        {
            _cliHandlers.Add(new HandlerDescriptor
            {
                HandlerName = handlerName,
                CommandPattern = commandPattern,
                ImplementationClass = implementationClass,
                AssemblyName = string.IsNullOrEmpty(assemblyName) ? GetDefaultHandlerAssemblyName() : assemblyName,
                Type = type,
                IsEnabled = true
            });
        }

        /// <summary>
        /// Configure vendor prompts
        /// </summary>
        protected void ConfigurePrompts(string defaultPrompt, string enabledPrompt, string configPrompt)
        {
            _configuration.DefaultPrompt = defaultPrompt;
            _configuration.EnabledPrompt = enabledPrompt;
            _configuration.ConfigPrompt = configPrompt;
        }

        /// <summary>
        /// Add a prompt mode
        /// </summary>
        protected void AddPromptMode(string modeName, string prompt)
        {
            _configuration.PromptModes[modeName] = prompt;
        }

        public virtual bool SupportsModel(string modelName)
        {
            return _supportedModels.Any(m => 
                m.ModelName.Equals(modelName, StringComparison.OrdinalIgnoreCase) ||
                m.ModelFamily.Equals(modelName, StringComparison.OrdinalIgnoreCase));
        }

        public virtual bool SupportsProtocol(NetworkProtocolType protocolType)
        {
            return _supportedProtocols.Any(p => p.ProtocolType == protocolType && p.IsEnabled);
        }

        public virtual ProtocolDescriptor? GetProtocolDescriptor(NetworkProtocolType protocolType)
        {
            return _supportedProtocols.FirstOrDefault(p => p.ProtocolType == protocolType);
        }

        public virtual DeviceModelDescriptor? GetModelDescriptor(string modelName)
        {
            return _supportedModels.FirstOrDefault(m => 
                m.ModelName.Equals(modelName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Get default assembly name for a protocol
        /// </summary>
        protected virtual string GetDefaultAssemblyName(NetworkProtocolType protocolType)
        {
            return $"NetForge.Simulation.Protocols.{protocolType}";
        }

        /// <summary>
        /// Get default handler assembly name
        /// </summary>
        protected virtual string GetDefaultHandlerAssemblyName()
        {
            return $"NetForge.Simulation.CliHandlers.{VendorName}";
        }
    }
}