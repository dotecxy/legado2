using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 服务器数据访问实现（对应 Kotlin 的 ServerDao.kt）
    /// </summary>
    public class ServerDao : IServerDao
    {
        private readonly SQLiteAsyncConnection _database;

        public ServerDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<Server>> GetAllAsync()
        {
            return await _database.Table<Server>().ToListAsync();
        }

        public async Task<Server> GetAsync(long id)
        {
            return await _database.Table<Server>()
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(params Server[] servers)
        {
            foreach (var server in servers)
                await _database.InsertOrReplaceAsync(server);
        }

        public async Task UpdateAsync(params Server[] servers)
        {
            foreach (var server in servers)
                await _database.UpdateAsync(server);
        }

        public async Task DeleteAsync(params Server[] servers)
        {
            foreach (var server in servers)
                await _database.DeleteAsync(server);
        }
    }
}
