using Newtonsoft.Json;
using SQLite;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书籍分组（对应 BookGroup.kt）
    /// </summary>
    [Table("book_groups")]
    public class BookGroup
    {
        /// <summary>
        /// 分组ID
        /// </summary>
        [PrimaryKey]
        [JsonProperty("groupId")]
        public long GroupId { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
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
        [JsonProperty("order")]
        public int Order { get; set; } = 0;

        /// <summary>
        /// 是否显示
        /// </summary>
        [JsonProperty("show")]
        public bool Show { get; set; } = true;

        public override int GetHashCode()
        {
            return GroupId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is BookGroup other)
            {
                return GroupId == other.GroupId;
            }
            return false;
        }
    }
}
