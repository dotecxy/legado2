using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书籍章节数据访问接口（对应 Kotlin 的 BookChapterDao.kt）
    /// </summary>
    public interface IBookChapterDao
    {
        Task<List<BookChapter>> GetChaptersAsync(string bookUrl);
        Task<BookChapter> GetChapterAsync(string bookUrl, int index);
        Task InsertAsync(params BookChapter[] chapters);
        Task UpdateAsync(params BookChapter[] chapters);
        Task DeleteAsync(string bookUrl);
        Task<int> GetCountAsync(string bookUrl);
        // TODO: 实现更多查询方法
    }
}
