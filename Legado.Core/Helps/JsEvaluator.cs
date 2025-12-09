using Jint;
using Jint.Native;
using Jint.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Helps
{
    /// <summary>
    /// JavaScript 执行器 (对应 Kotlin: evalJS 相关功能)
    /// 使用 Jint 引擎执行 JavaScript 代码
    /// </summary>
    public class JsEvaluator
    {
        private readonly Engine _engine;
        private readonly object _lockObject = new object();

        public JsEvaluator()
        {
            _engine = new Engine(options =>
            {
                // 可以在这里配置引擎选项
                options.AllowClr(); // 允许访问CLR类型
            });
        }

        // 模拟Kotlin中的evalJS函数
        public object EvalJs(string jsStr, object result = null, AnalyzeRuleContext context = null)
        {
            lock (_lockObject)
            {
                try
                {
                    // 设置绑定
                    if (context != null)
                    {
                        _engine.SetValue("java", context.JavaObject);
                        _engine.SetValue("baseUrl", context.BaseUrl);
                        _engine.SetValue("cookie", context.CookieStore);
                        _engine.SetValue("cache", context.CacheManager);
                        _engine.SetValue("page", context.Page);
                        _engine.SetValue("key", context.Key);
                        _engine.SetValue("speakText", context.SpeakText);
                        _engine.SetValue("speakSpeed", context.SpeakSpeed);
                        _engine.SetValue("book", context.Book);
                        _engine.SetValue("source", context.Source);
                        _engine.SetValue("result", result);
                    }

                    // 执行JavaScript代码
                    var jsResult = _engine.Evaluate(jsStr);

                    // 将Jint的结果转换为C#对象
                    return ConvertJsValue(jsResult);
                }
                catch (JavaScriptException jsEx)
                {
                    // 处理JavaScript异常
                    // 你可以根据需要记录日志或重新抛出
                    throw new Exception($"JavaScript执行错误: {jsEx.Message}", jsEx);
                }
                catch (Exception ex)
                {
                    // 处理其他异常
                    throw new Exception($"执行JavaScript时发生错误: {ex.Message}", ex);
                }
            }
        }

        private object ConvertJsValue(JsValue jsValue)
        {
            if (jsValue.IsNull() || jsValue.IsUndefined())
                return null;

            if (jsValue.IsBoolean())
                return jsValue.AsBoolean();

            if (jsValue.IsNumber())
            {
                // 根据数值类型返回
                var number = jsValue.AsNumber();
                if (number % 1 == 0)
                {
                    // 如果是整数
                    if (number >= int.MinValue && number <= int.MaxValue)
                        return (int)number;
                    else
                        return (long)number;
                }
                else
                {
                    return number;
                }
            }

            if (jsValue.IsString())
                return jsValue.AsString();

            if (jsValue.IsArray())
            {
                var array = jsValue.AsArray();
                var list = new List<object>();
                foreach (var item in array)
                {
                    list.Add(ConvertJsValue(item));
                }
                return list;
            }

            if (jsValue.IsObject())
            {
                // 对于对象，我们可以返回一个字典
                var obj = jsValue.AsObject();
                var dict = new Dictionary<string, object>();
                foreach (var property in obj.GetOwnProperties())
                {
                    dict[property.Key.ToString()] = ConvertJsValue(property.Value.Value);
                }
                return dict;
            }

            // 默认返回字符串
            return jsValue.ToString();
        }
    }

    /// <summary>
    /// 分析规则上下文 (对应 Kotlin: AnalyzeRule 的上下文绑定)
    /// 提供 JS 执行时需要的上下文对象
    /// </summary>
    public class AnalyzeRuleContext
    {
        public object JavaObject { get; set; }
        public string BaseUrl { get; set; }
        public object CookieStore { get; set; }
        public object CacheManager { get; set; }
        public object Page { get; set; }
        public object Key { get; set; }
        public object SpeakText { get; set; }
        public object SpeakSpeed { get; set; }
        public object Book { get; set; }
        public object Source { get; set; }
    }
}
