using AngleSharp.Html.Parser;
using Flurl.Http;
using Legado.Core.Data.Entities;
using Legado.Core.Models;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Models.WebBooks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        const string URL2 = "https://legado.aoaostar.com/sources/e29e19ee.json";
        static readonly List<BookSource> bookList=new List<BookSource>();
        string webViewTrue = JsonConvert.SerializeObject(new { webView = true });

        [OneTimeSetUp]
        public async Task Initialize()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            var json = await URL.GetStringAsync();
            var json2 = await URL2.GetStringAsync();
            json = File.ReadAllText(@"..\..\..\..\bookSource.json");
            json2= json2.Replace("[]", "{}");
            bookList .AddRange(JsonConvert.DeserializeObject<List<BookSource>>(json));
            bookList .AddRange(JsonConvert.DeserializeObject<List<BookSource>>(json2));
        }


        [Test]
        public void TestAnalyze()
        {

        }

        [Test]
        public async Task TestSearchAsync()
        {
            var first = bookList.First();
            WebBook wb = new WebBook();
            await wb.SearchBookAwait(first, "诡秘之主");

        }
    }
}
