using Legado.Core.App;
using Legado.Core.Data.Entities;
using Legado.Core.Helps;
using Legado.Core.Helps.Source;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static SQLite.SQLite3;
using Timer = System.Timers.Timer;

namespace Legado.Windows
{
    public partial class BrowserControl : Form, IDialog
    {
        private Timer _closeTimer;
        public IntentData Data { get; private set; }
        public string Url { get; private set; }
        public string SourceCode { get; private set; }
        public string Cookies { get; private set; }

        public BrowserControl()
        {
            InitializeComponent();
            _closeTimer = new Timer();
            _closeTimer.Interval = 2000;
            _closeTimer.Elapsed += _closeTimer_Elapsed;
        }

        private void _closeTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            _closeTimer.Stop();
            bool ok = Monitor.TryEnter(this);
            if (!ok) return;
            //this.Data.Put("cookies", Newtonsoft.Json.JsonConvert.DeserializeObject(Cookies));
            var c = Newtonsoft.Json.JsonConvert.DeserializeObject(Cookies) as string;
            SourceVerificationHelper.SetResult(this.Data.Get<string>("sourceOrigin"), c);
            DialogResult = DialogResult.OK;
            Monitor.Exit(this);
        }

        public void PutIntentData(IntentData data)
        {
            this.Data = data;
            Url = data.Get<string>("url");
            this.Text = data.Get<string>("title");
        }

        public bool? ShowDailog()
        {
            return base.ShowDialog() == DialogResult.OK;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _closeTimer?.Dispose();
            _closeTimer = null;
            base.OnHandleDestroyed(e);
        }


        protected async override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2");
            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(folder);
            await webView21.EnsureCoreWebView2Async(env);
            webView21.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
            webView21.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            webView21.CoreWebView2.Navigate(Url);

        }

        private async void CoreWebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            SourceCode = webView21.CoreWebView2.Source;
            //var cookies = await webView21.CoreWebView2.CookieManager.GetCookiesAsync("");
            var result = await webView21.ExecuteScriptAsync("document.cookie");
            Cookies = result;
            _closeTimer.Stop();
            _closeTimer.Start();
        }
    }
}
