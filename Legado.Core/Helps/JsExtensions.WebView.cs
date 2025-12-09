using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks; 

namespace Legado.Core.Helps
{
    /// <summary> 
    /// </summary>
    public partial class JsExtensions
    {
        private BrowserService browserService;

        /// <summary>
        /// 对应 Legado: webView(html, url, js)
        /// 加载页面并执行 JS，返回结果
        /// </summary>
        public Task<string> webViewAsync(string html, string url, string js)
        {
            if (browserService == null)
            {
                browserService = new BrowserService();
            }
            return browserService.WebViewAsync(html, url, js);
        }

         
    }
}

