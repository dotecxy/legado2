using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 阅读记录数据访问实现（对应 Kotlin 的 RssReadRecordDao.kt）
    /// </summary>
    public class RssReadRecordDao : IRssReadRecordDao
    {
        private readonly SQLiteAsyncConnection _database;

        public RssReadRecordDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<RssReadRecord> GetAsync(string record)
        {
            return await _database.Table<RssReadRecord>()
                .Where(r => r.Record == record)
                .FirstOrDefaultAsync();
        }

        public async Task<List<RssReadRecord>> GetAllAsync()
        {
            return await _database.Table<RssReadRecord>().ToListAsync();
        }

        public async Task InsertAsync(params RssReadRecord[] records)
        {
            foreach (var record in records)
                await _database.InsertOrReplaceAsync(record);
        }

        public async Task DeleteAsync(params RssReadRecord[] records)
        {
            foreach (var record in records)
                await _database.DeleteAsync(record);
        }

        public async Task DeleteAsync(string record)
        {
            await _database.ExecuteAsync(
                "DELETE FROM rss_read_records WHERE record = ?",
                record
            );
        }
    }
}
