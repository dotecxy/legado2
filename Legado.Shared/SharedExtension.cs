using Autofac;
using Legado.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;
using MudBlazor.Services;
using Legado.Core.Models.WebBooks;

namespace Legado.Shared
{
    public static class SharedExtension
    {

        public static void AddSharedExtension(this IHostBuilder builder)
        {
            builder.ConfigureServices((c, s) =>
            {
                s.AddMudServices();
                s.AddSingleton(s => new AppStates());
                s.AddSingleton(s => new WebBook()); 
                // Register a default implementation that does nothing for non-Windows platforms
                // The actual Windows implementation will override this in the Windows project
                s.AddSingleton<IWindowTitleBar, DefaultWindowTitleBarService>();
            });
        } 
    }

    [DependsOn(typeof(QuickCoreModule))]
    public class SharedModule : QModule
    {
        public SharedModule()
        {
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnApplicationInitialization(context);
        }

        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            base.PreConfigureServices(context);
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        { 
            TaskHelper.WaitAsync(() =>
            {
                var func = context.ServiceProvider.GetRequiredService<LegadoContext>().SaveToShelfAsync();
                return func;
            });
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            base.ConfigureServices(context);
            context.ServiceBuilder.RegisterAssemblyTypes(typeof(SharedModule).Assembly)
                .Where(t => t.Name.EndsWith("Service") && t.Name != nameof(SharedModule))
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }

}
