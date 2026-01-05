using Legado.Core.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Legado.Core.Data.Dao
{
    /// <summary>
    /// 书籍分组数据访问实现（对应 Kotlin 的 BookGroupDao.kt）
    /// </summary>
    public class BookGroupDao : BaseDao<BookGroup>, IBookGroupDao
    {
        public BookGroupDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // ================= 查询方法 =================

        /// <summary>
        /// 根据 ID 获取分组
        /// </summary>
        public async Task<BookGroup> GetByIdAsync(long id)
        {
            return await GetFirstOrDefaultAsync(g => g.GroupId == id);
        }

        /// <summary>
        /// 根据名称获取分组
        /// </summary>
        public async Task<BookGroup> GetByNameAsync(string groupName)
        {
            return await GetFirstOrDefaultAsync(g => g.GroupName == groupName);
        }

        /// <summary>
        /// 获取所有分组
        /// </summary>
        public override async Task<List<BookGroup>> GetAllAsync()
        {
            var sql = "SELECT * FROM book_groups ORDER BY `order`";
            var result = await QueryAsync<BookGroup>(sql);
            return result;
        }

        /// <summary>
        /// 获取可选择的分组（groupId >= 0）
        /// </summary>
        public async Task<List<BookGroup>> GetSelectAsync()
        {
            var sql = "SELECT * FROM book_groups WHERE groupId >= 0 ORDER BY `order`";
            var result = await QueryAsync<BookGroup>(sql);
            return result;
        }

        /// <summary>
        /// 获取显示的分组（复杂逻辑，需要联表查询）
        /// </summary>
        public async Task<List<BookGroup>> GetShowAsync()
        {
            // 复杂的SQL查询，模拟Kotlin版本的show查询逻辑
            var sql = @"
                WITH const AS (SELECT SUM(groupId) AS sumGroupId FROM book_groups WHERE groupId > 0)
                SELECT bg.* FROM book_groups bg 
                JOIN const c
                WHERE bg.show > 0
                AND (
                    (bg.groupId >= 0 AND EXISTS (SELECT 1 FROM books b WHERE (b.[group] & bg.groupId) > 0))
                    OR bg.groupId = -1
                    OR (bg.groupId = -2 AND EXISTS (SELECT 1 FROM books b WHERE (b.type & 1) > 0))  -- 假设BookType.local = 1
                    OR (bg.groupId = -3 AND EXISTS (SELECT 1 FROM books b WHERE (b.type & 2) > 0))  -- 假设BookType.audio = 2
                    OR (bg.groupId = -11 AND EXISTS (SELECT 1 FROM books b WHERE (b.type & 4) > 0)) -- 假设BookType.updateError = 4
                    OR (bg.groupId = -4
                        AND EXISTS (
                            SELECT 1 FROM books b 
                            WHERE (b.type & 2) = 0  -- not audio
                            AND (b.type & 1) = 0   -- not local
                            AND (c.sumGroupId & b.[group]) = 0
                        )
                    )
                    OR (bg.groupId = -5
                        AND EXISTS (
                            SELECT 1 FROM books b 
                            WHERE (b.type & 2) = 0  -- not audio
                            AND (b.type & 1) > 0   -- is local
                            AND (c.sumGroupId & b.[group]) = 0
                        )
                    )
                )
                ORDER BY bg.[order]";
            
            var result = await QueryAsync<BookGroup>(sql);
            return result;
        }

        /// <summary>
        /// 获取分组名称（按 ID）
        /// </summary>
        public async Task<List<string>> GetGroupNamesAsync(long id)
        {
            var groups = await GetAllAsync();
            return groups
                .Where(g => g.GroupId > 0 && (g.GroupId & id) > 0)
                .Select(g => g.GroupName)
                .ToList();
        }

        // ================= 统计方法 =================

        /// <summary>
        /// 获取所有分组 ID 之和
        /// </summary>
        public async Task<long> GetIdsSumAsync()
        {
            var groups = await GetAllAsync();
            return groups.Where(g => g.GroupId >= 0).Sum(g => g.GroupId);
        }

        /// <summary>
        /// 获取最大排序值
        /// </summary>
        public async Task<int> GetMaxOrderAsync()
        {
            var groups = await GetAllAsync();
            var selectGroups = groups.Where(g => g.GroupId >= 0);
            return selectGroups.Any() ? selectGroups.Max(g => g.Order) : 0;
        }

        /// <summary>
        /// 检查是否可以添加分组（少于 64 个）
        /// </summary>
        public async Task<bool> CanAddGroupAsync()
        {
            var sql = "SELECT CASE WHEN COUNT(*) < 64 THEN 1 ELSE 0 END FROM book_groups WHERE groupId >= 0 OR groupId = @a";
            var result = await ExecuteScalarAsync<int>(sql, new { a = long.MinValue });
            return result > 0;
        }

        // ================= 增删改操作 =================

        /// <summary>
        /// 插入分组
        /// </summary>
        public async Task InsertAsync(params BookGroup[] bookGroups)
        {
            if (bookGroups == null || bookGroups.Length == 0)
                return;

            await InsertOrReplaceAllAsync(bookGroups);
        }

        /// <summary>
        /// 更新分组
        /// </summary>
        public async Task UpdateAsync(params BookGroup[] bookGroups)
        {
            if (bookGroups == null || bookGroups.Length == 0)
                return;

            await base.UpdateAllAsync(bookGroups);
        }

        /// <summary>
        /// 删除分组
        /// </summary>
        public async Task DeleteAsync(params BookGroup[] bookGroups)
        {
            if (bookGroups == null || bookGroups.Length == 0)
                return;

            await base.DeleteAllAsync(bookGroups);
        }

        /// <summary>
        /// 启用分组
        /// </summary>
        public async Task EnableGroupAsync(long groupId)
        {
            var sql = "UPDATE book_groups SET show = 1 WHERE groupId = @a";
            await ExecuteAsync(sql, new { a= groupId });
        }

        // ================= 辅助方法 =================

        /// <summary>
        /// 检查 ID 是否在规则内（对应 Kotlin 的 isInRules）
        /// </summary>
        public bool IsInRules(long id)
        {
            if (id < 0)
                return true;
            return (id & (id - 1)) == 0L;
        }

        /// <summary>
        /// 获取未使用的 ID（对应 Kotlin 的 getUnusedId）
        /// </summary>
        public async Task<long> GetUnusedIdAsync()
        {
            long id = 1L;
            var idsSum = await GetIdsSumAsync();
            while ((id & idsSum) != 0L)
            {
                id = id << 1;
            }
            return id;
        }

        // ================= Observable 数据流 =================

        /// <summary>
        /// 观察所有分组
        /// </summary>
        public IObservable<List<BookGroup>> ObserveAll()
        {
            return Observable.Create<List<BookGroup>>(async observer =>
            {
                var groups = await GetAllAsync();
                observer.OnNext(groups);
                observer.OnCompleted();
            });
        }

        /// <summary>
        /// 观察可选择的分组
        /// </summary>
        public IObservable<List<BookGroup>> ObserveSelect()
        {
            return Observable.Create<List<BookGroup>>(async observer =>
            {
                var groups = await GetSelectAsync();
                observer.OnNext(groups);
                observer.OnCompleted();
            });
        }
    }
}
