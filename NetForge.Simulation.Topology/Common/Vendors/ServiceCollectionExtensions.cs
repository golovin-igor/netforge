using Microsoft.Extensions.DependencyInjection;
using NetForge.Interfaces.Vendors;

namespace NetForge.Simulation.Common.Vendors
{
    /// <summary>
    /// Extension methods for registering vendor services with IoC container
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add vendor system services to the service collection
        /// </summary>
        public static IServiceCollection AddVendorSystem(this IServiceCollection services)
        {
            // Register vendor registry as singleton
            services.AddSingleton<IVendorRegistry, VendorRegistry>();
            
            // Register vendor service
            services.AddSingleton<IVendorService, VendorService>();

            // Register the initialization callback
            services.AddSingleton(provider =>
            {
                var registry = provider.GetRequiredService<IVendorRegistry>();
                // Discovery will happen when vendors are explicitly registered
                return registry;
            });

            return services;
        }

        /// <summary>
        /// Register a specific vendor descriptor
        /// </summary>
        public static IServiceCollection AddVendor<TVendor>(this IServiceCollection services) 
            where TVendor : class, IVendorDescriptor, new()
        {
            services.AddSingleton<IVendorDescriptor, TVendor>();
            
            // Register with the vendor registry
            services.AddSingleton(provider =>
            {
                var registry = provider.GetRequiredService<IVendorRegistry>();
                var vendor = new TVendor();
                registry.RegisterVendor(vendor);
                return registry;
            });

            return services;
        }

        /// <summary>
        /// Register vendor descriptors from an assembly
        /// </summary>
        public static IServiceCollection AddVendorsFromAssembly(this IServiceCollection services, string assemblyName)
        {
            services.AddSingleton(provider =>
            {
                var registry = provider.GetRequiredService<IVendorRegistry>();
                
                // Load assembly and discover vendors
                var assembly = System.Reflection.Assembly.Load(assemblyName);
                var vendorType = typeof(IVendorDescriptor);
                
                var types = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && 
                               !t.IsInterface && 
                               vendorType.IsAssignableFrom(t));

                foreach (var type in types)
                {
                    var vendor = Activator.CreateInstance(type) as IVendorDescriptor;
                    if (vendor != null)
                    {
                        registry.RegisterVendor(vendor);
                    }
                }

                return registry;
            });

            return services;
        }
    }
}