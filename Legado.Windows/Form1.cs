
using Flurl.Http;
using Legado.Core;
using Legado.Core.App;
using Legado.Core.Data.Entities;
using Legado.Core.DependencyInjection;
using Legado.Core.Helps.Books;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Models.WebBooks;
using Legado.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;
using System.DirectoryServices;
using System.IO;
using static Legado.Windows.Program;

namespace Legado.Windows
{
    [SingletonDependency]
    public partial class Form1 : AntdUI.Window, IMessageProc
    {
        private bool _quick = false;
        string webViewTrue = JsonConvert.SerializeObject(new { webView = true });

        static Form1()
        {
        }


        BlazorWebView blazorWebView;

        public event EventHandler<Message> OnMessage;

        public Form1(IServiceProvider services)
        {
            InitializeComponent();
            notifyIcon.Visible = true;
            // 创建 BlazorWebView 控件
            blazorWebView = new BlazorWebView
            {
                Dock = DockStyle.Fill,
                HostPage = "wwwroot/index.html",
                Services = services
            };

            // 注册根组件
            blazorWebView.RootComponents.Add<App>("#app");

            // 添加到窗体
            Controls.Add(blazorWebView);

            // 设置窗体属性
            this.Text = "Legado";

            CenterToScreen();
        }


        protected override void WndProc(ref Message m)
        {
            OnMessage?.Invoke(this, m);
            base.WndProc(ref m);
        }

        protected async override void OnClosed(EventArgs e)
        {
            var host = QServiceProvider.GetService<IHost>();
            await host.StopAsync();
            base.OnClosed(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_quick && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;


                this.WindowState = FormWindowState.Minimized;

                Hide();
                return;
            }
            base.OnFormClosing(e);
        }


        private void QuitQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _quick = true;
            this.Close();
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
        }
    }
}

