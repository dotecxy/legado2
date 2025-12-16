using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 键盘辅助数据访问接口（对应 Kotlin 的 KeyboardAssistsDao.kt）
    /// </summary>
    public interface IKeyboardAssistDao
    {
        Task<List<KeyboardAssist>> GetAllAsync();
        Task<KeyboardAssist> GetAsync(string key);
        Task<List<KeyboardAssist>> GetByTypeAsync(string type);
        Task InsertAsync(params KeyboardAssist[] assists);
        Task UpdateAsync(params KeyboardAssist[] assists);
        Task DeleteAsync(params KeyboardAssist[] assists);
        // TODO: 实现更多查询方法
    }
}
