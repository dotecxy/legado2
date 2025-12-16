using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书籍分组数据访问接口（对应 Kotlin 的 BookGroupDao.kt）
    /// </summary>
    public interface IBookGroupDao
    {
        Task<List<BookGroup>> GetAllAsync();
        Task<BookGroup> GetByIdAsync(long groupId);
        Task InsertAsync(params BookGroup[] groups);
        Task UpdateAsync(params BookGroup[] groups);
        Task DeleteAsync(params BookGroup[] groups);
        // TODO: 实现更多查询方法
    }
}
