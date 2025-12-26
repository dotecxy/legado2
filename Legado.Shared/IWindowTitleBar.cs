using System;
using System.Threading.Tasks;

namespace Legado.Shared
{
    /// <summary>
    /// 窗口标题栏接口，提供窗口控制功能
    /// </summary>
    public interface IWindowTitleBar
    {
        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();

        /// <summary>
        /// 最大化窗口
        /// </summary>
        /// <returns></returns>
        Task MaximizeAsync();

        /// <summary>
        /// 最小化窗口
        /// </summary>
        /// <returns></returns>
        Task MinimizeAsync();

        /// <summary>
        /// 还原窗口（从最大化状态恢复）
        /// </summary>
        /// <returns></returns>
        Task RestoreAsync();

        /// <summary>
        /// 拖拽窗口
        /// </summary>
        /// <returns></returns>
        Task DragMouseDown();
        Task DragMouseUp();

        /// <summary>
        /// 获取或设置窗口标题
        /// </summary>
        /// <returns></returns>
        Task<string> GetTitleAsync();
        
        /// <summary>
        /// 设置窗口标题
        /// </summary>
        /// <param name="title">标题文本</param>
        /// <returns></returns>
        Task SetTitleAsync(string title);

        /// <summary>
        /// 检查窗口是否处于最大化状态
        /// </summary>
        /// <returns>如果窗口最大化则返回true，否则返回false</returns>
        Task<bool> IsMaximizedAsync();

        /// <summary>
        /// 检查窗口是否处于最小化状态
        /// </summary>
        /// <returns>如果窗口最小化则返回true，否则返回false</returns>
        Task<bool> IsMinimizedAsync();
    }
}