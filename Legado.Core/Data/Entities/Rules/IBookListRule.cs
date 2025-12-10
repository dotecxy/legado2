namespace Legado.Core.Data.Entities.Rules
{
    /// <summary>
    /// 书籍列表规则接口（对应 Kotlin 的 BookListRule.kt）
    /// 搜索和发现共用
    /// </summary>
    public interface IBookListRule
    {
        string BookList { get; set; }
        string Name { get; set; }
        string Author { get; set; }
        string Intro { get; set; }
        string Kind { get; set; }
        string LastChapter { get; set; }
        string UpdateTime { get; set; }
        string BookUrl { get; set; }
        string CoverUrl { get; set; }
        string WordCount { get; set; }
    }
}
