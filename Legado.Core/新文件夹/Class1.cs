using System.Collections.Generic;

namespace Legado.Core
{
    // 定义解析结果类型
    public class AnalyzeResult
    {
        public bool IsSuccess { get; set; }
        public List<string> ResultList { get; set; } = new List<string>();
        public string ResultString => string.Join("", ResultList.ToArray());
    }

    // 核心接口
    public interface IRuleParser
    {
        AnalyzeResult Parse(object content, string rule, string baseUrl);
    }
}