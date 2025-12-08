using AngleSharp.Html.Parser;
using Flurl.Http;
using Legado.Core.Data.Entities;
using Legado.Core.Models;
using Legado.Core.Models.AnalyzeRules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Test
{
    internal class AnalyzeTest
    {
        const string URL = "https://jihulab.com/aoaostar/legado/-/raw/release/cache/71e56d4f1d8f1bff61fdd3582ef7513600a9e108.json";
        List<BookSource> bookList;
        string webViewTrue = JsonConvert.SerializeObject(new { webView = true });

        [OneTimeSetUp]
        public async Task Initialize()
        {
            var json = await URL.GetStringAsync();
            json = File.ReadAllText(@"..\..\..\..\bookSource.json");
            bookList = JsonConvert.DeserializeObject<List<BookSource>>(json);
        }


        [Test]
        public void TestAnalyze()
        {

        }

        [Test]
        public async Task TestSearchAsync()
        {
            var first = bookList.First();
            AnalyzeUrl analyzeUrl = new AnalyzeUrl(first.SearchUrl, page: 1, source: first,baseUrl:first.BookSourceUrl,key:"我的");
           var result= await analyzeUrl.GetStrResponseAwait(useWebView:true);

            webbook
        }
    }
}
