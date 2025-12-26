using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// HTTP TTS 数据访问实现（对应 Kotlin 的 HttpTTSDao.kt）
    /// </summary>
    public class HttpTTSDao : ProxyDao<HttpTTS>, IHttpTTSDao
    {
        public HttpTTSDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<List<HttpTTS>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        public async Task<HttpTTS> GetAsync(long id)
        {
            return await FindAsync(id);
        }

        public async Task InsertAsync(params HttpTTS[] httpTTS)
        {
            await InsertOrReplaceAllAsync(httpTTS);
        }

        public async Task UpdateAsync(params HttpTTS[] httpTTS)
        {
            await base.UpdateAllAsync(httpTTS);
        }

        public async Task DeleteAsync(params HttpTTS[] httpTTS)
        {
            await base.DeleteAllAsync(httpTTS);
        }
    }
}
