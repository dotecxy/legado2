using Legado.Core.Data.Entities;

namespace Legado.Core.Helps.Source
{
    /// <summary>
    /// 基础源扩展方法
    /// </summary>
    public static class BaseSourceExtensions
    {
        /// <summary>
        /// 获取共享JS作用域
        /// </summary>
        /// <param name="source">源对象</param>
        /// <returns>JS作用域对象</returns>
        public static object GetShareScope(this IBaseSource source)
        {
            // TODO: 实现共享JS作用域
            // return SharedJsScope.GetScope(source.JsLib);
            return null;
        }

        /// <summary>
        /// 获取源类型
        /// </summary>
        /// <param name="source">源对象</param>
        /// <returns>源类型（0=书源，1=RSS源）</returns>
        public static int GetSourceType(this IBaseSource source)
        {
            switch (source)
            {
                case BookSource _:
                    return 0; // SourceType.Book
                case RssSource _:
                    return 1; // SourceType.Rss
                default:
                    throw new System.InvalidOperationException($"Unknown source type: {source.GetType().Name}");
            }
        }
    }
}
