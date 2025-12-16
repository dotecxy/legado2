using System;
using System.Data;

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
                if (_connectionString == null)
                {
                    string connStringName = ConnectionStringNameAttribute.GetConnStringName(GetType());
                    var dbConfig = _serviceProvider.GetService<DatabaseConfiguration>();
                    if (!dbConfig.ContainsKey(connStringName))
                    {
                        throw new QException($"The database connection name {connStringName} dose not exist in configuration.");
                    }
                    _connectionString = dbConfig[connStringName];
                }
                return _connectionString;
            }
        }

        private readonly IServiceProvider _serviceProvider;
        public IServiceProvider ServiceProvider => _serviceProvider;
         

        public void Dispose()
        { 
        }
    }
}
