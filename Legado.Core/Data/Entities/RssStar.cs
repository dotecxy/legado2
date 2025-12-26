using Newtonsoft.Json;
using SQLite;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// RSS收藏（对应 RssStar.kt）
    /// </summary>
    [Table("rss_stars")]
    public class RssStar
    {
        /// <summary>
        /// 来源URL（主键之一）
        /// </summary>
        [PrimaryKey]
        [Column("origin")]
        [JsonProperty("origin")]
        public string Origin { get; set; } = "";

        /// <summary>
        /// 文章链接（主键之一）
        /// </summary>
        [PrimaryKey]
        [Column("link")]
        [JsonProperty("link")]
        public string Link { get; set; } = "";

        /// <summary>
        /// 排序
        /// </summary>
        [Column("sort")]
        [JsonProperty("sort")]
        public string Sort { get; set; } = "";

        /// <summary>
        /// 标题
        /// </summary>
        [Column("title")]
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// 收藏时间
        /// </summary>
        [Column("star_time")]
        [JsonProperty("starTime")]
        public long StarTime { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /// <summary>
        /// 发布日期
        /// </summary>
        [Column("pub_date")]
        [JsonProperty("pubDate")]
        public string PubDate { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column("description")]
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        [Column("content")]
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [Column("image")]
        [JsonProperty("image")]
        public string Image { get; set; }

        /// <summary>
        /// 分组
        /// </summary>
        [Column("group")]
        [JsonProperty("group")]
        public string Group { get; set; } = "默认分组";

        /// <summary>
        /// 变量
        /// </summary>
        [Column("variable")]
        [JsonProperty("variable")]
        public string Variable { get; set; }

        public override int GetHashCode()
        {
            return Link.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is RssStar other)
            {
                return Origin == other.Origin && Link == other.Link;
            }
            return false;
        }

        /// <summary>
        /// 转换为文章（对应 Kotlin 的 toRssArticle）
        /// </summary>
        public RssArticle ToRssArticle()
        {
            return new RssArticle
            {
                Origin = Origin,
                Sort = Sort,
                Title = Title,
                Order = StarTime,
                Link = Link,
                PubDate = PubDate,
                Description = Description,
                Content = Content,
                Image = Image,
                Group = Group,
                Variable = Variable
            };
        }
    }
}
