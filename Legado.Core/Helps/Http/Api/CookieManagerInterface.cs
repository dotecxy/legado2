using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Core.Helps.Http.Api
{
    /// <summary>
    /// Cookie 管理器接口 (对应 Kotlin: ICookieManager 接口)
    /// 定义 Cookie 管理的标准接口
    /// </summary>
    public interface ICookieManager
    {
        /// <summary>
        /// 保存cookie
        /// </summary>
        Task SetCookieAsync(string url, string? cookie);

        /// <summary>
        /// 替换cookie
        /// </summary>
        Task ReplaceCookieAsync(string url, string cookie);

        /// <summary>
        /// 获取cookie
        /// </summary>
        Task<string> GetCookieAsync(string url);

        /// <summary>
        /// 移除cookie
        /// </summary>
        Task RemoveCookieAsync(string url);

        /// <summary>
        /// 将cookie字符串转换为字典
        /// </summary>
        IDictionary<string, string> CookieToMap(string cookie);

        /// <summary>
        /// 将字典转换为cookie字符串
        /// </summary>
        string? MapToCookie(IDictionary<string, string>? cookieMap);
    }
}
