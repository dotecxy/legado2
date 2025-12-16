using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// TXT 目录规则数据访问接口（对应 Kotlin 的 TxtTocRuleDao.kt）
    /// </summary>
    public interface ITxtTocRuleDao
    {
        Task<List<TxtTocRule>> GetAllAsync();
        Task<List<TxtTocRule>> GetEnabledAsync();
        Task<TxtTocRule> GetAsync(long id);
        Task InsertAsync(params TxtTocRule[] rules);
        Task UpdateAsync(params TxtTocRule[] rules);
        Task DeleteAsync(params TxtTocRule[] rules);
        // TODO: 实现更多查询方法
    }
}
