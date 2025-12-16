using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 收藏数据访问接口（对应 Kotlin 的 RssStarDao.kt）
    /// </summary>
    public interface IRssStarDao
    {
        Task<List<RssStar>> GetAllAsync();
        Task<RssStar> GetAsync(string origin, string link);
        Task InsertAsync(params RssStar[] stars);
        Task UpdateAsync(params RssStar[] stars);
        Task DeleteAsync(params RssStar[] stars);
        // TODO: 实现更多查询方法
    }
}
