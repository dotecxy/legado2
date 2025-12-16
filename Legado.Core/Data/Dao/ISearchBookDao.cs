using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 搜索书籍数据访问接口（对应 Kotlin 的 SearchBookDao.kt）
    /// </summary>
    public interface ISearchBookDao
    {
        Task<List<SearchBook>> GetByNameAsync(string name);
        Task<SearchBook> GetAsync(string name, string author);
        Task InsertAsync(params SearchBook[] searchBooks);
        Task UpdateAsync(params SearchBook[] searchBooks);
        Task DeleteAsync(params SearchBook[] searchBooks);
        Task ClearAsync();
        // TODO: 实现更多查询方法
    }
}
