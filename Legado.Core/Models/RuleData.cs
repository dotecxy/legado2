using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Models
{
    /// <summary>
    /// 规则数据接口
    /// </summary>
    public interface IRuleData
    {
        /// <summary>
        /// 存储变量（对应 Kotlin 的 putVariable）
        /// </summary>
        bool PutVariable(string key, string value);

        /// <summary>
        /// 存储大变量（对应 Kotlin 的 putBigVariable）
        /// </summary>
        void PutBigVariable(string key, string value);

        /// <summary>
        /// 获取大变量（对应 Kotlin 的 getBigVariable）
        /// </summary>
        string GetBigVariable(string key);

        /// <summary>
        /// 获取所有变量的 JSON 字符串（对应 Kotlin 的 getVariable）
        /// </summary>
        string GetVariable();

        /// <summary>
        /// 获取指定变量（对应 Kotlin 的 getVariable(key)）
        /// </summary>
        string GetVariable(string key);
    }

    /// <summary>
    /// 规则数据基类
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
        public virtual bool PutVariable(string key, string value)
        {
            variableMap.AddOrUpdate(key, value, (_, _) => value);
            return true;
        }

        /// <summary>
        /// 存储大变量
        /// </summary>
        public virtual void PutBigVariable(string key, string value)
        {
            variableMap.AddOrUpdate(key, value, (_, _) => value);
        }

        /// <summary>
        /// 获取大变量
        /// </summary>
        public virtual string GetBigVariable(string key)
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
        public virtual string GetVariable()
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
        public virtual string GetVariable(string key)
        {
            if (variableMap.TryGetValue(key, out var value))
            {
                return value;
            }
            return "";
        }
    }
}
