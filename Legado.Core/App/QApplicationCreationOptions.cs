using Autofac;
using System;
using System.IO;

namespace Legado.Core
{
    /// <summary>
    /// 应用程序创建选项
    /// </summary>
    public class QApplicationCreationOptions
    {
        /// <summary>
        /// IOC容器构造器
        /// </summary>
        public ContainerBuilder ServiceBuilder { get; }

        /// <summary>
        /// 配置选项
        /// </summary>
        public QConfigurationBuilderOptions Configuration { get; }

        public QApplicationCreationOptions(ContainerBuilder serviceBuilder)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Legado");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            ServiceBuilder = serviceBuilder;
            Configuration = new QConfigurationBuilderOptions();
            Configuration.BasePath = folder;
            Configuration.CommandLineArgs = Environment.GetCommandLineArgs();
        }
    }
}
