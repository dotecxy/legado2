using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 收藏数据访问实现（对应 Kotlin 的 RssStarDao.kt）
    /// </summary>
    public class RssStarDao : IRssStarDao
    {
        private readonly SQLiteAsyncConnection _database;

        public RssStarDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<RssStar>> GetAllAsync()
        {
            return await _database.Table<RssStar>()
                .OrderByDescending(s => s.StarTime)
                .ToListAsync();
        }

        public async Task<RssStar> GetAsync(string origin, string link)
        {
            return await _database.Table<RssStar>()
                .Where(s => s.Origin == origin && s.Link == link)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params RssStar[] stars)
        {
            foreach (var star in stars)
                await _database.InsertOrReplaceAsync(star);
        }

        public async Task UpdateAsync(params RssStar[] stars)
        {
            foreach (var star in stars)
                await _database.UpdateAsync(star);
        }

        public async Task DeleteAsync(params RssStar[] stars)
        {
            foreach (var star in stars)
                await _database.DeleteAsync(star);
        }
    }
}
