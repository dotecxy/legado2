using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 规则订阅数据访问实现（对应 Kotlin 的 RuleSubDao.kt）
    /// </summary>
    public class RuleSubDao : BaseDao<RuleSub>, IRuleSubDao
    {
        public RuleSubDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<List<RuleSub>> GetAllAsync()
        {
            var list = await base.GetAllAsync();
            list.OrderBy(r => r.CustomOrder);
            return list;
        }

        public async Task<RuleSub> GetAsync(long id)
        {
            return await FindAsync(id);
        }

        public async Task InsertAsync(params RuleSub[] ruleSubs)
        {
            await base.InsertAllAsync(ruleSubs);
        }

        public async Task UpdateAsync(params RuleSub[] ruleSubs)
        {
            await base.UpdateAllAsync(ruleSubs);
        }

        public async Task DeleteAsync(params RuleSub[] ruleSubs)
        {
            await base.DeleteAllAsync(ruleSubs);
        }
         
    }
}
