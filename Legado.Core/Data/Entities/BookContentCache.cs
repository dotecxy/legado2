using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍缓存，离线正文缓存 (对应 Kotlin 的 BookContentCache.kt)
    /// </summary>
    [Table(Name = "book_content_cache")]
    public class BookContentCache
    {

        /// <summary>
        /// 书籍URL，作为主键
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } 
        /// <summary>
        /// 正文
        /// </summary>
        [Column(StringLength = -1)] // TEXT类型
        [JsonProperty("content")]
        public string Content { get; set; }

        
        [JsonProperty("createdTime")]
        public long CreatedTime { get; set; } = 0;

        
        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; } = 0;
    }
}
