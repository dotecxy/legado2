using Legado.Core;
using Legado.Core.Data;
using Legado.Core.Data.Dao;
using Legado.Core.Data.Entities;
using Legado.Core.Models.WebBooks;
using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Legado.Shared
{
    [SingletonDependency]
    public class LegadoContext
    {
        private readonly IEventProvider _eventProvider;
        private readonly AppDatabase _appDb;
        public List<BookSource> BookSources { get; private set; } = new List<BookSource>();
        public WebBook WebBook { get; private set; } = new WebBook();
        public int BookIndex { get; set; } = 6;
        public Book CurrentBook { get; set; }

        public List<BookChapter> CurrentChapters { get; private set; }

        public BookChapter CurrentChapter { get; set; }
        public BookSource CurrentBookSource
        {
            get
            {
                return BookSources[BookIndex];
            }
        }

        public LegadoContext(IEventProvider eventProvider, AppDatabase appDatabase)
        {
            _eventProvider = eventProvider;
            _appDb = appDatabase;
        }

        public void Load()
        {
            try
            {

                var json = _eventProvider.GetResourceManager().GetString("bs");
                var list = JsonConvert.DeserializeObject<List<BookSource>>(json);
                if (list?.Count > 0)
                {
                    BookSources.AddRange(list);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SaveToShelf()
        {
            if (CurrentBookSource != null)
            {
                await _appDb.BookSourceDao.InsertOrReplaceAsync(CurrentBookSource);
            }


            if (CurrentBook != null && !await IsBookInBookshelfAsync(CurrentBook.BookUrl))
            {
                CurrentBook.Origin = CurrentBookSource?.BookSourceUrl ?? "unknow";
                CurrentBook.SetCurrentChapter(this.CurrentChapter, CurrentChapters?.LastOrDefault(), BookIndex, 0);
                await _appDb.BookDao.InsertOrReplaceAsync(CurrentBook);
            }
        }

        public async Task<List<Book>> SearchAsync(string key, int page = 1, Func<string, string, bool> filter = null,
            Func<int, bool> shouldBreak = null)
        {
            CurrentBookSource.ThrowIfNull();

            var searchBooks = await WebBook.SearchBookAwait(CurrentBookSource, key, page, filter, shouldBreak);
            if (searchBooks?.Count > 0)
            {
                return searchBooks.Select(s => s.ToBook()).ToList();
            }
            return new List<Book>();
        }

        public async Task<Book> UpdateBookInfoAsync(bool canRename = true)
        {
            CurrentBookSource.ThrowIfNull();
            CurrentBook.ThrowIfNull();
            return await WebBook.GetBookInfoAwait(CurrentBookSource, CurrentBook, canRename);
        }

        public async Task<List<BookChapter>> GetBookChapterListAsync(bool runPreUpdateJs = false,
            CancellationToken cancellationToken = default)
        {
            CurrentBookSource.ThrowIfNull();
            CurrentBook.ThrowIfNull();

            if (CurrentChapters?.Count > 0)
            {
                return CurrentChapters;
            }

            var result = await _appDb.BookChapterDao.GetAllAsync();

            if (result != null)
            {
                var result2 = await WebBook.GetChapterListAwait(CurrentBookSource, CurrentBook, runPreUpdateJs, cancellationToken);

                if (result.SequenceEqual(result2))
                {
                    return result;
                }

                if (result2.Count > 0)
                {
                    await _appDb.BookChapterDao.InsertOrReplaceAllAsync(result2);
                    result = result2;
                }
            }

            return CurrentChapters = (result ?? new List<BookChapter>());
        }

        public async Task<string> GetBookContent()
        {
            CurrentBookSource.ThrowIfNull();
            CurrentBook.ThrowIfNull();
            CurrentChapter.ThrowIfNull();

            return await WebBook.GetContentAwait(CurrentBookSource, CurrentBook, CurrentChapter);
        }

        public async Task SaveAllChapterAsync(List<BookChapter> list)
        {
            await _appDb.BookChapterDao.InsertAllAsync(list);
        }

        public async Task<List<Book>> GetBookshelfAsync()
        {
            return await _appDb.BookDao.GetAllAsync();
        }

        /// <summary>
        /// 将书籍添加到书架
        /// </summary>
        public async Task AddToBookshelfAsync(Book book)
        {
            await _appDb.BookDao.InsertAsync(book);
        }

        /// <summary>
        /// 从书架移除书籍
        /// </summary>
        public async Task RemoveFromBookshelfAsync(Book book)
        {
            await _appDb.BookDao.DeleteAsync(book);
        }

        /// <summary>
        /// 检查书籍是否已在书架中
        /// </summary>
        public async Task<bool> IsBookInBookshelfAsync(string bookUrl)
        {
            return await _appDb.BookDao.HasAsync(bookUrl);
        }
    }
}
