using Legado.Core.Models;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// RSS文章基础接口（对应 BaseRssArticle.kt）
    /// </summary>
    public interface IBaseRssArticle : IRuleData
    {
        string Origin { get; set; }
        string Link { get; set; }
        string Variable { get; set; }
    }
}
