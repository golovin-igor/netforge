using NetForge.Interfaces.Cli;
using NetForge.Interfaces.Devices;
using NetForge.Interfaces.Vendors;
using NetForge.Simulation.Common.CLI.Base;
using NetForge.Simulation.Common.CLI.Services;
using NetForge.Simulation.Common.Common;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// CLI handler manager that uses vendor descriptors
    /// </summary>
    public class VendorAwareHandlerManager : ICliHandlerManager
    {
        private readonly IVendorService _vendorService;
        private readonly IVendorRegistry _vendorRegistry;
        private readonly INetworkDevice _device;
        private readonly List<ICliHandler> _handlers = new();
        private bool _initialized = false;

        public VendorAwareHandlerManager(
            INetworkDevice device, 
            IVendorService vendorService, 
            IVendorRegistry vendorRegistry)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _vendorService = vendorService ?? throw new ArgumentNullException(nameof(vendorService));
            _vendorRegistry = vendorRegistry ?? throw new ArgumentNullException(nameof(vendorRegistry));
        }

        /// <summary>
        /// Initialize handlers from vendor descriptor
        /// </summary>
        private void InitializeHandlers()
        {
            if (_initialized || string.IsNullOrEmpty(_device.Vendor))
                return;

            var handlers = _vendorService.GetVendorHandlers(_device.Vendor);
            foreach (var handler in handlers)
            {
                _handlers.Add(handler);
            }

            _initialized = true;
        }

        public void RegisterHandler(ICliHandler handler)
        {
            if (handler != null && !_handlers.Contains(handler))
            {
                _handlers.Add(handler);
            }
        }

        public void UnregisterHandler(ICliHandler handler)
        {
            _handlers.Remove(handler);
        }

        public IEnumerable<ICliHandler> GetHandlers()
        {
            InitializeHandlers();
            return _handlers.ToList();
        }

        public ICliHandler? GetHandler(string command)
        {
            InitializeHandlers();
            return _handlers.FirstOrDefault(h => h.CanHandle(command));
        }

        public async Task<CliResult> ProcessCommandAsync(string command)
        {
            InitializeHandlers();
            
            var handler = GetHandler(command);
            if (handler != null)
            {
                return await handler.HandleAsync(command, _device);
            }

            return new CliResult
            {
                Success = false,
                Output = $"% Unknown command: {command}"
            };
        }

        public List<string> GetCompletions(string command)
        {
            InitializeHandlers();
            
            var completions = new List<string>();
            foreach (var handler in _handlers)
            {
                completions.AddRange(handler.GetCompletions(command));
            }
            
            return completions.Distinct().ToList();
        }

        public void ClearHandlers()
        {
            _handlers.Clear();
            _initialized = false;
        }

        public int GetHandlerCount()
        {
            InitializeHandlers();
            return _handlers.Count;
        }

        public bool HasHandler(string command)
        {
            InitializeHandlers();
            return _handlers.Any(h => h.CanHandle(command));
        }
    }
}