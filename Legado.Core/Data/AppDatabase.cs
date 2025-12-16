using Legado.Core.Data.Dao;
using Legado.Core.Data.Entities;
using SQLite;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Legado.Core.Data
{
    /// <summary>
    /// 应用数据库管理器
    /// 负责数据库初始化、迁移和DAO访问
    /// 支持通过IDbContext访问数据库（兼容Dapper）
    /// </summary>
    public class AppDatabase : IDbContext
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        public const string DatabaseName = "legado.db";

        /// <summary>
        /// 书籍表名
        /// </summary>
        public const string BookTableName = "books";

        /// <summary>
        /// 书源表名
        /// </summary>
        public const string BookSourceTableName = "book_sources";

        /// <summary>
        /// RSS源表名
        /// </summary>
        public const string RssSourceTableName = "rssSources";

        private readonly IDbConnection _dbConnection;
        private readonly SQLiteAsyncConnectionWrapper _asyncConnection;
        private readonly string _databasePath;
        private readonly IServiceProvider _serviceProvider;
        private bool _initialized = false;

        // 实现IDbContext接口
        public IDbConnection DbConnection => _dbConnection;
        public string ConnectionString { get; }
        public IServiceProvider ServiceProvider => _serviceProvider;

        // DAO属性
        private BookDao _bookDao;
        private BookGroupDao _bookGroupDao;
        private BookSourceDao _bookSourceDao;
        private BookChapterDao _bookChapterDao;
        private ReplaceRuleDao _replaceRuleDao;
        private SearchBookDao _searchBookDao;
        private SearchKeywordDao _searchKeywordDao;
        private RssSourceDao _rssSourceDao;
        private BookmarkDao _bookmarkDao;
        private RssArticleDao _rssArticleDao;
        private RssStarDao _rssStarDao;
        private RssReadRecordDao _rssReadRecordDao;
        private CookieDao _cookieDao;
        private TxtTocRuleDao _txtTocRuleDao;
        private ReadRecordDao _readRecordDao;
        private HttpTTSDao _httpTTSDao;
        private CacheDao _cacheDao;
        private RuleSubDao _ruleSubDao;
        private DictRuleDao _dictRuleDao;
        private KeyboardAssistsDao _keyboardAssistsDao;
        private ServerDao _serverDao;

        /// <summary>
        /// 书籍DAO
        /// </summary>
        public BookDao BookDao => _bookDao ??= new BookDao(ServiceProvider);

        /// <summary>
        /// 书籍分组DAO
        /// </summary>
        public BookGroupDao BookGroupDao => _bookGroupDao ??= new BookGroupDao(ServiceProvider);

        /// <summary>
        /// 书源DAO
        /// </summary>
        public BookSourceDao BookSourceDao => _bookSourceDao ??= new BookSourceDao(ServiceProvider);

        /// <summary>
        /// 书籍章节DAO
        /// </summary>
        public BookChapterDao BookChapterDao => _bookChapterDao ??= new BookChapterDao(ServiceProvider);

        /// <summary>
        /// 替换规则DAO
        /// </summary>
        public ReplaceRuleDao ReplaceRuleDao => _replaceRuleDao ??= new ReplaceRuleDao(ServiceProvider);

        /// <summary>
        /// 搜索书籍DAO
        /// </summary>
        public SearchBookDao SearchBookDao => _searchBookDao ??= new SearchBookDao(ServiceProvider);

        /// <summary>
        /// 搜索关键词DAO
        /// </summary>
        public SearchKeywordDao SearchKeywordDao => _searchKeywordDao ??= new SearchKeywordDao(ServiceProvider);

        /// <summary>
        /// RSS源DAO
        /// </summary>
        public RssSourceDao RssSourceDao => _rssSourceDao ??= new RssSourceDao(ServiceProvider);

        /// <summary>
        /// 书签DAO
        /// </summary>
        public BookmarkDao BookmarkDao => _bookmarkDao ??= new BookmarkDao(ServiceProvider);

        /// <summary>
        /// RSS文章DAO
        /// </summary>
        public RssArticleDao RssArticleDao => _rssArticleDao ??= new RssArticleDao(ServiceProvider);

        /// <summary>
        /// RSS收藏DAO
        /// </summary>
        public RssStarDao RssStarDao => _rssStarDao ??= new RssStarDao(ServiceProvider);

        /// <summary>
        /// RSS阅读记录DAO
        /// </summary>
        public RssReadRecordDao RssReadRecordDao => _rssReadRecordDao ??= new RssReadRecordDao(ServiceProvider);

        /// <summary>
        /// Cookie DAO
        /// </summary>
        public CookieDao CookieDao => _cookieDao ??= new CookieDao(ServiceProvider);

        /// <summary>
        /// TXT目录规则DAO
        /// </summary>
        public TxtTocRuleDao TxtTocRuleDao => _txtTocRuleDao ??= new TxtTocRuleDao(ServiceProvider);

        /// <summary>
        /// 阅读记录DAO
        /// </summary>
        public ReadRecordDao ReadRecordDao => _readRecordDao ??= new ReadRecordDao(ServiceProvider);

        /// <summary>
        /// HTTP TTS DAO
        /// </summary>
        public HttpTTSDao HttpTTSDao => _httpTTSDao ??= new HttpTTSDao(ServiceProvider);

        /// <summary>
        /// 缓存DAO
        /// </summary>
        public CacheDao CacheDao => _cacheDao ??= new CacheDao(ServiceProvider);

        /// <summary>
        /// 规则订阅DAO
        /// </summary>
        public RuleSubDao RuleSubDao => _ruleSubDao ??= new RuleSubDao(ServiceProvider);

        /// <summary>
        /// 字典规则DAO
        /// </summary>
        public DictRuleDao DictRuleDao => _dictRuleDao ??= new DictRuleDao(ServiceProvider);

        /// <summary>
        /// 键盘辅助DAO
        /// </summary>
        public KeyboardAssistsDao KeyboardAssistsDao => _keyboardAssistsDao ??= new KeyboardAssistsDao(ServiceProvider);

        /// <summary>
        /// 服务器DAO
        /// </summary>
        public ServerDao ServerDao => _serverDao ??= new ServerDao(ServiceProvider);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databasePath">数据库文件路径</param>
        /// <param name="serviceProvider">服务提供者（可选）</param>
        public AppDatabase(string databasePath, IServiceProvider serviceProvider = null)
        {
            _databasePath = databasePath;
            ConnectionString = $"Data Source={databasePath}";
            _serviceProvider = serviceProvider;
            // 初始化 SQLite-net-pcl 连接
            _asyncConnection = new SQLiteAsyncConnectionWrapper(databasePath);
            _dbConnection = null;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            // 检查数据库版本
            var currentVersion = await GetDatabaseVersionAsync();

            // 执行数据库迁移
            if (currentVersion < DatabaseMigrations.CurrentVersion)
            {
                await DatabaseMigrations.MigrateAsync(_asyncConnection, currentVersion, DatabaseMigrations.CurrentVersion);
                await SetDatabaseVersionAsync(DatabaseMigrations.CurrentVersion);
            }

            // 创建表
            await CreateTablesAsync();

            // 数据库回调（首次创建或每次打开）
            if (currentVersion == 0)
            {
                await OnCreateAsync();
            }

            await OnOpenAsync();

            _initialized = true;
        }

        /// <summary>
        /// 创建所有表
        /// </summary>
        private async Task CreateTablesAsync()
        {
            // 使用Dapper执行创建表的SQL
            // TODO: 根据实体类自动生成CREATE TABLE语句
            // 或者使用SQL脚本文件
            await Task.CompletedTask;
        }

        /// <summary>
        /// 数据库首次创建回调
        /// </summary>
        private async Task OnCreateAsync()
        {
            // TODO: 可以在这里设置数据库区域设置等
            await Task.CompletedTask;
        }

        /// <summary>
        /// 数据库打开回调
        /// </summary>
        private async Task OnOpenAsync()
        {
            // 打开连接
            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            // 插入默认书籍分组
            await InsertDefaultBookGroupsAsync();

            // 更新书源登录UI字段（清除无效的"null"字符串）
            await _asyncConnection.ExecuteAsync("UPDATE book_sources SET loginUi = null WHERE loginUi = 'null'");
            await _asyncConnection.ExecuteAsync("UPDATE rssSources SET loginUi = null WHERE loginUi = 'null'");
            await _asyncConnection.ExecuteAsync("UPDATE httpTTS SET loginUi = null WHERE loginUi = 'null'");
            await _asyncConnection.ExecuteAsync("UPDATE httpTTS SET concurrentRate = '0' WHERE concurrentRate IS NULL");

            // 插入默认键盘辅助数据
            await InsertDefaultKeyboardAssistsAsync();
        }

        /// <summary>
        /// 插入默认书籍分组
        /// </summary>
        private async Task InsertDefaultBookGroupsAsync()
        {
            // 全部分组
            await _asyncConnection.ExecuteAsync(@"
                INSERT OR IGNORE INTO book_groups(groupId, groupName, `order`, show) 
                VALUES (-100, '全部', -10, 1)");

            // 本地分组
            await _asyncConnection.ExecuteAsync(@"
                INSERT OR IGNORE INTO book_groups(groupId, groupName, `order`, enableRefresh, show) 
                VALUES (-101, '本地', -9, 0, 1)");

            // 音频分组
            await _asyncConnection.ExecuteAsync(@"
                INSERT OR IGNORE INTO book_groups(groupId, groupName, `order`, show) 
                VALUES (-102, '音频', -8, 1)");

            // 网络未分组
            await _asyncConnection.ExecuteAsync(@"
                INSERT OR IGNORE INTO book_groups(groupId, groupName, `order`, show) 
                VALUES (-103, '网络未分组', -7, 1)");

            // 本地未分组
            await _asyncConnection.ExecuteAsync(@"
                INSERT OR IGNORE INTO book_groups(groupId, groupName, `order`, show) 
                VALUES (-104, '本地未分组', -6, 0)");

            // 更新失败分组
            await _asyncConnection.ExecuteAsync(@"
                INSERT OR IGNORE INTO book_groups(groupId, groupName, `order`, show) 
                VALUES (-105, '更新失败', -1, 1)");
        }

        /// <summary>
        /// 插入默认键盘辅助数据
        /// </summary>
        private async Task InsertDefaultKeyboardAssistsAsync()
        {
            var count = await _asyncConnection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM keyboardAssists");

            if (count == 0)
            {
                // TODO: 从DefaultData加载默认键盘辅助数据
                // 这里需要实现DefaultData.KeyboardAssists的加载逻辑
            }
        }

        /// <summary>
        /// 获取数据库版本
        /// </summary>
        private async Task<int> GetDatabaseVersionAsync()
        {
            try
            {
                var result = await _asyncConnection.ExecuteScalarAsync<int>("PRAGMA user_version");
                return result;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 设置数据库版本
        /// </summary>
        private async Task SetDatabaseVersionAsync(int version)
        {
            await _asyncConnection.ExecuteAsync($"PRAGMA user_version = {version}");
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void Dispose()
        {
            if (_dbConnection != null)
            {
                if (_dbConnection.State == ConnectionState.Open)
                    _dbConnection.Close();
                _dbConnection.Dispose();
            }
        }

        /// <summary>
        /// 创建数据库实例（单例模式）
        /// </summary>
        private static AppDatabase _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取数据库实例
        /// </summary>
        /// <param name="databasePath">数据库文件路径（可选，首次调用时必须提供）</param>
        public static AppDatabase GetInstance(string databasePath = null)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        if (string.IsNullOrEmpty(databasePath))
                        {
                            // 使用默认路径
                            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            databasePath = Path.Combine(appDataPath, "Legado", DatabaseName);

                            // 确保目录存在
                            var directory = Path.GetDirectoryName(databasePath);
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }
                        }

                        _instance = new AppDatabase(databasePath);
                    }
                }
            }

            return _instance;
        }
    }
}
