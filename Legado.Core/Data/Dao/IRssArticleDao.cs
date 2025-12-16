using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 文章数据访问接口（对应 Kotlin 的 RssArticleDao.kt）
    /// </summary>
    public interface IRssArticleDao
    {
        Task<List<RssArticle>> GetByOriginAsync(string origin);
        Task<RssArticle> GetAsync(string origin, string link);
        Task InsertAsync(params RssArticle[] articles);
        Task UpdateAsync(params RssArticle[] articles);
        Task DeleteAsync(params RssArticle[] articles);
        Task DeleteByOriginAsync(string origin);
        // TODO: 实现更多查询方法
    }
}
