using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 替换规则数据访问接口（对应 Kotlin 的 ReplaceRuleDao.kt）
    /// </summary>
    public interface IReplaceRuleDao
    {
        Task<List<ReplaceRule>> GetAllAsync();
        Task<List<ReplaceRule>> GetEnabledAsync();
        Task<ReplaceRule> GetAsync(long id);
        Task InsertAsync(params ReplaceRule[] rules);
        Task UpdateAsync(params ReplaceRule[] rules);
        Task DeleteAsync(params ReplaceRule[] rules);
        // TODO: 实现更多查询方法
    }
}
