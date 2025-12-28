using Newtonsoft.Json;
using SQLite;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 服务器配置（对应 Server.kt）
    /// </summary>
    [Table("servers")]
    public class Server
    {
        /// <summary>
        /// ID
        /// </summary>
        [PrimaryKey]
        [Column("id")]
        [JsonProperty("id")]
        public long Id { get; set; } = IdGenerator.New();

        /// <summary>
        /// 名称
        /// </summary>
        [Column("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 地址
        /// </summary>
        [Column("url")]
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// 账号
        /// </summary>
        [Column("account")]
        [JsonProperty("account")]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Column("password")]
        [JsonProperty("password")]
        public string Password { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Server other)
            {
                return Id == other.Id;
            }
            return false;
        }
    }
}
