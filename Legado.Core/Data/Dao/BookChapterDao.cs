using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书籍章节数据访问实现（对应 Kotlin 的 BookChapterDao.kt）
    /// </summary>
    public class BookChapterDao : IBookChapterDao
    {
        private readonly SQLiteAsyncConnection _database;

        public BookChapterDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// 搜索章节（对应 Kotlin 的 search）
        /// </summary>
        public async Task<List<BookChapter>> SearchAsync(string bookUrl, string key)
        {
            var chapters = await GetChaptersAsync(bookUrl);
            return chapters.Where(c => c.Title?.Contains(key) ?? false).ToList();
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
            var chapters = await GetChaptersAsync(bookUrl);
            return chapters
                .Where(c => c.Index >= start && c.Index <= end && (c.Title?.Contains(key) ?? false))
                .ToList();
        }

        /// <summary>
        /// 获取书籍的所有章节（对应 Kotlin 的 getChapterList）
        /// </summary>
        public async Task<List<BookChapter>> GetChaptersAsync(string bookUrl)
        {
            return await _database.Table<BookChapter>()
                .Where(c => c.BookUrl == bookUrl)
                .OrderBy(c => c.Index)
                .ToListAsync();
        }

        /// <summary>
        /// 获取指定范围的章节
        /// </summary>
        public async Task<List<BookChapter>> GetChaptersAsync(
            string bookUrl,
            int start,
            int end)
        {
            return await _database.Table<BookChapter>()
                .Where(c => c.BookUrl == bookUrl && c.Index >= start && c.Index <= end)
                .OrderBy(c => c.Index)
                .ToListAsync();
        }

        /// <summary>
        /// 根据索引获取章节（对应 Kotlin 的 getChapter）
        /// </summary>
        public async Task<BookChapter> GetChapterAsync(string bookUrl, int index)
        {
            return await _database.Table<BookChapter>()
                .Where(c => c.BookUrl == bookUrl && c.Index == index)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根据标题获取章节
        /// </summary>
        public async Task<BookChapter> GetChapterByTitleAsync(string bookUrl, string title)
        {
            return await _database.Table<BookChapter>()
                .Where(c => c.BookUrl == bookUrl && c.Title == title)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 获取章节数量（对应 Kotlin 的 getChapterCount）
        /// </summary>
        public async Task<int> GetCountAsync(string bookUrl)
        {
            return await _database.Table<BookChapter>()
                .Where(c => c.BookUrl == bookUrl)
                .CountAsync();
        }

        /// <summary>
        /// 插入章节（对应 Kotlin 的 insert）
        /// </summary>
        public async Task InsertAsync(params BookChapter[] chapters)
        {
            if (chapters == null || chapters.Length == 0)
                return;

            foreach (var chapter in chapters)
            {
                await _database.InsertOrReplaceAsync(chapter);
            }
        }

        /// <summary>
        /// 更新章节（对应 Kotlin 的 update）
        /// </summary>
        public async Task UpdateAsync(params BookChapter[] chapters)
        {
            if (chapters == null || chapters.Length == 0)
                return;

            foreach (var chapter in chapters)
            {
                await _database.UpdateAsync(chapter);
            }
        }

        /// <summary>
        /// 删除书籍的所有章节（对应 Kotlin 的 delByBook）
        /// </summary>
        public async Task DeleteAsync(string bookUrl)
        {
            await _database.ExecuteAsync(
                "DELETE FROM chapters WHERE bookUrl = ?",
                bookUrl
            );
        }

        /// <summary>
        /// 更新章节字数（对应 Kotlin 的 upWordCount）
        /// </summary>
        public async Task UpdateWordCountAsync(string bookUrl, string url, string wordCount)
        {
            await _database.ExecuteAsync(
                "UPDATE chapters SET wordCount = ? WHERE bookUrl = ? AND url = ?",
                wordCount,
                bookUrl,
                url
            );
        }
    }
}
