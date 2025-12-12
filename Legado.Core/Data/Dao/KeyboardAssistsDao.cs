using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 键盘辅助数据访问实现（对应 Kotlin 的 KeyboardAssistsDao.kt）
    /// </summary>
    public class KeyboardAssistsDao : IKeyboardAssistDao
    {
        private readonly SQLiteAsyncConnection _database;

        public KeyboardAssistsDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<List<KeyboardAssist>> GetAllAsync()
        {
            return await _database.Table<KeyboardAssist>()
                .OrderBy(k => k.SerialNo)
                .ToListAsync();
        }

        public async Task<KeyboardAssist> GetAsync(string key)
        {
            return await _database.Table<KeyboardAssist>()
                .Where(k => k.Key == key)
                .FirstOrDefaultAsync();
        }

        public async Task<List<KeyboardAssist>> GetByTypeAsync(string type)
        {
            return await _database.Table<KeyboardAssist>()
                .Where(k => k.Type == type)
                .OrderBy(k => k.SerialNo)
                .ToListAsync();
        }

        public async Task InsertAsync(params KeyboardAssist[] assists)
        {
            foreach (var assist in assists)
                await _database.InsertOrReplaceAsync(assist);
        }

        public async Task UpdateAsync(params KeyboardAssist[] assists)
        {
            foreach (var assist in assists)
                await _database.UpdateAsync(assist);
        }

        public async Task DeleteAsync(params KeyboardAssist[] assists)
        {
            foreach (var assist in assists)
                await _database.DeleteAsync(assist);
        }
    }
}
