using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// Cookie 数据访问实现（对应 Kotlin 的 CookieDao.kt）
    /// </summary>
    public class CookieDao : BaseDao<CookieEntity>, ICookieDao
    {
        public CookieDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// 根据 URL 获取 Cookie
        /// </summary>
        public async Task<CookieEntity> GetAsync(string url)
        {
            return await GetFirstOrDefaultAsync(c => c.Url == url);
        }

        /// <summary>
        /// 获取 OkHttp Cookies（URL 包含 '|'）
        /// </summary>
        public async Task<List<CookieEntity>> GetOkHttpCookiesAsync()
        {
            var sql = "SELECT * FROM cookies WHERE url LIKE '%|%'";
            var result = await QueryAsync<CookieEntity>(sql);
            return result;
        }

        /// <summary>
        /// 插入 Cookie
        /// </summary>
        public async Task InsertAsync(params CookieEntity[] cookies)
        {
            if (cookies == null || cookies.Length == 0)
                return;

            await InsertOrReplaceAllAsync(cookies);
        }

        /// <summary>
        /// 更新 Cookie
        /// </summary>
        public async Task UpdateAsync(params CookieEntity[] cookies)
        {
            if (cookies == null || cookies.Length == 0)
                return;

            await base.UpdateAllAsync(cookies);
        }

        /// <summary>
        /// 删除 Cookie
        /// </summary>
        public async Task DeleteAsync(string url)
        {
            var sql = "DELETE FROM cookies WHERE url = ?";
            await ExecuteAsync(sql, url);
        }

        /// <summary>
        /// 删除所有 OkHttp Cookie
        /// </summary>
        public async Task DeleteOkHttpAsync()
        {
            var sql = "DELETE FROM cookies WHERE url LIKE '%|%'";
            await ExecuteAsync(sql);
        }
    }
}
