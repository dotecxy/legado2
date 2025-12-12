using Legado.Core.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书籍章节数据访问接口（对应 Kotlin 的 BookChapterDao.kt）
    /// </summary>
    public interface IBookChapterDao
    {
        Task<List<BookChapter>> GetChaptersAsync(string bookUrl);
        Task<BookChapter> GetChapterAsync(string bookUrl, int index);
        Task InsertAsync(params BookChapter[] chapters);
        Task UpdateAsync(params BookChapter[] chapters);
        Task DeleteAsync(string bookUrl);
        Task<int> GetCountAsync(string bookUrl);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 书籍分组数据访问接口（对应 Kotlin 的 BookGroupDao.kt）
    /// </summary>
    public interface IBookGroupDao
    {
        Task<List<BookGroup>> GetAllAsync();
        Task<BookGroup> GetByIdAsync(long groupId);
        Task InsertAsync(params BookGroup[] groups);
        Task UpdateAsync(params BookGroup[] groups);
        Task DeleteAsync(params BookGroup[] groups);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 书签数据访问接口（对应 Kotlin 的 BookmarkDao.kt）
    /// </summary>
    public interface IBookmarkDao
    {
        Task<List<Bookmark>> GetByBookAsync(string bookUrl);
        Task<Bookmark> GetByIndexAsync(string bookUrl, int chapterIndex);
        Task InsertAsync(params Bookmark[] bookmarks);
        Task UpdateAsync(params Bookmark[] bookmarks);
        Task DeleteAsync(params Bookmark[] bookmarks);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 缓存数据访问接口（对应 Kotlin 的 CacheDao.kt）
    /// </summary>
    public interface ICacheDao
    {
        Task<Cache> GetAsync(string key);
        Task InsertAsync(params Cache[] caches);
        Task UpdateAsync(params Cache[] caches);
        Task DeleteAsync(string key);
        Task DeleteExpiredAsync();
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 字典规则数据访问接口（对应 Kotlin 的 DictRuleDao.kt）
    /// </summary>
    public interface IDictRuleDao
    {
        Task<List<DictRule>> GetAllAsync();
        Task<DictRule> GetAsync(string name);
        Task InsertAsync(params DictRule[] dictRules);
        Task UpdateAsync(params DictRule[] dictRules);
        Task DeleteAsync(params DictRule[] dictRules);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// HTTP TTS 数据访问接口（对应 Kotlin 的 HttpTTSDao.kt）
    /// </summary>
    public interface IHttpTTSDao
    {
        Task<List<HttpTTS>> GetAllAsync();
        Task<HttpTTS> GetAsync(long id);
        Task InsertAsync(params HttpTTS[] httpTTSs);
        Task UpdateAsync(params HttpTTS[] httpTTSs);
        Task DeleteAsync(params HttpTTS[] httpTTSs);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 键盘辅助数据访问接口（对应 Kotlin 的 KeyboardAssistsDao.kt）
    /// </summary>
    public interface IKeyboardAssistDao
    {
        Task<List<KeyboardAssist>> GetByTypeAsync(string type);
        Task InsertAsync(params KeyboardAssist[] assists);
        Task UpdateAsync(params KeyboardAssist[] assists);
        Task DeleteAsync(params KeyboardAssist[] assists);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 阅读记录数据访问接口（对应 Kotlin 的 ReadRecordDao.kt）
    /// </summary>
    public interface IReadRecordDao
    {
        Task<List<ReadRecord>> GetAllAsync();
        Task<ReadRecord> GetAsync(string bookName);
        Task InsertAsync(params ReadRecord[] records);
        Task UpdateAsync(params ReadRecord[] records);
        Task DeleteAsync(params ReadRecord[] records);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 替换规则数据访问接口（对应 Kotlin 的 ReplaceRuleDao.kt）
    /// </summary>
    public interface IReplaceRuleDao
    {
        Task<List<ReplaceRule>> GetAllAsync();
        Task<List<ReplaceRule>> GetEnabledAsync();
        Task<ReplaceRule> GetAsync(long id);
        Task InsertAsync(params ReplaceRule[] rules);
        Task UpdateAsync(params ReplaceRule[] rules);
        Task DeleteAsync(params ReplaceRule[] rules);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// RSS 文章数据访问接口（对应 Kotlin 的 RssArticleDao.kt）
    /// </summary>
    public interface IRssArticleDao
    {
        Task<List<RssArticle>> GetByOriginAsync(string origin);
        Task<RssArticle> GetAsync(string origin, string link);
        Task InsertAsync(params RssArticle[] articles);
        Task UpdateAsync(params RssArticle[] articles);
        Task DeleteAsync(params RssArticle[] articles);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// RSS 阅读记录数据访问接口（对应 Kotlin 的 RssReadRecordDao.kt）
    /// </summary>
    public interface IRssReadRecordDao
    {
        Task<List<RssReadRecord>> GetAllAsync();
        Task<RssReadRecord> GetAsync(string record);
        Task InsertAsync(params RssReadRecord[] records);
        Task DeleteAsync(string record);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// RSS 源数据访问接口（对应 Kotlin 的 RssSourceDao.kt）
    /// </summary>
    public interface IRssSourceDao
    {
        Task<List<RssSource>> GetAllAsync();
        Task<List<RssSource>> GetEnabledAsync();
        Task<RssSource> GetAsync(string sourceUrl);
        Task InsertAsync(params RssSource[] sources);
        Task UpdateAsync(params RssSource[] sources);
        Task DeleteAsync(params RssSource[] sources);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// RSS 收藏数据访问接口（对应 Kotlin 的 RssStarDao.kt）
    /// </summary>
    public interface IRssStarDao
    {
        Task<List<RssStar>> GetAllAsync();
        Task<RssStar> GetAsync(string origin, string link);
        Task InsertAsync(params RssStar[] stars);
        Task DeleteAsync(params RssStar[] stars);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 规则订阅数据访问接口（对应 Kotlin 的 RuleSubDao.kt）
    /// </summary>
    public interface IRuleSubDao
    {
        Task<List<RuleSub>> GetAllAsync();
        Task<RuleSub> GetAsync(long id);
        Task InsertAsync(params RuleSub[] ruleSubs);
        Task UpdateAsync(params RuleSub[] ruleSubs);
        Task DeleteAsync(params RuleSub[] ruleSubs);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 搜索书籍数据访问接口（对应 Kotlin 的 SearchBookDao.kt）
    /// </summary>
    public interface ISearchBookDao
    {
        Task<List<SearchBook>> GetByNameAsync(string name);
        Task<SearchBook> GetAsync(string name, string author);
        Task InsertAsync(params SearchBook[] searchBooks);
        Task UpdateAsync(params SearchBook[] searchBooks);
        Task DeleteAsync(params SearchBook[] searchBooks);
        Task ClearAsync();
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 搜索关键词数据访问接口（对应 Kotlin 的 SearchKeywordDao.kt）
    /// </summary>
    public interface ISearchKeywordDao
    {
        Task<List<SearchKeyword>> GetAllAsync();
        Task<SearchKeyword> GetAsync(string word);
        Task InsertAsync(params SearchKeyword[] keywords);
        Task UpdateAsync(params SearchKeyword[] keywords);
        Task DeleteAsync(params SearchKeyword[] keywords);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// 服务器数据访问接口（对应 Kotlin 的 ServerDao.kt）
    /// </summary>
    public interface IServerDao
    {
        Task<List<Server>> GetAllAsync();
        Task<Server> GetAsync(long id);
        Task InsertAsync(params Server[] servers);
        Task UpdateAsync(params Server[] servers);
        Task DeleteAsync(params Server[] servers);
        // TODO: 实现更多查询方法
    }

    /// <summary>
    /// TXT 目录规则数据访问接口（对应 Kotlin 的 TxtTocRuleDao.kt）
    /// </summary>
    public interface ITxtTocRuleDao
    {
        Task<List<TxtTocRule>> GetAllAsync();
        Task<TxtTocRule> GetAsync(long id);
        Task InsertAsync(params TxtTocRule[] rules);
        Task UpdateAsync(params TxtTocRule[] rules);
        Task DeleteAsync(params TxtTocRule[] rules);
        // TODO: 实现更多查询方法
    }
}
