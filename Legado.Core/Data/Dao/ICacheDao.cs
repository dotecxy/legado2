using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 缓存数据访问接口（对应 Kotlin 的 CacheDao.kt）
    /// </summary>
    public interface ICacheDao
    {
        Task<Cache> GetAsync(string key);
        Task<List<Cache>> GetAllAsync();
        Task InsertAsync(params Cache[] caches);
        Task UpdateAsync(params Cache[] caches);
        Task DeleteAsync(params Cache[] caches);
        Task DeleteAsync(string key);
        Task ClearExpiredAsync(long time);
        Task DeleteExpiredAsync();
        // TODO: 实现更多查询方法
    }
}
