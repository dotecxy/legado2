using Jint;
using Legado.Core.Helps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static Legado.Core.Constants.AppPattern;

namespace Legado.Core.Models.AnalyzeRules
{
    public partial class AnalyzeUrl
    {

        private JsEvaluator jsEvaluator = new JsEvaluator();

        public object EvalJS(string jsCode, object result = null)
        {
            return jsEvaluator.EvalJs(jsCode, result, new AnalyzeRuleContext()
            {
                JavaObject = this,
                BaseUrl = baseUrl,
                CookieStore = this.CookieStore,
                Key = key,
                Page = this.page,
                Source = this.source,
                Book = this.ruleData,
                SpeakSpeed = this.speakSpeed,
                SpeakText = this.speakText,
            });
        }


    }
}
