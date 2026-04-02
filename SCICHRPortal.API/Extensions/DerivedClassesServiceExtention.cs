using System.Reflection;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.API.Extensions
{
    public static class DerivedClassesServiceExtension
    {
        public static IServiceCollection AddDerivedClassesServices(this IServiceCollection services)
        {
            var loadedPaths = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic)
                .Select(a => a.Location).ToArray();
            var toLoad = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
            var assembliesLoaded = toLoad.Where(x => AssemblyName.GetAssemblyName(x).Name!.StartsWith("SCICHRPortal.Repository")
                                                    || AssemblyName.GetAssemblyName(x).Name! == "SCICHRPortal.Service")
                .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x))).ToList();

            var scopedServiceType = typeof(IScopedService);
            var scopedServices = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => scopedServiceType.IsAssignableFrom(p))
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterfaces().FirstOrDefault(),
                    Implementation = t
                })
                .Where(t => t.Service != null);

            foreach (var scopedService in scopedServices)
            {
                if (scopedServiceType.IsAssignableFrom(scopedService.Service))
                {
                    services.AddScoped(scopedService.Service, scopedService.Implementation);
                }
            }

            return services;
        }
    }
}
