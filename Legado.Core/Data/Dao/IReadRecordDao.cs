using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 阅读记录数据访问接口（对应 Kotlin 的 ReadRecordDao.kt）
    /// </summary>
    public interface IReadRecordDao
    {
        Task<List<ReadRecord>> GetAllAsync();
        Task<ReadRecord> GetAsync(string bookName);
        Task InsertAsync(params ReadRecord[] records);
        Task UpdateAsync(params ReadRecord[] records);
        Task DeleteAsync(params ReadRecord[] records);
        Task ClearAsync();
        // TODO: 实现更多查询方法
    }
}
