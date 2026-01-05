using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍分组（对应 Kotlin 的 BookGroup.kt）
    /// </summary>
    [Table(Name = "book_groups")]
    public class BookGroup
    {
        /// <summary>
        /// 分组ID
        /// </summary>
        [Column(IsPrimary = true)]
        [JsonProperty("groupId")]
        public long GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        [Column(StringLength = 255)]
        [JsonProperty("groupName")]
        public string GroupName { get; set; } = "";

        /// <summary>
        /// 封面
        /// </summary>
        [JsonProperty("cover")]
        public string Cover { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Column()]
        [JsonProperty("order")]
        public int Order { get; set; } = 0;

        /// <summary>
        /// 是否启用刷新
        /// </summary>
        [Column()]
        [JsonProperty("enableRefresh")]
        public bool EnableRefresh { get; set; } = true;

        /// <summary>
        /// 是否显示
        /// </summary>
        [Column()]
        [JsonProperty("show")]
        public bool Show { get; set; } = true;

        /// <summary>
        /// 书籍排序
        /// </summary>
        [Column()]
        [JsonProperty("bookSort")]
        public int BookSort { get; set; } = -1;

        public override int GetHashCode()
        {
            return GroupId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is BookGroup other && other.GroupId == GroupId;
        }

        // 静态常量定义（对应 Kotlin 的 companion object）
        public enum BookId : long
        {
            Root = -100L,
            All = -1L,
            Local = -2L,
            Audio = -3L,
            NetNone = -4L,
            LocalNone = -5L,
            Error = -11L,
        }
    }
}