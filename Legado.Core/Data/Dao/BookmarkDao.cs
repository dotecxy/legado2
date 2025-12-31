using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书签数据访问实现（对应 Kotlin 的 BookmarkDao.kt）
    /// </summary>
    public class BookmarkDao : BaseDao<Bookmark>, IBookmarkDao
    {
        public BookmarkDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public async Task<List<Bookmark>> GetByBookAsync(string bookUrl)
        {
            var sql = "SELECT * FROM bookmarks WHERE bookUrl = @a ORDER BY chapterIndex";
            var result = await QueryAsync<Bookmark>(sql, new { a= bookUrl });
            return result;
        }

        public async Task<Bookmark> GetAsync(long time)
        {
            return await FindAsync(time);
        }

        public async Task<Bookmark> GetByIndexAsync(string bookUrl, int chapterIndex)
        {
            var sql = "SELECT * FROM bookmarks WHERE bookUrl = @a AND chapterIndex = @b LIMIT 1";
            var result = await QueryAsync<Bookmark>(sql, new { a=bookUrl, b=chapterIndex });
            return result.FirstOrDefault();
        }

        public async Task InsertAsync(params Bookmark[] bookmarks)
        {
            await InsertOrReplaceAllAsync(bookmarks);
        }

        public async Task UpdateAsync(params Bookmark[] bookmarks)
        {
            await base.UpdateAllAsync(bookmarks);
        }

        public async Task DeleteAsync(params Bookmark[] bookmarks)
        {
            await base.DeleteAllAsync(bookmarks);
        }
    }
}
