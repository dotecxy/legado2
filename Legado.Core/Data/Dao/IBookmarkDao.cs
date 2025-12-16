using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书签数据访问接口（对应 Kotlin 的 BookmarkDao.kt）
    /// </summary>
    public interface IBookmarkDao
    {
        Task<List<Bookmark>> GetByBookAsync(string bookUrl);
        Task<Bookmark> GetByIndexAsync(string bookUrl, int chapterIndex);
        Task InsertAsync(params Bookmark[] bookmarks);
        Task UpdateAsync(params Bookmark[] bookmarks);
        Task DeleteAsync(params Bookmark[] bookmarks);
        Task<Bookmark> GetAsync(long time);
        // TODO: 实现更多查询方法
    }
}
