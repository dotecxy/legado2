using Legado.Core.Constants;
using Legado.Core.Data.Entities;
using Legado.Core.Models.AnalyzeRules;
using Legado.Core.Utils;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Legado.Core.Models.WebBooks
{
    [Table("chapters")]
    public class BookChapter : IEquatable<BookChapter>, IRuleData
    {
        // 复合主键在 sqlite-net-pcl 中不直接支持，通常建议添加一个自增 ID 或联合唯一索引
        // 这里为了保持逻辑，我们假设 url 和 bookUrl 的组合是唯一的

        [Indexed(Name = "IX_BookChapter_Url_BookUrl", Order = 1, Unique = true)]
        public string Url { get; set; } = "";               // 章节地址

        [Indexed(Name = "IX_BookChapter_BookUrl", Order = 1)] // 普通索引
        [Indexed(Name = "IX_BookChapter_Url_BookUrl", Order = 2, Unique = true)]
        [Indexed(Name = "IX_BookChapter_BookUrl_Index", Order = 1, Unique = true)]
        public string BookUrl { get; set; } = "";           // 书籍地址

        public string Title { get; set; } = "";             // 章节标题
        public bool IsVolume { get; set; } = false;         // 是否是卷名
        public string BaseUrl { get; set; } = "";           // 用来拼接相对url

        [Indexed(Name = "IX_BookChapter_BookUrl_Index", Order = 2, Unique = true)]
        public int Index { get; set; } = 0;                 // 章节序号

        public bool IsVip { get; set; } = false;            // 是否VIP
        public bool IsPay { get; set; } = false;            // 是否已购买
        public string ResourceUrl { get; set; }             // 音频真实URL
        public string Tag { get; set; }                     // 更新时间或其他章节附加信息
        public string WordCount { get; set; }               // 本章节字数
        public long? Start { get; set; }                    // 章节起始位置
        public long? End { get; set; }                      // 章节终止位置
        public string StartFragmentId { get; set; }         // EPUB书籍当前章节的fragmentId
        public string EndFragmentId { get; set; }           // EPUB书籍下一章节的fragmentId
        public string Variable { get; set; }                // 变量 (JSON 字符串)

        #region Ignored Properties / Logic

        private Dictionary<string, string> _variableMap;

        [Ignore]
        public Dictionary<string, string> VariableMap
        {
            get
            {
                if (_variableMap == null)
                {
                    try
                    {
                        _variableMap = !string.IsNullOrEmpty(Variable)
                            ? JsonConvert.DeserializeObject<Dictionary<string, string>>(Variable)
                            : new Dictionary<string, string>();
                    }
                    catch
                    {
                        _variableMap = new Dictionary<string, string>();
                    }
                }
                return _variableMap;
            }
        }

        [Ignore]
        public string TitleMD5 { get; set; }

        #endregion

        #region Methods

        public virtual bool putVariable(string key, string value)
        {
            // 简单模拟父类行为
            if (VariableMap.ContainsKey(key))
            {
                VariableMap[key] = value;
            }
            else
            {
                VariableMap.Add(key, value);
            }

            Variable = JsonConvert.SerializeObject(VariableMap);
            return true;
        }

        public void putBigVariable(string key, string value)
        {
            //RuleBigDataHelp.PutChapterVariable(BookUrl, Url, key, value);
        }

        public string getBigVariable(string key)
        {
            //return RuleBigDataHelp.GetChapterVariable(BookUrl, Url, key);
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return Url?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is BookChapter other)
            {
                return other.Url == Url;
            }
            return false;
        }

        public bool Equals(BookChapter other)
        {
            return other != null && other.Url == Url;
        }

        public string PrimaryStr()
        {
            return BookUrl + Url;
        }

        public string GetDisplayTitle(List<ReplaceRule> replaceRules = null, bool useReplace = true, bool chineseConvert = true)
        {
            // 移除换行符
            var displayTitle = Title;//Regex.Replace(Title, AppPattern.RnRegex, "");

            //if (chineseConvert)
            //{
            //    switch (AppConfig.ChineseConverterType)
            //    {
            //        case 1:
            //            displayTitle = ChineseUtils.T2S(displayTitle); // 繁转简
            //            break;
            //        case 2:
            //            displayTitle = ChineseUtils.S2T(displayTitle); // 简转繁
            //            break;
            //    }
            //}

            if (useReplace && replaceRules != null)
            {
                foreach (var item in replaceRules)
                {
                    if (!string.IsNullOrEmpty(item.Pattern))
                    {
                        try
                        {
                            string mDisplayTitle;
                            if (item.IsRegex)
                            {
                                // C# Regex 也有超时设置
                                mDisplayTitle = Regex.Replace(
                                    displayTitle,
                                    item.Pattern,
                                    item.Replacement,
                                    RegexOptions.None,
                                    TimeSpan.FromMilliseconds(30000)
                                );
                            }
                            else
                            {
                                mDisplayTitle = displayTitle.Replace(item.Pattern, item.Replacement);
                            }

                            if (!string.IsNullOrWhiteSpace(mDisplayTitle))
                            {
                                displayTitle = mDisplayTitle;
                            }
                        }
                        catch (RegexMatchTimeoutException)
                        {
                            item.IsEnabled = false;
                        }
                        catch (OperationCanceledException)
                        {
                            return displayTitle;
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }
            return displayTitle;
        }

        public string GetAbsoluteURL()
        {
            // 二级目录解析的卷链接为空 返回目录页的链接
            if (Url.StartsWith(Title) && IsVolume) return BaseUrl;

            // 模拟 AnalyzeUrl.paramPattern
            var urlMatcher = AppPattern.PARAM_PATTERN.Match(Url);
            string urlBefore;

            if (urlMatcher.Success)
            {
                urlBefore = Url.Substring(0, urlMatcher.Index);
            }
            else
            {
                urlBefore = Url;
            }

            var urlAbsoluteBefore = BaseUrl + urlBefore;
            //var urlAbsoluteBefore = NetworkUtils.GetAbsoluteURL(BaseUrl, urlBefore);

            if (urlBefore.Length == Url.Length)
            {
                return urlAbsoluteBefore;
            }
            else
            {
                return $"{urlAbsoluteBefore},{Url.Substring(urlMatcher.Index + urlMatcher.Length)}"; // 注意：Kotlin的end()对应C#的Index+Length
            }
        }

        private void EnsureTitleMD5Init()
        {
            if (TitleMD5 == null)
            {
                //TitleMD5 = MD5Utils.Md5Encode16(Title);
            }
        }

        public string GetFileName(string suffix = "nb")
        {
            EnsureTitleMD5Init();
            return $"{Index:D5}-{TitleMD5}.{suffix}";
        }

        public string GetFontName()
        {
            EnsureTitleMD5Init();
            return $"{Index:D5}-{TitleMD5}.ttf";
        }

        public string getVariable()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}