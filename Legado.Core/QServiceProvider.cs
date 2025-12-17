using Legado.Core;

namespace System
{
    public static class QServiceProvider
    {
        public static object GetService(Type serviceType)
        {
            return QApplication.QServiceProvider.GetService(serviceType);
        }

        public static TService GetService<TService>()
        {
            return (TService)GetService(typeof(TService));
        }

        public static object LazyGetRequiredService(Type serviceType, ref object reference)
        {
            return QApplication.QServiceProvider.LazyGetRequiredService(serviceType, ref reference);
        }

        public static TService LazyGetRequiredService<TService>(ref TService reference)
        {
            return QApplication.QServiceProvider.LazyGetRequiredService<TService>(ref reference);
        }

        public static bool TryGetService(Type serviceType, out object service)
        {
            return QApplication.QServiceProvider.TryGetService(serviceType, out service);
        }

        public static bool TryGetService<TService>(out TService service)
        {
            return QApplication.QServiceProvider.TryGetService<TService>(out service);
        }
    }
}
