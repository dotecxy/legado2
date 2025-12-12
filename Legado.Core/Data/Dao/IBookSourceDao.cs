using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书源数据访问接口（对应 Kotlin 的 BookSourceDao.kt）
    /// </summary>
    public interface IBookSourceDao
    {
        // ================= 查询方法 =================

        /// <summary>
        /// 获取所有书源（Part）
        /// </summary>
        Task<List<BookSourcePart>> GetAllPartAsync();

        /// <summary>
        /// 搜索书源
        /// </summary>
        Task<List<BookSourcePart>> SearchAsync(string searchKey);

        /// <summary>
        /// 搜索已启用的书源
        /// </summary>
        Task<List<BookSourcePart>> SearchEnabledAsync(string searchKey);

        /// <summary>
        /// 按分组搜索
        /// </summary>
        Task<List<BookSourcePart>> GroupSearchAsync(string searchKey);

        /// <summary>
        /// 获取已启用的书源
        /// </summary>
        Task<List<BookSourcePart>> GetEnabledAsync();

        /// <summary>
        /// 获取已禁用的书源
        /// </summary>
        Task<List<BookSourcePart>> GetDisabledAsync();

        /// <summary>
        /// 获取已启用发现的书源
        /// </summary>
        Task<List<BookSourcePart>> GetExploreAsync();

        /// <summary>
        /// 获取有登录URL的书源
        /// </summary>
        Task<List<BookSourcePart>> GetLoginAsync();

        /// <summary>
        /// 获取未分组的书源
        /// </summary>
        Task<List<BookSourcePart>> GetNoGroupAsync();

        /// <summary>
        /// 根据分组获取书源
        /// </summary>
        Task<List<BookSource>> GetByGroupAsync(string group);

        /// <summary>
        /// 根据分组获取已启用的书源
        /// </summary>
        Task<List<BookSource>> GetEnabledByGroupAsync(string group);

        /// <summary>
        /// 根据类型获取已启用的书源
        /// </summary>
        Task<List<BookSource>> GetEnabledByTypeAsync(int type);

        /// <summary>
        /// 根据URL获取书源
        /// </summary>
        Task<BookSource> GetBookSourceAsync(string key);

        /// <summary>
        /// 根据URL获取书源（Part）
        /// </summary>
        Task<BookSourcePart> GetBookSourcePartAsync(string key);

        /// <summary>
        /// 获取所有书源
        /// </summary>
        Task<List<BookSource>> GetAllAsync();

        /// <summary>
        /// 获取所有已启用的书源
        /// </summary>
        Task<List<BookSource>> GetAllEnabledAsync();

        /// <summary>
        /// 获取所有分组（未处理）
        /// </summary>
        Task<List<string>> GetAllGroupsUnProcessedAsync();

        /// <summary>
        /// 获取所有分组（已处理）
        /// </summary>
        Task<List<string>> GetAllGroupsAsync();

        // ================= 统计方法 =================

        /// <summary>
        /// 获取书源总数
        /// </summary>
        Task<int> GetCountAsync();

        /// <summary>
        /// 检查是否存在指定书源
        /// </summary>
        Task<bool> HasAsync(string key);

        /// <summary>
        /// 获取最小排序值
        /// </summary>
        Task<int> GetMinOrderAsync();

        /// <summary>
        /// 获取最大排序值
        /// </summary>
        Task<int> GetMaxOrderAsync();

        // ================= 增删改方法 =================

        /// <summary>
        /// 插入书源
        /// </summary>
        Task InsertAsync(params BookSource[] bookSources);

        /// <summary>
        /// 更新书源
        /// </summary>
        Task UpdateAsync(params BookSource[] bookSources);

        /// <summary>
        /// 删除书源
        /// </summary>
        Task DeleteAsync(params BookSource[] bookSources);

        /// <summary>
        /// 根据URL删除书源
        /// </summary>
        Task DeleteAsync(string key);

        /// <summary>
        /// 批量删除书源
        /// </summary>
        Task DeleteAsync(List<BookSourcePart> bookSources);

        /// <summary>
        /// 启用/禁用书源
        /// </summary>
        Task EnableAsync(string bookSourceUrl, bool enable);

        /// <summary>
        /// 批量启用/禁用书源
        /// </summary>
        Task EnableAsync(bool enable, List<BookSourcePart> bookSources);

        /// <summary>
        /// 启用/禁用发现功能
        /// </summary>
        Task EnableExploreAsync(string bookSourceUrl, bool enable);

        /// <summary>
        /// 批量启用/禁用发现功能
        /// </summary>
        Task EnableExploreAsync(bool enable, List<BookSourcePart> bookSources);

        /// <summary>
        /// 更新排序
        /// </summary>
        Task UpdateOrderAsync(string bookSourceUrl, int customOrder);

        /// <summary>
        /// 批量更新排序
        /// </summary>
        Task UpdateOrderAsync(List<BookSourcePart> bookSources);

        /// <summary>
        /// 更新分组
        /// </summary>
        Task UpdateGroupAsync(string bookSourceUrl, string bookSourceGroup);

        /// <summary>
        /// 批量更新分组
        /// </summary>
        Task UpdateGroupAsync(List<BookSourcePart> bookSources);

        // TODO: 实现 Flow 相关的实时数据流方法
        // TODO: 实现复杂的 SQL 查询方法
    }
}
