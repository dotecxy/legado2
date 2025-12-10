using Newtonsoft.Json;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 阅读记录显示（对应 ReadRecordShow.kt）
    /// </summary>
    public class ReadRecordShow
    {
        /// <summary>
        /// 书名
        /// </summary>
        [JsonProperty("bookName")]
        public string BookName { get; set; }

        /// <summary>
        /// 阅读时长
        /// </summary>
        [JsonProperty("readTime")]
        public long ReadTime { get; set; }

        /// <summary>
        /// 最后阅读时间
        /// </summary>
        [JsonProperty("lastRead")]
        public long LastRead { get; set; }
    }
}
