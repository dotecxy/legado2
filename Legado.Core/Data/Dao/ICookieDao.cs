using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// Cookie 数据访问接口（对应 Kotlin 的 CookieDao.kt）
    /// </summary>
    public interface ICookieDao
    {
        /// <summary>
        /// 根据URL获取Cookie
        /// </summary>
        Task<Cookie> GetAsync(string url);

        /// <summary>
        /// 获取所有 OkHttp Cookie（URL包含'|'）
        /// </summary>
        Task<List<Cookie>> GetOkHttpCookiesAsync();

        /// <summary>
        /// 插入Cookie
        /// </summary>
        Task InsertAsync(params Cookie[] cookies);

        /// <summary>
        /// 更新Cookie
        /// </summary>
        Task UpdateAsync(params Cookie[] cookies);

        /// <summary>
        /// 根据URL删除Cookie
        /// </summary>
        Task DeleteAsync(string url);

        /// <summary>
        /// 删除所有 OkHttp Cookie
        /// </summary>
        Task DeleteOkHttpAsync();
    }
}
