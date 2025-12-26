using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using Legado.Shared;
using Vanara.PInvoke;
using Timer = System.Threading.Timer;

namespace Legado.Windows
{
    /// <summary>
    /// Windows窗口标题栏服务实现
    /// </summary>
    public class WindowTitleBarService : IWindowTitleBar
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _dragMoveTimer;
        private bool _isMoving = false;
        private double _mouseStartX;
        private double _mouseStartY;
        private double _windowStartLeft;
        private double _windowStartTop;
        private IntPtr hWnd;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public WindowTitleBarService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _dragMoveTimer = new Timer((_) =>
            {
                if (!_isMoving)
                {
                    return;
                }
                if (User32.GetCursorPos(out var point))
                {
                    var mauiWindow = InvokeOnUIAsync(frm =>
                    {
                        var x = _windowStartLeft - _mouseStartX + point.X;
                        var y = _windowStartTop - _mouseStartY + point.Y;
                        frm.Location = new System.Drawing.Point((int)x, (int)y);
                    });

                }
            }, null, 10, 10);
        }

        private Form GetMainForm()
        {
            if (_serviceProvider.GetService(typeof(Form1)) is Form1 form)
            {
                return form;
            }
            throw new InvalidOperationException("无法获取主窗体实例");
        }
        private async Task InvokeOnUIAsync(Action<Form> act)
        {
            _ = await InvokeOnUIAsync(new Func<Form, object>((f) =>
            {
                act.Invoke(f);
                return null;
            }));
        }

        private async Task<T> InvokeOnUIAsync<T>(Func<Form, T> act)
        {
            var result = default(T);
            if (act == null)
            {
                return result;
            }
            await Task.Run(() =>
            {
                var frm = GetMainForm();
                if (frm.InvokeRequired)
                {
                    result = frm.Invoke(() =>
                    {
                        return act.Invoke(frm);
                    });
                }
                else
                {
                    result = act(frm);
                }
            });
            return result;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public async Task CloseAsync()
        {
            await InvokeOnUIAsync((frm) => frm.Close());
        }

        /// <summary>
        /// 最大化窗口
        /// </summary>
        public async Task MaximizeAsync()
        {
            await InvokeOnUIAsync((frm) =>
            {
                if (frm.WindowState == FormWindowState.Maximized)
                {
                    frm.WindowState = FormWindowState.Normal;
                }
                else
                {
                    frm.WindowState = FormWindowState.Maximized;
                }
            });
        }

        /// <summary>
        /// 最小化窗口
        /// </summary>
        public async Task MinimizeAsync()
        {
            await InvokeOnUIAsync((frm) =>
            {
                frm.WindowState = FormWindowState.Minimized;
            });
        }

        /// <summary>
        /// 还原窗口（从最大化状态恢复）
        /// </summary>
        public async Task RestoreAsync()
        {
            await MaximizeAsync();
        }

        /// <summary>
        /// 拖拽窗口
        /// </summary>
        /// <param name="x">X坐标偏移量</param>
        /// <param name="y">Y坐标偏移量</param>
        public async Task DragMoveAsync(int x, int y)
        {
            await InvokeOnUIAsync((frm) =>
            {
                //ReleaseCapture();
                var pos = frm.Location;
                pos.X += x;
                pos.Y += y;
                frm.Location = pos;
            });
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        public async Task<string> GetTitleAsync()
        {
            return await InvokeOnUIAsync((frm) => frm.Text);
        }

        /// <summary>
        /// 设置窗口标题
        /// </summary>
        /// <param name="title">标题文本</param>
        public async Task SetTitleAsync(string title)
        {
            await InvokeOnUIAsync((frm) => frm.Text = title);
        }

        /// <summary>
        /// 检查窗口是否处于最大化状态
        /// </summary>
        public async Task<bool> IsMaximizedAsync()
        {
            return await InvokeOnUIAsync((frm) => frm.WindowState == FormWindowState.Maximized);
        }

        /// <summary>
        /// 检查窗口是否处于最小化状态
        /// </summary>
        public async Task<bool> IsMinimizedAsync()
        {
            return await InvokeOnUIAsync((frm) => frm.WindowState == FormWindowState.Minimized);
        }

        public async Task DragMouseDown()
        {
            if (User32.GetCursorPos(out var point))
            {
                _mouseStartX = point.X;
                _mouseStartY = point.Y;
                var location = await InvokeOnUIAsync((frm) =>
                {
                    hWnd = frm.Handle;
                    return frm.Location;
                });
                _windowStartLeft = location.X;
                _windowStartTop = location.Y;
                _isMoving = true;
            }
        }

        public Task DragMouseUp()
        {
            _isMoving = false;
            return Task.CompletedTask;
        }
    }
}