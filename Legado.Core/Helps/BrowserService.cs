using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Legado.Core.Helps
{
    public class BrowserService : IAsyncDisposable
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPage _page;
        private readonly TimeSpan _pageLoadTimeout = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _scriptTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 创建浏览器实例
        /// </summary>
        public async Task<IBrowserContext> CreateBrowserAsync(string userAgent = null, bool headless = true)
        {
            // 初始化 Playwright
            _playwright = await Playwright.CreateAsync();

            // 设置浏览器启动选项
            var launchOptions = new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Args = new[]
                {
                "--disable-blink-features=AutomationControlled",
                "--disable-extensions",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--start-maximized"
            }
            };

            // 创建浏览器
            _browser = await _playwright.Chromium.LaunchAsync(launchOptions);

            // 创建浏览器上下文（类似 Selenium 的 Driver）
            var contextOptions = new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                UserAgent = userAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                IgnoreHTTPSErrors = true
            };

            _context = await _browser.NewContextAsync(contextOptions);

            // 设置默认超时
            _context.SetDefaultTimeout((float)_pageLoadTimeout.TotalMilliseconds);

            // 创建页面
            _page = await _context.NewPageAsync();

            // 设置页面超时
            _page.SetDefaultTimeout((float)_scriptTimeout.TotalMilliseconds);

            // 注入脚本禁用 WebDriver 检测
            await _page.AddInitScriptAsync(@"
            Object.defineProperty(navigator, 'webdriver', {
                get: () => undefined
            });
            Object.defineProperty(navigator, 'plugins', {
                get: () => [1, 2, 3, 4, 5]
            });
        ");

            return _context;
        }

        /// <summary>
        /// 对应 Legado: webView(html, url, js)
        /// 加载页面并执行 JS，返回结果
        /// </summary>
        public async Task<string> WebViewAsync(string html = null, string url = null, string js = null)
        {
            try
            {
                // 确保浏览器已初始化
                if (_page == null)
                {
                    await CreateBrowserAsync(headless: true);
                }

                // 1. 导航到页面
                if (!string.IsNullOrEmpty(url))
                {
                    await _page.GotoAsync(url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle,
                        Timeout = (float)_pageLoadTimeout.TotalMilliseconds
                    });
                }
                else if (!string.IsNullOrEmpty(html))
                {
                    // 使用 setContent 直接设置 HTML 内容
                    await _page.SetContentAsync(html, new PageSetContentOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded
                    });
                }
                else
                {
                    throw new ArgumentException("必须提供 url 或 html 参数");
                }

                // 等待页面完全加载
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // 2. 执行注入的 JS
                if (!string.IsNullOrEmpty(js))
                {
                    // 自动包装 JS 代码（如果用户没有写 return）
                    string wrappedJs = js.Trim();

                    // 执行 JavaScript
                    var result = await _page.EvaluateAsync<object>(wrappedJs);

                    // 处理不同类型的返回值
                    return ConvertResultToString(result);
                }

                // 如果没有 JS，返回整个 HTML
                return await _page.ContentAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebView Error] {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 将 JavaScript 返回值转换为字符串
        /// </summary>
        private string ConvertResultToString(object result)
        {
            if (result == null) return string.Empty;

            if (result is string str) return str;
            if (result is IElementHandle element) return element.InnerHTMLAsync().Result;
            if (result is JsonElement jsonElement) return jsonElement.ToString();

            // 尝试序列化复杂对象
            try
            {
                return JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch
            {
                return result.ToString();
            }
        }

        /// <summary>
        /// 执行 JavaScript 代码并返回指定类型的结果
        /// </summary>
        public async Task<T> EvaluateAsync<T>(string script, object arg = null)
        {
            if (_page == null)
                throw new InvalidOperationException("浏览器未初始化");

            return await _page.EvaluateAsync<T>(script, arg);
        }

        /// <summary>
        /// 获取页面源代码
        /// </summary>
        public async Task<string> GetPageSourceAsync()
        {
            return await _page.ContentAsync();
        }

        /// <summary>
        /// 截屏
        /// </summary>
        public async Task<byte[]> TakeScreenshotAsync(string path = null)
        {
            var screenshot = await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = path,
                FullPage = true
            });

            return screenshot;
        }

        /// <summary>
        /// 获取页面标题
        /// </summary>
        public async Task<string> GetTitleAsync()
        {
            return await _page.TitleAsync();
        }

        /// <summary>
        /// 获取当前 URL
        /// </summary>
        public async Task<string> GetCurrentUrlAsync()
        {
            return _page.Url;
        }

        /// <summary>
        /// 设置 Cookie
        /// </summary>
        public async Task SetCookiesAsync(IEnumerable<Cookie> cookies, string url = null)
        {
            var playwrightCookies = cookies.Select(c => new Microsoft.Playwright.Cookie
            {
                Name = c.Name,
                Value = c.Value,
                Domain = c.Domain,
                Path = c.Path,
                Expires = c?.Expires,
                HttpOnly = c.HttpOnly,
                Secure = c.Secure,
                SameSite = c.SameSite
            }).ToArray();

            await _context.AddCookiesAsync(playwrightCookies);

            if (!string.IsNullOrEmpty(url))
            {
                await _page.GotoAsync(url);
            }
        }

        /// <summary>
        /// 获取所有 Cookie
        /// </summary>
        public async Task<IReadOnlyList<Microsoft.Playwright.BrowserContextCookiesResult>> GetCookiesAsync()
        {
            return await _context.CookiesAsync();
        }

        /// <summary>
        /// 等待元素出现
        /// </summary>
        public async Task<IElementHandle> WaitForSelectorAsync(string selector, float? timeout = null)
        {

            return await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
            {
                Timeout = timeout ?? (float)_scriptTimeout.TotalMilliseconds
            });
        }

        /// <summary>
        /// 资源清理
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_page != null)
            {
                await _page.CloseAsync();
                _page = null;
            }

            if (_context != null)
            {
                await _context.CloseAsync();
                _context = null;
            }

            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }

            _playwright?.Dispose();
            _playwright = null;
        }
    }
}
