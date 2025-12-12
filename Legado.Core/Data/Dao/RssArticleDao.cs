using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 文章数据访问实现（对应 Kotlin 的 RssArticleDao.kt）
    /// </summary>
    public class RssArticleDao : IRssArticleDao
    {
        private readonly SQLiteAsyncConnection _database;

        public RssArticleDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<RssArticle>> GetByOriginAsync(string origin)
        {
            return await _database.Table<RssArticle>()
                .Where(a => a.Origin == origin)
                .OrderByDescending(a => a.PubDate)
                .ToListAsync();
        }

        public async Task<RssArticle> GetAsync(string origin, string link)
        {
            return await _database.Table<RssArticle>()
                .Where(a => a.Origin == origin && a.Link == link)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params RssArticle[] articles)
        {
            foreach (var article in articles)
                await _database.InsertOrReplaceAsync(article);
        }

        public async Task UpdateAsync(params RssArticle[] articles)
        {
            foreach (var article in articles)
                await _database.UpdateAsync(article);
        }

        public async Task DeleteAsync(params RssArticle[] articles)
        {
            foreach (var article in articles)
                await _database.DeleteAsync(article);
        }

        public async Task DeleteByOriginAsync(string origin)
        {
            await _database.ExecuteAsync(
                "DELETE FROM rss_articles WHERE origin = ?",
                origin
            );
        }
    }
}
