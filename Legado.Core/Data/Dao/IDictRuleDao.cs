using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 字典规则数据访问接口（对应 Kotlin 的 DictRuleDao.kt）
    /// </summary>
    public interface IDictRuleDao
    {
        Task<List<DictRule>> GetAllAsync();
        Task<DictRule> GetAsync(long id);
        Task<DictRule> GetAsync(string name);
        Task InsertAsync(params DictRule[] dictRules);
        Task UpdateAsync(params DictRule[] dictRules);
        Task DeleteAsync(params DictRule[] dictRules);
        // TODO: 实现更多查询方法
    }
}
