using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 搜索书籍数据访问实现（对应 Kotlin 的 SearchBookDao.kt）
    /// </summary>
    public class SearchBookDao : BaseDao<SearchBook>, ISearchBookDao
    {
        public SearchBookDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// 根据 URL 获取搜索书籍（对应 Kotlin 的 getSearchBook）
        /// </summary>
        public async Task<SearchBook> GetByUrlAsync(string bookUrl)
        {
            return await GetFirstOrDefaultAsync(b => b.BookUrl == bookUrl);
        }

        /// <summary>
        /// 根据书名和作者获取（对应 Kotlin 的 getFirstByNameAuthor）
        /// </summary>
        public async Task<SearchBook> GetAsync(string name, string author)
        {
            // TODO: 需要联表查询 book_sources，这里简化实现
            var sql = "SELECT * FROM search_books WHERE name = @a AND author = @b ORDER BY origin_order LIMIT 1";
            var result = await QueryAsync<SearchBook>(sql, new { a = name, b = author });
            return result.FirstOrDefault();
        }

        /// <summary>
        /// 按书名查询
        /// </summary>
        public async Task<List<SearchBook>> GetByNameAsync(string name)
        {
            var sql = "SELECT * FROM search_books WHERE name = @a";
            var result = await QueryAsync<SearchBook>(sql, new { a = name });
            return result;
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
            var sql = "SELECT * FROM search_books WHERE name = @a AND author LIKE @b ORDER BY origin_order";
            var result = await QueryAsync<SearchBook>(sql, new { a = name, b = $"%{author}%" });
            return result;
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
            var sql = "SELECT * FROM search_books WHERE name = @a AND author LIKE @b ORDER BY origin_order";
            var books = await QueryAsync<SearchBook>(sql, new { a = name, b = $"%{author}%" });

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
            var sql = "SELECT * FROM search_books WHERE name = @a AND author = @b AND cover_url IS NOT NULL AND cover_url != '' ORDER BY origin_order";
            var result = await QueryAsync<SearchBook>(sql, new { a = name, b = author });
            return result;
        }

        /// <summary>
        /// 插入搜索书籍
        /// </summary>
        public async Task InsertAsync(params SearchBook[] searchBooks)
        {
            if (searchBooks == null || searchBooks.Length == 0)
                return;

            await InsertOrReplaceAllAsync(searchBooks);
        }

        /// <summary>
        /// 更新搜索书籍
        /// </summary>
        public async Task UpdateAsync(params SearchBook[] searchBooks)
        {
            if (searchBooks == null || searchBooks.Length == 0)
                return;

            await base.UpdateAllAsync(searchBooks);
        }

        /// <summary>
        /// 删除搜索书籍
        /// </summary>
        public async Task DeleteAsync(params SearchBook[] searchBooks)
        {
            if (searchBooks == null || searchBooks.Length == 0)
                return;

            await base.DeleteAllAsync(searchBooks);
        }

        /// <summary>
        /// 清空指定书籍的搜索结果（对应 Kotlin 的 clear）
        /// </summary>
        public async Task ClearAsync(string name, string author)
        {
            var sql = "DELETE FROM search_books WHERE name = @a AND author = @b";
            await ExecuteAsync(sql, new { a = name, b = author });
        }

        /// <summary>
        /// 清空所有搜索结果
        /// </summary>
        public async Task ClearAsync()
        {
            await ExecuteAsync("DELETE FROM search_books");
        }

        /// <summary>
        /// 清空过期的搜索结果（对应 Kotlin 的 clearExpired）
        /// </summary>
        public async Task ClearExpiredAsync(long time)
        {
            var sql = "DELETE FROM search_books WHERE time < @a";
            await ExecuteAsync(sql, new { a = time });
        }
    }
}
