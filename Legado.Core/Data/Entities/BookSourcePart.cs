using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书源部分信息（对应 BookSourcePart.kt，用于数据库视图）
    /// </summary>
    public class BookSourcePart
    {
        /// <summary>
        /// 地址，包括 http/https
        /// </summary>
        [JsonProperty("bookSourceUrl")]
        public string BookSourceUrl { get; set; } = "";

        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("bookSourceName")]
        public string BookSourceName { get; set; } = "";

        /// <summary>
        /// 分组
        /// </summary>
        [JsonProperty("bookSourceGroup")]
        public string BookSourceGroup { get; set; }

        /// <summary>
        /// 手动排序编号
        /// </summary>
        [JsonProperty("customOrder")]
        public int CustomOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 启用发现
        /// </summary>
        [JsonProperty("enabledExplore")]
        public bool EnabledExplore { get; set; } = true;

        /// <summary>
        /// 是否有登录地址
        /// </summary>
        [JsonProperty("hasLoginUrl")]
        public bool HasLoginUrl { get; set; } = false;

        /// <summary>
        /// 最后更新时间，用于排序
        /// </summary>
        [JsonProperty("lastUpdateTime")]
        public long LastUpdateTime { get; set; } = 0;

        /// <summary>
        /// 响应时间，用于排序
        /// </summary>
        [JsonProperty("respondTime")]
        public long RespondTime { get; set; } = 180000L;

        /// <summary>
        /// 智能排序的权重
        /// </summary>
        [JsonProperty("weight")]
        public int Weight { get; set; } = 0;

        /// <summary>
        /// 是否有发现url
        /// </summary>
        [JsonProperty("hasExploreUrl")]
        public bool HasExploreUrl { get; set; } = false;

        public override int GetHashCode()
        {
            return BookSourceUrl.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is BookSourcePart other)
            {
                return other.BookSourceUrl == BookSourceUrl;
            }
            return false;
        }

        /// <summary>
        /// 获取显示名称和分组（对应 Kotlin 的 getDisPlayNameGroup）
        /// </summary>
        public string GetDisplayNameGroup()
        {
            if (string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                return BookSourceName;
            }
            return $"{BookSourceName} ({BookSourceGroup})";
        }

        /// <summary>
        /// 添加分组（对应 Kotlin 的 addGroup）
        /// </summary>
        public void AddGroup(string groups)
        {
            if (!string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                var groupSet = new HashSet<string>(
                    BookSourceGroup.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim())
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                );

                var newGroups = groups.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .Where(g => !string.IsNullOrWhiteSpace(g));

                foreach (var g in newGroups)
                {
                    groupSet.Add(g);
                }

                BookSourceGroup = string.Join(",", groupSet);
            }

            if (string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                BookSourceGroup = groups;
            }
        }

        /// <summary>
        /// 移除分组（对应 Kotlin 的 removeGroup）
        /// </summary>
        public void RemoveGroup(string groups)
        {
            if (!string.IsNullOrWhiteSpace(BookSourceGroup))
            {
                var groupSet = new HashSet<string>(
                    BookSourceGroup.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim())
                        .Where(g => !string.IsNullOrWhiteSpace(g))
                );

                var removeGroups = groups.Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .Where(g => !string.IsNullOrWhiteSpace(g));

                foreach (var g in removeGroups)
                {
                    groupSet.Remove(g);
                }

                BookSourceGroup = string.Join(",", groupSet);
            }
        }
    }
}
