using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
// 引入 DevTools 对应的网络模块，版本可能需根据你的 Chrome 版本调整，通常使用 V1xx
using DevToolsSessionDomains = OpenQA.Selenium.DevTools.V142.DevToolsSessionDomains;
using Network = OpenQA.Selenium.DevTools.V142.Network;

namespace Legado.Core.Helps
{
    /// <summary>
    /// JsExtensions WebView 扩展部分 (对应 Kotlin: JsExtensions.kt 中的 WebView 相关方法)
    /// 提供基于 Selenium 的 WebView 模拟功能
    /// </summary>
    public partial class JsExtensions
    {
        // 设置 WebDriver 的超时时间
        private readonly TimeSpan _pageLoadTimeout = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _scriptTimeout = TimeSpan.FromSeconds(15);

        /// <summary>
        /// 创建配置好的 ChromeDriver
        /// </summary>
        private IWebDriver CreateDriver(string userAgent = null)
        { 
            // 设置Chrome选项
            var options = new ChromeOptions();

            // 可选：无头模式（不显示浏览器界面）
            options.AddArgument("--headless");
             
            // 设置 User-Agent (如果有传入，否则使用默认)
            if (!string.IsNullOrEmpty(userAgent))
            {
                options.AddArgument($"--user-agent={userAgent}");
            }
            else
            {
                // 默认伪装成普通浏览器
                options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            }

            // 禁用自动化控制提示
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            // 常用配置选项
            options.AddArguments(
                "--disable-blink-features=AutomationControlled",
                "--disable-extensions",
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--start-maximized"
            );

            var driver = new ChromeDriver(options);
            driver.Manage().Timeouts().PageLoad = _pageLoadTimeout;
            driver.Manage().Timeouts().AsynchronousJavaScript = _scriptTimeout;

            // TODO: 如果需要，可以在这里注入 Cookies
            // foreach (var cookie in _cookieContainer.GetCookies(...)) driver.Manage().Cookies.AddCookie(...);
            // 设置窗口大小（可选）
            driver.Manage().Window.Maximize();

            // 设置隐式等待时间
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            return driver;
        }

        /// <summary>
        /// 对应 Legado: webView(html, url, js)
        /// 加载页面并执行 JS，返回结果
        /// </summary>
        public string webView(string html, string url, string js)
        {
            using (var driver = CreateDriver())
            {
                try
                {
                    // 1. 导航
                    if (!string.IsNullOrEmpty(url))
                    {
                        driver.Navigate().GoToUrl(url);
                    }
                    else if (!string.IsNullOrEmpty(html))
                    {
                        // 如果只有 HTML，使用 Data URI 加载
                        // 注意：这无法解决相对路径资源加载问题，除非 base 标签在 html 里
                        string base64Html = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html));
                        driver.Navigate().GoToUrl($"data:text/html;charset=utf-8;base64,{base64Html}");
                    }

                    // 显式等待页面加载完成
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));


                    // 2. 执行注入的 JS
                    if (!string.IsNullOrEmpty(js))
                    {
                        var jsExecutor = (IJavaScriptExecutor)driver;
                        // Legado 的规则中，js 代码通常直接写逻辑。
                        // 在 Selenium ExecuteScript 中，必须有 "return" 关键字才能拿到返回值。
                        // 我们尝试自动包装一下，或者假设用户规则里写了 return。
                        // 这里简单处理：如果 JS 不包含 return 且很短，可能是一个变量名，尝试返回它

                        object result = jsExecutor.ExecuteScript(js);
                        return result?.ToString();
                    }

                    // 如果没有 JS，Legado 默认行为是返回整个 HTML
                    return driver.PageSource;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WebView Error] {ex.Message}");
                    return ""; // 出错返回空或错误信息
                }
            }
        }

        /// <summary>
        /// 对应 Legado: webViewGetSource(html, url, js, sourceRegex)
        /// 网络嗅探：加载页面 -> 拦截网络请求 -> 匹配正则 -> 返回 URL
        /// </summary>
        public string webViewGetSource(string html, string url, string js, string sourceRegex)
        {
            string foundUrl = null;
            var regex = new Regex(sourceRegex);
            using (var driver = CreateDriver())
            using (var signal = new ManualResetEventSlim(false)) // 用于阻塞等待
            {
                try
                {
                    // 获取 DevTools 会话以监听网络
                    IDevTools devTools = driver as IDevTools;
                    var session = devTools.GetDevToolsSession();
                    var domains = session.GetVersionSpecificDomains<DevToolsSessionDomains>();

                    // 开启网络监听
                    domains.Network.Enable(new Network.EnableCommandSettings());

                    // 注册请求拦截事件
                    domains.Network.RequestWillBeSent += (sender, e) =>
                    {
                        if (regex.IsMatch(e.Request.Url))
                        {
                            foundUrl = e.Request.Url;
                            signal.Set(); // 找到了，通知主线程解锁
                        }
                    };

                    // 1. 导航
                    if (!string.IsNullOrEmpty(url)) driver.Navigate().GoToUrl(url);
                    else if (!string.IsNullOrEmpty(html))
                    {
                        string base64Html = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html));
                        driver.Navigate().GoToUrl($"data:text/html;charset=utf-8;base64,{base64Html}");
                    }

                    // 2. 执行 JS (有时需要点击按钮才能触发资源加载)
                    if (!string.IsNullOrEmpty(js))
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript(js);
                    }

                    // 3. 等待结果 (最多等 15 秒)
                    signal.Wait(TimeSpan.FromSeconds(15));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WebView Sniff Error] {ex.Message}");
                }
            }
            return foundUrl;
        }

        /// <summary>
        /// 对应 Legado: webViewGetOverrideUrl(html, url, js, overrideUrlRegex)
        /// 拦截页面跳转/重定向 URL
        /// </summary>
        public string webViewGetOverrideUrl(string html, string url, string js, string overrideUrlRegex)
        {
            string foundUrl = null;
            var regex = new Regex(overrideUrlRegex);

            using (var driver = CreateDriver())
            {
                try
                {
                    // 在 Selenium 中，GetOverrideUrl 比较难完美模拟 Android WebView 的 shouldOverrideUrlLoading
                    // 但我们可以检查 URL 的变化 或 监听主文档的请求

                    // 方法 A: 简单的 URL 变化检测 (适用于最终跳转)
                    // 方法 B: 同样使用 DevTools 监听 RequestWillBeSent，但只关注 Document 类型的请求

                    // 这里使用方法 B (DevTools)
                    IDevTools devTools = driver as IDevTools;
                    var session = devTools.GetDevToolsSession();
                    var domains = session.GetVersionSpecificDomains<DevToolsSessionDomains>();

                    domains.Network.Enable(new Network.EnableCommandSettings());

                    var waitTask = new TaskCompletionSource<string>();

                    domains.Network.RequestWillBeSent += (sender, e) =>
                    {
                        // 过滤条件：匹配正则 且 或者是 Document 类型 (或者是 XHR 视规则而定)
                        if (regex.IsMatch(e.Request.Url))
                        {
                            waitTask.TrySetResult(e.Request.Url);
                        }
                    };

                    // 导航
                    if (!string.IsNullOrEmpty(url)) driver.Navigate().GoToUrl(url);

                    // 执行 JS
                    if (!string.IsNullOrEmpty(js))
                    {
                        ((IJavaScriptExecutor)driver).ExecuteScript(js);
                    }

                    // 等待捕获，超时 10s
                    if (waitTask.Task.Wait(TimeSpan.FromSeconds(10)))
                    {
                        foundUrl = waitTask.Task.Result;
                    }

                    // 兜底：如果上面的监听没抓到，检查一下当前 URL 是否符合
                    if (string.IsNullOrEmpty(foundUrl) && regex.IsMatch(driver.Url))
                    {
                        foundUrl = driver.Url;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WebView Override Error] {ex.Message}");
                }
            }
            return foundUrl;
        }
    }
}

