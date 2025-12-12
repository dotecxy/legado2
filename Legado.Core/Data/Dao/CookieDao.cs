using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// Cookie 数据访问实现（对应 Kotlin 的 CookieDao.kt）
    /// </summary>
    public class CookieDao : ICookieDao
    {
        private readonly SQLiteAsyncConnection _database;

        public CookieDao(SQLiteAsyncConnection database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// 根据 URL 获取 Cookie
        /// </summary>
        public async Task<CookieEntity> GetAsync(string url)
        {
            return await _database.Table<CookieEntity>()
                .Where(c => c.Url == url)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 获取 OkHttp Cookies（URL 包含 '|'）
        /// </summary>
        public async Task<List<CookieEntity>> GetOkHttpCookiesAsync()
        {
            var allCookies = await _database.Table<CookieEntity>().ToListAsync();
            return allCookies.Where(c => c.Url?.Contains("|") ?? false).ToList();
        }

        /// <summary>
        /// 插入 Cookie
        /// </summary>
        public async Task InsertAsync(params CookieEntity[] cookies)
        {
            if (cookies == null || cookies.Length == 0)
                return;

            foreach (var cookie in cookies)
            {
                await _database.InsertOrReplaceAsync(cookie);
            }
        }

        /// <summary>
        /// 更新 Cookie
        /// </summary>
        public async Task UpdateAsync(params CookieEntity[] cookies)
        {
            if (cookies == null || cookies.Length == 0)
                return;

            foreach (var cookie in cookies)
            {
                await _database.UpdateAsync(cookie);
            }
        }

        /// <summary>
        /// 删除 Cookie
        /// </summary>
        public async Task DeleteAsync(string url)
        {
            await _database.ExecuteAsync(
                "DELETE FROM cookies WHERE url = ?",
                url
            );
        }

        /// <summary>
        /// 删除所有 OkHttp Cookie
        /// </summary>
        public async Task DeleteOkHttpAsync()
        {
            await _database.ExecuteAsync(
                "DELETE FROM cookies WHERE url LIKE '%|%'"
            );
        }
    }
}
