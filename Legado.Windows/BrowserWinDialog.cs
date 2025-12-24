using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebView2Control = Microsoft.Web.WebView2.WinForms.WebView2;

namespace Legado.Windows
{
    public class BrowserWinDialog : Form
    {
        private WebView2Control webView;

        public string Url { get; private set; }

        public string Cookie { get; private set; }

        public BrowserWinDialog(string url)
        {
            this.Url = url;
            webView = new WebView2Control();
            webView.Dock = DockStyle.Fill;
            Controls.Add(webView);
        }

        protected async override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            webView.CoreWebView2.Navigate(Url);
        }

        private async void CoreWebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            var cookie = await webView.ExecuteScriptAsync("document.cookie");
            Cookie = cookie;
        }
    }
}
