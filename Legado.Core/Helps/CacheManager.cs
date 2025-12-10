using Legado.Core.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Legado.Core.Helps
{
    /// <summary>
    /// 应用缓存管理器（对应 Kotlin 的 AppCacheManager）
    /// </summary>
    public sealed class AppCacheManager
    {
        private readonly static int MaxCacheSize = 4;
        private readonly ConcurrentDictionary<string, object> QueryTTFMap = new ConcurrentDictionary<string, object>();
        private readonly int MaxMemorySize = 1024 * 1024 * 50; // 50MB
        private readonly MemoryLruCache MemoryLruCache = new MemoryLruCache(MaxCacheSize);

        public static readonly AppCacheManager Instance = new AppCacheManager();

        /// <summary>
        /// 存储 QueryTTF（对应 Kotlin 的 put）
        /// </summary>
        public void Put(string key, object queryTTF)
        {
            if (QueryTTFMap.Count >= MaxCacheSize)
            {
                // 移除最旧的项
                var firstKey = QueryTTFMap.Keys.FirstOrDefault();
                if (firstKey != null)
                {
                    QueryTTFMap.TryRemove(firstKey, out _);
                }
            }
            QueryTTFMap[key] = queryTTF;
        }

        /// <summary>
        /// 获取 QueryTTF（对应 Kotlin 的 getQueryTTF）
        /// </summary>
        public object GetQueryTTF(string key)
        {
            QueryTTFMap.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// 清除书源变量（对应 Kotlin 的 clearSourceVariables）
        /// </summary>
        public void ClearSourceVariables()
        {
            var keysToRemove = MemoryLruCache.GetKeys()
                .Where(k => k.StartsWith("v_")
                         || k.StartsWith("userInfo_")
                         || k.StartsWith("loginHeader_")
                         || k.StartsWith("sourceVariable_"))
                .ToList();

            foreach (var key in keysToRemove)
            {
                MemoryLruCache.Remove(key);
            }
        }
    }

    /// <summary>
    /// 缓存管理器（对应 Kotlin 的 CacheManager）
    /// </summary>
    public sealed class CacheManager
    {
        private readonly MemoryLruCache MemoryLruCache = new MemoryLruCache(1024 * 1024 * 50);
        // TODO: 需要实现 ACache 和数据库访问
        // private  readonly ACache FileCache = ACache.Get();
        // private  readonly ICacheDao CacheDao = appDb.cacheDao;

        public static readonly CacheManager Instance = new CacheManager();

        /// <summary>
        /// 存储缓存（对应 Kotlin 的 put）
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="saveTime">保存时间（秒），0 表示永久保存</param>
        public void Put(string key, object value, int saveTime = 0)
        {
            var deadline = saveTime == 0 ? 0L : DateTimeOffset.Now.ToUnixTimeMilliseconds() + saveTime * 1000;

            if (value is byte[] byteArray)
            {
                // TODO: 实现文件缓存
                // FileCache.Put(key, byteArray, saveTime);
            }
            else
            {
                var cache = new Cache
                {
                    Key = key,
                    Value = value?.ToString(),
                    Deadline = deadline
                };
                PutMemory(key, value);
                // TODO: 保存到数据库
                // CacheDao.Insert(cache);
            }
        }

        /// <summary>
        /// 存储到内存缓存（对应 Kotlin 的 putMemory）
        /// </summary>
        public void PutMemory(string key, object value)
        {
            MemoryLruCache.Put(key, value);
        }

        /// <summary>
        /// 从内存中获取数据（对应 Kotlin 的 getFromMemory）
        /// </summary>
        public object GetFromMemory(string key)
        {
            return MemoryLruCache.Get(key);
        }

        /// <summary>
        /// 从内存中删除数据（对应 Kotlin 的 deleteMemory）
        /// </summary>
        public void DeleteMemory(string key)
        {
            MemoryLruCache.Remove(key);
        }

        /// <summary>
        /// 获取缓存（对应 Kotlin 的 get）
        /// </summary>
        public string Get(string key)
        {
            var memoryValue = GetFromMemory(key);
            if (memoryValue is string strValue)
            {
                return strValue;
            }

            // TODO: 从数据库读取
            // var cache = CacheDao.Get(key);
            // if (cache != null && (cache.Deadline == 0L || cache.Deadline > DateTimeOffset.Now.ToUnixTimeMilliseconds()))
            // {
            //     PutMemory(key, cache.Value ?? "");
            //     return cache.Value;
            // }

            return null;
        }

        /// <summary>
        /// 获取整数缓存（对应 Kotlin 的 getInt）
        /// </summary>
        public int? GetInt(string key)
        {
            var value = Get(key);
            if (int.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 获取长整数缓存（对应 Kotlin 的 getLong）
        /// </summary>
        public long? GetLong(string key)
        {
            var value = Get(key);
            if (long.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 获取双精度浮点数缓存（对应 Kotlin 的 getDouble）
        /// </summary>
        public double? GetDouble(string key)
        {
            var value = Get(key);
            if (double.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 获取单精度浮点数缓存（对应 Kotlin 的 getFloat）
        /// </summary>
        public float? GetFloat(string key)
        {
            var value = Get(key);
            if (float.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 获取字节数组缓存（对应 Kotlin 的 getByteArray）
        /// </summary>
        public byte[] GetByteArray(string key)
        {
            // TODO: 实现文件缓存读取
            // return FileCache.GetAsBinary(key);
            return null;
        }

        /// <summary>
        /// 存储文件缓存（对应 Kotlin 的 putFile）
        /// </summary>
        public void PutFile(string key, string value, int saveTime = 0)
        {
            // TODO: 实现文件缓存
            // FileCache.Put(key, value, saveTime);
        }

        /// <summary>
        /// 获取文件缓存（对应 Kotlin 的 getFile）
        /// </summary>
        public string GetFile(string key)
        {
            // TODO: 实现文件缓存读取
            // return FileCache.GetAsString(key);
            return null;
        }

        /// <summary>
        /// 删除缓存（对应 Kotlin 的 delete）
        /// </summary>
        public void Delete(string key)
        {
            // TODO: 从数据库删除
            // CacheDao.Delete(key);
            DeleteMemory(key);
            // TODO: 从文件缓存删除
            // FileCache.Remove(key);
        }
    }

    /// <summary>
    /// 内存LRU缓存实现
    /// </summary>
    internal class MemoryLruCache
    {
        private readonly int _maxSize;
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
        private readonly object _lock = new object();
        private int _currentSize = 0;

        public MemoryLruCache(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Put(string key, object value)
        {
            if (value == null) return;

            var size = GetSizeOf(key, value);
            lock (_lock)
            {
                // 如果已存在，先移除旧值
                if (_cache.TryRemove(key, out var oldItem))
                {
                    _currentSize -= oldItem.Size;
                }

                // 确保有足够空间
                while (_currentSize + size > _maxSize && _cache.Count > 0)
                {
                    // 移除最旧的项（最先添加的）
                    var oldestKey = _cache.OrderBy(x => x.Value.Timestamp).First().Key;
                    if (_cache.TryRemove(oldestKey, out var removedItem))
                    {
                        _currentSize -= removedItem.Size;
                    }
                }

                // 添加新项
                var item = new CacheItem
                {
                    Value = value,
                    Size = size,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                };
                _cache[key] = item;
                _currentSize += size;
            }
        }

        public object Get(string key)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                // 更新访问时间
                item.Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                return item.Value;
            }
            return null;
        }

        public void Remove(string key)
        {
            lock (_lock)
            {
                if (_cache.TryRemove(key, out var item))
                {
                    _currentSize -= item.Size;
                }
            }
        }

        public IEnumerable<string> GetKeys()
        {
            return _cache.Keys.ToList();
        }

        private int GetSizeOf(string key, object value)
        {
            // 估算内存大小（字符串长度 * 2 因为 C# 中 char 是 2 字节）
            var str = value?.ToString() ?? "";
            return str.Length * 2;
        }

        private class CacheItem
        {
            public object Value { get; set; }
            public int Size { get; set; }
            public long Timestamp { get; set; }
        }
    }
}
