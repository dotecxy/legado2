using Legado.Core.Helps.Source;
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
    public abstract partial class JsExtensions
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

        public string webViewGetOverrideUrl(string html, string url, string js)
        {
            return webViewAsync(html, url, js).Result;
        }

        public string webView(string html, string url, string js)
        {
            return webViewAsync(html, url, js).Result;
        }

        public string StartBrowser(string url, string title)
        {
            if (browserService == null)
            {
                browserService = new BrowserService();
            }
            return browserService.WebViewAsync(null, url, null, false).Result;
        }

        public async Task<StrResponse> StartBrowserAwait(string url, string title, bool refetchAfterSuccess)
        {
            if (browserService == null)
            {
                browserService = new BrowserService();
            }
            var body = await SourceVerificationHelper.GetVerificationResult(getSource(), url, title, true, refetchAfterSuccess);
            return new StrResponse(url, body);
        }

        public Task<StrResponse> StartBrowserAwait(string url, string title)
        {
            return StartBrowserAwait(url, title, true);
        }
    }
}

