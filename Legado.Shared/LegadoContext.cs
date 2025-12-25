using Legado.Core;
using Legado.Core.Data.Dao;
using Legado.Core.Data.Entities;
using Legado.Core.Models.WebBooks;
using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Shared
{
    [SingletonDependency]
    public class LegadoContext
    {
        private readonly IEventProvider _ep;
        private readonly IBookDao _bookDao;
        public List<BookSource> BookSources { get; private set; } = new List<BookSource>();
        public WebBook WebBook { get; private set; } = new WebBook();
        public int BookIndex { get; set; } = 6;
        public Book CurrentBook { get; set; }
        public BookChapter CurrentChapter { get; set; }
        public BookSource CurrentBookSource
        {
            get
            {
                return BookSources[BookIndex];
            }
        }

        public LegadoContext(IEventProvider eventProvider, IBookDao bookDao)
        {
            _ep = eventProvider;
            _bookDao = bookDao;
        }

        public void Load()
        {
            try
            {

                var json = _ep.GetResourceManager().GetString("bs");
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

        /// <summary>
        /// 获取书架中的所有书籍
        /// </summary>
        public async Task<List<Book>> GetBookshelfAsync()
        {
            return await _bookDao.GetAllAsync();
        }

        /// <summary>
        /// 将书籍添加到书架
        /// </summary>
        public async Task AddToBookshelfAsync(Book book)
        {
            await _bookDao.InsertAsync(book);
        }

        /// <summary>
        /// 从书架移除书籍
        /// </summary>
        public async Task RemoveFromBookshelfAsync(Book book)
        {
            await _bookDao.DeleteAsync(book);
        }

        /// <summary>
        /// 检查书籍是否已在书架中
        /// </summary>
        public async Task<bool> IsBookInBookshelfAsync(string bookUrl)
        {
            return await _bookDao.HasAsync(bookUrl);
        }
    }
}
