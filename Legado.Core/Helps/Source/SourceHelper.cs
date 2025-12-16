using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Helps.Source
{
    /// <summary>
    /// 源帮助类
    /// 用于源的获取、删除、启用等操作
    /// </summary>
    public static class SourceHelper
    {
        // TODO: 实现18+域名列表
        // private static readonly HashSet<string> _list18Plus;

        /// <summary>
        /// 获取源（根据key自动判断类型）
        /// </summary>
        /// <param name="key">源标识（URL）</param>
        /// <returns>基础源对象</returns>
        public static IBaseSource GetSource(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            // TODO: 从缓存或当前阅读状态获取
            // if (ReadBook.BookSource?.BookSourceUrl == key)
            //     return ReadBook.BookSource;
            // if (AudioPlay.BookSource?.BookSourceUrl == key)
            //     return AudioPlay.BookSource;
            // if (ReadManga.BookSource?.BookSourceUrl == key)
            //     return ReadManga.BookSource;

            // TODO: 从数据库获取
            // return appDb.BookSourceDao.GetBookSource(key)
            //     ?? appDb.RssSourceDao.GetByKey(key);

            return null;
        }

        /// <summary>
        /// 获取源（指定类型）
        /// </summary>
        /// <param name="key">源标识（URL）</param>
        /// <param name="type">源类型（0=书源，1=RSS源）</param>
        /// <returns>基础源对象</returns>
        public static IBaseSource GetSource(string key, int type)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            // TODO: 从数据库获取
            // switch (type)
            // {
            //     case 0: // SourceType.Book
            //         return appDb.BookSourceDao.GetBookSource(key);
            //     case 1: // SourceType.Rss
            //         return appDb.RssSourceDao.GetByKey(key);
            //     default:
            //         return null;
            // }

            return null;
        }

        /// <summary>
        /// 删除源
        /// </summary>
        /// <param name="key">源标识</param>
        /// <param name="type">源类型（0=书源，1=RSS源）</param>
        public static void DeleteSource(string key, int type)
        {
            switch (type)
            {
                case 0: // SourceType.Book
                    DeleteBookSource(key);
                    break;
                case 1: // SourceType.Rss
                    DeleteRssSource(key);
                    break;
            }
        }

        /// <summary>
        /// 删除多个书源（BookSourcePart版本）
        /// </summary>
        public static void DeleteBookSourceParts(List<BookSourcePart> sources)
        {
            // TODO: 在事务中执行
            // appDb.RunInTransaction(() =>
            // {
            foreach (var source in sources)
            {
                DeleteBookSourceInternal(source.BookSourceUrl);
            }
            // });

            // TODO: 清除源变量缓存
            // AppCacheManager.ClearSourceVariables();
        }

        /// <summary>
        /// 删除多个书源
        /// </summary>
        public static void DeleteBookSources(List<BookSource> sources)
        {
            // TODO: 在事务中执行
            // appDb.RunInTransaction(() =>
            // {
            foreach (var source in sources)
            {
                DeleteBookSourceInternal(source.BookSourceUrl);
            }
            // });

            // TODO: 清除源变量缓存
            // AppCacheManager.ClearSourceVariables();
        }

        /// <summary>
        /// 删除书源（内部方法）
        /// </summary>
        private static void DeleteBookSourceInternal(string key)
        {
            // TODO: 从数据库删除
            // appDb.BookSourceDao.Delete(key);
            // appDb.CacheDao.DeleteSourceVariables(key);
            // SourceConfig.RemoveSource(key);
        }

        /// <summary>
        /// 删除单个书源
        /// </summary>
        public static void DeleteBookSource(string key)
        {
            DeleteBookSourceInternal(key);
            
            // TODO: 清除源变量缓存
            // AppCacheManager.ClearSourceVariables();
        }

        /// <summary>
        /// 删除多个RSS源
        /// </summary>
        public static void DeleteRssSources(List<RssSource> sources)
        {
            // TODO: 在事务中执行
            // appDb.RunInTransaction(() =>
            // {
            foreach (var source in sources)
            {
                DeleteRssSourceInternal(source.SourceUrl);
            }
            // });

            // TODO: 清除源变量缓存
            // AppCacheManager.ClearSourceVariables();
        }

        /// <summary>
        /// 删除RSS源（内部方法）
        /// </summary>
        private static void DeleteRssSourceInternal(string key)
        {
            // TODO: 从数据库删除
            // appDb.RssSourceDao.Delete(key);
            // appDb.RssArticleDao.Delete(key);
            // appDb.CacheDao.DeleteSourceVariables(key);
        }

        /// <summary>
        /// 删除单个RSS源
        /// </summary>
        public static void DeleteRssSource(string key)
        {
            DeleteRssSourceInternal(key);
            
            // TODO: 清除源变量缓存
            // AppCacheManager.ClearSourceVariables();
        }

        /// <summary>
        /// 启用/禁用源
        /// </summary>
        /// <param name="key">源标识</param>
        /// <param name="type">源类型（0=书源，1=RSS源）</param>
        /// <param name="enable">是否启用</param>
        public static void EnableSource(string key, int type, bool enable)
        {
            // TODO: 更新数据库
            // switch (type)
            // {
            //     case 0: // SourceType.Book
            //         appDb.BookSourceDao.Enable(key, enable);
            //         break;
            //     case 1: // SourceType.Rss
            //         appDb.RssSourceDao.Enable(key, enable);
            //         break;
            // }
        }

        /// <summary>
        /// 插入RSS源（过滤18+网址）
        /// </summary>
        public static void InsertRssSource(params RssSource[] rssSources)
        {
            var groups = rssSources.GroupBy(s => Is18Plus(s.SourceUrl)).ToDictionary(g => g.Key, g => g.ToList());

            // 18+源禁止导入
            if (groups.ContainsKey(true))
            {
                foreach (var source in groups[true])
                {
                    // TODO: 显示提示
                    // appCtx.ToastOnUi($"{source.SourceName}是18+网址,禁止导入.");
                }
            }

            // 非18+源可以导入
            if (groups.ContainsKey(false))
            {
                var validSources = groups[false].ToArray();
                // TODO: 插入数据库
                // appDb.RssSourceDao.Insert(validSources);
            }
        }

        /// <summary>
        /// 插入书源（过滤18+网址）
        /// </summary>
        public static void InsertBookSource(params BookSource[] bookSources)
        {
            var groups = bookSources.GroupBy(s => Is18Plus(s.BookSourceUrl)).ToDictionary(g => g.Key, g => g.ToList());

            // 18+源禁止导入
            if (groups.ContainsKey(true))
            {
                foreach (var source in groups[true])
                {
                    // TODO: 显示提示
                    // appCtx.ToastOnUi($"{source.BookSourceName}是18+网址,禁止导入.");
                }
            }

            // 非18+源可以导入
            if (groups.ContainsKey(false))
            {
                var validSources = groups[false].ToArray();
                // TODO: 插入数据库
                // appDb.BookSourceDao.Insert(validSources);
                
                // 异步调整排序序号
                Task.Run(() => AdjustSortNumber());
            }
        }

        /// <summary>
        /// 判断是否为18+网址
        /// </summary>
        private static bool Is18Plus(string url)
        {
            // TODO: 实现18+域名检查
            // if (_list18Plus == null || _list18Plus.Count == 0)
            //     return false;
            
            // if (string.IsNullOrEmpty(url))
            //     return false;

            // var baseUrl = NetworkUtils.GetBaseUrl(url);
            // if (string.IsNullOrEmpty(baseUrl))
            //     return false;

            // try
            // {
            //     var parts = baseUrl.Split(new[] { "//", "." }, StringSplitOptions.RemoveEmptyEntries);
            //     if (parts.Length > 2)
            //     {
            //         var host = $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}";
            //         return _list18Plus.Contains(host);
            //     }
            // }
            // catch { }

            return false;
        }

        /// <summary>
        /// 调整排序序号
        /// 当排序号超出范围或存在重复时，重新分配序号
        /// </summary>
        public static void AdjustSortNumber()
        {
            // TODO: 实现排序调整
            // if (appDb.BookSourceDao.MaxOrder > 99999 ||
            //     appDb.BookSourceDao.MinOrder < -99999 ||
            //     appDb.BookSourceDao.HasDuplicateOrder)
            // {
            //     var sources = appDb.BookSourceDao.AllPart;
            //     for (int i = 0; i < sources.Count; i++)
            //     {
            //         sources[i].CustomOrder = i;
            //     }
            //     appDb.BookSourceDao.UpOrder(sources);
            // }
        }
    }
}
