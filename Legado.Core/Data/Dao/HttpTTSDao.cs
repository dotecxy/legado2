using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// HTTP TTS 数据访问实现（对应 Kotlin 的 HttpTTSDao.kt）
    /// </summary>
    public class HttpTTSDao : IHttpTTSDao
    {
        private readonly SQLiteAsyncConnection _database;

        public HttpTTSDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<HttpTTS>> GetAllAsync()
        {
            return await _database.Table<HttpTTS>().ToListAsync();
        }

        public async Task<HttpTTS> GetAsync(long id)
        {
            return await _database.Table<HttpTTS>()
                .Where(h => h.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params HttpTTS[] httpTTS)
        {
            foreach (var tts in httpTTS)
                await _database.InsertOrReplaceAsync(tts);
        }

        public async Task UpdateAsync(params HttpTTS[] httpTTS)
        {
            foreach (var tts in httpTTS)
                await _database.UpdateAsync(tts);
        }

        public async Task DeleteAsync(params HttpTTS[] httpTTS)
        {
            foreach (var tts in httpTTS)
                await _database.DeleteAsync(tts);
        }
    }
}
