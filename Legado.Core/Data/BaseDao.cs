using Legado.Core.Data.Entities;
using SQLite;
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
    /// 基于 SQLite-net-pcl 的通用 DAO 基类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public abstract class BaseDao<T> : QDbContext where T : class, new()
    {
        static object lockObj = new object();
        public BaseDao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            using var conn = CreateConn();
            conn.CreateTable<T>();
        }


        /// <summary>
        /// 创建同步连接（内部使用）
        /// </summary>
        protected SQLiteConnection CreateConn()
        {
            lock (lockObj)
            {
                var connStr = ConnectionString;
                var conn = new SQLiteConnection(connStr);
                return conn;
            }
        }

        /// <summary>
        /// 创建异步连接（内部使用）
        /// </summary>
        protected SQLiteAsyncConnectionWrapper CreateAsyncConn()
        {
            lock (lockObj)
            {
                var connStr = ConnectionString;
                var conn = new SQLiteAsyncConnectionWrapper(connStr);
                return conn;
            }
        }

        // ================= 查询方法 =================

        /// <summary>
        /// 获取所有记录
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.Table<T>().ToListAsync();
            }
        }

        /// <summary>
        /// 根据条件获取列表
        /// </summary>
        public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate = null)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.Table<T>().Where(predicate).ToListAsync();
            }
        }

        /// <summary>
        /// 获取第一条记录（如果不存在返回 null）
        /// </summary>
        public virtual async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.Table<T>().Where(predicate).FirstOrDefaultAsync();
            }
        }

        /// <summary>
        /// 根据主键获取记录
        /// </summary>
        public virtual async Task<T> GetAsync(object primaryKey)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.GetAsync<T>(primaryKey);
            }
        }

        /// <summary>
        /// 根据主键获取记录（如果不存在返回 null）
        /// </summary>
        public virtual async Task<T> FindAsync(object primaryKey)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.FindAsync<T>(primaryKey);
            }
        }

        // ================= 计数/存在性检查 =================

        /// <summary>
        /// 获取记录总数
        /// </summary>
        public virtual async Task<int> CountAsync()
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.Table<T>().CountAsync();
            }
        }

        /// <summary>
        /// 根据条件获取记录数
        /// </summary>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.Table<T>().Where(predicate).CountAsync();
            }
        }

        /// <summary>
        /// 检查是否存在符合条件的记录
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate = null)
        {
            await using (var conn = CreateAsyncConn())
            {
                var count = await conn.Table<T>().Where(predicate).CountAsync();
                return count > 0;
            }
        }

        // ================= 插入方法 =================

        /// <summary>
        /// 插入单条记录
        /// </summary>
        public virtual async Task<int> InsertAsync(T entity)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.InsertAsync(entity);
            }
        }

        /// <summary>
        /// 批量插入记录
        /// </summary>
        public virtual async Task<int> InsertAllAsync(IEnumerable<T> entities)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.InsertAllAsync(entities);
            }
        }

        /// <summary>
        /// 插入或替换单条记录
        /// </summary>
        public virtual async Task<int> InsertOrReplaceAsync(T entity)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.InsertOrReplaceAsync(entity);
            }
        }

        /// <summary>
        /// 批量插入或替换记录
        /// </summary>
        public virtual async Task<int> InsertOrReplaceAllAsync(IEnumerable<T> entities)
        {
            int count = 0;
            await using (var conn = CreateAsyncConn())
            {
                foreach (var item in entities)
                {
                    await conn.RunInTransactionAsync(transaction =>
                    {
                        count += transaction.InsertOrReplace(item);
                    });
                }
                return count;
            }
        }

        // ================= 更新方法 =================

        /// <summary>
        /// 更新单条记录
        /// </summary>
        public virtual async Task<int> UpdateAsync(T entity)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.UpdateAsync(entity);
            }
        }

        /// <summary>
        /// 批量更新记录
        /// </summary>
        public virtual async Task<int> UpdateAllAsync(IEnumerable<T> entities)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.UpdateAllAsync(entities);
            }
        }

        // ================= 删除方法 =================

        /// <summary>
        /// 删除单条记录
        /// </summary>
        public virtual async Task<int> DeleteAsync(T entity)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.DeleteAsync(entity);
            }
        }

        /// <summary>
        /// 根据主键删除记录
        /// </summary>
        public virtual async Task<int> DeleteAsync(object primaryKey)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.DeleteAsync<T>(primaryKey);
            }
        }

        /// <summary>
        /// 批量删除记录
        /// </summary>
        public virtual async Task<int> DeleteAllAsync(IEnumerable<T> entities)
        {
            await using (var conn = CreateAsyncConn())
            {
                var count = 0;
                await conn.RunInTransactionAsync(transaction =>
                {
                    foreach (var entity in entities)
                    {
                        count += transaction.Delete(entity);
                    }
                });
                return count;
            }
        }

        /// <summary>
        /// 根据条件删除记录
        /// </summary>
        public virtual async Task<int> DeleteAllAsync(Expression<Func<T, bool>> predicate = null)
        {
            await using (var conn = CreateAsyncConn())
            {
                var items = await conn.Table<T>().Where(predicate).ToListAsync();
                var count = 0;
                await conn.RunInTransactionAsync(transaction =>
                {
                    foreach (var item in items)
                    {
                        count += transaction.Delete(item);
                    }
                });
                return count;
            }
        }

        // ================= 执行 SQL 方法 =================

        /// <summary>
        /// 执行 SQL 语句（返回受影响的行数）
        /// </summary>
        public virtual async Task<int> ExecuteAsync(string sql, params object[] args)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.ExecuteAsync(sql, args);
            }
        }

        /// <summary>
        /// 执行查询 SQL（返回标量值）
        /// </summary>
        public virtual async Task<TResult> ExecuteScalarAsync<TResult>(string sql, params object[] args)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.ExecuteScalarAsync<TResult>(sql, args);
            }
        }

        /// <summary>
        /// 执行查询 SQL（返回对象列表）
        /// </summary>
        public virtual async Task<List<T>> QueryAsync(string sql, params object[] args)
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.QueryAsync<T>(sql, args);
            }
        }

        /// <summary>
        /// 执行查询 SQL（返回指定类型的对象列表）
        /// </summary>
        public virtual async Task<List<TResult>> QueryAsync<TResult>(string sql, params object[] args) where TResult : new()
        {
            await using (var conn = CreateAsyncConn())
            {
                return await conn.QueryAsync<TResult>(sql, args);
            }
        }

        // ================= 事务方法 =================

        /// <summary>
        /// 在事务中执行操作
        /// </summary>
        public virtual async Task RunInTransactionAsync(Action<SQLiteConnection> action)
        {
            await using (var conn = CreateAsyncConn())
            {
                await conn.RunInTransactionAsync(action);
            }
        }

        // ================= 同步方法（供特殊场景使用） =================

        /// <summary>
        /// 同步获取所有记录
        /// </summary>
        public virtual List<T> GetAll()
        {
            using (var conn = CreateConn())
            {
                return conn.Table<T>().ToList();
            }
        }

        /// <summary>
        /// 同步插入记录
        /// </summary>
        public virtual int Insert(T entity)
        {
            using (var conn = CreateConn())
            {
                return conn.Insert(entity);
            }
        }

        /// <summary>
        /// 同步更新记录
        /// </summary>
        public virtual int Update(T entity)
        {
            using (var conn = CreateConn())
            {
                return conn.Update(entity);
            }
        }

        /// <summary>
        /// 同步删除记录
        /// </summary>
        public virtual int Delete(T entity)
        {
            using (var conn = CreateConn())
            {
                return conn.Delete(entity);
            }
        }
    }

    public sealed class SQLiteAsyncConnectionWrapper : SQLiteAsyncConnection, IAsyncDisposable
    {
        public SQLiteAsyncConnectionWrapper(SQLiteConnectionString connectionString) : base(connectionString)
        {
        }

        public SQLiteAsyncConnectionWrapper(string databasePath, bool storeDateTimeAsTicks = true) : base(databasePath, storeDateTimeAsTicks)
        {
        }

        public SQLiteAsyncConnectionWrapper(string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true) : base(databasePath, openFlags, storeDateTimeAsTicks)
        {
        }

        public async ValueTask DisposeAsync()
        {
            await CloseAsync();
        }
    }

}
