using Legado.Core.Helps.Http.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Helps.Http
{
    /// <summary>
    /// Cookie 存储管理器 (对应 Kotlin: CookieStore.kt)
    /// 实现 Cookie 的保存、获取、替换、删除等功能
    /// </summary>
    public sealed class CookieStore : ICookieManager
    {
        public static CookieStore Instance { get; } = new CookieStore();
         

        public IDictionary<string, string> CookieToMap(string cookie)
        {
            throw new NotImplementedException();
        }

        public string GetCookie(string url)
        {
            return null;
        }

        public string MapToCookie(IDictionary<string, string> cookieMap)
        {
            throw new NotImplementedException();
        }

        public void RemoveCookie(string url)
        {
            throw new NotImplementedException();
        }

        public void ReplaceCookie(string url, string cookie)
        {
            throw new NotImplementedException();
        }

        public void SetCookie(string url, string cookie)
        { 
        }
    }
}
