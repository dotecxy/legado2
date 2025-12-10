using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Test
{
    internal class BrowserTest
    {
        //pwsh bin\Debug\net8.0\playwright.ps1 install
        [Test]
        public async Task TestWebView()
        {
            await using var browser = new PlaywrightBrowserService();

            // 方式1: 通过 URL 加载
            var result1 = await browser.WebViewAsync(
                url: "htts://sinsam.com",
                js: "document.title"
            );

            // 方式2: 直接加载 HTML
            var html = "<html><body><h1>Hello World</h1></body></html>";
            var result2 = await browser.WebViewAsync(
                html: html,
                js: "document.querySelector('h1').textContent"
            );

            // 方式3: 只获取 HTML
            var fullHtml = await browser.WebViewAsync(url: "https://example.com");
             
        }

        // 高级配置
        [Test]
        public async Task TestAdvanced()
        {
            var browser = new PlaywrightBrowserService();

            // 自定义 UserAgent 和非无头模式
            await browser.CreateBrowserAsync(
                userAgent: "Mozilla/5.0 (Custom Agent)",
                headless: true
            );

            // 执行复杂 JavaScript
            var data = await browser.EvaluateAsync<object>(@"
          let data={
            title: document.title,
            url: window.location.href,
            userAgent: navigator.userAgent,
            screen: {
                width: window.screen.width,
                height: window.screen.height
            }
        }
            data
        ");

            // 截图
            await browser.TakeScreenshotAsync("screenshot.png");

            await browser.DisposeAsync();
        }
    }
}
