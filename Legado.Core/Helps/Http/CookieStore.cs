using Legado.Core.Data;
using Legado.Core.Data.Dao;
using Legado.Core.Data.Entities;
using Legado.Core.Helps.Http.Api;
using Legado.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Legado.Core.Helps.Http
{
    [SingletonDependency]
    [ExposeServices(typeof(ICookieManager), IncludeSelf = true)]
    /// <summary>
    /// Cookie 存储管理（对应 Kotlin 的 CookieStore.kt）
    /// </summary>
    public class CookieStore : ICookieManager
    {

        private readonly AppDatabase _appDb;

        // 私有构造函数
        public CookieStore(AppDatabase appDatabase)
        {
            _appDb = appDatabase;
        }

        // 正则表达式（对应 Kotlin 的 AppPattern）
        private static readonly Regex SemicolonRegex = new Regex(@";");
        private static readonly Regex EqualsRegex = new Regex(@"=");

        private ICookieDao _cookieDao
        {
            get
            {
                return _appDb.CookieDao;
            }
        }

        /// <summary>
        /// 保存cookie到数据库，会自动识别url的二级域名（对应 Kotlin 的 setCookie）
        /// </summary>
        public async Task SetCookieAsync(string url, string cookie)
        {
            try
            {
                var domain = NetworkUtils.GetSubDomain(url);
                CacheManager.Instance.PutMemory($"{domain}_cookie", cookie ?? "");

                var cookieBean = new Cookie { Url = domain, Value = cookie ?? "" };
                await _cookieDao.InsertAsync(cookieBean);
            }
            catch (Exception e)
            {
                // TODO: 日志记录
                // AppLog.Put($"保存Cookie失败\n{e}", e);
                Console.WriteLine($"保存Cookie失败: {e.Message}");
            }
        }

        /// <summary>
        /// 替换cookie（对应 Kotlin 的 replaceCookie）
        /// </summary>
        public async Task ReplaceCookieAsync(string url, string cookie)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(cookie))
            {
                return;
            }

            var oldCookie = await GetCookieNoSessionAsync(url);
            if (string.IsNullOrEmpty(oldCookie))
            {
                await SetCookieAsync(url, cookie);
            }
            else
            {
                var cookieMap = CookieToMap(oldCookie);
                var newCookieMap = CookieToMap(cookie);
                foreach (var kvp in newCookieMap)
                {
                    cookieMap[kvp.Key] = kvp.Value;
                }
                var newCookie = MapToCookie(cookieMap);
                await SetCookieAsync(url, newCookie);
            }
        }

        /// <summary>
        /// 获取url所属的二级域名的cookie（对应 Kotlin 的 getCookie）
        /// </summary>
        public async Task<string> GetCookieAsync(string url)
        {
            var domain = NetworkUtils.GetSubDomain(url);

            var cookie = await GetCookieNoSessionAsync(url);
            var sessionCookie = GetSessionCookie(domain);

            var cookieMap = MergeCookiesToMap(cookie, sessionCookie);

            var ck = MapToCookie(cookieMap) ?? "";

            // 限制 Cookie 长度不超过 4096 字节
            while (ck.Length > 4096)
            {
                // 随机移除一个 cookie
                var removeKey = cookieMap.Keys.ElementAt(new Random().Next(cookieMap.Count));
                await RemoveCookieKeyAsync(url, removeKey);
                cookieMap.Remove(removeKey);
                ck = MapToCookie(cookieMap) ?? "";
            }

            return ck;
        }

        /// <summary>
        /// 获取特定key的cookie值（对应 Kotlin 的 getKey）
        /// </summary>
        public async Task<string> GetKeyAsync(string url, string key)
        {
            var cookie = await GetCookieAsync(url);
            var sessionCookie = GetSessionCookie(url);
            var cookieMap = MergeCookiesToMap(cookie, sessionCookie);
            return cookieMap.TryGetValue(key, out var value) ? value : "";
        }

        /// <summary>
        /// 移除cookie（对应 Kotlin 的 removeCookie）
        /// </summary>
        public async Task RemoveCookieAsync(string url)
        {
            var domain = NetworkUtils.GetSubDomain(url);

            await _cookieDao.DeleteAsync(domain);

            CacheManager.Instance.DeleteMemory($"{domain}_cookie");
            CacheManager.Instance.DeleteMemory($"{domain}_session_cookie");
        }

        /// <summary>
        /// 将cookie字符串转换为字典（对应 Kotlin 的 cookieToMap）
        /// </summary>
        public IDictionary<string, string> CookieToMap(string cookie)
        {
            var cookieMap = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(cookie))
            {
                return cookieMap;
            }

            var pairArray = SemicolonRegex.Split(cookie)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            foreach (var pair in pairArray)
            {
                var pairs = EqualsRegex.Split(pair, 2)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();

                if (pairs.Length <= 1)
                {
                    continue;
                }

                var key = pairs[0].Trim();
                var value = pairs[1];

                if (!string.IsNullOrWhiteSpace(value) || value.Trim() == "null")
                {
                    cookieMap[key] = value.Trim();
                }
            }

            return cookieMap;
        }

        /// <summary>
        /// 将字典转换为cookie字符串（对应 Kotlin 的 mapToCookie）
        /// </summary>
        public string MapToCookie(IDictionary<string, string> cookieMap)
        {
            if (cookieMap == null || cookieMap.Count == 0)
            {
                return null;
            }

            var builder = new StringBuilder();
            var index = 0;

            foreach (var kvp in cookieMap)
            {
                if (index > 0)
                {
                    builder.Append("; ");
                }
                builder.Append(kvp.Key).Append("=").Append(kvp.Value);
                index++;
            }

            return builder.ToString();
        }

        /// <summary>
        /// 清空所有cookie（对应 Kotlin 的 clear）
        /// </summary>
        public void Clear()
        {
            _cookieDao.DeleteOkHttpAsync();
        }

        // ================= 辅助方法（对应 CookieManager 中的方法） =================

        /// <summary>
        /// 获取不包含session的cookie（对应 CookieManager.getCookieNoSession）
        /// </summary>
        private async Task<string> GetCookieNoSessionAsync(string url)
        {
            var domain = NetworkUtils.GetSubDomain(url);

            // 先从内存缓存获取
            var cookie = CacheManager.Instance.GetFromMemory($"{domain}_cookie") as string;
            if (!string.IsNullOrEmpty(cookie))
            {
                return cookie;
            }

            var cookieEntity = await _cookieDao.GetAsync(domain);
            if (cookieEntity != null)
            {
                CacheManager.Instance.PutMemory($"{domain}_cookie", cookieEntity.Value);
                return cookieEntity.Value;
            }

            return "";
        }

        /// <summary>
        /// 获取session cookie（对应 CookieManager.getSessionCookie）
        /// </summary>
        private string GetSessionCookie(string domain)
        {
            var cookie = CacheManager.Instance.GetFromMemory($"{domain}_session_cookie") as string;
            return cookie ?? "";
        }

        /// <summary>
        /// 合并多个cookie到字典（对应 CookieManager.mergeCookiesToMap）
        /// </summary>
        private Dictionary<string, string> MergeCookiesToMap(params string[] cookies)
        {
            var cookieMap = new Dictionary<string, string>();

            foreach (var cookie in cookies)
            {
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    continue;
                }

                var map = CookieToMap(cookie);
                foreach (var kvp in map)
                {
                    cookieMap[kvp.Key] = kvp.Value;
                }
            }

            return cookieMap;
        }

        /// <summary>
        /// 移除指定key的cookie（对应 CookieManager.removeCookie）
        /// </summary>
        private async Task RemoveCookieKeyAsync(string url, string key)
        {
            var cookie = await GetCookieAsync(url);
            if (string.IsNullOrEmpty(cookie))
            {
                return;
            }

            var cookieMap = CookieToMap(cookie);
            cookieMap.Remove(key);
            var newCookie = MapToCookie(cookieMap);
            await SetCookieAsync(url, newCookie);
        }
    }
}
