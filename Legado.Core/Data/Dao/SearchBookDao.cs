using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 搜索书籍数据访问实现（对应 Kotlin 的 SearchBookDao.kt）
    /// </summary>
    public class SearchBookDao : ISearchBookDao
    {
        private readonly SQLiteAsyncConnection _database;

        public SearchBookDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// 根据 URL 获取搜索书籍（对应 Kotlin 的 getSearchBook）
        /// </summary>
        public async Task<SearchBook> GetByUrlAsync(string bookUrl)
        {
            return await _database.Table<SearchBook>()
                .Where(s => s.BookUrl == bookUrl)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根据书名和作者获取（对应 Kotlin 的 getFirstByNameAuthor）
        /// </summary>
        public async Task<SearchBook> GetAsync(string name, string author)
        {
            // TODO: 需要联表查询 book_sources，这里简化实现
            var books = await _database.Table<SearchBook>()
                .Where(s => s.Name == name && s.Author == author)
                .OrderBy(s => s.OriginOrder)
                .ToListAsync();
            
            return books.FirstOrDefault();
        }

        /// <summary>
        /// 按书名查询
        /// </summary>
        public async Task<List<SearchBook>> GetByNameAsync(string name)
        {
            return await _database.Table<SearchBook>()
                .Where(s => s.Name == name)
                .ToListAsync();
        }

        /// <summary>
        /// 换源查询（按分组）- 对应 Kotlin 的 changeSourceByGroup
        /// </summary>
        public async Task<List<SearchBook>> ChangeSourceByGroupAsync(
            string name,
            string author,
            string sourceGroup)
        {
            // TODO: 需要实现联表查询
            // 简化实现：先获取所有匹配的书籍
            var books = await _database.Table<SearchBook>()
                .Where(s => s.Name == name && s.Author.Contains(author))
                .OrderBy(s => s.OriginOrder)
                .ToListAsync();
            
            return books;
        }

        /// <summary>
        /// 换源搜索 - 对应 Kotlin 的 changeSourceSearch
        /// </summary>
        public async Task<List<SearchBook>> ChangeSourceSearchAsync(
            string name,
            string author,
            string key,
            string sourceGroup)
        {
            // TODO: 需要实现联表查询和模糊搜索
            var books = await _database.Table<SearchBook>()
                .Where(s => s.Name == name && s.Author.Contains(author))
                .OrderBy(s => s.OriginOrder)
                .ToListAsync();
            
            // 在内存中过滤
            return books.Where(b =>
                (b.OriginName?.Contains(key) ?? false) ||
                (b.LatestChapterTitle?.Contains(key) ?? false)
            ).ToList();
        }

        /// <summary>
        /// 获取有封面的书籍 - 对应 Kotlin 的 getEnableHasCover
        /// </summary>
        public async Task<List<SearchBook>> GetEnableHasCoverAsync(string name, string author)
        {
            // TODO: 需要联表查询
            var books = await _database.Table<SearchBook>()
                .Where(s => s.Name == name && s.Author == author)
                .OrderBy(s => s.OriginOrder)
                .ToListAsync();
            
            return books.Where(b => !string.IsNullOrEmpty(b.CoverUrl)).ToList();
        }

        /// <summary>
        /// 插入搜索书籍
        /// </summary>
        public async Task InsertAsync(params SearchBook[] searchBooks)
        {
            if (searchBooks == null || searchBooks.Length == 0)
                return;

            foreach (var book in searchBooks)
            {
                await _database.InsertOrReplaceAsync(book);
            }
        }

        /// <summary>
        /// 更新搜索书籍
        /// </summary>
        public async Task UpdateAsync(params SearchBook[] searchBooks)
        {
            if (searchBooks == null || searchBooks.Length == 0)
                return;

            foreach (var book in searchBooks)
            {
                await _database.UpdateAsync(book);
            }
        }

        /// <summary>
        /// 删除搜索书籍
        /// </summary>
        public async Task DeleteAsync(params SearchBook[] searchBooks)
        {
            if (searchBooks == null || searchBooks.Length == 0)
                return;

            foreach (var book in searchBooks)
            {
                await _database.DeleteAsync(book);
            }
        }

        /// <summary>
        /// 清空指定书籍的搜索结果（对应 Kotlin 的 clear）
        /// </summary>
        public async Task ClearAsync(string name, string author)
        {
            await _database.ExecuteAsync(
                "DELETE FROM searchBooks WHERE name = ? AND author = ?",
                name,
                author
            );
        }

        /// <summary>
        /// 清空所有搜索结果
        /// </summary>
        public async Task ClearAsync()
        {
            await _database.ExecuteAsync("DELETE FROM searchBooks");
        }

        /// <summary>
        /// 清空过期的搜索结果（对应 Kotlin 的 clearExpired）
        /// </summary>
        public async Task ClearExpiredAsync(long time)
        {
            await _database.ExecuteAsync(
                "DELETE FROM searchBooks WHERE time < ?",
                time
            );
        }
    }
}
