using System;
using System.Collections.Generic;

namespace Legado.Core.Helps
{
    /// <summary>
    /// 意图数据管理器，用于在组件间传递大数据对象
    /// 避免Intent传递大对象导致的异常
    /// </summary>
    public class IntentData
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, object> _bigData = new Dictionary<string, object>();

        /// <summary>
        /// 存储数据，使用指定的键
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="data">要存储的数据</param>
        /// <returns>返回键</returns>
        public string Put(string key, object data)
        {
            if (data != null)
            {
                lock (_lock)
                {
                    _bigData[key] = data;
                }
            }
            return key;
        }

        /// <summary>
        /// 存储数据，自动生成键（使用当前时间戳）
        /// </summary>
        /// <param name="data">要存储的数据</param>
        /// <returns>返回生成的键</returns>
        public string Put(object data)
        {
            var key = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            if (data != null)
            {
                lock (_lock)
                {
                    _bigData[key] = data;
                }
            }
            return key;
        }

        /// <summary>
        /// 获取数据并删除
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键</param>
        /// <returns>返回数据，如果不存在或类型不匹配则返回null</returns>
        public T Get<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                return null;

            lock (_lock)
            {
                if (_bigData.TryGetValue(key, out var data))
                {
                    _bigData.Remove(key);
                    return data as T;
                }
            }
            return null;
        }
    }
}
