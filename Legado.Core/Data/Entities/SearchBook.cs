using System;
using Newtonsoft.Json;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 搜索书籍结果 (对应 SearchBook.kt)
    /// 通常不存入数据库，仅用于内存展示
    /// </summary>
    public class SearchBook
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [JsonProperty("intro")]
        public string Intro { get; set; }

        [JsonProperty("latestChapterTitle")]
        public string LatestChapterTitle { get; set; }

        [JsonProperty("tocUrl")]
        public string TocUrl { get; set; }

        // 转换方法：将搜索结果转换为加入书架的书籍对象
        public Book ToBook()
        {
            return new Book
            {
                Name = Name,
                Author = Author,
                Kind = Kind,
                BookUrl = BookUrl,
                Origin = Origin,
                OriginName = Origin, // 假设
                CoverUrl = CoverUrl,
                Intro = Intro,
                LatestChapterTitle = LatestChapterTitle,
                TocUrl = string.IsNullOrEmpty(TocUrl) ? BookUrl : TocUrl,
                LatestChapterTime = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
        }
    }
}