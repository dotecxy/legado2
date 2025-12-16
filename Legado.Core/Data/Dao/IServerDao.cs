using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 服务器数据访问接口（对应 Kotlin 的 ServerDao.kt）
    /// </summary>
    public interface IServerDao
    {
        Task<List<Server>> GetAllAsync();
        Task<Server> GetAsync(long id);
        Task InsertAsync(params Server[] servers);
        Task UpdateAsync(params Server[] servers);
        Task DeleteAsync(params Server[] servers);
        // TODO: 实现更多查询方法
    }
}
