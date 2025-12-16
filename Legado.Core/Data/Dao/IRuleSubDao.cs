using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 规则订阅数据访问接口（对应 Kotlin 的 RuleSubDao.kt）
    /// </summary>
    public interface IRuleSubDao
    {
        Task<List<RuleSub>> GetAllAsync();
        Task<RuleSub> GetAsync(long id);
        Task InsertAsync(params RuleSub[] ruleSubs);
        Task UpdateAsync(params RuleSub[] ruleSubs);
        Task DeleteAsync(params RuleSub[] ruleSubs);
        // TODO: 实现更多查询方法
    }
}
