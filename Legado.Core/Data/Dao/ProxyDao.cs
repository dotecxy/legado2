using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
     
    public abstract class ProxyDao<T> :BaseDao<T> where T : class,new()
    {
        protected ProxyDao(IServiceProvider serviceProvider) :base(serviceProvider)
        {
           
        }
         
    }
}
