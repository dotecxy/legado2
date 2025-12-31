using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书籍章节数据访问实现（对应 Kotlin 的 BookChapterDao.kt）
    /// </summary>
    public class BookChapterDao : BaseDao<BookChapter>, IBookChapterDao
    {
        public BookChapterDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// 搜索章节（对应 Kotlin 的 search）
        /// </summary>
        public async Task<List<BookChapter>> SearchAsync(string bookUrl, string key)
        {
            var sql = "SELECT * FROM chapters WHERE bookUrl = @a AND title LIKE @b";
            var result = await QueryAsync<BookChapter>(sql, new { a = bookUrl, b = $"%{key}%" });
            return result;
        }

        /// <summary>
        /// 搜索章节（指定范围）
        /// </summary>
        public async Task<List<BookChapter>> SearchAsync(
            string bookUrl,
            string key,
            int start,
            int end)
        {
            var sql = "SELECT * FROM chapters WHERE bookUrl = @a AND `index` >= @b AND `index` <= @c AND title LIKE @d";
            var result = await QueryAsync<BookChapter>(sql, new { a=bookUrl, b=start, c=end, d=$"%{key}%" });
            return result;
        }

        /// <summary>
        /// 获取书籍的所有章节（对应 Kotlin 的 getChapterList）
        /// </summary>
        public async Task<List<BookChapter>> GetChaptersAsync(string bookUrl)
        {
            var sql = "SELECT * FROM chapters WHERE bookUrl = @a ORDER BY `index`";
            var result = await QueryAsync<BookChapter>(sql, new { a= bookUrl });
            return result;
        }

        /// <summary>
        /// 获取指定范围的章节
        /// </summary>
        public async Task<List<BookChapter>> GetChaptersAsync(
            string bookUrl,
            int start,
            int end)
        {
            var sql = "SELECT * FROM chapters WHERE bookUrl = @a AND `index` >= @b AND `index` <= @c ORDER BY `index`";
            var result = await QueryAsync<BookChapter>(sql, new { a = bookUrl, b = start, c = end });
            return result;
        }

        /// <summary>
        /// 根据索引获取章节（对应 Kotlin 的 getChapter）
        /// </summary>
        public async Task<BookChapter> GetChapterAsync(string bookUrl, int index)
        {
            return await GetFirstOrDefaultAsync(c => c.BookUrl == bookUrl && c.Index == index);
        }

        /// <summary>
        /// 根据标题获取章节
        /// </summary>
        public async Task<BookChapter> GetChapterByTitleAsync(string bookUrl, string title)
        {
            return await GetFirstOrDefaultAsync(c => c.BookUrl == bookUrl && c.Title == title);
        }

        /// <summary>
        /// 获取章节数量（对应 Kotlin 的 getChapterCount）
        /// </summary>
        public async Task<int> GetCountAsync(string bookUrl)
        {
            var sql = "SELECT COUNT(*) FROM chapters WHERE bookUrl = @a";
            return await ExecuteScalarAsync<int>(sql, new { a= bookUrl });
        }

        /// <summary>
        /// 插入章节（对应 Kotlin 的 insert）
        /// </summary>
        public async Task InsertAsync(params BookChapter[] chapters)
        {
            if (chapters == null || chapters.Length == 0)
                return;

            await InsertOrReplaceAllAsync(chapters);
        }

        /// <summary>
        /// 更新章节（对应 Kotlin 的 update）
        /// </summary>
        public async Task UpdateAsync(params BookChapter[] chapters)
        {
            if (chapters == null || chapters.Length == 0)
                return;

            await base.UpdateAllAsync(chapters);
        }

        /// <summary>
        /// 删除书籍的所有章节（对应 Kotlin 的 delByBook）
        /// </summary>
        public async Task DeleteAsync(string bookUrl)
        {
            var sql = "DELETE FROM chapters WHERE bookUrl = @a";
            await ExecuteAsync(sql, new { a = bookUrl });
        }

        /// <summary>
        /// 更新章节字数（对应 Kotlin 的 upWordCount）
        /// </summary>
        public async Task UpdateWordCountAsync(string bookUrl, string url, string wordCount)
        {
            var sql = "UPDATE chapters SET word_count = @a WHERE book_url = @b AND url = @c";
            await ExecuteAsync(sql, new { a = wordCount, b = bookUrl, c = url }); 
        }
    }
}
