using FreeSql.DataAnnotations;
using Legado.Core.Constants;
using Legado.Core.Models;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 章节模型（功能增强版，对应 Kotlin 的 BookChapter.kt）
    /// 实现 IRuleData 接口，支持规则解析
    /// </summary>
    [Table(Name = "chapters")]
    [Index("IX_Chapters_BookUrl", "bookUrl", false)] // 对应 Kotlin 的 indices = [(Index(value = ["bookUrl"], unique = false))]
    [Index("IX_Chapters_BookUrl_Index", "bookUrl,index", true)] // 对应 Kotlin 的 (Index(value = ["bookUrl", "index"], unique = true))
    public class BookChapter : IEquatable<BookChapter>, IRuleData
    {
        /// <summary>
        /// 章节地址
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// 章节标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// 是否是卷名
        /// </summary>
        [JsonProperty("isVolume")]
        public bool IsVolume { get; set; } = false;

        /// <summary>
        /// 用来拼接相对url
        /// </summary>
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; } = "";

        /// <summary>
        /// 书籍地址
        /// </summary>
        [Column(IsPrimary = true, StringLength = 255)]
        [JsonProperty("bookUrl")]
        public string BookUrl { get; set; } = "";

        /// <summary>
        /// 章节序号
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; set; } = 0;

        /// <summary>
        /// 是否VIP
        /// </summary>
        [JsonProperty("isVip")]
        public bool IsVip { get; set; } = false;

        /// <summary>
        /// 是否已购买
        /// </summary>
        [JsonProperty("isPay")]
        public bool IsPay { get; set; } = false;

        /// <summary>
        /// 音频真实URL
        /// </summary>
        [JsonProperty("resourceUrl")]
        public string? ResourceUrl { get; set; } = null;

        /// <summary>
        /// 更新时间或其他章节附加信息
        /// </summary>
        [JsonProperty("tag")]
        public string? Tag { get; set; } = null;

        /// <summary>
        /// 本章节字数
        /// </summary>
        [JsonProperty("wordCount")]
        public string? WordCount { get; set; } = null;

        /// <summary>
        /// 章节起始位置
        /// </summary>
        [JsonProperty("start")]
        public long? Start { get; set; } = null;

        /// <summary>
        /// 章节终止位置
        /// </summary>
        [JsonProperty("end")]
        public long? End { get; set; } = null;

        /// <summary>
        /// EPUB书籍当前章节的fragmentId
        /// </summary>
        [JsonProperty("startFragmentId")]
        public string? StartFragmentId { get; set; } = null;

        /// <summary>
        /// EPUB书籍下一章节的fragmentId
        /// </summary>
        [JsonProperty("endFragmentId")]
        public string? EndFragmentId { get; set; } = null;

        /// <summary>
        /// 变量
        /// </summary>
        [JsonProperty("variable")]
        public string? Variable { get; set; } = null;

        /// <summary>
        /// 标题MD5（用于文件名）
        /// </summary>
        [Column(IsIgnore = true)]
        [JsonIgnore]
        public string? TitleMD5 { get; set; }

        /// <summary>
        /// 变量映射表（用于规则解析）
        /// </summary>
        [Column(IsIgnore = true)]
        [JsonIgnore]
        public Dictionary<string, string> VariableMap
        {
            get
            {
                if (_variableMap == null)
                {
                    _variableMap = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(Variable))
                    {
                        try
                        {
                            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Variable);
                            if (dict != null)
                            {
                                _variableMap = dict;
                            }
                        }
                        catch
                        {
                            // 解析失败，使用空字典
                        }
                    }
                }
                return _variableMap;
            }
        }
        private Dictionary<string, string> _variableMap;

        /// <summary>
        /// 存储变量
        /// </summary>
        public virtual bool PutVariable(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (value == null)
                {
                    VariableMap.Remove(key);
                }
                else
                {
                    VariableMap[key] = value;
                }
                Variable = JsonConvert.SerializeObject(VariableMap);
                return true;
            }
            return false;
        }

        public virtual void PutBigVariable(string key, string value)
        {
            // TODO: 实现大数据存储
        }

        public virtual string GetBigVariable(string key)
        {
            // TODO: 实现大数据读取
            return null;
        }

        public virtual string GetVariable()
        {
            if (VariableMap == null || VariableMap.Count == 0)
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
        /// 获取主键字符串
        /// </summary>
        public string PrimaryStr()
        {
            return BookUrl + Url;
        } 

        /// <summary>
        /// 获取章节的绝对URL（对应 Kotlin 的 getAbsoluteURL）
        /// </summary>
        public string GetAbsoluteURL()
        {
            // 二级目录解析的卷链接为空 返回目录页的链接
            if (Url.StartsWith(Title) && IsVolume) return BaseUrl;
            
            // 简化的URL处理逻辑，完整实现需要NetworkUtils
            var urlBefore = Url;
            var paramStartIndex = Url.IndexOf(',');
            if (paramStartIndex > 0)
            {
                urlBefore = Url.Substring(0, paramStartIndex);
            }
            
            // 这里简化处理，实际实现需要完整的URL解析逻辑
            var urlAbsoluteBefore = GetAbsoluteUrl(BaseUrl, urlBefore);
            if (urlBefore.Length == Url.Length)
            {
                return urlAbsoluteBefore;
            }
            else
            {
                return urlAbsoluteBefore + "," + Url.Substring(paramStartIndex + 1);
            }
        }

        private string GetAbsoluteUrl(string baseUrl, string relativeUrl)
        {
            return NetworkUtils.GetAbsoluteURL(baseUrl, relativeUrl);
        }

        /// <summary>
        /// 获取文件名（对应 Kotlin 的 getFileName）
        /// </summary>
        public string GetFileName(string suffix = "nb")
        {
            EnsureTitleMD5Init();
            return string.Format("{0:D5}-{1}.{2}", Index, TitleMD5, suffix);
        }

        /// <summary>
        /// 获取字体名（对应 Kotlin 的 getFontName）
        /// </summary>
        public string GetFontName()
        {
            EnsureTitleMD5Init();
            return string.Format("{0:D5}-{1}.ttf", Index, TitleMD5);
        }

        private void EnsureTitleMD5Init()
        {
            if (string.IsNullOrEmpty(TitleMD5))
            {
                TitleMD5 = GetMd5Encode16(Title);
            }
        }

        private string GetMd5Encode16(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var md5Hash = input.ToMd5(); // 使用项目中已有的ToMd5扩展方法
            return md5Hash.Length > 16 ? md5Hash.Substring(0, 16) : md5Hash;
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
