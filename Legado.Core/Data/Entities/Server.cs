using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 服务器配置（对应 Kotlin 的 Server.kt）
    /// </summary>
    [Table(Name = "servers")]
    public class Server
    {
        /// <summary>
        /// ID
        /// </summary>
        [Column(IsPrimary = true)]
        [JsonProperty("id")]
        public long Id { get; set; } = IdGenerator.New();

        /// <summary>
        /// 名称
        /// </summary>
        
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// 地址
        /// </summary>
        
        [JsonProperty("url")]
        public string Url { get; set; } = "";

        /// <summary>
        /// 账号
        /// </summary>
        
        [JsonProperty("account")]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        
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
