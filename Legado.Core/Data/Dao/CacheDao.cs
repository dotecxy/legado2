using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 缓存数据访问实现（对应 Kotlin 的 CacheDao.kt）
    /// </summary>
    public class CacheDao : DapperDao<Cache>, ICacheDao
    {
        public CacheDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<Cache> GetAsync(string key)
        {
            return await GetFirstOrDefaultAsync(c => c.Key == key);
        }

        public new async Task<List<Cache>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        public async Task InsertAsync(params Cache[] caches)
        {
            await InsertOrReplaceAllAsync(caches);
        }

        public async Task UpdateAsync(params Cache[] caches)
        {
            await base.UpdateAllAsync(caches);
        }

        public async Task DeleteAsync(params Cache[] caches)
        {
            await base.DeleteAllAsync(caches);
        }

        public async Task DeleteAsync(string key)
        {
            var sql = "DELETE FROM cache WHERE key = ?";
            await ExecuteAsync(sql, key);
        }

        public async Task ClearExpiredAsync(long time)
        {
            var sql = "DELETE FROM cache WHERE deadline < ? AND deadline > 0";
            await ExecuteAsync(sql, time);
        }

        public async Task DeleteExpiredAsync()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await ClearExpiredAsync(now);
        }
    }
}
