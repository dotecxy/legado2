using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// RSS 阅读记录数据访问接口（对应 Kotlin 的 RssReadRecordDao.kt）
    /// </summary>
    public interface IRssReadRecordDao
    {
        Task<List<RssReadRecord>> GetAllAsync();
        Task<RssReadRecord> GetAsync(string record);
        Task InsertAsync(params RssReadRecord[] records);
        Task DeleteAsync(params RssReadRecord[] records);
        Task DeleteAsync(string record);
        // TODO: 实现更多查询方法
    }
}
