using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Helps.Http.Api
{
    public interface ICookieManager
    {
        /// <summary>
        /// 保存cookie
        /// </summary>
        void SetCookie(string url, string? cookie);

        /// <summary>
        /// 替换cookie
        /// </summary>
        void ReplaceCookie(string url, string cookie);

        /// <summary>
        /// 获取cookie
        /// </summary>
        string GetCookie(string url);

        /// <summary>
        /// 移除cookie
        /// </summary>
        void RemoveCookie(string url);

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
