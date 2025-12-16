using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 服务器数据访问实现（对应 Kotlin 的 ServerDao.kt）
    /// </summary>
    public class ServerDao : DapperDao<Server>, IServerDao
    {
        public ServerDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<Server> GetAsync(long id)
        {
            return await FindAsync(id);
        }

        public async Task InsertAsync(params Server[] servers)
        {
            await InsertOrReplaceAllAsync(servers);
        }

        public async Task UpdateAsync(params Server[] servers)
        {
            await base.UpdateAllAsync(servers);
        }

        public async Task DeleteAsync(params Server[] servers)
        {
            await base.DeleteAllAsync(servers);
        }
    }
}
