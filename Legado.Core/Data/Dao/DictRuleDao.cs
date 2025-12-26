using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 字典规则数据访问实现（对应 Kotlin 的 DictRuleDao.kt）
    /// </summary>
    public class DictRuleDao : ProxyDao<DictRule>, IDictRuleDao
    {
        public DictRuleDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<List<DictRule>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        public async Task<DictRule> GetAsync(long id)
        {
            return await FindAsync(id);
        }

        public async Task<DictRule> GetAsync(string name)
        {
            return await GetFirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task InsertAsync(params DictRule[] rules)
        {
            await InsertOrReplaceAllAsync(rules);
        }

        public async Task UpdateAsync(params DictRule[] rules)
        {
            await base.UpdateAllAsync(rules);
        }

        public async Task DeleteAsync(params DictRule[] rules)
        {
            await base.DeleteAllAsync(rules);
        }
    }
}
