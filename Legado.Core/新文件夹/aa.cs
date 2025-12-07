using Jint;
using System;

namespace Legado.Core.JsEngine
{
    public class LegadoJsContext
    {
        private Engine _engine;

        public LegadoJsContext()
        {
            _engine = new Engine();

            // 模拟 Legado 的 java 对象
            var javaObj = new
            {
                ajax = new Func<string, string>(Url => {
                    // TODO: 实现 HTTP GET 请求
                    return "";
                }),
                log = new Action<string>(msg => Console.WriteLine(msg)),
                // 模拟 Base64
                base64Decode = new Func<string, string>(s => {
                    var bytes = Convert.FromBase64String(s);
                    return System.Text.Encoding.UTF8.GetString(bytes);
                })
                // ... 添加更多 Legado API ...
            };

            _engine.SetValue("java", javaObj);
            _engine.SetValue("baseUrl", "");
            _engine.SetValue("result", "");
        }

        public string Evaluate(string script, string html, string prevResult)
        {
            _engine.SetValue("src", html);
            _engine.SetValue("result", prevResult);

            try
            {
                var result = _engine.Evaluate(script);
                return result.ToString();
            }
            catch (Exception ex)
            {
                return ""; // JS 执行错误处理
            }
        }
    }
}