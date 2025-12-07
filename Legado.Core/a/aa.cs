using Newtonsoft.Json;

namespace Legado.Core.Models
{
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

        // 转换为 Book 实体的方法
        public Book ToBook()
        {
            return new Book
            {
                Name = this.Name,
                Author = this.Author,
                BookUrl = this.BookUrl,
                Origin = this.Origin,
                CoverUrl = this.CoverUrl,
                Intro = this.Intro,
                LatestChapterTitle = this.LatestChapterTitle,
                Kind = this.Kind
            };
        }
    }
}