using AngleSharp.Dom;
using Legado.Core.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using SharpCompress.Common;
using  Legado.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Core.Data
{
    /// <summary>
    /// 通用 DAO 基类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public abstract class BaseDao<T> : QDbContext where T : class, new()
    {
        protected readonly IFreeSql _fsql;
        static object lockObj = new object();

        public BaseDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _fsql = serviceProvider.GetRequiredService<IFreeSql>();
            //fsql.CodeFirst.SyncStructure<T>(); 
        }


        // ================= 查询方法 =================

        /// <summary>
        /// 获取所有记录
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            return await _fsql.Queryable<T>().ToListAsync();
        }

        /// <summary>
        /// 根据条件获取列表
        /// </summary>
        public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate = null)
        {
            return await _fsql.Queryable<T>().Where(predicate).ToListAsync();
        }

        /// <summary>
        /// 获取第一条记录（如果不存在返回 null）
        /// </summary>
        public virtual async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null)
        {
            return await _fsql.Queryable<T>().Where(predicate).FirstAsync();
        }

        /// <summary>
        /// 根据主键获取记录
        /// </summary>
        public virtual async Task<T> GetAsync(object primaryKey)
        {
            return await _fsql.Queryable<T>().WhereDynamic(primaryKey).FirstAsync();
        }

        /// <summary>
        /// 根据主键获取记录（如果不存在返回 null）
        /// </summary>
        public virtual async Task<T> FindAsync(object primaryKey)
        {
            return await _fsql.Queryable<T>().WhereDynamic(primaryKey).FirstAsync();
        }

        // ================= 计数/存在性检查 =================

        /// <summary>
        /// 获取记录总数
        /// </summary>
        public virtual async Task<int> CountAsync()
        {
            return (int)await _fsql.Queryable<T>().CountAsync();

        }

        /// <summary>
        /// 根据条件获取记录数
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            return (int)await _fsql.Queryable<T>().Where(predicate).CountAsync();
        }

        /// <summary>
        /// 检查是否存在符合条件的记录
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate = null)
        {
            return await _fsql.Queryable<T>().Where(predicate).AnyAsync();
        }

        // ================= 插入方法 =================

        /// <summary>
        /// 插入单条记录
        /// </summary>
        public virtual async Task<int> InsertAsync(T entity)
        {
            return await _fsql.Insert<T>(entity).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 批量插入记录
        /// </summary>
        public virtual async Task<int> InsertAllAsync(IEnumerable<T> entities)
        {
            return await _fsql.Insert<T>(entities).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 插入或替换单条记录
        /// </summary>
        public virtual async Task<int> InsertOrReplaceAsync(T entity)
        {
            return await _fsql.InsertOrUpdate<T>().SetSource(entity).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 批量插入或替换记录
        /// </summary>
        public virtual async Task<int> InsertOrReplaceAllAsync(IEnumerable<T> entities)
        {
            return await _fsql.InsertOrUpdate<T>().SetSource(entities).ExecuteAffrowsAsync();
        }

        // ================= 更新方法 =================

        /// <summary>
        /// 更新单条记录
        /// </summary>
        public virtual async Task<int> UpdateAsync(T entity)
        {
            return await _fsql.Update<T>(entity).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 批量更新记录
        /// </summary>
        public virtual async Task<int> UpdateAllAsync(IEnumerable<T> entities)
        {
            return await _fsql.Update<T>(entities).ExecuteAffrowsAsync();
        }

        // ================= 删除方法 =================

        /// <summary>
        /// 删除单条记录
        /// </summary>
        public virtual async Task<int> DeleteAsync(T entity)
        {
            return await _fsql.Delete<T>(entity).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 根据主键删除记录
        /// </summary>
        public virtual async Task<int> DeleteAsync(object primaryKey)
        {
            return await _fsql.Delete<T>(primaryKey).ExecuteAffrowsAsync();

        }

        /// <summary>
        /// 批量删除记录
        /// </summary>
        public virtual async Task<int> DeleteAllAsync(IEnumerable<T> entities)
        {
            return await _fsql.Delete<T>(entities).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 根据条件删除记录
        /// </summary>
        public virtual async Task<int> DeleteAllAsync(Expression<Func<T, bool>> predicate = null)
        {
            return await _fsql.Delete<T>().Where(predicate).ExecuteAffrowsAsync();
        }

        // ================= 执行 SQL 方法 =================

        /// <summary>
        /// 执行 SQL 语句（返回受影响的行数）
        /// </summary>
        public virtual async Task<int> ExecuteAsync(string sql, object args = null)
        {
            return await _fsql.Ado.ExecuteNonQueryAsync(sql, args);
        }

        /// <summary>
        /// 执行查询 SQL（返回标量值）
        /// </summary>
        public virtual async Task<T> ExecuteScalarAsync<T>(string sql, object args = null)
        {
            return await _fsql.Ado.QuerySingleAsync<T>(sql, args);

        }

        /// <summary>
        /// 执行查询 SQL（返回对象列表）
        /// </summary>
        public virtual async Task<List<T>> QueryAsync(string sql, object args = null)
        {
            return await _fsql.Ado.QueryAsync<T>(sql, args);

        }

        /// <summary>
        /// 执行查询 SQL（返回对象列表）
        /// </summary>
        public virtual async Task<List<T>> QueryAsync<T>(string sql, object args = null)
        {
            return await _fsql.Ado.QueryAsync<T>(sql, args);
        }


        // ================= 事务方法 =================

        /// <summary>
        /// 在事务中执行操作
        /// </summary>
        public virtual async Task RunInTransactionAsync(Action<IFreeSql> action)
        {
            await Task.Delay(1);
            _fsql.Transaction(() =>
            {
                action?.Invoke(_fsql);
            });
        }

        // ================= 同步方法（供特殊场景使用） =================

        /// <summary>
        /// 同步获取所有记录
        /// </summary>
        public virtual List<T> GetAll()
        {
            return _fsql.Queryable<T>().ToList();
        }

        /// <summary>
        /// 同步插入记录
        /// </summary>
        public virtual int Insert(T entity)
        {
            return _fsql.Insert<T>(entity).ExecuteAffrows();
        }

        /// <summary>
        /// 同步更新记录
        /// </summary>
        public virtual int Update(T entity)
        {
            return _fsql.Update<T>(entity).ExecuteAffrows();

        }

        /// <summary>
        /// 同步删除记录
        /// </summary>
        public virtual int Delete(T entity)
        {
            return _fsql.Delete<T>(entity).ExecuteAffrows();

        } 

    }

}
