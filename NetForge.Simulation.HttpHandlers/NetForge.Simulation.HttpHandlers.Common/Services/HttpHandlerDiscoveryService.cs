using System.Reflection;

namespace NetForge.Simulation.HttpHandlers.Common.Services
{
    /// <summary>
    /// Auto-discovery service for HTTP handlers using reflection
    /// </summary>
    public class HttpHandlerDiscoveryService
    {
        private readonly List<Type> _handlerTypes;

        public HttpHandlerDiscoveryService()
        {
            _handlerTypes = new List<Type>();
            DiscoverHandlerTypes();
        }

        /// <summary>
        /// Discover all HTTP handler types in loaded assemblies
        /// </summary>
        public IEnumerable<IHttpHandler> DiscoverHandlers()
        {
            var handlers = new List<IHttpHandler>();

            foreach (var handlerType in _handlerTypes)
            {
                try
                {
                    // Create instance using reflection
                    var handler = (IHttpHandler)Activator.CreateInstance(handlerType)!;
                    handlers.Add(handler);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other handlers
                    Console.WriteLine($"Failed to create handler {handlerType.Name}: {ex.Message}");
                }
            }

            return handlers.OrderByDescending(h => h.Priority);
        }

        /// <summary>
        /// Discover handler types using reflection
        /// </summary>
        private void DiscoverHandlerTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => typeof(IHttpHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                    _handlerTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // Handle cases where some types can't be loaded
                    foreach (var type in ex.Types.Where(t => t != null))
                    {
                        if (typeof(IHttpHandler).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            _handlerTypes.Add(type);
                        }
                    }
                }
            }
        }
    }
}