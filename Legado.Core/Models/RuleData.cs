using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Models
{
    /// <summary>
    /// 规则数据接口（对应 Kotlin 的 RuleData 接口）
    /// </summary>
    public interface IRuleData
    {
        /// <summary>
        /// 存储变量（对应 Kotlin 的 putVariable）
        /// </summary>
        bool putVariable(string key, string value);

        /// <summary>
        /// 存储大变量（对应 Kotlin 的 putBigVariable）
        /// </summary>
        void putBigVariable(string key, string value);

        /// <summary>
        /// 获取大变量（对应 Kotlin 的 getBigVariable）
        /// </summary>
        string getBigVariable(string key);

        /// <summary>
        /// 获取所有变量的 JSON 字符串（对应 Kotlin 的 getVariable）
        /// </summary>
        string getVariable();

        /// <summary>
        /// 获取指定变量（对应 Kotlin 的 getVariable(key)）
        /// </summary>
        string getVariable(string key);
    }

    /// <summary>
    /// 规则数据基类（对应 Kotlin 的 RuleData 类）
    /// </summary>
    public class RuleData : IRuleData
    {
        /// <summary>
        /// 变量映射表
        /// </summary>
        protected ConcurrentDictionary<string, string> variableMap { get; private set; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 存储变量
        /// </summary>
        public virtual bool putVariable(string key, string value)
        {
            variableMap.AddOrUpdate(key, value, (_, _) => value);
            return true;
        }

        /// <summary>
        /// 存储大变量
        /// </summary>
        public virtual void putBigVariable(string key, string value)
        {
            variableMap.AddOrUpdate(key, value, (_, _) => value);
        }

        /// <summary>
        /// 获取大变量
        /// </summary>
        public virtual string getBigVariable(string key)
        {
            if (variableMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// 获取所有变量的 JSON 字符串
        /// </summary>
        public virtual string getVariable()
        {
            if (variableMap.IsEmpty)
            {
                return null;
            }

            return JsonConvert.SerializeObject(variableMap);
        }

        /// <summary>
        /// 获取指定变量
        /// </summary>
        public virtual string getVariable(string key)
        {
            if (variableMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return "";
        }
    }
}
