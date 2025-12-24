using Autofac.Core;
using Legado.Core.Helps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            QApplication qApplication = new QApplication();
            builder.UseServiceProviderFactory(qApplication.AutofacFactory);
            qApplication.Build(builder, moduleType, action);
            return builder;
        }
         

        public static (bool?, IntentData) StartDialog<T>(this QApplication application, Action<IntentData> dataBuilder) where T : IDialog, new()
        {
            return StartDialog(application, typeof(T), dataBuilder);
        }

        public static (bool?, IntentData) StartDialog(this QApplication application, Type type, Action<IntentData> dataBuilder)
        {
            IntentData intentData = new IntentData();
            dataBuilder?.Invoke(intentData);
            var dialog = Activator.CreateInstance(type) as IDialog;
            dialog.PutIntentData(intentData);
            return (dialog.ShowDailog(), intentData);
        }

        public static (bool?, IntentData) StartBrowserDialog(this QApplication application, Action<IntentData> dataBuilder)
        {
            return StartDialog(application, QApplication.BrowserControlType, dataBuilder);
        }
    }

    public interface IDialog
    {
        void PutIntentData(IntentData data);

        bool? ShowDailog();
    }
}
