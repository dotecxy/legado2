using Autofac;
using Legado.Core.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Legado.Core
{
    public class QApplication
    {
        public static QApplication Context { get; private set; }

        public static IServiceProvider QServiceProvider { get; private set; }
        public static QApplicationCreationOptions QCreationOptions { get; private set; }
        public static Type BrowserControlType { get; set; }

        public IServiceProvider ServiceProvider => _serviceProviderAccessor.Value;
        public IContainer ServiceContainer => _serviceContainerAccessor.Value;
        public ContainerBuilder ServiceBuilder { get; private set; }
        public Type StartupModuleType { get; private set; }
        public QApplicationCreationOptions CreationOptions { get; private set; }
        public IReadOnlyList<IQModuleDescriptor> Modules { get; private set; }
        private ObjectAccessor<IServiceProvider> _serviceProviderAccessor;
        private ObjectAccessor<IContainer> _serviceContainerAccessor;

        private QAutofacFactory autofacFactory;
        public QAutofacFactory AutofacFactory
        {
            get
            {
                return autofacFactory;
            }
        }

        public QApplication()
        {
            autofacFactory = new QAutofacFactory();
            autofacFactory.OnContainerCreated += (sender, e) => {
                _serviceContainerAccessor.Value = e;
                _serviceProviderAccessor.Value = new ServiceProviderImp(e);
                QServiceProvider = _serviceProviderAccessor.Value;
            };
            _serviceContainerAccessor = new ObjectAccessor<IContainer>();
            _serviceProviderAccessor = new ObjectAccessor<IServiceProvider>();
            Context = this;
        }

        #region Privte functions
        private void AddCoreServices()
        {
            AddConfiguration();
            AddLogger();
            AddLocalStorage();
            AddText();
        }

        private void AddConfiguration()
        {
            string configFilePath = Path.Combine(CreationOptions.Configuration.BasePath, CreationOptions.Configuration.ConfigFileName);
            ServiceBuilder.AddConfiguration(configFilePath);
        }

        private void AddText()
        {
            ServiceBuilder.AddText();
        }
        private void AddLogger()
        {
            ServiceBuilder.Configure<QLoggerConfiguration>();
            ServiceBuilder.Register(p =>
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

        private void AddLocalStorage()
        {
            string localStorageFilePath = null;
            if (!CreationOptions.Configuration.LocalStorageFileName.IsNullOrEmpty())
            {
                localStorageFilePath = Path.Combine(CreationOptions.Configuration.BasePath, CreationOptions.Configuration.LocalStorageFileName);
            }
            ServiceBuilder.AddLocalStorage(localStorageFilePath);
        }

        private IQModuleDescriptor[] LoadModules()
        {
            ModuleLoader loader = new ModuleLoader();
            return loader.LoadModules(ServiceBuilder, _serviceProviderAccessor, StartupModuleType);
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

        #region Public functions
        public void Build(IHostBuilder builder, Type startupModuleType, Action<QApplicationCreationOptions> optionsAction)
        {
            builder.ConfigureContainer<ContainerBuilder>((hostContext, containerBuilder) =>
            {
                ServiceBuilder = containerBuilder;
                containerBuilder.RegisterInstance<QApplication>(this);
                ServiceBuilder.Register(p => _serviceContainerAccessor.Value).As<IContainer>().SingleInstance();
                ServiceBuilder.Register(p => _serviceProviderAccessor.Value).As<IServiceProvider>().SingleInstance();
                containerBuilder.RegisterBuildCallback((cb) =>
                { 
                    Initialize();
                });


                StartupModuleType = startupModuleType;
                var options = new QApplicationCreationOptions(ServiceBuilder);
                optionsAction?.Invoke(options);
                CreationOptions = options;
                QCreationOptions = options;
                AddCoreServices();
                Modules = LoadModules();
            });
        }

        public void Initialize()
        { 
            InitializeModules();
        }

        public void Shutdown()
        {
            ApplicationShutdownContext context = new ApplicationShutdownContext(ServiceProvider);
            foreach (IQModuleDescriptor qModule in Modules.Reverse())
            {
                qModule.Instance.OnApplicationShutdown(context);
            }
            ServiceProvider.GetService<IQConfiguration>().Save();
            ServiceProvider.GetService<ILocalStorage>().Save();
            _serviceContainerAccessor.Value.Dispose();
        }
        #endregion
    }
}
