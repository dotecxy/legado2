using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书签数据访问实现（对应 Kotlin 的 BookmarkDao.kt）
    /// </summary>
    public class BookmarkDao : IBookmarkDao
    {
        private readonly SQLiteAsyncConnection _database;

        public BookmarkDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<Bookmark>> GetByBookAsync(string bookUrl)
        {
            return await _database.Table<Bookmark>()
                .Where(b => b.BookUrl == bookUrl)
                .OrderBy(b => b.BookmarkChapterIndex)
                .ToListAsync();
        }

        public async Task<Bookmark> GetAsync(long time)
        {
            return await _database.Table<Bookmark>()
                .Where(b => b.BookmarkTime == time)
                .FirstOrDefaultAsync();
        }

        public async Task<Bookmark> GetByIndexAsync(string bookUrl, int chapterIndex)
        {
            return await _database.Table<Bookmark>()
                .Where(b => b.BookUrl == bookUrl && b.BookmarkChapterIndex == chapterIndex)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params Bookmark[] bookmarks)
        {
            foreach (var bookmark in bookmarks)
                await _database.InsertOrReplaceAsync(bookmark);
        }

        public async Task UpdateAsync(params Bookmark[] bookmarks)
        {
            foreach (var bookmark in bookmarks)
                await _database.UpdateAsync(bookmark);
        }

        public async Task DeleteAsync(params Bookmark[] bookmarks)
        {
            foreach (var bookmark in bookmarks)
                await _database.DeleteAsync(bookmark);
        }
    }
}
