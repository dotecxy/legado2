using Legado.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍基础接口（对应 BaseBook.kt）
    /// </summary>
    public interface IBaseBook : IRuleData
    {
        string Name { get; set; }
        string Author { get; set; }
        string BookUrl { get; set; }
        string Kind { get; set; }
        string WordCount { get; set; }
        string Variable { get; set; }

        [JsonIgnore]
        string InfoHtml { get; set; }

        [JsonIgnore]
        string TocHtml { get; set; }
    }

    /// <summary>
    /// IBaseBook 接口的扩展方法（对应 Kotlin 的默认实现）
    /// </summary>
    public static class BaseBookExtensions
    {
        /// <summary>
        /// 设置自定义变量（对应 Kotlin 的 putCustomVariable）
        /// </summary>
        public static void PutCustomVariable(this IBaseBook book, string value)
        {
            book.PutVariable("custom", value);
        }

        /// <summary>
        /// 获取自定义变量（对应 Kotlin 的 getCustomVariable）
        /// </summary>
        public static string GetCustomVariable(this IBaseBook book)
        {
            return book.GetVariable("custom");
        }

        /// <summary>
        /// 获取分类列表（对应 Kotlin 的 getKindList）
        /// </summary>
        public static List<string> GetKindList(this IBaseBook book)
        {
            var kindList = new List<string>();

            if (!string.IsNullOrWhiteSpace(book.WordCount))
            {
                kindList.Add(book.WordCount);
            }

            if (!string.IsNullOrWhiteSpace(book.Kind))
            {
                var kinds = book.Kind.Split(new[] { ',', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim())
                    .Where(k => !string.IsNullOrWhiteSpace(k));
                kindList.AddRange(kinds);
            }

            return kindList;
        }
    }
}
