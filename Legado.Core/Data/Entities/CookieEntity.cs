using SQLite; // 对应 sqlite-net-pcl

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// Cookie 存储 (对应 Cookie.kt)
    /// </summary>
    [Table("cookies")]
    public class CookieEntity
    {
        [PrimaryKey] // sourceUrl 作为主键
        [Column("url")]
        public string Url { get; set; }

        [Column("cookie")]
        public string Cookie { get; set; }
    }
}