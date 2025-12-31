using Legado.Core.Data.Entities;
using Legado.FreeSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{

    [SingletonDependency]
    [ExposeServices(typeof(IBookDao), IncludeSelf = true)]
    [ConnectionStringName("Book")]
    /// <summary>
    /// 书籍数据访问实现（对应 Kotlin 的 BookDao.kt）
    /// 使用 SQLite-net-pcl 进行数据库访问
    /// </summary>
    public class BookDao : BaseDao<Book>, IBookDao
    {
        public BookDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        // ================= 按分组查询 =================

        /// <summary>
        /// 按分组 ID 获取书籍
        /// </summary>
        public async Task<List<Book>> GetByGroupAsync(long groupId)
        {
            // TODO: 实现不同分组逻辑
            // BookGroup.IdRoot, IdAll, IdLocal, IdAudio 等
            return await GetAllAsync();
        }

        /// <summary>
        /// 获取所有书籍
        /// </summary>
        public override async Task<List<Book>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        /// <summary>
        /// 获取本地书籍
        /// </summary>
        public async Task<List<Book>> GetLocalAsync()
        {
            // TODO: 实现 BookType 的位运算逻辑
            var books = await GetAllAsync();
            return books; // 待实现
        }

        /// <summary>
        /// 获取音频书籍
        /// </summary>
        public async Task<List<Book>> GetAudioAsync()
        {
            // TODO: 实现 BookType 的位运算逻辑
            var books = await GetAllAsync();
            return books; // 待实现
        }

        /// <summary>
        /// 获取网络书籍
        /// </summary>
        public async Task<List<Book>> GetWebBooksAsync()
        {
            // TODO: 实现 BookType 的位运算逻辑
            var books = await GetAllAsync();
            return books; // 待实现
        }

        /// <summary>
        /// 按用户分组获取书籍
        /// </summary>
        public async Task<List<Book>> GetByUserGroupAsync(long group)
        {
            var books = await GetAllAsync();
            return books.Where(b => (b.Group & group) > 0).ToList();
        }

        /// <summary>
        /// 搜索书籍（按书名或作者）
        /// </summary>
        public async Task<List<Book>> SearchAsync(string key)
        {
            var books = await GetAllAsync();
            return books.Where(b =>
                (b.Name?.Contains(key) ?? false) ||
                (b.Author?.Contains(key) ?? false)
            ).ToList();
        }

        // ================= 单个查询 =================

        /// <summary>
        /// 获取书籍总数
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            return await base.CountAsync();
        }

        /// <summary>
        /// 根据 URL 获取书籍
        /// </summary>
        public async Task<Book> GetByUrlAsync(string bookUrl)
        {
            return await GetFirstOrDefaultAsync(b => b.BookUrl == bookUrl);
        }

        /// <summary>
        /// 根据 URL 获取书籍（接口要求）
        /// </summary>
        public async Task<Book> GetBookAsync(string bookUrl)
        {
            return await GetByUrlAsync(bookUrl);
        }

        /// <summary>
        /// 根据书名和作者获取书籍
        /// </summary>
        public async Task<Book> GetAsync(string name, string author)
        {
            return await GetFirstOrDefaultAsync(b => b.Name == name && b.Author == author);
        }

        /// <summary>
        /// 根据书名和作者获取书籍（接口要求）
        /// </summary>
        public async Task<Book> GetBookAsync(string name, string author)
        {
            return await GetAsync(name, author);
        }

        /// <summary>
        /// 根据文件名获取书籍
        /// </summary>
        public async Task<Book> GetByFileNameAsync(string fileName)
        {
            return await GetFirstOrDefaultAsync(b => b.OriginName == fileName);
        }

        /// <summary>
        /// 根据文件名获取书籍（接口要求）
        /// </summary>
        public async Task<Book> GetBookByFileNameAsync(string fileName)
        {
            return await GetByFileNameAsync(fileName);
        }

        /// <summary>
        /// 根据书名和来源获取书籍
        /// </summary>
        public async Task<Book> GetByOriginAsync(string name, string origin)
        {
            return await GetFirstOrDefaultAsync(b => b.Name == name && b.Origin == origin);
        }

        /// <summary>
        /// 按书名查询多本
        /// </summary>
        public async Task<List<Book>> FindByNameAsync(params string[] names)
        {
            if (names == null || names.Length == 0)
                return new List<Book>();

            var books = await GetAllAsync();
            return books.Where(b => names.Contains(b.Name)).ToList();
        }

        /// <summary>
        /// 获取所有使用的书源（联表查询）
        /// </summary>
        public async Task<List<BookSource>> GetAllUseBookSourceAsync()
        {
            var sql = @"
                SELECT DISTINCT bs.* 
                FROM books b
                INNER JOIN book_sources bs ON b.origin = bs.bookSourceUrl
            ";
            var result = await QueryAsync<BookSource>(sql);
            return result;
        }

        // ================= 统计查询 =================

        /// <summary>
        /// 检查书籍是否存在（按 URL）
        /// </summary>
        public async Task<bool> HasAsync(string bookUrl)
        {
            return await ExistsAsync(b => b.BookUrl == bookUrl);
        }

        /// <summary>
        /// 检查书籍是否存在（按书名和作者）
        /// </summary>
        public async Task<bool> HasAsync(string name, string author)
        {
            return await ExistsAsync(b => b.Name == name && b.Author == author);
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public async Task<bool> HasFileAsync(string fileName)
        {
            // 需要 LIKE 查询,不能使用 ExistsAsync,保持原有逻辑
            var sql = "SELECT COUNT(1) FROM books WHERE originName = @a OR origin LIKE @b";
            var result = await ExecuteScalarAsync<int>(sql, new { a = fileName, b = $"%{fileName}%" });
            return result > 0;
        }

        /// <summary>
        /// 获取最小排序值
        /// </summary>
        public async Task<int> GetMinOrderAsync()
        {
            var books = await GetAllAsync();
            return books.Any() ? books.Min(b => b.Order) : 0;
        }

        /// <summary>
        /// 获取最大排序值
        /// </summary>
        public async Task<int> GetMaxOrderAsync()
        {
            var books = await GetAllAsync();
            return books.Any() ? books.Max(b => b.Order) : 0;
        }

        /// <summary>
        /// 获取最后阅读的书籍
        /// </summary>
        public async Task<Book> GetLastReadBookAsync()
        {
            var sql = "SELECT * FROM books ORDER BY durChapterTime DESC LIMIT 1";
            var result = await QueryAsync<Book>(sql);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// 获取所有书籍 URL
        /// </summary>
        public async Task<List<string>> GetAllBookUrlsAsync()
        {
            var books = await GetAllAsync();
            return books.Select(b => b.BookUrl).ToList();
        }

        // ================= 增删改操作 =================

        /// <summary>
        /// 插入书籍
        /// </summary>
        public new async Task InsertAsync(params Book[] books)
        {
            if (books == null || books.Length == 0)
                return;

            await InsertOrReplaceAllAsync(books);
        }

        /// <summary>
        /// 更新书籍
        /// </summary>
        public new async Task UpdateAsync(params Book[] books)
        {
            if (books == null || books.Length == 0)
                return;

            await base.UpdateAllAsync(books);
        }

        /// <summary>
        /// 删除书籍
        /// </summary>
        public new async Task DeleteAsync(params Book[] books)
        {
            if (books == null || books.Length == 0)
                return;

            await base.DeleteAllAsync(books);
        }

        /// <summary>
        /// 替换书籍（事务操作）
        /// </summary>
        public async Task ReplaceAsync(Book oldBook, Book newBook)
        {
            await RunInTransactionAsync((fsql) =>
            {
                var sql = "DELETE FROM books WHERE bookUrl = @a";
                fsql.Ado.ExecuteNonQuery(sql, new { a = oldBook.BookUrl });
                fsql.Insert(newBook);
            });
        }

        /// <summary>
        /// 更新阅读进度
        /// </summary>
        public async Task UpdateProgressAsync(string bookUrl, int pos)
        {
            var sql = "UPDATE books SET durChapterPos = @a WHERE bookUrl = @b";
            await ExecuteAsync(sql, new { a = pos, b = bookUrl });
        }

        /// <summary>
        /// 更新分组
        /// </summary>
        public async Task UpdateGroupAsync(long oldGroupId, long newGroupId)
        {
            var sql = "UPDATE books SET `group` = @a WHERE `group` = @b";
            await ExecuteAsync(sql, new { a = newGroupId, b = oldGroupId });
        }

        /// <summary>
        /// 移除分组
        /// </summary>
        public async Task RemoveGroupAsync(long group)
        {
            var sql = "UPDATE books SET `group` = `group` - @a WHERE (`group` & @b) > 0";
            await ExecuteAsync(sql, new { a = group, b = group });
        }

        /// <summary>
        /// 删除非书架书籍
        /// </summary>
        public async Task DeleteNotShelfBookAsync()
        {
            // TODO: 实现 BookType.notShelf 的位运算逻辑
            var sql = "DELETE FROM books WHERE type & ? > @a";
            await ExecuteAsync(sql, new { a= 0 });
        }

        // ================= Observable 数据流 =================

        /// <summary>
        /// 获取所有书籍的 Observable 流
        /// </summary>
        public IObservable<List<Book>> ObserveAll()
        {
            return Observable.Create<List<Book>>(async observer =>
            {
                var books = await GetAllAsync();
                observer.OnNext(books);
                observer.OnCompleted();
            });
        }

        /// <summary>
        /// 按分组观察书籍
        /// </summary>
        public IObservable<List<Book>> ObserveByGroup(long groupId)
        {
            return Observable.Create<List<Book>>(async observer =>
            {
                var books = await GetByGroupAsync(groupId);
                observer.OnNext(books);
                observer.OnCompleted();
            });
        }

        /// <summary>
        /// 搜索书籍的 Observable 流
        /// </summary>
        public IObservable<List<Book>> ObserveSearch(string key)
        {
            return Observable.Create<List<Book>>(async observer =>
            {
                var books = await SearchAsync(key);
                observer.OnNext(books);
                observer.OnCompleted();
            });
        }

    }
}
