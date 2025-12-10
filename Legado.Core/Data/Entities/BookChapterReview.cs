using Newtonsoft.Json;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 章节段评（对应 BookChapterReview.kt）
    /// </summary>
    public class BookChapterReview
    {
        /// <summary>
        /// 书籍ID
        /// </summary>
        [JsonProperty("bookId")]
        public long BookId { get; set; } = 0;

        /// <summary>
        /// 章节ID
        /// </summary>
        [JsonProperty("chapterId")]
        public long ChapterId { get; set; } = 0;

        /// <summary>
        /// 摘要URL
        /// </summary>
        [JsonProperty("summaryUrl")]
        public string SummaryUrl { get; set; } = "";
    }
}
