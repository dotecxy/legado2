using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Legado.Core.Constants
{
    public static class AppPattern
    {

        // ================= 常量与正则 =================
        public static readonly Regex JS_PATTERN = new Regex(@"(<js>([\w\W]*?)</js>|@js:([\w\W]*?$))", RegexOptions.IgnoreCase);
        public static readonly Regex PARAM_PATTERN = new Regex(@"\s*,\s*(?=\{)"); // 匹配 URL 和 JSON 选项之间的逗号
        public static readonly Regex PAGE_PATTERN = new Regex(@"<(.*?)>"); // 匹配分页 <1,10>
        public static readonly Regex DATA_URI_REGEX = new Regex(@"data:.*?;base64,(.*)");
        public static readonly Regex XML_CONTENT_TYPE_REGEX = new Regex(@"(?i)((text|application)/xml|application/[a-z]+\+xml)");
    }
}
