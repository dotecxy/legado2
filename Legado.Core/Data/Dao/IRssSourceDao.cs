using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 源数据访问接口（对应 Kotlin 的 RssSourceDao.kt）
    /// </summary>
    public interface IRssSourceDao
    {
        Task<List<RssSource>> GetAllAsync();
        Task<List<RssSource>> GetEnabledAsync();
        Task<RssSource> GetAsync(string sourceUrl);
        Task InsertAsync(params RssSource[] sources);
        Task UpdateAsync(params RssSource[] sources);
        Task DeleteAsync(params RssSource[] sources);
        // TODO: 实现更多查询方法
    }
}
