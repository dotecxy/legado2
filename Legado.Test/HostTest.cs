using Legado.Core;
using Legado.Core.App;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Legado.Test
{

    public sealed class TestApplication
    {
        private QApplication qApplication;
        public TestApplication(Type moduleType = null)
        {
            if (moduleType == null)
            {
                moduleType = typeof(TestModule);
            }

            var host = Host.CreateDefaultBuilder()
              .UseQApplication(moduleType, (opt) =>
              {
                  opt.Configuration.BasePath = Directory.GetCurrentDirectory() + @"\data2";
              })
              .Build();
            qApplication = host.Services.GetService<QApplication>();
            qApplication.Initialize();
            host.RunAsync();
        }
    }

    [DependsOn(typeof(QuickCoreModule))]
    public sealed class TestModule : QModule
    {
        public TestModule()
        {

        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnApplicationInitialization(context);
            var application = context.ServiceProvider.GetService<QApplication>();
        }
    }
}
