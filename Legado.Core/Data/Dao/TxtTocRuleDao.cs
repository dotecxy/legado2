using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// TXT 目录规则数据访问实现（对应 Kotlin 的 TxtTocRuleDao.kt）
    /// </summary>
    public class TxtTocRuleDao : ITxtTocRuleDao
    {
        private readonly SQLiteAsyncConnection _database;

        public TxtTocRuleDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<TxtTocRule>> GetAllAsync()
        {
            return await _database.Table<TxtTocRule>()
                .OrderBy(t => t.SerialNumber)
                .ToListAsync();
        }

        public async Task<List<TxtTocRule>> GetEnabledAsync()
        {
            return await _database.Table<TxtTocRule>()
                .Where(t => t.Enable)
                .OrderBy(t => t.SerialNumber)
                .ToListAsync();
        }

        public async Task<TxtTocRule> GetAsync(long id)
        {
            return await _database.Table<TxtTocRule>()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params TxtTocRule[] rules)
        {
            foreach (var rule in rules)
                await _database.InsertOrReplaceAsync(rule);
        }

        public async Task UpdateAsync(params TxtTocRule[] rules)
        {
            foreach (var rule in rules)
                await _database.UpdateAsync(rule);
        }

        public async Task DeleteAsync(params TxtTocRule[] rules)
        {
            foreach (var rule in rules)
                await _database.DeleteAsync(rule);
        }
    }
}
