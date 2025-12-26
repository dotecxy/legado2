using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Legado.Core.DependencyInjection
{
    /// <summary>
    /// A factory for creating a <see cref="ContainerBuilder"/> and an <see cref="IServiceProvider" />.
    /// </summary>
    public class QAutofacFactory : IServiceProviderFactory<ContainerBuilder>
    {
        private readonly Action<ContainerBuilder, QApplicationCreationOptions> _configurationAction;
        private readonly ContainerBuildOptions _containerBuildOptions = ContainerBuildOptions.None;
        private readonly ObjectAccessor<IContainer> _containerAccessor = new ObjectAccessor<IContainer>();
        private readonly ObjectAccessor<IServiceProvider> _serviceProviderAccessor = new ObjectAccessor<IServiceProvider>();

        public IContainer Container { get; private set; }
        public IServiceProvider ServiceProvider { get; set; }
        public QApplicationCreationOptions CreationOptions { get; private set; }
        public IReadOnlyList<IQModuleDescriptor> Modules { get; private set; }

        public Type StartupModuleType { get; private set; }

        public event EventHandler<IContainer> OnContainerCreated;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacServiceProviderFactory"/> class.
        /// </summary>
        /// <param name="containerBuildOptions">The container options to use when building the container.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
        public QAutofacFactory(Type startModuleType,
            ContainerBuildOptions containerBuildOptions = ContainerBuildOptions.None,
            Action<ContainerBuilder, QApplicationCreationOptions>? configurationAction = null)
            : this(configurationAction)
        {
            _containerBuildOptions = containerBuildOptions;
            StartupModuleType = startModuleType;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacServiceProviderFactory"/> class.
        /// </summary>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container..</param>
        public QAutofacFactory(Action<ContainerBuilder, QApplicationCreationOptions>? configurationAction = null) =>
            _configurationAction = configurationAction ?? ((_, _) => { });

        /// <summary>
        /// Creates a container builder from an <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The collection of services.</param>
        /// <returns>A container builder that can be used to create an <see cref="IServiceProvider" />.</returns>
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services);

            builder.Register(_ => this._containerAccessor.Value).As<IContainer>().SingleInstance();

            QApplicationCreationOptions opt = new QApplicationCreationOptions(builder);
            _configurationAction(builder, opt);
            CreationOptions = opt;

            //builder.RegisterInstance<QApplicationCreationOptions>(opt);

            AddCoreServices(builder);
            Modules = LoadModules(builder);

            return builder;
        }

        #region Privte functions
        private void AddCoreServices(ContainerBuilder builder)
        {
            AddConfiguration(builder);
            AddLogger(builder);
            AddLocalStorage(builder);
            AddText(builder);
        }

        private void AddConfiguration(ContainerBuilder builder)
        {
            string configFilePath = Path.Combine(CreationOptions.Configuration.BasePath, CreationOptions.Configuration.ConfigFileName);
            builder.AddConfiguration(configFilePath);
        }

        private void AddText(ContainerBuilder builder)
        {
            builder.AddText();
        }
        private void AddLogger(ContainerBuilder builder)
        {
            builder.Configure<QLoggerConfiguration>();
            builder.Register(p =>
            {
                QLoggerConfiguration qLogCfg = p.Resolve<QLoggerConfiguration>();
                if (qLogCfg.SinkTypes.IsNullOrEmpty())
                {
                    qLogCfg.SinkTypes = new List<LoggerSinkType>();
                    qLogCfg.SinkTypes.Add(LoggerSinkType.Console);
                    qLogCfg.SinkTypes.Add(LoggerSinkType.File);
                }

                var loggerConfig = new LoggerConfiguration();
                loggerConfig.MinimumLevel.Is(qLogCfg.ConsoleLevel < qLogCfg.FileLevel ? qLogCfg.ConsoleLevel : qLogCfg.FileLevel);

                if (qLogCfg.SinkTypes.Contains(LoggerSinkType.Console))
                {
                    loggerConfig.WriteTo.Debug(qLogCfg.ConsoleLevel, qLogCfg.OutputTemplate);
                }

                if (qLogCfg.SinkTypes.Contains(LoggerSinkType.File))
                {
                    string logFileName = null;
                    if (!qLogCfg.LogStorageDir.IsNullOrEmpty())
                    {
                        logFileName = Path.Combine(qLogCfg.LogStorageDir, qLogCfg.FileName);
                    }
                    else
                    {
                        logFileName = Path.Combine(CreationOptions.Configuration.BasePath, QProperties.LogDefaultStorageDir, qLogCfg.FileName);
                    }
                    loggerConfig.WriteTo.File(path: logFileName, restrictedToMinimumLevel: qLogCfg.FileLevel,
                        outputTemplate: qLogCfg.OutputTemplate, rollingInterval: qLogCfg.RollingInterval);
                }
                var logger = loggerConfig.CreateLogger();
                var factory = new SerilogLoggerFactory(logger);
                return logger;
            }).As<Serilog.ILogger>().SingleInstance();
        }

        private void AddLocalStorage(ContainerBuilder builder)
        {
            string localStorageFilePath = null;
            if (!CreationOptions.Configuration.LocalStorageFileName.IsNullOrEmpty())
            {
                localStorageFilePath = Path.Combine(CreationOptions.Configuration.BasePath, CreationOptions.Configuration.LocalStorageFileName);
            }
            builder.AddLocalStorage(localStorageFilePath);
        }

        private IQModuleDescriptor[] LoadModules(ContainerBuilder builder)
        {
            ModuleLoader loader = new ModuleLoader();
            return loader.LoadModules(builder, _serviceProviderAccessor, this.StartupModuleType);
        }

        private void InitializeModules()
        {
            ApplicationInitializationContext context = new ApplicationInitializationContext(ServiceProvider);
            foreach (IQModuleDescriptor qModule in Modules)
            {
                qModule.Instance.OnPreApplicationInitialization(context);
            }

            foreach (IQModuleDescriptor qModule in Modules)
            {
                qModule.Instance.OnApplicationInitialization(context);
            }

            foreach (IQModuleDescriptor qModule in Modules)
            {
                qModule.Instance.OnPostApplicationInitialization(context);
            }
        }
        #endregion

        /// <summary>
        /// Creates an <see cref="IServiceProvider" /> from the container builder.
        /// </summary>
        /// <param name="containerBuilder">The container builder.</param>
        /// <returns>An <see cref="IServiceProvider" />.</returns>
        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }

            var container = Container = containerBuilder.Build(_containerBuildOptions);
            _containerAccessor.Value = container;


            OnContainerCreated?.Invoke(this, container);

            ServiceProvider = QServiceProvider.ServiceProvider = new AutofacServiceProvider(container);
            _serviceProviderAccessor.Value = ServiceProvider;

            InitializeModules();

            return ServiceProvider;
        }

    }

}