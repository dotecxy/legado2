using System;
using System.Data;

namespace Legado.Core
{
    public interface IDbContext : IDisposable
    { 
        string ConnectionString { get; }
        IServiceProvider ServiceProvider { get; }
    }
}
