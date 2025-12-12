using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书籍数据访问接口（对应 Kotlin 的 BookDao.kt）
    /// </summary>
    public interface IBookDao
    {
        // ================= 查询方法 =================

        /// <summary>
        /// 根据分组ID获取书籍
        /// </summary>
        Task<List<Book>> GetByGroupAsync(long groupId);

        /// <summary>
        /// 获取所有书籍
        /// </summary>
        Task<List<Book>> GetAllAsync();

        /// <summary>
        /// 获取音频书籍
        /// </summary>
        Task<List<Book>> GetAudioAsync();

        /// <summary>
        /// 获取本地书籍
        /// </summary>
        Task<List<Book>> GetLocalAsync();

        /// <summary>
        /// 搜索书籍
        /// </summary>
        Task<List<Book>> SearchAsync(string key);

        /// <summary>
        /// 根据书名查找书籍
        /// </summary>
        Task<List<Book>> FindByNameAsync(params string[] names);

        /// <summary>
        /// 根据文件名获取书籍
        /// </summary>
        Task<Book> GetBookByFileNameAsync(string fileName);

        /// <summary>
        /// 根据URL获取书籍
        /// </summary>
        Task<Book> GetBookAsync(string bookUrl);

        /// <summary>
        /// 根据书名和作者获取书籍
        /// </summary>
        Task<Book> GetBookAsync(string name, string author);

        /// <summary>
        /// 获取所有网络书籍
        /// </summary>
        Task<List<Book>> GetWebBooksAsync();

        /// <summary>
        /// 获取最后阅读的书籍
        /// </summary>
        Task<Book> GetLastReadBookAsync();

        /// <summary>
        /// 获取所有书籍URL
        /// </summary>
        Task<List<string>> GetAllBookUrlsAsync();

        // ================= 统计方法 =================

        /// <summary>
        /// 获取书籍总数
        /// </summary>
        Task<int> GetCountAsync();

        /// <summary>
        /// 检查书籍是否存在
        /// </summary>
        Task<bool> HasAsync(string bookUrl);

        /// <summary>
        /// 检查书籍是否存在（根据书名和作者）
        /// </summary>
        Task<bool> HasAsync(string name, string author);

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        Task<bool> HasFileAsync(string fileName);

        /// <summary>
        /// 获取最小排序值
        /// </summary>
        Task<int> GetMinOrderAsync();

        /// <summary>
        /// 获取最大排序值
        /// </summary>
        Task<int> GetMaxOrderAsync();

        // ================= 增删改方法 =================

        /// <summary>
        /// 插入书籍
        /// </summary>
        Task InsertAsync(params Book[] books);

        /// <summary>
        /// 更新书籍
        /// </summary>
        Task UpdateAsync(params Book[] books);

        /// <summary>
        /// 删除书籍
        /// </summary>
        Task DeleteAsync(params Book[] books);

        /// <summary>
        /// 替换书籍
        /// </summary>
        Task ReplaceAsync(Book oldBook, Book newBook);

        /// <summary>
        /// 更新阅读进度
        /// </summary>
        Task UpdateProgressAsync(string bookUrl, int pos);

        /// <summary>
        /// 更新分组
        /// </summary>
        Task UpdateGroupAsync(long oldGroupId, long newGroupId);

        /// <summary>
        /// 移除分组
        /// </summary>
        Task RemoveGroupAsync(long group);

        /// <summary>
        /// 删除非书架书籍
        /// </summary>
        Task DeleteNotShelfBookAsync();

        // TODO: 实现 Flow 相关的实时数据流方法
        // TODO: 实现复杂的 SQL 查询方法（如根据类型查询等）
    }
}
