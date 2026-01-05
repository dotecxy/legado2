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
        /// <summary>
        /// 根据ID获取分组
        /// </summary>
        Task<BookGroup> GetByIdAsync(long id);

        /// <summary>
        /// 根据名称获取分组
        /// </summary>
        Task<BookGroup> GetByNameAsync(string groupName);

        /// <summary>
        /// 获取所有分组
        /// </summary>
        Task<List<BookGroup>> GetAllAsync();

        /// <summary>
        /// 获取可选择的分组（groupId >= 0）
        /// </summary>
        Task<List<BookGroup>> GetSelectAsync();

        /// <summary>
        /// 获取显示的分组
        /// </summary>
        Task<List<BookGroup>> GetShowAsync();

        /// <summary>
        /// 获取分组ID总和
        /// </summary>
        Task<long> GetIdsSumAsync();

        /// <summary>
        /// 获取最大排序值
        /// </summary>
        Task<int> GetMaxOrderAsync();

        /// <summary>
        /// 获取分组名称列表（根据ID）
        /// </summary>
        Task<List<string>> GetGroupNamesAsync(long id);

        /// <summary>
        /// 启用分组
        /// </summary>
        Task EnableGroupAsync(long groupId);

        /// <summary>
        /// 检查ID是否在规则中
        /// </summary>
        bool IsInRules(long id);

        /// <summary>
        /// 获取未使用的ID
        /// </summary>
        Task<long> GetUnusedIdAsync();

        /// <summary>
        /// 插入分组
        /// </summary>
        Task InsertAsync(params BookGroup[] groups);

        /// <summary>
        /// 更新分组
        /// </summary>
        Task UpdateAsync(params BookGroup[] groups);

        /// <summary>
        /// 删除分组
        /// </summary>
        Task DeleteAsync(params BookGroup[] groups);
    }
}
