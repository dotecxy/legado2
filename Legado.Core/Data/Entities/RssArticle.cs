using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// RSS文章（对应 RssArticle.kt）
    /// </summary>
    [Table("rssArticles")]
    public class RssArticle : IBaseRssArticle
    {
        /// <summary>
        /// 来源URL（主键之一）
        /// </summary>
        [PrimaryKey]
        [JsonProperty("origin")]
        public string Origin { get; set; } = "";

        /// <summary>
        /// 文章链接（主键之一）
        /// </summary>
        [PrimaryKey]
        [JsonProperty("link")]
        public string Link { get; set; } = "";

        /// <summary>
        /// 排序
        /// </summary>
        [JsonProperty("sort")]
        public string Sort { get; set; } = "";

        /// <summary>
        /// 标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// 顺序
        /// </summary>
        [JsonProperty("order")]
        public long Order { get; set; } = 0;

        /// <summary>
        /// 发布日期
        /// </summary>
        [JsonProperty("pubDate")]
        public string PubDate { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        [JsonProperty("image")]
        public string Image { get; set; }

        /// <summary>
        /// 分组
        /// </summary>
        [JsonProperty("group")]
        public string Group { get; set; } = "默认分组";

        /// <summary>
        /// 是否已读
        /// </summary>
        [JsonProperty("read")]
        public bool Read { get; set; } = false;

        /// <summary>
        /// 变量
        /// </summary>
        [JsonProperty("variable")]
        public string Variable { get; set; }

        [Ignore]
        [JsonIgnore]
        public Dictionary<string, string> VariableMap
        {
            get
            {
                if (_variableMap == null)
                {
                    if (string.IsNullOrEmpty(Variable))
                    {
                        _variableMap = new Dictionary<string, string>();
                    }
                    else
                    {
                        try
                        {
                            _variableMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(Variable) 
                                ?? new Dictionary<string, string>();
                        }
                        catch
                        {
                            _variableMap = new Dictionary<string, string>();
                        }
                    }
                }
                return _variableMap;
            }
        }

        private Dictionary<string, string> _variableMap;

        public override int GetHashCode()
        {
            return Link.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is RssArticle other)
            {
                return Origin == other.Origin && Link == other.Link;
            }
            return false;
        }

        /// <summary>
        /// 转换为收藏（对应 Kotlin 的 toStar）
        /// </summary>
        public RssStar ToStar()
        {
            return new RssStar
            {
                Origin = Origin,
                Sort = Sort,
                Title = Title,
                StarTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Link = Link,
                PubDate = PubDate,
                Description = Description,
                Content = Content,
                Image = Image,
                Group = Group,
                Variable = Variable
            };
        }

        // IRuleData 接口实现
        public virtual bool PutVariable(string key, string value)
        {
            VariableMap[key] = value;
            Variable = JsonConvert.SerializeObject(VariableMap);
            return true;
        }

        public virtual string GetVariable()
        {
            return Variable;
        }

        public virtual string GetVariable(string key)
        {
            return VariableMap.TryGetValue(key, out var value) ? value : "";
        }

        public virtual void PutBigVariable(string key, string value)
        {
            // TODO: 实现大变量存储
        }

        public virtual string GetBigVariable(string key)
        {
            // TODO: 实现大变量读取
            return null;
        }
    }
}
