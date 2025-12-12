using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书籍数据访问实现（对应 Kotlin 的 BookDao.kt）
    /// </summary>
    public class BookDao : IBookDao
    {
        private readonly SQLiteAsyncConnection _database;

        public BookDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
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
        public async Task<List<Book>> GetAllAsync()
        {
            return await _database.Table<Book>()
                .OrderByDescending(b => b.DurChapterTime)
                .ToListAsync();
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
        /// 根据 URL 获取书籍
        /// </summary>
        public async Task<Book> GetByUrlAsync(string bookUrl)
        {
            return await _database.Table<Book>()
                .Where(b => b.BookUrl == bookUrl)
                .FirstOrDefaultAsync();
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
            return await _database.Table<Book>()
                .Where(b => b.Name == name && b.Author == author)
                .FirstOrDefaultAsync();
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
            return await _database.Table<Book>()
                .Where(b => b.OriginName == fileName)
                .FirstOrDefaultAsync();
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
            return await _database.Table<Book>()
                .Where(b => b.Name == name && b.Origin == origin)
                .FirstOrDefaultAsync();
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
            // TODO: 需要联表查询 books 和 book_sources
            // SELECT DISTINCT bs.* FROM books, book_sources bs 
            // WHERE origin == bookSourceUrl
            
            var books = await GetAllAsync();
            var sourceUrls = books.Select(b => b.Origin).Distinct().ToList();
            
            var sources = new List<BookSource>();
            foreach (var url in sourceUrls)
            {
                var source = await _database.Table<BookSource>()
                    .Where(s => s.BookSourceUrl == url)
                    .FirstOrDefaultAsync();
                if (source != null)
                    sources.Add(source);
            }
            return sources;
        }

        // ================= 统计查询 =================

        /// <summary>
        /// 获取书籍总数
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            return await _database.Table<Book>().CountAsync();
        }

        /// <summary>
        /// 检查书籍是否存在（按 URL）
        /// </summary>
        public async Task<bool> HasAsync(string bookUrl)
        {
            var count = await _database.Table<Book>()
                .Where(b => b.BookUrl == bookUrl)
                .CountAsync();
            return count > 0;
        }

        /// <summary>
        /// 检查书籍是否存在（按书名和作者）
        /// </summary>
        public async Task<bool> HasAsync(string name, string author)
        {
            var count = await _database.Table<Book>()
                .Where(b => b.Name == name && b.Author == author)
                .CountAsync();
            return count > 0;
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public async Task<bool> HasFileAsync(string fileName)
        {
            var count = await _database.Table<Book>()
                .Where(b => b.OriginName == fileName || b.Origin.Contains(fileName))
                .CountAsync();
            return count > 0;
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
            return await _database.Table<Book>()
                .OrderByDescending(b => b.DurChapterTime)
                .FirstOrDefaultAsync();
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
        public async Task InsertAsync(params Book[] books)
        {
            if (books == null || books.Length == 0)
                return;

            foreach (var book in books)
            {
                await _database.InsertOrReplaceAsync(book);
            }
        }

        /// <summary>
        /// 更新书籍
        /// </summary>
        public async Task UpdateAsync(params Book[] books)
        {
            if (books == null || books.Length == 0)
                return;

            foreach (var book in books)
            {
                await _database.UpdateAsync(book);
            }
        }

        /// <summary>
        /// 删除书籍
        /// </summary>
        public async Task DeleteAsync(params Book[] books)
        {
            if (books == null || books.Length == 0)
                return;

            foreach (var book in books)
            {
                await _database.DeleteAsync(book);
            }
        }

        /// <summary>
        /// 替换书籍（事务操作）
        /// </summary>
        public async Task ReplaceAsync(Book oldBook, Book newBook)
        {
            await _database.RunInTransactionAsync((connection) =>
            {
                connection.Delete(oldBook);
                connection.InsertOrReplace(newBook);
            });
        }

        /// <summary>
        /// 更新阅读进度
        /// </summary>
        public async Task UpdateProgressAsync(string bookUrl, int pos)
        {
            await _database.ExecuteAsync(
                "UPDATE books SET durChapterPos = ? WHERE bookUrl = ?",
                pos,
                bookUrl
            );
        }

        /// <summary>
        /// 更新分组
        /// </summary>
        public async Task UpdateGroupAsync(long oldGroupId, long newGroupId)
        {
            await _database.ExecuteAsync(
                "UPDATE books SET `group` = ? WHERE `group` = ?",
                newGroupId,
                oldGroupId
            );
        }

        /// <summary>
        /// 移除分组
        /// </summary>
        public async Task RemoveGroupAsync(long group)
        {
            await _database.ExecuteAsync(
                "UPDATE books SET `group` = `group` - ? WHERE (`group` & ?) > 0",
                group,
                group
            );
        }

        /// <summary>
        /// 删除非书架书籍
        /// </summary>
        public async Task DeleteNotShelfBookAsync()
        {
            // TODO: 实现 BookType.notShelf 的位运算逻辑
            await _database.ExecuteAsync("DELETE FROM books WHERE type & ? > 0", 0);
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
