using Legado.Core;
using Legado.Core.App;
using Microsoft.Extensions.Hosting;

namespace Legado.Windows
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize(); 
            Application.Run(new Form1());


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
}