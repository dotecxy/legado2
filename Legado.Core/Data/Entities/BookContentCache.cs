using Newtonsoft.Json;
using Legado.FreeSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍缓存，离线正文缓存 (对应 BookContentCache.kt)
    /// </summary>
    [Table("book_content_cache")]
    public class BookContentCache
    {

        /// <summary>
        /// 书籍URL，作为主键
        /// </summary>
        [PrimaryKey, Column("book_url")]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } 
        /// <summary>
        /// 正文
        /// </summary>
        [Column("content")]
        [JsonProperty("content")]
        public string Content { get; set; }

        [Column("created_time")]
        [JsonProperty("createdTime")]
        public long CreatedTime { get; set; } = 0;

        [Column("update_time")]
        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; } = 0;
    }
}
