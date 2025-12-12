using Legado.Core.Models;

namespace Legado.Core.Data.Entities
{
    /// <summary>
    /// 书源基础接口（对应 Kotlin 的 BaseSource.kt）
    /// </summary>
    public interface IBaseSource : IRuleData
    {
        /// <summary>
        /// 获取标签（名称）
        /// </summary>
        string GetTag();

        /// <summary>
        /// 获取唯一键（URL）
        /// </summary>
        string GetKey();

        /// <summary>
        /// JS 库
        /// </summary>
        string JsLib { get; set; }

        /// <summary>
        /// 启用 okhttp CookieJar 自动保存每次请求的 cookie
        /// </summary>
        bool? EnabledCookieJar { get; set; }

        /// <summary>
        /// 并发率
        /// </summary>
        string ConcurrentRate { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        string Header { get; set; }

        /// <summary>
        /// 登录地址
        /// </summary>
        string LoginUrl { get; set; }

        /// <summary>
        /// 登录 UI
        /// </summary>
        string LoginUi { get; set; }
    }
}