using Legado.Core.Constants;
using Legado.Core.Models;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 章节模型（功能增强版，对应 Kotlin 的 BookChapter.kt）
    /// 实现 IRuleData 接口，支持规则解析
    /// </summary>
    [Table("book_chapters")]
    public class BookChapter : IEquatable<BookChapter>, IRuleData
    {
        /// <summary>
        /// 章节地址
        /// </summary>
        [Indexed(Name = "IX_BookChapter_Url", Order = 1, Unique = true)]
        [Column("url")]
        [PrimaryKey]
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// 书籍URL（外键）
        /// </summary>
        [Indexed(Name = "IX_BookChapter_BookUrl", Order = 2)]
        [Column("book_url")]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } = "";

        /// <summary>
        /// 章节序号
        /// </summary>
        [Indexed(Name = "IX_BookChapter_Index")]
        [Column("index")]
        [JsonProperty("index")]
        public int Index { get; set; } = 0;

        /// <summary>
        /// 章节标题
        /// </summary>
        [Column("title")]
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// 章节解析时的BaseURL（用于相对路径）
        /// </summary>
        [Column("base_url")]
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; } = "";

        /// <summary>
        /// 书签内容（用于定位）
        /// </summary>
        [Column("bookmark_text")]
        [JsonProperty("bookmarkText")]
        public string BookmarkText { get; set; } = "";

        /// <summary>
        /// 内容字符数
        /// </summary>
        [Column("bookmark_content")]
        [JsonProperty("bookmarkContent")]
        public long BookmarkContent { get; set; } = 0;

        /// <summary>
        /// 页面索引（用于分页）
        /// </summary>
        [Column("page_index")]
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; } = -1;

        /// <summary>
        /// 总章节数
        /// </summary>
        [Column("total_chapter_num")]
        [JsonProperty("totalChapterNum")]
        public int TotalChapterNum { get; set; } = 0;

        /// <summary>
        /// 是否为卷名（分卷）
        /// </summary>
        [Ignore]
        [JsonProperty("isVolume")]
        public bool IsVolume { get; set; } = false;

        /// <summary>
        /// 是否VIP章节
        /// </summary>
        [Ignore]
        [JsonProperty("isVip")]
        public bool IsVip { get; set; } = false;

        /// <summary>
        /// 是否付费章节
        /// </summary>
        [Ignore]
        [JsonProperty("isPay")]
        public bool IsPay { get; set; } = false;

        /// <summary>
        /// 章节标签（如：最新、VIP等）
        /// </summary>
        [Column("tag")]
        [JsonProperty("tag")]
        public string Tag { get; set; } = "";

        /// <summary>
        /// 起始片段ID
        /// </summary>
        [Column("start_fragment_id")]
        [JsonProperty("startFragmentId")]
        public long StartFragmentId { get; set; } = 0;

        /// <summary>
        /// 结束片段ID
        /// </summary>
        [Column("end_fragment_id")]
        [JsonProperty("endFragmentId")]
        public long EndFragmentId { get; set; } = 0;

        /// <summary>
        /// 变量（JSON格式）
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public string Variable { get; set; }

        /// <summary>
        /// 变量映射表（用于规则解析）
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public Dictionary<string, string> VariableMap { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 存储变量
        /// </summary>
        public virtual bool PutVariable(string key, string value)
        {
            VariableMap[key] = value;
            return true;
        }

        public virtual void PutBigVariable(string key, string value)
        {
            VariableMap[key] = value;
        }

        public virtual string GetBigVariable(string key)
        {
            if (VariableMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        public virtual string GetVariable()
        {
            if (VariableMap.Count == 0)
            {
                return null;
            }
            return JsonConvert.SerializeObject(VariableMap);
        }

        public virtual string GetVariable(string key)
        {
            if (VariableMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return "";
        }

        /// <summary>
        /// 获取章节的绝对URL（对应 Kotlin 的 getAbsoluteURL）
        /// </summary>
        public string GetAbsoluteURL()
        {
            if (string.IsNullOrEmpty(Url))
            {
                return BookUrl;
            }

            // 如果已经是完整URL
            if (Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return Url;
            }

            // 获取基准URL
            var baseUrl = string.IsNullOrEmpty(BaseUrl) ? BookUrl : BaseUrl;
            
            // 处理相对路径
            try
            {
                var baseUri = new Uri(baseUrl);
                var absoluteUri = new Uri(baseUri, Url);
                return absoluteUri.ToString();
            }
            catch
            {
                return Url;
            }
        }

        /// <summary>
        /// 获取章节显示名称（对应 Kotlin 的 getDisplayTitle）
        /// </summary>
        public string GetDisplayTitle()
        {
            return !string.IsNullOrEmpty(Title) ? Title : $"第{Index}章";
        }

        /// <summary>
        /// IEquatable<BookChapter> 实现（用于去重）
        /// </summary>
        public bool Equals(BookChapter other)
        {
            if (other == null) return false;
            
            // 基于 URL 和 BookUrl 判断章节唯一性
            return Url == other.Url && BookUrl == other.BookUrl;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BookChapter);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Url?.GetHashCode() ?? 0);
                hash = hash * 23 + (BookUrl?.GetHashCode() ?? 0);
                return hash;
            }
        }

        /// <summary>
        /// 复制章节（对应 Kotlin 的 copy）
        /// </summary>
        public BookChapter Copy()
        {
            return new BookChapter
            {
                Url = this.Url,
                BookUrl = this.BookUrl,
                Index = this.Index,
                Title = this.Title,
                BaseUrl = this.BaseUrl,
                BookmarkText = this.BookmarkText,
                BookmarkContent = this.BookmarkContent,
                PageIndex = this.PageIndex,
                TotalChapterNum = this.TotalChapterNum,
                IsVolume = this.IsVolume,
                IsVip = this.IsVip,
                IsPay = this.IsPay,
                Tag = this.Tag,
                StartFragmentId = this.StartFragmentId,
                EndFragmentId = this.EndFragmentId,
                Variable = this.Variable
            };
        }

        /// <summary>
        /// 获取文件名（用于缓存文件名，对应 Kotlin 的 getFileName）
        /// </summary>
        public string GetFileName()
        {
            // 使用 Index 和 Title 生成安全的文件名
            var safeTitle = Regex.Replace(Title ?? "", @"[^\w\s-]", "");
            return $"{Index:D5}_{safeTitle}".Trim();
        }

        public override string ToString()
        {
            return $"BookChapter(url='{Url}', title='{Title}', index={Index})";
        }
    }
}
