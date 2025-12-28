
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
using System.DirectoryServices;
using System.IO;
using static Legado.Windows.Program;

namespace Legado.Windows
{
    [SingletonDependency]
    public partial class Form1 : BorderlessForm, IMessageProc
    {

        const string URL = "https://jihulab.com/aoaostar/legado/-/raw/release/cache/71e56d4f1d8f1bff61fdd3582ef7513600a9e108.json";
        const string URL2 = "https://legado.aoaostar.com/sources/e29e19ee.json";
        static readonly List<BookSource> bookList = new List<BookSource>();
        static List<BookSource> bookSource;
        string webViewTrue = JsonConvert.SerializeObject(new { webView = true });

        static Form1()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            var json = URL.GetStringAsync().Result;
            var json2 = URL2.GetStringAsync().Result;
            json = File.ReadAllText(@"..\..\..\..\bookSource.json");
            json2 = json2.Replace("[]", "{}");
            var bookSourcePath = @"D:\testb\bs.txt";
            if (!File.Exists(bookSourcePath))
            {
                Console.WriteLine("错误：文件不存在！");
                return;
            }

            var bookSourceContent = File.ReadAllText(bookSourcePath);
            bookList.AddRange(JsonConvert.DeserializeObject<List<BookSource>>(bookSourceContent));
            bookList = new List<BookSource>() { bookList[bookList.Count - 4] };
            bookSource = bookList;

            //bookList.AddRange(JsonConvert.DeserializeObject<List<BookSource>>(json));
            //bookList.AddRange(JsonConvert.DeserializeObject<List<BookSource>>(json2));
        }


        BlazorWebView blazorWebView;

        public event EventHandler<Message> OnMessage;

        public Form1(IServiceProvider services)
        {
            InitializeComponent();

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
            this.Text = "Blazor Windows Forms Hybrid App";
        }

        protected async override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // WebBook webBook = new WebBook();
            //var searchResults= await webBook.SearchBookAwait(bookList.First(), "诡秘之主");

            // var first = searchResults.Where(s => s.Author == "爱潜水的乌贼" && s.Name == "诡秘之主").First();
            // var book = first.ToBook();
            // var infoList = await webBook.GetBookInfoAwait(bookList.First(), book);
            // var chapterList = await webBook.GetChapterListAwait(bookList.First(), book);
            // var first2 = chapterList.First();
            // var content = await webBook.GetContentAwait(bookList.First(), book, first2);
            // var content3 = ContentHelper.ReSegment(content, first2.Title);
            // AnalyzeUrl ar = new AnalyzeUrl(first2.BookUrl);
            // content = ar.htmlFormat(content);




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
    }
}
