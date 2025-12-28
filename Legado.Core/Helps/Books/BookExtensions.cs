using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Legado.Core.Data.Entities;
using Legado.Core.Constants;
using Legado.Core.Utils;
using Newtonsoft.Json;
using Legado.Core.Data;
using System.Threading.Tasks;

namespace Legado.Core.Helps.Books
{
    /// <summary>
    /// Book扩展方法
    /// </summary>
    public static class BookExtensions
    {
        // 本地URI缓存
        private static readonly ConcurrentDictionary<string, string> _localUriCache = new ConcurrentDictionary<string, string>();

        #region 书籍类型判断

        /// <summary>
        /// 是否为有声书
        /// </summary>
        public static bool IsAudio(this Book book)
        {
            return book.IsType(BookType.Audio);
        }

        /// <summary>
        /// 是否为图片书籍
        /// </summary>
        public static bool IsImage(this Book book)
        {
            return book.IsType(BookType.Image);
        }

        /// <summary>
        /// 是否为本地书籍
        /// </summary>
        public static bool IsLocal(this Book book)
        {
            if (book.Type == 0)
            {
                return book.Origin == BookType.LocalTag || 
                       (book.Origin?.StartsWith(BookType.WebDavTag) ?? false);
            }
            return book.IsType(BookType.Local);
        }

        /// <summary>
        /// 是否为本地TXT文件
        /// </summary>
        public static bool IsLocalTxt(this Book book)
        {
            return book.IsLocal() && 
                   (book.OriginName?.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        /// <summary>
        /// 是否为EPUB文件
        /// </summary>
        public static bool IsEpub(this Book book)
        {
            return book.IsLocal() && 
                   (book.OriginName?.EndsWith(".epub", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        /// <summary>
        /// 是否为UMD文件
        /// </summary>
        public static bool IsUmd(this Book book)
        {
            return book.IsLocal() && 
                   (book.OriginName?.EndsWith(".umd", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        /// <summary>
        /// 是否为PDF文件
        /// </summary>
        public static bool IsPdf(this Book book)
        {
            return book.IsLocal() && 
                   (book.OriginName?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        /// <summary>
        /// 是否为Mobi文件
        /// </summary>
        public static bool IsMobi(this Book book)
        {
            if (!book.IsLocal()) return false;
            var name = book.OriginName;
            if (string.IsNullOrEmpty(name)) return false;
            return name.EndsWith(".mobi", StringComparison.OrdinalIgnoreCase) ||
                   name.EndsWith(".azw3", StringComparison.OrdinalIgnoreCase) ||
                   name.EndsWith(".azw", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 是否为在线TXT
        /// </summary>
        public static bool IsOnLineTxt(this Book book)
        {
            return !book.IsLocal() && book.IsType(BookType.Text);
        }

        /// <summary>
        /// 是否为Web文件
        /// </summary>
        public static bool IsWebFile(this Book book)
        {
            return book.IsType(BookType.WebFile);
        }

        /// <summary>
        /// 是否更新失败
        /// </summary>
        public static bool IsUpError(this Book book)
        {
            return book.IsType(BookType.UpdateError);
        }

        /// <summary>
        /// 是否为归档文件
        /// </summary>
        public static bool IsArchive(this Book book)
        {
            return book.IsType(BookType.Archive);
        }

        /// <summary>
        /// 是否不在书架
        /// </summary>
        public static bool IsNotShelf(this Book book)
        {
            return book.IsType(BookType.NotShelf);
        }

        /// <summary>
        /// 获取归档文件名
        /// </summary>
        public static string GetArchiveName(this Book book)
        {
            if (!book.IsArchive())
            {
                throw new InvalidOperationException("Book is not deCompressed from archive");
            }
            // local_book::archive.rar
            // webDav::https://...../archive.rar
            var origin = book.Origin ?? "";
            var afterSeparator = origin.Contains("::") 
                ? origin.Substring(origin.IndexOf("::") + 2) 
                : origin;
            var lastSlash = afterSeparator.LastIndexOf('/');
            return lastSlash >= 0 ? afterSeparator.Substring(lastSlash + 1) : afterSeparator;
        }

        #endregion

        #region 内容搜索

        /// <summary>
        /// 检查书籍是否包含指定关键词
        /// </summary>
        public static bool Contains(this Book book, string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return true;
            }
            return (book.Name?.Contains(word) ?? false) ||
                   (book.Author?.Contains(word) ?? false) ||
                   (book.OriginName?.Contains(word) ?? false) ||
                   (book.Origin?.Contains(word) ?? false) ||
                   (book.Kind?.Contains(word) ?? false) ||
                   (book.Intro?.Contains(word) ?? false);
        }

        #endregion

        #region URI缓存管理

        /// <summary>
        /// 获取本地URI
        /// </summary>
        public static string GetLocalUri(this Book book)
        {
            if (!book.IsLocal())
            {
                throw new InvalidOperationException("不是本地书籍");
            }

            if (_localUriCache.TryGetValue(book.BookUrl, out string cachedUri))
            {
                return cachedUri;
            }

            // TODO: 实现本地文件查找和Uri处理逻辑
            // 包括检查defaultBookDir、importBookDir等
            
            _localUriCache[book.BookUrl] = book.BookUrl;
            return book.BookUrl;
        }

        /// <summary>
        /// 获取归档文件URI
        /// </summary>
        public static string GetArchiveUri(this Book book)
        {
            // TODO: 实现归档文件URI获取逻辑
            return null;
        }

        /// <summary>
        /// 缓存本地URI
        /// </summary>
        public static void CacheLocalUri(this Book book, string uri)
        {
            _localUriCache[book.BookUrl] = uri;
        }

        /// <summary>
        /// 移除本地URI缓存
        /// </summary>
        public static void RemoveLocalUriCache(this Book book)
        {
            _localUriCache.TryRemove(book.BookUrl, out _);
        }

        /// <summary>
        /// 获取远程URL
        /// </summary>
        public static string GetRemoteUrl(this Book book)
        {
            if (book.Origin?.StartsWith(BookType.WebDavTag) ?? false)
            {
                return book.Origin.Substring(BookType.WebDavTag.Length);
            }
            return null;
        }

        #endregion

        #region 类型操作

        /// <summary>
        /// 设置书籍类型
        /// </summary>
        public static void SetType(this Book book, params int[] types)
        {
            book.Type = 0;
            book.AddType(types);
        }

        /// <summary>
        /// 添加书籍类型
        /// </summary>
        public static void AddType(this Book book, params int[] types)
        {
            foreach (var type in types)
            {
                book.Type |= type;
            }
        }

        /// <summary>
        /// 移除书籍类型
        /// </summary>
        public static void RemoveType(this Book book, params int[] types)
        {
            foreach (var type in types)
            {
                book.Type &= ~type;
            }
        }

        /// <summary>
        /// 移除所有书籍类型
        /// </summary>
        public static void RemoveAllBookType(this Book book)
        {
            book.RemoveType(BookType.AllBookType);
        }

        /// <summary>
        /// 清除类型
        /// </summary>
        public static void ClearType(this Book book)
        {
            book.Type = 0;
        }

        /// <summary>
        /// 检查是否为指定类型
        /// </summary>
        public static bool IsType(this Book book, int bookType)
        {
            return (book.Type & bookType) > 0;
        }

        /// <summary>
        /// 更新类型（从旧版本迁移）
        /// </summary>
        public static void UpType(this Book book)
        {
            if (book.Type < 8)
            {
                book.Type = book.Type switch
                {
                    BookSourceType.Image => BookType.Image,
                    BookSourceType.Audio => BookType.Audio,
                    BookSourceType.File => BookType.WebFile,
                    _ => BookType.Text
                };
                if (book.Origin == BookType.LocalTag || 
                    (book.Origin?.StartsWith(BookType.WebDavTag) ?? false))
                {
                    book.Type |= BookType.Local;
                }
            }
        }

        #endregion

        #region 数据同步

        /// <summary>
        /// 同步书籍信息
        /// </summary>
        public static void Sync(this Book book, Book oldBook)
        {
            // TODO: 实现从数据库获取当前书籍并同步
            // var curBook = appDb.BookDao.GetBook(oldBook.BookUrl);
            // book.DurChapterTime = curBook.DurChapterTime;
            // ...
        }

        /// <summary>
        /// 更新书籍到数据库
        /// </summary>
        public static void Update(this Book book)
        {
            // TODO: 实现数据库更新
            // appDb.BookDao.Update(book);
        }

        /// <summary>
        /// 获取主键字符串
        /// </summary>
        public static string PrimaryStr(this Book book)
        {
            return (book.Origin ?? "") + (book.BookUrl ?? "");
        }

        /// <summary>
        /// 更新到新书籍（合并信息）
        /// </summary>
        public static Book UpdateTo(this Book book, Book newBook)
        {
            newBook.DurChapterIndex = book.DurChapterIndex;
            newBook.DurChapterTitle = book.DurChapterTitle;
            newBook.DurChapterPos = book.DurChapterPos;
            newBook.DurChapterTime = book.DurChapterTime;
            newBook.Group = book.Group;
            newBook.Order = book.Order;
            // newBook.CustomCoverUrl = book.CustomCoverUrl; // TODO: 添加属性
            // newBook.CustomIntro = book.CustomIntro; // TODO: 添加属性
            // newBook.CustomTag = book.CustomTag; // TODO: 添加属性
            newBook.CanUpdate = book.CanUpdate;
            // newBook.ReadConfig = book.ReadConfig; // TODO: 添加属性

            // TODO: 实现variableMap合并逻辑
            
            return newBook;
        }

        /// <summary>
        /// 检查是否有指定变量
        /// </summary>
        public static bool HasVariable(this Book book, string key)
        {
            // TODO: 实现变量检查逻辑
            // return variableMap.ContainsKey(key) || RuleBigDataHelp.HasBookVariable(book.BookUrl, key);
            return false;
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 获取文件夹名（不使用缓存）
        /// </summary>
        public static string GetFolderNameNoCache(this Book book)
        {
            var name = book.Name ?? "";
            // 移除文件名不允许的字符
            name = Regex.Replace(name, AppPattern.FileNameRegex, "");
            var length = Math.Min(9, name.Length);
            // TODO: 实现MD5加密
            var md5Part = book.BookUrl?.GetHashCode().ToString("X8") ?? "00000000";
            return name.Substring(0, length) + md5Part;
        }

        /// <summary>
        /// 获取书源
        /// </summary>
        public static Task<BookSource> GetBookSourceAsync(this Book book,AppDatabase appDb)
        { 
            return appDb.BookSourceDao.GetAsync(book.Origin); 
        }

        /// <summary>
        /// 检查本地文件是否已修改
        /// </summary>
        public static bool IsLocalModified(this Book book)
        {
            // TODO: 实现本地文件修改检查
            // return book.IsLocal() && LocalBook.GetLastModified(book) > book.LatestChapterTime;
            return false;
        }

        /// <summary>
        /// 释放HTML数据
        /// </summary>
        public static void ReleaseHtmlData(this Book book)
        {
            book.InfoHtml = null;
            book.TocHtml = null;
        }

        /// <summary>
        /// 检查是否同名同作者
        /// </summary>
        public static bool IsSameNameAuthor(this Book book, object other)
        {
            if (other is IBaseBook baseBook)
            {
                return book.Name == baseBook.Name && book.Author == baseBook.Author;
            }
            return false;
        }

        /// <summary>
        /// 获取导出文件名
        /// </summary>
        public static string GetExportFileName(this Book book, string suffix)
        {
            // TODO: 实现JS规则处理
            // var jsStr = AppConfig.BookExportFileName;
            var author = book.GetRealAuthor();
            return $"{book.Name} 作者：{author}.{suffix}";
        }

        /// <summary>
        /// 获取分卷导出文件名
        /// </summary>
        public static string GetExportFileName(this Book book, string suffix, int epubIndex, string jsStr = null)
        {
            var author = book.GetRealAuthor();
            var defaultName = $"{book.Name} 作者：{author} [{epubIndex}].{suffix}";
            
            if (string.IsNullOrEmpty(jsStr))
            {
                return defaultName;
            }

            // TODO: 实现JS规则处理
            return defaultName;
        }

        /// <summary>
        /// 获取真实作者名
        /// </summary>
        public static string GetRealAuthor(this Book book)
        {
            // TODO: 实现作者名清理逻辑
            return book.Author ?? "";
        }

        #endregion

        #region 阅读模拟

        /// <summary>
        /// 计算模拟总章节数
        /// </summary>
        public static int SimulatedTotalChapterNum(this Book book)
        {
            // TODO: 实现阅读模拟逻辑
            // if (book.ReadSimulating())
            // {
            //     var currentDate = DateTime.Now.Date;
            //     var daysPassed = (currentDate - config.StartDate).Days + 1;
            //     var chaptersToUnlock = Math.Max(0, (config.StartChapter ?? 0) + (daysPassed * config.DailyChapters));
            //     return Math.Min(book.TotalChapterNum, chaptersToUnlock);
            // }
            return book.TotalChapterNum;
        }

        /// <summary>
        /// 是否正在模拟阅读
        /// </summary>
        public static bool ReadSimulating(this Book book)
        {
            // TODO: 实现阅读模拟状态检查
            // return config.ReadSimulating;
            return false;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 尝试解析导出文件名JS规则
        /// </summary>
        public static bool TryParseExportFileName(string jsStr)
        {
            // TODO: 实现JS规则验证
            // var bindings = new Dictionary<string, object>
            // {
            //     ["name"] = "name",
            //     ["author"] = "author",
            //     ["epubIndex"] = "epubIndex"
            // };
            // return RhinoScriptEngine.Eval(jsStr, bindings) != null;
            return true;
        }

        #endregion
    }

    /// <summary>
    /// 书籍类型常量
    /// </summary>
    public static class BookType
    {
        public const int Text = 0;
        public const int Local = 1;
        public const int Audio = 2;
        public const int Image = 4;
        public const int WebFile = 8;
        public const int UpdateError = 16;
        public const int Archive = 32;
        public const int NotShelf = 64;
        
        public const int AllBookType = Audio | Image | WebFile | Local;

        public const string LocalTag = "loc_book";
        public const string WebDavTag = "webDav::";
    }

    /// <summary>
    /// 书源类型常量
    /// </summary>
    public static class BookSourceType
    {
        public const int Default = 0;
        public const int Audio = 2;
        public const int Image = 4;
        public const int File = 8;
    }
}
