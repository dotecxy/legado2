using Autofac;
using Autofac.Extensions.DependencyInjection;
using Legado.Core;
using Legado.Core.App;
using Legado.Core.Data;
using Legado.Core.Data.Dao;
using Legado.Core.Data.Entities;
using Legado.Core.DependencyInjection;
using Legado.Core.Helps;
using Legado.Core.Helps.Source;
using Legado.Core.Utils;
using Legado.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Resources;
using System.Text;

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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ApplicationConfiguration.Initialize();
            WinformHostedService service = new WinformHostedService();
            service.Run();
        }

        public sealed class WinformHostedService : IHostedService
        {
            private IServiceProvider serviceProvider;

            public void Run()
            {
                var builder = Host.CreateDefaultBuilder();
                builder.UseServiceProviderFactory(new QAutofacFactory(typeof(WinformModule)))
                    .ConfigureContainer<ContainerBuilder>(cb =>
                    {
                        cb.RegisterType<Form1>().AsSelf().SingleInstance();
                    });
                builder.ConfigureServices(s =>
                {
                    s.AddWindowsFormsBlazorWebView();
                    s.AddHostedService<WinformHostedService>((_) => this);
                    s.AddBlazorWebViewDeveloperTools();
                    s.AddSingleton<IEventProvider, WinformEventProvider>();
                });
                builder.AddSharedExtension();
                var host = builder.Build();
                serviceProvider = host.Services;
                host.Run();
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                if (serviceProvider.TryGetService<Form1>(out var form))
                {

                    Application.Run(form);
                }
                else
                {
                    Application.Run(new Form1(serviceProvider));
                }
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        public sealed class WinformEventProvider : IEventProvider
        {
            public void DragMove(int x, int y)
            {

            }

            public ResourceManager GetResourceManager()
            {
                return Resource.ResourceManager;
            }
        }

        [SingletonDependency]
        [ExposeServices(serviceTypes: typeof(IWinDialog))]
        public sealed class WinformWinDialog : IWinDialog
        {
            public async Task<bool> ShowBrowserDialogAsync(IntentData data)
            { 
                return await Task.Factory.StartNew(() =>
                {
                    using AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                    if (QServiceProvider.TryGetService<Form1>(out var form))
                    {
                        form.BeginInvoke( () =>
                        {
                            var url = "";
                            BrowserWinDialog dlg = new BrowserWinDialog(url = data.Get<string>("url")) { Text = data.Get<string>("title") };
                            dlg.FormClosed += async (sender, e) =>
                            {
                                var cookie = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(dlg.Cookie);
                                SourceVerificationHelper.SetResult(data.Get<string>("sourceOrigin"), cookie);
                                CookieEntity cookieEntity = new CookieEntity()
                                {
                                    Url = NetworkUtils.GetDomain(url),
                                    Cookie = cookie
                                };
                                 
                                await AppDatabase.GetInstance("test.db").CookieDao.InsertOrReplaceAsync(cookieEntity);
                                autoResetEvent.Set();
                            };
                            dlg.Show();
                            return true;
                        });
                    }
                    autoResetEvent.WaitOne();
                    return true;
                });


            }

            public Task<bool> ShowDialogAsync(IntentData data)
            {
                return Task.FromResult(false);
            }
        }

        [DependsOn(typeof(SharedModule))]
        public sealed class WinformModule : QModule
        {
            public WinformModule()
            {

            }

            public override void OnApplicationInitialization(ApplicationInitializationContext context)
            {
                base.OnApplicationInitialization(context);
                //var application = context.ServiceProvider.GetService<QApplication>();
            }
        }
    }
}