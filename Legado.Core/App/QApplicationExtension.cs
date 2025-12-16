using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.App
{
    public static class QApplicationExtension
    {
        public static Microsoft.Extensions.Hosting.IHostBuilder UseQApplication(this Microsoft.Extensions.Hosting.IHostBuilder builder, Type moduleType, Action<QApplicationCreationOptions> action)
        {
            Action<QApplicationCreationOptions> action2 = new Action<QApplicationCreationOptions>((opt) =>
            {
                action?.Invoke(opt);
            });
            QApplication qApplication = new QApplication(moduleType, action2);
            builder.UseServiceProviderFactory(qApplication.AutofacFactory).ConfigureServices((context, serivces) =>
            {
                serivces.AddSingleton<QApplication>(qApplication);
            });
            return builder;
        }
    }
}
