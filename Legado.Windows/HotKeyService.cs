using Legado.Core;
using Legado.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke; 

namespace Legado.Windows
{
    [SingletonDependency]
    [ExposeServices(typeof(IHotKeyService))]
    public class HotKeyService : IHotKeyService, IDisposable
    {
        // 定义修饰键常量
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_NOREPEAT = 0x4000;

        public const int WM_HOTKEY = 0x0312;


        private readonly Form1 _form;
        private readonly INativeAudioService _audio;
        private readonly ILogger _logger;
        private AsyncLock _mutex = new AsyncLock();
        private bool _initialization;

        private int _id = 1;

        public int ID
        {
            get
            {
                return Interlocked.Increment(ref _id);
            }
        }


        private readonly ConcurrentDictionary<int, (nint, Action)> _registerHotKetRecords = new ConcurrentDictionary<int, (nint, Action)>();

        public HotKeyService(ILogger<HotKeyService> logger, Form1 form, INativeAudioService audioService)
        {
            _form = form;
            _audio = audioService;
            _logger = logger;
        }

        // 引入系统 API
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public async Task ApplyAsync()
        {
            using var _ = await _mutex.LockAsync();

            if (_initialization)
            {
                return;
            }
            _initialization = true;
            await Task.Delay(10);
            var id = ID;
            var hWnd = _form.Handle;
            if (RegisterHotKey(hWnd, id, MOD_ALT | MOD_CONTROL, (uint)Keys.Space))
            {
                var action = new Action(async () =>
                {


                    if (_audio.IsPlaying)
                    {
                        await _audio.PauseAsync();
                    }
                    else
                    {
                        await _audio.PlayAsync(_audio.CurrentPositionMillisecond);
                    }
                });
                _registerHotKetRecords.AddOrUpdate(id, (hWnd, action), (_, _) => (hWnd, action));
            }
            else
            {
                _logger.LogError($"注册{Keys.Space.ToString()}热键失败!");
            }

            if (_form is IMessageProc recv)
            {
                recv.OnMessage += Recv_OnMessage;
            }
        }

        public void Dispose()
        {
            if (_form is IMessageProc recv)
            {
                recv.OnMessage -= Recv_OnMessage;
            }
            if (_registerHotKetRecords.Any())
            {
                var keys = _registerHotKetRecords.Keys;
                foreach (var item in keys)
                {
                    if (_registerHotKetRecords.TryGetValue(item, out var item2))
                    {
                       UnregisterHotKey(item2.Item1, item);
                    }
                }
            }
        }

        private void Recv_OnMessage(object? sender, Message e)
        {

            if (e.Msg == 0x0312)
            {
                int hotkeyId = e.WParam.ToInt32();
                if (_registerHotKetRecords.TryGetValue(hotkeyId, out var item))
                {
                    try
                    {
                        item.Item2.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Recv_OnMessage]");
                    }
                }
            }
        }
    }

    public interface IMessageProc
    {
        event EventHandler<Message> OnMessage;
    }
}
