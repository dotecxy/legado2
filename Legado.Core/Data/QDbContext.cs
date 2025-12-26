using Legado.Core.Data;
using System;
using System.Data;
using System.IO;

namespace Legado.Core
{
    public abstract class QDbContext : IDbContext
    {
        public QDbContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private string _connectionString;
        public string ConnectionString
        {
            get
            {
                var options = _serviceProvider.GetService<QApplicationCreationOptions>();
                return Path.Combine(options?.Configuration?.BasePath ?? AppDomain.CurrentDomain.BaseDirectory, AppDatabase.DatabaseName);
            }
        }

        private readonly IServiceProvider _serviceProvider;
        public IServiceProvider ServiceProvider => _serviceProvider;


        public void Dispose()
        {
        }
    }
}
