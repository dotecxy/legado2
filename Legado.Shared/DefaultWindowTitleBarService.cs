using System;
using System.Threading.Tasks;

namespace Legado.Shared
{
    /// <summary>
    /// 默认窗口标题栏服务实现（用于非Windows平台）
    /// </summary>
    public class DefaultWindowTitleBarService : IWindowTitleBar
    {
        /// <summary>
        /// 关闭窗口 - 在非Windows平台上不执行任何操作
        /// </summary>
        public async Task CloseAsync()
        {
            // 在非Windows平台上不执行任何操作
            await Task.CompletedTask;
        }

        /// <summary>
        /// 最大化窗口 - 在非Windows平台上不执行任何操作
        /// </summary>
        public async Task MaximizeAsync()
        {
            // 在非Windows平台上不执行任何操作
            await Task.CompletedTask;
        }

        /// <summary>
        /// 最小化窗口 - 在非Windows平台上不执行任何操作
        /// </summary>
        public async Task MinimizeAsync()
        {
            // 在非Windows平台上不执行任何操作
            await Task.CompletedTask;
        }

        /// <summary>
        /// 还原窗口 - 在非Windows平台上不执行任何操作
        /// </summary>
        public async Task RestoreAsync()
        {
            // 在非Windows平台上不执行任何操作
            await Task.CompletedTask;
        }

        /// <summary>
        /// 拖拽窗口 - 在非Windows平台上不执行任何操作
        /// </summary>
        /// <param name="x">X坐标偏移量</param>
        /// <param name="y">Y坐标偏移量</param>
        public async Task DragMoveAsync(int x, int y)
        {
            // 在非Windows平台上不执行任何操作
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取窗口标题 - 返回默认标题
        /// </summary>
        public async Task<string> GetTitleAsync()
        {
            return await Task.FromResult("Legado Application");
        }

        /// <summary>
        /// 设置窗口标题 - 在非Windows平台上不执行任何操作
        /// </summary>
        /// <param name="title">标题文本</param>
        public async Task SetTitleAsync(string title)
        {
            // 在非Windows平台上不执行任何操作
            await Task.CompletedTask;
        }

        /// <summary>
        /// 检查窗口是否处于最大化状态 - 始终返回false
        /// </summary>
        public async Task<bool> IsMaximizedAsync()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// 检查窗口是否处于最小化状态 - 始终返回false
        /// </summary>
        public async Task<bool> IsMinimizedAsync()
        {
            return await Task.FromResult(false);
        }
    }
}