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

    }

}
