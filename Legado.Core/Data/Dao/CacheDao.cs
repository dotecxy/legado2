using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 缓存数据访问实现（对应 Kotlin 的 CacheDao.kt）
    /// </summary>
    public class CacheDao : ICacheDao
    {
        private readonly SQLiteAsyncConnection _database;

        public CacheDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<Cache> GetAsync(string key)
        {
            return await _database.Table<Cache>()
                .Where(c => c.Key == key)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Cache>> GetAllAsync()
        {
            return await _database.Table<Cache>().ToListAsync();
        }

        public async Task InsertAsync(params Cache[] caches)
        {
            foreach (var cache in caches)
                await _database.InsertOrReplaceAsync(cache);
        }

        public async Task UpdateAsync(params Cache[] caches)
        {
            foreach (var cache in caches)
                await _database.UpdateAsync(cache);
        }

        public async Task DeleteAsync(params Cache[] caches)
        {
            foreach (var cache in caches)
                await _database.DeleteAsync(cache);
        }

        public async Task DeleteAsync(string key)
        {
            await _database.ExecuteAsync("DELETE FROM cache WHERE key = ?", key);
        }

        public async Task ClearExpiredAsync(long time)
        {
            await _database.ExecuteAsync("DELETE FROM cache WHERE deadline < ?", time);
        }

        public async Task DeleteExpiredAsync()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await ClearExpiredAsync(now);
        }
    }
}
