using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Legado.Shared;

namespace Legado.Windows
{
    /// <summary>
    /// Windows窗口标题栏服务实现
    /// </summary>
    public class WindowTitleBarService : IWindowTitleBar
    {
        // Windows API常量
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CLOSE = 0xF060;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_RESTORE = 0xF120;
        private const int HTCAPTION = 0x0002;

        // Windows API函数导入
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public WindowTitleBarService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        private Form GetMainForm()
        {
            if (_serviceProvider.GetService(typeof(Form1)) is Form1 form)
            {
                return form;
            }
            throw new InvalidOperationException("无法获取主窗体实例");
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public async Task CloseAsync()
        {
            var mainForm = GetMainForm();
            await Task.Run(() =>
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() => mainForm.Close()));
                }
                else
                {
                    mainForm.Close();
                }
            });
        }

        /// <summary>
        /// 最大化窗口
        /// </summary>
        public async Task MaximizeAsync()
        {
            var mainForm = GetMainForm();
            await Task.Run(() =>
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() => mainForm.WindowState = FormWindowState.Maximized));
                }
                else
                {
                    mainForm.WindowState = FormWindowState.Maximized;
                }
            });
        }

        /// <summary>
        /// 最小化窗口
        /// </summary>
        public async Task MinimizeAsync()
        {
            var mainForm = GetMainForm();
            await Task.Run(() =>
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() => mainForm.WindowState = FormWindowState.Minimized));
                }
                else
                {
                    mainForm.WindowState = FormWindowState.Minimized;
                }
            });
        }

        /// <summary>
        /// 还原窗口（从最大化状态恢复）
        /// </summary>
        public async Task RestoreAsync()
        {
            var mainForm = GetMainForm();
            await Task.Run(() =>
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() => mainForm.WindowState = FormWindowState.Normal));
                }
                else
                {
                    mainForm.WindowState = FormWindowState.Normal;
                }
            });
        }

        /// <summary>
        /// 拖拽窗口
        /// </summary>
        /// <param name="x">X坐标偏移量</param>
        /// <param name="y">Y坐标偏移量</param>
        public async Task DragMoveAsync(int x, int y)
        {
            var mainForm = GetMainForm();
            await Task.Run(() =>
            {
                ReleaseCapture();
                PostMessage(mainForm.Handle, WM_SYSCOMMAND, (IntPtr)(HTCAPTION), IntPtr.Zero);
            });
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        public async Task<string> GetTitleAsync()
        {
            var mainForm = GetMainForm();
            return await Task.Run(() =>
            {
                if (mainForm.InvokeRequired)
                {
                    return (string)mainForm.Invoke(new Func<string>(() => mainForm.Text));
                }
                else
                {
                    return mainForm.Text;
                }
            });
        }

        /// <summary>
        /// 设置窗口标题
        /// </summary>
        /// <param name="title">标题文本</param>
        public async Task SetTitleAsync(string title)
        {
            var mainForm = GetMainForm();
            await Task.Run(() =>
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(new Action(() => mainForm.Text = title ?? string.Empty));
                }
                else
                {
                    mainForm.Text = title ?? string.Empty;
                }
            });
        }

        /// <summary>
        /// 检查窗口是否处于最大化状态
        /// </summary>
        public async Task<bool> IsMaximizedAsync()
        {
            var mainForm = GetMainForm();
            return await Task.Run(() =>
            {
                if (mainForm.InvokeRequired)
                {
                    return (bool)mainForm.Invoke(new Func<bool>(() => mainForm.WindowState == FormWindowState.Maximized));
                }
                else
                {
                    return mainForm.WindowState == FormWindowState.Maximized;
                }
            });
        }

        /// <summary>
        /// 检查窗口是否处于最小化状态
        /// </summary>
        public async Task<bool> IsMinimizedAsync()
        {
            var mainForm = GetMainForm();
            return await Task.Run(() =>
            {
                if (mainForm.InvokeRequired)
                {
                    return (bool)mainForm.Invoke(new Func<bool>(() => mainForm.WindowState == FormWindowState.Minimized));
                }
                else
                {
                    return mainForm.WindowState == FormWindowState.Minimized;
                }
            });
        }
    }
}