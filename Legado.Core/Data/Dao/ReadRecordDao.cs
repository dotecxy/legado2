using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 阅读记录数据访问实现（对应 Kotlin 的 ReadRecordDao.kt）
    /// </summary>
    public class ReadRecordDao : IReadRecordDao
    {
        private readonly SQLiteAsyncConnection _database;

        public ReadRecordDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<ReadRecord>> GetAllAsync()
        {
            return await _database.Table<ReadRecord>()
                .OrderByDescending(r => r.ReadTime)
                .ToListAsync();
        }

        public async Task<ReadRecord> GetAsync(string bookName)
        {
            return await _database.Table<ReadRecord>()
                .Where(r => r.BookName == bookName)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params ReadRecord[] records)
        {
            foreach (var record in records)
                await _database.InsertOrReplaceAsync(record);
        }

        public async Task UpdateAsync(params ReadRecord[] records)
        {
            foreach (var record in records)
                await _database.UpdateAsync(record);
        }

        public async Task DeleteAsync(params ReadRecord[] records)
        {
            foreach (var record in records)
                await _database.DeleteAsync(record);
        }

        public async Task ClearAsync()
        {
            await _database.ExecuteAsync("DELETE FROM read_record");
        }
    }
}
