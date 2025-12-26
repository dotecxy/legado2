using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// TXT 目录规则数据访问实现（对应 Kotlin 的 TxtTocRuleDao.kt）
    /// </summary>
    public class TxtTocRuleDao : ProxyDao<TxtTocRule>, ITxtTocRuleDao
    {
        public TxtTocRuleDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<List<TxtTocRule>> GetAllAsync()
        {
            var sql = "SELECT * FROM txtTocRules ORDER BY serialNumber";
            var result = await QueryAsync<TxtTocRule>(sql);
            return result;
        }

        public async Task<List<TxtTocRule>> GetEnabledAsync()
        {
            var sql = "SELECT * FROM txtTocRules WHERE enable = 1 ORDER BY serialNumber";
            var result = await QueryAsync<TxtTocRule>(sql);
            return result;
        }

        public async Task<TxtTocRule> GetAsync(long id)
        {
            return await FindAsync(id);
        }

        public async Task InsertAsync(params TxtTocRule[] rules)
        {
            await InsertOrReplaceAllAsync(rules);
        }

        public async Task UpdateAsync(params TxtTocRule[] rules)
        {
            await base.UpdateAllAsync(rules);
        }

        public async Task DeleteAsync(params TxtTocRule[] rules)
        {
            await base.DeleteAllAsync(rules);
        }
    }
}
