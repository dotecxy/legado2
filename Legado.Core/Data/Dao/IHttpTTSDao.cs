using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// HTTP TTS 数据访问接口（对应 Kotlin 的 HttpTTSDao.kt）
    /// </summary>
    public interface IHttpTTSDao
    {
        Task<List<HttpTTS>> GetAllAsync();
        Task<HttpTTS> GetAsync(long id);
        Task InsertAsync(params HttpTTS[] httpTTSs);
        Task UpdateAsync(params HttpTTS[] httpTTSs);
        Task DeleteAsync(params HttpTTS[] httpTTSs);
        // TODO: 实现更多查询方法
    }
}
