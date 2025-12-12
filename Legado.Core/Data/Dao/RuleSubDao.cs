using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 规则订阅数据访问实现（对应 Kotlin 的 RuleSubDao.kt）
    /// </summary>
    public class RuleSubDao : IRuleSubDao
    {
        private readonly SQLiteAsyncConnection _database;

        public RuleSubDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<RuleSub>> GetAllAsync()
        {
            return await _database.Table<RuleSub>()
                .OrderBy(r => r.CustomOrder)
                .ToListAsync();
        }

        public async Task<RuleSub> GetAsync(long id)
        {
            return await _database.Table<RuleSub>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params RuleSub[] ruleSubs)
        {
            foreach (var sub in ruleSubs)
                await _database.InsertOrReplaceAsync(sub);
        }

        public async Task UpdateAsync(params RuleSub[] ruleSubs)
        {
            foreach (var sub in ruleSubs)
                await _database.UpdateAsync(sub);
        }

        public async Task DeleteAsync(params RuleSub[] ruleSubs)
        {
            foreach (var sub in ruleSubs)
                await _database.DeleteAsync(sub);
        }
    }
}
