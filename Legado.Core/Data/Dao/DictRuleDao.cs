using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 字典规则数据访问实现（对应 Kotlin 的 DictRuleDao.kt）
    /// </summary>
    public class DictRuleDao : IDictRuleDao
    {
        private readonly SQLiteAsyncConnection _database;

        public DictRuleDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<DictRule>> GetAllAsync()
        {
            return await _database.Table<DictRule>().ToListAsync();
        }

        public async Task<DictRule> GetAsync(long id)
        {
            return await _database.Table<DictRule>()
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<DictRule> GetAsync(string name)
        {
            return await _database.Table<DictRule>()
                .Where(d => d.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params DictRule[] rules)
        {
            foreach (var rule in rules)
                await _database.InsertOrReplaceAsync(rule);
        }

        public async Task UpdateAsync(params DictRule[] rules)
        {
            foreach (var rule in rules)
                await _database.UpdateAsync(rule);
        }

        public async Task DeleteAsync(params DictRule[] rules)
        {
            foreach (var rule in rules)
                await _database.DeleteAsync(rule);
        }
    }
}
