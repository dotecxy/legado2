using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Legado.Core.Data
{
    /// <summary>
    /// 数据库迁移管理器
    /// 负责处理数据库版本升级和结构变更
    /// 使用SQLite-net-pcl进行数据库操作
    /// </summary>
    public static class DatabaseMigrations
    {
        /// <summary>
        /// 当前数据库版本号
        /// </summary>
        public const int CurrentVersion = 75;

        /// <summary>
        /// 执行数据库迁移
        /// </summary>
        /// <param name="connection">异步连接</param>
        /// <param name="oldVersion">旧版本号</param>
        /// <param name="newVersion">新版本号</param>
        public static async Task MigrateAsync(SQLiteAsyncConnectionWrapper connection, int oldVersion, int newVersion)
        {
            if (oldVersion >= newVersion)
                return;
            
            // 如果版本差距太大，使用破坏性迁移（重建数据库）
            if (oldVersion < 10)
            {
                await DestructiveMigrationAsync(connection);
                return;
            }

            // 执行渐进式迁移
            for (int version = oldVersion + 1; version <= newVersion; version++)
            {
                await MigrateToVersionAsync(connection, version);
            }
        }

        /// <summary>
        /// 迁移到指定版本
        /// </summary>
        private static async Task MigrateToVersionAsync(SQLiteAsyncConnectionWrapper connection, int version)
        {
            switch (version)
            {
                case 11:
                    await Migration_10_11(connection);
                    break;
                case 12:
                    await Migration_11_12(connection);
                    break;
                case 13:
                    await Migration_12_13(connection);
                    break;
                case 14:
                    await Migration_13_14(connection);
                    break;
                case 15:
                    await Migration_14_15(connection);
                    break;
                case 17:
                    await Migration_15_17(connection);
                    break;
                case 18:
                    await Migration_17_18(connection);
                    break;
                case 19:
                    await Migration_18_19(connection);
                    break;
                case 20:
                    await Migration_19_20(connection);
                    break;
                case 21:
                    await Migration_20_21(connection);
                    break;
                case 22:
                    await Migration_21_22(connection);
                    break;
                case 23:
                    await Migration_22_23(connection);
                    break;
                case 24:
                    await Migration_23_24(connection);
                    break;
                case 25:
                    await Migration_24_25(connection);
                    break;
                case 26:
                    await Migration_25_26(connection);
                    break;
                case 27:
                    await Migration_26_27(connection);
                    break;
                case 28:
                    await Migration_27_28(connection);
                    break;
                case 29:
                    await Migration_28_29(connection);
                    break;
                case 30:
                    await Migration_29_30(connection);
                    break;
                case 31:
                    await Migration_30_31(connection);
                    break;
                case 32:
                    await Migration_31_32(connection);
                    break;
                case 33:
                    await Migration_32_33(connection);
                    break;
                case 34:
                    await Migration_33_34(connection);
                    break;
                case 35:
                    await Migration_34_35(connection);
                    break;
                case 36:
                    await Migration_35_36(connection);
                    break;
                case 37:
                    await Migration_36_37(connection);
                    break;
                case 38:
                    await Migration_37_38(connection);
                    break;
                case 39:
                    await Migration_38_39(connection);
                    break;
                case 40:
                    await Migration_39_40(connection);
                    break;
                case 41:
                    await Migration_40_41(connection);
                    break;
                case 42:
                    await Migration_41_42(connection);
                    break;
                case 43:
                    await Migration_42_43(connection);
                    break;
                case 55:
                    await Migration_54_55(connection);
                    break;
                default:
                    // 版本43-75之间的自动迁移在SQLite-net-pcl中通过表结构自动处理
                    break;
            }
        }

        #region 版本10-11: 重建TXT目录规则表
        private static async Task Migration_10_11(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("DROP TABLE IF EXISTS txtTocRules");
            await connection.ExecuteAsync(@"
                CREATE TABLE txtTocRules(
                    id INTEGER NOT NULL, 
                    name TEXT NOT NULL, 
                    rule TEXT NOT NULL, 
                    serialNumber INTEGER NOT NULL, 
                    enable INTEGER NOT NULL, 
                    PRIMARY KEY (id)
                )");
        }
        #endregion

        #region 版本11-12: 添加RSS源样式字段
        private static async Task Migration_11_12(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE rssSources ADD COLUMN style TEXT");
        }
        #endregion

        #region 版本12-13: 添加RSS源文章样式字段
        private static async Task Migration_12_13(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE rssSources ADD COLUMN articleStyle INTEGER NOT NULL DEFAULT 0");
        }
        #endregion

        #region 版本13-14: 重建书籍表并添加索引
        private static async Task Migration_13_14(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS books_new (
                    bookUrl TEXT NOT NULL, 
                    tocUrl TEXT NOT NULL, 
                    origin TEXT NOT NULL,
                    originName TEXT NOT NULL, 
                    name TEXT NOT NULL, 
                    author TEXT NOT NULL, 
                    kind TEXT, 
                    customTag TEXT, 
                    coverUrl TEXT, 
                    customCoverUrl TEXT, 
                    intro TEXT, 
                    customIntro TEXT, 
                    charset TEXT, 
                    type INTEGER NOT NULL, 
                    `group` INTEGER NOT NULL, 
                    latestChapterTitle TEXT, 
                    latestChapterTime INTEGER NOT NULL, 
                    lastCheckTime INTEGER NOT NULL, 
                    lastCheckCount INTEGER NOT NULL, 
                    totalChapterNum INTEGER NOT NULL, 
                    durChapterTitle TEXT, 
                    durChapterIndex INTEGER NOT NULL, 
                    durChapterPos INTEGER NOT NULL, 
                    durChapterTime INTEGER NOT NULL, 
                    wordCount TEXT, 
                    canUpdate INTEGER NOT NULL, 
                    `order` INTEGER NOT NULL, 
                    originOrder INTEGER NOT NULL, 
                    useReplaceRule INTEGER NOT NULL, 
                    variable TEXT, 
                    PRIMARY KEY(bookUrl)
                )");
            await connection.ExecuteAsync("INSERT INTO books_new SELECT * FROM books");
            await connection.ExecuteAsync("DROP TABLE books");
            await connection.ExecuteAsync("ALTER TABLE books_new RENAME TO books");
            await connection.ExecuteAsync("CREATE UNIQUE INDEX IF NOT EXISTS index_books_name_author ON books (name, author)");
        }
        #endregion

        #region 版本14-15: 添加书签作者字段
        private static async Task Migration_14_15(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE bookmarks ADD COLUMN bookAuthor TEXT NOT NULL DEFAULT ''");
        }
        #endregion

        #region 版本15-17: 创建阅读记录表
        private static async Task Migration_15_17(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS readRecord (
                    bookName TEXT NOT NULL, 
                    readTime INTEGER NOT NULL, 
                    PRIMARY KEY(bookName)
                )");
        }
        #endregion

        #region 版本17-18: 创建HTTP TTS表
        private static async Task Migration_17_18(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS httpTTS (
                    id INTEGER NOT NULL, 
                    name TEXT NOT NULL, 
                    url TEXT NOT NULL, 
                    PRIMARY KEY(id)
                )");
        }
        #endregion

        #region 版本18-19: 重建阅读记录表添加设备ID
        private static async Task Migration_18_19(SQLiteAsyncConnectionWrapper connection)
        {
            // TODO: 需要实现获取设备ID的逻辑
            var androidId = "default_device";
            
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS readRecordNew (
                    androidId TEXT NOT NULL, 
                    bookName TEXT NOT NULL, 
                    readTime INTEGER NOT NULL, 
                    PRIMARY KEY(androidId, bookName)
                )");
            await connection.ExecuteAsync($@"
                INSERT INTO readRecordNew(androidId, bookName, readTime) 
                SELECT '{androidId}' as androidId, bookName, readTime FROM readRecord");
            await connection.ExecuteAsync("DROP TABLE readRecord");
            await connection.ExecuteAsync("ALTER TABLE readRecordNew RENAME TO readRecord");
        }
        #endregion

        #region 版本19-20: 添加书源备注字段
        private static async Task Migration_19_20(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE book_sources ADD COLUMN bookSourceComment TEXT");
        }
        #endregion

        #region 版本20-21: 添加书籍分组显示字段
        private static async Task Migration_20_21(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE book_groups ADD COLUMN show INTEGER NOT NULL DEFAULT 1");
        }
        #endregion

        #region 版本21-22: 添加书籍阅读配置字段
        private static async Task Migration_21_22(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS books_new (
                    bookUrl TEXT NOT NULL, 
                    tocUrl TEXT NOT NULL, 
                    origin TEXT NOT NULL, 
                    originName TEXT NOT NULL, 
                    name TEXT NOT NULL, 
                    author TEXT NOT NULL, 
                    kind TEXT, 
                    customTag TEXT, 
                    coverUrl TEXT, 
                    customCoverUrl TEXT, 
                    intro TEXT, 
                    customIntro TEXT, 
                    charset TEXT, 
                    type INTEGER NOT NULL, 
                    `group` INTEGER NOT NULL, 
                    latestChapterTitle TEXT, 
                    latestChapterTime INTEGER NOT NULL, 
                    lastCheckTime INTEGER NOT NULL, 
                    lastCheckCount INTEGER NOT NULL, 
                    totalChapterNum INTEGER NOT NULL, 
                    durChapterTitle TEXT, 
                    durChapterIndex INTEGER NOT NULL, 
                    durChapterPos INTEGER NOT NULL, 
                    durChapterTime INTEGER NOT NULL, 
                    wordCount TEXT, 
                    canUpdate INTEGER NOT NULL, 
                    `order` INTEGER NOT NULL, 
                    originOrder INTEGER NOT NULL, 
                    variable TEXT, 
                    readConfig TEXT, 
                    PRIMARY KEY(bookUrl)
                )");
            await connection.ExecuteAsync(@"
                INSERT INTO books_new 
                SELECT bookUrl, tocUrl, origin, originName, name, author, kind, customTag, coverUrl, 
                    customCoverUrl, intro, customIntro, charset, type, `group`, latestChapterTitle, latestChapterTime, lastCheckTime, 
                    lastCheckCount, totalChapterNum, durChapterTitle, durChapterIndex, durChapterPos, durChapterTime, wordCount, canUpdate, 
                    `order`, originOrder, variable, null
                FROM books");
            await connection.ExecuteAsync("DROP TABLE books");
            await connection.ExecuteAsync("ALTER TABLE books_new RENAME TO books");
            await connection.ExecuteAsync("CREATE UNIQUE INDEX IF NOT EXISTS index_books_name_author ON books (name, author)");
        }
        #endregion

        #region 版本22-23: 添加章节基础URL字段
        private static async Task Migration_22_23(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE chapters ADD COLUMN baseUrl TEXT NOT NULL DEFAULT ''");
        }
        #endregion

        #region 版本23-24: 创建缓存表
        private static async Task Migration_23_24(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS caches (
                    key TEXT NOT NULL, 
                    value TEXT, 
                    deadline INTEGER NOT NULL, 
                    PRIMARY KEY(key)
                )");
            await connection.ExecuteAsync("CREATE UNIQUE INDEX IF NOT EXISTS index_caches_key ON caches (key)");
        }
        #endregion

        #region 版本24-25: 创建源订阅表
        private static async Task Migration_24_25(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS sourceSubs (
                    id INTEGER NOT NULL, 
                    name TEXT NOT NULL, 
                    url TEXT NOT NULL, 
                    type INTEGER NOT NULL, 
                    customOrder INTEGER NOT NULL, 
                    PRIMARY KEY(id)
                )");
        }
        #endregion

        #region 版本25-26: 重建规则订阅表
        private static async Task Migration_25_26(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS ruleSubs (
                    id INTEGER NOT NULL, 
                    name TEXT NOT NULL, 
                    url TEXT NOT NULL, 
                    type INTEGER NOT NULL, 
                    customOrder INTEGER NOT NULL, 
                    autoUpdate INTEGER NOT NULL, 
                    `update` INTEGER NOT NULL, 
                    PRIMARY KEY(id)
                )");
            await connection.ExecuteAsync("INSERT INTO ruleSubs SELECT *, 0, 0 FROM sourceSubs");
            await connection.ExecuteAsync("DROP TABLE sourceSubs");
        }
        #endregion

        #region 版本26-27: 添加RSS单URL和重建书签表
        private static async Task Migration_26_27(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE rssSources ADD COLUMN singleUrl INTEGER NOT NULL DEFAULT 0");
            
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS bookmarks1 (
                    time INTEGER NOT NULL, 
                    bookUrl TEXT NOT NULL, 
                    bookName TEXT NOT NULL, 
                    bookAuthor TEXT NOT NULL, 
                    chapterIndex INTEGER NOT NULL, 
                    chapterPos INTEGER NOT NULL, 
                    chapterName TEXT NOT NULL, 
                    bookText TEXT NOT NULL, 
                    content TEXT NOT NULL, 
                    PRIMARY KEY(time)
                )");
            await connection.ExecuteAsync(@"
                INSERT INTO bookmarks1 
                SELECT time, bookUrl, bookName, bookAuthor, chapterIndex, pageIndex, chapterName, '', content 
                FROM bookmarks");
            await connection.ExecuteAsync("DROP TABLE bookmarks");
            await connection.ExecuteAsync("ALTER TABLE bookmarks1 RENAME TO bookmarks");
            await connection.ExecuteAsync("CREATE UNIQUE INDEX IF NOT EXISTS index_bookmarks_time ON bookmarks (time)");
        }
        #endregion

        #region 版本27-28: 添加RSS文章和收藏变量字段
        private static async Task Migration_27_28(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE rssArticles ADD COLUMN variable TEXT");
            await connection.ExecuteAsync("ALTER TABLE rssStars ADD COLUMN variable TEXT");
        }
        #endregion

        #region 版本28-29: 添加RSS源备注字段
        private static async Task Migration_28_29(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE rssSources ADD COLUMN sourceComment TEXT");
        }
        #endregion

        #region 版本29-30: 添加章节分段标识
        private static async Task Migration_29_30(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE chapters ADD COLUMN startFragmentId TEXT");
            await connection.ExecuteAsync("ALTER TABLE chapters ADD COLUMN endFragmentId TEXT");
        }
        #endregion

        #region 版本30-31: 重命名阅读记录设备ID字段
        private static async Task Migration_30_31(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE readRecord RENAME TO readRecord1");
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS readRecord (
                    deviceId TEXT NOT NULL, 
                    bookName TEXT NOT NULL, 
                    readTime INTEGER NOT NULL, 
                    PRIMARY KEY(deviceId, bookName)
                )");
            await connection.ExecuteAsync(@"
                INSERT INTO readRecord (deviceId, bookName, readTime) 
                SELECT androidId, bookName, readTime FROM readRecord1");
        }
        #endregion

        #region 版本31-32: 删除EPUB章节表（不再使用）
        private static async Task Migration_31_32(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("DROP TABLE IF EXISTS epubChapters");
        }
        #endregion

        #region 版本32-33: 重建书签表移除bookUrl字段
        private static async Task Migration_32_33(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE bookmarks RENAME TO bookmarks_old");
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS bookmarks (
                    time INTEGER NOT NULL,
                    bookName TEXT NOT NULL, 
                    bookAuthor TEXT NOT NULL, 
                    chapterIndex INTEGER NOT NULL, 
                    chapterPos INTEGER NOT NULL, 
                    chapterName TEXT NOT NULL, 
                    bookText TEXT NOT NULL, 
                    content TEXT NOT NULL, 
                    PRIMARY KEY(time)
                )");
            await connection.ExecuteAsync(@"
                CREATE INDEX IF NOT EXISTS index_bookmarks_bookName_bookAuthor ON bookmarks (bookName, bookAuthor)");
            await connection.ExecuteAsync(@"
                INSERT INTO bookmarks (time, bookName, bookAuthor, chapterIndex, chapterPos, chapterName, bookText, content)
                SELECT time, ifNull(b.name, bookName) bookName, ifNull(b.author, bookAuthor) bookAuthor, 
                chapterIndex, chapterPos, chapterName, bookText, content FROM bookmarks_old o
                LEFT JOIN books b ON o.bookUrl = b.bookUrl");
        }
        #endregion

        #region 版本33-34: 添加书籍分组封面字段
        private static async Task Migration_33_34(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE book_groups ADD COLUMN cover TEXT");
        }
        #endregion

        #region 版本34-35: 添加书源并发率字段
        private static async Task Migration_34_35(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE book_sources ADD COLUMN concurrentRate TEXT");
        }
        #endregion

        #region 版本35-36: 添加书源登录UI和验证JS字段
        private static async Task Migration_35_36(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE book_sources ADD COLUMN loginUi TEXT");
            await connection.ExecuteAsync("ALTER TABLE book_sources ADD COLUMN loginCheckJs TEXT");
        }
        #endregion

        #region 版本36-37: 添加RSS源登录相关字段
        private static async Task Migration_36_37(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE rssSources ADD COLUMN loginUrl TEXT");
            await connection.ExecuteAsync("ALTER TABLE rssSources ADD COLUMN loginUi TEXT");
            await connection.ExecuteAsync("ALTER TABLE rssSources ADD COLUMN loginCheckJs TEXT");
        }
        #endregion

        #region 版本37-38: 添加书源响应时间字段
        private static async Task Migration_37_38(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE book_sources ADD COLUMN respondTime INTEGER NOT NULL DEFAULT 180000");
        }
        #endregion

        #region 版本38-39: 添加RSS源并发率字段
        private static async Task Migration_38_39(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE rssSources ADD COLUMN concurrentRate TEXT");
        }
        #endregion

        #region 版本39-40: 添加章节VIP和付费标识字段
        private static async Task Migration_39_40(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE chapters ADD COLUMN isVip INTEGER NOT NULL DEFAULT 0");
            await connection.ExecuteAsync("ALTER TABLE chapters ADD COLUMN isPay INTEGER NOT NULL DEFAULT 0");
        }
        #endregion

        #region 版本40-41: 添加HTTP TTS登录和并发控制字段
        private static async Task Migration_40_41(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE httpTTS ADD COLUMN loginUrl TEXT");
            await connection.ExecuteAsync("ALTER TABLE httpTTS ADD COLUMN loginUi TEXT");
            await connection.ExecuteAsync("ALTER TABLE httpTTS ADD COLUMN loginCheckJs TEXT");
            await connection.ExecuteAsync("ALTER TABLE httpTTS ADD COLUMN header TEXT");
            await connection.ExecuteAsync("ALTER TABLE httpTTS ADD COLUMN concurrentRate TEXT");
        }
        #endregion

        #region 版本41-42: 添加HTTP TTS内容类型字段
        private static async Task Migration_41_42(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE httpTTS ADD COLUMN contentType TEXT");
        }
        #endregion

        #region 版本42-43: 添加章节卷标识字段
        private static async Task Migration_42_43(SQLiteAsyncConnectionWrapper connection)
        {
            await connection.ExecuteAsync("ALTER TABLE chapters ADD COLUMN isVolume INTEGER NOT NULL DEFAULT 0");
        }
        #endregion

        #region 版本54-55: 更新书籍类型字段值
        private static async Task Migration_54_55(SQLiteAsyncConnectionWrapper connection)
        {
            // BookType常量定义
            const int BookTypeAudio = 2;
            const int BookTypeImage = 4;
            const int BookTypeWebFile = 8;
            const int BookTypeText = 0;
            const int BookTypeLocal = 1;

            // BookSourceType常量定义
            const int BookSourceTypeAudio = 2;
            const int BookSourceTypeImage = 4;
            const int BookSourceTypeFile = 8;
            const int BookSourceTypeDefault = 0;

            await connection.ExecuteAsync($@"
                UPDATE books SET type = {BookTypeAudio}
                WHERE type = {BookSourceTypeAudio}");
            
            await connection.ExecuteAsync($@"
                UPDATE books SET type = {BookTypeImage}
                WHERE type = {BookSourceTypeImage}");
            
            await connection.ExecuteAsync($@"
                UPDATE books SET type = {BookTypeWebFile}
                WHERE type = {BookSourceTypeFile}");
            
            await connection.ExecuteAsync($@"
                UPDATE books SET type = {BookTypeText}
                WHERE type = {BookSourceTypeDefault}");
            
            await connection.ExecuteAsync($@"
                UPDATE books SET type = type | {BookTypeLocal}
                WHERE origin LIKE 'local%' OR origin LIKE 'webDav%'");
        }
        #endregion

        /// <summary>
        /// 破坏性迁移（删除并重建所有表）
        /// </summary>
        private static async Task DestructiveMigrationAsync(SQLiteAsyncConnectionWrapper connection)
        {
            // TODO: 实现破坏性迁移逻辑
            // 这里应该删除所有旧表并创建新表
            await Task.CompletedTask;
        }
    }
}
