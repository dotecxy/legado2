using Legado.Core;
using Microsoft.Extensions.DependencyInjection;

namespace System
{
    public static class QServiceProvider
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        public static TService GetService<TService>()
        {
            return (TService)GetService(typeof(TService));
        }

        public static object LazyGetRequiredService(Type serviceType, ref object reference)
        {
            return ServiceProvider.LazyGetRequiredService(serviceType, ref reference);
        }

        public static TService LazyGetRequiredService<TService>(ref TService reference)
        {
            return ServiceProvider.LazyGetRequiredService<TService>(ref reference);
        }

        public static bool TryGetService(Type serviceType, out object service)
        {
            return ServiceProvider.TryGetService(serviceType, out service);
        }

        public static bool TryGetService<TService>(out TService service)
        {
            return ServiceProvider.TryGetService<TService>(out service);
        }
    }
}
