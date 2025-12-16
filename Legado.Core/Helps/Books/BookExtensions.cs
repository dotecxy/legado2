using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Legado.Core.Helps.Books
{
    /// <summary>
    /// Book 扩展方法类（对应 BookExtensions.kt）
    /// 提供书籍相关的扩展功能
    /// </summary>
    public static class BookExtensions
    {
        /// <summary>
        /// 是否为音频书籍
        /// </summary>
        public static bool IsAudio(this Book book)
        {
            return book != null && book.IsType(BookType.Audio);
        }

        /// <summary>
        /// 是否为图片书籍
        /// </summary>
        public static bool IsImage(this Book book)
        {
            return book != null && book.IsType(BookType.Image);
        }

        /// <summary>
        /// 是否为本地书籍
        /// </summary>
        public static bool IsLocal(this Book book)
        {
            if (book == null) return false;

            if (book.Type == 0)
            {
                return book.Origin == BookType.LocalTag || 
                       book.Origin.StartsWith(BookType.WebDavTag);
            }
            return book.IsType(BookType.Local);
        }

        /// <summary>
        /// 是否为网络书籍
        /// </summary>
        public static bool IsWeb(this Book book)
        {
            if (book == null) return false;

            if (book.Type == 0)
            {
                return book.Origin != BookType.LocalTag && 
                       !book.Origin.StartsWith(BookType.WebDavTag);
            }
            return !book.IsType(BookType.Local);
        }

        /// <summary>
        /// 是否为指定类型
        /// </summary>
        public static bool IsType(this Book book, int type)
        {
            return book != null && (book.Type & type) == type;
        }

        /// <summary>
        /// 获取真实作者名（去除"作者："等前缀）
        /// </summary>
        public static string GetRealAuthor(this Book book)
        {
            if (book == null || string.IsNullOrWhiteSpace(book.Author))
                return string.Empty;

            return Regex.Replace(book.Author, @"作者[：:]\s*", "").Trim();
        }

        /// <summary>
        /// 获取主键字符串
        /// </summary>
        public static string PrimaryStr(this Book book)
        {
            if (book == null) return string.Empty;
            return book.Origin + book.BookUrl;
        }

        /// <summary>
        /// 获取文件夹名称（无缓存）
        /// </summary>
        public static string GetFolderNameNoCache(this Book book)
        {
            if (book == null) return string.Empty;

            var name = Regex.Replace(book.Name, @"[\\/:*?""<>|]", "");
            var length = Math.Min(9, name.Length);
            var shortName = name.Substring(0, length);
            
            // TODO: 实现 MD5 编码
            var md5Hash = book.BookUrl.GetHashCode().ToString("X8");
            return shortName + md5Hash;
        }

        /// <summary>
        /// 获取导出文件名
        /// </summary>
        public static string GetExportFileName(this Book book, string suffix)
        {
            if (book == null) return $"unknown.{suffix}";

            var author = book.GetRealAuthor();
            return $"{book.Name} 作者：{author}.{suffix}";
        }

        /// <summary>
        /// 获取分割文件后的文件名
        /// </summary>
        public static string GetExportFileName(this Book book, string suffix, int epubIndex)
        {
            if (book == null) return $"unknown_{epubIndex}.{suffix}";

            var author = book.GetRealAuthor();
            return $"{book.Name} 作者：{author} [{epubIndex}].{suffix}";
        }

        /// <summary>
        /// 是否相同书名作者
        /// </summary>
        public static bool IsSameNameAuthor(this Book book, object other)
        {
            if (book == null || other == null) return false;

            if (other is Book otherBook)
            {
                return book.Name == otherBook.Name && book.Author == otherBook.Author;
            }

            return false;
        }
    }

    /// <summary>
    /// 书籍类型常量（对应 BookType.kt）
    /// </summary>
    public static class BookType
    {
        public const int Local = 1;      // 本地
        public const int Audio = 2;      // 音频
        public const int Image = 4;      // 图片
        public const int File = 8;       // 文件

        public const string LocalTag = "local";
        public const string WebDavTag = "webDav";
    }
}
