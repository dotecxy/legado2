using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 文章数据访问实现（对应 Kotlin 的 RssArticleDao.kt）
    /// </summary>
    public class RssArticleDao : ProxyDao<RssArticle>, IRssArticleDao
    {
        public RssArticleDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<RssArticle>> GetByOriginAsync(string origin)
        {
            var sql = "SELECT * FROM rssArticles WHERE origin = ? ORDER BY pubDate DESC";
            var result = await QueryAsync<RssArticle>(sql, origin);
            return result;
        }

        public async Task<RssArticle> GetAsync(string origin, string link)
        {
            return await GetFirstOrDefaultAsync(a => a.Origin == origin && a.Link == link);
        }

        public async Task InsertAsync(params RssArticle[] articles)
        {
            await InsertOrReplaceAllAsync(articles);
        }

        public async Task UpdateAsync(params RssArticle[] articles)
        {
            await base.UpdateAllAsync(articles);
        }

        public async Task DeleteAsync(params RssArticle[] articles)
        {
            await base.DeleteAllAsync(articles);
        }

        public async Task DeleteByOriginAsync(string origin)
        {
            var sql = "DELETE FROM rssArticles WHERE origin = ?";
            await ExecuteAsync(sql, origin);
        }
    }
}
