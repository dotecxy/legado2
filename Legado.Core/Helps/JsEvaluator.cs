using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Legado.Core.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Legado.Core.Helps
{
    /// <summary>
    /// JavaScript 执行器 
    /// 使用 Jint 引擎执行 JavaScript 代码
    /// </summary>
    public class JsEvaluator : IDisposable
    {
        private readonly Engine _engine;
        private readonly object _lockObject = new object();
        public Dictionary<string, object> Bindings { get; set; }

        public JsEvaluator()
        {
            _engine = new Engine(options =>
            {
                // 可以在这里配置引擎选项
                options.AllowClr(); // 允许访问CLR类型

                options.SetTypeResolver(new TypeResolver
                {
                    MemberNameComparer = StringComparer.Ordinal,
                    MemberNameCreator = new Func<System.Reflection.MemberInfo, IEnumerable<string>>(NameCreator)

                });
            });
        }

        private static IEnumerable<string> NameCreator(MemberInfo info)
        {
            yield return info.Name.FirstCharToLower();
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }

        // 模拟Kotlin中的evalJS函数
        public object EvalJs(string jsStr, object result = null)
        {
            lock (_lockObject)
            {
                try
                {
                    // 设置绑定
                    if (Bindings != null && Bindings.Count > 0)
                    {
                        foreach (var b in Bindings)
                        {
                            _engine.SetValue(b.Key, b.Value);
                        }
                        //_engine.SetValue("java", context.JavaObject);
                        //_engine.SetValue("baseUrl", context.BaseUrl);
                        //_engine.SetValue("cookie", context.CookieStore);
                        //_engine.SetValue("cache", context.CacheManager);
                        //_engine.SetValue("page", context.Page);
                        //_engine.SetValue("key", context.Key);
                        //_engine.SetValue("speakText", context.SpeakText);
                        //_engine.SetValue("speakSpeed", context.SpeakSpeed);
                        //_engine.SetValue("book", context.Book);
                        //_engine.SetValue("source", context.Source);
                        //_engine.SetValue("result", result);
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

}
