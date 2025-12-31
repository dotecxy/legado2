using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 键盘辅助数据访问实现（对应 Kotlin 的 KeyboardAssistsDao.kt）
    /// </summary>
    public class KeyboardAssistsDao : BaseDao<KeyboardAssist>, IKeyboardAssistDao
    {
        public KeyboardAssistsDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<List<KeyboardAssist>> GetAllAsync()
        {
            var sql = "SELECT * FROM keyboardAssists ORDER BY serialNo";
            var result = await QueryAsync<KeyboardAssist>(sql);
            return result;
        }

        public async Task<KeyboardAssist> GetAsync(string key)
        {
            return await GetFirstOrDefaultAsync(k => k.Key == key);
        }

        public async Task<List<KeyboardAssist>> GetByTypeAsync(string type)
        {
            // 将 string 类型转换为 int
            if (int.TryParse(type, out int typeValue))
            {
                var sql = "SELECT * FROM keyboardAssists WHERE type = ? ORDER BY serialNo";
                var result = await QueryAsync<KeyboardAssist>(sql, typeValue);
                return result;
            }
            return new List<KeyboardAssist>();
        }

        public async Task InsertAsync(params KeyboardAssist[] assists)
        {
            await InsertOrReplaceAllAsync(assists);
        }

        public async Task UpdateAsync(params KeyboardAssist[] assists)
        {
            await base.UpdateAllAsync(assists);
        }

        public async Task DeleteAsync(params KeyboardAssist[] assists)
        {
            await base.DeleteAllAsync(assists);
        }
    }
}
